/**
 *  ONTRACK RULEZ ENGINE
 *  
 * rulez repository
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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using OnTrack.Core;
using OnTrack.Rulez.eXPressionTree;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Concurrent;
using OnTrack.Collections.Concurrent;

namespace OnTrack.Rulez
{
    /// <summary>
    /// declares something which can be run by the engine
    /// </summary>
    public interface ICodeBit
    {
        /// <summary>
        /// the ID of the Bit
        /// </summary>
        string Handle { get; set; }
        /// <summary>
        /// a Helper Tag for the Generator to attach a custom object
        /// </summary>
        Object Tag { get; set; }
        /// <summary>
        /// delegate for the Code
        /// </summary>
        Func<Context, Boolean> Code { get; set; }
    }
    /// <summary>
    /// types of operator
    /// </summary>
    public enum otOperatorType
    {
        Logical,
        Arithmetic,
        Assignement,
        Compare
    }
    /// <summary>
    /// defines the Operator Token
    /// </summary>
    public class Token : IComparable<Token>
    {
#pragma warning disable JustCode_NamingConventions // Naming conventions inconsistency
        /// <summary>
        /// static - must be ascending and discrete ! (do not leave one out !!)
        /// </summary>
        public const uint TRUE = 0;
        public const uint AND = 1;
        public const uint ANDALSO = 2;
        public const uint OR = 3;
        public const uint ORELSE = 4;
        public const uint NOT = 5;

        public const uint EQ = 10;
        public const uint NEQ = 11;
        public const uint GT = 12;
        public const uint GE = 13;
        public const uint LT = 14;
        public const uint LE = 15;

        public const uint PLUS = 16;
        public const uint MINUS = 17;
        public const uint MULT = 18;
        public const uint DIV = 19;
        public const uint MOD = 20;
        public const uint CONCAT = 21; // Concat must be the last one for functions to be found

        public const uint BEEP = 22;

#pragma warning restore JustCode_NamingConventions // Naming conventions inconsistency

        private readonly static string[] _ids = {"TRUE", "AND", "ANDALSO", "OR", "ORELSE", "NOT", "","","","",
                                            "=", "!=", "GT", "GE", "LT", "LE", "+", "-", "*", "/", "MOD", "CONCAT",
                                            "BEEP"};
        /// <summary>
        /// variable
        /// </summary>
        private readonly uint _token;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="value"></param>
        public Token(uint value)
        {
            _token = value;
        }
        /// <summary>
        /// returns the token
        /// </summary>
        public uint ToUint { get { return (uint) _token; } }

        /// <summary>
        /// implementation of comparable
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(Token obj)
        {
            if (obj.ToUint < this.ToUint) return -1;
            if (obj.ToUint == this.ToUint) return 0;
            if (obj.ToUint > this.ToUint) return 1;

            throw new NotImplementedException();
        }
        /// <summary>
        /// == comparerer on datatypes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Token a, Token b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object) a == null) || ((object) b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.ToUint == b.ToUint;
        }
        /// <summary>
        /// != comparer
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Token a, Token b)
        {
            return !(a == b);
        }
        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (obj == null || !(obj is Token))
                return false;
            else
                return this.CompareTo((Token) obj) == 0;
        }
        /// <summary>
        /// override Hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int) this.ToUint;
        }
        /// <summary>
        /// To string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.ToUint <= _ids.GetUpperBound(0)) return "'" + _ids[this.ToUint] + "'";
            return this.ToUint.ToString();
        }
    }

   
    
    /// <summary>

    /// <summary>
    /// a scope repository unit
    /// 
    /// design requirements:
    /// 
    /// 1) scopes are nested in a 1:n tree (which are subscopes)
    /// 2) a scope as an id in canonical form with the special Id CanonicalName.Global as root
    /// 3) queries for objects in the scopes are upbound travel through the tree to the root
    /// 4) scope id are independent from the tree (but in the engine created in this order)
    /// </summary>
    public class Scope : System.ComponentModel.INotifyPropertyChanged, IScope
    {
        /// <summary>
        /// vistor pattern class for the rulez scope tree.
        /// make the stack of the visitor
        /// </summary>
        public class Visitor<T> : IVisitor<IScope, T> where T : new()
        {
            /// Event Args for Visitor 
            /// </summary>
            public class EventArgs<T> : System.EventArgs where T : new()
            {
                /// <summary>
                /// constructor
                /// </summary>
                /// <param name="currentNode"></param>
                public EventArgs(IScope current = null)
                {
                    if (current != null)
                        this.Current = current;
                    this.Result = new T();
                }

                /// <summary>
                /// returns Current Node
                /// </summary>
                public IScope Current { get; set; }

                /// <summary>
                /// returns the Result
                /// </summary>
                public T Result { get; set; }
            }

            // declare events
            public delegate void Eventhandler(object o, EventArgs<T> e);

            public event Eventhandler VisitingScope;
            public event Eventhandler VisitedScope;

            //
            private readonly T _result = new T();

            /// <summary>
            /// return the Result of a run
            /// </summary>
            public T Result
            {
                get
                {
                    return _result;
                }
            }

            /// <summary>
            /// visit scope
            /// </summary>
            /// <param name="expression"></param>
            public void Visit(IScope scope)
            {
                var args = new EventArgs<T>(current: scope);

                // visit subnodes from left to right
                if (VisitingScope != null)
                    VisitingScope(scope, args);
                foreach (Scope aScope in scope.SubScopes)
                    Visit(aScope);
                if (VisitedScope != null)
                    VisitedScope(scope, args);
            }
        }
        private readonly CanonicalName _name; // name of the scope
        private IScope _parent; // parent scope
        private readonly ObservableCollection<IScope> _children = new ObservableCollection<IScope>(); // children scopes
        private IRepository _current; // this scope;
        private Engine _engine; // my engine

        // event
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        // constants
        private const string ConstPropertyEngine = "Engine";
        private const string ConstPropertyRepository = "Repository";
        private const string ConstPropertyParent = "Parent";

        /// <summary>
        /// constructor of an scope hierachy
        /// </summary>
        public Scope(Engine engine, string id = null, IScope parent = null)
        {
            if (id == null)
                _name = new CanonicalName(Guid.NewGuid().ToString());
            else
                _name = new CanonicalName(id);
            _parent = parent;
            SubScopes.CollectionChanged += Scope_CollectionChanged;
            this.PropertyChanged += Scope_PropertyChanged;
            // Register with Data types
            DataType.OnCreation += Scope_DataTypeOnCreation;
            DataType.OnRemoval += Scope_DataTypeOnRemoval;
        }
        public Scope(Engine engine, CanonicalName name = null, IScope parent = null)
        {
            if (name == null)
                _name = new CanonicalName(Guid.NewGuid().ToString());
            else
                _name = name;
            _parent = parent;
            SubScopes.CollectionChanged += Scope_CollectionChanged;
            this.PropertyChanged += Scope_PropertyChanged;
            // Register with Data types
            DataType.OnCreation += Scope_DataTypeOnCreation;
            DataType.OnRemoval += Scope_DataTypeOnRemoval;
        }

        #region "Properties"

        /// <summary>
        /// gets the unique id of this scope level
        /// </summary>
        public virtual string Id
        {
            get
            {
                return _name.FullId;
            }
        }
        /// <summary>
        /// gets or sets the name
        /// </summary>
        public virtual CanonicalName Name
        {
            get { return _name; }
        }
        /// <summary>
        /// gets or sets the parent scope
        /// </summary>
        public virtual IScope Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
                RaisePropertyChanged(ConstPropertyParent);
            }
        }
        /// <summary>
        /// gets the children scope
        /// </summary>
        public virtual ObservableCollection<IScope> SubScopes
        {
            get
            {
                return _children;
            }
        }

        /// <summary>
        /// gets or sets the repository of this scope
        /// </summary>
        public virtual IRepository Repository
        {
            get
            {
                return _current;
            }
            set
            {
                _current = value;
                RaisePropertyChanged(ConstPropertyRepository);
            }
        }

        /// <summary>
        /// gets or sets the Engine
        /// </summary>
        public virtual Engine Engine
        {
            get
            {
                return _engine;
            }
            set
            {
                _engine = value;
                RaisePropertyChanged(ConstPropertyEngine);
            }
        }

        #endregion

        /// <summary>
        /// raise the property changed event
        /// </summary>
        /// <param name="name"></param>
        protected virtual void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        /// <summary>
        /// PropertyChanged Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal virtual void Scope_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // set the engine property also to the nodes
            if (e.PropertyName == ConstPropertyEngine)
            {
                // add the known data object repositories
                foreach (IDataObjectEngine anEngine in this.Engine.DataObjectEngines)
                    this.Repository.RegisterDataObjectRepository(anEngine.Objects);
                this.Engine.DataObjectRepositoryAdded += Scope_DataObjectRepositoryAdded;
                // add the engine also to the children
                foreach (IScope aScope in SubScopes)
                    if (aScope != null)
                        aScope.Engine = this.Engine;
            }
        }
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal virtual void Scope_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // set the parent
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                foreach (IScope aScope in e.NewItems)
                {
                    if (aScope != null)
                    {
                        aScope.Parent = this;
                        aScope.Engine = this.Engine;
                    }
                }
        }
        /// <summary>
        /// handle added Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void Scope_DataObjectRepositoryAdded(object sender, Engine.EventArgs e)
        {
            this.Repository.RegisterDataObjectRepository(e.DataObjectRepository);
        }
        /// <summary>
        /// handle removed Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void Scope_DataObjectRepositoryRemoved(object sender, Engine.EventArgs e)
        {
            this.Repository.DeRegisterDataObjectRepository(e.DataObjectRepository);
        }
        /// <summary>
        /// event Handling routine of Datatype On Creation Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public virtual void Scope_DataTypeOnCreation(object sender, Core.DataType.EventArgs args)
        {
            // if we are in the same scope than create
            if ((args.Engine == null || args.Engine == this.Engine) && String.Compare(args.DataType.Name.ModuleId, this.Id, true) == 00)
            {
                this.Repository.Add(args.DataType);
            }
        }
        /// <summary>
        ///  event Handling routine of Datatype On Removal Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public virtual void Scope_DataTypeOnRemoval(object sender, Core.DataType.EventArgs args)
        {
            // remove
            if ((args.Engine == null || args.Engine == this.Engine) && String.Compare(args.DataType.Name.ModuleId, this.Id, true) == 00)
                this.Repository.Remove(args.DataType.Signature);
        }
        /// <summary>
        /// create a new scope and return it without doing it
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual IScope CreateScope(string id)
        {
            return new Scope(engine: this.Engine, id: id);
        }
        /// <summary>
        /// returns true if the Children have an ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool HasSubScope(string id)
        {
            if (this.SubScopes.Where(x => String.Compare(x.Id, id, true) == 00).FirstOrDefault() != null)
                return true;
            else
                return false;
        }
        /// <summary>
        /// returns true if one descendant has the scope name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual bool HasScope(CanonicalName name)
        {
            CanonicalName normalized = name.Reduce(this.Name);
            foreach (IScope aSub in this.SubScopes)
            {
                // if we have the name
                if (aSub.Name == name) return true;
                if (String.Compare(normalized.IDs.First(), aSub.Name.IDs.First(), ignoreCase: true) == 00)
                    return aSub.HasScope(name);
            }

            return false;
        }
        /// <summary>
        /// returns true if one descendant scope has the same id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool HasScope(string id)
        {
            return HasScope(new CanonicalName(id));
        }
        /// <summary>
        /// returns a Subscope of an given id or null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual IScope GetSubScope(string id)
        {
            return this.SubScopes.Where(x => String.Compare(x.Id, id, true) == 00).FirstOrDefault();
        }
        /// <summary>
        /// returns a scope object from the scope descendants by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual IScope GetScope(CanonicalName name)
        {
            CanonicalName normalized = name.Reduce(this.Name);
            foreach (IScope aSub in this.SubScopes)
            {
                // if we have the name
                if (aSub.Name == name) return aSub;
                if (String.Compare(normalized.IDs.First(), aSub.Name.IDs.First(), ignoreCase: true) == 00)
                    return aSub.GetScope(name);
            }
            return null;
        }
        /// <summary>
        /// returns a scope object from the descendants by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual IScope GetScope(string id)
        {
            return GetScope(new CanonicalName(id));
        }
        /// <summary>
        /// create an Subscope of an given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual IScope AddSubScope(string id)
        {
            // add the scope
            if (!HasSubScope(id))
            {
                this.SubScopes.Add(CreateScope(id));
            }
            // return the last scope
            return this.GetSubScope(id);
        }
        public virtual IScope GetRoot()
        {
            if (_parent != null) return _parent.GetRoot();
            return this;
        }
        /// <summary>
        /// adds a scope object to the descendants
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public virtual bool AddScope(IScope scope)
        {
            if (!HasScope(scope.Name))
            {
                IScope aSub;
                CanonicalName normalized = scope.Name.Reduce(this.Name);
                if (!HasSubScope(normalized.IDs.First()))
                    aSub = CreateScope(CanonicalName.Push(this.Id, normalized.IDs.First()));
                else aSub = GetSubScope(normalized.IDs.First());

                return aSub.AddScope(scope);
            }
            return false;
        }
        /// <summary>
        /// add new scope object by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool AddScope(string id)
        {
            return AddScope(NewScope(id));
        }
        /// <summary>
        /// Add new scope object by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual bool AddScope(CanonicalName name)
        {
            return AddScope(NewScope(name));
        }
        /// <summary>
        /// creates a new scope object
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual IScope NewScope(string id)
        {
            return NewScope(new CanonicalName(id));
        }
        /// <summary>
        /// creates a new scope object
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual IScope NewScope(CanonicalName name)
        {
            // create the nested scope
            CanonicalName aNestedName = null;
            foreach (var subscope in name.IDs)
            {
                IScope aScope = null;
                if (aNestedName == null)
                {
                    aNestedName = new CanonicalName(subscope);
                    // create or get
                    if (this.Engine.HasScope(aNestedName)) aScope = this.Engine.GetScope(aNestedName);
                    else aScope = CreateScope(aNestedName.FullId);
                }
                else
                {
                    // create or get
                    if (this.Engine.HasScope(aNestedName)) aScope = this.Engine.GetScope(aNestedName);
                    else aScope = CreateScope(aNestedName.FullId); ;

                    aNestedName = new CanonicalName(aNestedName.Push(subscope));
                    // create
                    aScope.AddSubScope(aNestedName.FullId);
                }

            }
            // return the scope
            return this.Engine.GetScope(name);
        }
        /*
        /// <summary>
        /// returns a rule rule from the repository or creates a new one and returns this
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public virtual SelectionRule GetSelectionRule(string id = null)
        {
            if (Repository.HasSelectionRule(id))
                return Repository.GetSelectionRule(id);
            else if (IScope != null && IScope.HasSelectionRule(id))
                return IScope.GetSelectionRule(id);
            // create a selection rule and return
            var aRule = new SelectionRule(id);
            Repository.AddSelectionRule(aRule);
            return aRule;
        }
        /// <summary>
        /// add a symbol to the scope
        /// </summary>
        /// <param name="symbol"></param>
        public virtual bool AddSelectionRule(SelectionRule rule)
        {
            if (!this.Repository.HasSelectionRule(rule.Id))
                return this.Repository.AddSelectionRule(rule);

            return false;
        }
        /// <summary>
        /// returns true if the selection rule by id is found in this Scope
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        public virtual bool HasSelectionRule(string id)
        {
            if (Repository.HasSelectionRule(id))
                return true;
            else if (IScope != null)
                return IScope.HasSelectionRule(id);
            return false;
        }
        /// <summary>
        /// gets the Operator definition for the Token ID
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public virtual Operator GetOperator(Token id)
        {
            if (Repository.HasOperator(id))
                return Repository.GetOperator(id);
            else if (IScope != null && IScope.HasOperator(id))
                return IScope.GetOperator(id);
            return null;
        }
        /// <summary>
        /// return true if the operator is found here
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool HasOperator(Token id)
        {
            if (Repository.HasOperator(id))
                return true;
            else if (IScope != null)
                return IScope.HasOperator(id);
            return false;
        }
        /// <summary>
        /// gets the Operator definition for the Token ID
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public virtual Function GetFunction(Token id)
        {
            if (Repository.HasFunction(id))
                return Repository.GetFunction(id);
            else if (IScope != null && IScope.HasFunction(id))
                return IScope.GetFunction(id);
            return null;
        }
        /// <summary>
        /// returns true if the function is in scope
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool HasFunction(Token id)
        {
            if (Repository.HasFunction(id))
                return true;
            else if (IScope != null)
                return IScope.HasFunction(id);
            return false;
        }
        /// <summary>
        /// adds a data object definition to the repository
        /// </summary>
        /// <param name="objectdefinition"></param>
        /// <returns></returns>
        public virtual bool AddDataObjectDefinition(IObjectDefinition objectdefinition)
        {
            if (!this.Repository.HasDataObjectDefinition(objectdefinition.Id))
                return this.Repository.AddDataObjectDefinition(objectdefinition);

            return false;
        }
        /// <summary>
        /// returns true if the data object is in scope
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool HasDataObjectDefinition(string id)
        {
            if (Repository.HasDataObjectDefinition(new ObjectName(moduleid: this.Id, objectid: id)))
                return true;
            else if (IScope != null)
                return IScope.HasDataObjectDefinition(id);
            return false;
        }
        /// <summary>
        /// gets the Operator definition for the ID
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public virtual IObjectDefinition GetDataObjectDefinition(string id)
        {
            if (Repository.HasDataObjectDefinition(id))
                return Repository.GetDataObjectDefinition(id);
            else if (IScope != null && IScope.HasDataObjectDefinition(id))
                return IScope.GetDataObjectDefinition(id);
            return null;
        }
        /// <summary>
        /// add a Data type to the scope
        /// </summary>
        /// <param name="datatype"></param>
        public virtual bool AddDataType(IDataType datatype)
        {
            if (!this.Repository.Has<IDataType>(datatype.Id))
                return this.Repository.AddDataType(datatype);

            return false;
        }
        /// <summary>
        /// returns true if the data type is in scope
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Has<IDataType>(string id)
        {
            if (Repository.Has<IDataType>(id))
                return true;
            else if (IScope != null)
                return IScope.Has<IDataType>(id);
            return false;
        }
        /// <summary>
        /// gets the data type definition for the ID
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public virtual IDataType GetDataType(string id)
        {
            if (Repository.Has<IDataType>(id))
                return Repository.GetDataType(id);
            else if (IScope != null && IScope.Has<IDataType>(id))
                return IScope.GetDataType(id);
            return null;
        }
        /// <summary>
        /// return the data type by object name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual IDataType GetDataType (ObjectName name)
        {
            return GetDataType(name.FullId);
        }
       
        /// <summary>
        /// add a symbol to the scope
        /// </summary>
        /// <param name="symbol"></param>
        public virtual bool AddSymbol(ISymbol symbol)
        {
            if (!this.Repository.HasSymbol(symbol.Id))
                return this.Repository.AddSymbol(symbol);

            return false;
        }
        /// <summary>
        /// remove a symbol from the scope
        /// </summary>
        /// <param name="symbol"></param>
        public virtual bool RemoveSymbol(string id)
        {
            if (this.Repository.HasSymbol(id))
                return this.Repository.RemoveSymbol(id);
            return false;
        }
        /// <summary>
        /// returns true if the symbol is known in this scope by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool HasSymbol(string id)
        {
            if (this.Repository.HasSymbol(id)) return true;
            return false;
        }
        /// <summary>
        /// returns the ISymbol from this Scope
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual ISymbol GetSymbol(string id)
        {
            if (this.Repository.HasSymbol(id)) return this.Repository.GetSymbol(id);
            return null;
        }
        /// <summary>
        /// add a Data type to the scope
        /// </summary>
        /// <param name="datatype"></param>
        public virtual bool AddModule(Module module)
        {
            if (!this.Repository.HasModule(module.Id))
                return this.Repository.AddModule(module);

            return false;
        }
        /// <summary>
        /// returns true if the data type is in scope
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool HasModule(string id)
        {
            if (Repository.HasModule(id))
                return true;
            else if (IScope != null)
                return IScope.HasModule(id);
            return false;
        }
        /// <summary>
        /// gets the data type definition for the ID
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public virtual Module GetModule(string id)
        {
            if (Repository.HasModule(id))
                return Repository.GetModule(id);
            else if (IScope != null && IScope.HasModule(id))
                return IScope.GetModule(id);
            return null;
        }
        /// <summary>
        /// return the data type by object name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual Module GetModule(CanonicalName name)
        {
            return GetModule(name.FullId);
        }
        */
        public bool RegisterDataObjectRepository(IDataObjectRepository iDataObjectRepository)
        {
            throw new NotImplementedException();
        }

        public bool DeRegisterDataObjectRepository(IDataObjectRepository iDataObjectRepository)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// add an isigned object to the repository
        /// </summary>
        /// <param name="signed"></param>
        /// <returns></returns>
        public bool Add(ISigned signed)
        {
            // add a scope to the scope
            if (signed.GetType().GetInterface(name: typeof(IScope).Name) != null)
                this.AddScope((IScope)signed);

            return false;
        }

        public bool Has(ISignature signature)
        {
            throw new NotImplementedException();
        }

        public bool Has<T>(ISignature signature = null) where T : ISigned
        {
            throw new NotImplementedException();
        }

        public bool Has(CanonicalName name)
        {
            throw new NotImplementedException();
        }

        public bool Has<T>(CanonicalName name) where T : ISigned
        {
            throw new NotImplementedException();
        }

        public IList<ISigned> Get(ISignature signature)
        {
            throw new NotImplementedException();
        }

        public IList<T> Get<T>(ISignature signature = null) where T : ISigned
        {
            throw new NotImplementedException();
        }

        public IList<T> Get<T>(CanonicalName name) where T : ISigned
        {
            throw new NotImplementedException();
        }

        public bool Remove(ISignature signature)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// a repository
    /// </summary>
    public class Repository : IRepository
    {
        /// <summary>
        /// Attribute class for marking the to be stored interface classes (for ISigned objects)
        /// </summary>
        [System.AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Interface)]
        public class StoreType : System.Attribute
        {
            readonly bool _askDataObjectRepository = false;
            public StoreType(bool askDataObjectRepository = false)
            {
                _askDataObjectRepository = askDataObjectRepository;
            }
            /// <summary>
            /// returns true if the Type has to be asked at the data object repository
            /// </summary>
            public bool AskDataObjectRepository { get { return _askDataObjectRepository;}}
        }
        private readonly string _id; // ID of the Repository
        private readonly Engine _engine; // my engine

        /// <summary>
        /// main dictionary
        /// </summary>
        protected readonly ObservableConcurrentDictionary<ISignature, IList<ISigned>> _main = new ObservableConcurrentDictionary<ISignature, IList<ISigned>>();
        /// <summary>
        /// indexed by type and then name
        /// </summary>
        protected readonly ObservableConcurrentDictionary<System.Type, IDictionary <CanonicalName, IList<ISigned>>> _byType_Name 
            = new ObservableConcurrentDictionary<Type, IDictionary<CanonicalName, IList<ISigned>>>();
        /// <summary>
        /// indexed by name
        /// </summary>
        protected readonly ObservableConcurrentDictionary<CanonicalName, IList<ISigned>> _byName = new ObservableConcurrentDictionary<CanonicalName, IList<ISigned>>();

        // Stack of dataObject Repositories
        protected readonly List<IDataObjectRepository> _dataobjectRepositories = new List<IDataObjectRepository>();
       
        // initialize Flag
        private bool _isInitialized = false;

        /// <summary>
        /// constructor of an engine
        /// </summary>
        public Repository(Engine engine, string id = null)
        {
            if (id == null)
                _id = Guid.NewGuid().ToString();
            else _id = id;
            _engine = engine;

            _main.PropertyChanged += Repository_main_PropertyChanged;
            _main.CollectionChanged += Repository_main_CollectionChanged;
        }

        #region "Properties"
        /// <summary>
        /// gets the unique handle of the engine
        /// </summary>
        public string Id
        {
            get
            {
                return _id;
            }
        }
        /// <summary>
        /// returns the Engine
        /// </summary>
        public Engine Engine
        {
            get
            {
                return _engine;
            }
        }
        /// <summary>
        /// gets all the rule rules in the repository
        /// </summary>
        public IList<T> Objects<T> () where T : ISigned
        {
            return this.Get<T>().ToList();
        }
        /// <summary>
        /// return true if initialized
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
        }
        #endregion
        #region EventHandling
        /// <summary>
        /// gets the StoreType Attribute of a Type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Repository.StoreType GetStoreTypeAttribute(System.Type type)
        {
            // if not an interface then check all interfaces
            if (!type.IsInterface) 
                foreach (var aType in type.GetInterfaces())
                {
                    var anAttribute =GetStoreTypeAttribute(aType);
                    if (anAttribute!=null)  return anAttribute;
                }
                     
            else
            // return matching interfaces
             return (Repository.StoreType) type.GetCustomAttributes(inherit:false)
                 .Where(x => x.GetType() == typeof(Repository.StoreType))
                 .FirstOrDefault();
            // else null
            return null;
        }
        /// <summary>
        /// returns the Type of the ISigned Item to store
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        private System.Type GetItemStoreType(System.Type type)
        {
            foreach (var aType in type.GetInterfaces())
                // store if the StoreType Attribute is found
                if (GetStoreTypeAttribute(aType)!=null)  return aType;

            // everything else
            return null;
        }
        /// <summary>
        /// Main Collection changed - update the secondary indices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Repository_main_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IList<ISigned> aList;
            IDictionary<CanonicalName, IList<ISigned>> aBucket;

            // Items added
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();

                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                
                    ///
                    /// remove the old items
                    ///
                    foreach (var item in e.OldItems)
                    {

                        if (item.GetType().GetInterfaces().Where(x => x == typeof(ISigned)).FirstOrDefault() != null)
                        {
                            var signed = (ISigned)item;
                            var storeType = this.GetItemStoreType(item.GetType());
                            CanonicalName aName = null;
                            // get the canonical name
                            System.Reflection.PropertyInfo property = item.GetType().GetProperties(System.Reflection.BindingFlags.FlattenHierarchy)
                                .Where(x => x.PropertyType.IsSubclassOf(typeof(CanonicalName)) && String.Compare(x.Name, strB: "Name", ignoreCase: true) == 0)
                                .FirstOrDefault();
                            if (property != null)
                                aName = (CanonicalName)property.GetValue(item, index: null);
                            if (aName != null)
                            {
                                // remove by Type and Name
                                //
                                if (storeType != null && _byType_Name.ContainsKey(key: storeType))
                                {
                                    aBucket = _byType_Name[key: GetType()];
                                    if (aBucket.ContainsKey(key: aName))
                                    {
                                        aList = aBucket[key: aName];
                                        if (aList.Contains(signed)) aList.Remove(signed);
                                        if (aList.Count() == 0) aBucket.Remove(key: aName);
                                    }
                                    if (aBucket.Count() == 0) _byType_Name.Remove(key: storeType);
                                }

                                // remove by Name
                                //
                                // IDictionary<CanonicalName, IList<ISigned>> aBucket;
                                if (_byName.ContainsKey(key: aName))
                                {
                                    aList = _byName[key: aName];
                                    // finally update the inner list
                                    if (aList.Contains(signed)) aList.Remove(signed);
                                    if (aList.Count() == 0) _byName.Remove(key: aName);
                                }
                            }
                        }

                    }
                    ///
                    /// add the new items
                    /// 
                    foreach (var item in e.NewItems)
                    {

                        if (item.GetType().GetInterfaces().Where(x => x == typeof(ISigned)).FirstOrDefault() != null)
                        {
                            var signed = (ISigned)item;
                            var storeType = this.GetItemStoreType(item.GetType());
                            CanonicalName aName = null;
                            // get the canonical name
                            System.Reflection.PropertyInfo property = item.GetType().GetProperties(System.Reflection.BindingFlags.FlattenHierarchy)
                                .Where(x => x.PropertyType.IsSubclassOf(typeof(CanonicalName)) && String.Compare(x.Name, strB: "Name", ignoreCase: true) == 0)
                                .FirstOrDefault();
                            if (property != null)
                                aName = (CanonicalName)property.GetValue(item, index: null);
                            if (storeType != null && aName != null)
                            {
                                // update by Type and Name
                                //
                                if (!_byType_Name.ContainsKey(key:storeType))
                                {
                                    aBucket = new ConcurrentDictionary<CanonicalName, IList<ISigned>>();
                                    _byType_Name.Add(key: item.GetType(), value: aBucket);
                                }
                                else aBucket = _byType_Name[key: storeType];
                                /// update the bucket
                                if (!aBucket.ContainsKey(key: aName))
                                {
                                    aList = new List<ISigned>();
                                    aBucket.Add(key: aName, value: aList);
                                }
                                else aList = aBucket[key: aName];
                                // finally update the inner list
                                if (aList.Contains(signed)) aList.Remove(signed);
                                aList.Add(signed);

                                // update by Name
                                //
                                // IDictionary<CanonicalName, IList<ISigned>> aBucket;
                                if (!_byName.ContainsKey(key: aName))
                                {
                                    aList = new List<ISigned>();
                                    _byName.Add(key: aName, value: aList);
                                }
                                else aList = _byName[key: aName];
                                // finally update the inner list
                                if (aList.Contains(signed)) aList.Remove(signed);
                                aList.Add(signed);
                            }
                        }

                    }
                    
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    _byName.Clear();
                    _byType_Name.Clear();
                    break;
                default:
                    break;
            }
           if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
               
            }
        }
        /// <summary>
        /// Property Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Repository_main_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
        }
        /// <summary>
        /// register the DataObjectEntrySymbol Repository
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public virtual bool RegisterDataObjectRepository(IDataObjectRepository repository)
        {
            if (_dataobjectRepositories.Contains(repository))
            {
                _dataobjectRepositories.Add(repository);
                return true;
            }
            return false;
        }
        /// <summary>
        /// register the DataObjectEntrySymbol Repository
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public virtual bool DeRegisterDataObjectRepository(IDataObjectRepository repository)
        {
            if (_dataobjectRepositories.Contains(repository))
            {
                _dataobjectRepositories.Remove(repository);
                return true;
            }
            return false;
        }
        #endregion
        /// <summary>
        /// lazy initialize
        /// </summary>
        /// <returns></returns>
        private bool Initialize()
        {
            if (_isInitialized)
                return false;

            _isInitialized = true;
            return _isInitialized;
        }
        /// <summary>
        /// adds a ISigned object to the repository
        /// </summary>
        /// <param name="signed"></param>
        /// <returns></returns>
        public bool Add(ISigned signed)
        {
            Initialize();
            if (GetStoreTypeAttribute(signed.GetType()).AskDataObjectRepository)
                foreach (IDataObjectRepository aRepository in _dataobjectRepositories)
                {
                    var aDefinition = aRepository.Get(signed.Signature);
                    if (aDefinition != null)
                    {
                        throw new NotImplementedException("Add DataObject from the engine");
                    }
                }
            else
            {
                if (this.Has(signed.Signature))   return false;

                // add the signed element
                IList<ISigned> aList ;
                if (!_main.ContainsKey(signed.Signature)) 
                {   
                    aList = new List<ISigned>();
                    _main.Add(key: signed.Signature, value: aList);
                }
                else aList = _main[signed.Signature];
                // add object
                aList.Add(signed);
                return true;
            }

            return false;
        }
        /// <summary>
        /// returns true if the repository has an ISigned object with signature
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        public bool Has(ISignature signature)
        {
            Initialize();
            // check _byName
            if ( _main.ContainsKey(key: signature)) return true;
            // check data object repositories
            foreach (IDataObjectRepository aRepository in this._dataobjectRepositories)
                    if (aRepository.Has(signature)) return true;
            // return false
            return false;
        }
        /// <summary>
        /// returns true if the ISigned derived type T is in the repository
        /// optional: AND an ISigned of T with the signature exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signature"></param>
        /// <returns></returns>
        public bool Has<T>(ISignature signature = null) where T : ISigned
        {
            IList<ISigned> aList = new List<ISigned>();
            Initialize();
            // check the signature and then lookup if the type is correct
            if (signature != null && _main.TryGetValue(key: signature, value: out aList))
            {
                if (aList.OfType<T>().FirstOrDefault() != null) return true;
                 // check if object must be found in data object repository
                if (GetStoreTypeAttribute(typeof(T)).AskDataObjectRepository)
                {
                    foreach (IDataObjectRepository aRepository in this._dataobjectRepositories)
                        if (aRepository.Has<T>(signature)) return true;
                }
            }
            else if (signature == null) 
            {
                if (_byType_Name.ContainsKey(typeof(T)))  return true;
                // check if object must be found in data object repository
                if (GetStoreTypeAttribute(typeof(T)).AskDataObjectRepository)
                {
                    foreach (IDataObjectRepository aRepository in this._dataobjectRepositories)
                        if (aRepository.Has<T>()) return true;
                }
            }

            return false;
        }
        /// <summary>
        /// returns true if the repository has an ISigned object with the canonical name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Has(CanonicalName name) 
        {
            Initialize();
            // check _byName
            if ( _byName.ContainsKey(key: name)) return true;
            // check data object repositories
            foreach (IDataObjectRepository aRepository in this._dataobjectRepositories)
                    if (aRepository.Has(name)) return true;
            // return false
            return false;
        }
        /// <summary>
        /// returns true if the repository has an ISigned object derived Class T and
        /// the CanonicalName name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Has<T> (CanonicalName name) where T : ISigned
        {
            Initialize();
             // check if object must be found in data object repository
            if (GetStoreTypeAttribute(typeof(T)).AskDataObjectRepository)
            {
                foreach (IDataObjectRepository aRepository in this._dataobjectRepositories)
                    if (aRepository.Has<T>(name)) return true;
            }
            // check if object is not to be found in the data object repository
            else
                if (_byType_Name.ContainsKey (key: typeof(T)))
                    return _byType_Name[key: typeof(T)].ContainsKey(name);
            // else
            return false;
        }
        /// <summary>
        /// gets all ISigned objects in the repository with the signature or empty list
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        public IList<ISigned> Get (ISignature signature)
        {
            IList<ISigned> aList = new List<ISigned>();
            Initialize();
            // check the main
            _main.TryGetValue(key: signature, value: out aList);
            // look into the data object repositories
            foreach (IDataObjectRepository aRepository in this._dataobjectRepositories)
                    if (aRepository.Has(signature))
                        foreach(var item in aRepository.Get(signature)) aList.Add(item);
            // return
            return aList;
        }
        /// <summary>
        /// gets all ISigned derived objects with the optional signature
        /// or empty list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signed"></param>
        /// <returns></returns>
        public IList<T> Get<T> (ISignature signature = null) where T : ISigned
        {
            var aList = new List<T>();
            Initialize();
            // check if object must be found in data object repository
            if (GetStoreTypeAttribute(typeof(T)).AskDataObjectRepository)
            {
                foreach (IDataObjectRepository aRepository in this._dataobjectRepositories)
                    if (aRepository.Has<T>(signature))
                        aList.Add((T)aRepository.Get<T>(signature));
            }
            else 
            {
                // get the list
                foreach (var innerList in _byType_Name[key: typeof(T)])
                    foreach (var item in innerList.Value)
                        if (signature == null || (signature != null && item.Signature == signature))
                            aList.Add((T)item);
            }
            return aList;
        }
        /// <summary>
        /// gets all ISigned derived objects whith the canonical name
        /// or empty list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signed"></param>
        /// <returns></returns>
        public IList<T> Get<T>(CanonicalName name) where T : ISigned
        {
            var aList = new List<T>();
            Initialize();
            // check if object must be found in data object repository
            if (GetStoreTypeAttribute(typeof(T)).AskDataObjectRepository)
            {
                foreach (IDataObjectRepository aRepository in this._dataobjectRepositories)
                    if (aRepository.Has<T>(name: name))
                        aList.Add((T)aRepository.Get<T>(name: name));
            }
            else 
            {
                // empty list if not found
                if (!this.Has<T>() || (!this.Has<T>(name: name)))
                    return aList;

                // get the list
                foreach (var innerList in _byType_Name[key: typeof(T)])
                    if (innerList.Key == name)
                        foreach (var item in innerList.Value)
                            aList.Add((T)item);
            }

            return aList;
        }
        /// <summary>
        /// remove ISigned object in the repository with the signature
        /// returns true on success
        /// </summary>
        /// <param name="signature"></param>
        /// <returns>True if successfull</returns>
        public bool Remove(ISignature signature)
        {
            bool aFlag = false;
            Initialize();
            // remove from data object repositories
            foreach (IDataObjectRepository aRepository in _dataobjectRepositories)
                if (aRepository.Has(signature))
                    aFlag |= aRepository.Remove(signature);

            // remove from main
            if (this.Has(signature)) 
                aFlag |= _main.Remove(key: signature);

            return aFlag;
        }

        /// <summary>
        /// add a dataobject to the scope
        /// </summary>
        /// <param name="dataobject"></param>
        public  bool AddDataObjectDefinition(IObjectDefinition dataobject)
        {
            
            Initialize();
            foreach (IDataObjectRepository aRepository in _dataobjectRepositories)
            {
                IObjectDefinition aDefinition = aRepository.GetIObjectDefinition(dataobject.Name);
                if (aDefinition != null)
                {
                    throw new NotImplementedException("Add DataObject from the engine");
                }
            }
            throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { dataobject.Name, "DataObjectEntrySymbol Repositories" });
            return false;
        }
        /// <summary>
        /// returns true if the id exists in the Repository
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasDataObjectDefinition(string id)
        {
            return HasDataObjectDefinition(new ObjectName(id.ToUpper()));
        }
        /// <summary>
        /// returns true if the name exits in the repository
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasDataObjectDefinition(ObjectName name)
        {
            Initialize();
            foreach (IDataObjectRepository aRepository in _dataobjectRepositories)
            {
                if (aRepository.HasObjectDefinition(name))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// returns dataobject definition by object id
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public IObjectDefinition GetDataObjectDefinition(String id)
        {
            return GetDataObjectDefinition(new ObjectName(id.ToUpper()));
        }
        /// <summary>
        /// returns a dataobject definition by object name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IObjectDefinition GetDataObjectDefinition(ObjectName name)
        {
            Initialize();
            foreach (IDataObjectRepository aRepository in _dataobjectRepositories)
            {
                IObjectDefinition aDefinition = aRepository.GetIObjectDefinition(name);
                if (aDefinition != null) return aDefinition;
            }
            throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { name, "DataObjectEntrySymbol Repositories" });
        }
        
    }
}