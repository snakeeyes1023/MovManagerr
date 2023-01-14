namespace MovManagerr.Core.Services.Bases.ContentService
{
    public class SearchQuery
    {
        public string EnteredText { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; } = 50;
    }
}
