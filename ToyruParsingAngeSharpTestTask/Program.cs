namespace ToyruParsingAngeSharpTestTask
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser parser = new();

            _ = parser.GetToys().Result;
        }
    }
}