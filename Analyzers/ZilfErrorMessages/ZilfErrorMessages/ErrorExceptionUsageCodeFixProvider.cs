﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using Microsoft.CodeAnalysis.Formatting;

namespace ZilfErrorMessages
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ErrorExceptionUsageCodeFixProvider)), Shared]
    public class ErrorExceptionUsageCodeFixProvider : CodeFixProvider
    {
        private const string title = "Convert message to diagnostic constant";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(ErrorExceptionUsageAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return new ErrorExceptionUsageFixAllProvider();
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // find the 'new XError()' expression
            var creation = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ObjectCreationExpressionSyntax>().First();

            string exceptionTypeName;
            ExpressionSyntax locationSyntax;
            LiteralExpressionSyntax literalSyntax;

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);

            if (ErrorExceptionUsageAnalyzer.TryMatchLiteralCreation(creation, semanticModel, out exceptionTypeName,
                out locationSyntax, out literalSyntax))
            {
                // Register a code action that will invoke the fix.
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedSolution: c => ConvertMessagesToConstantsAsync(context.Document, new[] { creation }, c),
                        equivalenceKey: title),
                    diagnostic);
            }
        }

        private class Invocation
        {
            public string MessagesTypeName;
            public LiteralExpressionSyntax LiteralSyntax;
            public MemberAccessExpressionSyntax ConstantAccessSyntax;
            public Func<int, FieldDeclarationSyntax> GetConstantDeclarationSyntax;
        }

        private async Task<Solution> ConvertMessagesToConstantsAsync(Document document, ObjectCreationExpressionSyntax[] creations, CancellationToken cancellationToken)
        {
            var invocations = await GetInvocationsAsync(document, creations, cancellationToken);
            return await ApplyInvocationsAsync(
                document.Project.Solution,
                Enumerable.Repeat(new KeyValuePair<DocumentId, Invocation[]>(document.Id, invocations), 1),
                cancellationToken);
        }

        private static async Task<Invocation[]> GetInvocationsAsync(Document document, ObjectCreationExpressionSyntax[] creations, CancellationToken cancellationToken)
        {
            var solution = document.Project.Solution;

            var invocations = new List<Invocation>();

            foreach (var creation in creations)
            {
                string exceptionTypeName;
                ExpressionSyntax locationSyntax;
                LiteralExpressionSyntax literalSyntax;

                // get the name of the class where the constant will go
                var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
                var qualifiedFormat = new SymbolDisplayFormat(
                    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
                var messagesTypeAnnotation = new SyntaxAnnotation("MessagesTypeTracker");

                if (!ErrorExceptionUsageAnalyzer.TryMatchLiteralCreation(creation, semanticModel, out exceptionTypeName,
                    out locationSyntax, out literalSyntax))
                {
                    throw new InvalidOperationException();
                }

                var messagesTypeName = ZilfFacts.MessageTypeMap[exceptionTypeName];
                var messagesTypeSyntax = SyntaxFactory.ParseTypeName(messagesTypeName)
                    .WithAdditionalAnnotations(messagesTypeAnnotation);

                // compute the name of the new constant
                var constantName = GetConstantName(literalSyntax.Token.ValueText);
                var constantNameSyntax = SyntaxFactory.IdentifierName(constantName);

                // replace the invocation
                var constantAccessSyntax = SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    messagesTypeSyntax,
                    constantNameSyntax);

                invocations.Add(new Invocation
                {
                    ConstantAccessSyntax = constantAccessSyntax,
                    LiteralSyntax = literalSyntax,
                    MessagesTypeName = messagesTypeName,
                    GetConstantDeclarationSyntax = code => MakeConstantSyntax(literalSyntax, constantName, code)
                });
            }

            return invocations.ToArray();
        }

        private static async Task<Solution> ApplyInvocationsAsync(Solution solution, IEnumerable<KeyValuePair<DocumentId, Invocation[]>> invocationsByDocument, CancellationToken cancellationToken)
        {
            // apply changes at invocation sites
            foreach (var pair in invocationsByDocument)
            {
                var document = solution.GetDocument(pair.Key);
                var invocations = pair.Value;

                var syntaxMapping = invocations.ToDictionary(i => (SyntaxNode)i.LiteralSyntax, i => i.ConstantAccessSyntax);

                var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken);
                var newSyntaxRoot = syntaxRoot.ReplaceSyntax(syntaxMapping.Keys, (node, _) => syntaxMapping[node], null, null, null, null);

                solution = solution.WithDocumentSyntaxRoot(pair.Key, AddUsingIfNeeded(newSyntaxRoot));
            }

            // refresh model and find message set types where we need to add constants
            var constantsByTypeName = from pair in invocationsByDocument
                                      from inv in pair.Value
                                      group inv.GetConstantDeclarationSyntax by inv.MessagesTypeName;

            foreach (var group in constantsByTypeName)
            {
                // find type
                bool ok = false;

                foreach (var project in solution.Projects)
                {
                    var compilation = await project.GetCompilationAsync(cancellationToken);

                    var messagesTypeSymbol = GetAllTypes(compilation).FirstOrDefault(t => t.Name == group.Key);

                    if (messagesTypeSymbol != null)
                    {
                        var messagesDefinition = messagesTypeSymbol.OriginalDefinition;
                        var messagesDefSyntaxRef = messagesDefinition.DeclaringSyntaxReferences.First();
                        var messagesDefDocument = solution.GetDocument(messagesDefSyntaxRef.SyntaxTree);

                        var messagesDefSyntaxRoot = await messagesDefDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                        var messagesDefSyntax = (ClassDeclarationSyntax)await messagesDefSyntaxRef.GetSyntaxAsync(cancellationToken).ConfigureAwait(false);

                        // find next unused message number
                        var nextCode = (from child in messagesDefSyntax.DescendantNodes().OfType<FieldDeclarationSyntax>()
                                        where child.Modifiers.Any(SyntaxKind.ConstKeyword)
                                        from v in child.Declaration.Variables
                                        let initializer = v.Initializer.Value as LiteralExpressionSyntax
                                        where initializer != null && initializer.Kind() == SyntaxKind.NumericLiteralExpression
                                        select (int)initializer.Token.Value)
                                       .Concat(Enumerable.Repeat(1, 1))
                                       .Max() + 1;

                        var newConstants = group.Select((getConstant, i) => getConstant(nextCode + i)).ToArray();

                        var newMessagesDefSyntax = messagesDefSyntax.AddMembers(newConstants);
                        var newSyntaxRoot = messagesDefSyntaxRoot.ReplaceNode(messagesDefSyntax, newMessagesDefSyntax);

                        solution = solution.WithDocumentSyntaxRoot(messagesDefDocument.Id, newSyntaxRoot);
                        ok = true;
                        break;
                    }
                }

                if (!ok)
                    throw new InvalidOperationException("Can't find message set type " + group.Key);
            }

            return solution;
        }

        private static IEnumerable<INamedTypeSymbol> GetAllTypes(Compilation compilation)
        {
            var nsQueue = new Queue<INamespaceSymbol>();
            nsQueue.Enqueue(compilation.GlobalNamespace);

            while (nsQueue.Count > 0)
            {
                var ns = nsQueue.Dequeue();

                foreach (var t in ns.GetTypeMembers())
                {
                    yield return t;
                }

                foreach (var cns in ns.GetNamespaceMembers())
                {
                    nsQueue.Enqueue(cns);
                }
            }
        }

        private static SyntaxNode AddUsingIfNeeded(SyntaxNode syntaxRoot)
        {
            if (syntaxRoot is CompilationUnitSyntax)
            {
                var compilationUnitSyntax = (CompilationUnitSyntax)syntaxRoot;
                if (!compilationUnitSyntax.Usings.Any(
                    u => u.Name.ToString() == "Zilf.Diagnostics"))
                {
                    var newCompilationUnit = compilationUnitSyntax.AddUsings(
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.ParseName("Zilf.Diagnostics")));
                    return newCompilationUnit;
                }
            }

            return syntaxRoot;
        }

        private static FieldDeclarationSyntax MakeConstantSyntax(LiteralExpressionSyntax literalSyntax, string constantName, int code)
        {
            return SyntaxFactory.FieldDeclaration(
                attributeLists: SyntaxFactory.List(new[]
                {
                    SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(
                            name: SyntaxFactory.ParseName("Message"),
                            argumentList: SyntaxFactory.AttributeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.AttributeArgument(literalSyntax))))))
                }),
                modifiers: SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                    SyntaxFactory.Token(SyntaxKind.ConstKeyword)),
                declaration: SyntaxFactory.VariableDeclaration(
                    type: SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                    variables: SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                            identifier: SyntaxFactory.Identifier(constantName),
                            argumentList: null,
                            initializer: SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
                                    SyntaxFactory.Literal(code)))))),
                semicolonToken: SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        private static string GetConstantName(string message)
        {
            var sb = new StringBuilder();
            bool capNext = true;

            foreach (char c in message)
            {
                if (char.IsWhiteSpace(c))
                {
                    sb.Append('_');
                    capNext = true;
                }
                else if (char.IsLetterOrDigit(c))
                {
                    if (capNext)
                        sb.Append(char.ToUpperInvariant(c));
                    else
                        sb.Append(c);

                    capNext = false;
                }
            }

            if (sb.Length == 0)
                return "__InvalidMessage";

            return sb.ToString();
        }

        private class ErrorExceptionUsageFixAllProvider : FixAllProvider
        {
            public override async Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
            {
                var diagnosticsToFix = new List<KeyValuePair<Project, ImmutableArray<Diagnostic>>>();
                string titleFormat = "Convert all messages in {0} {1} to diagnostic constants";
                string fixAllTitle = null;

                switch (fixAllContext.Scope)
                {
                    case FixAllScope.Document:
                        {
                            var diagnostics = await fixAllContext.GetDocumentDiagnosticsAsync(fixAllContext.Document).ConfigureAwait(false);
                            diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(fixAllContext.Project, diagnostics));
                            fixAllTitle = string.Format(titleFormat, "document", fixAllContext.Document.Name);
                            break;
                        }

                    case FixAllScope.Project:
                        {
                            var project = fixAllContext.Project;
                            ImmutableArray<Diagnostic> diagnostics = await fixAllContext.GetAllDiagnosticsAsync(project).ConfigureAwait(false);
                            diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(fixAllContext.Project, diagnostics));
                            fixAllTitle = string.Format(titleFormat, "project", fixAllContext.Project.Name);
                            break;
                        }

                    case FixAllScope.Solution:
                        {
                            foreach (var project in fixAllContext.Solution.Projects)
                            {
                                ImmutableArray<Diagnostic> diagnostics = await fixAllContext.GetAllDiagnosticsAsync(project).ConfigureAwait(false);
                                diagnosticsToFix.Add(new KeyValuePair<Project, ImmutableArray<Diagnostic>>(project, diagnostics));
                            }

                            fixAllTitle = "Add all items in the solution to the public API";
                            break;
                        }

                    case FixAllScope.Custom:
                        return null;

                    default:
                        throw new ArgumentException("Unknown scope", nameof(fixAllContext));
                }

                return CodeAction.Create(
                    title: fixAllTitle,
                    createChangedSolution: async c =>
                    {
                        var diagsByDocument = from ds in diagnosticsToFix
                                              from d in ds.Value
                                              where d.Location.IsInSource
                                              let document = fixAllContext.Solution.GetDocument(d.Location.SourceTree)
                                              group d by document;

                        Func<Document, IEnumerable<Diagnostic>, Task<KeyValuePair<DocumentId, Invocation[]>>> getInvocations =
                            async (doc, diags) =>
                            {
                                var root = await doc.GetSyntaxRootAsync(c).ConfigureAwait(false);
                                var creations = from d in diags
                                                let span = d.Location.SourceSpan
                                                let ancestors = root.FindToken(span.Start).Parent.AncestorsAndSelf()
                                                select ancestors.OfType<ObjectCreationExpressionSyntax>().First();
                                var invocations = await GetInvocationsAsync(doc, creations.ToArray(), c);
                                return new KeyValuePair<DocumentId, Invocation[]>(doc.Id, invocations);
                            };

                        var results = await Task.WhenAll(from grouping in diagsByDocument
                                                         select getInvocations(grouping.Key, grouping));

                        return await ApplyInvocationsAsync(fixAllContext.Solution, results, c);
                    },
                    equivalenceKey: fixAllTitle);
            }
        }
    }
}