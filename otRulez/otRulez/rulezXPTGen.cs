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
using Antlr4.Runtime.Misc;

namespace OnTrack.Rulez
{
    /// <summary>
    /// lister to generate all the declarations in a XPT Scope
    /// </summary>
    public class XPTDeclarator : RulezParserBaseListener
    {
        private readonly RulezParser _parser;
        private readonly eXPressionTree.XPTree _xptree; // the output tree
        private readonly Engine _engine;
        private IScope _currentScope;
        private readonly Stack<IScope> _scopeStack = new Stack<IScope>();
        /// <summary>
        /// constructor
        /// </summary>
        /// <param id="parser"></param>
        public XPTDeclarator(RulezParser parser, Engine engine = null)
        {
            _parser = parser;
            if (engine == null) _engine = parser.Engine;
            else { _engine = engine; }
            // default scope is the global scope of the same engine
            _currentScope = new XPTScope(engine: _engine, id: CanonicalName.GlobalID);
            
        }
        #region Properties
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
        public RulezParser Parser
        {
            get
            {
                return _parser;
            }
        }
        /// <summary>
        /// gets or sets the  Current Scope
        /// </summary>
        private IScope CurrentScope
        {
            get { return _currentScope; }
            set { _currentScope = value; }
        }
        #endregion
        #region Helpers
        /// <summary>
        /// push scope element on the stack
        /// </summary>
        /// <param name="scope"></param>

        private void PushScope(IScope scope)
        {
            _scopeStack.Push(scope);
        }
        /// <summary>
        /// push a new scope (which will be created) on the stack an sets the current scope to this scope
        /// returns the created scope
        /// </summary>
        /// <param name="id"></param>
        private IScope PushNewScope(string id)
        {
            // create Scope
            CurrentScope.AddScope(id: id);
            IScope aScope = CurrentScope.GetScope(id: id);
            if (aScope != null) CurrentScope = aScope;
            PushScope(aScope);
            return aScope;
 
        }
        /// <summary>
        /// pop the last scope from stack and sets  the current scope to the remaining top element
        /// </summary>
        /// <returns></returns>
        private IScope PopScope()
        {
            IScope aScope =  _scopeStack.Pop();
            CurrentScope = aScope;
            return aScope;
        }
        #endregion
        /// <summary>
        /// Enter the type declaration
        /// </summary>
        /// <param name="context"></param>
        public override void EnterTypeDeclaration([NotNull] RulezParser.TypeDeclarationContext context)
        {
            base.EnterTypeDeclaration(context);
            // push the new scope of this type
            context.Scope = PushNewScope(context.typeid().GetText());
        }
        /// <summary>
        /// exit the type declaraton
        /// </summary>
        /// <param id="ctx"></param>
        /// <returns></returns>
        public override void ExitTypeDeclaration(RulezParser.TypeDeclarationContext ctx)
        {
            // pop the scope
            PopScope();
        }
        /// <summary>
        /// enter the Variable declaration
        /// </summary>
        /// <param name="context"></param>
        public override void EnterVariableDeclaration([NotNull] RulezParser.VariableDeclarationContext context)
        {
            base.EnterVariableDeclaration(context);
            // set scope to current scope
            context.Scope = this.CurrentScope;
        }
        /// <summary>
        /// exit variable declaration
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitVariableDeclaration (RulezParser.VariableDeclarationContext ctx)
        {
            // Add the variables to the scope will be handled by end of block Visitor
            return;
        }
        /// <summary>
        /// enter the module declaration
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void EnterModuleDeclaration(RulezParser.ModuleDeclarationContext ctx)
        {
            // create a scope and assign
            ctx.Scope = PushNewScope(ctx.canonicalName().GetText());

        }
        /// <summary>
        /// exit the module declaration
        /// </summary>
        /// <param name="context"></param>
        public override void ExitModuleDeclaration(RulezParser.ModuleDeclarationContext context)
        {
            base.ExitModuleDeclaration(context);
            // leave scope
            PopScope();
        }
        /// <summary>
        /// enter the selection rulez
        /// </summary>
        /// <param name="ctx"></param>
        public override void EnterSelectionRulez(RulezParser.SelectionRulezContext ctx)
        {
            base.EnterSelectionRulez(ctx);
            
            // create new scope
            if (this.CurrentScope.HasSelectionRule(ctx.ruleid().GetText()))
                Parser.NotifyErrorListeners(string.Format(Messages.RCM_13, ctx.ruleid().GetText()));
            // set scope
            ctx.Scope = PushNewScope(ctx.ruleid().GetText());
           
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
            var aRule = new SelectionRule(ctx.ruleid().GetText(), engine: this.Engine);
            ctx.XPTreeNode = aRule;
            // add the parameters
            foreach (RulezParser.ParameterDefinition aParameter in ctx.names.Values)
            {
                ISymbol symbol= aRule.AddNewParameter(aParameter.name, datatype: aParameter.datatype);
                
                // defaultvalue assignment
                if (aParameter.defaultvalue != null) aRule.Selection.Nodes.Insert(0, new eXPressionTree.IfThenElse(
                    eXPressionTree.CompareExpression.EQ(symbol, new Literal( null, otDataType.@Null)),
                    new eXPressionTree.Assignment(symbol, (IExpression)aParameter.defaultvalue)));

            }
            // leave the scope
            PopScope();
        }
        /// <summary>
        /// Enter SelectStatementBlock
        /// </summary>
        /// <param name="ctx"></param>
         public override void EnterSelectStatementBlock(RulezParser.SelectStatementBlockContext ctx)
        {
            // create a scope and assign
            if (ctx.XPTreeNode == null) ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.SelectionStatementBlock();
            var aBlock = (SelectionStatementBlock)ctx.XPTreeNode;
            // push the new scope to the current scope
            ctx.Scope = PushNewScope(aBlock.Id);

            return;
        }
        /// <summary>
        /// build the XPTNode of a select statement block
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public override void ExitSelectStatementBlock(RulezParser.SelectStatementBlockContext ctx)
        {
            // get the XPTreeNode
            var aBlock = (SelectionStatementBlock)ctx.XPTreeNode;

            // add the defined variables to the XPT
            foreach (RulezParser.VariableDefinition aVariable in ctx.names.Values)
            {
                /// check the repository (on defined in here) else (if defined in higher scopes) we overwrite
                if (!aBlock.Scope.Repository.HasSymbol(aVariable.name))
                {
                    ISymbol symbol = aBlock.AddNewVariable(aVariable.name, datatype: aVariable.datatype);
                    
                    // defaultvalue assignment
                    if (aVariable.defaultvalue != null)
                        aBlock.Nodes.Insert(0, new eXPressionTree.Assignment(symbol, (IExpression)aVariable.defaultvalue));
                }else
                    // ToDO redefined variable name
                { Parser.NotifyErrorListeners(string.Format(Messages.RCM_1, aVariable.name, aBlock.Id)); ; }
            }
            // pop the current scope
            PopScope();
        }
        
    }
    /// <summary>
    /// listener to generate a XPTree out of a ANTLR parse tree
    /// </summary>
    public class XPTGenerator : RulezParserBaseListener 
    {
        private readonly RulezParser _parser;
        private readonly XPTDeclarator _declarations;
        private eXPressionTree.XPTree _xptree; // the output tree
        private readonly Engine _engine;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="parser"></param>
        public XPTGenerator(RulezParser parser, XPTDeclarator declaration,  Engine engine = null)
        {
            _parser = parser;
            _declarations = declaration;
            if (engine == null) _engine = parser.Engine;
            else { _engine = engine; }
        }
        #region Properties
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
        /// returns the Declaration Tree
        /// </summary>
        public XPTDeclarator Declarations
        {
            get { return _declarations; }
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
        #endregion
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
                if (ctx.XPTreeNode == null) ctx.XPTreeNode = new eXPressionTree.Unit(engine:this.Engine);
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
            var aRule = new SelectionRule(ctx.ruleid().GetText(), engine: this.Engine);
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
            var aBlock = (SelectionStatementBlock)ctx.XPTreeNode;

            // add the defined variables to the XPT
            foreach (RulezParser.VariableDefinition aVariable in ctx.names.Values)
            {
                ISymbol symbol = aBlock.Variables.Where ( X=> X.Id == aVariable.name).FirstOrDefault ();
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
            if (String.IsNullOrEmpty(ctx.ClassName)) ctx.ClassName = ctx.dataObject.GetText();

            // create the result with the data object class name
            var aResult = (ResultList)ctx.resultSelection().XPTreeNode;

            // create a selection expression with the result
            var aSelection = new eXPressionTree.SelectionExpression(result: aResult, engine: this.Engine);

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
            var theResults = new List<INode>();

            // add the class
            if (ctx.identifier() == null || ctx.identifier().Count() == 0)
                theResults.Add(new eXPressionTree.DataObjectSymbol(ctx.ClassName, engine: this.Engine));
            else
                // add the entries
                foreach (RulezParser.IdentifierContext anEntryCTX in ctx.identifier())
                    theResults.Add(new eXPressionTree.DataObjectEntrySymbol(anEntryCTX.GetText(), engine: this.Engine));

            ctx.XPTreeNode = new ResultList(theResults);
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
                var theLogical = (LogicalExpression)ctx.selectCondition()[0].XPTreeNode;
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
                        var right = (IExpression)theLogical.RightOperand;
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
                if (ctx.dataObjectEntry  == null)
                {
                    string aClassName = RulezParser.GetDefaultClassName(ctx);
                    if (this.Engine.Globals.HasDataObjectDefinition(aClassName))
                    {
                        IObjectDefinition aObjectDefinition = this.Engine.GetDataObjectDefinitions(aClassName).First();
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
                else entryName = ctx.dataObjectEntry.GetText();

                // get the symbol
                var aSymbol = new DataObjectEntrySymbol(entryName, engine: this.Engine);

                // Operator
                Operator anOperator;
                // default operator is the EQ operator
                if (ctx.Operator == null) anOperator = Engine.GetOperators(new Token(Token.EQ)).FirstOrDefault();
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
            if (ctx.canonicalName() != null)
            {
                ctx.XPTreeNode = ctx.canonicalName().XPTreeNode;
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
        public override void ExitParameterName(RulezParser.ParameterNameContext ctx)
        {
            RuleContext root = RulezParser.GetRootContext(ctx, typeof(RulezParser.SelectionRulezContext));
            if (root != null)
            {
                if (((RulezParser.SelectionRulezContext)root).names.ContainsKey(ctx.GetText()))
                {
                    // set the XPTreeNode to the Symbol
                    ctx.XPTreeNode = ((SelectionRule)((RulezParser.SelectionRulezContext)root).XPTreeNode).Parameters.Where(x => x.Id == ctx.GetText()).FirstOrDefault();
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
                    ctx.XPTreeNode = ((StatementBlock)((RulezParser.SelectStatementBlockContext)root).XPTreeNode).Variables.Where(x => x.Id == ctx.GetText()).FirstOrDefault();
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
            if (ctx.identifier().Count() ==1) aClassName = RulezParser.GetDefaultClassName(ctx);
            else
            {
                for (uint i = 0; i < ctx.identifier().Count() - 1; i++)
                    if (String.IsNullOrEmpty(aClassName)) aClassName = ctx.identifier()[i].GetText();
                    else aClassName += CanonicalName.ConstDelimiter + ctx.identifier()[i].GetText();

            }
            // full entry name
            EntryName anEntryName = new EntryName (aClassName + CanonicalName.ConstDelimiter + ctx.identifier().Last().GetText());
            // get the symbol from the engine
            DataObjectEntrySymbol aSymbol = new DataObjectEntrySymbol(anEntryName, engine: this.Engine);
            ctx.XPTreeNode = aSymbol;
            if (aSymbol != null) return ;
        }
    }
}