using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestGenerator.CodeGenerators
{
    public sealed class NUnitTestCodeGenerator : BaseTestCodeGenerator, ICodeGenerator
    {
        public NUnitTestCodeGenerator() : base("Test")
        {
        }

        public string[] Generate(string text)
        {
            var root = CSharpSyntaxTree.ParseText(text).GetCompilationUnitRoot();
            return GenerateClasses(root).ToArray();
        }

        protected override List<UsingDirectiveSyntax> GetDefaultUsings()
        {
            var res = new List<UsingDirectiveSyntax>();
            res.Add(UsingDirective(IdentifierName("System")));
            var usingName = QualifiedName(IdentifierName("System"), IdentifierName("Collections"));
            res.Add(UsingDirective(QualifiedName(usingName, IdentifierName("Generic"))));
            res.Add(UsingDirective(QualifiedName(IdentifierName("System"), IdentifierName("Linq"))));
            res.Add(UsingDirective(QualifiedName(IdentifierName("System"), IdentifierName("Text"))));
            usingName = QualifiedName(IdentifierName("System"), IdentifierName("Threading"));
            res.Add(UsingDirective(QualifiedName(usingName, IdentifierName("Tasks"))));
            res.Add(UsingDirective(QualifiedName(IdentifierName("NUnit"), IdentifierName("Framework"))));
            return res;
        }

        protected override StatementSyntax GetUnitTestBody()
        {
            return ExpressionStatement(
              InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("Assert"), IdentifierName("Fail")),
                ArgumentList(SeparatedList(
                    new List<ArgumentSyntax>() {
                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("autogenerated")))
                    } ))));
        }
    }
}
