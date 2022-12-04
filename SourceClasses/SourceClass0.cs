namespace TestGenerator
{
    public class GeneratorConfig
    {
        public int MaxDegreeOfRead { get; } = 5;
        public int MaxDegreeOfGenerate { get; } = 5;
        public int MaxDegreeOfWrite { get; } = 5;

        public GeneratorConfig(int maxDegreeOfRead, int maxDegreeOfGenerate, int maxDegreeOfWrite)
        {
            MaxDegreeOfRead = maxDegreeOfRead;
            MaxDegreeOfGenerate = maxDegreeOfGenerate;
            MaxDegreeOfWrite = maxDegreeOfWrite;
        }

        public GeneratorConfig() { }
    }
}
