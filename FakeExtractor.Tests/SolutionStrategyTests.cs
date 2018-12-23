using FakeExtractor.Strategy.Target;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FakeExtractor.Tests
{
    [TestClass]
    public class SolutionStrategyTests
    {
        private SolutionTarget _solutionTarget;

        [TestInitialize]
        public void Init()
        {
            const string path = @"c:\projects\crossover\bootcamp\gfi-mail-archiver\HUT\HandcraftedUT.sln";
            _solutionTarget = new SolutionTarget(path);
        }

        [TestMethod]
        public void Test()
        {
            _solutionTarget.Process();
        }
    }
}