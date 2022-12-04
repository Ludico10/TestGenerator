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
                if (!File.Exists(file))
                {
                    throw new FileNotFoundException($"File not found: {file}");
                }
                readFileBlock.Post(file);
            }
            readFileBlock.Complete();
            return writeFileBlock.Completion;
        }

        private static async Task<string> ReadFileAsync(string fileName)
        {
            using StreamReader sr = new StreamReader(fileName);
            return await sr.ReadToEndAsync();
        }

        private static async Task WriteFileAsync(string text, string writeFolder)
        {
            var root = await CSharpSyntaxTree.ParseText(text).GetRootAsync();
            var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
            var path = writeFolder + classDeclaration.Identifier.Text + "_" + Guid.NewGuid().ToString() + ".cs";
            using StreamWriter sw = new StreamWriter(path);
            await sw.WriteAsync(text);
        }
    }
}
