using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using FakeExtractor.Helpers;
using static FakeExtractor.Helpers.Tracer;

namespace FakeExtractor.Strategy.Target
{
    public class ProjectTarget : ITargetStrategy
    {
        private readonly string ProjectFilePath;
        private AppDomain _fakeDomain;
        private XElement _projectXml;

        public ProjectTarget(string projectFilePath)
        {
            ProjectFilePath = projectFilePath ?? throw new ArgumentNullException(nameof(projectFilePath));
        }

        public void Process()
        {
            EmptyLine();
            Info($"Start processing the project '{ProjectFilePath}'");
            if (!FileIsProject())
            {
                return;
            }

            if (!ProjectExists())
            {
                return;
            }

            AssemblyTypeDetector.AddAssemblySearchPath(ProjectDirectory);
            _projectXml = XElement.Load(ProjectFilePath);
            ProcessFiles();
            ProcessReferences();
            SearchFakesConfigurationFiles();
            Info($"Stop processing the project '{ProjectFilePath}'");
        }

        private void SearchFakesConfigurationFiles()
        {
            var fakesPath = Path.Combine(ProjectDirectory, "Fakes");
            if (!Directory.Exists(fakesPath))
            {
                return;
            }

            var files = Directory.GetFiles(fakesPath, "*.fakes");
            foreach (var file in files)
            {
                FoundFakesConfiguration.Add(file);
            }
        }

        private void ProcessReferences()
        {
            AssemblyTypeDetector.AddReferencePaths(new[] { ProjectDirectory });
            var references = _projectXml
                .Descendants(XName.Get("Reference", Namespace))
                .Select(x => new
                {
                    x.Element(XName.Get("HintPath", Namespace))?.Value,
                    Include = x.Attribute(XName.Get("Include"))?.Value
                }).ToArray();

            Info($"Project: detected {references.Length} references.");

            var fileReferences = references
                .Where(x => x.Value.EndsWithOrdinalIgnoreCase(FileExtensions.LibraryFileExtension))
                .ToArray();
            var referencePaths = fileReferences
                .Select(x => Path.GetDirectoryName(GetFullPath(x.Value)))
                .Distinct()
                .ToArray();
            AssemblyTypeDetector.AddReferencePaths(referencePaths);

            AssemblyTypeDetector.AddAssembly(Assembly.Load("mscorlib").Location);
            foreach (var reference in fileReferences)
            {
                if (reference.Include.EndsWithOrdinalIgnoreCase(".Fakes"))
                {
                    Info($"Project: add '{reference.Include}' assembly to check types.");
                    AssemblyTypeDetector.AddAssembly(GetFullPath(reference.Value));
                }
                else
                {
                    Info($"Project: '{reference.Include}' is not fake assembly.");
                }
            }
        }

        private void ProcessFiles()
        {
            var filePaths = _projectXml
                .Descendants(XName.Get("Compile", Namespace))
                .Select(x => x.Attribute(XName.Get("Include"))?.Value)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();
            Info($"Project: detected {filePaths.Length} file(s) for compilation.");

            foreach (var filePath in filePaths)
            {
                var fullFilePath = GetFullPath(filePath);
                var fileStrategy = new FileTarget(fullFilePath);
                fileStrategy.Process();
                AssemblyTypeDetector.AddTypeDefinitions(fileStrategy.Definition.AllTypeCombinations);
            }
        }

        private string GetFullPath(string relativePath)
        {
            var path = Path.GetFullPath(Path.Combine(ProjectDirectory, relativePath));
            return path;
        }

        private string Namespace => _projectXml?.Name.NamespaceName ?? string.Empty;

        private string ProjectDirectory => Path.GetDirectoryName(ProjectFilePath);

        private bool FileIsProject()
        {
            if (ProjectFilePath.EndsWith(FileExtensions.ProjectFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            Warning($"The project file '{ProjectFilePath}' must have the extension '{FileExtensions.ProjectFileExtension}'");
            return false;
        }

        private bool ProjectExists()
        {
            var exits = File.Exists(ProjectFilePath);
            if (!exits)
            {
                Warning($"The project '{ProjectFilePath}' is missing.");
            }

            return exits;
        }

        public Dictionary<FakesType, HashSet<Type>> DetectedTypes { get; } = AssemblyTypeDetector.DetectedTypes;
        public HashSet<string> FoundFakesConfiguration { get; } = new HashSet<string>();
    }
}