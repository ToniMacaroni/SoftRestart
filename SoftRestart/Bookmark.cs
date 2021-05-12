namespace SoftRestart
{
    public class Bookmark
    {
        public bool IsSet { get; private set; }

        public float Time { get; private set; }

        public void Set(float time)
        {
            Time = time;
            IsSet = true;
        }
    }
}