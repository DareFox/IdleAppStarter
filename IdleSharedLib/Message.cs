namespace IdleSharedLib
{
    public class Message
    {
        private bool isIdle;
        private int processId;

        public bool IsIdle { get => isIdle; set => isIdle = value; }
        public int ProcessId { get => processId; set => processId = value; }
    }
}
