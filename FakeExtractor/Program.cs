using System.Configuration;
using System.Diagnostics;
using FakeExtractor.Helpers;
using FakeExtractor.Strategy;
using FakeExtractor.Strategy.Output;
using FakeExtractor.Strategy.Target;
using static FakeExtractor.Helpers.Tracer;

namespace FakeExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            var listener = new ConsoleTraceListener
            {
                Name = "[Fake Extractor]",
                TraceOutputOptions = TraceOptions.Timestamp
            };
            Trace.Listeners.Clear();
            Trace.Listeners.Add(listener);
            Info("FakeExtractor: Start working...");
            StartStrategy();
            Info("FakeExtractor: Stop working...");
        }

        private static void StartStrategy()
        {
            var extractorOptions = ReadOptions();
            var target = extractorOptions?.Target;
            var targetPath = extractorOptions?.TargetPath;
            ITargetStrategy targetStrategy;
            if (target.EqualTo("Folder"))
            {
                targetStrategy = new FolderTarget(targetPath);
            }
            else if (target.EqualTo("Solution"))
            {
                targetStrategy = new SolutionTarget(targetPath);
            }
            else if (target.EqualTo("Project"))
            {
                targetStrategy = new ProjectTarget(targetPath);
            }
            else
            {
                Warning($"Unknown target '{target}'");
                return;
            }

            var outputStrategy = new SingleFolder(extractorOptions?.OutputFolder, targetStrategy);
            outputStrategy.Process();
        }

        /// <summary>
        /// Read the extractor options from App.config
        /// </summary>
        /// <returns></returns>
        private static ExtractorOptions ReadOptions()
        {
            var extractorOptions = new ExtractorOptions();
            var appSettings = ConfigurationManager.AppSettings;
            foreach (string key in appSettings.Keys)
            {
                extractorOptions.ApplyValue(key, appSettings.Get(key));
            }

            return extractorOptions;
        }
    }
}