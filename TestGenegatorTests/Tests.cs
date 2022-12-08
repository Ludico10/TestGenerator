using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestGenerator;
using TestGenerator.CodeGenerators;
using static System.Net.Mime.MediaTypeNames;

namespace TestGenegatorTests
{
    [TestFixture]
    public class Tests
    {
        private readonly TestGenerator.TestGenerator defaultGenerator;
        private readonly GeneratorConfig defaultConfig = new();
        const string writeFolder = @"..\..\..\..\TestClasses\";
        const string path = @"..\..\..\..\SourceClasses\SourceClass";
        private readonly string[] files = new string[12];

        public Tests()
        {
            defaultGenerator = new(new NUnitTestCodeGenerator(), defaultConfig);

            if (Directory.Exists(writeFolder)) Directory.Delete(writeFolder, true);
            for (int i = 0; i < files.Length; i++)
            {
                var fileName = path + i.ToString() + ".cs";
                files[i] = fileName;
            }
            var task = defaultGenerator.Generate(files, writeFolder);
            task.Wait();
        }

        [Test]
        public void OneDegreeTest()
        {
            var oneGeneratorConfig = new GeneratorConfig(1, 1, 1);
            var testOneGenerator = new TestGenerator.TestGenerator(new NUnitTestCodeGenerator(), oneGeneratorConfig);
            var task = testOneGenerator.Generate(files, writeFolder);
            Assert.That(task, Is.Not.Null);
            task.Wait();
        }

        [Test]
        public void ManyNamespasesTest() 
        {
            var test = ManyNamespacesTask();
            test.Wait();
        }

        private async Task ManyNamespacesTask()
        {
            var text = await TestGenerator.TestGenerator.ReadFileAsync(@"..\..\..\..\TestClasses\ManyNamespaces.Tests.ManyNamespacesTests.cs");
            var root = await CSharpSyntaxTree.ParseText(text).GetRootAsync();
            var space = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>();
            Assert.That(space.Count(), Is.EqualTo(1));
            Assert.That(space.First().Name.ToString(), Is.EqualTo("ManyNamespaces.Tests"));
            text = await TestGenerator.TestGenerator.ReadFileAsync(@"..\..\..\..\TestClasses\ManyManyNamespaces.Tests.ManyManyNamespacesTests.cs");
            root = await CSharpSyntaxTree.ParseText(text).GetRootAsync();
            space = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>();
            Assert.That(space.Count(), Is.EqualTo(1));
            Assert.That(space.First().Name.ToString(), Is.EqualTo("ManyManyNamespaces.Tests"));
        }

        [Test]
        public void GlobalNamespaceTest()
        {
            var test = GlobalNamespaceTask();
            test.Wait();
        }

        private async Task GlobalNamespaceTask()
        {
            var text = await TestGenerator.TestGenerator.ReadFileAsync(@"..\..\..\..\TestClasses\GlobalNamespace.Tests.GlobalNamespace1Tests.cs");
            var root = await CSharpSyntaxTree.ParseText(text).GetRootAsync();
            var space = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>();
            Assert.That(space.Count(), Is.EqualTo(1));
            Assert.That(space.First().Name.ToString(), Is.EqualTo("GlobalNamespace.Tests"));
            text = await TestGenerator.TestGenerator.ReadFileAsync(@"..\..\..\..\TestClasses\GlobalNamespace.Tests.GlobalNamespace2Tests.cs");
            root = await CSharpSyntaxTree.ParseText(text).GetRootAsync();
            space = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>();
            Assert.That(space.Count(), Is.EqualTo(1));
            Assert.That(space.First().Name.ToString(), Is.EqualTo("GlobalNamespace.Tests"));
        }

        [Test]
        public void CrossedUsingsTest()
        {
            var test = CrossedUsingsTask();
            test.Wait();
        }

        private async Task CrossedUsingsTask()
        {
            var text = await TestGenerator.TestGenerator.ReadFileAsync(@"..\..\..\..\TestClasses\CrossedUsings.Tests.CrossedUsingsTests.cs");
            var root = await CSharpSyntaxTree.ParseText(text).GetRootAsync();
            var usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
            Assert.That(usings.Count(), Is.EqualTo(8));
        }

        [Test]
        public void OverrideTest()
        {
            var test = OverrideTask();
            test.Wait();
        }

        private async Task OverrideTask()
        {
            var text = await TestGenerator.TestGenerator.ReadFileAsync(@"..\..\..\..\TestClasses\Overrider.Tests.OverriderTests.cs");
            var root = await CSharpSyntaxTree.ParseText(text).GetRootAsync();
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            Assert.That(methods.Count(), Is.EqualTo(2));
            Assert.That(methods.First().Identifier.Text, Is.EqualTo("OverrideTest"));
            Assert.That(methods.Last().Identifier.Text, Is.EqualTo("Override2Test"));
        }

        [Test]
        public void PrivateTest()
        {
            var test = PrivateTask();
            test.Wait();
        }

        private async Task PrivateTask()
        {
            var text = await TestGenerator.TestGenerator.ReadFileAsync(@"..\..\..\..\TestClasses\Tests.PrivateTests.cs");
            var root = await CSharpSyntaxTree.ParseText(text).GetRootAsync();
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            Assert.That(methods.Count(), Is.EqualTo(0));
        }
    }
}
