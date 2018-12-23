using System.IO;
using FakeExtractor.Helpers;
using static FakeExtractor.Helpers.Tracer;

namespace FakeExtractor.Strategy.Output
{
    public class SingleFolder : IOutputStrategy
    {
        private readonly string OutputFolder;
        private readonly ITargetStrategy TargetStrategy;

        public SingleFolder(string outputFolder, ITargetStrategy targetStrategy)
        {
            OutputFolder = outputFolder;
            TargetStrategy = targetStrategy;
        }

        public void Process()
        {
            if (Validate())
            {
                TargetStrategy?.Process();
            }

            var configuration = FakesConfigurationConverter.Convert(TargetStrategy);
            if (configuration?.Count > 0)
            {
                foreach (var item in configuration)
                {
                    var fileName = Path.Combine(OutputFolder, $"{item.Key}.fakes");
                    File.WriteAllText(fileName, item.Value);
                }
            }
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(OutputFolder))
            {
                Warning("No output folder specified.");
                return false;
            }

            if (TargetStrategy == null)
            {
                Warning("No target strategy specified.");
                return false;
            }

            return true;
        }
    }
}