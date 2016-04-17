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
        OperatorDefinition,
        FunctionDefinition,
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
        Core.otDataType ReturnTypeId { get;  }
        /// <summary>
        /// gets or sets the type 
        /// </summary>
        Core.IDataType ReturnType { get;}
    }
    /// <summary>
    /// describes a rule which is the top level
    /// </summary>
    public interface IRule : IXPTree, Core.ISigned
    {
        /// <summary>
        /// returns the ID of the rule
        /// </summary>
        String Id { get;  }
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
    /// describes a selection rule
    /// </summary>
    [Repository.StoreType()]
    public interface ISelectionRule : IRule, IExpression
    {
        /// <summary>
        /// gets the parameters
        /// </summary>
        ParameterList Parameters { get; }
        /// <summary>
        /// gets the resulting list definition
        /// </summary>
        ResultList Result { get; }
        /// <summary>
        /// gets or sets the selection statement block
        /// </summary>
        SelectionStatementBlock Selection { get; set; }
        /// <summary>
        /// creates and adds a new parameter by datatype id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dataTypeId"></param>
        /// <returns></returns>
        Parameter AddNewParameter(string id, Core.otDataType dataTypeId);
        /// <summary>
        /// creates and adds a new parameter by datatype
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dataTypeId"></param>
        /// <returns></returns>
        Parameter AddNewParameter(string id, Core.IDataType datatype);
        /// <summary>
        /// gets a parameter by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Parameter GetParameter(string id);
        /// <summary>
        /// returns true if the rule has the parameter by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool HasParameter(string id);
        /// <summary>
        /// returns the resulting list of data object ids
        /// </summary>
        /// <returns></returns>
        IList<string> ResultObjectIds();
    }
    /// <summary>
    /// defines a function
    /// </summary>
    [Repository.StoreType()]
    public interface IFunctionDefinition
    {
        ObjectName Name { get; }
        ParameterList Parameters { get; }
        Core.IDataType ReturnType { get; }
    }
    /// <summary>
    /// defines an Operator
    /// </summary>
     [Repository.StoreType()]
    public interface IOperatorDefinition : IXPTree, Core.ISigned
    {
        ushort Arguments { get; }
        ushort Priority { get; }
        Core.IDataType ReturnType { get; }
        Core.otDataType? ReturnTypeId { get; }
        Token Token { get; }
        otOperatorType Type { get; }
    }
    /// <summary>
    /// function calls
    /// </summary>
    public interface IFunctionCall: IXPTree, IStatement, IExpression, Core.ISigned
    {
        /// <summary>
        /// gets or sets the ID of the function
        /// </summary>
        String Id { get; set; }
    }
    /// <summary>
    /// describes a expression tree symbol 
    /// </summary>
    [Repository.StoreType()]
    public interface ISymbol : INode, IExpression, Core.ISigned
    {
        /// <summary>
        /// gets or sets the ID of the variable
        /// </summary>
        String Id { get; }
        /// <summary>
        /// defines the scope where this symbol lives
        /// </summary>
        IScope Scope { get;  }
        /// <summary>
        /// returns true if the symbol is valid in the engine (late binding)
        /// </summary>
        bool? IsValid { get; }
        /// <summary>
        /// create a parameter
        /// </summary>
        /// <returns></returns>
        Parameter ToParameter();
    }
    /// <summary>
    /// describes a Module
    /// </summary>
     [Repository.StoreType()]
    public interface IModule : INode, Core.ISigned
    {
        /// <summary>
        /// gets the ID
        /// </summary>
        string Id { get; }
        /// <summary>
        /// gets the name
        /// </summary>
        CanonicalName Name { get; }
        /// <summary>
        /// gets the version
        /// </summary>
        ulong Version { get; }
        /// <summary>
        /// defines the IeXPressionTree scope of the dataobject
        /// </summary>
        IScope Scope { get; }
    }
}
