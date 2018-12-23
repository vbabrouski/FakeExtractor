using System;
using System.Collections.Generic;
using System.Fakes;
using System.IO.Fakes;
using System.Linq;
//using System.Linq.Fakes;
using System.Text;
using System.Threading.Tasks;

namespace SamplesForFakeExtractor
{
    using DataFakes = System.Data.Fakes;
    using ai= ShimTextReader.AllInstances;

    public class TestClass1
    {
        public TestClass1()
        {
            ShimFile.GetAccessControlString = s => null;
            var i = ai.Dispose = reader => { };
            var stub = new DataFakes.StubDataRow(null);
            // ShimSqlConnection.Open = o => {};
        }
    }
}