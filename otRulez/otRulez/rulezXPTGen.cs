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
using OnTrack.Rulez;
using OnTrack.Rulez.Resources;

namespace OnTrack.Rulez
{
    /// <summary>
    /// lister to generate all the declarations in a symbol table
    /// </summary>
    public class XPTDeclarationGenerator : RulezParserBaseListener
    {
        private RulezParser _parser;
        private eXPressionTree.XPTree _xptree; // the output tree
        private Engine _engine;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="parser"></param>
        public XPTDeclarationGenerator(RulezParser parser, Engine engine = null)
        {
            _parser = parser;
            if (engine == null) _engine = parser.Engine;
            else { _engine = engine; }
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
        /// define nodes
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitTypeDeclarationContext(RulezParser.TypeDeclarationContext ctx)
        {
            // selection Rulez

            return ;
        }
        /// <summary>
        /// define nodes
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitVariableDeclaration (RulezParser.VariableDeclarationContext ctx)
        {
            // selection Rulez

            return;
        }
        /// <summary>
        /// builds the XPT node of this
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitSelectionRulez(RulezParser.SelectionRulezContext ctx)
        {
            // store the symbol for the rule
            // plus signature

            // create local scope and add the parameters

            // $ctx.XPTreeNode = (eXPressionTree.IeXPressionTree) new SelectionRule($ctx.ruleid().GetText(), engine: this.Engine);
            // get the name
            SelectionRule aRule = new SelectionRule(ctx.ruleid().GetText(), engine: this.Engine);
            ctx.XPTreeNode = aRule;

            
            // add the parameters
            foreach (RulezParser.ParameterDefinition aParameter in ctx.names.Values)
            {
                ISymbol symbol = aRule.AddNewParameter(aParameter.name, datatype: aParameter.datatype);
                // defaultvalue assignment
                if (aParameter.defaultvalue != null) aRule.Selection.Nodes.Insert(0, new eXPressionTree.IfThenElse(
                    eXPressionTree.CompareExpression.EQ(symbol, new Literal(null, otDataType.@Null)),
                    new eXPressionTree.Assignment(symbol, (IExpression)aParameter.defaultvalue)));
            }
            return ;
        }
        /// <summary>
        /// build the XPTNode of a select statement block
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitSelectStatementBlock(RulezParser.SelectStatementBlockContext ctx)
        {
            // create a local scope and add the variables

            if (ctx.XPTreeNode == null) ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.SelectionStatementBlock();
            SelectionStatementBlock aBlock = (SelectionStatementBlock)ctx.XPTreeNode;

            // add the defined variables to the XPT
            foreach (RulezParser.VariableDefinition aVariable in ctx.names.Values)
            {
                ISymbol symbol = aBlock.Variables.Where ( X=> X.ID == aVariable.name).FirstOrDefault ();
                if (symbol == null) 
                    symbol = aBlock.AddNewVariable(aVariable.name, datatype: aVariable.datatype);
                // defaultvalue assignment
                if (aVariable.defaultvalue != null) aBlock.Nodes.Insert(0,new eXPressionTree.Assignment(symbol, (IExpression)aVariable.defaultvalue));
            }
        }
        
    }
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
        /// define nodes
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitRulezUnit(RulezParser.RulezUnitContext ctx)
        {
            // selection Rulez
            if (ctx.oneRulez() != null && ctx.oneRulez().Count() > 0)
            {
                if (ctx.XPTreeNode == null) ctx.XPTreeNode = new eXPressionTree.Unit(this.Engine);
                foreach (RulezParser.OneRulezContext aCtx in ctx.oneRulez()) ctx.XPTreeNode.Add((IXPTree)aCtx.XPTreeNode);
                return ;
            }

            return ;
        }
        /// <summary>
        /// define nodes
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitOneRulez(RulezParser.OneRulezContext ctx)
        {
            // selection Rulez
            if (ctx.selectionRulez() != null)
            {
                ctx.XPTreeNode = ctx.selectionRulez().XPTreeNode;
                return ;
            }
            if (ctx.typeDeclaration() != null)
            {
                ctx.XPTreeNode = null;
                return ;
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
        /// builds the XPT node of this
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitSelectionRulez(RulezParser.SelectionRulezContext ctx)
        {
            // $ctx.XPTreeNode = (eXPressionTree.IeXPressionTree) new SelectionRule($ctx.ruleid().GetText(), engine: this.Engine);
            // get the name
            SelectionRule aRule = new SelectionRule(ctx.ruleid().GetText(), engine: this.Engine);
            ctx.XPTreeNode = aRule;

            // add expression
            if (ctx.selection() != null)
            {
                aRule.Selection = new SelectionStatementBlock(engine: this.Engine);
                aRule.Selection.Add(new @Return((SelectionExpression)ctx.selection().XPTreeNode));
            }
            else if (ctx.selectStatementBlock() != null) aRule.Selection = (SelectionStatementBlock)ctx.selectStatementBlock().XPTreeNode;
            // add the parameters
            foreach (RulezParser.ParameterDefinition aParameter in ctx.names.Values)
            {
                ISymbol symbol = aRule.AddNewParameter(aParameter.name, datatype: aParameter.datatype);
                // defaultvalue assignment
                if (aParameter.defaultvalue != null) aRule.Selection.Nodes.Insert(0, new eXPressionTree.IfThenElse(
                    eXPressionTree.CompareExpression.EQ(symbol, new Literal(null, otDataType.@Null)),
                    new eXPressionTree.Assignment(symbol, (IExpression)aParameter.defaultvalue)));
            }
            return ;
        }
        /// <summary>
        /// build the XPTNode of a select statement block
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitSelectStatementBlock(RulezParser.SelectStatementBlockContext ctx)
        {
            if (ctx.XPTreeNode == null) ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.SelectionStatementBlock();
            SelectionStatementBlock aBlock = (SelectionStatementBlock)ctx.XPTreeNode;

            // add the defined variables to the XPT
            foreach (RulezParser.VariableDefinition aVariable in ctx.names.Values)
            {
                ISymbol symbol = aBlock.Variables.Where ( X=> X.ID == aVariable.name).FirstOrDefault ();
                if (symbol == null) 
                    symbol = aBlock.AddNewVariable(aVariable.name, datatype: aVariable.datatype);
                // defaultvalue assignment
                if (aVariable.defaultvalue != null) aBlock.Nodes.Insert(0,new eXPressionTree.Assignment(symbol, (IExpression)aVariable.defaultvalue));
            }
            // add statements
            foreach (RulezParser.SelectStatementContext statementCTX in ctx.selectStatement())
            {
                // add it to the Block
                aBlock.Nodes.Add((IStatement)statementCTX.XPTreeNode);
            }

            return;
        }
        /// <summary>
        /// build the XPTNode of a select statement 
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitSelectStatement(RulezParser.SelectStatementContext ctx)
        {

            return ;
        }
        /// <summary>
        /// build the XPTNode of an assignment context statement 
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitAssignment(RulezParser.AssignmentContext ctx)
        {

            return;
        }
        /// <summary>
        /// build the XPTNode of an assignment context statement 
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitMatch(RulezParser.MatchContext ctx)
        {

            return ;
        }
        public override void ExitMatchcase (RulezParser.MatchcaseContext ctx)
        {

            return ;
        }
        /// <summary>
        /// build the XPTNode of a return statement
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitReturn(RulezParser.ReturnContext ctx)
        {
            if (ctx.selectExpression() != null)
            {
                ctx.XPTreeNode = new Return(@return: (IExpression)ctx.selectExpression().XPTreeNode, engine: this.Engine);
                return ;
            }
            return ;
        }
        /// <summary>
        /// build a XPT Node out of a selection
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitSelection(RulezParser.SelectionContext ctx)
        {
            // extract the class name
            if (String.IsNullOrEmpty(ctx.ClassName)) ctx.ClassName = ctx.dataObjectClass().GetText();

            // create the result with the data object class name
            eXPressionTree.ResultList Result = (ResultList)ctx.resultSelection().XPTreeNode;

            // create a selection expression with the result
            eXPressionTree.SelectionExpression aSelection = new eXPressionTree.SelectionExpression(result: Result, engine: this.Engine);

            //  L_SQUARE_BRACKET  R_SQUARE_BRACKET // all
            if (ctx.selectConditions() == null)
            {
                // simple true operator
                aSelection.Nodes.Add(LogicalExpression.TRUE());
            }
            else
            {
                // add the subtree to the selection
                aSelection.Nodes.Add(ctx.selectConditions().XPTreeNode);
            }
            // add it to selection as XPTreeNode
            ctx.XPTreeNode = aSelection;
            return ;
        }
        /// <summary>
        /// build an XPTree with the results
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitResultSelection(RulezParser.ResultSelectionContext ctx)
        {
            List<INode> results = new List<INode>();

            // add the class
            if (ctx.symbol() == null || ctx.dataObjectEntryName().Count() == 0)
                results.Add(new eXPressionTree.DataObjectSymbol(ctx.ClassName, engine: this.Engine));
            else
                // add the entries
                foreach (RulezParser.DataObjectEntryNameContext anEntryCTX in ctx.dataObjectEntryName())
                    results.Add(new eXPressionTree.DataObjectEntrySymbol(anEntryCTX.entryname, engine: this.Engine));

            ctx.XPTreeNode = new ResultList(results);
            return ;
        }
        /// <summary>
        /// build a XPT Node out of a selection conditions
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitSelectConditions(RulezParser.SelectConditionsContext ctx)
        {
            //  L_SQUARE_BRACKET  R_SQUARE_BRACKET // all
            if (ctx.selectCondition() == null || ctx.selectCondition().Count() == 0)
            {
                // simple true operator
                ctx.XPTreeNode = LogicalExpression.TRUE();
                return ;
            }
            // only one condition
            //    selectCondition [$ClassName, $keypos]

            if (ctx.selectCondition().Count() == 1)
            {
                if (ctx.NOT().Count() == 0) ctx.XPTreeNode = ctx.selectCondition()[0].XPTreeNode;
                else ctx.XPTreeNode = LogicalExpression.NOT((IExpression)ctx.selectCondition()[0].XPTreeNode);
                return ;
            }

            // if we have more than this 
            //|	selectCondition [$ClassName, $keypos] (logicalOperator_2 selectCondition [$ClassName, $keypos])* 
            if (ctx.selectCondition().Count() > 1)
            {
                eXPressionTree.LogicalExpression theLogical = (LogicalExpression)ctx.selectCondition()[0].XPTreeNode;
                if (theLogical == null) return ;

                for (uint i = 0; i < ctx.selectCondition().Count() - 1; i++)
                {
                    Operator anOperator = ctx.logicalOperator_2()[i].Operator;

                    // x or y and z ->  ( x or  y) and z )
                    if (theLogical.Priority >= anOperator.Priority)
                    {
                        if ((LogicalExpression)ctx.selectCondition()[i + 1].XPTreeNode != null)
                            theLogical = new LogicalExpression(anOperator, theLogical, (LogicalExpression)ctx.selectCondition()[i + 1].XPTreeNode);
                        else return ;
                        // negate
                        if (ctx.NOT().Count() >= i + 1 && ctx.NOT()[i + 1] != null)
                            theLogical = LogicalExpression.NOT((IExpression)theLogical);
                    }
                    else
                    {   // x and y or z ->  x and ( y or z )
                        // build the new (lower) operation in the higher level tree (right with the last operand)
                        IExpression right = (IExpression)theLogical.RightOperand;
                        theLogical.RightOperand = new LogicalExpression(anOperator, right, (IExpression)ctx.selectCondition()[i + 1].XPTreeNode);
                        // negate
                        if (ctx.NOT().Count() >= i + 1 && ctx.NOT()[i + 1] != null)
                            theLogical.RightOperand = LogicalExpression.NOT((IExpression)theLogical.RightOperand);

                    }

                }
                ctx.XPTreeNode = theLogical;
                return ;
            }

            return ;
        }

        /// <summary>
        /// build a XPT Node out of a selection condition
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitSelectCondition(RulezParser.SelectConditionContext ctx)
        {
            string entryName;
            //|   RPAREN selectConditions [$ClassName, $keypos] LPAREN 
            if (ctx.selectConditions() != null)
            {
                if (ctx.NOT() == null) ctx.XPTreeNode = ctx.selectConditions().XPTreeNode;
                else ctx.XPTreeNode = LogicalExpression.NOT((IExpression)ctx.selectConditions().XPTreeNode);
                // set the max priority for disabling reshuffle
                ((OperationExpression)ctx.XPTreeNode).Priority = uint.MaxValue;
                return ;
            }
            else
            {
                // determine the key name with the key is not provided by the key position
                //
                if (ctx.symbol  == null)
                {
                    string aClassName = RulezParser.GetDefaultClassName(ctx);
                    if (this.Engine.Globals.HasDataObjectDefinition(aClassName))
                    {
                        iObjectDefinition aObjectDefinition = this.Engine.GetDataObjectDefinitions(aClassName).First();
                        if (ctx.keypos <= aObjectDefinition.Keys.Count())
                            entryName = aClassName + "." + aObjectDefinition.Keys[ctx.keypos - 1];
                        else
                        {
                            _parser.NotifyErrorListeners((String.Format(Messages.RCM_8, aClassName, aObjectDefinition.Keys.Count(), ctx.keypos)));
                            return ;
                        }
                    }
                    else
                    {
                        _parser.NotifyErrorListeners(String.Format(Messages.RCM_9, aClassName));
                        return ;
                    }

                }
                else entryName = ctx.dataObjectEntry.entryname;

                // get the symbol
                DataObjectEntrySymbol aSymbol = new DataObjectEntrySymbol(entryName, engine: this.Engine);

                // Operator
                Operator anOperator;
                // default operator is the EQ operator
                if (ctx.Operator == null) anOperator = Engine.GetOperators(new Token(Token.EQ));
                else anOperator = ctx.Operator.Operator;

                // build the comparer expression
                CompareExpression aCompare = null;
                if (aSymbol != null && ctx.select.XPTreeNode != null) aCompare = new CompareExpression(anOperator, aSymbol, (IExpression)ctx.select.XPTreeNode);
                else return ;
                // set it
                ctx.XPTreeNode = aCompare;
                return ;
            }
        }
        /// <summary>
        /// build an XPTreeNode for a select expression
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitSelectExpression(RulezParser.SelectExpressionContext ctx)
        {
            //  literal 
            if (ctx.literal() != null)
            {
                ctx.XPTreeNode = ctx.literal().XPTreeNode;
                return ;
            }
            //| parameterName
            if (ctx.parameterName() != null)
            {
                ctx.XPTreeNode = ctx.parameterName().XPTreeNode;
                return ;
            }
            //| variableName
            if (ctx.variableName() != null)
            {
                ctx.XPTreeNode = ctx.variableName().XPTreeNode;
                return ;
            }
            if (ctx.selection() != null)
            {
                ctx.XPTreeNode = ctx.selection().XPTreeNode;
                return ;
            }
            //| dataObjectEntryName
            if (ctx.dataObjectEntryName() != null)
            {
                ctx.XPTreeNode = ctx.dataObjectEntryName().XPTreeNode;
                return ;
            }
            //| LPAREN selectExpression RPAREN
            if (ctx.LPAREN() != null && ctx.selectExpression().Count() == 1)
            {
                ctx.XPTreeNode = (IExpression)ctx.selectExpression()[0].XPTreeNode;
                // set the max priority for disabling reshuffle
                if (ctx.XPTreeNode != null && ctx.XPTreeNode is OperationExpression) ((OperationExpression)ctx.XPTreeNode).Priority = uint.MaxValue;
                return ;
            }
            //| ( PLUS | MINUS ) selectExpression
            if (ctx.selectExpression().Count() == 1)
            {
                if (ctx.selectExpression()[0].XPTreeNode != null)
                {
                    if (ctx.MINUS() != null)
                        ctx.XPTreeNode = new OperationExpression(new Token(Token.MINUS), new Literal(0), (IExpression)ctx.selectExpression()[0].XPTreeNode);
                    else ctx.XPTreeNode = (IExpression)ctx.selectExpression()[0].XPTreeNode;
                }
                else return ;

                return ;
            }
            //| logicalOperator_1 selectExpression
            if (ctx.logicalOperator_1() != null && ctx.selectExpression().Count() == 1)
            {
                if (ctx.selectExpression()[0].XPTreeNode != null)
                    ctx.XPTreeNode = new LogicalExpression(ctx.logicalOperator_1().Operator, (IExpression)ctx.selectExpression()[0].XPTreeNode);
                else return ;
                return ;
            }
            //| selectExpression arithmeticOperator selectExpression
            if (ctx.arithmeticOperator().Count() > 0 && ctx.selectExpression().Count() > 1)
            {
                IExpression theExpression = (IExpression)ctx.selectExpression()[0].XPTreeNode;
                if (theExpression == null) return ;


                for (uint i = 0; i < ctx.selectExpression().Count() - 1; i++)
                {
                    Operator anOperator = ctx.arithmeticOperator()[i].Operator;
                    if (!(theExpression is OperationExpression))
                    {
                        if ((IExpression)ctx.selectExpression()[i + 1].XPTreeNode != null)
                            theExpression = new OperationExpression(anOperator, theExpression, (IExpression)ctx.selectExpression()[i + 1].XPTreeNode);
                        else return ;
                    }
                    else
                    {

                        // x * y + z ->  ( x *  y) + z )
                        if (((OperationExpression)theExpression).Priority > anOperator.Priority)
                        {
                            if ((IExpression)ctx.selectExpression()[i + 1].XPTreeNode != null)
                                theExpression = new OperationExpression(anOperator, theExpression, (IExpression)ctx.selectExpression()[i + 1].XPTreeNode);
                            else return ;
                        }
                        else
                        {   // x + y o* z ->  x + ( y * z )
                            // build the new (lower) operation in the higher level tree (right with the last operand)
                            IExpression right = (IExpression)((OperationExpression)theExpression).RightOperand;
                            if (right != null && (IExpression)ctx.selectExpression()[i + 1].XPTreeNode != null)
                                ((OperationExpression)theExpression).RightOperand = new LogicalExpression(anOperator, right, (IExpression)ctx.selectExpression()[i + 1].XPTreeNode);
                            else return ;
                        }
                    }
                }
                ctx.XPTreeNode = theExpression;
                return ;
            }
            
        }
        /// <summary>
        /// build a XPT Node for a parameter name
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitParameterNameContext(RulezParser.ParameterNameContext ctx)
        {
            RuleContext root = RulezParser.GetRootContext(ctx, typeof(RulezParser.SelectionRulezContext));
            if (root != null)
            {
                if (((RulezParser.SelectionRulezContext)root).names.ContainsKey(ctx.GetText()))
                {
                    // set the XPTreeNode to the Symbol
                    ctx.XPTreeNode = ((SelectionRule)((RulezParser.SelectionRulezContext)root).XPTreeNode).Parameters.Where(x => x.ID == ctx.GetText()).FirstOrDefault();
                    return;
                }
            }
            else
                _parser.NotifyErrorListeners(String.Format(Messages.RCM_4, ctx.GetText(), "SelectionRule"));
        }
        /// <summary>
        /// build a XPT Node for a parameter name
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitVariableName(RulezParser.VariableNameContext ctx)
        {
            RuleContext root = RulezParser.GetRootContext(ctx, typeof(RulezParser.SelectStatementBlockContext));
            if (root != null)
            {
                if (((RulezParser.SelectStatementBlockContext)root).names.ContainsKey(ctx.GetText()))
                {
                    // set the XPTreeNode to the Symbol
                    ctx.XPTreeNode = ((StatementBlock)((RulezParser.SelectStatementBlockContext)root).XPTreeNode).Variables.Where(x => x.ID == ctx.GetText()).FirstOrDefault();
                }
            }
            _parser.NotifyErrorListeners(String.Format(Messages.RCM_5, ctx.GetText(), "StatementBlock"));
        }
        /// <summary>
        /// build a XPTree Node for a data object entry name 
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitDataObjectEntryName (RulezParser.DataObjectEntryNameContext ctx)
        {
            string aClassName = String.Empty;

            /// build the entry name
            if (ctx.dataObjectClass() == null) aClassName = RulezParser.GetDefaultClassName(ctx);
            else aClassName = ctx.dataObjectClass().ClassName;
            // full entry name
            ctx.entryname = aClassName + "." + ctx.identifier().GetText();
            ctx.ClassName = aClassName;
            // get the symbol from the engine
            DataObjectEntrySymbol aSymbol = new DataObjectEntrySymbol(ctx.entryname, engine: this.Engine);
            ctx.XPTreeNode = aSymbol;
            if (aSymbol != null) return ;
        }
    }
}