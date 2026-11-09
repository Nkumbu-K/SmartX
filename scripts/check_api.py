"""Integration checks against a temporary store; never touches your saved app data.
Build SmartX.Api first, then run: python scripts/check_api.py [path-to-dotnet]
"""
import json
import os
from pathlib import Path
import socket
import subprocess
import sys
import tempfile
import time
import urllib.error
import urllib.parse
import urllib.request

root = Path(__file__).resolve().parents[1]
dll = root / "SmartX.Api/bin/Debug/net10.0/SmartX.Api.dll"
dotnet = sys.argv[1] if len(sys.argv) > 1 else "dotnet"
with socket.socket() as sock:
    sock.bind(("127.0.0.1", 0))
    port = sock.getsockname()[1]
base = f"http://127.0.0.1:{port}"
opener = urllib.request.build_opener(urllib.request.ProxyHandler({}))


def request(path, method="GET", body=None, expected=200):
    data = None if body is None else json.dumps(body).encode()
    req = urllib.request.Request(base + path, data=data, method=method,
                                 headers={"Content-Type": "application/json"})
    try:
        response = opener.open(req, timeout=10)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        payload = response.read()
        assert response.status == expected, (path, response.status, payload)
        return json.loads(payload) if payload else None


with tempfile.TemporaryDirectory(prefix="smartx-check-") as directory:
    env = dict(os.environ, ASPNETCORE_URLS=base, ASPNETCORE_ENVIRONMENT="Production")
    log_path = Path(directory) / "api.log"
    process = None

    def start():
        global process
        log = open(log_path, "a")
        process = subprocess.Popen([dotnet, str(dll), "--contentRoot", directory],
                                   cwd=directory, env=env, stdout=log, stderr=subprocess.STDOUT)
        log.close()
        for _ in range(100):
            if process.poll() is not None:
                raise RuntimeError(log_path.read_text())
            try:
                request("/api/devices")
                return
            except (OSError, AssertionError):
                time.sleep(0.1)
        raise RuntimeError("API did not become ready: " + log_path.read_text())

    def stop():
        if process and process.poll() is None:
            process.terminate()
            process.wait(timeout=10)

    try:
        start()
        nodes = request("/api/devices")
        a, b, c = [node["id"] for node in nodes[:3]]
        unknown = "ffffffff-ffff-ffff-ffff-ffffffffffff"
        assert request("/api/network/")["links"] == []
        for u, v, cost in [(a, b, 1), (b, c, 2), (a, c, 8)]:
            request("/api/network/links", "PUT", {"from": u, "to": v, "cost": cost})
        for u, v, cost in [(a, a, 1), (a, unknown, 1), (a, b, -1)]:
            request("/api/network/links", "PUT", {"from": u, "to": v, "cost": cost}, 400)
        # Upserting a reverse edge must update rather than duplicate it.
        assert len(request("/api/network/links", "PUT", {"from": b, "to": a, "cost": 1})["links"]) == 3
        assert request("/api/network/mst")["totalCost"] == 3
        assert request(f"/api/network/route?source={a}&gateway={c}")["hops"] == [a, b, c]
        request(f"/api/network/nodes/{b}/availability", "PUT", {"available": False})
        assert request(f"/api/network/route?source={a}&gateway={c}")["hops"] == [a, c]
        traversal = request(f"/api/network/traverse/{a}")
        assert b in traversal["unavailable"] and b not in traversal["visited"]
        request(f"/api/network/traverse/{a}?algorithm=wrong", expected=400)
        request(f"/api/network/traverse/{unknown}", expected=404)
        request("/api/network/demo", "POST", expected=409)
        request(f"/api/devices/{c}/faulty", "POST")
        assert not request(f"/api/network/route?source={a}&gateway={c}")["reachable"]
        request(f"/api/devices/{c}/recover", "POST")
        packet = request("/api/telemetry", "POST", {"sensorId": a, "number": 42.5, "unit": "test"})
        bounds = urllib.parse.urlencode({"from": packet["timestamp"], "to": packet["timestamp"]})
        path = f"/api/telemetry/history/{a}/range?{bounds}"
        assert any(r["packetId"] == packet["packetId"] for r in request(path))
        request(f"/api/telemetry/history/{a}/range?from=2026-11-10T00:00:00Z&to=2026-11-09T00:00:00Z", expected=400)
        request(f"/api/telemetry/history/{a}/range?from=invalid&to=invalid", expected=400)
        request(f"/api/telemetry/history/{unknown}/range?{bounds}", expected=404)
        stop()
        start()
        assert len(request("/api/network/")["links"]) == 3
        assert b in request(f"/api/network/traverse/{a}")["unavailable"]
        assert any(r["packetId"] == packet["packetId"] for r in request(path)), "History index must rebuild after restart"
        request(f"/api/network/links/{a}/{b}", "DELETE", expected=204)
        request(f"/api/network/links/{a}/{b}", "DELETE", expected=404)
        for u, v in [(a, c), (b, c)]:
            request(f"/api/network/links/{u}/{v}", "DELETE", expected=204)
        demo = request("/api/network/demo", "POST")
        assert len(demo["links"]) == 2 * len(nodes) - 3
        request(f"/api/network/nodes/{b}/availability", "PUT", {"available": True})
        assert request("/api/network/mst")["components"] == 1
        print("API checks passed: validation, failure rerouting, fault integration, UTC bounds, restart persistence, link deletion and demo topology.")
    finally:
        stop()
