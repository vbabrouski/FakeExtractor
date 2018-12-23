using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FakeExtractor.Helpers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FakeExtractor.Strategy.Target
{
    public class FileTarget : ITargetStrategy
    {
        private CompilationUnitSyntax _syntax;

        public FileFakesDefinition Definition { get; }

        public FileTarget(string path)
        {
            if (IsCsharpFile(path) &&
                File.Exists(path))
            {
                Definition = new FileFakesDefinition { Path = Path.GetFullPath(path) };
            }
        }

        public void Process()
        {
            LoadFile();
            Parse();
        }

        private void Parse()
        {
            if (_syntax != null)
            {
                var nodes = _syntax.DescendantNodes()?.ToArray();
                var qualified = nodes?.Where(x => x is QualifiedNameSyntax).Select(x => x as QualifiedNameSyntax);
                ProcessQualified(qualified);
                var identifiers = nodes?.Where(x => x is IdentifierNameSyntax).Select(x => x as IdentifierNameSyntax);
                ProcessIdentifiers(identifiers);
                var generics = nodes?.Where(x => x is GenericNameSyntax).Select(x => x as GenericNameSyntax);
                ProcessGenerics(generics);
            }
        }

        private void ProcessQualified(IEnumerable<QualifiedNameSyntax> nodes)
        {
            if (nodes != null)
            {
                foreach (var node in nodes.Where(x => x != null))
                {
                    if (node.ToString().ContainsOrdinalIgnoreCase("Microsoft.QualityTools.Testing.Fakes"))
                    {
                        continue;
                    }

                    var right = node.Right?.ToString();
                    if (right.EqualTo(ParsingParts.FakesNamespaceSuffix))
                    {
                        Definition?.Namespaces.AddIfNotNull(node.ToString());
                        continue;
                    }

                    if (AddIfShimOrStub(right))
                    {
                        continue;
                    }

                    var left = node.Left?.ToString();
                    if (AddIfShim(left))
                    {
                        continue;
                    }

                    AddIfStub(left);
                }
            }
        }

        private void ProcessGenerics(IEnumerable<GenericNameSyntax> nodes)
        {
            if (nodes != null)
            {
                foreach (var node in nodes.Where(x=>x!=null))
                {
                    AddIfShimOrStub($"{node.Identifier.ValueText}`{node.TypeArgumentList.Arguments.Count}");
                }
            }
        }

        private bool AddIfShimOrStub(string item)
        {
            return AddIfShim(item) || AddIfStub(item);
        }

        private bool AddIfShim(string item)
        {
            if (IsShim(item))
            {
                Definition?.Shims.AddIfNotNull(item);
                return true;
            }

            return false;
        }

        private bool AddIfStub(string item)
        {
            if (IsStub(item))
            {
                Definition?.Stubs.AddIfNotNull(item);
                return true;
            }

            return false;
        }

        private bool IsShim(string item) => item.StartsWithOrdinal(ParsingParts.ShimPrefix);

        private bool IsStub(string item) => item.StartsWithOrdinal(ParsingParts.StubPrefix);

        private bool IsShimOrStub(string item) => IsShim(item) || IsStub(item);

        private void ProcessIdentifiers(IEnumerable<IdentifierNameSyntax> nodes)
        {
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var nodeString = node?.ToString();
                    if (IsShimOrStub(nodeString))
                    {
                        var identifier = GetIdentifierWithNamespace(node);
                        if (IsShim(nodeString))
                        {
                            Definition?.Shims.AddIfNotNull(identifier);
                        }
                        else
                        {
                            Definition?.Stubs.AddIfNotNull(identifier);
                        }
                    }
                }
            }
        }

        private string GetIdentifierWithNamespace(IdentifierNameSyntax node)
        {
            var identifier = node.ToString();
            var parent = node.Parent as MemberAccessExpressionSyntax;
            while (parent != null && parent.ToString().EndsWithOrdinalIgnoreCase(identifier))
            {
                identifier = parent.ToString();
                parent = parent.Parent as MemberAccessExpressionSyntax;
            }

            return identifier;
        }

        private void LoadFile()
        {
            if (!string.IsNullOrWhiteSpace(Definition?.Path))
            {
                var content = File.ReadAllText(Definition.Path);
                var syntax = CSharpSyntaxTree.ParseText(content);
                if (syntax?.HasCompilationUnitRoot == true)
                {
                    _syntax = syntax.GetCompilationUnitRoot();
                }
            }
        }

        private bool IsCsharpFile(string path) => path?.EndsWith(FileExtensions.CSharpFileExtension, StringComparison.OrdinalIgnoreCase) == true;
        public Dictionary<FakesType, HashSet<Type>> DetectedTypes { get; } = null;
        public HashSet<string> FoundFakesConfiguration { get; } = null;
    }
}