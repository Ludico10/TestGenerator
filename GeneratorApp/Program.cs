using TestGenerator;
using TestGenerator.CodeGenerators;

internal class Program
{
    private static void Main()
    {
        List<string> files = new List<string>(1);
        const string writeFolder = @"..\..\..\..\TestClasses\";
        const string path = @"..\..\..\..\SourceClasses\SourceClass";

        for (int i = 0; i < files.Capacity; i++)
        {
            var fileName = path + i.ToString() + ".cs";
            files.Add(fileName);
        }

        var testGenerator = new TestGenerator.TestGenerator(new NUnitTestCodeGenerator(), new GeneratorConfig());
        var task = testGenerator.Generate(files.ToArray(), writeFolder);
        task.Wait();
    }
}