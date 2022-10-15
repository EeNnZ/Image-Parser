namespace ParserCore
{
    public class ProgressInfo
    {
        public string TextStatus { get; set; } = "";
        public int Percentage { get; set; } = 0;
        public List<string> ItemsProcessed { get; set; } = new();
    }
}
