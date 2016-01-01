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
    /// defines the function
    /// </summary>
    public class @Function : IComparable<@Function>
    {

        /// <summary>
        /// get the _BuildInFunctions -> must be in Order of the TokenID
        /// </summary>
        private readonly static List<@Function> _buildInFunctions = new List<@Function>();
        /// <summary>
        /// static constructor
        /// </summary>
        static @Function()
        {
            // build the build-in functions
            _buildInFunctions.Add(new @Function(Token.BEEP, CreateSignature(PrimitiveType.GetPrimitiveType(otDataType.Null)), PrimitiveType.GetPrimitiveType(otDataType.Bool)));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static string CreateSignature(params IDataType[] types)
        {
            string signature = String.Empty;
            foreach (IDataType aType in types)
            {
                if (!String.IsNullOrEmpty(signature)) signature += ",";
                signature += aType.Signature;
            }
            return signature;
        }
        /// <summary>
        /// inner variables
        /// </summary>
        private readonly Token _token;
        private readonly string _signature;
        private readonly IDataType _returntype;
        /// <summary>
        /// returns a List of BuildInFunctions
        /// </summary>
        /// <returns></returns>
        public static List<@Function> BuildInFunctions()
        {
            return _buildInFunctions.ToList();
        }
        /// <summary>
        /// return the Operator Definition
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static OnTrack.Rulez.@Function GetFunction(Token token)
        {
            if (token.ToUint < _buildInFunctions.Count) return _buildInFunctions.ToArray()[token.ToUint - Token.CONCAT];
            throw new RulezException(RulezException.Types.OutOfArraySize, arguments: new object[] { token.ToUint, _buildInFunctions.Count });
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="arguments"></param>
        /// <param name="priority"></param>
        public @Function(Token token, string signature, IDataType returnType)
        {
            _token = token;
            _signature = signature;
            _returntype = returnType;
        }
        public @Function(uint tokenID, string signature, IDataType returnType)
        {
            _token = new Token(tokenID);
            _signature = signature;
            _returntype = returnType;
        }
        #region "Properties"
        /// <summary>
        /// gets the Token
        /// </summary>
        public Token Token { get { return _token; } }
        /// <summary>
        /// gets the signature
        /// </summary>
        public string Signature { get { return _signature; } }
        /// <summary>
        /// gets or sets the return type of the operation
        /// </summary>
        public IDataType ReturnType { get { return _returntype; } }
        #endregion
        /// <summary>
        /// implementation of comparable
        /// </summary>
        /// <param id="obj"></param>
        /// <returns></returns>
        public int CompareTo(@Function obj)
        {
            return this.Token.CompareTo(obj.Token);
        }
        /// <summary>
        /// override Hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int) this.Token.GetHashCode();
        }
        /// <summary>
        /// To string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Token.ToString() + "<" + this.Signature + ">";
        }
        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (obj == null || !(obj is @Function))
                return false;
            else
                return this.CompareTo((@Function) obj) == 0;
        }
    }

    /// <summary>
    /// defines the operators
    /// </summary>
    public class Operator : IComparable<Operator>
    {

        /// <summary>
        /// get the _BuildInFunctions -> must be in Order of the TokenID
        /// </summary>
        private readonly static Operator[] buildInOperators = {

                                                  // logical Operations
                                                  new Operator(Token.TRUE,arguments:0,priority:7,returnTypeId:otDataType .Bool ,   type:otOperatorType.Logical  ) ,
                                                  new Operator(Token.AND,arguments:2,priority:5,  returnTypeId:otDataType .Bool , type: otOperatorType.Logical ) ,
                                                  new Operator(Token.ANDALSO,arguments:2,priority:5 ,  returnTypeId:otDataType .Bool,  type:otOperatorType.Logical ) ,
                                                  new Operator(Token.OR,arguments:2,priority:6, returnTypeId: otDataType .Bool ,  type:otOperatorType.Logical ) ,
                                                  new Operator(Token.ORELSE,arguments:2,priority:6,  returnTypeId:otDataType .Bool ,  type:otOperatorType.Logical ) ,
                                                  new Operator(Token.NOT,arguments:1,priority:7, returnTypeId:otDataType .Bool,  type:otOperatorType.Logical   ) ,
                                                  new Operator(Token.EQ,arguments:2,priority:8,  returnTypeId:otDataType .Bool ,  type:otOperatorType.Compare ) ,
                                                  new Operator(Token.NEQ,arguments:2,priority:8, returnTypeId: otDataType .Bool ,  type:otOperatorType.Compare ) ,
                                                  new Operator(Token.GT,arguments:2,priority:8, returnTypeId: otDataType .Bool ,  type:otOperatorType.Compare ) ,
                                                  new Operator(Token.GE,arguments:2,priority:8, returnTypeId: otDataType .Bool,  type:otOperatorType.Compare  ) ,
                                                  new Operator(Token.LT,arguments:2,priority:8, returnTypeId: otDataType .Bool ,  type:otOperatorType.Compare ) ,
                                                  new Operator(Token.LE,arguments:2,priority:8, returnTypeId: otDataType .Bool ,  type:otOperatorType.Compare ) ,

                                                  // Arithmetic - null means return type is determined by the operands
                                                  new Operator(Token.PLUS,arguments:2,priority:2, returnTypeId: null ,  type:otOperatorType.Arithmetic ) ,
                                                  new Operator(Token.MINUS,arguments:2,priority:2, returnTypeId: null ,  type:otOperatorType.Arithmetic ) ,
                                                  new Operator(Token.MULT,arguments:2,priority:1, returnTypeId: null ,  type:otOperatorType.Arithmetic ) ,
                                                  new Operator(Token.DIV,arguments:2,priority:1, returnTypeId: null ,  type:otOperatorType.Arithmetic ) ,
                                                  new Operator(Token.MOD,arguments:2,priority:1, returnTypeId: null ,  type:otOperatorType.Arithmetic ) ,
                                                  new Operator(Token.CONCAT,arguments: 2, priority: 1, returnTypeId:  null , type: otOperatorType.Arithmetic ) ,

        };

        /// <summary>
        /// inner variables
        /// </summary>
        private readonly Token _token;
        private readonly UInt16 _arguments;
        private readonly UInt16 _priority;
        private readonly IDataType _returntype;
        private readonly otOperatorType _type;

        /// <summary>
        /// returns a List of BuildInFunctions
        /// </summary>
        /// <returns></returns>
        public static List<Operator> BuildInOperators()
        {
            return buildInOperators.ToList();
        }
        /// <summary>
        /// return the Operator Definition
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Operator GetOperator(Token token)
        {
            Operator o = buildInOperators.Where(x => x.Token == token).FirstOrDefault();
            if (o == null) throw new RulezException(RulezException.Types.OperatorNotDefined, arguments: new object[] { token.ToString() });
            return o;
        }
        /// <summary>
        /// return the Operator Definition
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Operator GetOperator(uint tokenid)
        {
            if (tokenid < buildInOperators.Length) return buildInOperators[tokenid];
            throw new RulezException(RulezException.Types.OutOfArraySize, arguments: new object[] { tokenid, buildInOperators.Length });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param id="types"></param>
        /// <returns></returns>
        public static string CreateSignature(Token token, UInt16 arguments, UInt16 priority, IDataType returnType)
        {
            return token.ToString() + "<" + arguments.ToString() + "," + priority.ToString() + "," + (returnType == null ? returnType.ToString() : "*") + ">";
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param id="Token"></param>
        /// <param id="arguments"></param>
        /// <param id="priority"></param>
        public Operator(Token token, UInt16 arguments, UInt16 priority, otDataType? returnTypeId, otOperatorType type)
        {
            _token = token;
            _arguments = arguments;
            _priority = priority;
            if (returnTypeId.HasValue) _returntype = DataType.GetDataType(returnTypeId.Value);
            _type = type;
        }
        public Operator(Token token, UInt16 arguments, UInt16 priority, IDataType returnType, otOperatorType type)
        {
            _token = token;
            _arguments = arguments;
            _priority = priority;
            _returntype = returnType;
            _type = type;
        }
        public Operator(uint tokenID, UInt16 arguments, UInt16 priority, otDataType? returnTypeId, otOperatorType type)
        {
            _token = new Token(tokenID);
            _arguments = arguments;
            _priority = priority;
            if (returnTypeId.HasValue) _returntype = DataType.GetDataType(returnTypeId.Value); ;
            _type = type;

        }
        public Operator(uint tokenID, UInt16 arguments, UInt16 priority, IDataType returnType, otOperatorType type)
        {
            _token = new Token(tokenID);
            _arguments = arguments;
            _priority = priority;
            _returntype = returnType;
            _type = type;
        }
        #region "Properties"
        /// <summary>
        /// gets the Token
        /// </summary>
        public Token Token { get { return _token; } }

        /// <summary>
        /// gets the Number of Arguments
        /// </summary>
        public UInt16 Arguments { get { return _arguments; } }

        /// <summary>
        /// gets the Priority
        /// </summary>
        public UInt16 Priority { get { return _priority; } }

        /// <summary>
        /// gets or sets the return type id
        /// </summary>
        public otDataType? ReturnTypeId
        {
            get { return _returntype != null ? _returntype.TypeId : new otDataType?(); }
        }
        /// <summary>
        /// gets the Returntype
        /// </summary>
        public IDataType ReturnType
        {
            get { return _returntype; }
        }
        /// <summary>
        /// gets the type of operator
        /// </summary>
        public otOperatorType Type { get { return _type; } }
        #endregion

        /// <summary>
        /// override Hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int) this.Token.GetHashCode();
        }
        /// <summary>
        /// Equals
        /// </summary>
        /// <param id="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (obj == null || !(obj is Operator))
                return false;
            else
                return this.CompareTo((Operator) obj) == 0;
        }
        /// <summary>
        /// implementation of comparable
        /// </summary>
        /// <param id="obj"></param>
        /// <returns></returns>
        public int CompareTo(Operator obj)
        {
            return this.CompareTo(obj);
        }
        /// <summary>
        /// == comparerer on datatypes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Operator a, Operator b)
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
            return a.Token == b.Token;
        }
        /// <summary>
        /// != comparer
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Operator a, Operator b)
        {
            return !(a == b);
        }
        /// <summary>
        /// To string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Token.ToString();
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
                foreach (Scope aScope in scope.Children)
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
        public Scope(Engine engine, string id = null)
        {
            if (id == null)
                _name = new CanonicalName(Guid.NewGuid().ToString());
            else
                _name = new CanonicalName(id);

            Children.CollectionChanged += Scope_CollectionChanged;
            this.PropertyChanged += Scope_PropertyChanged;
            // Register with Data types
            DataType.OnCreation += Scope_DataTypeOnCreation;
            DataType.OnRemoval += Scope_DataTypeOnRemoval;
        }
        public Scope(Engine engine, CanonicalName name = null)
        {
            if (name == null)
                _name = new CanonicalName(Guid.NewGuid().ToString());
            else
                _name = name;

            Children.CollectionChanged += Scope_CollectionChanged;
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
        public virtual ObservableCollection<IScope> Children
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
                foreach (IScope aScope in Children)
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
                this.Repository.AddDataType(args.DataType);
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
                this.Repository.RemoveDataType(args.DataType);
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
            if (this.Children.Where(x => String.Compare(x.Id, id, true) == 00).FirstOrDefault() != null)
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
            foreach (IScope aSub in this.Children)
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
            return this.Children.Where(x => String.Compare(x.Id, id, true) == 00).FirstOrDefault();
        }
        /// <summary>
        /// returns a scope object from the scope descendants by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual IScope GetScope(CanonicalName name)
        {
            CanonicalName normalized = name.Reduce(this.Name);
            foreach (IScope aSub in this.Children)
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
                this.Children.Add(CreateScope(id));
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
        /// <summary>
        /// returns a rule rule from the repository or creates a new one and returns this
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public virtual SelectionRule GetSelectionRule(string id = null)
        {
            if (Repository.HasSelectionRule(id))
                return Repository.GetSelectionRule(id);
            else if (Parent != null && Parent.HasSelectionRule(id))
                return Parent.GetSelectionRule(id);
            // create a selection rule and return
            var aRule = new SelectionRule(id);
            Repository.AddSelectionRule(aRule.Id, aRule);
            return aRule;
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
            else if (Parent != null)
                return Parent.HasSelectionRule(id);
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
            else if (Parent != null && Parent.HasOperator(id))
                return Parent.GetOperator(id);
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
            else if (Parent != null)
                return Parent.HasOperator(id);
            return false;
        }
        /// <summary>
        /// gets the Operator definition for the Token ID
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public virtual @Function GetFunction(Token id)
        {
            if (Repository.HasFunction(id))
                return Repository.GetFunction(id);
            else if (Parent != null && Parent.HasFunction(id))
                return Parent.GetFunction(id);
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
            else if (Parent != null)
                return Parent.HasFunction(id);
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
            else if (Parent != null && Parent.HasDataObjectDefinition(id))
                return Parent.GetDataObjectDefinition(id);
            return null;
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
            else if (Parent != null)
                return Parent.HasDataObjectDefinition(id);
            return false;
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
    }

    /// <summary>
    /// a repository
    /// </summary>
    public class Repository : IRepository
    {
        private readonly string _id; // ID of the Repository
        private readonly Engine _engine; // my engine
        // Dictionary of operators
        protected readonly Dictionary<Token, Operator> _operators = new Dictionary<Token, Operator>();
        // Dictionary of functions
        protected readonly Dictionary<Token, @Function> _functions = new Dictionary<Token, @Function>();
        // Dictionary of the rule rules
        protected readonly Dictionary<String, SelectionRule> _selectionrules = new Dictionary<string, SelectionRule>();
        // Stack of dataObject Repositories
        protected readonly List<IDataObjectRepository> _dataobjectRepositories = new List<IDataObjectRepository>();
        // dictionary of types
        protected readonly Dictionary<string, IDataType> _datatypes = new Dictionary<string, IDataType>();
        protected readonly Dictionary<string, List<IDataType>> _datatypesSignature = new Dictionary<string, List<IDataType>>();
        // dictionary of symbols
        protected readonly Dictionary<string, ISymbol> _symbols = new Dictionary<string, ISymbol>();

        // initialize Flag
        private bool _isInitialized = false;

        /// <summary>
        /// constructor of an engine
        /// </summary>
        public Repository(Engine engine, string id = null)
        {
            if (id == null)
                _id = Guid.NewGuid().ToString();
            else
                _id = id;
            _engine = engine;
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
        public List<SelectionRule> SelectionRules
        {
            get
            {
                return _selectionrules.Values.ToList();
            }
        }

        /// <summary>
        /// gets all rule rule IDs in the repository
        /// </summary>
        public List<String> SelectionRuleIDs
        {
            get
            {
                return _selectionrules.Keys.ToList();
            }
        }

        /// <summary>
        /// gets all the operators in the repository
        /// </summary>
        public List<Operator> Operators
        {
            get
            {
                return _operators.Values.ToList();
            }
        }

        /// <summary>
        /// gets all operator tokens rule IDs in the repository
        /// </summary>
        public List<Token> OperatorTokens
        {
            get
            {
                return _operators.Keys.ToList();
            }
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
        /// <summary>
        /// lazy initialize
        /// </summary>
        /// <returns></returns>
        private bool Initialize()
        {
            if (_isInitialized)
                return false;

            // operator
            foreach (Operator anOperator in Operator.BuildInOperators())
            {
                if (!_operators.ContainsKey(anOperator.Token))
                    _operators.Add(anOperator.Token, anOperator);
            }

            // Functions
            foreach (@Function aFunction in @Function.BuildInFunctions())
            {
                if (!_functions.ContainsKey(aFunction.Token))
                    _functions.Add(aFunction.Token, aFunction);
            }
            // primitve Datatypes
            foreach (IDataType aDatatype in PrimitiveType.DataTypes)
            {
                if (!_datatypes.ContainsKey(aDatatype.Id.ToUpper()))
                {
                    _datatypes.Add(aDatatype.Id.ToUpper(), aDatatype);
                    if (!_datatypesSignature.ContainsKey(aDatatype.Signature.ToUpper()))
                        _datatypesSignature.Add(aDatatype.Signature.ToUpper(), new List<IDataType>());
                    List<IDataType> aList = _datatypesSignature[aDatatype.Signature.ToUpper()];
                    // remove all existing
                    aList.RemoveAll(x => x.Id == aDatatype.Id);
                    aList.Add(aDatatype);
                }
            }
            _isInitialized = true;
            return _isInitialized;
        }
        /// <summary>
        /// returns true if the repository has the rule rule
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool HasSelectionRule(string id)
        {
            Initialize();
            return _selectionrules.ContainsKey(id);
        }
        /// <summary>
        /// returns the selectionrule by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public SelectionRule GetSelectionRule(string id)
        {
            Initialize();
            if (this.HasSelectionRule(id))
                return _selectionrules[id];
            throw new KeyNotFoundException(id + " was not found in repository");
        }
        /// <summary>
        /// adds a rule rule to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public bool AddSelectionRule(string id, SelectionRule rule)
        {
            Initialize();
            if (this.HasSelectionRule(id))
                _selectionrules.Remove(id);
            _selectionrules.Add(id, rule);
            return true;
        }
        /// <summary>
        /// adds a rule rule to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param id="rule"></param>
        /// <returns></returns>
        public bool RemoveSelectionRule(string id)
        {
            Initialize();
            if (this.HasSelectionRule(id))
                return _selectionrules.Remove(id);
            return false;
        }
        /// <summary>
        /// returns true if the repository has the function
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool HasFunction(Token id)
        {
            Initialize();
            return _functions.ContainsKey(id);
        }
        /// <summary>
        /// returns the function by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public @Function GetFunction(Token id)
        {
            Initialize();
            if (this.HasFunction(id))
                return _functions[id];
            throw new KeyNotFoundException(id + " was not found in repository");
        }
        /// <summary>
        /// adds a function to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public bool AddFunction(@Function function)
        {
            Initialize();
            if (this.HasFunction(function.Token))
                _functions.Remove(function.Token);
            _functions.Add(function.Token, function);
            return true;
        }
        /// <summary>
        /// returns true if the repository has the rule rule
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool HasOperator(Token id)
        {
            Initialize();
            if (id != null)
                return _operators.ContainsKey(id);
            return false;
        }
        /// <summary>
        /// returns the selectionrule by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public Operator GetOperator(Token id)
        {
            Initialize();
            if (this.HasOperator(id))
                return _operators[id];
            throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { id.ToString(), "Operator" });
        }
        /// <summary>
        /// adds a rule rule to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public bool AddOperator(Operator Operator)
        {
            Initialize();
            if (this.HasOperator(Operator.Token))
                _operators.Remove(Operator.Token);
            _operators.Add(Operator.Token, Operator);
            return true;
        }
        /// <summary>
        /// adds a rule rule to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public bool RemoveOperator(Token id)
        {
            Initialize();
            if (this.HasOperator(id))
                return _operators.Remove(id);
            return false;
        }
        /// <summary>
        /// returns true if the repository has the function
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool HasDataType(string name)
        {
            Initialize();
            return _datatypes.ContainsKey(name.ToUpper());
        }
        /// <summary>
        /// returns true if the repository has the function
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool HasDataType(IDataType datatype)
        {
            return HasDataType(datatype.Id);
        }
        /// <summary>
        /// returns true if the repository has the function
        /// </summary>
        /// <param signature="handle"></param>
        /// <returns></returns>
        public bool HasDataTypeSignature(string signature)
        {
            Initialize();
            return _datatypesSignature.ContainsKey(signature.ToUpper());
        }
        /// <summary>
        /// returns the datatype by name
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public IDataType GetDatatype(string Name)
        {
            Initialize();
            if (this.HasDataType(Name))
                return _datatypes[Name.ToUpper()];
            throw new RulezException(RulezException.Types.DataTypeNotFound, arguments: new object[] { Name.ToUpper() });
        }
        /// <summary>
        /// returns the datatype by name
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public List<IDataType> GetDatatypeBySignature(string signature)
        {
            Initialize();
            if (this.HasDataTypeSignature(signature))
                return _datatypesSignature[signature.ToUpper()];
            throw new RulezException(RulezException.Types.DataTypeNotFound, arguments: new object[] { signature.ToUpper() });
        }
        /// <summary>
        /// adds a datatype to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public bool AddDataType(IDataType datatype)
        {
            Initialize();
            if (this.HasDataType(datatype.Id))
                _datatypes.Remove(datatype.Id.ToUpper());
            _datatypes.Add(datatype.Id.ToUpper(), datatype);
            if (!this.HasDataTypeSignature(datatype.Signature))
                _datatypesSignature.Add(datatype.Signature.ToUpper(), new List<IDataType>());
            List<IDataType> aList = _datatypesSignature[datatype.Signature.ToUpper()];
            // remove all existing
            aList.RemoveAll(x => x.Id.ToUpper() == datatype.Id.ToUpper());
            aList.Add(datatype);
            return true;
        }
        /// <summary>
        /// adds a datatype to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public bool RemoveDataType(IDataType datatype)
        {
            Initialize();
            if (this.HasDataType(datatype.Id))
                _datatypes.Remove(datatype.Id.ToUpper());
            if (this.HasDataTypeSignature(datatype.Signature))
            {
                List<IDataType> aList = _datatypesSignature[datatype.Signature.ToUpper()];
                // remove all existing
                aList.RemoveAll(x => x.Id.ToUpper() == datatype.Id.ToUpper());
            }
            return true;
        }
        /// <summary>
        /// returns true if the repository has the symbol
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool HasSymbol(string id)
        {
            Initialize();
            if (!String.IsNullOrEmpty(id))
                return _symbols.ContainsKey(id.ToUpper());
            return false;
        }
        /// <summary>
        /// returns the symbol by id
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public ISymbol GetSymbol(string id)
        {
            Initialize();
            if (this.HasSymbol(id))
                return _symbols[id.ToUpper()];
            throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { id.ToString(), "Symbol" });
        }
        /// <summary>
        /// adds a rule rule to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public bool AddSymbol(ISymbol symbol)
        {
            Initialize();
            if (this.HasSymbol(symbol.Id)) RemoveSymbol(symbol.Id);
            _symbols.Add(symbol.Id.ToUpper(), symbol);
            return true;
        }
        /// <summary>
        /// adds a symbol to the repository by id
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public bool RemoveSymbol(string id)
        {
            Initialize();
            if (this.HasSymbol(id)) return _symbols.Remove(id.ToUpper());
            return false;
        }
        /// <summary>
        /// returns true if the id exists in the Repository
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasDataObjectDefinition(string id)
        {
            return HasDataObjectDefinition(new ObjectName(id));
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
            return GetDataObjectDefinition(new ObjectName(id));
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