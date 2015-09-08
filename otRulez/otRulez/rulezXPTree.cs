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
        public otXPTNodeType NodeType { get { return this.NodeType; } protected set { this.NodeType = value; } }
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
            PropertyChanged(sender, new PropertyChangedEventArgs(property));
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
            _value = Core.DataType.To(value, datatype);
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
        public object Value { get { return _value; } set { _value = value; _hasValue = true; RaiseOnPropertyChanged(this, "Value"); } }
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
    }

    /// <summary>
    /// Base class for all tree nodes
    /// </summary>
    public abstract class XPTree : IXPTree
    {
        private ObservableCollection<INode> _nodes = new ObservableCollection<INode>();
        protected Engine _engine;
        private otXPTNodeType _nodeType;
        private IXPTree _parent;
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
                foreach (INode aNode in e.NewItems) { aNode.Parent = this; aNode.Engine = this.Engine; }
        }
        /// <summary>
        /// return the node type
        /// </summary>
        public otXPTNodeType NodeType { get { return this.NodeType; } protected set { this.NodeType = value; } }
        /// <summary>
        /// return all the leaves
        /// </summary>
        public ObservableCollection<INode> Nodes { get { return this.Nodes; } set { this.Nodes = value; this.Nodes.CollectionChanged += _Nodes_CollectionChanged; } }
        /// <summary>
        /// returns true if node is a leaf
        /// </summary>
        public bool HasSubNodes { get { return true; } }
        /// <summary>
        /// gets the Parent of the Node
        /// </summary>
        public IXPTree Parent { get { return _parent; } set { _parent = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Parent")); } }
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
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Engine"));
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
            PropertyChanged(sender, new PropertyChangedEventArgs(property));
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
    }
    /// <summary>
    /// defines a data object symbol in a IeXPressionTree object
    /// </summary>
    public class DataObjectSymbol : Node, ISymbol
    {
        private IXPTree _scope;
        private iObjectDefinition _objectdefinition;

        /// <summary>
        /// constructor in 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="Type"></param>
        /// <param name="scope"></param>
        public DataObjectSymbol(string id, Engine engine = null)
            : base(engine: engine)
        {
            
            ///
            if (id.Contains('.'))
            {
                string[] names = id.Split('.');
                if (engine.Repository.HasDataObjectDefinition(names[0]))
                {
                    _objectdefinition = engine.Repository.GetDataObjectDefinition(names[0]);
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
                throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { id, "data object repository" });
            }

            _scope = null;
            _engine = engine;
            this.NodeType = otXPTNodeType.DataObjectSymbol;
        }

        /// <summary>
        /// gets or sets the ID
        /// </summary>
        public string ID
        {
            get { return _objectdefinition.Objectname; }
            set
            {   ///
                if (value.Contains('.'))
                {
                    string[] names = value.Split('.');
                    if (_engine.Repository.HasDataObjectDefinition(names[0]))
                    {
                        _objectdefinition = _engine.Repository.GetDataObjectDefinition(names[0]);
                        if (_objectdefinition == null) throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { names[1], names[0] });

                    }
                    else
                    { throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { names[0], "data object repository" }); }

                }
                else
                {
                    throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { value, "data object repository" });
                };
            }
        }

        /// <summary>
        /// returns the ObjectID of the entry
        /// </summary>
        public String ObjectID { get { return _objectdefinition.Objectname; } }
        /// <summary>
        /// returns the IObjectDefinition
        /// </summary>
        public iObjectDefinition Definition { get { return _objectdefinition; } }
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
        public Core.IDataType DataType { get { return this.Engine.Repository.GetDatatype(_objectdefinition.Objectname); } set { throw new InvalidOperationException(); } }
        /// <summary>
        /// returns true if node is a leaf
        /// </summary>
        public override bool HasSubNodes { get { return false; } }
    }
    /// <summary>
    /// defines a local variable in a IeXPressionTree object
    /// </summary>
    public class DataObjectEntrySymbol :  Node, ISymbol 
    {
        private IXPTree _scope;
        private iObjectEntryDefinition _entrydefinition;

        /// <summary>
        /// constructor in 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="Type"></param>
        /// <param name="scope"></param>
        public DataObjectEntrySymbol(string id, Engine engine = null): base(engine: engine)
        {
           
            ///
            if (id.Contains ('.')) {
                string[] names = id.Split ('.');
                if (engine.Repository.HasDataObjectDefinition(names[0]))
                {
                    Core.iObjectDefinition aDefinition = engine.Repository.GetDataObjectDefinition(names[0]);
                    _entrydefinition = aDefinition.GetiEntryDefinition(names[1]);
                    if (_entrydefinition == null)
                    {
                        throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[]
                        {
                            names[1],
                            names[0]
                        });
                    }
                   
                }else
                { throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { names[0], "data object repository" }); }

            }else{
                throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { id, "data object repository" });
            }

            _scope = null;
            _engine = engine;
            this.NodeType = otXPTNodeType.DataObjectSymbol;
        }
        public DataObjectEntrySymbol( string objectid, string entryname,  Engine engine = null): base()
        {
            // default engine
            if (engine == null) engine = OnTrack.Rules.Engine;

                if (engine.Repository.HasDataObjectDefinition(objectid))
                {
                    Core.iObjectDefinition aDefinition = engine.Repository.GetDataObjectDefinition(objectid);
                    _entrydefinition = aDefinition.GetiEntryDefinition(entryname);
                    if (_entrydefinition == null) throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { entryname, objectid });
                    else { }
                }
                else
                { throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { objectid, "data object repository" }); }


            _scope = null;
            _engine = engine;
            this.NodeType = otXPTNodeType.DataObjectSymbol;
        }

        /// <summary>
        /// gets or sets the ID
        /// </summary>
        public string ID
        {
            get { return _entrydefinition .Objectname  + '.' + _entrydefinition .Entryname ; }
            set
            {   ///
                if (value.Contains('.'))
                {
                    string[] names = value.Split('.');
                    if (_engine.Repository.HasDataObjectDefinition(names[0]))
                    {
                        Core.iObjectDefinition aDefinition = _engine.Repository.GetDataObjectDefinition(names[0]);
                        _entrydefinition = aDefinition.GetiEntryDefinition(names[1]);
                        if (_entrydefinition == null) throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { names[1], names[0] });

                    }
                    else
                    { throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { names[0], "data object repository" }); }

                }
                else
                {
                    throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { value, "data object repository" });
                };
            }
        }
        /// <summary>
        /// returns the IObjectDefinition
        /// </summary>
        public iObjectEntryDefinition Definition { get { return _entrydefinition; } }
        /// <summary>
        /// returns the ObjectID of the entry
        /// </summary>
        public String ObjectID { get { return _entrydefinition.Objectname; } }
        /// <summary>
        /// returns the ObjectID of the entry
        /// </summary>
        public String Entryname { get { return _entrydefinition.Entryname; } }
        /// <summary>
        /// gets or sets the Type of the variable
        /// </summary>
        public otDataType  TypeId { get { return _entrydefinition.TypeId; } set { throw new NotImplementedException(); } }
        /// <summary>
        /// gets the Datatype
        /// </summary>
        public Core.IDataType DataType { get { return _entrydefinition.DataType; } set { throw new InvalidOperationException(); } }
        /// <summary>
        /// returns the scope
        /// </summary>
        public IXPTree Scope { get { return _scope; } set { _scope = value; } }
        /// <summary>
        /// returns true if node is a leaf
        /// </summary>
        public override bool HasSubNodes { get { return false; } }
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
            this.Nodes = new ObservableCollection<INode>(new INode[]{expression, @do, @else});
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
            if (Nodes[1] != null && !Nodes[1].GetType().IsAssignableFrom(typeof(IStatement)))
                throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { Nodes[1].NodeType.ToString(), otXPTNodeType.StatementBlock.ToString() });
            if (Nodes[2] != null && !Nodes[2].GetType().IsAssignableFrom(typeof(IStatement)))
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
        public @Return(IExpression @Return, Engine engine = null)
            : base(engine)
        {
            this.NodeType = otXPTNodeType.Return;
            Nodes[0] = @Return;
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
            if (!Nodes[0].GetType().IsAssignableFrom(typeof(IExpression)))
                throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { Nodes[0].NodeType.ToString(),"Expression" });
            
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
                if (aNode != null && !aNode.GetType().IsAssignableFrom(typeof(IStatement)))
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
    public class SelectionStatementBlock : StatementBlock, IStatement
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
        /// gets or sets the result (which is a ResultList)
        /// </summary>
        public ResultList Result { get { return _result; } set { _result = value; } }
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
                    if (aNode != null && !aNode.GetType().IsAssignableFrom(typeof(IStatement)))
                        throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { aNode.NodeType.ToString(), "Statement" });
                }
            }
        }
        /// <summary>
        /// returns a List of objectnames retrieved with this rule
        /// </summary>
        /// <returns></returns>
        public List<String> ResultingObjectnames()
        {
            List<String> aList = new List<String>();

            /// collect unique alle the objectnames in the result nodes
            /// 
            foreach (Result aNode in _result)
            {
                if (aNode.Objectnames.Count != 0) foreach (String aName in aNode.Objectnames) if (!aList.Contains(aName)) aList.Add(aName);
            }

            return aList;
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
                if (aNode != null && !aNode.GetType().IsAssignableFrom(typeof(IExpression)))
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
        public Assignment(ISymbol symbol,INode expression)
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
            if (Nodes[1] != null && !Nodes[1].GetType().IsAssignableFrom(typeof(IExpression)))
                throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { Nodes[1].NodeType.ToString(), "Expression" });
            
        }
    }
    /// <summary>
    /// Operation Selection
    /// </summary>
    public class OperationExpression: XPTree , IExpression
    {
        protected Token _op; // operation Token

        #region "Properties"
    
        /// <summary>
        /// gets or sets the Operation
        /// </summary>
        public Token TokenID { get { return _op; } set { _op = value; } }

        /// <summary>
        /// gets the Operator definition
        /// </summary>
        public Operator Operator { get { return OnTrack.Rules.Engine.GetOperator(_op); } }

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
        #region "Properties"
        /// <summary>
        /// gets the Datatype of this Expression
        /// </summary>
        public IDataType DataType { get { throw new NotImplementedException(); } set { throw new InvalidOperationException(); } }
        /// <summary>
        /// gets the typeId of this Expression
        /// </summary>
        public otDataType TypeId { get { throw new NotImplementedException(); } set { throw new InvalidOperationException(); } }
        #endregion
        /// <summary>
        /// build and return a recursive LogicalExpression Tree from arguments
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private INode BuildExpressionTree(int i)
        {
            // build right-hand a subtree
            if (this.Nodes.Count >= i + 1) return new OperationExpression(this.Operator, this.Nodes[i], BuildExpressionTree(i + 1));
            // return the single node
            return this.Nodes[i];
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
        #endregion

         /// <summary>
        /// constructor
        /// </summary>
        /// <param name="op"></param>
        /// <param name="operand"></param>
        public OperationExpression(Engine engine = null): base(engine)
        { 
        }
        public OperationExpression(Token op, INode operand, Engine engine = null): base(engine)
        {
            if (OnTrack.Rules.Engine.GetOperator(op) == null) throw new RulezException(RulezException.Types.OperatorNotDefined, arguments: new object[] { op.ToString() });
            _op = op;
            if (this.Operator.Arguments != 1) throw new RulezException(RulezException.Types.OperandsNotEqualOperatorDefinition, arguments: new object[] { op.ToString(), this.Operator.Arguments, 1 });
            if (operand != null) this.Nodes[0] = operand;
            else throw new RulezException(RulezException.Types.OperandNull, arguments: new object[] { op.ToString(), "" });

            _engine = engine;
            this.NodeType = otXPTNodeType.OperationExpression;          
        }
        public OperationExpression( Operator op, INode operand, Engine engine = null): base()
        {
            _op = op.Token;
            if (op == null) throw new RulezException(RulezException.Types.OperatorNotDefined, arguments: new object[] { "(null)" });
            if (this.Operator.Arguments != 1) throw new RulezException(RulezException.Types.OperandsNotEqualOperatorDefinition, arguments: new object[] { op.Token.ToString(), op.Arguments, 1 });
            if (operand != null) this.Nodes[0] = operand;
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
        public OperationExpression( Token op, INode leftoperand, INode rightoperand, Engine engine = null): base()
        {
            // default engine
            if (engine == null) engine = OnTrack.Rules.Engine;

            if (OnTrack.Rules.Engine.GetOperator(op) == null ) throw new RulezException(RulezException.Types.OperatorNotDefined,arguments:new object[]{ op.ToString() });
            _op = op;
            if (this.Operator.Arguments != 2) throw new RulezException(RulezException.Types.OperandsNotEqualOperatorDefinition, arguments: new object[] { op.ToString(), this.Operator.Arguments, 2 });
            if (leftoperand != null) this.Nodes[0] = leftoperand;
            else throw new RulezException(RulezException.Types.OperandNull, arguments: new object[] { op.ToString(), "left" });

            if (rightoperand != null) this.Nodes[1] = rightoperand;
            else throw new RulezException(RulezException.Types.OperandNull, arguments: new object[] { op.ToString(), "right" });
            _engine = engine;
            this.NodeType = otXPTNodeType.OperationExpression;     
        }
        public OperationExpression(Operator op, INode leftoperand, INode rightoperand, Engine engine = null)
            : base()
        {
            // default engine
            if (engine == null) engine = OnTrack.Rules.Engine;

            _op = op.Token;
            if (op.Arguments != 2) throw new RulezException(RulezException.Types.OperandsNotEqualOperatorDefinition, arguments: new object[] { op.Token.ToString(), op.Arguments, 2 });
            if (leftoperand != null) this.Nodes[0] = leftoperand;
            else throw new RulezException(RulezException.Types.OperandNull, arguments: new object[] { op.Token.ToString(), "left" });

            if (rightoperand != null) this.Nodes[1] = rightoperand;
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
                if (aNode != null && !aNode.GetType().IsAssignableFrom(typeof(IExpression)))
                    throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { aNode.NodeType.ToString(), "Expression" });
            }

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
        public LogicalExpression(Token op, IExpression operand, Engine engine = null)
            : base(op, operand, engine)
        {
            if (this.Operator.Type !=  otOperatorType.Logical )
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected , arguments: new object[] { op.ToString(), "logical" });
            this.NodeType = otXPTNodeType.LogicalExpression;          
        }
        public LogicalExpression(Operator op, IExpression operand, Engine engine = null)
            : base(op, operand, engine)
        {
            if (this.Operator.Type !=  otOperatorType.Logical )
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
            if (this.Operator.Type !=  otOperatorType.Logical )
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected , arguments: new object[] { op.ToString(), "logical" });
            this.NodeType = otXPTNodeType.LogicalExpression;          
        }
        public LogicalExpression(Operator op, INode leftoperand, INode rightoperand, Engine engine = null)
            : base(op, leftoperand, rightoperand, engine)
        {
            if (this.Operator.Type !=  otOperatorType.Logical )
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
         /// returns an LogicalExpression with ORELSE
         /// </summary>
         /// <param name="leftoperand"></param>
         /// <param name="rightoperand"></param>
         /// <returns></returns>
         static public LogicalExpression NOT(IExpression operand)
         {
             return new LogicalExpression(new Token(Token.NOT), operand);
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
                 if (aNode != null && !aNode.GetType().IsAssignableFrom(typeof(IExpression)))
                     throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { aNode.NodeType.ToString(), "Expression" });
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
            if (this.Operator.Type != otOperatorType.Logical)
                throw new RulezException(RulezException.Types.OperatorTypeNotExpected, arguments: new object[] { op.ToString(), "compare" });
            this.NodeType = otXPTNodeType.CompareExpression;
        }
        public CompareExpression(Operator op, IExpression leftoperand, IExpression rightoperand, Engine engine = null)
            : base(op, leftoperand, rightoperand, engine)
        {
            if (this.Operator.Type != otOperatorType.Logical)
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
            return new CompareExpression(new Token(Token.NEQ), leftoperand, rightoperand);
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
                if (aNode != null && !aNode.GetType().IsAssignableFrom(typeof(IExpression)))
                    throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { aNode.NodeType.ToString(), "Expression" });
            }

        }
    }
    /// <summary>
    /// defines a node for holding an result
    /// </summary>
    public class Result : XPTree
    {
        private String _ID;
        private List<String> _objectnames = new List<String> (); // objectnames the result is referring to

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
                    if (Nodes[0] != null && Nodes[0].GetType ().IsAssignableFrom (typeof(IExpression))) 
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
                if (Nodes[0] != null && Nodes[0].GetType().IsAssignableFrom(typeof(IExpression)))
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
    /// <summary>
    /// define a list of results
    /// </summary>
    public class ResultList: XPTree
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="results"></param>
        public ResultList(params Result[] results)
        {
            this.NodeType = otXPTNodeType.ResultList;
            foreach (INode aNode in results) this.Nodes.Add(aNode);
        }
        public ResultList(List<Result> results)
        {
            this.NodeType = otXPTNodeType.ResultList;
            foreach (INode aNode in results) this.Nodes.Add(aNode);
        }
        public ResultList(params INode[] results)
        {
            this.NodeType = otXPTNodeType.ResultList;
            foreach (INode aNode in results) this.Nodes.Add(aNode);
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
            if (node.NodeType == otXPTNodeType.ResultList)
            {
                throw new RulezException(RulezException.Types.InvalidOperandNodeType, arguments: new object[] { otXPTNodeType.ResultList .ToString(), otXPTNodeType.Result .ToString() });
                
            }else if (node.NodeType != otXPTNodeType.Result)
            {
                String anID = (this.Nodes.Count + 1).ToString();
                this.Nodes.Add(new Result(ID:anID, node: node));

            }else if (node.NodeType == otXPTNodeType.Result)
            {
                // check entries
                foreach (Result aNode in this.Nodes) if ((node as Result).ID == aNode.ID)
                        throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { (node as Result).ID });
                this.Nodes.Add(node);
            }
            return true;
        }

        /// <summary>
        /// return a unique list of used objectnames in the result list
        /// </summary>
        public List<String> Objectnames ()
        {
            List<String> aList = new List<String>();
            foreach (Result aNode in this.Nodes)
            {
                foreach (String aName in aNode.Objectnames )
                    if ( !aList.Contains(aName)) aList.Add(aName);
            }
            // return list
            return aList;
        }
    }
    /// <summary>
    /// defines a selection rule expression
    /// </summary>
    public class SelectionRule : Rule
    {
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
        /// gets or sets the result (which is a ResultList)
        /// </summary>
        public ResultList Result { get { return ((SelectionExpression)this.Nodes[0]).Result; } set { ((SelectionExpression)this.Nodes[0]).Result = value; } }

        /// <summary>
        /// gets or sets the selection expression
        /// </summary>
        public SelectionStatementBlock Selection
        {
            get
            {
                return (SelectionStatementBlock)this.Nodes.First();
            }
            set
            {
                if (value.NodeType == otXPTNodeType.SelectionExpression || value.NodeType == otXPTNodeType.SelectionStatementBlock) this.Nodes[0] = value;
                throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { value.NodeType.ToString() });
            }
        }

        /// <summary>
        /// gets the list of parameters
        /// </summary>
        public IEnumerable<ISymbol> Parameters { get { return _parameters.Values.ToList(); } }
        /// <summary>
        /// returns a List of objectnames retrieved with this rule
        /// </summary>
        /// <returns></returns>
        public List<string> ResultingObjectnames ()
        {
            return (this.Selection == null) ?  new List<string>() :  this.Selection.ResultingObjectnames();
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
       
    }
    /// <summary>
    /// defines a selection rule expression
    /// </summary>
    public class SelectionExpression : LogicalExpression
    {
        /// <summary>
        /// resultlist
        /// </summary>
        private ResultList _result;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="handle"></param>
        public SelectionExpression(ResultList result=null, Engine engine = null): base(engine)
        {
            this.NodeType = otXPTNodeType.SelectionExpression;
            this.Result = result;
        }
        /// <summary>
        /// gets or sets the result (which is a ResultList)
        /// </summary>
        public ResultList Result { get { return _result; } set { _result = value; } }
        /// <summary>
        /// returns a List of objectnames retrieved with this rule
        /// </summary>
        /// <returns></returns>
        public List<String> ResultingObjectnames()
        {
            List<String> aList = new List<String>();

            /// collect unique alle the objectnames in the result nodes
            /// 
            foreach (Result aNode in _result)
            {
                if (aNode.Objectnames.Count != 0) foreach (String aName in aNode.Objectnames) if (!aList.Contains(aName)) aList.Add(aName);
            }

            return aList;
        }
    }
}