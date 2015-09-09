
/**
 *  ONTRACK RULEZ ENGINE
 *  
 * rulez parser extensions
 * 
 * Version: 1.0
 * Created: 2015-08-14
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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using OnTrack.Core;
using OnTrack.Rulez.eXPressionTree;
using Antlr4.Runtime;
using OnTrack.Rulez.Resources;

namespace OnTrack.Rulez
{

    /// <summary>
    /// extensions to RulezParser class (which is the actual parser)
    /// </summary>
    partial class RulezParser
    {
        /// <summary> 
        /// structure to hold a parameter definition
        /// </summary>
        public struct ParameterDefinition
        {
            public uint pos;
            public IDataType datatype;
            public string name;
            public INode defaultvalue;

            public ParameterDefinition(string name, IDataType datatype, uint pos, INode defaultvalue = null)
            { this.name = name; this.datatype = datatype; this.pos = pos; this.defaultvalue = defaultvalue; }
        }
        /// <summary>
        /// structure to hold a variable definition
        /// </summary>
        public struct VariableDefinition
        {
            public IDataType datatype;
            public string name;
            public INode defaultvalue;
            public VariableDefinition(string name, IDataType datatype, INode defaultvalue = null)
            { this.name = name; this.datatype = datatype; this.defaultvalue = defaultvalue; }
        }
        /// <summary>
        /// gets or sets the Engine for the parser
        /// </summary>
        public Engine Engine { get; set; }
        /// <summary>
        /// add a parametername from the current context to a context which has names
        /// </summary>
        /// <returns></returns>
        bool AddVariable(string name, IDataType datatype, LiteralContext literal, RuleContext context)
        {
            RuleContext root = GetRootContext(context, typeof(SelectStatementBlockContext));
            VariableDefinition def = new VariableDefinition(name: name, datatype: datatype, defaultvalue: literal.XPTreeNode);
            if (root != null)
            {
                if (!((SelectStatementBlockContext)root).names.ContainsKey(name))
                {
                    ((SelectStatementBlockContext)root).names.Add(name, def);
                    ((StatementBlock)((SelectStatementBlockContext)root).XPTreeNode).AddNewVariable(name, datatype);
                }
                else { this.NotifyErrorListeners(String.Format(Messages.RCM_1, name, "SelectStatementBlock")); return false; }
                return true;
            }
            this.NotifyErrorListeners(String.Format(Messages.RCM_2, name, "SelectStatementBlock"));
            return false;
        }
        /// <summary>
        /// add a parametername from the current context to a context which has names
        /// </summary>
        /// <returns></returns>
        bool IsVariableName(string name, RuleContext context)
        {
            RuleContext root = GetRootContext(context, typeof(SelectStatementBlockContext));
            if (root != null)
            {
                
                return ((SelectStatementBlockContext)root).names.ContainsKey(name);
            }
            this.NotifyErrorListeners(String.Format(Messages.RCM_2, name, "SelectStatementBlock"));
            return false;
        }
        /// <summary>
        /// checks if the name is a variable name and throws an error
        /// </summary>
        /// <returns></returns>
        bool CheckVariableName(string name, RuleContext context)
        {
            RuleContext root = GetRootContext(context, typeof(SelectStatementBlockContext));
            if (root != null)
            {
                if (!((SelectStatementBlockContext)root).names.ContainsKey(name)) 
                    { this.NotifyErrorListeners(String.Format(Messages.RCM_5, name, "SelectStatementBlock")); return false; }
                return true;
            }
            this.NotifyErrorListeners(String.Format(Messages.RCM_2, name, "SelectStatementBlock"));
            return false;
        }
        /// <summary>
        /// add a parametername from the current context to a context which has names
        /// </summary>
        /// <returns></returns>
        bool AddParameter(string name, uint pos, IDataType datatype, LiteralContext literal, RuleContext context)
        {
            RuleContext root = GetRootContext(context, typeof(SelectionRulezContext));
            ParameterDefinition def = new ParameterDefinition(name: name, pos: pos, datatype: datatype, defaultvalue: literal.XPTreeNode);
            if (root != null)
            {
                if (!((SelectionRulezContext)root).names.ContainsKey(name)) ((SelectionRulezContext)root).names.Add(name, def);
                else { this.NotifyErrorListeners(String.Format(Messages.RCM_3, name, "SelectionRule")); return false; }

                ((SelectionRulezContext)root).names.Add(name, def);
                ((SelectionRule) ((SelectionRulezContext)root).XPTreeNode).AddNewParameter (name, datatype);

                return true;
            }
            this.NotifyErrorListeners(String.Format(Messages.RCM_4, name, "SelectionRule"));
            return false;
        }
        /// <summary>
        /// add a parametername from the current context to a context which has names
        /// </summary>
        /// <returns></returns>
        bool IsParameterName(string name, RuleContext context)
        {
            RuleContext root = GetRootContext(context, typeof(SelectionRulezContext));
            if (root != null)
            {
                return ((SelectionRulezContext)root).names.ContainsKey(name);
            }
            this.NotifyErrorListeners(String.Format(Messages.RCM_4, name, "SelectionRule"));
            return false;
        }
        /// <summary>
        /// checks if the name is a parameter name and throws an error
        /// </summary>
        /// <returns></returns>
        bool CheckParameterName(string name, RuleContext context)
        {
            RuleContext root = GetRootContext(context, typeof(SelectionRulezContext));
            if (root != null)
            {
                if (!((SelectionRulezContext)root).names.ContainsKey(name))
                { this.NotifyErrorListeners(String.Format(Messages.RCM_6, name, "SelectionRule")); return false; }
                return true;
            }
            this.NotifyErrorListeners(String.Format(Messages.RCM_4, name, "SelectionRule"));
            return false;
        }
        /// <summary>
        /// checks if the name is a unique rule id and throws an error
        /// </summary>
        /// <returns></returns>
        public bool CheckUniqueSelectionRuleId(string name)
        {
            if (Engine.Repository.HasSelectionRule (name))
            {
                this.NotifyErrorListeners(String.Format(Messages.RCM_7, name)); 
                return false;
            }
            return true;
        }
        /// <summary>
        /// returns the ancestor node context of a certain type of root from this context
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        RuleContext GetRootContext(RuleContext context, System.Type type)
        {
            if (context.GetType() == type) return context;
            if (context.Parent == null) return null;

            return GetRootContext(context.Parent, type);
        }
        /// <summary>
        /// returns the ancestor node context of a certain type of root from this context
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetDefaultClassName(RuleContext context)
        {
            if (context.GetType() == typeof(SelectionContext)) return ((SelectionContext) context).dataObjectClass ().GetText ();
            if (context.Parent == null) return null;

            return GetDefaultClassName(context.Parent);
        }
        /// <summary>
        /// returns true if the id is a data type name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool IsDataTypeName(string id)
        {
            if (this.Engine != null)
            {
                return this.Engine.Repository.HasDataType(id);
            }
            return false;
        }
        /// <summary>
        /// returns true if the id is a data object type name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool IsDataObjectType(string id)
        {
            if (this.Engine != null)
            {
                if (this.Engine.Repository.HasDataType(id))
                {
                    IDataType aDatatype = this.Engine.Repository.GetDatatype(id);
                    return (aDatatype.TypeId == otDataType.DataObject);
                }
            }
            return false;
        }
        /// <summary>
        /// returns true if the id is a data object type name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool IsSymbolType(string id)
        {
            if (this.Engine != null)
            {
                if (this.Engine.Repository.HasDataType(id))
                {
                    IDataType aDatatype = this.Engine.Repository.GetDatatype(id);
                    return (aDatatype.TypeId == otDataType.Symbol);
                }
            }
            return false;
        }
        /// <summary>
        /// define nodes
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool BuildXPTNode(RulezUnitContext ctx)
        {
            // selection Rulez
            if (ctx.oneRulez() != null && ctx.oneRulez ().Count() > 0)
            {
                ctx.XPTree = new List<INode>();
                foreach (INode aNode in ctx.oneRulez()) ctx.XPTree.Add(aNode);
                return true;
            }
            
            return false;
        }
        /// <summary>
        /// define nodes
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool BuildXPTNode(OneRulezContext ctx)
        {
            // selection Rulez
            if (ctx.selectionRulez() != null)
            {
                ctx.XPTreeNode = ctx.selectionRulez().XPTreeNode;
                return true;
            }
            if (ctx.typeDeclaration ()!= null)
            {
                ctx.XPTreeNode = null;
                return true;
            }
            return false;
        }
        /// <summary>
        /// define nodes
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool BuildXPTNode(TypeDeclarationContext ctx)
        {
            // selection Rulez
            
            return false;
        }
        /// <summary>
        /// builds the XPT node of this
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool BuildXPTNode(SelectionRulezContext ctx)
        {
            // $ctx.XPTreeNode = (eXPressionTree.IeXPressionTree) new SelectionRule($ctx.ruleid().GetText(), engine: this.Engine);
            // get the name
            SelectionRule aRule = new SelectionRule(ctx.ruleid().GetText(), engine: this.Engine);
            ctx.XPTreeNode = aRule;
            aRule.Selection = new eXPressionTree.SelectionStatementBlock(engine: this.Engine);
            // add the parameters
            foreach (ParameterDefinition aParameter in ctx.names.Values)
            {
                ISymbol symbol = aRule.AddNewParameter(aParameter.name, datatype: aParameter.datatype);
                // defaultvalue assignment
                if (aParameter.defaultvalue != null) aRule.Selection.Nodes.Add(new eXPressionTree.IfThenElse(eXPressionTree.CompareExpression.EQ(symbol, new Literal(null, otDataType.@Null)), 
                                                                                                    new eXPressionTree.Assignment(symbol, (IXPTree) aParameter.defaultvalue)));
            }
            
            // add expression
            if (ctx.selection() != null) aRule.Selection.Add( new @Return((SelectionExpression)ctx.selection().XPTreeNode));
            else if (ctx.selectStatementBlock() != null) aRule.Selection = (SelectionStatementBlock)ctx.selectStatementBlock().XPTreeNode;

            return true;
        }
        /// <summary>
        /// build the XPTNode of a select statement block
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool BuildXPTNode(SelectStatementBlockContext ctx)
        {
            SelectionStatementBlock aBlock = new OnTrack.Rulez.eXPressionTree.SelectionStatementBlock();
            ctx.XPTreeNode = aBlock;
            // add the defined variables to the XPT
            foreach (VariableDefinition aVariable in ctx.names.Values)
            {
                ISymbol symbol = aBlock.AddNewVariable(aVariable.name, datatype: aVariable.datatype);
                // defaultvalue assignment
                if (aVariable.defaultvalue != null) aBlock.Nodes.Add(new eXPressionTree.Assignment(symbol, (IXPTree)aVariable.defaultvalue));
            }
            // add statements
            foreach (SelectStatementContext statementCTX in ctx.selectStatement() )
            {
                // add it to the Block
                aBlock.Nodes.Add((IStatement)statementCTX.XPTreeNode);
            }

            return true;
        }
        /// <summary>
        /// build the XPTNode of a select statement 
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool BuildXPTNode(SelectStatementContext ctx)
        {
            
            return true;
        }
        /// <summary>
        /// build the XPTNode of an assignment context statement 
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool BuildXPTNode(AssignmentContext ctx)
        {

            return true;
        }
        /// <summary>
        /// build the XPTNode of an assignment context statement 
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool BuildXPTNode(MatchContext ctx)
        {

            return false;
        }
        public bool BuildXPTNode(MatchcaseContext ctx)
        {

            return false;
        }
        /// <summary>
        /// build the XPTNode of a return statement
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool BuildXPTNode(ReturnContext ctx)
        {
            if (ctx.selectExpression() != null)
            {
                ctx.XPTreeNode = new Return(Return: (IExpression) ctx.selectExpression().XPTreeNode, engine: this.Engine);
                return true;
            }
            return false;
        }
        /// <summary>
        /// build a XPT Node out of a selection
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool BuildXPTNode(SelectionContext ctx)
        {
            // to-do data object entries

            // extract the class name
            string aClassName = ctx.dataObjectClass().GetText();
            // create the result with the data object class name
            eXPressionTree.ResultList Result = new ResultList(new Result[] { new Result(aClassName, node: new eXPressionTree.DataObjectSymbol(aClassName)) });
            // create a selection expression with the result
            eXPressionTree.SelectionExpression aSelection = new eXPressionTree.SelectionExpression(result: Result, engine: this.Engine);
            // add the subtree to the selection
            aSelection.Nodes.Add(ctx.selectConditions().XPTreeNode);
            // add it to selection as XPTreeNode
            ctx.XPTreeNode = aSelection;
            return true;
        }
        /// <summary>
        /// build a XPT Node out of a selection conditions
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
         public bool BuildXPTNode(SelectConditionsContext ctx)
        {
            // only one condition
            //    selectCondition [$ClassName, $keypos]
     
            if (ctx.selectCondition().Count() == 1)
            {
                if (ctx.NOT().Count() == 0) ctx.XPTreeNode = ctx.selectCondition()[0].XPTreeNode;
                else ctx.XPTreeNode = LogicalExpression.NOT((IExpression)ctx.selectCondition()[0].XPTreeNode);
                return true;
            }
            //|   RPAREN selectConditions [$ClassName, $keypos] LPAREN
            if (ctx.selectConditions() != null)
            {
                if (ctx.NOT().Count() == 0) ctx.XPTreeNode = ctx.selectConditions().XPTreeNode;
                else ctx.XPTreeNode = LogicalExpression.NOT((IExpression)ctx.selectConditions().XPTreeNode);
                return true;
            }
            // if we have more than this 
            //|	selectCondition [$ClassName, $keypos] (logicalOperator_2 selectCondition [$ClassName, $keypos])* 
            if (ctx.selectCondition().Count() > 1)
            {
               eXPressionTree.LogicalExpression theLogical = (LogicalExpression)ctx.selectCondition()[0].XPTreeNode;
               for(uint i = 0; i < ctx.selectCondition().Count();i++)
               {
                   Operator anOperator = ctx.logicalOperator_2()[i].Operator;
                  
                    // x or y and z ->  ( x or  y) and z )
                   if (theLogical.Operator.Priority > anOperator.Priority)
                   {
                       theLogical = new LogicalExpression(anOperator, theLogical, (LogicalExpression)ctx.selectCondition()[i + 1].XPTreeNode);
                       // negate
                       if (ctx.NOT()[i + 1] != null)
                           theLogical = LogicalExpression.NOT((IExpression)theLogical);
                   }
                   else
                   {   // x and y or z ->  x and ( y or z )
                       // build the new (lower) operation in the higher level tree (right with the last operand)
                       IExpression right = (IExpression)theLogical.RightOperand;
                       theLogical.RightOperand = new LogicalExpression(anOperator, right, (IExpression)ctx.selectCondition()[i + 1].XPTreeNode);
                       // negate
                       if (ctx.NOT()[i + 1] != null)
                           theLogical.RightOperand = LogicalExpression.NOT((IExpression)theLogical.RightOperand);
                    
                   }
                      
               }
               ctx.XPTreeNode = theLogical;
               return true;
           }
            
           return false;
        }
        /// <summary>
        /// increase the key position depending on the logical Operator
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
         public uint incIncreaseKeyNo(SelectConditionsContext ctx)
         {
             if (ctx.logicalOperator_2().Last().Operator.Token.ToUint == Token.AND || ctx.logicalOperator_2().Last().Operator.Token.ToUint == Token.ANDALSO) return ctx.keypos + 1;
             return ctx.keypos;
         }
         /// <summary>
         /// build a XPT Node out of a selection condition
         /// </summary>
         /// <param name="ctx"></param>
         /// <returns></returns>
         public bool BuildXPTNode(SelectConditionContext ctx)
         {
             string entryName;
             // determine the key name with the key is not provided by the key position
             //
             if (ctx.dataObjectEntryName() == null)
             {
                 iObjectDefinition aObjectDefinition = this.Engine.GetDataObjectDefinition(ctx.ClassName);
                 entryName = aObjectDefinition.Keys[ctx.keypos - 1];
             }
             else entryName = ctx.dataObjectEntryName().GetText();

             // get the symbol
             DataObjectEntrySymbol aSymbol = new DataObjectEntrySymbol(ctx.ClassName + "." + entryName, engine: this.Engine);

             // Operator
             Operator anOperator ;
             // default operator is the EQ operator
             if (ctx.compareOperator()== null) anOperator = Engine.GetOperator(new Token(Token.EQ));
             else anOperator = ctx.compareOperator().Operator;

             // build the comparer expression
             CompareExpression aCompare = new CompareExpression(anOperator, aSymbol, (IExpression) ctx.selectExpression().XPTreeNode);
             // set it
             ctx.XPTreeNode = aCompare;
             return true; 
         }
        /// <summary>
        /// build an XPTreeNode for a select expression
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
         public bool BuildXPTNode(SelectExpressionContext ctx)
         { 
             //  literal 
             if (ctx.literal()!= null)
             {
                 ctx.XPTreeNode = ctx.literal().XPTreeNode;
                 return true;
             }
             //| parameterName
             if (ctx.parameterName() != null)
             {
                 ctx.XPTreeNode = ctx.parameterName().XPTreeNode;
                 return true;
             }
             //| variableName
             if (ctx.variableName() != null)
             {
                 ctx.XPTreeNode = ctx.variableName().XPTreeNode;
                 return true;
             }
             //| dataObjectEntryName
             if (ctx.dataObjectEntryName()!= null)
             {
                 ctx.XPTreeNode = ctx.dataObjectEntryName().XPTreeNode;
                 return true;
             }
             //| LPAREN selectExpression RPAREN
             if (ctx.LPAREN() != null && ctx.selectExpression().Count() == 1)
             {
                 ctx.XPTreeNode =  (IExpression)ctx.selectExpression()[0].XPTreeNode;
                 return true;
             }
             //| ( PLUS | MINUS ) selectExpression
             if (ctx.selectExpression().Count() == 1)
             {
                 if (ctx.MINUS() != null)
                 {
                     ctx.XPTreeNode = new OperationExpression(new Token(Token.MINUS), new Literal(0), (IExpression)ctx.selectExpression()[0].XPTreeNode);
                     return true;
                 }
                 else ctx.XPTreeNode = (IExpression)ctx.selectExpression()[0].XPTreeNode;
             }
             //| logicalOperator_1 selectExpression
             if (ctx.logicalOperator_1() != null && ctx.selectExpression().Count() == 1)
             {
                 ctx.XPTreeNode = new LogicalExpression(ctx.logicalOperator_1().Operator, (IExpression)ctx.selectExpression()[0].XPTreeNode);
                 return true;
             }
             //| selectExpression arithmeticOperator selectExpression
             if (ctx.arithmeticOperator().Count() > 0 && ctx.selectExpression().Count() > 1)
             {
                OperationExpression theExpression = (OperationExpression)ctx.selectExpression()[0].XPTreeNode;
               for(uint i = 0; i < ctx.selectExpression().Count();i++)
               {
                   Operator anOperator = ctx.arithmeticOperator()[i].Operator;
                  
                    // x * y + z ->  ( x *  y) + z )
                   if (theExpression.Operator.Priority > anOperator.Priority)
                   {
                       theExpression = new OperationExpression(anOperator, theExpression, (OperationExpression)ctx.selectExpression()[i + 1].XPTreeNode);
                   }
                   else
                   {   // x + y o* z ->  x + ( y * z )
                       // build the new (lower) operation in the higher level tree (right with the last operand)
                       IExpression right = (IExpression)theExpression.RightOperand;
                       theExpression.RightOperand = new LogicalExpression(anOperator, right, (IExpression)ctx.selectExpression()[i + 1].XPTreeNode);
                   }
               }
               ctx.XPTreeNode = theExpression;
               return true;
           }
             // return false
             return false; 
         }
        /// <summary>
        /// build a XPT Node for a parameter name
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool BuildXPTNode(ParameterNameContext ctx)
         {
             RuleContext root = GetRootContext(ctx, typeof(SelectionRulezContext));
             if (root != null)
             {
                 if (((SelectionRulezContext)root).names.ContainsKey(ctx.GetText())) 
                 {
                     // set the XPTreeNode to the Symbol
                     ctx.XPTreeNode = ((SelectionRule)((SelectionRulezContext)root).XPTreeNode).Parameters.Where(x => x.ID == ctx.GetText()).FirstOrDefault();
                 }
             }
             this.NotifyErrorListeners(String.Format(Messages.RCM_4, ctx.GetText(), "SelectionRule"));
             return false;
         }
        /// <summary>
        /// build a XPT Node for a parameter name
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool BuildXPTNode(VariableNameContext ctx)
        {
            RuleContext root = GetRootContext(ctx, typeof(SelectStatementBlockContext));
            if (root != null)
            {
                if (((SelectStatementBlockContext)root).names.ContainsKey(ctx.GetText()))
                {
                    // set the XPTreeNode to the Symbol
                    ctx.XPTreeNode = ((StatementBlock)((SelectStatementBlockContext)root).XPTreeNode).Variables.Where(x => x.ID == ctx.GetText()).FirstOrDefault();
                }
            }
            this.NotifyErrorListeners(String.Format(Messages.RCM_5, ctx.GetText(), "StatementBlock"));
            return false;
        }
        /// <summary>
        /// build a XPTree Node for a data object entry name 
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool BuildXPTNode(DataObjectEntryNameContext ctx)
        {
            /// build the entry name
            if (ctx.dataObjectClass() == null) ctx.entryname = GetDefaultClassName(ctx) + "." + ctx.IDENTIFIER().GetText();
	        else ctx.entryname = ctx.dataObjectClass().ClassName + "." + ctx.IDENTIFIER().GetText();
            // get the symbol from the engine
            DataObjectEntrySymbol aSymbol = new DataObjectEntrySymbol(ctx.entryname, engine: this.Engine);
            ctx.XPTreeNode = aSymbol;
            if (aSymbol != null) return true;
            return false;
        }
        
    }
}
