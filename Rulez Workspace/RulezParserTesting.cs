using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTrack.Rulez ;

namespace OnTrack.Testing
{
    [TestClass]
    public class RulezParserTesting
    {
        private Rulez.Engine _engine = new Rulez.Engine();
        
        // Test-Sources
        String Test1 = "selection s1 as deliverables[100];";
        String Test2 = "selection s2 (p1 as number) as deliverables[p1];";
        String Test3 = "selection s3 (p1 as number) as deliverables[.uid=p1];";
        String Test4 = "selection s4 as deliverables[.uid=p1 as number];";
        [TestMethod]
        public void SelectionTest1()
        {
            try 
            { 
                Assert.IsFalse(_engine.Generate(Test1), "Failed: " + Test1 ) ;
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
       
        [TestMethod]
        public void SelectionTest2()
        {
            try
            {
                Assert.IsFalse(_engine.Generate(Test2), "Failed: " + Test2);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}