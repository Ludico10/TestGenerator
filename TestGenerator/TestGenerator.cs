using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks.Dataflow;

namespace TestGenerator
{
    public sealed class TestGenerator
    {
        private readonly ICodeGenerator generator;
        private readonly GeneratorConfig generatorConfig;

        public TestGenerator(ICodeGenerator generator, GeneratorConfig generatorConfig)
        {
            this.generator = generator;
            this.generatorConfig = generatorConfig;
        }

        public Task Generate(string[] files, string writeFolder)
        {

            var readFileBlock = new TransformBlock<string, string>(
                async fileName => await ReadFileAsync(fileName),
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = generatorConfig.MaxDegreeOfRead,
                });

            var generateCodeBlock = new TransformManyBlock<string, string>(
                text => generator.Generate(text),
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = generatorConfig.MaxDegreeOfGenerate,
                });

            var writeFileBlock = new ActionBlock<string>(
                async text => await WriteFileAsync(text, writeFolder),
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = generatorConfig.MaxDegreeOfWrite,
                });

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            readFileBlock.LinkTo(generateCodeBlock, linkOptions);
            generateCodeBlock.LinkTo(writeFileBlock, linkOptions);

            if (!Directory.Exists(writeFolder))
            {
                Directory.CreateDirectory(writeFolder);
            }

            foreach (var file in files)
            {
                if (File.Exists(file)) 
                    readFileBlock.Post(file);
            }
            readFileBlock.Complete();
            return writeFileBlock.Completion;
        }

        public static async Task<string> ReadFileAsync(string fileName)
        {
            using StreamReader sr = new StreamReader(fileName);
            return await sr.ReadToEndAsync();
        }

        private static async Task WriteFileAsync(string text, string writeFolder)
        {
            var root = await CSharpSyntaxTree.ParseText(text).GetRootAsync();
            var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var path = writeFolder + GetPrefix(classDeclaration) + classDeclaration.Identifier.Text + ".cs";
            using StreamWriter sw = new StreamWriter(path);
            await sw.WriteAsync(text);
        }

        private static string GetPrefix(ClassDeclarationSyntax classDeclaration)
        {
            string res = "";
            if (classDeclaration.Parent != null && classDeclaration.Parent.GetType().Name == "NamespaceDeclarationSyntax")
            {
                var space = (BaseNamespaceDeclarationSyntax)classDeclaration.Parent;
                res = space.Name.ToString() + ".";
            }
            return res;
        }
    }
}
