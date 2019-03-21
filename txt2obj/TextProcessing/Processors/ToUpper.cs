namespace txt2obj.TextProcessing.Processors
{
    public class ToUpper : IStringProcessor
    {
        public string Name => "ToUpper";
        public string[] Parameters { get; set; }
        public string Execute(string input)
        {
            return input.ToUpper();
        }
    }
}
