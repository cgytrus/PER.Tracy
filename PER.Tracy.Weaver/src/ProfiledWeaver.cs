using System.Collections.Immutable;
using System.Text;

using JetBrains.Annotations;

using Metalama.Compiler;
using Metalama.Framework.Engine.AspectWeavers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PER.Tracy.Weaver;

// this sucks so much but it works lmao
[MetalamaPlugIn]
[PublicAPI]
public class ProfiledWeaver : IAspectWeaver {
    private static bool _autoZones;

    public void Transform(AspectWeaverContext context) {
        if(!context.Project.PreprocessorSymbols.Contains("TracyEnable"))
            return;
        _autoZones = context.Project.PreprocessorSymbols.Contains("TracyAutoZones");
        context.RewriteAspectTargetsBruh(new CreateFieldsRewriter());
        context.RewriteAspectTargetsBruh(new ReplaceCallsRewriter());
    }

    private const string ZoneDefinitionName = "ZoneScoped";

    private static readonly HashSet<string> zoneCalls = new() {
        "ZoneText",
        "ZoneName",
        "ZoneColor",
        "ZoneValue"
    };

    // int is argument position, was supposed to be used for TracyAllocN/TracyFreeN
    // but i ended up not implementing these cuz too lazy
    private static readonly Dictionary<string, int> stringCalls = new() {
        { "ZoneText", 0 },
        { "ZoneName", 0 },
        { "FrameMarkNamed", 0 },
        { "FrameMarkStart", 0 },
        { "FrameMarkEnd", 0 },
        { "TracyPlot", 0 },
        { "TracyPlotConfig", 0 },
        { "TracyAppInfo", 0 },
        { "TracyMessage", 0 }
    };

    private const string CreateStringName = "CreateString";
    private const string CreateLocationName = "CreateLocation";
    private const string ZoneStartName = "StartScopedZone";
    private const string ZoneEndName = "EndScopedZone";

    private static string GetStringName(int stringIndex) => $"__tracy_string_{stringIndex.ToString()}";
    private static string GetLocationName(int zoneIndex) => $"__tracy_location_{zoneIndex.ToString()}";
    private static string GetZoneName(int zoneIndex) => $"__tracy_zone_{zoneIndex.ToString()}";

    private static readonly MemberAccessExpressionSyntax profilerInternal =
        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("PER"),
                SyntaxFactory.IdentifierName("Tracy")),
            SyntaxFactory.IdentifierName("ProfilerInternal"));

    private record struct Call(StatementSyntax statement, ArgumentListSyntax arguments, string name,
        FileLinePositionSpan location);

    private static MemberAccessExpressionSyntax CreateProfilerExpression(string name) =>
        CreateProfilerExpression(SyntaxFactory.IdentifierName(name));

    private static MemberAccessExpressionSyntax CreateProfilerExpression(IdentifierNameSyntax name) =>
        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, profilerInternal, name);

    private static IEnumerable<Call> FindAllCalls(IReadOnlyList<StatementSyntax> statements, SyntaxNode? node = null) {
        if(_autoZones && node?.Parent is MethodDeclarationSyntax or LocalFunctionStatementSyntax)
            yield return new Call(SyntaxFactory.EmptyStatement(), SyntaxFactory.ArgumentList(), ZoneDefinitionName,
                node.Parent.GetLocation().GetMappedLineSpan());

        foreach(StatementSyntax baseStatement in statements) {
            if(baseStatement is not ExpressionStatementSyntax {
                    Expression: InvocationExpressionSyntax {
                        Expression: MemberAccessExpressionSyntax expression
                    } invocation
                } statement)
                continue;
            string namespaceAndClassName = expression.Expression.ToString();
            if(namespaceAndClassName != "PER.Tracy.Profiler" && namespaceAndClassName != "Profiler")
                continue;
            yield return new Call(statement, invocation.ArgumentList, expression.Name.Identifier.ValueText,
                invocation.GetLocation().GetMappedLineSpan());
        }
    }

    private static bool TryFindDefinition(IReadOnlyList<StatementSyntax> statements, SyntaxNode node, out Call def) {
        def = new Call();
        foreach(Call call in FindAllCalls(statements, node)) {
            if(call.name != ZoneDefinitionName)
                continue;
            def = call;
            return true;
        }
        return false;
    }

    private static IEnumerable<Call> FindOtherCalls(IReadOnlyList<StatementSyntax> statements) =>
        FindAllCalls(statements).Where(call => call.name != ZoneDefinitionName);

    private static bool IsStringCall(Call call, out int argIndex) => stringCalls.TryGetValue(call.name, out argIndex) &&
        call.arguments.Arguments[argIndex].Expression.IsKind(SyntaxKind.StringLiteralExpression);

    private class CreateFieldsRewriter : CSharpSyntaxRewriter {
        private readonly List<MemberDeclarationSyntax> _fields = new();
        private int _nesting;
        private int _stringIndex;
        private int _zoneIndex;

        public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) =>
            VisitTypeDeclaration(node, base.VisitClassDeclaration);

        public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node) =>
            VisitTypeDeclaration(node, base.VisitStructDeclaration);

        public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax node) =>
            VisitTypeDeclaration(node, base.VisitRecordDeclaration);

        public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) =>
            VisitTypeDeclaration(node, base.VisitInterfaceDeclaration);

        private SyntaxNode? VisitTypeDeclaration<T>(T node, Func<T, SyntaxNode?> baseMethod)
            where T : TypeDeclarationSyntax {
            if(_nesting > 0)
                return baseMethod(node);
            _fields.Clear();
            _nesting++;
            T? visited = baseMethod(node) as T;
            _nesting--;
            return _fields.Count > 0 ? visited?.AddMembers(_fields.ToArray()) : visited;
        }

        public override SyntaxNode? VisitBlock(BlockSyntax node) {
            IReadOnlyList<StatementSyntax> statements = node.Statements;

            foreach(Call call in FindOtherCalls(statements)) {
                if(!IsStringCall(call, out int argIndex))
                    continue;
                _fields.Add(CreateStringField(GetStringName(_stringIndex++), call.arguments.Arguments[argIndex]));
            }

            if(!TryFindDefinition(statements, node, out Call definition))
                return base.VisitBlock(node);

            _fields.Add(CreateLocationField(GetLocationName(_zoneIndex++), GetFullSyntaxName(node),
                definition.arguments, definition.location));

            return base.VisitBlock(node);
        }

        private static FieldDeclarationSyntax CreateLocationField(string locationName, string methodName,
            ArgumentListSyntax arguments, FileLinePositionSpan location) => SyntaxFactory.FieldDeclaration(
            default(SyntaxList<AttributeListSyntax>),
            new SyntaxTokenList(
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword).WithTrailingTrivia(SyntaxFactory.ElasticSpace),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword).WithTrailingTrivia(SyntaxFactory.ElasticSpace),
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword).WithTrailingTrivia(SyntaxFactory.ElasticSpace)),
            SyntaxFactory.VariableDeclaration(
                SyntaxFactory.IdentifierName("nuint").WithTrailingTrivia(SyntaxFactory.ElasticSpace),
                SyntaxFactory.SeparatedList(new[] {
                    SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(locationName), null,
                        SyntaxFactory.EqualsValueClause(SyntaxFactory.InvocationExpression(
                            CreateProfilerExpression(CreateLocationName),
                            arguments.AddArguments(GetStringLiteralArgument(methodName),
                                GetStringLiteralArgument(location.Path),
                                GetNumericLiteralArgument((uint)location.StartLinePosition.Line + 1u)))))
                }))).WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);

        private static FieldDeclarationSyntax CreateStringField(string stringName, ArgumentSyntax str) {
            return SyntaxFactory.FieldDeclaration(default(SyntaxList<AttributeListSyntax>),
                new SyntaxTokenList(
                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword).WithTrailingTrivia(SyntaxFactory.ElasticSpace),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword).WithTrailingTrivia(SyntaxFactory.ElasticSpace),
                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword).WithTrailingTrivia(SyntaxFactory.ElasticSpace)),
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName("nuint").WithTrailingTrivia(SyntaxFactory.ElasticSpace),
                    SyntaxFactory.SeparatedList(new[] {
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(stringName), null,
                            SyntaxFactory.EqualsValueClause(SyntaxFactory.InvocationExpression(
                                CreateProfilerExpression(CreateStringName),
                                SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] { str })))))
                    }))).WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);
        }

        private static ArgumentSyntax GetStringLiteralArgument(string s) => SyntaxFactory.Argument(
            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(s)));

        private static ArgumentSyntax GetNumericLiteralArgument(uint value) => SyntaxFactory.Argument(
            SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(value)));

        private static string GetFullSyntaxName(SyntaxNode? node) {
            Stack<string> chain = new();
            bool ended = false;
            while(node is not null) {
                switch(node) {
                    case LocalFunctionStatementSyntax n:
                        chain.Push(n.Identifier.ToString());
                        break;
                    case MethodDeclarationSyntax n:
                        chain.Push(n.Identifier.ToString());
                        break;
                    case BaseTypeDeclarationSyntax n:
                        chain.Push(n.Identifier.ToString());
                        break;
                    case BaseNamespaceDeclarationSyntax n:
                        chain.Push(n.Name.ToString());
                        ended = true;
                        break;
                }
                node = node.Parent;
            }
            if(!ended)
                chain.Push("<unknown>");
            StringBuilder builder = new();
            builder.Append(chain.Pop());
            while(chain.Count > 0) {
                builder.Append("::");
                builder.Append(chain.Pop());
            }
            return builder.ToString();
        }
    }

    private class ReplaceCallsRewriter : CSharpSyntaxRewriter {
        private int _stringIndex;
        private int _zoneIndex;

        public override SyntaxNode? VisitBlock(BlockSyntax node) {
            List<StatementSyntax> statements = node.Statements.ToList();

            string zoneName = GetZoneName(_zoneIndex);

            foreach(Call call in FindOtherCalls(statements).ToImmutableHashSet()) {
                List<ArgumentSyntax> args = call.arguments.Arguments.ToList();

                if(IsStringCall(call, out int argIndex))
                    args[argIndex] =
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(GetStringName(_stringIndex++)));

                if(zoneCalls.Contains(call.name))
                    args.Insert(0, SyntaxFactory.Argument(SyntaxFactory.IdentifierName(zoneName)));

                statements[statements.IndexOf(call.statement)] = SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(CreateProfilerExpression(call.name),
                        call.arguments.WithArguments(SyntaxFactory.SeparatedList(args))));
            }

            if(!TryFindDefinition(statements, node, out Call definition))
                return base.VisitBlock(node.WithStatements(SyntaxFactory.List(statements)));

            statements.Remove(definition.statement);
            return base.VisitBlock(CreateZone(node.WithStatements(SyntaxFactory.List(statements)), zoneName,
                GetLocationName(_zoneIndex++)));
        }

        private static BlockSyntax CreateZone(BlockSyntax block, string zoneName, string locationName) =>
            SyntaxFactory.Block(SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.IdentifierName("var").WithTrailingTrivia(SyntaxFactory.ElasticSpace),
                    SyntaxFactory.SeparatedList(new[] {
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(zoneName), null,
                            SyntaxFactory.EqualsValueClause(SyntaxFactory.InvocationExpression(
                                CreateProfilerExpression(ZoneStartName),
                                SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] {
                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName(locationName))
                                })))))
                    })))
            .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed), SyntaxFactory.TryStatement(block,
            SyntaxFactory.List<CatchClauseSyntax>(),
            SyntaxFactory.FinallyClause(SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(CreateProfilerExpression(ZoneEndName),
                    SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new[] {
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(zoneName))
                    }))))))));
    }
}
