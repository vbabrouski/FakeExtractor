using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using FakeExtractor.Strategy;

namespace FakeExtractor.Helpers
{
    public static class FakesConfigurationConverter
    {
        private const string FakesUri = "http://schemas.microsoft.com/fakes/2011/";
        private static readonly XNamespace FakesNamespace = FakesUri;
        private static readonly XName FakesElementName = FakesNamespace + "Fakes";
        private static readonly XName AssemblyElementName = FakesNamespace + "Assembly";
        private static readonly XName StubGenerationElementName = FakesNamespace + "StubGeneration";
        private static readonly XName ShimGenerationElementName = FakesNamespace + "ShimGeneration";
        private static readonly XName ClearElementName = FakesNamespace + "Clear";
        private static readonly XName AddElementName = FakesNamespace + "Add";
        private static readonly XName Compilation = FakesNamespace + "Compilation";

        public static IDictionary<string, string> Convert(ITargetStrategy strategy)
        {
            var result = new Dictionary<string, string>();
            var types = strategy?.DetectedTypes;
            if (types != null)
            {
                var fakes = LoadFakesFiles(strategy?.FoundFakesConfiguration);
                var configuration = new Dictionary<string, XElement>();
                foreach (var fakeType in types.Keys)
                {
                    foreach (var type in types[fakeType])
                    {
                        var assemblyName = type?.Assembly.GetName();
                        var baseName = assemblyName?.Name;
                        if (string.IsNullOrWhiteSpace(baseName))
                        {
                            continue;
                        }

                        if (!configuration.ContainsKey(baseName))
                        {
                            XElement compilation = null;
                            if (fakes.ContainsKey(baseName))
                            {
                                compilation = fakes[baseName].Element(Compilation);
                            }

                            configuration[baseName] = CreateBaseFakeElement(baseName, AssemblyVersion(assemblyName), compilation);
                        }

                        var baseElement = configuration[baseName];
                        AddTypeToBaseElement(baseElement, type, fakeType);
                    }
                }

                if (configuration.Count > 0)
                {
                    result = configuration.ToDictionary(x => x.Key, y => y.Value.ToString());
                }
            }

            return result;
        }

        private static string AssemblyVersion(AssemblyName assemblyName)
        {
            return assemblyName?.Version?.ToString();
        }

        private static XElement CreateBaseFakeElement(string assemblyName, string version, XElement compilation)
        {
            var element = new XElement(FakesElementName,
                                       new XAttribute("xmlns", FakesNamespace.NamespaceName),
                                       new XElement(AssemblyElementName, new XAttribute("Name", assemblyName), new XAttribute("Version", version)),
                                       new XElement(StubGenerationElementName, new XElement(ClearElementName)),
                                       new XElement(ShimGenerationElementName, new XElement(ClearElementName)));
            if (compilation != null)
            {
                element.Add(compilation);
            }

            return element;
        }

        private static void AddTypeToBaseElement(XElement baseElement, Type type, FakesType fakeType)
        {
            var fullName = $"{type?.FullName}!";
            if (fakeType == FakesType.Shim)
            {
                var shim = baseElement?.Element(ShimGenerationElementName);
                shim?.Add(new XElement(AddElementName, new XAttribute("FullName", fullName)));
                return;
            }

            if (fakeType == FakesType.Stub)
            {
                var stub = baseElement?.Element(StubGenerationElementName);
                stub?.Add(new XElement(AddElementName, new XAttribute("FullName", fullName)));
            }
        }

        private static Dictionary<string, XElement> LoadFakesFiles(IEnumerable<string> fileNames)
        {
            var result = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
            if (fileNames != null)
            {
                foreach (var fileName in fileNames)
                {
                    var key = Path.GetFileNameWithoutExtension(fileName);
                    result[key] = LoadFakesFile(fileName);
                }
            }

            return result;
        }

        private static XElement LoadFakesFile(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName) &&
                File.Exists(fileName))
            {
                return XElement.Load(fileName);
            }

            return null;
        }
    }
}