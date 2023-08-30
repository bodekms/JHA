namespace MyTracker.Models
{
    public class CountStore
    {
        public int CountOnInitalRequest { get; set; }
        public int CurrentCount { get; set; }

        public int DifferenceAfterMostRecentRequest => CurrentCount - CountOnInitalRequest;

        public CountStore(int countOnInitialRequest, int currentCount)
        {
            CountOnInitalRequest = countOnInitialRequest;
            CurrentCount = currentCount;
        }
    }
}
