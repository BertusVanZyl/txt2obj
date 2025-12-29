using txt2obj;

namespace YamlTemplateSmokeTestRunner
{
    internal static class Program
    {
        private static void Main()
        {
            YamlTemplateSmokeTest.Run();
            Console.WriteLine("YAML template smoke test passed.");
        }
    }
}
