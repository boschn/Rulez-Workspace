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
        Created = 1,
        Updated = 2,
        GeneratedCode = 4
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
        Unit,
    }

    /// <summary>
    /// defines a tree visitor of nodes T and Result R
    /// </summary>
    public interface IVisitor<T, R>
    {
        /// <summary>Rulez Workspace
        /// returns the whatever result
        /// </summary>
        R Result { get; }
        /// <summary>
        /// generic visit to a node
        /// </summary>
        /// <param name="node"></param>
        void Visit(T node);
    }
    /// <summary>
    /// defines a node of the AST
    /// </summary>
    public interface INode : IEnumerable <INode>, System.ComponentModel.INotifyPropertyChanged 
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
        bool Accept(IVisitor<INode,object> visitor);
        /// <summary>
        /// returns the Errors of the Node
        /// </summary>
        IList<Rulez.Message> Messages { get; }
        /// <summary>
        /// Scope id of the node
        /// </summary>
        string ScopeId { get; set; }
    }
    /// <summary>
    /// describes an abstract syntax tree
    /// </summary>
    public interface IXPTree: INode
    {
        /// <summary>
        /// gets and sets the list of nodes
        /// </summary>
        ObservableCollection<INode> Nodes { get; set; }
        /// <summary>
        /// gets or sets the scope
        /// </summary>
        IScope Scope { get; set; }
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
        String Id { get; set; }
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
        String Id { get; set; }
    }
    /// <summary>
    /// describes a expression tree symbol 
    /// </summary>
    public interface ISymbol : INode, IExpression
    {
        /// <summary>
        /// gets or sets the ID of the variable
        /// </summary>
        String Id { get; set; }
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
