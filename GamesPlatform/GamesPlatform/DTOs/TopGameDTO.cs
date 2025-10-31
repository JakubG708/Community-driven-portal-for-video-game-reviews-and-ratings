namespace GamesPlatform.DTOs
{
    public class TopGameDTO
    {
        public int GameId { get; set; }
        public string Title { get; set; } = "";
        public double Score { get; set; }
        public int RatingsCount { get; set; }
    }
}