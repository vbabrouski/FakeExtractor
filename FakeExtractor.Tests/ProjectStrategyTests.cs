using System.Linq;
using System.Reflection;
using FakeExtractor.Strategy.Target;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace FakeExtractor.Tests
{
    [TestClass]
    public class ProjectStrategyTests
    {
        private ProjectTarget _projectTarget;

        [TestInitialize]
        public void Init()
        {
            const string projectPath = @"c:\projects\crossover\bootcamp\gfi-mail-archiver\HUT\Web\MarUI.UnitTest\MarUI.UnitTest.csproj";
            //const string projectPath = @"C:\projects\crossover\bootcamp\gfi-mail-archiver\HUT\Web\MArc.Web.Services.Helper.FakesTests\MArc.Web.Services.Helper.FakesTests.csproj";
            _projectTarget = new ProjectTarget(projectPath);
        }

        [TestMethod]
        public void Process_ExistingProject_ShouldReadDefinition()
        {
            _projectTarget?.Process();
        }

        [TestMethod]
        public void Process_Test()
        {
            const string assemblyPath = @"c:\projects\crossover\bootcamp\gfi-mail-archiver\HUT\MArc.HUT.Common\FakesAssemblies\MarUI.10.0.0.0.Fakes.dll";
            //var types = _projectTarget?.LoadAssembly(assemblyPath);
        }
    }
}