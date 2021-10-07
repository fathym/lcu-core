using Fathym;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LCU.Graphs.Tests
{
    [TestClass]
    public class TestSymbols
    {
        [TestMethod]
        public void TestStepThrough()
        {
            var blah = "this is audit named: ";            
            var audit = new Audit()
            {
                At = System.DateTime.Now,
                By = "Testing",
                Description = "TestDesc",
                Details = "TestDetails"
            };

            Console.WriteLine($"{blah}{audit.Details}");
        }
    }
}
