/**
 *  ONTRACK RULEZ ENGINE
 *  
 * Abstract Syntax Tree Declaration
 * 
 * Version: 1.0
 * Created: 2015-04-14
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
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace OnTrack.Rulez.eXPressionTree
{
    /// <summary>
    /// state of the Rule
    /// </summary>
    public enum otRuleState
    {
        created = 1,
        updated = 2,
        generatedCode = 4
    }
    /// <summary>
    /// token type of the Node
    /// </summary>
    public enum otXPTNodeType
    {
        Literal,
        Variable,
        Operand,
        Operation,
        CompareExpression,
        LogicalExpression,
        OperationExpression,
        FunctionCall,
        DataObjectSymbol,
        Rule,
        SelectionRule,
        Result,
        ResultList,
        SelectionExpression,
        StatementBlock,
        Assignment,
        SelectionStatementBlock,
        IfThenElse,
        Return,
        
    }

    /// <summary>
    /// defines a tree visitor
    /// </summary>
    public interface IVisitor
    {
        /// <summary>Rulez Workspace
        /// returns the whatever result
        /// </summary>
        Object Result { get; }
        /// <summary>
        /// generic visit to a node
        /// </summary>
        /// <param name="node"></param>
        void Visit(INode node);
    }
    /// <summary>
    /// defines a node of the AST
    /// </summary>
    public interface INode : IEnumerable <INode>
    {
        /// <summary>
        /// gets the type of the node
        /// </summary>
        otXPTNodeType  NodeType { get; }
        /// <summary>
        /// returns true if the node is a leaf
        /// </summary>
        bool HasSubNodes { get; }
        /// <summary>
        /// returns the parent of the node
        /// </summary>
        IXPTree Parent { get; set; }
        /// <summary>
        /// returns the engine
        /// </summary>
        Engine Engine { get; set; }
        /// <summary>
        /// accepts a visitor
        /// </summary>
        /// <param name="visitor"></param>
        bool Accept(IVisitor visitor);
    }
    /// <summary>
    /// describes an abstract syntax tree
    /// </summary>
    public interface IXPTree: INode, System.ComponentModel.INotifyPropertyChanged 
    {
        /// <summary>
        /// gets and sets the list of nodes
        /// </summary>
        ObservableCollection<INode> Nodes { get; set; }
    }
    /// <summary>
    /// executable rule statement(s)
    /// </summary>
    public interface IStatement: INode
    {

    }
    /// <summary>
    /// describes an Expression which returns a value
    /// </summary>
    public interface IExpression : INode
    {
        /// <summary>
        /// gets or sets the type id of the variable
        /// </summary>
        Core.otDataType TypeId { get; set; }
        /// <summary>
        /// gets or sets the type 
        /// </summary>
        Core.IDataType DataType { get; set; }
    }
    /// <summary>
    /// describes a rule which is the top level
    /// </summary>
    public interface IRule : IXPTree
    {
        /// <summary>
        /// returns the ID of the rule
        /// </summary>
        String ID { get; set; }

        /// <summary>
        /// returns the state of the rule
        /// </summary>
        otRuleState RuleState { get; set; }

        /// <summary>
        ///  Code Handle
        /// </summary>
        string Handle { get; set; }
    }
    /// <summary>
    /// function calls
    /// </summary>
    public interface IFunction: IXPTree, IStatement, IExpression
    {
        /// <summary>
        /// gets or sets the ID of the function
        /// </summary>
        String ID { get; set; }
    }
    /// <summary>
    /// describes a expression tree symbol 
    /// </summary>
    public interface ISymbol : INode, IExpression
    {
        /// <summary>
        /// gets or sets the ID of the variable
        /// </summary>
        String ID { get; set; }
        
        /// <summary>
        /// defines the IeXPressionTree scope of the symbol
        /// </summary>
        IXPTree Scope { get; set; }
        /// <summary>
        /// returns true if the symbol is valid in the engine (late binding)
        /// </summary>
        bool? IsValid { get; }
        
    }
}
