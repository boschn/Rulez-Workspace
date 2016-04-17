/**
 *  ONTRACK RULEZ ENGINE
 *  
 * eXpression Tree
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OnTrack.Core;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace OnTrack.Rulez.eXPressionTree
{
    /// <summary>
    /// base class for all nodes 
    /// </summary>
    public abstract class Node : INode
    {

        protected Engine _engine; // internal engine
        protected readonly otXPTNodeType _nodeType;
        protected IXPTree _parent;
        protected List<Rulez.Message> _errorlist = new List<Message>();
        private CanonicalName _scopeName = new CanonicalName(CanonicalName.GlobalID);
        // event
        public event PropertyChangedEventHandler PropertyChanged;
        // constants
        public const string ConstPropertyParent = "Parent";
        public const string ConstPropertyEngine = "Engine";
        public const string ConstPropertyScopeID = "ScopeID";
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="engine"></partm>
        protected Node(otXPTNodeType nodetype, Engine engine = null)
        {
            _nodeType = nodetype;
            // default engine
            this.Engine = engine;
        }
        /// <summary>
        /// gets the node type
        /// </summary>
        public otXPTNodeType NodeType { get { return _nodeType; } }
        /// <summary>
        /// returns 
        /// </summary>
        public abstract bool HasSubNodes { get; }
        /// <summary>
        /// gets the Parent of the Node
        /// </summary>
        public IXPTree Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(ConstPropertyParent));
            }
        }
        /// <summary>
        /// returns the engine
        /// </summary>
        public Engine Engine
        {
            get { return _engine; }
            set
            {
                _engine = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(ConstPropertyEngine));
            }
        }
        /// <summary>
        /// returns the Errors of the Node
        /// </summary>
        public IList<Rulez.Message> Messages { get { return _errorlist; } }
        /// <summary>
        /// accept the visitor
        /// </summary>
        /// <param name="visitor"></param>
        public bool Accept(IVisitor<INode, object> visitor) { visitor.Visit(this); return true; }
        /// <summary>
        /// Scope id of the node
        /// </summary>
        public string ScopeId
        {
            get { return _scopeName.ToString(); }
            set
            {
                _scopeName = new CanonicalName(value);
                RaiseOnPropertyChanged(this, ConstPropertyScopeID);
            }
        }
        /// <summary>
        /// returns an IEnumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            var aList = new List<INode>();
            aList.Add(this);
            return aList.GetEnumerator();
        }
        public IEnumerator<INode> GetEnumerator()
        {
            var aList = new List<INode>();
            aList.Add(this);
            return aList.GetEnumerator();
        }
        /// <summary>
        /// raise the Property Changed Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="property"></param>
        protected void RaiseOnPropertyChanged(object sender, string property)
        {
            if (PropertyChanged != null) PropertyChanged(sender, new PropertyChangedEventArgs(property));
        }
        /// <summary>
        /// to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + NodeType.ToString() + ">";
        }
    }

    /// <summary>
    /// declare a constant node in an AST
    /// </summary>
    public class Literal : Node, IExpression
    {
        private readonly object _value;
        private readonly bool _hasValue = false;
        private readonly IDataType _datatype;

        /// <summary>
        /// constructor
        /// </summary>
        public Literal(object value = null, otDataType? typeId = null) : base(nodetype: otXPTNodeType.Literal)
        {
            if (typeId != null && typeId.HasValue) _datatype = Core.DataType.GetDataType(typeId.Value);
            if (value != null) _value = value;

            if ((typeId == null) && (value != null))
            {
                throw new NotImplementedException(message: "data type determination by value");
            }
        }
        public Literal(object value, IDataType datatype)
            : base(nodetype: otXPTNodeType.Literal)
        {
            _datatype = datatype;
            if (value != null) _value = Core.DataType.To(value, datatype); ;
        }

        /// <summary>
        /// returns true if node is a leaf
        /// </summary>
        public override bool HasSubNodes { get { return false; } }
        /// <summary>
        /// returns true if the value really has a value
        /// </summary>
        public bool HasValue { get { return _hasValue; } }
        /// <summary>
        /// gets or sets the constant value
        /// </summary>
        public object Value { get { return _value; } }
        /// <summary>
        /// returns the datatype of the literal
        /// </summary>
        public otDataType ReturnTypeId { get { return _datatype.TypeId; } }
        /// <summary>
        /// returns the datatype of the literal
        /// </summary>
        public IDataType ReturnType { get { return _datatype; } }
        /// <summary>
        /// gets or sets the type of the literal
        /// </summary>
        public System.Type NativeType
        {
            get
            {
                if (this.HasValue) if (this.Value != null) return this.Value.GetType();
                    else return this.ReturnType.NativeType;
                else return null;
            }
        }
        /// <summary>
        /// to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("<{0}:{1}>", (this.ReturnType != null) ? this.ReturnType.ToString() : NodeType.ToString(), this.Value);
        }
    }

    /// <summary>
    /// Base class for all tree nodes
    /// </summary>
    public abstract class XPTree : IXPTree
    {
        // instance variables
        private readonly ObservableCollection<INode> _nodes = new ObservableCollection<INode>();
        protected Engine _engine;
        private readonly otXPTNodeType _nodeType;
        private IXPTree _parent;
        private readonly List<Message> _errorlist = new List<Message>();
        private IScope _scope;
        // constants
        public const string ConstPropertyParent = "Parent";
        public const string ConstPropertyEngine = "Engine";
        public const string ConstPropertyScopeID = "ScopeID";
        // event
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="engine"></param>
        protected XPTree(otXPTNodeType nodetype, Engine engine = null)
        {
            _nodeType = nodetype;
            // default engine
            if (engine == null) engine = OnTrack.Rules.Engine;
            _scope = new XPTScope(engine);

            this.Nodes.CollectionChanged += XPTree_Nodes_CollectionChanged;
            this.PropertyChanged += XPTree_PropertyChanged;

        }
        /// <summary>
        /// PropertyChanged Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void XPTree_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // set the engine property also to the nodes
            if (e.PropertyName == ConstPropertyEngine)
            {
                foreach (INode aNode in Nodes) if (aNode != null) aNode.Engine = this.Engine;
            }
        }
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void XPTree_Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // set the parent
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (INode aNode in e.NewItems) { if (aNode != null) { aNode.Parent = this; aNode.Engine = this.Engine; } }
        }
        /// <summary>
        /// return the node type
        /// </summary>
        public otXPTNodeType NodeType { get { return _nodeType; } }
        /// <summary>
        /// set or get all the leaves .. setting is merging
        /// </summary>
        public ObservableCollection<INode> Nodes
        {
            get { return _nodes; }
            set {   // add all nodes
                foreach (var aNode in value) if (!_nodes.Contains(aNode)) _nodes.Add(aNode);
            }
        }
        /// <summary>
        /// returns true if node is a leaf
        /// </summary>
        public bool HasSubNodes { get { return true; } }
        /// <summary>
        /// gets the Parent of the Node
        /// </summary>
        public IXPTree Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _parent = value;
                RaiseOnPropertyChanged(this, ConstPropertyParent);
            }
        }
        /// <summary>
        /// returns the Errors of the Node
        /// </summary>
        public IList<Rulez.Message> Messages { get { return _errorlist; } }

        /// <summary>
        /// Scope id of the node
        /// </summary>
        /// <value></value>
        string INode.ScopeId
        {
            get
            {
                return _scope.Id;
            }
            set
            {
                _scope = new XPTScope(Engine, value);
            }
        }

        /// <summary>
        /// returns the engine
        /// </summary>
        public Engine Engine
        {
            get
            {
                return _engine;
            }
            set
            {
                _engine = value;
                RaiseOnPropertyChanged(this, ConstPropertyEngine);
            }
        }

        /// <summary>
        /// Scope of the XPTree
        /// </summary>
        public IScope Scope
        {
            get
            {
                return _scope;
            }
            set
            {
                _scope = value;
            }
        }

        /// <summary>
        /// accept the visitor
        /// </summary>
        /// <param id="visitor"></param>
        public bool Accept(IVisitor<INode, object> visitor) { visitor.Visit(this); return true; }
        /// <summary>
        /// returns an IEnumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Nodes.GetEnumerator();
        }
        public IEnumerator<INode> GetEnumerator()
        {
            return this.Nodes.GetEnumerator();
        }
        /// <summary>
        /// returns all DataObjectEntry names in the expression tree
        /// </summary>
        /// <returns></returns>
        public List<String> DataObjectEntryNames()
        {
            List<String> aList = new List<string>();
            Visitor<String> aVistor = new Visitor<String>();
            // define a simple handler via lambda
            Visitor<String>.Eventhandler aVisitingHandling
                = (o, e) => {
                    if (e.CurrentNode.GetType() == typeof(DataObjectEntrySymbol))
                        e.Stack.Push((e.CurrentNode as DataObjectEntrySymbol).Name.ToString());
                };
            aVistor.VisitingDataObjectSymbol += aVisitingHandling; // register
            aVistor.Visit(this); // run
            // get uniques
            foreach (String aName in aVistor.Stack.ToList<String>())
                if (!aList.Contains(aName)) aList.Add(aName);

            // return
            return aList;
        }
        /// <summary>
        /// raise the Property Changed Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="property"></param>
        protected void RaiseOnPropertyChanged(object sender, string property)
        {
            if (PropertyChanged != null) PropertyChanged(sender, new PropertyChangedEventArgs(property));
        }
        /// <summary>
        /// toString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            bool first = true;
            string aString = "{" + this.NodeType.ToString() + ":";
            foreach (INode aNode in Nodes)
            {
                aString += ((first == false) ? "," : String.Empty) + ((aNode != null) ? aNode.ToString() : "<NULL>");
                if (first) first = false;
            }
            aString += "}";
            return aString;
        }


        IScope IXPTree.Scope
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
    /// <summary>
    /// defines the operators
    /// </summary>
    public class Operator : XPTree, IComparable<IOperatorDefinition>, IOperatorDefinition
    {

        /// <summary>
        /// get the _BuildInFunctions -> must be in Order of the TokenID
        /// </summary>
        private readonly static IOperatorDefinition[] _buildInOperators = {

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
        private readonly ObjectName _name;
        private readonly ParameterList _parameters = new ParameterList();
        private readonly UInt16 _arguments;
        private readonly UInt16 _priority;
        private readonly IDataType _returntype;
        private readonly otOperatorType _type;

        /// <summary>
        /// returns a List of BuildInFunctions
        /// </summary>
        /// <returns></returns>
        public static List<IOperatorDefinition> BuildInOperators()
        {
            return _buildInOperators.ToList();
        }
        /// <summary>
        /// return the Operator Definition
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IOperatorDefinition GetOperator(Token token)
        {
            IOperatorDefinition o = _buildInOperators.Where(x => x.Token == token).FirstOrDefault();
            if (o == null) throw new RulezException(RulezException.Types.OperatorNotDefined, arguments: new object[] { token.ToString() });
            return o;
        }
        /// <summary>
        /// return the Operator Definition
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IOperatorDefinition GetOperator(uint tokenid)
        {
            if (tokenid < _buildInOperators.Length) return _buildInOperators[tokenid];
            throw new RulezException(RulezException.Types.OutOfArraySize, arguments: new object[] { tokenid, _buildInOperators.Length });
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
        public Operator(Token token, UInt16 arguments, UInt16 priority, otDataType? returnTypeId, otOperatorType type, Engine engine = null)
             : base(nodetype: otXPTNodeType.OperatorDefinition, engine: engine)
        {
            _token = token;
            _arguments = arguments;
            _priority = priority;
            if (returnTypeId.HasValue) _returntype = DataType.GetDataType(returnTypeId.Value);
            _type = type;
        }
        public Operator(Token token, UInt16 arguments, UInt16 priority, IDataType returnType, otOperatorType type, Engine engine = null)
            : base(nodetype: otXPTNodeType.OperatorDefinition, engine: engine)
        {
            _token = token;
            _arguments = arguments;
            _priority = priority;
            _returntype = returnType;
            _type = type;
        }
        public Operator(uint tokenID, UInt16 arguments, UInt16 priority, otDataType? returnTypeId, otOperatorType type, Engine engine = null)
            : base(nodetype: otXPTNodeType.OperatorDefinition, engine: engine)
        {
            _token = new Token(tokenID);
            _arguments = arguments;
            _priority = priority;
            if (returnTypeId.HasValue) _returntype = DataType.GetDataType(returnTypeId.Value); ;
            _type = type;

        }
        public Operator(uint tokenID, UInt16 arguments, UInt16 priority, IDataType returnType, otOperatorType type, Engine engine = null)
             : base(nodetype: otXPTNodeType.OperatorDefinition, engine: engine)
        {
            _token = new Token(tokenID);
            _arguments = arguments;
            _priority = priority;
            _returntype = returnType;
            _type = type;
        }
        #region "Properties"
        /// <summary>
        /// returns the object name
        /// </summary>
        public ObjectName Name { get { return _name; } }
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
        /// <summary>
        /// return list of parameters of the operator
        /// </summary>
        public ParameterList Parameters { get { return _parameters; } }
        /// <summary>
        /// gets the signature
        /// </summary>
        public ISignature Signature
        {
            get
            {
                return new NamedListSignature(name: this.Name, datatype: this.ReturnType, parameters: this.Parameters);
            }
        }
        #endregion

        /// <summary>
        /// override Hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)this.Signature.GetHashCode();
        }
        /// <summary>
        /// Equals
        /// </summary>
        /// <param id="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (obj == null || !(obj.GetType().GetInterfaces().Where(x => x.Equals(typeof(IOperatorDefinition))).FirstOrDefault() == null))
                return false;
            else
                return this.CompareTo((IOperatorDefinition)obj) == 0;
        }
        /// <summary>
        /// implementation of comparable
        /// </summary>
        /// <param id="obj"></param>
        /// <returns></returns>
        public int CompareTo(IOperatorDefinition obj)
        {
            return this.Signature.CompareTo(obj.Signature);
        }
        /// <summary>
        /// == comparerer on datatypes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Operator a, IOperatorDefinition b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Signature == b.Signature;
        }
        /// <summary>
        /// != comparer
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Operator a, IOperatorDefinition b)
        {
            return !(a == b);
        }
        /// <summary>
        /// To string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name.ToString();
        }

        public bool Equals(ISigned x, ISigned y)
        {
            return x.Signature.Equals(y.Signature);
        }

        public int GetHashCode(ISigned obj)
        {
            return obj.Signature.GetHashCode();
        }
    }

    /// <summary>
    /// defines a rule
    /// </summary>
    public abstract class Rule : XPTree, IRule
    {
        private readonly string _id; // unique ID of the rule
        private otRuleState _state; // state of the rule
        private String _handle; // handle of the rule theCode in the engine

        // constants
        public const string ConstPropertyState = "State";
        public const string ConstPropertyHandle = "Handle";
        public const string ConstPropertyID = "ID";
        /// <summary>
        /// constructor
        /// </summary>
        /// <param id="handle"></param>
        public Rule(string id = null, Engine engine = null, otXPTNodeType? nodetype = null)
            : base(nodetype: (nodetype.HasValue) ? nodetype.Value : otXPTNodeType.Rule, engine: engine)
        {
            if (id == null) { _id = Guid.NewGuid().ToString(); }
            else { _id = id.ToUpper(); }
            _state = otRuleState.Created;
            _engine = engine;
            _handle = Guid.NewGuid().ToString();
        }
        /// <summary>
        /// sets or gets the handle of the rule
        /// </summary>
        public string Id { get { return _id; } }
        /// <summary>
        /// returns the theCode handle
        /// </summary>
        public string Handle { get { return _handle; } set { _handle = value; RaiseOnPropertyChanged(this, ConstPropertyHandle); } }
        /// <summary>
        /// returns the state of the rule
        /// </summary>
        public otRuleState RuleState { get { return _state; } set { _state = value; RaiseOnPropertyChanged(this, ConstPropertyState); } }
        /// <summary>
        /// gets the Signature
        /// </summary>
        public ISignature Signature
        {
            get
            {
                return new TypeSignature(Id);
            }
        }

        /// <summary>
        /// set the state of the rule
        /// </summary>
        /// <param name="newState"></param>
        protected void SetState(otRuleState newState) { _state = newState; RaiseOnPropertyChanged(this, ConstPropertyState); }
        /// <summary>
        /// compares a signed object
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(ISigned x, ISigned y)
        {
            return x.Signature.Equals(y.Signature);
        }
        /// <summary>
        /// returns the hashcode of a signed object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(ISigned obj)
        {
            return obj.Signature.GetHashCode();
        }
    }

    /// <summary>
    /// defines a data object in a IeXPressionTree object
    /// </summary>
    public class Variable : Node, ISymbol, ISigned
    {
        private readonly string _id;
        private readonly IDataType _datatype;
        private readonly IScope _scope;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="dataTypeId"></param>
        /// <param name="scope"></param>
        public Variable(string id, otDataType dataTypeId, IScope scope)
            : base(nodetype: otXPTNodeType.Variable, engine: null)
        {
            _id = id;
            _datatype = Core.DataType.GetDataType(dataTypeId);
            _scope = scope;
        }
        public Variable(string id, IDataType datatype, IScope scope)
            : base(nodetype: otXPTNodeType.Variable, engine: null)
        {
            _id = id;
            _datatype = datatype;
            _scope = scope;
        }
        public Variable(Parameter parameter, IScope scope)
            : base(nodetype: otXPTNodeType.Variable, engine: null)
        {
            _id = parameter.Id;
            _datatype = parameter.DataType;
            _scope = scope;
        }
        /// <summary>
        /// gets or sets the ID
        /// </summary>
        public string Id
        {
            get { return _id; }
        }
        /// <summary>
        /// gets or sets the Type of the variable
        /// </summary>
        public otDataType ReturnTypeId
        {
            get { return _datatype.TypeId; }
        }
        /// <summary>
        /// sets or gets the datatype
        /// </summary>
        public IDataType ReturnType
        {
            get { return _datatype; }
        }
        /// <summary>
        /// sets or gets the Scope
        /// </summary>
        public IScope Scope
        {
            get { return _scope; }
        }
        /// <summary>
        /// returns true if node is a leaf
        /// </summary>
        public override bool HasSubNodes { get { return false; } }
        /// <summary>
        /// gets true if the symbol is valid in the engine
        /// </summary>
        public bool? IsValid
        {
            get
            {
                if (Scope != null)
                {
                    if (Scope is StatementBlock) return ((StatementBlock)Scope).HasVariable(this.Id);
                    if (Scope is SelectionRule) return ((SelectionRule)Scope).HasParameter(this.Id);
                }
                return null;

            }
        }
        /// <summary>
        /// gets the signature of this variable
        /// </summary>
        public ISignature Signature
        {
            get
            {
                return new TypedNameSignature(new ObjectName(moduleid: this.ScopeId, objectid: Id), this.ReturnType);
            }
        }
        /// <summary>
        /// gets a Parameter out of this Variable
        /// </summary>
        /// <returns></returns>
        public virtual Parameter ToParameter()
        {
            return new Parameter(id: this.Id, datatype: this.ReturnType);
        }
        /// <summary>
        /// to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Signature.ToString();
        }

        public bool Equals(ISigned x, ISigned y)
        {
            return x.Signature.Equals(y.Signature);
        }

        public int GetHashCode(ISigned obj)
        {
            return obj.GetHashCode();
        }
    }
    /// <summary>
    /// defines a data object symbol in a IeXPressionTree object
    /// </summary>
    public class DataObjectSymbol : Node, ISymbol
    {
        private IScope _scope;
        private IObjectDefinition _objectdefinition;
        private readonly ObjectName _name;
        private bool? _isChecked = false;
        // constants
        public const string ConstPropertyTypeId = "TypeId";
        public const string ConstPropertyDataType = "DataType";
        public const string ConstPropertyScope = "Scope";
        /// <summary>
        /// constructor in 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="Type"></param>
        /// <param name="scope"></param>
        public DataObjectSymbol(string id, Engine engine = null)
            : base(nodetype: otXPTNodeType.DataObjectSymbol, engine: engine)
        {
            _engine = engine; // first
            _name = new ObjectName(id);
            _scope = null;
        }
        public DataObjectSymbol(ObjectName name, Engine engine = null)
            : base(nodetype: otXPTNodeType.DataObjectSymbol, engine: engine)
        {
            _engine = engine; // first
            _name = name;
            _scope = null;
        }
        /// <summary>
        /// gets or sets the ID
        /// </summary>
        public ObjectName Name
        {
            get { return _name; }
        }
        /// <summary>
        /// returns the Id of the Symbol
        /// </summary>
        public string Id
        {
            get { return _name.FullId; }
        }
        /// <summary>
        /// returns the ObjectID of the entry
        /// </summary>
        public String ObjectID { get { return _objectdefinition.Id; } }
        /// <summary>
        /// returns the IObjectDefinition
        /// </summary>
        public IObjectDefinition ObjectDefinition
        {
            get
            {
                if (_objectdefinition != null) return _objectdefinition;
                if (_engine != null)
                { CheckValidity(); return _objectdefinition; }
                return null;
            }
        }
        /// <summary>
        /// returns the scope
        /// </summary>
        public IScope Scope
        {
            get { return _scope; }
            set {
                _scope = value;
                RaiseOnPropertyChanged(this, ConstPropertyScope);
            }
        }
        /// <summary>
        /// gets the typeid
        /// </summary>
        public Core.otDataType ReturnTypeId
        {
            get { return otDataType.DataObject; }
        }
        /// <summary>
        /// gets the Datatype
        /// </summary>
        public Core.IDataType ReturnType
        {
            get { return DataObjectType.GetDataType(id: Name.FullId, engine: this.Engine); }
        }
        /// <summary>
        /// returns true if node is a leaf
        /// </summary>
        public override bool HasSubNodes { get { return false; } }
        /// <summary>
        /// gets true if the symbol is valid in the engine
        /// </summary>
        public bool? IsValid
        {
            get
            {
                if (_isChecked.HasValue) return _isChecked.Value;
                return null;
            }
        }

        public ISignature Signature
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// check if the ID exists - returns true or false and if !HasValue then not checkable
        /// </summary>
        /// <returns></returns>
        public bool? CheckValidity()
        {
            if (_isChecked.HasValue) return _isChecked;
            if (this.Engine == null) return _isChecked.HasValue;


            if (Engine.Has<IObjectDefinition>(Name))
            {
                _objectdefinition = Engine.Get<IObjectDefinition>(Name).FirstOrDefault();
                if (_objectdefinition == null)
                    throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { Name.Id, Name.ModuleId });
            }
            else
            { throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { Name.ModuleId, "data object repository" }); }


            _isChecked = true;
            return _isChecked;
        }
        /// <summary>
        /// to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(format: "<{0}:{1}>", arg0: NodeType.ToString(), arg1: this.Name.FullId.ToUpper());
        }
        /// <summary>
        /// return a parameter
        /// </summary>
        /// <returns></returns>
        public Parameter ToParameter()
        {
            if (this.Engine == null)
                return new Parameter(id: this.Name.FullId, datatype: new DataObjectType(this.Name.FullId));
            else return new Parameter(id: this.Name.FullId, datatype: DataObjectType.GetDataType(id: this.Name.FullId, engine: Rules.Engine));
        }

        public bool Equals(ISigned x, ISigned y)
        {
            return x.Signature.Equals(y.Signature);
        }

        public int GetHashCode(ISigned obj)
        {
            return obj.Signature.GetHashCode();
        }
    }
    /// <summary>
    /// defines a local variable in a IeXPressionTree object
    /// </summary>
    public class DataObjectEntrySymbol : Node, ISymbol
    {
        private IScope _scope;
        private IObjectEntryDefinition _entrydefinition;
        private readonly EntryName _name = null;
        private bool? _isChecked = false;
        // constants
        public const string ConstPropertyID = "ID";
        public const string ConstPropertyTypeId = "TypeId";
        public const string ConstPropertyDataType = "DataType";
        public const string ConstPropertyScope = "Scope";
        /// <summary>
        /// constructor in 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="Type"></param>
        /// <param name="scope"></param>
        public DataObjectEntrySymbol(string id, Engine engine = null) : base(nodetype: otXPTNodeType.DataObjectSymbol, engine: engine)
        {

            _scope = null;
            _engine = engine;
            _name = new EntryName(id);
        }
        public DataObjectEntrySymbol(EntryName entryname, Engine engine = null) : base(nodetype: otXPTNodeType.DataObjectSymbol)
        {
            // default engine
            _engine = engine;
            _name = entryname;
            _scope = null;
        }

        /// <summary>
        /// gets or sets the ID
        /// </summary>
        public EntryName Name
        {
            get { return _name; }
        }
        /// <summary>
        /// returns the Id of the Symbol
        /// </summary>
        public string Id
        {
            get { return _name.FullId; }
        }
        /// <summary>
        /// returns the IObjectDefinition
        /// </summary>
        public IObjectDefinition ObjectDefinition { get { CheckValidity(); return _entrydefinition.ObjectDefinition; } }
        /// <summary>
        /// returns the IObjectEntryDefinition
        /// </summary>
        public IObjectEntryDefinition ObjectEntryDefinition { get { CheckValidity(); return _entrydefinition; } }
        /// <summary>
        /// returns the ObjectID of the entry
        /// </summary>
        public String ObjectID { get { CheckValidity(); return _entrydefinition.ObjectId; } }
        /// <summary>
        /// returns the ObjectID of the entry
        /// </summary>
        public String Entryname { get { CheckValidity(); return _entrydefinition.EntryId; } }
        /// <summary>
        /// gets or sets the Type of the variable
        /// </summary>
        public otDataType ReturnTypeId { get { CheckValidity(); return _entrydefinition.TypeId; } set { throw new NotImplementedException(); } }
        /// <summary>
        /// gets the Datatype
        /// </summary>
        public Core.IDataType ReturnType { get { CheckValidity(); return _entrydefinition.DataType; } set { throw new InvalidOperationException(); } }
        /// <summary>
        /// returns the scope
        /// </summary>
        public IScope Scope { get { return _scope; } set { _scope = value; RaiseOnPropertyChanged(this, ConstPropertyScope); } }
        /// <summary>
        /// returns true if node is a leaf
        /// </summary>
        public override bool HasSubNodes { get { return false; } }
        /// <summary>
        /// gets true if the symbol is valid in the engine
        /// </summary>
        public bool? IsValid
        {
            get
            {
                if (_isChecked.HasValue) return _isChecked.Value;
                return null;

            }
        }

        public ISignature Signature
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// check if the ID exists - returns true or false and if !HasValue then not checkable
        /// </summary>
        /// <returns></returns>
        public bool? CheckValidity()
        {
            if (_isChecked.HasValue) return _isChecked;
            if (this.Engine == null) return _isChecked.HasValue;

           
                if (Engine.Has<IObjectDefinition>(Name.ObjectName))
                {
                    Core.IObjectDefinition aDefinition = Engine.Get<IObjectDefinition>(Name.ObjectName).FirstOrDefault();
                    _entrydefinition = aDefinition.GetiEntryDefinition(Name.Id);
                    if (_entrydefinition == null)
                    {
                        throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[]
                        {
                           Name.ObjectId, Name.Id
                        });
                    }

                }
                else
                { throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { Name.FullId, "data object repository" }); }

           
            _isChecked = true;
            return _isChecked;
        }
        /// <summary>
        /// to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(format: "<{0}:{1}>", arg0: NodeType.ToString(), arg1: this.Name);
        }
        /// <summary>
        /// return a parameter
        /// </summary>
        /// <returns></returns>
        public Parameter ToParameter()
        {
            return new Parameter(id: this.Name.FullId, datatype: this.ReturnType);
        }

        public bool Equals(ISigned x, ISigned y)
        {
            return x.Signature.Equals(y.Signature);
        }

        public int GetHashCode(ISigned obj)
        {
            return obj.Signature.GetHashCode();
        }
    }
    /// <summary>
    /// if then else statement
    /// </summary>
    public class IfThenElse : XPTree, IStatement
    {
        /// <summary>
        /// constructor
        /// </summary>
        public IfThenElse(Engine engine = null)
            : base(nodetype: otXPTNodeType.IfThenElse, engine: engine)
        {
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        /// <param name="arguments"></param>
        public IfThenElse(LogicalExpression expression, IStatement @do, IStatement @else = null, Engine engine = null)
            : base(nodetype: otXPTNodeType.IfThenElse, engine: engine)
        {
            // TODO: check the argumetns
            if (@else != null)
                this.Nodes = new ObservableCollection<INode>(new INode[] { expression, @do, @else });
            else this.Nodes = new ObservableCollection<INode>(new INode[] { expression, @do });
        }

        #region "Properties"
        /// <summary>
        /// gets or sets the logical compare expression
        /// </summary>
        public LogicalExpression @LogicalExpression { get { return (LogicalExpression)this.Nodes[0]; } set { this.Nodes[0] = value; } }
        /// <summary>
        /// gets or sets the do 
        /// </summary>
        public IStatement @Do { get { return (IStatement)this.Nodes[1]; } set { this.Nodes[1] = value; } }
        /// <summary>
        /// gets or sets the do 
        /// </summary>
        public IStatement @Else { get { return (IStatement)this.Nodes[2]; } set { this.Nodes[2] = value; } }
        #endregion
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param id="sender"></param>
        /// <param id="e"></param>
        protected override void XPTree_Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.XPTree_Nodes_CollectionChanged(sender, e);
            // check the nodes which are added
            if (!Nodes[0].GetType().IsAssignableFrom(typeof(LogicalExpression)))
                throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { Nodes[0].NodeType.ToString(), otXPTNodeType.LogicalExpression.ToString() });
            if (Nodes[1] != null && !Nodes[1].GetType().GetInterfaces().Contains(typeof(IStatement)))
                throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { Nodes[1].NodeType.ToString(), otXPTNodeType.StatementBlock.ToString() });
            if (Nodes[2] != null && !Nodes[2].GetType().GetInterfaces().Contains(typeof(IStatement)))
                throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { Nodes[2].NodeType.ToString(), otXPTNodeType.StatementBlock.ToString() });
        }

    }
    /// <summary>
    /// 'return' control to caller and return a value
    /// </summary>
    public class @Return : XPTree, IStatement
    {
        /// <summary>
        /// constructor
        /// </summary>
        public @Return(Engine engine = null)
            : base(nodetype: otXPTNodeType.Return, engine: engine)
        {
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        /// <param name="arguments"></param>
        public @Return(IExpression @return, Engine engine = null)
            : base(nodetype: otXPTNodeType.Return, engine: engine)
        {
            Nodes.Add(@return);
        }
        /// <summary>
        /// gets or sets the return Expression
        /// </summary>
        public IExpression Expression { get { return (IExpression)Nodes[0]; } set { Nodes[0] = value; } }
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void XPTree_Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.XPTree_Nodes_CollectionChanged(sender, e);
            // check the nodes which are added
            if (!Nodes[0].GetType().GetInterfaces().Contains(typeof(IExpression)))
                throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { Nodes[0].NodeType.ToString(), "Expression" });

        }
        /// <summary>
        /// toString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string aString = "{" + this.NodeType.ToString() + " ";

            if (Nodes.Count() > 0 && Nodes.First().GetType().GetInterfaces().Contains(typeof(IExpression)))
                aString += ((IExpression)Nodes.First()).ReturnType.ToString() + " ";

            foreach (INode aNode in Nodes)
            {
                aString += aNode.ToString();
            }
            aString += "}";
            return aString;
        }

    }
    /// <summary>
    /// statement block
    /// </summary>
    public class StatementBlock : XPTree, IStatement
    {
        private readonly Dictionary<string, ISymbol> _variables = new Dictionary<string, ISymbol>(); // variables
        private readonly string _id;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        /// <param name="arguments"></param>
        public StatementBlock(INode[] arguments = null, string id = null, Engine engine = null, otXPTNodeType? nodetype = null)
            : base(nodetype: (nodetype.HasValue) ? nodetype.Value : otXPTNodeType.StatementBlock, engine: engine)
        {
            if (id == null) _id = "BLOCK-" + Guid.NewGuid().ToString();
            if (arguments != null) this.Nodes = new ObservableCollection<INode>(arguments.ToList());
        }
        #region "Properties"
        /// <summary>
        /// gets the list of parameters
        /// </summary>
        public IEnumerable<ISymbol> Variables { get { return _variables.Values.ToList(); } }
        /// <summary>
        /// sets the ID of the block
        /// </summary>
        public string Id { get { return _id; } }
        #endregion
        /// <summary>
        /// Add a node to the block
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool Add(IStatement node)
        {
            this.Nodes.Add(node);
            return true;
        }
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void XPTree_Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.XPTree_Nodes_CollectionChanged(sender, e);
            // check the nodes which are added
            foreach (INode aNode in Nodes)
            {
                if (aNode != null && !aNode.GetType().GetInterfaces().Contains(typeof(IStatement)))
                    throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { aNode.NodeType.ToString(), "Statement" });
            }
        }
        /// <summary>
        /// returns true if the parameter is already defined
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasVariable(string id)
        { return _variables.ContainsKey(id); }
        /// <summary>
        /// gets the parameter by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ISymbol GetVariable(string id)
        { return _variables[id]; }
        /// <summary>
        /// Adds a Parameter to the Selection Rule
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public ISymbol AddNewVariable(string id, otDataType typeId)
        {
            if (_variables.ContainsKey(id))
            {
                throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { id, this.Id });
            }
            var aVar = new Variable(id: id, dataTypeId: typeId, scope: this.Scope);
            _variables.Add(aVar.Id, aVar);
            // add the symbol
            if (this.Scope != null) this.Scope.Add(aVar);
            return aVar;
        }
        public ISymbol AddNewVariable(string id, IDataType datatype)
        {
            if (_variables.ContainsKey(id))
            {
                throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { id, this.Id });
            }
            Variable aVar = new Variable(id: id, datatype: datatype, scope: this.Scope);
            _variables.Add(aVar.Id, aVar);
            // add the symbol
            if (this.Scope != null) this.Scope.Add(aVar);
            return aVar;
        }
    }
    /// <summary>
    /// selection statement block
    /// </summary>
    public class SelectionStatementBlock : StatementBlock, IStatement, IExpression
    {
        /// <summary>
        /// result list
        /// </summary>
        private ResultList _result;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        /// <param name="arguments"></param>
        public SelectionStatementBlock(INode[] arguments = null, string id = null, Engine engine = null)
            : base(nodetype: otXPTNodeType.SelectionStatementBlock, id: id, engine: engine)
        {
            // arguments will be checked in event
            if (arguments != null) this.Nodes = new ObservableCollection<INode>(arguments.ToList());
        }
        #region "Properties"
        /// <summary>
        /// gets or sets the type id of the variable
        /// </summary>
        /// <value></value>
        public otDataType ReturnTypeId
        {
            get
            {
                if (_result != null) return _result.TypeId;
                return otDataType.Null;
            }
        }

        /// <summary>
        /// gets or sets the type
        /// </summary>
        /// <value></value>
        public IDataType ReturnType
        {
            get
            {
                if (_result != null) return _result.DataType;
                return null;
            }
        }

        /// <summary>
        /// gets or sets the result (which is a ResultList)
        /// </summary>
        public ResultList Result
        {
            get
            {

                return _result;
            }

        }

        #endregion
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void XPTree_Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // call the checks in the base class
            base.XPTree_Nodes_CollectionChanged(sender, e);
            // check the nodes which are added
            foreach (INode item in e.NewItems)
            {
                // check the nodes which are added
                foreach (INode aNode in Nodes)
                {
                    // check if return is added -> check if the resultlist is the same is in the Property
                    if (aNode != null && aNode is @Return)
                        // return expression must be a selectionExpression (or to-do a variable)
                        if ((((@Return)aNode).Expression) is SelectionExpression) _result = ((SelectionExpression)((@Return)aNode).Expression).Results;
                        else throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { aNode.NodeType.ToString(), "SelectionExpression" });
                    else
                    // only statements are allowed
                    if (aNode != null && !aNode.GetType().GetInterfaces().Contains(typeof(IStatement)))
                        throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { aNode.NodeType.ToString(), "Statement" });
                }
            }
        }
        /// <summary>
        /// returns a List of object names retrieved with this rule
        /// </summary>
        /// <returns></returns>
        public IList<String> ResultingObjectnames()
        {
            if (_result != null) return _result.DataObjectNames();
            return new List<String>();
        }
        /// <summary>
        /// toString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            bool comma = false;
            string aString = "{(" + NodeType.ToString() + ") ";
            if (this.Result != null)
                aString += this.Result.DataType.ToString();
            aString += "[";
            foreach (ISymbol aSymbol in Variables)
            {
                if (comma) aString += ",";
                aString += aSymbol.ToString();
                comma = true;
            }
            comma = false;
            aString += "]{";
            foreach (INode aNode in Nodes)
            {
                if (comma) aString += ",";
                aString += aNode.ToString();
                comma = true;
            }
            aString += "}}";
            return aString;
        }
    }
    /// <summary>
    /// module definition node
    /// </summary>
    public class Module : XPTree, IModule
    {
        private readonly CanonicalName _name;
        private readonly ulong _version;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name"></param>
        public Module(CanonicalName name) : base(nodetype: otXPTNodeType.Unit)
        {
            _name = name;
        }

        #region Properties
        /// <summary>
        /// gets the ID of the module
        /// </summary>
        public string Id { get { return _name.FullId; } }
        /// <summary>
        /// gets the version of the module
        /// </summary>
        public ulong Version { get { return _version; } }
        /// <summary>
        /// gets the canonical name of the module
        /// </summary>
        public CanonicalName Name { get { return _name; } }
        /// <summary>
        /// return signature
        /// </summary>
        public ISignature Signature
        {
            get
            {
                return new TypeSignature(id: this.Id);
            }
        }

        public bool Equals(ISigned x, ISigned y)
        {
            return x.Signature.Equals(y.Signature);
        }

        public int GetHashCode(ISigned obj)
        {
            return obj.Signature.GetHashCode();
        }
        #endregion
    }
    /// <summary>
    /// defines the function
    /// </summary>
    public class Function : XPTree, IComparable<Function>, ISigned, IFunctionDefinition
    {
        /// <summary>
        /// inner variables
        /// </summary>
        private readonly ObjectName _name;
        private readonly IDataType _returntype;
        private readonly ParameterList _parameters = new ParameterList();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="arguments"></param>
        /// <param name="priority"></param>
        public Function(ObjectName name, IDataType returnType, ParameterList parameters = null, Engine engine = null)
            : base(nodetype: otXPTNodeType.FunctionDefinition, engine: engine)
        {
            _name = name;
            if (parameters != null) _parameters.AddRange(parameters);
            _returntype = returnType;
        }

        #region "Properties"
        /// <summary>
        /// gets the id
        /// </summary>
        public string Id {  get { return _name.FullId;  } }
        /// <summary>
        /// gets the object name
        /// </summary>
        public ObjectName Name {  get { return _name; } }
        /// <summary>
        /// gets the parameters
        /// </summary>
        public ParameterList Parameters { get { return _parameters; } }
        /// <summary>
        /// gets or sets the return type of the operation
        /// </summary>
        public IDataType ReturnType { get { return _returntype; } }
        /// <summary>
        /// return Signature
        /// </summary>
        public ISignature Signature
        {
            get
            {
                return new NamedListSignature(name: this.Name, datatype: this.ReturnType, parameters: this.Parameters);
            }
        }
        #endregion
        /// <summary>
        /// implementation of comparable
        /// </summary>
        /// <param id="obj"></param>
        /// <returns></returns>
        public int CompareTo(Function obj)
        {
            return this.Signature.CompareTo(obj.Signature);
        }
        /// <summary>
        /// override Hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)this.Signature.GetHashCode();
        }
        /// <summary>
        /// To string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Signature.ToString();
        }
        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (obj == null || !(obj is Function))
                return false;
            else
                return this.CompareTo((Function)obj) == 0;
        }

        public bool Equals(ISigned x, ISigned y)
        {
            return x.Signature.Equals(y.Signature);
        }

        public int GetHashCode(ISigned obj)
        {
            return obj.GetHashCode();
        }
    }

    /// <summary>
    /// function call node
    /// </summary>
    public class FunctionCall: XPTree , IStatement, IExpression
    {
        private readonly IFunctionDefinition _function;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="function"></param>
        /// <param name="arguments"></param>
        public FunctionCall(Function function, INode [] arguments, Engine engine = null) 
            : base(nodetype: otXPTNodeType.FunctionCall, engine: engine)
        {
            // TODO: check the argumetns
            _function = function;
           
        }
        #region Properties
        /// <summary>
        /// gets the Operation
        /// </summary>
        public ObjectName Name { get { return _function.Name; } }
        /// <summary>
        /// gets the Operator definition
        /// </summary>
        public IFunctionDefinition Function { get { return _function; } }
        /// <summary>
        /// gets the Datatype of this Expression
        /// </summary>
        public IDataType ReturnType { get { return _function.ReturnType; }  }
        /// <summary>
        /// gets the typeId of this Expression
        /// </summary>
        public otDataType ReturnTypeId { get { return _function.ReturnType.TypeId; ; }  }
        #endregion
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void XPTree_Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.XPTree_Nodes_CollectionChanged(sender, e);
            // check the nodes which are added
            foreach (INode aNode in Nodes)
            {
                if (aNode != null && !aNode.GetType().GetInterfaces().Contains(typeof(IExpression)))
                    throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { aNode.NodeType.ToString(), "Expression" });
            }
            
        }
        
    }
    /// <summary>
    /// Assignment
    /// </summary>
    public class Assignment : XPTree,  IStatement
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        /// <param name="arguments"></param>
        public Assignment(ISymbol symbol,IExpression expression)
            : base(nodetype: otXPTNodeType.Assignment)
        {
            this.Nodes = new ObservableCollection<INode>(new INode []{symbol, expression});
        }
        #region Properties

        #endregion
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void XPTree_Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.XPTree_Nodes_CollectionChanged(sender, e);
            // check the nodes which are added
            if (Nodes[0].NodeType != otXPTNodeType.Variable && Nodes[0].NodeType != otXPTNodeType.DataObjectSymbol )
                throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { Nodes[0].NodeType.ToString(), otXPTNodeType.Variable.ToString() });
            if (Nodes[1] != null && !Nodes[1].GetType().GetInterfaces().Contains(typeof(IExpression)))
                throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { Nodes[1].NodeType.ToString(), "Expression" });
            
        }
    }
    /// <summary>
    /// Operation Selection
    /// </summary>
    public class OperationExpression: XPTree , IExpression
    {
        protected readonly Token _token; // operation Token
        protected uint? _prio; // overwrite priority of the operator
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="op"></param>
        /// <param name="operand"></param>
        public OperationExpression(otXPTNodeType? nodetype = null, Engine engine = null)
            : base(nodetype: (!nodetype.HasValue) ? otXPTNodeType.OperationExpression : nodetype.Value, engine: engine)
        {
        }
        public OperationExpression(Token token, otXPTNodeType? nodetype = null, Engine engine = null)
            : base(nodetype: (!nodetype.HasValue) ? otXPTNodeType.OperationExpression : nodetype.Value, engine: engine)
        {
            if (token == null)
                throw new RulezException(RulezException.Types.OperatorNotDefined, arguments: new object[] { "null" });
            if (eXPressionTree.Operator.GetOperator(token) == null)
                throw new RulezException(RulezException.Types.OperatorNotDefined, arguments: new object[] { token.ToString() });
            _token = token;
            if (this.Operator.Arguments != 0)
                throw new RulezException(RulezException.Types.OperandsNotEqualOperatorDefinition, arguments: new object[] { token.ToString(), this.Operator.Arguments, 0 });
        }
        public OperationExpression(Token token, INode operand, Engine engine = null, otXPTNodeType? nodetype = null)
            : base(nodetype: (!nodetype.HasValue) ? otXPTNodeType.OperationExpression : nodetype.Value, engine: engine)
        {
            if (token == null)
                throw new RulezException(RulezException.Types.OperatorNotDefined, arguments: new object[] { "null" });
            if (eXPressionTree.Operator.GetOperator(token) == null)
                throw new RulezException(RulezException.Types.OperatorNotDefined, arguments: new object[] { token.ToString() });
            _token = token;
            if (this.Operator.Arguments != 1)
                throw new RulezException(RulezException.Types.OperandsNotEqualOperatorDefinition, arguments: new object[] { token.ToString(), this.Operator.Arguments, 1 });
            if (operand != null)
                this.Nodes.Add(operand);
            else throw new RulezException(RulezException.Types.OperandNull, arguments: new object[] { token.ToString(), "" });
        }
        public OperationExpression(IOperatorDefinition op, INode operand, Engine engine = null, otXPTNodeType? nodetype = null)
            : base(nodetype: (!nodetype.HasValue) ? otXPTNodeType.OperationExpression : nodetype.Value, engine: engine)
        {

            if (op == null)
                throw new RulezException(RulezException.Types.OperatorNotDefined, arguments: new object[] { "(null)" });
            else _token = op.Token;

            if (this.Operator.Arguments != 1)
                throw new RulezException(RulezException.Types.OperandsNotEqualOperatorDefinition, arguments: new object[] { op.Token.ToString(), op.Arguments, 1 });

            if (operand != null) this.Nodes.Add(operand);
            else throw new RulezException(RulezException.Types.OperandNull, arguments: new object[] { op.Token.ToString(), "" });

        }
        /// <summary>
        /// constructor of an expression
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        public OperationExpression(Token token, INode leftoperand, INode rightoperand, Engine engine = null, otXPTNodeType? nodetype = null)
            : base(nodetype: (!nodetype.HasValue) ? otXPTNodeType.OperationExpression : nodetype.Value, engine: engine)
        {
            // default engine
            if (engine == null) engine = OnTrack.Rules.Engine;

            if (eXPressionTree.Operator.GetOperator(token) == null)
                throw new RulezException(RulezException.Types.OperatorNotDefined, arguments: new object[] { token.ToString() });

            _token = token;

            if (this.Operator.Arguments != 2)
                throw new RulezException(RulezException.Types.OperandsNotEqualOperatorDefinition, arguments: new object[] { token.ToString(), this.Operator.Arguments, 2 });

            if (leftoperand != null) this.Nodes.Add(leftoperand);
            else throw new RulezException(RulezException.Types.OperandNull, arguments: new object[] { token.ToString(), "left" });

            if (rightoperand != null) this.Nodes.Add(rightoperand);
            else throw new RulezException(RulezException.Types.OperandNull, arguments: new object[] { token.ToString(), "right" });

        }
        public OperationExpression(IOperatorDefinition op, INode leftoperand, INode rightoperand, Engine engine = null, otXPTNodeType? nodetype = null)
            : base(nodetype: (!nodetype.HasValue) ? otXPTNodeType.OperationExpression : nodetype.Value, engine: engine)
        {
            // default engine
            if (engine == null) engine = OnTrack.Rules.Engine;

            _token = op.Token;

            if (op.Arguments != 2)
                throw new RulezException(RulezException.Types.OperandsNotEqualOperatorDefinition, arguments: new object[] { op.Token.ToString(), op.Arguments, 2 });

            if (leftoperand != null) this.Nodes.Add(leftoperand);
            else throw new RulezException(RulezException.Types.OperandNull, arguments: new object[] { op.Token.ToString(), "left" });

            if (rightoperand != null) this.Nodes.Add(rightoperand);
            else throw new RulezException(RulezException.Types.OperandNull, arguments: new object[] { op.Token.ToString(), "right" });
        }
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void XPTree_Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.XPTree_Nodes_CollectionChanged(sender, e);
            // check the nodes which are added
            foreach (INode aNode in Nodes)
            {
                if (aNode != null && !aNode.GetType().GetInterfaces().Contains(typeof(IExpression)))
                    throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { aNode.NodeType.ToString(), "Expression" });

            }

        }
        #region Properties
        /// <summary>
        /// gets or sets the Operation
        /// </summary>
        public Token TokenID { get { return _token; } }
        /// <summary>
        /// gets the Operator definition
        /// </summary>
        public IOperatorDefinition Operator { get { return eXPressionTree.Operator.GetOperator(_token); } }
        /// <summary>
        /// get or sets the Priority of the Expression's Operator
        /// </summary>
        public uint Priority
        {
            get
            {
                if (_prio.HasValue) return _prio.Value;
                // return Operators Priority
                return Operator.Priority;
            }
            set
            {
                _prio = value;
            }
        }
       
        /// <summary>
        /// returns the left operand
        /// </summary>
        public INode LeftOperand
        {
            get
            {
                return this.Nodes[0];
            }
            set
            {
                if (value != null && ((value.GetType().GetInterfaces().Contains(typeof(INode))
                    || (value.GetType().GetInterfaces().Contains(typeof(IExpression)))
                   )))
                {
                    this.Nodes[0] = value;
                }
                else if (value == null) this.Nodes[1] = null;
                else throw new RulezException(RulezException.Types.InvalidOperandNodeType, arguments: value);
            }
        }
        /// <summary>
        /// returns the right operand
        /// </summary>
        public INode RightOperand
        {
            get
            {
                if ((this.Nodes == null) || (this.Nodes.Count == 0)) return null;
                if (this.Nodes.Count == 1) return this.Nodes[1];
                // create a tree of the rest
                return BuildExpressionTree(1);
            }
            set
            {
                if (value != null && ((value.GetType().GetInterfaces().Contains(typeof(INode))
                    || (value.GetType().GetInterfaces().Contains(typeof(IExpression)))
                   )))
                {
                    this.Nodes[1] = value;
                }
                else if (value == null) this.Nodes[1] = null;
                else throw new RulezException(RulezException.Types.InvalidOperandNodeType, arguments: value);
            }
        }
        /// <summary>
        /// gets the Datatype of this Expression
        /// </summary>
        public IDataType ReturnType { get { throw new NotImplementedException(); } set { throw new InvalidOperationException(); } }
        /// <summary>
        /// gets the typeId of this Expression
        /// </summary>
        public otDataType ReturnTypeId { get { throw new NotImplementedException(); } set { throw new InvalidOperationException(); } }
        /// <summary>
        /// returns true if node is a leaf
        /// </summary>
        public new bool HasSubNodes { get { if (this.Operator.Arguments != 0) return true; return false; } }
        #endregion
        /// <summary>
        /// build and return a recursive LogicalExpression Tree from arguments
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private INode BuildExpressionTree(int i)
        {
            // build right-hand a subtree
            if (this.Nodes.Count > i + 1) return new OperationExpression(this.Operator, this.Nodes[i], BuildExpressionTree(i + 1));
            // return the single node
            return this.Nodes[i];
        }
        /// <summary>
        /// toString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            bool comma = false;
            string aString = "{(" + NodeType.ToString ()+ ") " +  this.Operator.ToString () +":";
            foreach (INode aNode in Nodes)
            {
                if (comma) aString += "," ;
                aString += aNode.ToString();
                comma = true;
            }
            aString += "}";
            return aString;
        }
    }
    /// <summary>
    /// defines an logical expression
    /// </summary>
    public class LogicalExpression : OperationExpression
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="op"></param>
        /// <param name="operand"></param>
        public LogicalExpression(otXPTNodeType? nodetype = null, Engine engine =null)
            : base(nodetype: (! nodetype.HasValue) ? otXPTNodeType.LogicalExpression : nodetype.Value, engine: engine)
        {  }
        public LogicalExpression(Token op, Engine engine = null,otXPTNodeType ? nodetype = null)
            : base(op, engine: engine, nodetype: (!nodetype.HasValue) ? otXPTNodeType.LogicalExpression : nodetype.Value)
        {
            if (this.Operator.Type != otOperatorType.Logical && this.Operator.Type != otOperatorType.Logical )
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected, arguments: new object[] { op.ToString(), "logical" });
        }
        public LogicalExpression(Token op, IExpression operand, Engine engine = null, otXPTNodeType? nodetype = null)
            : base(op, operand, nodetype: (!nodetype.HasValue) ? otXPTNodeType.LogicalExpression : nodetype.Value, engine: engine)
        {
            if (this.Operator.Type != otOperatorType.Logical && this.Operator.Type != otOperatorType.Compare)
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected , arguments: new object[] { op.ToString(), "logical" });
        }
        public LogicalExpression(IOperatorDefinition op, IExpression operand, Engine engine = null, otXPTNodeType? nodetype = null)
            : base(op, operand, engine, nodetype: (!nodetype.HasValue) ? otXPTNodeType.LogicalExpression : nodetype.Value)
        {
            if (this.Operator.Type != otOperatorType.Logical && this.Operator.Type != otOperatorType.Compare)
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected , arguments: new object[] { op.ToString(), "logical" });

        }
        /// <summary>
        /// constructor of an expression
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        public LogicalExpression( Token op, INode leftoperand, INode rightoperand, Engine engine = null, otXPTNodeType? nodetype = null)
            : base( op,  leftoperand, rightoperand, engine:engine, nodetype: (!nodetype.HasValue) ? otXPTNodeType.LogicalExpression : nodetype.Value)
        {
            if (this.Operator.Type != otOperatorType.Logical && this.Operator.Type != otOperatorType.Compare)
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected , arguments: new object[] { op.ToString(), "logical" });
        }
        public LogicalExpression(IOperatorDefinition op, INode leftoperand, INode rightoperand, Engine engine = null, otXPTNodeType? nodetype = null)
            : base(op, leftoperand, rightoperand, engine: engine, nodetype: (!nodetype.HasValue) ? otXPTNodeType.LogicalExpression : nodetype.Value)
        {
            if (this.Operator.Type !=  otOperatorType.Logical  && this.Operator.Type != otOperatorType.Compare )
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected , arguments: new object[] { op.ToString(), "logical" });
        }
        #region "Properties"
        /// <summary>
        /// gets the Datatype of this Expression
        /// </summary>
        public new IDataType DataType { get { return Rulez.PrimitiveType.GetPrimitiveType(otDataType.Bool); } set { throw new InvalidOperationException();} }
        /// <summary>
        /// gets the typeId of this Expression
        /// </summary>
        public new otDataType TypeId { get { return otDataType.Bool; } set { throw new InvalidOperationException();} }
        #endregion

        #region "Helper"
        /// <summary>
        /// returns an LogicalExpression with AND
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
        static public LogicalExpression AND(IExpression leftoperand, IExpression rightoperand)
        {
            return new LogicalExpression(new Token(Token.AND), leftoperand, rightoperand);
        }
        /// <summary>
        /// returns an LogicalExpression with AND
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
        public LogicalExpression AND(IExpression rightoperand)
        {
            return new LogicalExpression(new Token(Token.AND), this, rightoperand);
        }
        /// <summary>
        /// returns an LogicalExpression with ANDALSO
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
         static public LogicalExpression ANDALSO(IExpression leftoperand, IExpression rightoperand)
        {
            return new LogicalExpression(new Token(Token.ANDALSO), leftoperand, rightoperand);
        }
        /// <summary>
        /// returns an LogicalExpression with ANDALSO
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
        public LogicalExpression ANDALSO(IExpression rightoperand)
        {
            return new LogicalExpression(new Token(Token.ANDALSO), this, rightoperand);
        }
         /// <summary>
         /// returns an LogicalExpression with OR
         /// </summary>
         /// <param name="leftoperand"></param>
         /// <param name="rightoperand"></param>
         /// <returns></returns>
         static public LogicalExpression OR(IExpression leftoperand, IExpression rightoperand)
         {
             return new LogicalExpression(new Token(Token.OR), leftoperand, rightoperand);
         }
         /// <summary>
         /// returns an LogicalExpression with OR
         /// </summary>
         /// <param name="leftoperand"></param>
         /// <param name="rightoperand"></param>
         /// <returns></returns>
         public LogicalExpression OR(IExpression rightoperand)
         {
             return new LogicalExpression(new Token(Token.OR), this, rightoperand);
         }
         /// <summary>
         /// returns an LogicalExpression with ORELSE
         /// </summary>
         /// <param name="leftoperand"></param>
         /// <param name="rightoperand"></param>
         /// <returns></returns>
         static public LogicalExpression ORELSE(IExpression leftoperand, IExpression rightoperand)
         {
             return new LogicalExpression(new Token(Token.ORELSE), leftoperand, rightoperand);
         }
         /// <summary>
         /// returns an LogicalExpression with ORELSE
         /// </summary>
         /// <param name="leftoperand"></param>
         /// <param name="rightoperand"></param>
         /// <returns></returns>
         public LogicalExpression ORELSE(IExpression rightoperand)
         {
             return new LogicalExpression(new Token(Token.ORELSE), this, rightoperand);
         }
         /// <summary>
         /// returns an LogicalExpression with NOT
         /// </summary>
         /// <param name="leftoperand"></param>
         /// <param name="rightoperand"></param>
         /// <returns></returns>
         static public LogicalExpression NOT(IExpression operand)
         {
             return new LogicalExpression(new Token(Token.NOT), operand);
         }
         /// <summary>
         /// returns an LogicalExpression with TRUE (always true)
         /// </summary>
         /// <param name="leftoperand"></param>
         /// <param name="rightoperand"></param>
         /// <returns></returns>
         static public LogicalExpression TRUE()
         {
             return new LogicalExpression(new Token(Token.TRUE));
         }
         
#endregion

         /// <summary>
         /// handler for changing the nodes list
         /// </summary>
         /// <param name="sender"></param>
         /// <param name="e"></param>
         protected override void XPTree_Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
         {
             base.XPTree_Nodes_CollectionChanged(sender, e);
             // check the nodes which are added
             foreach (INode aNode in Nodes)
             {
                // IEXPRESSION is already checked in base class
                //
                // if (aNode != null && !aNode.GetType().GetInterfaces().Contains(typeof(IExpression)))
                //     throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { aNode.NodeType.ToString(), "Expression" });
             }

         }
    }
    /// <summary>
    /// defines an logical expression
    /// </summary>
    public class CompareExpression : LogicalExpression
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="op"></param>
        /// <param name="operand"></param>
        public CompareExpression(otXPTNodeType? nodetype = null, Engine engine =null)
            : base(nodetype: (!nodetype.HasValue) ? otXPTNodeType.CompareExpression : nodetype.Value, engine: engine)
        {  }
        /// <summary>
        /// constructor of an expression
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        public CompareExpression(Token op, IExpression leftoperand, IExpression rightoperand, Engine engine = null, otXPTNodeType? nodetype = null)
            : base(op, leftoperand, rightoperand, nodetype: (!nodetype.HasValue) ? otXPTNodeType.CompareExpression : nodetype.Value, engine:engine)
        {
            if (this.Operator.Type != otOperatorType.Logical && this.Operator.Type != otOperatorType.Compare)
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected, arguments: new object[] { op.ToString(), "compare" });
        }
        public CompareExpression(IOperatorDefinition op, IExpression leftoperand, IExpression rightoperand, Engine engine = null, otXPTNodeType? nodetype = null)
            : base(op, leftoperand, rightoperand, nodetype: (!nodetype.HasValue) ? otXPTNodeType.CompareExpression : nodetype.Value, engine: engine)
        {
            if (this.Operator.Type != otOperatorType.Logical && this.Operator.Type != otOperatorType.Compare)
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected, arguments: new object[] { op.ToString(), "compare" });
        }

        #region "Helper"
        /// <summary>
        /// returns an LogicalExpression with EQUAL
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
        public CompareExpression EQ(IExpression rightoperand)
        {
            return new CompareExpression(new Token(Token.EQ), this, rightoperand);
        }
        /// <summary>
        /// returns an LogicalExpression with EQUAL
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
        static public CompareExpression EQ(IExpression leftoperand, IExpression rightoperand)
        {
            return new CompareExpression(new Token(Token.EQ), leftoperand, rightoperand);
        }
        /// <summary>
        /// returns an LogicalExpression with NEQUAL
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
        static public CompareExpression NEQ(IExpression leftoperand, IExpression rightoperand)
        {
            return new CompareExpression(new Token(Token.NEQ), leftoperand, rightoperand);
        }
        /// <summary>
        /// returns an LogicalExpression with EQUAL
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
        public CompareExpression NEQ(IExpression rightoperand)
        {
            return new CompareExpression(new Token(Token.NEQ), this, rightoperand);
        }
        /// <summary>
        /// returns an LogicalExpression with GREATER THAN
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
        static public CompareExpression GT(IExpression leftoperand, IExpression rightoperand)
        {
            return new CompareExpression(new Token(Token.GT), leftoperand, rightoperand);
        }
        /// <summary>
        /// returns an LogicalExpression with GREATER THAN
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
        public CompareExpression GT(IExpression rightoperand)
        {
            return new CompareExpression(new Token(Token.GT), this, rightoperand);
        }
        /// <summary>
        /// returns an LogicalExpression with GREATER EQUAL
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
        static public CompareExpression GE(IExpression leftoperand, IExpression rightoperand)
        {
            return new CompareExpression(new Token(Token.GE), leftoperand, rightoperand);
        }
        /// <summary>
        /// returns an LogicalExpression with GREATER EQUAL
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
        public CompareExpression GE(IExpression rightoperand)
        {
            return new CompareExpression(new Token(Token.GE), this, rightoperand);
        }
        /// <summary>
        /// returns an LogicalExpression with GREATER THAN
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
        static public CompareExpression LT(IExpression leftoperand, IExpression rightoperand)
        {
            return new CompareExpression(new Token(Token.LT), leftoperand, rightoperand);
        }
        /// <summary>
        /// returns an LogicalExpression with GREATER THAN
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
        public CompareExpression LT(IExpression rightoperand)
        {
            return new CompareExpression(new Token(Token.LT), this, rightoperand);
        }
        /// <summary>
        /// returns an LogicalExpression with GREATER EQUAL
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
        static public CompareExpression LE(IExpression leftoperand, IExpression rightoperand)
        {
            return new CompareExpression(new Token(Token.LE), leftoperand, rightoperand);
        }
        /// <summary>
        /// returns an LogicalExpression with GREATER EQUAL
        /// </summary>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        /// <returns></returns>
        public CompareExpression LE(IExpression rightoperand)
        {
            return new CompareExpression(new Token(Token.LE), this, rightoperand);
        }
        #endregion

        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void XPTree_Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.XPTree_Nodes_CollectionChanged(sender, e);
            // check the nodes which are added
            foreach (INode aNode in Nodes)
            {
                // IExpression is already be checked in base class
                //
                // if (aNode != null && !aNode.GetType().GetInterfaces().Contains(typeof(IExpression)))
                //    throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { aNode.NodeType.ToString(), "Expression" });
            }

        }
    }
    /// <summary>
    /// define a list of named results (Symbols) for a selection
    /// also can return the Datatype for this
    /// </summary>
    public class ResultList: XPTree
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="results"></param>
        public ResultList(params INode[] results) 
            : base(nodetype: otXPTNodeType.ResultList)
        {
            foreach (INode aNode in results) this.Nodes.Add(aNode);
        }
        public ResultList(params DataObjectSymbol[] results)
             : base(nodetype: otXPTNodeType.ResultList)
        {
            foreach (DataObjectSymbol aNode in results) this.Nodes.Add(aNode);
        }
        public ResultList(params DataObjectEntrySymbol[] results)
            : base(nodetype: otXPTNodeType.ResultList)
        {
            foreach (DataObjectEntrySymbol aNode in results) this.Nodes.Add(aNode);
        }
        public ResultList(List<INode> results)
             : base(nodetype: otXPTNodeType.ResultList)
        {
            foreach (INode aNode in results) this.Nodes.Add(aNode);
        }

        /// <summary>
        /// adds a ResultNode to the result list
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Boolean  Add(INode node)
        {
            // accept only  data object symbol
             if (node.NodeType == otXPTNodeType.DataObjectSymbol)
                {
                this.Nodes.Add(node);
                    return true;
                }
                throw new RulezException(RulezException.Types.InvalidOperandNodeType, arguments: new object[] { node.NodeType.ToString(), otXPTNodeType.DataObjectSymbol.ToString() });
        }
        /// <summary>
        /// gets or sets the type id of the result list
        /// </summary>
        /// <value></value>
        public otDataType TypeId
        {
            get
            {
                if (this.Nodes == null || this.Nodes.Count() == 0) return otDataType.Null;
                // returns a list of the innertype
                return otDataType.List;
            }
        }

        /// <summary>
        /// gets or sets the type
        /// </summary>
        /// <value></value>
        public IDataType DataType
        {
            get
            {
                if (this.Nodes == null || this.Nodes.Count() == 0) return Core.DataType.GetDataType (otDataType.Null);
                if (this.Nodes.Count() == 1) return ListType.GetDataType(innerDataType: ((IExpression)this.Nodes[0]).ReturnType, engine: this.Engine) ;
                // get a Datatype according to the structure
                var structure = new List<IDataType>();
                var names = new List<string>();
                foreach (ISymbol aResult in Nodes) { names.Add(aResult.Id);  structure.Add(aResult.ReturnType);}
                IDataType innerType = TupleType.GetDataType(structure: structure.ToArray(), memberNames: names.ToArray(), engine: this.Engine);
                return ListType.GetDataType(innerDataType: innerType, engine: this.Engine);
            }
        }
        /// <summary>
        /// return a unique list of used objectnames in the result list
        /// </summary>
        public IList<String> DataObjectNames ()
        {
            var aList = new List<String>();
            foreach (INode aNode in this.Nodes)
            {
                string aName = String.Empty;
                if (aNode is DataObjectEntrySymbol) aName = ((DataObjectEntrySymbol)aNode).Name.FullId;
                if (aNode is DataObjectSymbol) aName = ((DataObjectSymbol)aNode).Name.FullId;

                if ( !String.IsNullOrEmpty (aName) && !aList.Contains(aName)) aList.Add(aName);
            }
            // return list
            return aList;
        }
    }
    /// <summary>
    /// defines a selection rule expression
    /// </summary>
    public class SelectionRule : Rule, ISelectionRule, IScope
    {
        #region Static
        /// <summary>
        /// generate a selection Rule XPT out of a String
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public SelectionRule Generate(string source)
        {
            var aListener = new RulezParser.MessageListener();
            RulezParser.SelectionRulezContext aCtx = null;

            try
            {
                var aLexer = new RulezLexer(new Antlr4.Runtime.AntlrInputStream(source));
                // wrap a token-stream around the lexer
                var theTokens = new Antlr4.Runtime.CommonTokenStream(aLexer);
                // create the aParser
                var aParser = new RulezParser(theTokens);
                aParser.Trace = true;
                aParser.Engine = this.Engine;
                aParser.AddErrorListener(aListener);
                // parse
                aCtx = aParser.selectionRulez();
                // return
                return (SelectionRule) aCtx.XPTreeNode;
            }
            catch (Exception ex)
            {
                if (aCtx != null) return (SelectionRule) aCtx.XPTreeNode;
                return null;
            }
        }

        void Function(object sender, RulezParser.MessageListener.EventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion
        // parameters
        private readonly ParameterList  _parameters = new ParameterList(); 
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="handle"></param>
        public SelectionRule(string id = null,  Engine engine = null)
            : base(id, nodetype: otXPTNodeType.SelectionRule, engine: engine)
        {
        }
        /// <summary>
        /// gets or sets the type id of the variable
        /// </summary>
        /// <value></value>
        public otDataType ReturnTypeId
        {
            get
            {
                if (Selection != null) return Selection.ReturnTypeId;
                return otDataType.Null;
            }
        }

        /// <summary>
        /// gets or sets the type
        /// </summary>
        /// <value></value>
        public IDataType ReturnType
        {
            get
            {
                if (Selection != null) return Selection.ReturnType;
                return null;
            }
        }
        /// <summary>
        /// gets or sets the result (which is a ResultList)
        /// </summary>
        public ResultList Result
        {
            get
            {
                if (this.Nodes.Count > 0)  return ((SelectionStatementBlock)this.Nodes[0]).Result;
                return null;
            }
        }
        /// <summary>
        /// gets or sets the selection expression
        /// </summary>
        public SelectionStatementBlock Selection
        {
            get
            {
                if (this.Nodes.Count > 0)  return (SelectionStatementBlock)this.Nodes.First();
                return null;
            }
            set
            {
                if (value.NodeType == otXPTNodeType.SelectionStatementBlock) this.Nodes.Add(value);
                else throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { value.NodeType.ToString() });
            }
        }
        /// <summary>
        /// gets the list of parameters
        /// </summary>
        public ParameterList Parameters { get { return _parameters; } }

        public ObservableCollection<IScope> SubScopes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IRepository Repository
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IScope Parent
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public CanonicalName Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        IScope IScope.Parent
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// returns a List of object names retrieved with this rule
        /// </summary>
        /// <returns></returns>
        public IList<String> ResultObjectIds()
        {
            if (Selection != null) return Selection.ResultingObjectnames();
            return new List<String>();
        }
        /// <summary>
        /// returns true if the parameter is already defined
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasParameter(string id)
        { return _parameters.Where ( x => String.Compare (x.Id, id, ignoreCase:true)==0).Count() > 0; }
        /// <summary>
        /// gets the parameter by name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Parameter GetParameter(string id)
        { return _parameters.Where(x => String.Compare(x.Id, id, ignoreCase: true) == 0).FirstOrDefault(); }
        /// <summary>
        /// Adds a Parameter to the Selection Rule
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="dataTypeId"></param>
        /// <returns></returns>
        public Parameter AddNewParameter(string id, otDataType dataTypeId)
        {
            if (this.HasParameter(id))
                throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { id, this.Id });
            // create and add Variable
            var aVar = new Parameter(id:id, dataTypeId:dataTypeId);
             _parameters.Add(aVar);
            return aVar;
        }
        /// <summary>
        /// adds a Parameter by dataobject
        /// </summary>
        /// <param name="id"></param>
        /// <param name="datatype"></param>
        /// <returns></returns>
        public Parameter AddNewParameter(string id, IDataType datatype)
        {
            if (this.HasParameter(id))
                throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { id, this.Id });
            var aVar = new Parameter(id: id, datatype: datatype);
            // create and add Variable
            _parameters.Add(aVar);
            return aVar;
        }
        /// <summary>
        /// toString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string aString = "{(" + NodeType.ToString() + ") " + this.Id + "[";
            aString += _parameters.Signature.ToString();
            aString += "]";
            if (this.Result != null) aString += Result.ToString();
            aString += "{";
            if (Selection != null) aString += Selection.ToString();
            aString += "}}";
            return aString;
        }

        public bool HasSubScope(string id)
        {
            throw new NotImplementedException();
        }

        public bool HasScope(CanonicalName name)
        {
            throw new NotImplementedException();
        }

        public bool HasScope(string id)
        {
            throw new NotImplementedException();
        }

        public IScope GetSubScope(string id)
        {
            throw new NotImplementedException();
        }

        public IScope GetScope(CanonicalName name)
        {
            throw new NotImplementedException();
        }

        public IScope GetScope(string id)
        {
            throw new NotImplementedException();
        }

        public IScope AddSubScope(string id)
        {
            throw new NotImplementedException();
        }

        public bool AddScope(IScope scope)
        {
            throw new NotImplementedException();
        }

        public bool AddScope(string id)
        {
            throw new NotImplementedException();
        }

        public bool AddScope(CanonicalName name)
        {
            throw new NotImplementedException();
        }

        public IScope GetRoot()
        {
            throw new NotImplementedException();
        }

        public IScope NewScope(string id)
        {
            throw new NotImplementedException();
        }

        public IScope NewScope(CanonicalName name)
        {
            throw new NotImplementedException();
        }

        public void Scope_DataObjectRepositoryAdded(object sender, Engine.EventArgs e)
        {
            throw new NotImplementedException();
        }

        public bool RegisterDataObjectRepository(IDataObjectRepository dataObjectRepository)
        {
            throw new NotImplementedException();
        }

        public bool DeRegisterDataObjectRepository(IDataObjectRepository dataObjectRepository)
        {
            throw new NotImplementedException();
        }

        public bool Add(ISigned signed)
        {
            throw new NotImplementedException();
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
    /// defines a selection rule expression
    /// </summary>
    public class SelectionExpression : eXPressionTree.XPTree , IExpression
    {
        /// <summary>
        /// result list
        /// </summary>
        private readonly ResultList _result;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="handle"></param>
        public SelectionExpression(ResultList result=null, Engine engine = null)
            : base(nodetype: otXPTNodeType.SelectionExpression, engine:engine)
        {
            _result = result;
        }
        /// <summary>
        /// gets or sets the type id of the variable
        /// </summary>
        /// <value></value>
        public otDataType ReturnTypeId
        {
            get
            {
                return Results.TypeId;
            }
        }
        /// <summary>
        /// gets or sets the type
        /// </summary>
        /// <value></value>
        public IDataType ReturnType  {  get  {  return Results.DataType;   }  }
        /// <summary>
        /// gets or sets the result (which is a ResultList)
        /// </summary>
        public ResultList Results {   get { return _result; }    }
        /// <summary>
        /// returns a List of object names retrieved with this rule
        /// </summary>
        /// <returns></returns>
        public IList<String> ResultingObjectnames()
        {
            if (_result != null) return _result.DataObjectNames();
            return new List<String>();
        }
        /// <summary>
        /// toString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            bool comma = false;
            string aString = "{(" + NodeType.ToString ()+ ") " +  this.Results.ToString () +":";
            foreach (INode aNode in Nodes)
            {
                if (comma) aString += "," ;
                if (aNode != null)
                {
                    aString += aNode.ToString();
                    comma = true;
                }
            }
            aString += "}";
            return aString;
        }
    }
    /// <summary>
    /// mutual top level tree object
    /// </summary>
    public class Unit : XPTree
    {
        private readonly string _id = Guid.NewGuid().ToString();
        /// <summary>
        /// constructor
        /// </summary>
        public Unit(string id = null, Engine engine = null)
            : base(nodetype: otXPTNodeType.Unit,engine:engine)
        {
            if (!String.IsNullOrEmpty(id)) _id = id;
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        /// <param name="arguments"></param>
        public Unit(string id, INode[] arguments, Engine engine = null)
             : base(nodetype: otXPTNodeType.Unit, engine: engine)
        {
            if (!String.IsNullOrEmpty(id)) _id = id;
            this.Nodes = new ObservableCollection<INode>(arguments.ToList());
        }
        #region Properties
        /// <summary>
        /// sets the ID of the block
        /// </summary>
        public string ID { get { return _id; }  }
        #endregion
        /// <summary>
        /// Add a node 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool Add(IXPTree node)
        {
            this.Nodes.Add(node);
            return true;
        }
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void XPTree_Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.XPTree_Nodes_CollectionChanged(sender, e);
            // check the nodes which are added
            foreach (INode aNode in Nodes)
            {
                if (aNode != null && ! (aNode is SelectionRule ))
                    throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { aNode.NodeType.ToString(), "SelectionRule" });
            }
        }
        
    }
}