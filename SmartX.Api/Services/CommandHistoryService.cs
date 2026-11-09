using PROG7312_SmartX.Models;

namespace PROG7312_SmartX.Services
{
    public class CommandHistoryService
    {
        private readonly Stack<DeviceCommand> commandStack = new();
        private readonly object commandLock = new();

        // Record a new command
        public void PushCommand(DeviceCommand command)
        {
            lock (commandLock)
            {
                commandStack.Push(command);
            }
        }

        // Undo the latest command
        public DeviceCommand? UndoLastCommand()
        {
            lock (commandLock)
            {
                if (commandStack.Count == 0)
                    return null;

                return commandStack.Pop();
            }
        }

        // Get commands
        public List<DeviceCommand> GetCommandHistory()
        {
            lock (commandLock)
            {
                return commandStack.ToList();
            }
        }

        // Count commands in the stack
        public int GetCommandCount()
        {
            lock (commandLock) //(Microsoft, 2026a)
            {
                return commandStack.Count;
            }
        }
    }
}
/*Reference List
 
 Microsoft, 2026a. lock statement (C# Reference). Available at: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock
 */