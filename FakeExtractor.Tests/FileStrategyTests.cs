using System.IO;
using FakeExtractor.Strategy.Target;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FakeExtractor.Tests
{
    [TestClass]
    public class FileStrategyTests
    {
        private const string TestFilePath = @"..\..\..\SamplesForFakeExtractor\TestClass1.cs";

        [TestInitialize]
        public void Init()
        {
        }

        [TestMethod]
        public void FileStrategy_NotCsharpFile()
        {
            var strategy = CreateFileStrategy(string.Empty);
            Assert.IsNull(strategy?.Definition?.Path);
        }

        [TestMethod]
        public void FileStrategy_FileNotExist()
        {
            var strategy = CreateFileStrategy("a.cs");
            Assert.IsNull(strategy?.Definition?.Path);
        }

        [TestMethod]
        public void FileStrategy_FileExists()
        {
            var strategy = CreateFileStrategy(TestFilePath);
            strategy?.Process();

            Assert.IsNotNull(strategy?.Definition?.Path);
            Assert.IsNotNull(strategy?.Definition?.Namespaces);
        }

        private FileTarget CreateFileStrategy(string relativePath)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath ?? string.Empty);
            return new FileTarget(fullPath);
        }
    }
}