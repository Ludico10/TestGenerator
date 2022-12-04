using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.Formatting;

namespace TestGenerator.CodeGenerators
{
    public abstract class BaseTestCodeGenerator
    {
        private readonly AttributeListSyntax attr;
        private readonly StatementSyntax body;

        protected BaseTestCodeGenerator(string attr)
        {
            this.attr = CreateUnitTestAttribute(attr);
            body = GetUnitTestBody();
        }

        private AttributeListSyntax CreateUnitTestAttribute(string attrName)
        {
            return AttributeList(
                        SingletonSeparatedList(
                                 Attribute(IdentifierName(attrName))));
        }

        protected abstract StatementSyntax GetUnitTestBody();

        protected IEnumerable<string> GenerateClasses(CompilationUnitSyntax root)
        {
            var sourceNamespace = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            var newNamespace = CreateNewNamespace(sourceNamespace);

            var usings = root.Usings.AddRange(GetDefaultUsings());

            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            var testClasses = new List<string>();
            foreach (var classDeclaration in classes)
            {
                testClasses.Add(GenerateClass(classDeclaration, newNamespace, in usings));
            }
            return testClasses;
        }

        private NamespaceDeclarationSyntax CreateNewNamespace(NamespaceDeclarationSyntax? sourceNamespace)
        {
            if (sourceNamespace != null) return NamespaceDeclaration(IdentifierName($"{sourceNamespace.Name}.Tests"));
            else return NamespaceDeclaration(IdentifierName("Tests"));
        }

        protected abstract List<UsingDirectiveSyntax> GetDefaultUsings();

        private string GenerateClass(ClassDeclarationSyntax classDeclaration, NamespaceDeclarationSyntax newNamespace, in SyntaxList<UsingDirectiveSyntax> usings)
        {
            var newClass = ClassDeclaration(classDeclaration.Identifier.Text + "Tests")
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));

            var publicMethods = GetPublicMethods(classDeclaration);
            newClass = newClass.AddMembers(GenerateTestMethods(publicMethods).ToArray());

            var compilationUnit = CompilationUnit();
            compilationUnit = compilationUnit.AddUsings(usings.ToArray());
            compilationUnit = compilationUnit.AddMembers(newNamespace.AddMembers(newClass));
            return compilationUnit.NormalizeWhitespace().ToString();
        }

        private IEnumerable<MethodDeclarationSyntax> GetPublicMethods(ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>().
                Where(m => m.Modifiers.Any(x => x.IsKind(SyntaxKind.PublicKeyword)));
        }

        private IEnumerable<MethodDeclarationSyntax> GenerateTestMethods(IEnumerable<MethodDeclarationSyntax> sourceMethods)
        {
            var overloader = new Dictionary<string, int>();
            var newMethods = new List<MethodDeclarationSyntax>();
            foreach (var method in sourceMethods)
            {
                string key = method.Identifier.Text;
                if (overloader.ContainsKey(key))
                {
                    overloader[key]++;
                    newMethods.Add(GenerateTestMethod(key + overloader[key].ToString() + "Test"));
                }
                else
                {
                    overloader.Add(key, 1);
                    newMethods.Add(GenerateTestMethod(key + "Test"));
                }
            }
            return newMethods;
        }

        private MethodDeclarationSyntax GenerateTestMethod(string methodName)
        {
            return MethodDeclaration(attributeLists: List<AttributeListSyntax>().Add(attr),
                modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
                returnType: PredefinedType(Token(SyntaxKind.VoidKeyword)),
                explicitInterfaceSpecifier: null,
                identifier: Identifier(methodName + "Test"),
                typeParameterList: null,
                parameterList: ParameterList(),
                constraintClauses: List<TypeParameterConstraintClauseSyntax>(),
                body: Block(body),
                semicolonToken: new SyntaxToken()
                ).WithAdditionalAnnotations(Formatter.Annotation);
        }
    }
}
