namespace codingTracker._0lcm.Models
{
    internal class Timer
    {
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public bool IsStopped { get; private set; }

        public Timer()
        {
            StartTime = DateTime.Now;
        }

        public void Stop()
        {
            EndTime = DateTime.Now;
            IsStopped = true;
        }
    }
}
