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
using System.Data;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;
using OnTrack.Core;
using OnTrack.Rulez.Resources;
using OnTrack.Rulez.eXPressionTree;

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
            public IExpression defaultvalue;
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="name"></param>
            /// <param name="datatype"></param>
            /// <param name="pos"></param>
            /// <param name="defaultvalue"></param>
            public ParameterDefinition(string name, IDataType datatype, uint pos, IExpression defaultvalue = null)
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
            // this.NotifyErrorListeners(String.Format(Messages.RCM_2, name, "SelectStatementBlock"));
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
        bool AddParameter(string name, uint pos, IDataType datatype, LiteralContext defaultvalue, RuleContext context)
        {
            RuleContext root = GetRootContext(context, typeof(SelectionRulezContext));
            INode theDefaultValue = null;

            if (defaultvalue != null) theDefaultValue = defaultvalue.XPTreeNode;

            ParameterDefinition def = new ParameterDefinition(name: name, pos: pos, datatype: datatype, defaultvalue: (IExpression) theDefaultValue);
            if (root != null)
            {
                if (!((SelectionRulezContext)root).names.ContainsKey(name))
                {
                    ((SelectionRulezContext)root).names.Add(name, def);
                    ((SelectionRule)((SelectionRulezContext)root).XPTreeNode).AddNewParameter(name, datatype);
                }
                else 
                { this.NotifyErrorListeners(String.Format(Messages.RCM_3, name, "SelectionRule")); 
                       return false; 
                }

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
            // this.NotifyErrorListeners(String.Format(Messages.RCM_4, name, "SelectionRule"));
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
        /// returns true if this a Data Object class
        /// </summary>
        /// <returns></returns>
        bool IsDataObjectClass(string name, RuleContext context)
        {
            // check the name might be a full name
            return Engine.Repository.HasDataObjectDefinition(CanonicalName.ClassName(name));
        }
        /// <summary>
        /// returns true if this a Data Object class
        /// </summary>
        /// <returns></returns>
        bool IsDataObjectEntry(string name, RuleContext context)
        {
            // check the name might be a full name
            CanonicalName aName = new CanonicalName(name);
            string aClassname = aName.IsCanonical() ? aName.ClassName() : String.Empty;
            string anEntryName = aName.EntryName();

            // if we are in the right context
            if (context is DataObjectEntryNameContext)
            {
                DataObjectEntryNameContext ctx = (DataObjectEntryNameContext)context;
                if (string.IsNullOrEmpty(ctx.ClassName)) aClassname = GetDefaultClassName(context);
                else if (!String.IsNullOrWhiteSpace(ctx.ClassName))
                {
                    // if classname differs than it is not allowed
                    if (string.Compare(ctx.ClassName, aClassname, true) != 00)
                        this.NotifyErrorListeners(String.Format (Messages.RCM_12, ctx.ClassName));
                    else aClassname = ctx.ClassName;
                }
            }
            else if (context is SelectExpressionContext)
            {
                SelectExpressionContext ctx = (SelectExpressionContext)context;
                string aDefaultname = GetDefaultClassName(ctx);
                if (!(String.IsNullOrEmpty(aDefaultname))) aClassname = aDefaultname;
            }
            else if (context is SelectConditionContext)
            {
                SelectConditionContext ctx = (SelectConditionContext)context;
                string aDefaultname = GetDefaultClassName(ctx);
                if (!(String.IsNullOrEmpty(aDefaultname))) aClassname = aDefaultname;
            }
            else if (context is ResultSelectionContext)
            {
                ResultSelectionContext ctx = (ResultSelectionContext)context;
                string aDefaultname = GetDefaultClassName(ctx);
                if (string.IsNullOrEmpty(ctx.ClassName)) aClassname = GetDefaultClassName(context);
                else if (!String.IsNullOrWhiteSpace(ctx.ClassName)) aClassname = ctx.ClassName;
            }
                
            // check if DataObjectEntry is there
            if (!string.IsNullOrWhiteSpace(aClassname) && Engine.Repository.HasDataObjectDefinition(aClassname))
                return (Engine.Repository.GetDataObjectDefinition(aClassname).HasEntry(anEntryName));
            // no way to get classname and entryname
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
                if (ctx.XPTreeNode == null) ctx.XPTreeNode = new eXPressionTree.Unit(this.Engine);
                foreach (OneRulezContext aCtx in ctx.oneRulez()) ctx.XPTreeNode.Add((IXPTree)aCtx.XPTreeNode);
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
            
            // add expression
            if (ctx.selection() != null) {
                aRule.Selection = new SelectionStatementBlock(engine: this.Engine);
                aRule.Selection.Add(new @Return((SelectionExpression)ctx.selection().XPTreeNode)); 
            }
            else if (ctx.selectStatementBlock() != null) aRule.Selection = (SelectionStatementBlock)ctx.selectStatementBlock().XPTreeNode;
            // add the parameters
            foreach (ParameterDefinition aParameter in ctx.names.Values)
            {
                ISymbol symbol = aRule.AddNewParameter(aParameter.name, datatype: aParameter.datatype);
                // defaultvalue assignment
                if (aParameter.defaultvalue != null) aRule.Selection.Nodes.Insert(0, new eXPressionTree.IfThenElse(
                    eXPressionTree.CompareExpression.EQ(symbol, new Literal(null, otDataType.@Null)),
                    new eXPressionTree.Assignment(symbol, (IExpression)aParameter.defaultvalue)));
            }
            return true;
        }
        /// <summary>
        /// build the XPTNode of a select statement block
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool BuildXPTNode(SelectStatementBlockContext ctx)
        {
            if (ctx.XPTreeNode == null) ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.SelectionStatementBlock();
            SelectionStatementBlock aBlock = (SelectionStatementBlock) ctx.XPTreeNode ;

            // add the defined variables to the XPT
            foreach (VariableDefinition aVariable in ctx.names.Values)
            {
                ISymbol symbol = aBlock.AddNewVariable(aVariable.name, datatype: aVariable.datatype);
                // defaultvalue assignment
                if (aVariable.defaultvalue != null) aBlock.Nodes.Add(new eXPressionTree.Assignment(symbol, (IExpression)aVariable.defaultvalue));
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
                ctx.XPTreeNode = new Return(@return: (IExpression) ctx.selectExpression().XPTreeNode, engine: this.Engine);
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
            // extract the class name
            if (String.IsNullOrEmpty(ctx.ClassName)) ctx.ClassName = ctx.dataObjectClass().GetText();

            // create the result with the data object class name
            eXPressionTree.ResultList Result = (ResultList) ctx.resultSelection().XPTreeNode;
            
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
            return true;
        }
        /// <summary>
        /// build an XPTree with the results
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public bool BuildXPTNode(ResultSelectionContext ctx)
        {
            List<INode> results = new List<INode>();

            // add the class
            if (ctx.dataObjectEntryName() == null || ctx.dataObjectEntryName().Count () == 0)
                results.Add(new eXPressionTree.DataObjectSymbol(ctx.ClassName, engine: this.Engine));
            else
                // add the entries
                foreach (DataObjectEntryNameContext anEntryCTX in ctx.dataObjectEntryName())
                    results.Add(new eXPressionTree.DataObjectEntrySymbol(anEntryCTX.entryname, engine: this.Engine));

            ctx.XPTreeNode = new ResultList(results);
            return true;
        }
        /// <summary>
        /// build a XPT Node out of a selection conditions
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
         public bool BuildXPTNode(SelectConditionsContext ctx)
        {
            //  L_SQUARE_BRACKET  R_SQUARE_BRACKET // all
            if (ctx.selectCondition() == null || ctx.selectCondition().Count()==0)
            {
                // simple true operator
                ctx.XPTreeNode = LogicalExpression.TRUE();
                return true;
            }
            // only one condition
            //    selectCondition [$ClassName, $keypos]
     
            if (ctx.selectCondition().Count() == 1)
            {
                if (ctx.NOT().Count() == 0) ctx.XPTreeNode = ctx.selectCondition()[0].XPTreeNode;
                else ctx.XPTreeNode = LogicalExpression.NOT((IExpression)ctx.selectCondition()[0].XPTreeNode);
                return true;
            }
            
            // if we have more than this 
            //|	selectCondition [$ClassName, $keypos] (logicalOperator_2 selectCondition [$ClassName, $keypos])* 
            if (ctx.selectCondition().Count() > 1)
            {
               eXPressionTree.LogicalExpression theLogical = (LogicalExpression)ctx.selectCondition()[0].XPTreeNode;
               if (theLogical == null) return false;

               for(uint i = 0; i < ctx.selectCondition().Count()-1;i++)
               {
                   Operator anOperator = ctx.logicalOperator_2()[i].Operator;
                  
                    // x or y and z ->  ( x or  y) and z )
                   if (theLogical.Priority >= anOperator.Priority)
                   {
                       if ((LogicalExpression)ctx.selectCondition()[i + 1].XPTreeNode != null)
                            theLogical = new LogicalExpression(anOperator, theLogical, (LogicalExpression)ctx.selectCondition()[i + 1].XPTreeNode);
                       else return false;
                       // negate
                       if (ctx.NOT().Count() >= i+1 && ctx.NOT()[i + 1] != null)
                           theLogical = LogicalExpression.NOT((IExpression)theLogical);
                   }
                   else
                   {   // x and y or z ->  x and ( y or z )
                       // build the new (lower) operation in the higher level tree (right with the last operand)
                       IExpression right = (IExpression)theLogical.RightOperand;
                       theLogical.RightOperand = new LogicalExpression(anOperator, right, (IExpression)ctx.selectCondition()[i + 1].XPTreeNode);
                       // negate
                       if (ctx.NOT().Count() >= i + 1 &&  ctx.NOT()[i + 1] != null)
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
             
             // increase only if the last operator was an AND/ANDALSO
             if (ctx.logicalOperator_2() != null)
             {
                 
                 // if there is an AND or ANDALSO
                 if (ctx.logicalOperator_2().Last().Operator.Token.ToUint == Token.AND || ctx.logicalOperator_2().Last().Operator.Token.ToUint == Token.ANDALSO)
                 {
                     // check if the last named entry is a key -> reposition for unamed defaults
                     // (100,200,uid=150,250) -> keypos 1,2,1,2
                     if (ctx.selectCondition() != null  )
                     {
                         if (ctx.selectCondition().Last().dataObjectEntry != null)
                         {
                             DataObjectEntrySymbol aSymbol = (DataObjectEntrySymbol)ctx.selectCondition().Last().dataObjectEntry.XPTreeNode;
                             if (aSymbol.CheckValidity().HasValue && aSymbol.IsValid.Value == true)
                                 if (aSymbol.ObjectDefinition.Keys.Contains(aSymbol.Entryname))
                                 {
                                     int pos = Array.FindIndex(aSymbol.ObjectDefinition.Keys, x => String.Compare(x, aSymbol.Entryname.ToUpper(), true) == 0);
                                     if (pos >= 0) ctx.keypos = (uint)pos+1; // keypos is starting from 1 ... -> will be increased to next key lower down
                                 }
                         }
                     }

                     // simply increase and return
                     return ctx.keypos++;
                 }
                
             }
                         

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
             //|   RPAREN selectConditions [$ClassName, $keypos] LPAREN 
             if (ctx.selectConditions() != null)
             {
                 if (ctx.NOT()==null) ctx.XPTreeNode = ctx.selectConditions().XPTreeNode;
                 else ctx.XPTreeNode = LogicalExpression.NOT((IExpression)ctx.selectConditions().XPTreeNode);
                 // set the max priority for disabling reshuffle
                 ((OperationExpression)ctx.XPTreeNode).Priority = uint.MaxValue;
                 return true;
             }
             else
             {
                 // determine the key name with the key is not provided by the key position
                 //
                 if (ctx.dataObjectEntry == null)
                 {
                     string aClassName = GetDefaultClassName(ctx);
                     if (this.Engine.Repository.HasDataObjectDefinition(aClassName))
                     {
                         iObjectDefinition aObjectDefinition = this.Engine.GetDataObjectDefinition(aClassName);
                         if (ctx.keypos <= aObjectDefinition.Keys.Count())
                             entryName = aClassName + "." + aObjectDefinition.Keys[ctx.keypos - 1];
                         else
                         {
                             this.NotifyErrorListeners(String.Format(Messages.RCM_8, aClassName, aObjectDefinition.Keys.Count(), ctx.keypos));
                             return false;
                         }
                     }else
                     {
                         this.NotifyErrorListeners(String.Format(Messages.RCM_9, aClassName));
                         return false;
                     }

                 }
                 else entryName = ctx.dataObjectEntry.entryname;

                 // get the symbol
                 DataObjectEntrySymbol aSymbol = new DataObjectEntrySymbol(entryName, engine: this.Engine);

                 // Operator
                 Operator anOperator;
                 // default operator is the EQ operator
                 if (ctx.Operator == null) anOperator = Engine.GetOperator(new Token(Token.EQ));
                 else anOperator = ctx.Operator.Operator;

                 // build the comparer expression
                 CompareExpression aCompare = null;
                 if (aSymbol != null && ctx.select.XPTreeNode != null) aCompare = new CompareExpression(anOperator, aSymbol, (IExpression)ctx.select.XPTreeNode);
                 else return false;
                 // set it
                 ctx.XPTreeNode = aCompare;
                 return true;
             }
             return false;
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
             if (ctx.selection () != null)
             {
                 ctx.XPTreeNode = ctx.selection().XPTreeNode;
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
                 // set the max priority for disabling reshuffle
                 if (ctx.XPTreeNode != null && ctx.XPTreeNode is OperationExpression) ((OperationExpression)ctx.XPTreeNode).Priority = uint.MaxValue;
                 return true;
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
                 else return false;

                 return true;
             }
             //| logicalOperator_1 selectExpression
             if (ctx.logicalOperator_1() != null && ctx.selectExpression().Count() == 1)
             {
                 if (ctx.selectExpression()[0].XPTreeNode != null)
                     ctx.XPTreeNode = new LogicalExpression(ctx.logicalOperator_1().Operator, (IExpression)ctx.selectExpression()[0].XPTreeNode);
                 else return false;
                 return true;
             }
             //| selectExpression arithmeticOperator selectExpression
             if (ctx.arithmeticOperator().Count() > 0 && ctx.selectExpression().Count() > 1)
             {
                 IExpression theExpression = (IExpression)ctx.selectExpression()[0].XPTreeNode;
                 if (theExpression == null) return false;


               for(uint i = 0; i < ctx.selectExpression().Count()-1;i++)
               {
                   Operator anOperator = ctx.arithmeticOperator()[i].Operator;
                   if (!(theExpression is OperationExpression))
                   {
                       if ((IExpression)ctx.selectExpression()[i + 1].XPTreeNode != null)
                           theExpression = new OperationExpression(anOperator, theExpression, (IExpression)ctx.selectExpression()[i + 1].XPTreeNode);
                       else return false;
                   }
                   else
                   {

                       // x * y + z ->  ( x *  y) + z )
                       if (((OperationExpression)theExpression).Priority > anOperator.Priority)
                       {
                           if ((IExpression)ctx.selectExpression()[i + 1].XPTreeNode != null)
                               theExpression = new OperationExpression(anOperator, theExpression, (IExpression)ctx.selectExpression()[i + 1].XPTreeNode);
                           else return false;
                       }
                       else
                       {   // x + y o* z ->  x + ( y * z )
                           // build the new (lower) operation in the higher level tree (right with the last operand)
                           IExpression right = (IExpression) ((OperationExpression)theExpression).RightOperand;
                           if (right != null && (IExpression)ctx.selectExpression()[i + 1].XPTreeNode != null)
                               ((OperationExpression)theExpression).RightOperand = new LogicalExpression(anOperator, right, (IExpression)ctx.selectExpression()[i + 1].XPTreeNode);
                           else return false;
                       }
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
                     return true;
                 }
             }else
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
            string aClassName = String.Empty;

            /// build the entry name
            if (ctx.dataObjectClass() == null) aClassName = GetDefaultClassName (ctx);
	        else aClassName = ctx.dataObjectClass().ClassName ;
            // full entry name
            ctx.entryname = aClassName + "." + ctx.identifier().GetText();
            ctx.ClassName = aClassName;
            // get the symbol from the engine
            DataObjectEntrySymbol aSymbol = new DataObjectEntrySymbol(ctx.entryname, engine: this.Engine);
            ctx.XPTreeNode = aSymbol;
            if (aSymbol != null) return true;
            return false;
        }
        /// <summary>
        /// register a Messagehandler to the node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool RegisterMessages(INode node)
        {
            bool result = false;
            // register at the listener our event listener
            EventHandler<RulezParser.MessageListener.EventArgs> handler = (s,e) => node.Messages.Add(e.Message);
            foreach (var aListener in this.ErrorListeners )
                if (aListener is MessageListener)
                { ((MessageListener)aListener).OnMessageAdded += handler; result = true; 
                }
            return result;
        }
        /// <summary>
        /// register a Messagehandler to the node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool DeRegisterMessages(INode node)
        {
            bool result = false;
            // register at the listener our event listener
            EventHandler<RulezParser.MessageListener.EventArgs> handler = (s,e) => node.Messages.Add(e.Message);
            foreach (var aListener in this.ErrorListeners)
                if (aListener is MessageListener) 
                {
                    ((MessageListener)aListener).ClearOnMessageAddedEvents();
                    result = true;
                }
            return result;
        }

        /// <summary>
        /// build-in Message Listener
        /// </summary>
        public class MessageListener : Antlr4.Runtime.BaseErrorListener
        {
            /// <summary>
            /// event args
            /// </summary>
            public class EventArgs: System.EventArgs
            {
                public Message Message;

                /// <summary>
                /// constructor
                /// </summary>
                /// <param name="message"></param>
                public EventArgs(Message message)
                {
                    this.Message = message;
                }
            }
       
            /// <summary>
            /// list of errors
            /// </summary>
            private List<Message> _errors = new List<Message>();

            // the OnMessageAdded Event
            public event EventHandler<EventArgs> OnMessageAdded;

            /// <summary>
            /// constructor
            /// </summary>
            public MessageListener()
            {
            }

            /// <summary>
            /// get the errors
            /// </summary>
            public IEnumerable<Message> Errors
            {
                get
                {
                    return _errors.Where(x => x.Type == MessageType.Error);
                }
            }

            /// <summary>
            /// get the warnings
            /// </summary>
            public IEnumerable<Message> Warnings
            {
                get
                {
                    return _errors.Where(x => x.Type == MessageType.Warning);
                }
            }

            public override void ReportAmbiguity(Antlr4.Runtime.Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact, BitSet ambigAlts, ATNConfigSet configs)
            {
            }

            public override void ReportAttemptingFullContext(Parser recognizer, DFA dfa, int startIndex, int stopIndex, BitSet conflictingAlts, SimulatorState conflictState)
            {
            }

            public override void ReportContextSensitivity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, int prediction, SimulatorState acceptState)
            {
            }

            /// <summary>
            /// process the SyntaxError
            /// </summary>
            /// <param name="recognizer"></param>
            /// <param name="offendingSymbol"></param>
            /// <param name="line"></param>
            /// <param name="charPositionInLine"></param>
            /// <param name="msg"></param>
            /// <param name="e"></param>
            public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                // publish the message
                string text = String.Empty;
                if (e is FailedPredicateException) text = String.Format(Messages.RCM_11, offendingSymbol.Text );
                else text = msg;

                Message message = new Message(type: MessageType.Error, line: line, pos: charPositionInLine, message: text);
                if (OnMessageAdded != null) OnMessageAdded(this, new EventArgs(message));
                _errors.Add(message);

                if (charPositionInLine != 00)
                    Console.Out.WriteLine(String.Format("ERROR <{0},{1:D2}>:{2}", line, charPositionInLine, text));
                else
                    Console.Out.WriteLine(String.Format("ERROR <line {0}>:{1}", line, text));
            }
            /// <summary>
            /// clear all events
            /// </summary>
            public void ClearOnMessageAddedEvents()
            {
                foreach (EventHandler<MessageListener.EventArgs> aHandler in this.OnMessageAdded.GetInvocationList())
                    this.OnMessageAdded += aHandler;
            }
        }
    }

    /// <summary>
    /// type of messages
    /// </summary>
    public enum MessageType : uint
    {
        Error = 1,
        Warning
    }
    /// <summary>
    /// structure for erors
    /// </summary>
    public struct Message
    {
        public DateTime Timestamp;
        public MessageType Type;
        public int Line;
        public int Pos;
        public string Text;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="line"></param>
        /// <param name="pos"></param>
        /// <param name="message"></param>
        public Message(MessageType type = MessageType.Error, int line = 0, int pos = 0, string message = null)
        {
            this.Timestamp = DateTime.Now;
            this.Type = type;
            this.Line = line;
            this.Pos = pos;
            this.Text = message;
        }
        /// <summary>
        /// convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0:s}: {1} [Line {2}, Position {3}] {4}", this.Timestamp, this.Type.ToString(), this.Line, this.Pos, this.Text);
        }
    }
}
