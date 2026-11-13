"""Run after building SmartX.Api: python scripts/check_part2_feedback.py [dotnet-path].
Uses an isolated temporary store and exercises the endpoints called by the UI.
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
import urllib.request

root = Path(__file__).resolve().parents[1]
dotnet = sys.argv[1] if len(sys.argv) > 1 else "dotnet"
dll = root / "SmartX.Api/bin/Debug/net10.0/SmartX.Api.dll"
with socket.socket() as sock:
    sock.bind(("127.0.0.1", 0))
    port = sock.getsockname()[1]
base = f"http://127.0.0.1:{port}"
opener = urllib.request.build_opener(urllib.request.ProxyHandler({}))


def req(path, body=None, method=None, expected=200):
    method = method or ("POST" if body is not None else "GET")
    data = json.dumps(body).encode() if body is not None else None
    request = urllib.request.Request(base + path, data=data, method=method,
                                     headers={"Content-Type": "application/json"})
    try:
        response = opener.open(request, timeout=10)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        text = response.read()
        assert response.status == expected, (path, response.status, text)
        return json.loads(text) if text else None


with tempfile.TemporaryDirectory(prefix="smartx-feedback-") as directory:
    log_path = Path(directory) / "api.log"
    env = dict(os.environ, ASPNETCORE_URLS=base, ASPNETCORE_ENVIRONMENT="Production")
    with open(log_path, "w") as log:
        process = subprocess.Popen([dotnet, str(dll), "--contentRoot", directory],
                                   cwd=directory, env=env, stdout=log, stderr=subprocess.STDOUT)
    try:
        for _ in range(100):
            if process.poll() is not None:
                raise RuntimeError(log_path.read_text())
            try:
                devices = req("/api/devices")
                break
            except OSError:
                time.sleep(.1)
        else:
            raise RuntimeError("API not ready: " + log_path.read_text())
        a, b = devices[:2]
        aid, bid = a["id"], b["id"]
        req("/api/activity/search", {"searchTerm": ""}, expected=400)
        req("/api/activity/search", {"searchTerm": "x"}, expected=400)
        req("/api/activity/search", {"searchTerm": a["macAddress"], "sensorId": bid}, expected=400)
        # Two searches followed by ONE command count as one independent occurrence.
        for _ in range(2):
            req("/api/activity/search", {"searchTerm": a["macAddress"]})
        req("/api/commands/send", {"sensorId": aid, "commandName": "Restart"})
        assert req(f"/api/activity/patterns?sensorId={aid}") == []
        # More commands without another search must not manufacture a pattern.
        req("/api/commands/send", {"sensorId": aid, "commandName": "Restart"})
        assert req(f"/api/activity/patterns?sensorId={aid}") == []
        req("/api/activity/search", {"searchTerm": a["macAddress"]})
        req("/api/commands/send", {"sensorId": aid, "commandName": "restart"})
        suggestions = req(f"/api/activity/patterns?sensorId={aid}")
        assert len(suggestions) == 1 and "2 independent" in suggestions[0]
        assert req(f"/api/activity/patterns?sensorId={bid}") == []
        recent = req("/api/activity/recent")
        count = len(recent)
        assert sum(e["relatedSearchId"] is not None for e in recent) == 2
        req("/api/commands/send", {"sensorId": aid, "commandName": "Unsupported"}, expected=400)
        assert len(req("/api/activity/recent")) == count
        ambiguous = req("/api/activity/search", {"searchTerm": "Office"})
        assert len(ambiguous["matchedSensorIds"]) > 1 and ambiguous["sensorId"] is None
        req("/api/commands/send", {"sensorId": bid, "commandName": "Restart"})
        assert req(f"/api/activity/patterns?sensorId={bid}") == []
        reca = req(f"/api/recommendations/{aid}")
        recb = req(f"/api/recommendations/{bid}")
        assert reca["sensorId"] == aid and reca["macAddress"] == a["macAddress"] and len(reca["patternRecommendations"]) == 1
        assert recb["sensorId"] == bid and recb["patternRecommendations"] == []
        before = len(req(f"/api/telemetry/history/{aid}"))
        first = req("/api/telemetry/queue", {"sensorId": aid, "number": 11.1, "unit": "test"})
        second = req("/api/telemetry/queue", {"sensorId": bid, "number": 22.2, "unit": "test"})
        assert req("/api/telemetry/queue/status")["pendingPackets"] == 2
        assert len(req(f"/api/telemetry/history/{aid}")) == before, "Enqueue must not bypass processing"
        one = req("/api/telemetry/queue/process", method="POST")["packet"]
        two = req("/api/telemetry/queue/process", method="POST")["packet"]
        assert one["packetId"] == first["packetId"] and one["number"] == 11.1
        assert two["packetId"] == second["packetId"] and two["number"] == 22.2
        assert len(req(f"/api/telemetry/history/{aid}")) == before + 1
        assert req(f"/api/devices/{aid}")["lastCommunication"] == one["timestamp"]
        assert req("/api/telemetry/queue/status")["pendingPackets"] == 0
        req("/api/telemetry/queue/process", method="POST", expected=404)
        for text, priority in [("Low first", 3), ("High second", 1), ("Medium third", 2)]:
            req("/api/alerts/queue", {"message": text, "priority": priority})
        assert req("/api/alerts/queue/status")["pendingAlerts"] == 3
        handled = [req("/api/alerts/queue/process", method="POST")["processedAlert"] for _ in range(3)]
        assert handled == ["High second", "Medium third", "Low first"]
        req("/api/alerts/queue/process", method="POST", expected=404)
        req("/api/alerts/queue", {"message": "Invalid", "priority": 4}, expected=400)
        req(f"/api/devices/{aid}/faulty", method="POST")
        assert req(f"/api/devices/{aid}")["isFaulty"] is True
        assert req(f"/api/recommendations/{aid}")["isFaulty"] is True
        req(f"/api/devices/{aid}/recover", method="POST")
        assert req(f"/api/recommendations/{aid}")["isFaulty"] is False
        print("Part 2 feedback checks passed: independent search-command patterns, ambiguous searches, rejected commands, per-sensor recommendations, FIFO/history indexing, priority order, counts and actual fault status.")
    finally:
        if process.poll() is None:
            process.terminate()
            process.wait(timeout=10)
