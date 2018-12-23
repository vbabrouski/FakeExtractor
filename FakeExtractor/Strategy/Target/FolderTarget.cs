using System;
using System.Collections.Generic;
using System.IO;
using FakeExtractor.Helpers;
using static FakeExtractor.Helpers.Tracer;

namespace FakeExtractor.Strategy.Target
{
    public class FolderTarget : ITargetStrategy
    {
        private readonly string FolderPath;

        public FolderTarget(string folderPath)
        {
            FolderPath = folderPath ?? throw new ArgumentNullException(nameof(folderPath));
        }

        public void Process()
        {
            if (!Directory.Exists(FolderPath))
            {
                Warning($"'{FolderPath}' is missing.");
                return;
            }

            EmptyLine();
            Info($"Started working with the folder '{FolderPath}'");
            var projects = Directory.GetFiles(FolderPath, FileExtensions.ProjectSearchPattern, SearchOption.AllDirectories);
            Info($"Found {projects.Length} projects.");
            foreach (var project in projects)
            {
                var strategy = new ProjectTarget(project);
                strategy.Process();
                foreach (var config in strategy.FoundFakesConfiguration)
                {
                    FoundFakesConfiguration.Add(config);
                }
            }

            Info($"Stop working with the folder '{FolderPath}'");
        }

        public Dictionary<FakesType, HashSet<Type>> DetectedTypes { get; } = AssemblyTypeDetector.DetectedTypes;
        public HashSet<string> FoundFakesConfiguration { get; } = new HashSet<string>();
    }
}