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
        protected otXPTNodeType _nodeType;
        protected IXPTree _parent;
        protected List<Rulez.Message> _errorlist = new List<Message>();
        // event
        protected event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="engine"></param>
        protected Node(Engine engine=null)
        {
            // default engine
            this.Engine = engine;
        }
        /// <summary>
        /// gets the node type
        /// </summary>
        public otXPTNodeType NodeType { get { return _nodeType; } protected set { _nodeType = value; } }
        /// <summary>
        /// returns 
        /// </summary>
        public abstract bool HasSubNodes { get; }
        /// <summary>
        /// gets the Parent of the Node
        /// </summary>
        public IXPTree Parent { get { return _parent; } set { _parent = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Parent")); } }
        /// <summary>
        /// returns the engine
        /// </summary>
        public Engine Engine { get { return _engine; } set { _engine = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Engine")); } }
        /// <summary>
        /// returns the Errors of the Node
        /// </summary>
        public IList<Rulez.Message> Messages { get { return _errorlist; } }
        /// <summary>
        /// accept the visitor
        /// </summary>
        /// <param name="visitor"></param>
        public bool Accept(IVisitor visitor) { visitor.Visit(this); return true; }
        /// <summary>
        /// returns an IEnumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            List<INode> aList = new List<INode>();
            aList.Add(this);
            return aList.GetEnumerator ();
        }
        public IEnumerator<INode> GetEnumerator()
        {
            List<INode> aList = new List<INode>();
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
            return "<" + NodeType.ToString()+">";
        }
     }

    /// <summary>
    /// declare a constant node in an AST
    /// </summary>
    public class Literal: Node, IExpression
    {
        private object _value;
        private bool _hasValue = false;
        private IDataType _datatype;

        /// <summary>
        /// constructor
        /// </summary>
        public Literal(object value = null, otDataType? typeId = null): base()
        {
            this.NodeType = otXPTNodeType.Literal;
            if (typeId != null && typeId.HasValue ) _datatype = Core.DataType.GetDataType(typeId.Value);
            if (value != null) this.Value = value;

            if ((typeId == null) && (value != null))
            {
                throw new NotImplementedException("data type determination by value");
            }
        }
        public Literal(object value, IDataType datatype)
            : base()
        {
            this.NodeType = otXPTNodeType.Literal;
            _datatype = datatype;
            if (value != null) this.Value = value;
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
        public object Value { get { return _value; } set { _value = Core.DataType.To(value, _datatype ); _hasValue = true; RaiseOnPropertyChanged(this, "Value"); } }
        /// <summary>
        /// returns the datatype of the literal
        /// </summary>
        public otDataType TypeId { get { return _datatype.TypeId; } set { this.DataType = Core.DataType.GetDataType(value); RaiseOnPropertyChanged(this, "TypeId"); } }
        /// <summary>
        /// returns the datatype of the literal
        /// </summary>
        public IDataType DataType { get { return _datatype; } set { _datatype = value; RaiseOnPropertyChanged(this, "DataType"); if (this.HasValue) this.Value = Core.DataType.To(value, DataType); } }
        /// <summary>
        /// gets or sets the type of the literal
        /// </summary>
        public System.Type NativeType
        {
            get
            {
                if (this.HasValue) if (this.Value != null) return this.Value.GetType();
                else return this.DataType.NativeType;
                else return null;
            }
        }
        /// <summary>
        /// to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("<{0}:{1}>", (this.DataType != null) ? this.DataType.ToString(): NodeType.ToString(),  this.Value);
        }
    }

    /// <summary>
    /// Base class for all tree nodes
    /// </summary>
    public abstract class XPTree : IXPTree
    {
       

        // instance variables
        private ObservableCollection<INode> _nodes = new ObservableCollection<INode>();
        protected Engine _engine;
        private otXPTNodeType _nodeType;
        private IXPTree _parent;
        private List<Message> _errorlist = new List<Message>();

        // event
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="engine"></param>
        protected XPTree(Engine engine = null)
        {
            // default engine
            if (engine == null) engine = OnTrack.Rules.Engine;
            this.Nodes.CollectionChanged += _Nodes_CollectionChanged;
            this.PropertyChanged += XPTree_PropertyChanged;
        }
        /// <summary>
        /// PropertyChanged Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void  XPTree_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // set the engine property also to the nodes
            if (e.PropertyName == "Engine")
            {
                foreach (INode aNode in Nodes) if (aNode != null) aNode.Engine = this.Engine;
            }
        }
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void _Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // set the parent
            if (e.Action == NotifyCollectionChangedAction.Add)
                foreach (INode aNode in e.NewItems) { if (aNode != null) { aNode.Parent = this; aNode.Engine = this.Engine; } }
        }
        /// <summary>
        /// return the node type
        /// </summary>
        public otXPTNodeType NodeType { get { return _nodeType; } protected set { _nodeType = value; } }
        /// <summary>
        /// return all the leaves
        /// </summary>
        public ObservableCollection<INode> Nodes { get { return _nodes; } set { _nodes = value; _nodes.CollectionChanged += _Nodes_CollectionChanged; } }
        /// <summary>
        /// returns true if node is a leaf
        /// </summary>
        public bool HasSubNodes { get { return true; } }
        /// <summary>
        /// gets the Parent of the Node
        /// </summary>
        public IXPTree Parent { get { return _parent; } set { _parent = value; RaiseOnPropertyChanged(this,"Parent"); } }
        /// <summary>
        /// returns the Errors of the Node
        /// </summary>
        public IList<Rulez.Message> Messages { get { return _errorlist; } }
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
                RaiseOnPropertyChanged(this, "Engine");
            }
        }
        /// <summary>
        /// accept the visitor
        /// </summary>
        /// <param name="visitor"></param>
        public bool Accept(IVisitor visitor) { visitor.Visit(this); return true; }
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
                    e.Stack.Push((e.CurrentNode as DataObjectEntrySymbol).ID);
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
            if (PropertyChanged != null ) PropertyChanged(sender, new PropertyChangedEventArgs(property));
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
                aString += (( first==false )? "," : String.Empty) + ((aNode != null) ? aNode.ToString() : "<NULL>");
                if (first) first = false;
            }
            aString += "}";
            return aString;
        }
    }

    /// <summary>
    /// defines a rule
    /// </summary>
    public abstract class Rule : XPTree, IRule
    {
        private string _id; // unique ID of the rule
        private otRuleState _state; // state of the rule
        private String _handle; // handle of the rule theCode in the engine
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="handle"></param>
        public Rule( string id = null,  Engine engine = null): base(engine: engine)
        {
            this.NodeType = otXPTNodeType.Rule;

            if (id == null) { _id = Guid.NewGuid().ToString(); }
            else { _id = id; }
            _state = otRuleState.created;
            _engine = engine;
            _handle = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// sets or gets the handle of the rule
        /// </summary>
        public string ID { get { return _id; } set { _id = value; } }
        /// <summary>
        /// returns the theCode handle
        /// </summary>
        public string Handle { get { return _handle; } set { _handle = value; } }
        /// <summary>
        /// returns the state of the rule
        /// </summary>
        public otRuleState RuleState { get { return _state; } set { _state = value; } }

        /// <summary>
        /// set the state of the rule
        /// </summary>
        /// <param name="newState"></param>
        protected void SetState(otRuleState newState) { _state = newState; }
    }

    /// <summary>
    /// defines a data object in a IeXPressionTree object
    /// </summary>
    public class Variable : Node, ISymbol
    {
        private string _id;
        private IDataType _datatype;
        private IXPTree _scope;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="typeID"></param>
        /// <param name="scope"></param>
        public Variable(string id, otDataType typeID, IXPTree scope): base(engine: null)
        {
            _id = id;
            _datatype = Core.DataType.GetDataType (typeID);
            _scope = scope;
            this.NodeType = otXPTNodeType.Variable;
        }
        public Variable(string id, IDataType datatype, IXPTree scope)
            : base(engine: null)
        {
            _id = id;
            _datatype = datatype;
            _scope = scope;
            this.NodeType = otXPTNodeType.Variable;
        }

        /// <summary>
        /// gets or sets the ID
        /// </summary>
        public string ID { get { return _id; } set { _id = value; } }

        /// <summary>
        /// gets or sets the Type of the variable
        /// </summary>
        public otDataType TypeId { get { return _datatype.TypeId; } set { DataType = Core.DataType.GetDataType(value); RaiseOnPropertyChanged(this, "TypeId"); } }
        /// <summary>
        /// sets or gets the datatype
        /// </summary>
        public IDataType DataType { get { return _datatype; } set { _datatype = value; RaiseOnPropertyChanged(this, "DataType"); } }
        /// <summary>
        /// sets or gets the Scope
        /// </summary>
        public IXPTree Scope { get { return _scope; } set { _scope = value; RaiseOnPropertyChanged(this, "Scope"); } }
        /// <summary>
        /// returns true if node is a leaf
        /// </summary>
        public override bool HasSubNodes { get { return false; }}
        /// <summary>
        /// gets true if the symbol is valid in the engine
        /// </summary>
        public bool? IsValid
        {
            get
            {
                if (Scope != null)
                {
                    if (Scope is StatementBlock) return ((StatementBlock)Scope).HasVariable (this.ID);
                    if (Scope is SelectionRule) return ((SelectionRule)Scope).HasParameter(this.ID);
                }
                return null;

            }
        }
        /// <summary>
        /// to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("<{0}:{1}>", NodeType.ToString(), this.ID);
        }
    }
    /// <summary>
    /// defines a data object symbol in a IeXPressionTree object
    /// </summary>
    public class DataObjectSymbol : Node, ISymbol
    {
        private IXPTree _scope;
        private iObjectDefinition _objectdefinition;
        private string _id;
        private bool? _isChecked = false;

        /// <summary>
        /// constructor in 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="Type"></param>
        /// <param name="scope"></param>
        public DataObjectSymbol(string id, Engine engine = null)
            : base(engine: engine)
        {
            _engine = engine; // first
            this.ID = id;
            _scope = null;
            this.NodeType = otXPTNodeType.DataObjectSymbol;
        }

        /// <summary>
        /// gets or sets the ID
        /// </summary>
        public string ID
        {
            get { return _id; }
            set
            {
                if (String.Compare(_id, value) != 0)
                {
                    _id = value;
                    _isChecked = null; // reset
                    _objectdefinition = null;
                }
            }
        }

        /// <summary>
        /// returns the ObjectID of the entry
        /// </summary>
        public String ObjectID { get { return _objectdefinition.Objectname; } }
        /// <summary>
        /// returns the IObjectDefinition
        /// </summary>
        public iObjectDefinition ObjectDefinition
        {
            get
            {
                if (_objectdefinition != null) return _objectdefinition;
                if (_engine != null) 
                    { CheckValidity(); return _objectdefinition;}
                return null;
            }
        }

        /// <summary>
        /// returns the scope
        /// </summary>
        public IXPTree Scope { get { return _scope; } set { _scope = value; RaiseOnPropertyChanged(this, "Scope"); } }
        /// <summary>
        /// gets the typeid
        /// </summary>
        public Core.otDataType TypeId { get { return otDataType.DataObject; } set { throw new InvalidOperationException(); } }
        /// <summary>
        /// gets the Datatype
        /// </summary>
        public Core.IDataType DataType { get { return DataObjectType.GetDataType(name:ID, engine:this.Engine); } set { throw new InvalidOperationException(); } }
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
        /// <summary>
        /// check if the ID exists - returns true or false and if !HasValue then not checkable
        /// </summary>
        /// <returns></returns>
        public bool? CheckValidity()
        {
            if (_isChecked.HasValue) return _isChecked;
            if (this.Engine == null) return _isChecked.HasValue;

            /// get the data object definition
            if (ID.Contains('.'))
            {
                string[] names = ID.Split('.');
                if (Engine.Repository.HasDataObjectDefinition(names[0]))
                {
                    _objectdefinition = Engine.Repository.GetDataObjectDefinition(names[0]);
                    if (_objectdefinition == null)
                    {
                        throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[]
                        {
                            names[1],
                            names[0]
                        });
                    }

                }
                else
                { throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { names[0], "data object repository" }); }

            }
            else
            {
                if (Engine.Repository.HasDataObjectDefinition(ID))
                    _objectdefinition = Engine.Repository.GetDataObjectDefinition(ID);
                else
                    throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { ID, "data object repository" });
            }
            _isChecked = true;
            return _isChecked;
        }
        /// <summary>
        /// to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("<{0}:{1}>", NodeType.ToString(), this.ID.ToUpper());
        }
    }
    /// <summary>
    /// defines a local variable in a IeXPressionTree object
    /// </summary>
    public class DataObjectEntrySymbol :  Node, ISymbol 
    {
        private IXPTree _scope;
        private iObjectEntryDefinition _entrydefinition;
        private string _id = null;
        private bool? _isChecked = false;
        /// <summary>
        /// constructor in 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="Type"></param>
        /// <param name="scope"></param>
        public DataObjectEntrySymbol(string id, Engine engine = null): base(engine: engine)
        {
           
            _scope = null;
            _engine = engine;
            this.ID = id;
            this.NodeType = otXPTNodeType.DataObjectSymbol;
        }
        public DataObjectEntrySymbol( string objectid, string entryname,  Engine engine = null): base()
        {
            // default engine
            _engine = engine;
            this.ID = objectid + "." + entryname;
            _scope = null;
          
            this.NodeType = otXPTNodeType.DataObjectSymbol;
        }

        /// <summary>
        /// gets or sets the ID
        /// </summary>
        public string ID
        {
            get { return _id ; }
            set
            {
                if (String.Compare(_id, value) != 0)
                {
                    _id = value;
                    _isChecked = null; // reset
                    _entrydefinition = null;
                }
            }
        }
        /// <summary>
        /// returns the IObjectDefinition
        /// </summary>
        public iObjectDefinition ObjectDefinition { get { CheckValidity(); return _entrydefinition.ObjectDefinition; } }
        /// <summary>
        /// returns the IObjectEntryDefinition
        /// </summary>
        public iObjectEntryDefinition ObjectEntryDefinition { get { CheckValidity();  return _entrydefinition; } }
        /// <summary>
        /// returns the ObjectID of the entry
        /// </summary>
        public String ObjectID { get { CheckValidity();  return _entrydefinition.Objectname; } }
        /// <summary>
        /// returns the ObjectID of the entry
        /// </summary>
        public String Entryname { get { CheckValidity(); return _entrydefinition.Entryname; } }
        /// <summary>
        /// gets or sets the Type of the variable
        /// </summary>
        public otDataType TypeId { get { CheckValidity();  return _entrydefinition.TypeId; } set { throw new NotImplementedException(); } }
        /// <summary>
        /// gets the Datatype
        /// </summary>
        public Core.IDataType DataType { get { CheckValidity(); return _entrydefinition.DataType; } set { throw new InvalidOperationException(); } }
        /// <summary>
        /// returns the scope
        /// </summary>
        public IXPTree Scope { get { return _scope; } set { _scope = value; } }
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
        /// <summary>
        /// check if the ID exists - returns true or false and if !HasValue then not checkable
        /// </summary>
        /// <returns></returns>
        public bool? CheckValidity()
        {
            if (_isChecked.HasValue) return _isChecked;
            if (this.Engine == null) return _isChecked.HasValue;

            /// get the data object definition
            if (ID.Contains('.'))
            {
                string[] names = ID.Split('.');
                if (Engine.Repository.HasDataObjectDefinition(names[0]))
                {
                    Core.iObjectDefinition aDefinition = Engine.Repository.GetDataObjectDefinition(names[0]);
                    _entrydefinition = aDefinition.GetiEntryDefinition(names[1]);
                    if (_entrydefinition == null)
                    {
                        throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[]
                        {
                            names[1],
                            names[0]
                        });
                    }

                }
                else
                { throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { names[0], "data object repository" }); }

            }
            else
            {
                throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { ID, "data object repository (malformed object entry name)" });
            }
            _isChecked = true;
            return _isChecked;
        }
        /// <summary>
        /// to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format ("<{0}:{1}>", NodeType.ToString(), this.ID );
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
            : base(engine)
        {
            this.NodeType = otXPTNodeType.IfThenElse;
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        /// <param name="arguments"></param>
        public IfThenElse(LogicalExpression expression, IStatement @do, IStatement @else=null, Engine engine = null)
            : base(engine)
        {
            // TODO: check the argumetns
            this.NodeType = otXPTNodeType.IfThenElse;
            if (@else != null)
                this.Nodes = new ObservableCollection<INode>(new INode[]{expression, @do, @else});
            else this.Nodes = new ObservableCollection<INode>(new INode[] { expression, @do });
        }

        #region "Properties"
        /// <summary>
        /// gets or sets the logical compare expression
        /// </summary>
        public LogicalExpression @LogicalExpression { get { return (LogicalExpression) this.Nodes[0]; } set { this.Nodes[0] = value; } }
        /// <summary>
        /// gets or sets the do 
        /// </summary>
        public IStatement @Do { get { return (IStatement) this.Nodes[1]; } set { this.Nodes[1] = value; } }
        /// <summary>
        /// gets or sets the do 
        /// </summary>
        public IStatement @Else { get { return (IStatement)this.Nodes[2]; } set { this.Nodes[2] = value; } }
        #endregion
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void _Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base._Nodes_CollectionChanged(sender, e);
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
            : base(engine)
        {
            this.NodeType = otXPTNodeType.Return;
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        /// <param name="arguments"></param>
        public @Return(IExpression @return, Engine engine = null)
            : base(engine)
        {
            this.NodeType = otXPTNodeType.Return;
            Nodes.Add (@return);
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
        protected override void _Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base._Nodes_CollectionChanged(sender, e);
            // check the nodes which are added
            if (!Nodes[0].GetType().GetInterfaces().Contains(typeof(IExpression)))
                throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { Nodes[0].NodeType.ToString(),"Expression" });
            
        }
        /// <summary>
        /// toString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string aString = "{" + this.NodeType.ToString()+ " ";

            if (Nodes.Count() > 0 && Nodes.First().GetType().GetInterfaces ().Contains(typeof(IExpression))) 
                aString += ((IExpression)Nodes.First()).DataType.ToString() + " ";

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
        private Dictionary<string, ISymbol> _variables = new Dictionary<string, ISymbol>(); // variables
        private string _ID;
        /// <summary>
        /// constructor
        /// </summary>
        public StatementBlock(Engine engine = null)
            : base(engine)
        {
            this.NodeType = otXPTNodeType.StatementBlock;
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        /// <param name="arguments"></param>
        public StatementBlock(INode[] arguments, Engine engine = null)
            : base(engine)
        {
            this.NodeType = otXPTNodeType.StatementBlock;
            this.Nodes = new ObservableCollection<INode>(arguments.ToList());
        }
        #region "Properties"
        /// <summary>
        /// gets the list of parameters
        /// </summary>
        public IEnumerable<ISymbol> Variables { get { return _variables.Values.ToList(); } }
        /// <summary>
        /// sets the ID of the block
        /// </summary>
        public string ID { get {  if (_ID == null) _ID = Guid.NewGuid().ToString (); return _ID; } set { _ID = value; } }
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
        protected override  void _Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base._Nodes_CollectionChanged(sender, e);
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
        /// gets the parameter by name
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
                throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { id, this.ID });
            }
            Variable aVar = new Variable(id: id, typeID: typeId, scope: this);
            _variables.Add(aVar.ID, aVar);

            return aVar;
        }
        public ISymbol AddNewVariable(string id, IDataType datatype)
        {
            if (_variables.ContainsKey(id))
            {
                throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { id, this.ID });
            }
            Variable aVar = new Variable(id: id, datatype: datatype, scope: this);
            _variables.Add(aVar.ID, aVar);

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
        public SelectionStatementBlock(Engine engine = null)
            : base(engine)
        {
            this.NodeType = otXPTNodeType.SelectionStatementBlock;
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        /// <param name="arguments"></param>
        public SelectionStatementBlock(INode[] arguments, Engine engine = null)
            : base(engine)
        {
            this.NodeType = otXPTNodeType.SelectionStatementBlock;
            // arguments will be checked in event
            this.Nodes = new ObservableCollection<INode>(arguments.ToList());
        }
        #region "Properties"
        /// <summary>
        /// gets or sets the type id of the variable
        /// </summary>
        /// <value></value>
        public otDataType TypeId
        {
            get
            {
                if (_result != null) return _result.TypeId;
                return otDataType.Null;
            }
            set
            {
                throw new InvalidOperationException();
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
                if (_result != null) return _result.DataType;
                return null;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// gets or sets the result (which is a ResultList)
        /// </summary>
        public ResultList Result
        {
            get
            {
                if (_result != null)
                    return _result;
                return null;
            }
            set
            {
                //TO-DO: do not allow to change if not null thow exception if not equal
                _result = value;
                if (_result.Engine == null) _result.Engine = this.Engine;
            }
        }

        #endregion
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void _Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // call the checks in the base class
            base._Nodes_CollectionChanged(sender, e);
            // check the nodes which are added
            foreach (INode item in e.NewItems)
            {
                // check the nodes which are added
                foreach (INode aNode in Nodes)
                {
                    // check if return is added -> check if the resultlist is the same is in the Property
                    if (aNode != null && aNode is @Return)
                       // return expression must be a selectionExpression (or to-do a variable)
                       if ((((@Return)aNode).Expression) is SelectionExpression) this.Result = ((SelectionExpression)((@Return)aNode).Expression).Results;
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
    /// function call node
    /// </summary>
    public class FunctionCall: XPTree , IStatement, IExpression
    {
        protected Token _function; // function Token

        #region "Properties"

        /// <summary>
        /// gets or sets the Operation
        /// </summary>
        public Token TokenID { get { return _function; } set { _function = value; } }

        /// <summary>
        /// gets the Operator definition
        /// </summary>
        public @Function Function { get { return OnTrack.Rules.Engine.GetFunction(_function); } }
      
        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        /// <param name="arguments"></param>
        public FunctionCall(Token token, INode [] arguments, Engine engine = null) : base(engine: engine)
        {
            // TODO: check the argumetns
            _function = token;
            this.Nodes = new ObservableCollection<INode> (arguments.ToList());
        }
        #region "Properties"
        /// <summary>
        /// gets the Datatype of this Expression
        /// </summary>
        public IDataType DataType { get { return this.Engine.GetFunction(this.TokenID).ReturnType; } set { throw new InvalidOperationException(); } }
        /// <summary>
        /// gets the typeId of this Expression
        /// </summary>
        public otDataType TypeId { get { return this.Engine.GetFunction(this.TokenID).ReturnType.TypeId; ; } set { throw new InvalidOperationException(); } }
        #endregion
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void _Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base._Nodes_CollectionChanged(sender, e);
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

        #region "Properties"

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        /// <param name="arguments"></param>
        public Assignment(ISymbol symbol,IExpression expression)
            : base()
        {
            this.NodeType = otXPTNodeType.Assignment;
            this.Nodes = new ObservableCollection<INode>(new INode []{symbol, expression});
        }
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void _Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base._Nodes_CollectionChanged(sender, e);
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
        protected Token _token; // operation Token
        protected uint? _prio; // overwrite priority of the operator

        #region "Properties"
    
        /// <summary>
        /// gets or sets the Operation
        /// </summary>
        public Token TokenID { get { return _token; } set { _token = value; } }

        /// <summary>
        /// gets the Operator definition
        /// </summary>
        public Operator Operator { get { return OnTrack.Rules.Engine.GetOperator(_token); } }
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
        public IDataType DataType { get { throw new NotImplementedException(); } set { throw new InvalidOperationException(); } }
        /// <summary>
        /// gets the typeId of this Expression
        /// </summary>
        public otDataType TypeId { get { throw new NotImplementedException(); } set { throw new InvalidOperationException(); } }
        /// <summary>
        /// returns true if node is a leaf
        /// </summary>
        public bool HasSubNodes { get { if (this.Operator.Arguments != 0) return true; return false; } }
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
        /// constructor
        /// </summary>
        /// <param name="op"></param>
        /// <param name="operand"></param>
        public OperationExpression(Engine engine = null): base(engine)
        { 
        }
        public OperationExpression(Token token,  Engine engine = null)
            : base(engine)
        {
            if (token == null)
                throw new RulezException(RulezException.Types.OperatorNotDefined, arguments: new object[] { "null" });
            if (OnTrack.Rules.Engine.GetOperator(token) == null)
                throw new RulezException(RulezException.Types.OperatorNotDefined, arguments: new object[] { token.ToString() });
            _token = token;
            if (this.Operator.Arguments != 0)
                throw new RulezException(RulezException.Types.OperandsNotEqualOperatorDefinition, arguments: new object[] { token.ToString(), this.Operator.Arguments, 0 });
            
            _engine = engine;
            this.NodeType = otXPTNodeType.OperationExpression;
        }
        public OperationExpression(Token token, INode operand, Engine engine = null): base(engine)
        {
            if (token == null)
                throw new RulezException(RulezException.Types.OperatorNotDefined, arguments: new object[] { "null" });
            if (OnTrack.Rules.Engine.GetOperator(token) == null) 
                throw new RulezException(RulezException.Types.OperatorNotDefined, arguments: new object[] { token.ToString() });
            _token = token;
            if (this.Operator.Arguments != 1) 
                throw new RulezException(RulezException.Types.OperandsNotEqualOperatorDefinition, arguments: new object[] { token.ToString(), this.Operator.Arguments, 1 });
            if (operand != null) 
                this.Nodes.Add(operand);
            else throw new RulezException(RulezException.Types.OperandNull, arguments: new object[] { token.ToString(), "" });

            _engine = engine;
            this.NodeType = otXPTNodeType.OperationExpression;          
        }
        public OperationExpression( Operator op, INode operand, Engine engine = null): base()
        {
           
            if (op == null) 
                throw new RulezException(RulezException.Types.OperatorNotDefined, arguments: new object[] { "(null)" });
            else _token = op.Token;

            if (this.Operator.Arguments != 1) 
                throw new RulezException(RulezException.Types.OperandsNotEqualOperatorDefinition, arguments: new object[] { op.Token.ToString(), op.Arguments, 1 });

            if (operand != null) this.Nodes.Add(operand);
            else throw new RulezException(RulezException.Types.OperandNull, arguments: new object[] { op.Token.ToString(), "" });

            _engine = engine;
            this.NodeType = otXPTNodeType.OperationExpression;     
        }
        /// <summary>
        /// constructor of an expression
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        public OperationExpression( Token token, INode leftoperand, INode rightoperand, Engine engine = null): base()
        {
            // default engine
            if (engine == null) engine = OnTrack.Rules.Engine;

            if (OnTrack.Rules.Engine.GetOperator(token) == null ) 
                throw new RulezException(RulezException.Types.OperatorNotDefined,arguments:new object[]{ token.ToString() });

            _token = token;

            if (this.Operator.Arguments != 2) 
                throw new RulezException(RulezException.Types.OperandsNotEqualOperatorDefinition, arguments: new object[] { token.ToString(), this.Operator.Arguments, 2 });

            if (leftoperand != null) this.Nodes.Add(leftoperand);
            else throw new RulezException(RulezException.Types.OperandNull, arguments: new object[] { token.ToString(), "left" });

            if (rightoperand != null) this.Nodes.Add(rightoperand);
            else throw new RulezException(RulezException.Types.OperandNull, arguments: new object[] { token.ToString(), "right" });

            _engine = engine;
            this.NodeType = otXPTNodeType.OperationExpression;     
        }
        public OperationExpression(Operator op, INode leftoperand, INode rightoperand, Engine engine = null)
            : base()
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

            _engine = engine;
            this.NodeType = otXPTNodeType.OperationExpression;
        }
        /// <summary>
        /// handler for changing the nodes list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void _Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base._Nodes_CollectionChanged(sender, e);
            // check the nodes which are added
            foreach (INode aNode in Nodes)
            {
                if (aNode != null && !aNode.GetType().GetInterfaces().Contains(typeof(IExpression)))
                    throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { aNode.NodeType.ToString(), "Expression" });
             
            }

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
        public LogicalExpression(Engine engine): base(engine: engine)
        { this.NodeType = otXPTNodeType.LogicalExpression; }
        public LogicalExpression(Token op, Engine engine = null)
            : base(op, engine)
        {
            if (this.Operator.Type != otOperatorType.Logical && this.Operator.Type != otOperatorType.Logical )
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected, arguments: new object[] { op.ToString(), "logical" });
            this.NodeType = otXPTNodeType.LogicalExpression;
        }
        public LogicalExpression(Token op, IExpression operand, Engine engine = null)
            : base(op, operand, engine)
        {
            if (this.Operator.Type != otOperatorType.Logical && this.Operator.Type != otOperatorType.Compare)
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected , arguments: new object[] { op.ToString(), "logical" });
            this.NodeType = otXPTNodeType.LogicalExpression;          
        }
        public LogicalExpression(Operator op, IExpression operand, Engine engine = null)
            : base(op, operand, engine)
        {
            if (this.Operator.Type != otOperatorType.Logical && this.Operator.Type != otOperatorType.Compare)
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected , arguments: new object[] { op.ToString(), "logical" });
            this.NodeType = otXPTNodeType.LogicalExpression;          

        }
        /// <summary>
        /// constructor of an expression
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        public LogicalExpression( Token op, INode leftoperand, INode rightoperand, Engine engine = null): base( op,  leftoperand, rightoperand, engine )
        {
            if (this.Operator.Type != otOperatorType.Logical && this.Operator.Type != otOperatorType.Compare)
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected , arguments: new object[] { op.ToString(), "logical" });
            this.NodeType = otXPTNodeType.LogicalExpression;          
        }
        public LogicalExpression(Operator op, INode leftoperand, INode rightoperand, Engine engine = null)
            : base(op, leftoperand, rightoperand, engine)
        {
            if (this.Operator.Type !=  otOperatorType.Logical  && this.Operator.Type != otOperatorType.Compare )
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected , arguments: new object[] { op.ToString(), "logical" });
            this.NodeType = otXPTNodeType.LogicalExpression;          
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
         protected override void _Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
         {
             base._Nodes_CollectionChanged(sender, e);
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
        public CompareExpression(Engine engine)
            : base(engine: engine)
        { this.NodeType = otXPTNodeType.CompareExpression; }
        /// <summary>
        /// constructor of an expression
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="leftoperand"></param>
        /// <param name="rightoperand"></param>
        public CompareExpression(Token op, IExpression leftoperand, IExpression rightoperand, Engine engine = null)
            : base(op, leftoperand, rightoperand, engine)
        {
            if (this.Operator.Type != otOperatorType.Logical && this.Operator.Type != otOperatorType.Compare)
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected, arguments: new object[] { op.ToString(), "compare" });
            this.NodeType = otXPTNodeType.CompareExpression;
        }
        public CompareExpression(Operator op, IExpression leftoperand, IExpression rightoperand, Engine engine = null)
            : base(op, leftoperand, rightoperand, engine)
        {
            if (this.Operator.Type != otOperatorType.Logical && this.Operator.Type != otOperatorType.Compare)
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected, arguments: new object[] { op.ToString(), "compare" });
            this.NodeType = otXPTNodeType.CompareExpression;
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
        protected override void _Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base._Nodes_CollectionChanged(sender, e);
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
    /// defines a node for holding an result
    /// </summary>
/*    public class Result : XPTree
    {
        private String _ID;
        private List<String> _objectnames = new List<String> (); // object names the result is referring to

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="nodes"></param>
        public Result(string ID, INode node, Engine engine=null):base(engine: engine)
        {
            _ID = ID;
            this.NodeType = otXPTNodeType.Result;
            this.Nodes.Add(node);
        }

        /// <summary>
        /// return the embedded Result INode
        /// </summary>
        public INode Embedded { get { return this.Nodes[0]; } }
        /// <summary>
        /// gets or sets the ID of the result node
        /// </summary>
        public String ID { get { return _ID; } set { _ID = value; } }
        /// <summary>
        /// gets the Datatype of the REsult
        /// </summary>
        public IDataType DataType 
        { 
            get { 
                    if (Nodes[0] != null && Nodes[0].GetType ().GetInterfaces().Contains(typeof(IExpression)))
                                            return ((IExpression) Nodes[0]).DataType;
                    return null;
                }
            set
            {
                throw new InvalidOperationException();
            }
        }
        /// <summary>
        /// gets the TypeId of the REsult
        /// </summary>
        public otDataType TypeId
        {
            get
            {
                if (Nodes[0] != null && Nodes[0].GetType().GetInterfaces().Contains(typeof(IExpression)))
                    return ((IExpression)Nodes[0]).DataType.TypeId;
                return otDataType.Null;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }
        /// <summary>
        /// gets the Objectname referenced in the Node
        /// </summary>
        public List<String> Objectnames
        {
            get
            {
                if ((this.Nodes == null) || (Nodes.Count == 0)) { _objectnames.Clear(); return _objectnames; }
                // check the tree
                Visitor<String> aVisitor = new Visitor<string>();
               
                aVisitor.VisitingDataObjectSymbol += new Visitor<string>.Eventhandler(VisitorEvent);
                aVisitor.Visit(this.Nodes[0]);
                aVisitor.VisitingDataObjectSymbol -= new Visitor<string>.Eventhandler(VisitorEvent);
                
                return _objectnames;
            }
        }
        /// <summary>
        /// VisitorEvent
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public void VisitorEvent(object o, VisitorEventArgs<String> e)
        {
            String anObjectname = (e.CurrentNode as DataObjectEntrySymbol).ObjectID;
            // add it
            if (!_objectnames.Contains<String>(anObjectname)) _objectnames.Add(anObjectname);
        }
    }
    */
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
        {
            this.NodeType = otXPTNodeType.ResultList;
            foreach (INode aNode in results) this.Nodes.Add(aNode);
        }
        public ResultList(params DataObjectSymbol[] results)
        {
            this.NodeType = otXPTNodeType.ResultList;
            foreach (DataObjectSymbol aNode in results) this.Nodes.Add(aNode);
        }
        public ResultList(params DataObjectEntrySymbol[] results)
        {
            this.NodeType = otXPTNodeType.ResultList;
            foreach (DataObjectEntrySymbol aNode in results) this.Nodes.Add(aNode);
        }
        public ResultList(List<INode> results)
        {
            this.NodeType = otXPTNodeType.ResultList;
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
            set
            {
                throw new InvalidOperationException();
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
                if (this.Nodes.Count() == 1) return ListType.GetDataType(innerDataType: ((IExpression)this.Nodes[0]).DataType, engine: this.Engine) ;
                // get a Datatype according to the structure
                List<IDataType> structure = new List<IDataType>();
                List<string> names = new List<string>();
                foreach (ISymbol aResult in Nodes) { names.Add(aResult.ID);  structure.Add(aResult.DataType);}
                IDataType innerType = TupleType.GetDataType(structure: structure.ToArray(), memberNames: names.ToArray(), engine: this.Engine);
                return ListType.GetDataType(innerDataType: innerType, engine: this.Engine);
            }
            set
            {
                throw new InvalidOperationException();
            }
        }
        /// <summary>
        /// return a unique list of used objectnames in the result list
        /// </summary>
        public IList<String> DataObjectNames ()
        {
            List<String> aList = new List<String>();
            foreach (INode aNode in this.Nodes)
            {
                string aName = String.Empty;
                if (aNode is DataObjectEntrySymbol) aName = ((DataObjectEntrySymbol)aNode).ID;
                if (aNode is DataObjectSymbol) aName = ((DataObjectSymbol)aNode).ID;

                if ( !String.IsNullOrEmpty (aName) && !aList.Contains(aName)) aList.Add(aName);
            }
            // return list
            return aList;
        }
    }
    /// <summary>
    /// defines a selection rule expression
    /// </summary>
    public class SelectionRule : Rule, IExpression
    {
        #region Static
        /// <summary>
        /// generate a selection Rule XPT out of a String
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public SelectionRule Generate(string source)
        {
            RulezParser.MessageListener aListener = new RulezParser.MessageListener();
            RulezParser.SelectionRulezContext aCtx = null;

            try
            {
                RulezLexer aLexer = new RulezLexer(new Antlr4.Runtime.AntlrInputStream(source));
                // wrap a token-stream around the lexer
                Antlr4.Runtime.CommonTokenStream theTokens = new Antlr4.Runtime.CommonTokenStream(aLexer);
                // create the aParser
                RulezParser aParser = new RulezParser(theTokens);
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
        private Dictionary<string, ISymbol> _parameters = new Dictionary<string, ISymbol>(); // parameters
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="handle"></param>
        public SelectionRule(string id = null,  Engine engine = null): base(id, engine)
        {
            this.NodeType= otXPTNodeType.SelectionRule;
        }
        /// <summary>
        /// gets or sets the type id of the variable
        /// </summary>
        /// <value></value>
        public otDataType TypeId
        {
            get
            {
                if (Selection != null) return Selection.TypeId;
                return otDataType.Null;
            }
            set
            {
                throw new InvalidOperationException();
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
                if (Selection != null) return Selection.DataType;
                return null;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// gets or sets the result (which is a ResultList)
        /// </summary>
        public ResultList Result { get { if (this.Nodes.Count > 0)  return ((SelectionStatementBlock)this.Nodes[0]).Result; return null;  } set { throw new InvalidOperationException(); } }

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
        public IEnumerable<ISymbol> Parameters { get { return _parameters.Values.ToList(); } }
        /// <summary>
        /// returns a List of object names retrieved with this rule
        /// </summary>
        /// <returns></returns>
        public IList<String> ResultingObjectnames()
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
        { return _parameters.ContainsKey(id); }
        /// <summary>
        /// gets the parameter by name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ISymbol GetParameter(string id)
        { return _parameters[id]; }
        /// <summary>
        /// Adds a Parameter to the Selection Rule
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public ISymbol AddNewParameter(string id, otDataType typeId)
        {
            if (_parameters.ContainsKey(id))
            {
                throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { id, this.ID });
            }
            Variable aVar = new Variable(id:id, typeID:typeId, scope:this);
             _parameters.Add(aVar.ID,aVar);

             return aVar;
        }
        /// <summary>
        /// adds a Parameter by dataobject
        /// </summary>
        /// <param name="id"></param>
        /// <param name="datatype"></param>
        /// <returns></returns>
        public ISymbol AddNewParameter(string id, IDataType datatype)
        {
            if (_parameters.ContainsKey(id))
            {
                throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { id, this.ID });
            }
            Variable aVar = new Variable(id: id, datatype: datatype, scope: this);
            _parameters.Add(aVar.ID, aVar);

            return aVar;
        }
        /// <summary>
        /// toString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            bool comma = false;
            string aString = "{(" + NodeType.ToString() + ") " + this.ID + "[";
            foreach (ISymbol aSymbol in Parameters)
            {
                if (comma) aString += ",";
                aString += aSymbol.ToString();
                comma = true;
            }
            aString += "]";
            if (this.Result != null) aString += Result.ToString();
            aString += "{";
            if (Selection != null) aString += Selection.ToString();
            aString += "}}";
            return aString;
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
        private ResultList _result;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="handle"></param>
        public SelectionExpression(ResultList result=null, Engine engine = null): base(engine)
        {
            this.NodeType = otXPTNodeType.SelectionExpression;
            this.Results = result;
        }
        /// <summary>
        /// gets or sets the type id of the variable
        /// </summary>
        /// <value></value>
        public otDataType TypeId
        {
            get
            {
                return Results.TypeId;
            }
            set
            {
                throw new InvalidOperationException();
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
                return Results.DataType;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// gets or sets the result (which is a ResultList)
        /// </summary>
        public ResultList Results
        {   get { return _result; }
            set {
                if (_result != value)
                {
                    _result = value;
                    // set the engine to this Engine
                    foreach (INode aNode in _result) aNode.Engine = this.Engine;
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
            string aString = "{(" + NodeType.ToString ()+ ") " +  this.Results.ToString () +":";
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
    /// top level 
    /// </summary>
    public class Unit : XPTree
    {
        private string _ID;
        /// <summary>
        /// constructor
        /// </summary>
        public Unit(Engine engine = null)
            : base(engine)
        {
            this.NodeType = otXPTNodeType.Unit;
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="token"></param>
        /// <param name="arguments"></param>
        public Unit(INode[] arguments, Engine engine = null)
            : base(engine)
        {
            this.NodeType = otXPTNodeType.Unit;
            this.Nodes = new ObservableCollection<INode>(arguments.ToList());
        }
        #region "Properties"
        /// <summary>
        /// sets the ID of the block
        /// </summary>
        public string ID { get { if (_ID == null) _ID = Guid.NewGuid().ToString(); return _ID; } set { _ID = value; } }
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
        protected override void _Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base._Nodes_CollectionChanged(sender, e);
            // check the nodes which are added
            foreach (INode aNode in Nodes)
            {
                if (aNode != null && ! (aNode is SelectionRule ))
                    throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { aNode.NodeType.ToString(), "SelectionRule" });
            }
        }
        
    }
}