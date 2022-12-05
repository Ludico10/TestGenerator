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

        protected IEnumerable<string> GenerateNamespaces(CompilationUnitSyntax root)
        {
            var spaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();
            var usings = root.Usings.AddRange(GetDefaultUsings());
            usings = List(usings.UnionBy(GetFileUsings(root), u => u.Name.ToString()));

            var allClasses = new List<string>();
            foreach (var ns in spaces) 
            {
                allClasses.AddRange(GenerateClasses(ns, usings));
            }
            return allClasses;
        }

        protected abstract List<UsingDirectiveSyntax> GetDefaultUsings();

        private SyntaxList<UsingDirectiveSyntax> GetFileUsings(CompilationUnitSyntax root)
        {
            return List(root.DescendantNodes().OfType<UsingDirectiveSyntax>());
        }

        private IEnumerable<string> GenerateClasses(NamespaceDeclarationSyntax sourceNamespace, SyntaxList<UsingDirectiveSyntax> usings)
        {
            var newNamespace = CreateNewNamespace(sourceNamespace);
            var nsUsing = new SyntaxList<UsingDirectiveSyntax>();
            nsUsing.Add(UsingDirective(IdentifierName(sourceNamespace.Name.ToString())));
            usings = List(usings.UnionBy(nsUsing, u => u.Name.ToString()));

            var classes = sourceNamespace.DescendantNodes().OfType<ClassDeclarationSyntax>();
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
