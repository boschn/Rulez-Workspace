using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTrack.Rulez ;

namespace OnTrack.Testing
{
    [TestClass]
    public class RulezParserTesting
    {
        private Rulez.Engine _engine = new Rulez.Engine();
        
        [TestMethod]
        public void SelectionTesting()
        {
            try 
            { 
                _engine.Generate("selection s1 as deliverables[100];");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}