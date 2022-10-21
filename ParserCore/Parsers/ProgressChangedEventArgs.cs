namespace ParserCore.Parsers
{
    public class ProgressChangedEventArgs : EventArgs
    {
        public string TextStatus { get; set; } = "";
        public int Percentage { get; set; } = 0;
        public List<string> ItemsProcessed { get; set; } = new();
    }
}
