﻿/**
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
        private eXPressionTree.IeXPressionTree _xptree; // the output tree

        private eXPressionTree.SelectionRule _currentSelectionRule; // the output tree

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="parser"></param>
        public XPTGenerator(RulezParser parser)
        {
            _parser = parser;
        }

        /// <summary>
        /// gets the resulted tree
        /// </summary>
        public IeXPressionTree XPTree
        {
            get
            {
                return _xptree;
            }
        }

        /// <summary>
        /// enter a rule rule
        /// </summary>
        /// <param name="context"></param>
        public override void EnterSelectionRulez(RulezParser.SelectionRulezContext context)
        {
            _currentSelectionRule = new SelectionRule();
            // set the _xptree by a new SelectionRule xPTree
            if (_xptree == null)
            {
                _xptree = _currentSelectionRule;
            }
        }

        /// <summary>
        /// exit a rule rule
        /// </summary>
        /// <param name="context"></param>
        public override void ExitSelectionRulez(RulezParser.SelectionRulezContext context)
        {
            foreach (Antlr4.Runtime.Tree.IParseTree child in context.children)
            {
                if (child.GetType() == typeof(RulezParser.RuleidContext))
                    _currentSelectionRule.ID = child.GetText();
            }

            // reset -> hopefully the rule rule is included in the _xptree
            _currentSelectionRule = null;
        }

        /// <summary>
        /// exit a parameter definition
        /// </summary>
        /// <param name="context"></param>
        public override void ExitParameterdefinition(RulezParser.ParameterdefinitionContext context)
        {
            if (_currentSelectionRule != null)
            {
                if (_currentSelectionRule.HasParameter(context.IDENTIFIER().GetText()))
                {
                    ;
                }
                otDataType aType = otDataType.Void;
                // compare the sub-token
                if (context.valuetype().NUMBER() != null) aType = otDataType.Number;
                else if (context.valuetype().DECIMAL() != null) aType = otDataType.Decimal;
                else if (context.valuetype().TEXT() != null) aType = otDataType.Text;
                else if (context.valuetype().DATE() != null) aType = otDataType.Date;
                else if (context.valuetype().TIMESTAMP() != null) aType = otDataType.Timestamp;
                else if (context.valuetype().MEMO () != null) aType = otDataType.Memo;
                else if (context.valuetype().LIST () != null) aType = otDataType.List;
                else { throw new RulezException(RulezException.Types.DataTypeNotImplemented, arguments: new object[] { context.valuetype().GetText(), "XPTGenerator.ExitParameterdefinition" }); }
               
                // add the parameter
                _currentSelectionRule.AddNewParameter(id: context.IDENTIFIER().GetText(), type: aType);
            }
        }
    }
}