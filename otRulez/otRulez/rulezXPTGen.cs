/**
*  ONTRACK RULEZ ENGINE
*  
* rulez engine eXPression Tree generator out an ANTLR parse tree
* 
* Version: 1.0
* Created: 2015-07-14
* Last Change
* 
* Change Log
* 
* (C) by Boris Schneider, 2015
* 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;
using OnTrack.Rulez.eXPressionTree;
using OnTrack.Core;

namespace OnTrack.Rulez
{
    /// <summary>
    /// listener to generate a XPTree out of a ANTLR parse tree
    /// </summary>
    public class XPTGenerator : RulezParserBaseListener 
    {
        private RulezParser _parser;
        private eXPressionTree.XPTree _xptree; // the output tree
        private Engine _engine;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="parser"></param>
        public XPTGenerator(RulezParser parser, Engine engine = null)
        {
            _parser = parser;
            if (engine == null) _engine = parser.Engine;
            else { _engine = engine; }
        }
       
        /// <summary>
        /// gets the resulted tree
        /// </summary>
        public XPTree XPTree
        {
            get
            {
                return _xptree;
            }
            private set { _xptree = value; }
        }
        /// <summary>
        /// gets the associated Engine
        /// </summary>
        public Engine Engine
        {
            get
            {
                return _engine;
            }
        }
        /// <summary>
        /// enter a rule rule
        /// </summary>
        /// <param name="context"></param>
        public override void EnterSelectionRulez(RulezParser.SelectionRulezContext context)
        {
            if (context.XPTreeNode == null) context.XPTreeNode = new SelectionRule(engine: this.Engine);
            // set the _xptree by a new SelectionRule xPTree
            if (this.XPTree == null) this.XPTree = (XPTree)context.XPTreeNode;
        }

        /// <summary>
        /// exit a rule rule
        /// </summary>
        /// <param name="context"></param>
        public override void ExitSelectionRulez(RulezParser.SelectionRulezContext context)
        {
            
        }

        /// <summary>
        /// exit a parameter definition
        /// </summary>
        /// <param name="context"></param>
        public override void ExitParameterdefinition(RulezParser.ParameterdefinitionContext context)
        {
          
        }
    }
}