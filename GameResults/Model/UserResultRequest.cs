namespace GameResults.Model
{
    public class UserResultRequest
    {
        public int Id { get; set; }
        public int Duration { get; set; }
        public int Moves { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
    }
}
