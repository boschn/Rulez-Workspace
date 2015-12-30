/**
 *  ONTRACK RULEZ DECLARATION
 *  
 * eXpression Tree
 * 
 * Version: 1.0
 * Created: 2015-10-14
 * Last Change
 * 
 * Change Log
 * 
 * (C) by Boris Schneider, 2015
 * 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OnTrack.Core;
using OnTrack.Rulez;
using OnTrack.Rulez.eXPressionTree;

namespace OnTrack.Rulez
{
    /// <summary>
    /// nested scope definition
    /// </summary>
    public interface IScope
    {
        /// <summary>
        /// gets the list of children
        /// </summary>
        ObservableCollection<IScope> Children { get; }
        /// <summary>
        /// gets the Repository of the Scope
        /// </summary>
        IRepository Repository { get; }
        /// <summary>
        /// gets or sets the Engine
        /// </summary>
        Engine Engine { get; set; }
        /// <summary>
        /// returns true if the Children have an ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool HasSubScope(string id);
        /// <summary>
        /// returns true if the scope name exists in the descendants of this scope
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool HasScope(CanonicalName name);
        bool HasScope(string id);
        /// <summary>
        /// returns a Subscope of an given id or null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IScope GetSubScope(string id);
        /// <summary>
        /// returns a scope object from the descendants
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IScope GetScope(CanonicalName name);
        IScope GetScope(string id);
        /// <summary>
        /// create an Subscope of an given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IScope AddSubScope(string id);
        /// <summary>
        /// adds a scope object to the descendants
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool AddScope(IScope scope);
        bool AddScope(string id);
        bool AddScope(CanonicalName name);
        /// <summary>
        /// get root scope
        /// </summary>
        /// <returns></returns>
        IScope GetRoot();
        /// <summary>
        /// creates a new scope object
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IScope NewScope(string id);
        /// <summary>
        /// creates a new scope object
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IScope NewScope(CanonicalName name);
        /// <summary>
        /// returns a rule rule from the repository or creates a new one and returns this
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        SelectionRule GetSelectionRule(string id = null);
        /// <summary>
        /// returns true if the selection rule by id is found in this Scope
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        bool HasSelectionRule(string id);
        /// <summary>
        /// gets the Operator definition for the Token ID
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        Operator GetOperator(Token id);
        /// <summary>
        /// return true if the operator is found here
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool HasOperator(Token id);
        /// <summary>
        /// gets the Operator definition for the Token ID
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        @Function GetFunction(Token id);
        /// <summary>
        /// returns true if the function is in scope
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool HasFunction(Token id);
        /// <summary>
        /// gets the Operator definition for the ID
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        IObjectDefinition GetDataObjectDefinition(string id);
        /// <summary>
        /// returns true if the data object is in scope
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool HasDataObjectDefinition(string id);
        /// <summary>
        /// gets the Parent of this Scope
        /// </summary>
        IScope Parent { get; set; }
        /// <summary>
        /// gets or sets the ID of the scope
        /// </summary>
        string Id { get; set; }
        /// <summary>
        /// gets or sets the Name of the Scope
        /// </summary>
        CanonicalName Name { get; set; }
        /// <summary>
        /// event handler for dataObjectRepository Added
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Scope_DataObjectRepositoryAdded(object sender, Rulez.Engine.EventArgs e);
        /// <summary>
        /// returns true if the scope has the symbol by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool HasSymbol(string id);
        /// <summary>
        /// returns the symbol by ID from the scope
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ISymbol GetSymbol(string id);
        /// <summary>
        /// adds a symbol to the Scope
        /// </summary>
        /// <param name="symbol"></param>
        bool AddSymbol(ISymbol symbol);
    }
    /// <summary>
    /// Interface for Engine Repositories
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// gets the unique handle of the engine
        /// </summary>
        string Id { get; }

        /// <summary>
        /// gets all the rule rules in the repository
        /// </summary>
        List<SelectionRule> SelectionRules { get; }

        /// <summary>
        /// gets all rule rule IDs in the repository
        /// </summary>
        List<String> SelectionRuleIDs { get; }

        /// <summary>
        /// gets all the operators in the repository
        /// </summary>
        List<Operator> Operators { get; }

        /// <summary>
        /// gets all operator tokens rule IDs in the repository
        /// </summary>
        List<Token> OperatorTokens { get; }

        /// <summary>
        /// return true if initialized
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// returns true if the repository has the rule rule
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        bool HasSelectionRule(string id);

        /// <summary>
        /// returns the selectionrule by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        SelectionRule GetSelectionRule(string id);

        /// <summary>
        /// adds a rule rule to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        bool AddSelectionRule(string id, SelectionRule rule);

        /// <summary>
        /// adds a rule rule to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param id="rule"></param>
        /// <returns></returns>
        bool RemoveSelectionRule(string id);

        /// <summary>
        /// returns true if the repository has the function
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        bool HasFunction(Token id);

        /// <summary>
        /// returns the function by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        @Function GetFunction(Token id);

        /// <summary>
        /// adds a function to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        bool AddFunction(@Function function);

        /// <summary>
        /// returns true if the repository has the rule rule
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        bool HasOperator(Token id);

        /// <summary>
        /// returns the selectionrule by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        Operator GetOperator(Token id);

        /// <summary>
        /// adds a rule rule to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        bool AddOperator(Operator Operator);

        /// <summary>
        /// adds a rule rule to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        bool RemoveOperator(Token id);

        /// <summary>
        /// returns true if the repository has the function
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        bool HasDataType(string name);

        /// <summary>
        /// returns true if the repository has the function
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        bool HasDataType(IDataType datatype);

        /// <summary>
        /// returns true if the repository has the function
        /// </summary>
        /// <param signature="handle"></param>
        /// <returns></returns>
        bool HasDataTypeSignature(string signature);

        /// <summary>
        /// returns the datatype by name
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        IDataType GetDatatype(string Name);

        /// <summary>
        /// returns the datatype by name
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        List<IDataType> GetDatatypeBySignature(string signature);

        /// <summary>
        /// adds a datatype to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        bool AddDataType(IDataType datatype);

        /// <summary>
        /// adds a datatype to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        bool RemoveDataType(IDataType datatype);

        /// <summary>
        /// returns true if the id exists in the Repository
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool HasDataObjectDefinition(string id);

        bool HasDataObjectDefinition(ObjectName name);

        /// <summary>
        /// returns the selectionrule by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        IObjectDefinition GetDataObjectDefinition(String id);

        IObjectDefinition GetDataObjectDefinition(ObjectName name);

        bool RegisterDataObjectRepository(IDataObjectRepository iDataObjectRepository);

        bool DeRegisterDataObjectRepository(IDataObjectRepository iDataObjectRepository);
        /// <summary>
        /// returns true if the symbol exists in the repository
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        bool HasSymbol(string p);
        /// <summary>
        /// add the symbol to the repository
        /// </summary>
        /// <param name="symbol"></param>
        bool AddSymbol(ISymbol symbol);
        /// <summary>
        /// get the symbol from the Repository
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ISymbol GetSymbol(string id);
        /// <summary>
        /// remove the symbol from the Repository
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool RemoveSymbol(string id);
    }
}
