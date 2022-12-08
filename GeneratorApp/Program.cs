using TestGenerator;
using TestGenerator.CodeGenerators;

internal class Program
{
    public static void Main()
    {
        Console.WriteLine("Enter source files (STOP to move to the next step):");
        List<string> files = new List<string>();
        string? file = Console.ReadLine();
        while (file != "STOP")
        {
            if (!File.Exists(file)) Console.WriteLine($"Wrong file name: {file}");
            else if (Path.GetExtension(file) != ".cs") Console.WriteLine($"Wrong file extension: {file}");
            else files.Add(file);
            file = Console.ReadLine();
        }

        int maxDegreeOfRead = ReadInt("read");
        int maxDegreeOfGenerate = ReadInt("generate");
        int maxDegreeOfWrite = ReadInt("write");

        Console.WriteLine("Enter path to write:");
        string? path = Console.ReadLine();
        while (path == null)
        {
            Console.WriteLine("Invalid value. Try again.");
            path = Console.ReadLine();
        }

        var generator = new TestGenerator.TestGenerator(new NUnitTestCodeGenerator(), new GeneratorConfig(maxDegreeOfRead, maxDegreeOfGenerate, maxDegreeOfWrite));
        try
        {
            var task = generator.Generate(files.ToArray(), path);
            task.Wait();
            Console.WriteLine("Test generation complete");
        }
        catch 
        {
            Console.WriteLine("Test generation failed: it is not possible to write the result on the specified path.");
        }
    }

    public static int ReadInt(string param)
    {
        Console.WriteLine($"Enter maximum count of { param } tasks:");
        while (true)
        {
            string? read = Console.ReadLine();
            if (read != null)
            {
                int res = int.Parse(read);
                if (res > 0) return res;
                else Console.WriteLine("Invalid value. Try again.");
            }
            else Console.WriteLine("Invalid value. Try again.");
        }
    }
}
