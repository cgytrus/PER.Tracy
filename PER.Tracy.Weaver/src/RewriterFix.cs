using System.Collections.Immutable;

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.CodeModel;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PER.Tracy.Weaver;

internal static class RewriterFix {
    public static void RewriteAspectTargetsBruh(this AspectWeaverContext context, CSharpSyntaxRewriter rewriter) {
        IEnumerable<IGrouping<SyntaxTree, Microsoft.CodeAnalysis.SyntaxReference>> groupings = context.AspectInstances
            .Values
            .Select((Func<IAspectInstance, ISymbol>)(a =>
                a.TargetDeclaration.GetSymbol(context.Compilation.Compilation)!))
            .Where((Func<ISymbol, bool>)(s => s != null))
            .SelectMany(
                (Func<ISymbol, IEnumerable<Microsoft.CodeAnalysis.SyntaxReference>>)(s => s.DeclaringSyntaxReferences))
            .GroupBy((Func<Microsoft.CodeAnalysis.SyntaxReference, SyntaxTree>)(r => r.SyntaxTree));
        List<SyntaxTreeModification> modifications = new();
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach(IGrouping<SyntaxTree, Microsoft.CodeAnalysis.SyntaxReference> source in groupings) {
            SyntaxTree key = source.Key;
            Rewriter rewriter1 =
                new(
                    source.Select((Func<Microsoft.CodeAnalysis.SyntaxReference, SyntaxNode>)(r => r.GetSyntax()))
                        .ToImmutableHashSet(), rewriter);
            SyntaxNode root1 = key.GetRoot();
            SyntaxNode root2 = rewriter1.Visit(root1)!;
            if(root1 != root2)
                modifications.Add(new SyntaxTreeModification(key.WithRootAndOptions(root2, key.Options), key));
        }
        context.Compilation = context.Compilation.WithSyntaxTreeModifications(modifications);
    }

    private class Rewriter : CSharpSyntaxRewriter {
        private readonly ImmutableHashSet<SyntaxNode> _targets;
        private readonly CSharpSyntaxRewriter _userRewriter;

        public Rewriter(ImmutableHashSet<SyntaxNode> targets, CSharpSyntaxRewriter userRewriter) {
            _userRewriter = userRewriter;
            _targets = targets;
        }

        public override SyntaxNode? Visit(SyntaxNode? node) {
            switch(node) {
                case CompilationUnitSyntax:
                    return base.Visit(node);
                case MemberDeclarationSyntax:
                case AccessorDeclarationSyntax:
                    if(_targets.Contains(node))
                        return _userRewriter.Visit(node);
                    switch(node) {
                        case BaseTypeDeclarationSyntax:
                        // lmaooo this was NamespaceDeclarationSyntax so it broke with file scoped namespaces
                        // i had to copy the entire decompiled code just to fix this
                        // upd 19.07.2022: updated to 0.5.27-preview, they still didn't fix this lol
                        // upd 20.07.2022, 0.5.28-preview: no fix
                        case BaseNamespaceDeclarationSyntax:
                            return base.Visit(node);
                    }
                    break;
            }
            return node;
        }
    }
}
