using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FakeExtractor.Helpers;
using Onion.SolutionParser.Parser;
using static FakeExtractor.Helpers.Tracer;

namespace FakeExtractor.Strategy.Target
{
    public class SolutionTarget : ITargetStrategy
    {
        private readonly string SolutionFilePath;

        public SolutionTarget(string solutionFilePath)
        {
            SolutionFilePath = solutionFilePath ?? throw new ArgumentNullException(nameof(solutionFilePath));
        }

        public void Process()
        {
            EmptyLine();
            Info($"Start processing the solution '{SolutionFilePath}'");
            if (!FileIsSolution())
            {
                return;
            }

            if (!SolutionExists())
            {
                return;
            }

            var solution = SolutionParser.Parse(SolutionFilePath);
            var projects = solution.Projects.ToArray();
            foreach (var project in projects)
            {
                Info($"Found project: {project.Name} on {project.Path}");
                var projectStrategy = new ProjectTarget(GetFullPath(project.Path));
                projectStrategy.Process();
                foreach (var config in projectStrategy.FoundFakesConfiguration)
                {
                    FoundFakesConfiguration.Add(config);
                }
            }

            Info($"Stop processing the solution '{SolutionFilePath}'");
        }

        private bool SolutionExists()
        {
            var exits = File.Exists(SolutionFilePath);
            if (!exits)
            {
                Warning($"The solution '{SolutionFilePath}' is missing.");
            }

            return exits;
        }

        private bool FileIsSolution()
        {
            if (SolutionFilePath.EndsWith(FileExtensions.SolutionFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            Warning($"The solution file '{SolutionFilePath}' must have the extension '{FileExtensions.SolutionFileExtension}'");
            return false;
        }

        private string SolutionDirectory => Path.GetDirectoryName(SolutionFilePath);

        private string GetFullPath(string relativePath) => Path.GetFullPath(Path.Combine(SolutionDirectory, relativePath));

        public Dictionary<FakesType, HashSet<Type>> DetectedTypes { get; } = AssemblyTypeDetector.DetectedTypes;
        public HashSet<string> FoundFakesConfiguration { get; } = new HashSet<string>();
    }
}