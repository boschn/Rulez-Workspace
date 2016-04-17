/**
 *  ONTRACK RULEZ ENGINE
 *  
 * rulez engine
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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using OnTrack.Core;
using OnTrack.Rulez.eXPressionTree;
using System.ComponentModel;

namespace OnTrack.Rulez
{
    /// <summary>
    /// an engine for running rulez
    /// </summary>
    public class Engine : IScope
    {
        /// <summary>
        /// defines the arguments for an engine event
        /// </summary>
        public class EventArgs : System.EventArgs
        {
            private readonly IDataObjectEngine _engine;

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="engine"></param>
            public EventArgs(IDataObjectEngine engine)
            {
                _engine = engine;
            }
            /// <summary>
            /// gets the engine
            /// </summary>
            public IDataObjectEngine Engine { get { return _engine; } }
            /// <summary>
            /// gets the repository
            /// </summary>
            public IDataObjectRepository DataObjectRepository { get { return (_engine != null) ? _engine.Objects : null; ; } }
        }

        private readonly Scope _globalScope;
        private readonly string _id; // handle of the engine
        private readonly Context _context;
        private readonly Dictionary<String, ICodeBit> _code; // Code Dictionary
        private readonly List<IDataObjectEngine> _dataobjectEngines; // DataObject Engines for running data object against
        private bool _isInitialized = false;
        private readonly IRepository _repository; // global repository
        private readonly ObservableCollection<IScope> _subscopes = new ObservableCollection<IScope>(); // children scopes

        /// events
        public event EventHandler<EventArgs> DataObjectRepositoryAdded;
        public event EventHandler<EventArgs> DataObjectEngineAdded;
        public event EventHandler<EventArgs> DataObjectRepositoryRemoved;
        public event EventHandler<EventArgs> DataObjectEngineRemoved;
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// constructor of an engine
        /// </summary>
        public Engine(string id = null)
        {
            if (id == null) _id = System.Environment.MachineName + "_" + DateTime.Now.ToString("o");
            else _id = id;
            _globalScope = new Scope(engine: this, id: CanonicalName.GlobalID);
            _context = new Context(this);
            _dataobjectEngines = new List<IDataObjectEngine>();
            _code = new Dictionary<string, ICodeBit>();
            _repository = new Repository(engine: this);
            // Events
            SubScopes.CollectionChanged += Scope_CollectionChanged;
            this.PropertyChanged += Scope_PropertyChanged;
            // Register with Data types
            DataType.OnCreation += Scope_DataTypeOnCreation;
            DataType.OnRemoval += Scope_DataTypeOnRemoval;
        }

        ///
        /// Properties
        /// 
#region "Properties"

        /// <summary>
        /// gets the unique handle of the engine
        /// </summary>
        public string Id { get { return _id; } }
        /// <summary>
        /// gets the global scope ID
        /// </summary>
        string IScope.Id { get { return CanonicalName.GlobalID; } }
        /// <summary>
        /// returns the Toplevel Repository of the Engine
        /// </summary>
        public IRepository Repository { get { return _repository; } }
        /// <summary>
        /// returns the Toplevel Scope
        /// </summary>
        public IScope GlobalScope { get { return (IScope) this; } }
        /// <summary>
        /// gets the list of data object engines
        /// </summary>
        public IEnumerable<IDataObjectEngine> DataObjectEngines { get { return _dataobjectEngines; } }
        /// <summary>
        /// gets the initialized status
        /// </summary>
        public bool IsInitialized
        {
            get { return _isInitialized; }
            private set { _isInitialized = value; }
        }
        /// <summary>
        /// gets the subscopes
        /// </summary>
        public ObservableCollection<IScope> SubScopes
        {
            get
            {
                return _subscopes;
            }
        }
        /// <summary>
        /// gets the engine of the scope (this)
        /// </summary>
        Engine IScope.Engine
        {
            get
            {
                return this;
            }
            set { throw new InvalidOperationException(); }

        }
        /// <summary>
        /// set and get the parent of the scope
        /// </summary>
        public IScope Parent
        {
            get
            {
                return null;
            }

            set
            {
                throw new InvalidOperationException();
            }
        }
        /// <summary>
        /// gets the canonical name of the scope
        /// </summary>
        CanonicalName IScope.Name
        {
            get
            {
                return CanonicalName.GlobalName;
            }
        }
        #endregion
        /// <summary>
        /// initialize the engine
        /// </summary>
        /// <returns></returns>
        private bool Initialize()
        {
            if (this.IsInitialized) return false;

            /// add all primitve data types
            /// 
            foreach (IDataType aDatatype in PrimitiveType.DataTypes)
                if (!_repository.Has<IDataType>(aDatatype.Signature))
                    _repository.Add(aDatatype);
            // operator
            foreach (IObjectDefinition anOperator in Operator.BuildInOperators())
                if (!_repository.Has<IOperatorDefinition>(anOperator.Signature))
                    _repository.Add(anOperator);

            // to-do:
            /*
            foreach (Function aFunction in Function.BuildInFunctions())
                if (!_functions.ContainsKey(aFunction.Token))
                    _functions.Add(aFunction.Token, aFunction);
            */
            this.IsInitialized = true;
            return true;
        }
        /// <summary>
        /// Add a data object engine
        /// </summary>
        /// <param name="engine"></param>
        /// <returns></returns>
        public bool Add(IDataObjectEngine engine)
        {
            bool result = false;

            if (_dataobjectEngines.Where(x => x.Id == engine.Id).FirstOrDefault() != null)
                throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { engine.Id, "DataEngines" });

            _dataobjectEngines.Add(engine);
            // throw the added event
            if (DataObjectEngineAdded != null) DataObjectEngineAdded(this, new Engine.EventArgs(engine));
            // throw the added event
            if (DataObjectRepositoryAdded != null) DataObjectRepositoryAdded(this, new Engine.EventArgs(engine));
            // add the data object registery
            result &= this.Repository.RegisterDataObjectRepository(engine.Objects);
            // register the modules to the scope
            foreach (CanonicalName aModuleName in engine.Objects.ModuleNames)
                if (!this.HasScope(aModuleName)) this.CreateScope(aModuleName);
            return result;
        }
        /// <summary>
        /// Add a data object engine
        /// </summary>
        /// <param name="engine"></param>
        /// <returns></returns>
        public bool RemoveDataEngine(String id)
        {
            bool result = false;
            IDataObjectEngine aDataEngine = _dataobjectEngines.Where(x => x.Id == id).FirstOrDefault();

            if (aDataEngine != null)
                throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { id, "DataEngines" });


            result &= _dataobjectEngines.Remove(aDataEngine);
            result &= Repository.DeRegisterDataObjectRepository(aDataEngine.Objects);
            // throw the added event
            if (DataObjectEngineAdded != null) DataObjectEngineAdded(this, new Engine.EventArgs(aDataEngine));
            // throw the added event
            if (DataObjectRepositoryAdded != null) DataObjectRepositoryAdded(this, new Engine.EventArgs(aDataEngine));
            return result;
        }

        #region Access
        /// <summary>
        /// gets the ICodeBit of an handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        private ICodeBit GetCode(string handle)
        {
            Initialize();

            if (_code.ContainsKey(handle)) return _code[handle];
            return null;
        }
        /// <summary>
        /// adds or replaces a codebit
        /// </summary>
        /// <param name="theCode"></param>
        /// <returns></returns>
        private bool AddCode(ICodeBit code)
        {
            Initialize();

            if (_code.ContainsKey(code.Handle)) _code.Remove(code.Handle);
            _code.Add(key: code.Handle, value: code);
            return true;
        }

        #endregion

        /// <summary>
        /// generate from a source string a rule and store it
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool Generate(String source)
        {
            Initialize();

            return (this.Generate(new Antlr4.Runtime.AntlrInputStream(source)));
        }
        /// <summary>
        /// Verify a source code and return the Inode
        /// </summary>
        /// <param id="source"></param>
        /// <returns></returns>
        public INode Verify(string source)
        {
            Initialize();

            var aListener = new RulezParser.MessageListener();
            RulezParser.RulezUnitContext aCtx = null;
            try
            {
                var aLexer = new RulezLexer(new Antlr4.Runtime.AntlrInputStream(source));
                // wrap a token-stream around the lexer
                var theTokens = new Antlr4.Runtime.CommonTokenStream(aLexer);
                // create the aParser
                var aParser = new RulezParser(theTokens);
                aParser.Trace = true;
                aParser.Engine = this;
                aParser.AddErrorListener(aListener);
                // step 1: parse
                aCtx = aParser.rulezUnit();
                // step 2: generate the declarations
                var aDeclarator = new XPTDeclarator(aParser);
                Antlr4.Runtime.Tree.ParseTreeWalker.Default.Walk(aDeclarator, aCtx);
                // step 3: generate the XPTree of the code
                var aGenerator = new XPTGenerator(aParser, declaration: aDeclarator);
                Antlr4.Runtime.Tree.ParseTreeWalker.Default.Walk(aGenerator, aCtx);
                // return the XPTree
                if (aCtx != null) return aCtx.XPTreeNode;
                return null;
            }
            catch (Exception ex)
            {
                if (aCtx != null)
                {
                    if (aCtx.XPTreeNode != null)
                    {
                        aCtx.XPTreeNode.Messages.Add(new Message(type: MessageType.Error, message: ex.Message));
                        return aCtx.XPTreeNode;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// generate from a input string a rule by parsing and compiling and store it
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool Generate(Antlr4.Runtime.ICharStream input)
        {
            Initialize();

            var aListener = new RulezParser.MessageListener();
            RulezParser.RulezUnitContext aCtx = null;
            try
            {
                var aLexer = new RulezLexer(input);
                // wrap a token-stream around the lexer
                var theTokens = new Antlr4.Runtime.CommonTokenStream(aLexer);
                // create the aParser
                var aParser = new RulezParser(theTokens);
                aParser.Trace = true;
                aParser.Engine = this;
                aParser.AddErrorListener(aListener);
                // step 1: parse
                aCtx = aParser.rulezUnit();
                // step 2: generate the declarations
                var aDeclarator = new XPTDeclarator(aParser);
                Antlr4.Runtime.Tree.ParseTreeWalker.Default.Walk(aDeclarator, aCtx);
                // step 3: generate the XPTree of the code
                var aGenerator = new XPTGenerator(aParser, declaration: aDeclarator);
                Antlr4.Runtime.Tree.ParseTreeWalker.Default.Walk(aGenerator, aCtx);
                // result -> Generate and store from the XPTree
                return Generate((IRule) aGenerator);

            }
            catch (Exception ex)
            {
                if (aCtx != null)
                {
                    if (aCtx.XPTreeNode != null)
                    {
                        aCtx.XPTreeNode.Messages.Add(new Message(type: MessageType.Error, message: ex.Message));
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// Generate from a rule the intermediate Code and store it
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        public bool Generate(IRule rule)
        {
            Initialize();

            ICodeBit code = null;
            bool result;
            try
            {
                switch (rule.NodeType)
                {
                    // rule rule
                    case otXPTNodeType.SelectionRule:
                        result = Generate((rule as SelectionRule), out code);
                        break;
                    // no theCode
                    default:
                        throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { rule.NodeType.ToString(), "IRULE" });
                }

                // if successfull
                if (result)
                {
                    rule.RuleState = otRuleState.GeneratedCode;
                    //  get the handle
                    if (code != null && !String.IsNullOrEmpty(code.Handle)) code.Handle = rule.Handle;
                    // add it to the code base
                    if (code != null && !String.IsNullOrEmpty(code.Handle)) AddCode(code);
                    else throw new RulezException(RulezException.Types.HandleNotDefined, arguments: new object[] { rule.Id });
                }
                return result;

            }
            catch (Exception ex)
            {
                throw new RulezException(RulezException.Types.GenerateFailed, inner: ex);
            }
        }

        /// <summary>
        /// generate theCode for a rule rule
        /// </summary>
        /// <param name="rule"></param>
        /// <returns></returns>
        public bool Generate(SelectionRule rule, out ICodeBit code)
        {
            Initialize();

            try
            {
                bool result = true;
                code = null;

                // check if the object to which data engine
                foreach (IDataObjectEngine aDataEngine in _dataobjectEngines.Reverse<IDataObjectEngine>())
                {
                    foreach (String aName in rule.ResultObjectIds())
                        result &= aDataEngine.Objects.HasObjectDefinition(ObjectName.From(aName));
                    if (result)
                        return aDataEngine.Generate((rule as eXPressionTree.IRule), out code);
                }

                // failure
                if (!result)
                {
                    String theNames = DataType.ToString(rule.ResultObjectIds());
                    throw new RulezException(RulezException.Types.NoDataEngineAvailable, arguments: new object[] { theNames });
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new RulezException(RulezException.Types.GenerateFailed, inner: ex);
            }
        }
        /// <summary>
        /// run a rule rule and return an ienumerable of IDataObjects
        /// </summary>
        /// <param name="ruleid"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public IEnumerable<IDataObject> RunSelectionRule(string ruleid, params object[] parameters)
        {
            Initialize();

            ISelectionRule aRule = this.Get<ISelectionRule>(new CanonicalName(ruleid)).First();
            // search the rule
            if (aRule == null)
                throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { ruleid, "SelectionRule" });
            // not the required number of arguments
            if (parameters.Length != aRule.Parameters.Count())
                throw new RulezException(RulezException.Types.InvalidNumberOfArguments, arguments: new object[] { "SelectionRule", ruleid, aRule.Parameters.Count(), parameters.Length });
            // get the Codebit
            ICodeBit theCode = this.GetCode(aRule.Handle);
            if (theCode == null) throw new RulezException(RulezException.Types.HandleNotDefined, arguments: new object[] { aRule.Id });
            if (theCode.Code == null) throw new RulezException(RulezException.Types.InvalidCode, arguments: new object[] { aRule.Id, aRule.Handle });
            // push the arguments
            _context.PushParameters(parameters);
            try
            {
                // run the theCode
                if (theCode.Code(_context) == false) return null;
                // pop result
                IEnumerable<IDataObject> result = (_context.Pop() as IEnumerable<IDataObject>);
                return result;
            }
            catch (RulezException ex)
            {
                throw new RulezException(RulezException.Types.RunFailed, inner: ex, message: ex.Message);
            }
            catch (Exception ex)
            {
                throw new RulezException(RulezException.Types.RunFailed, inner: ex);
            }
        }

        #region Scope
        /// <summary>
        /// returns true if the scope Entries exists in the Global Scope
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasScope(string id)
        {
            Initialize();
            if (String.Compare(GlobalScope.Id, id, ignoreCase: true) == 00) return true;
            // define Visitor and return the REsult
            var aVisitor = new Scope.Visitor<bool>();
            Scope.Visitor<bool>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (String.Compare(id, e.Current.Id, ignoreCase: true) == 00)
                    e.Result = true;
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);
            return aVisitor.Result;
        }
        /// <summary>
        /// returns true if the scope by name exists in this scope
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasScope(CanonicalName name)
        {
            Initialize();
            return HasScope(name.FullId);
        }
        /// <summary>
        /// returns the Scope Object of an given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IScope GetScope(string id)
        {
            Initialize();
            if (String.Compare(GlobalScope.Id, id, true) == 00) return GlobalScope;
            // define Visitor and return the REsult
            var aVisitor = new Scope.Visitor<List<IScope>>();
            Scope.Visitor<List<IScope>>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (String.Compare(id, e.Current.Id, ignoreCase: true) == 00)
                    e.Result.Add(e.Current);
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);
            return aVisitor.Result.FirstOrDefault();
        }
        /// <summary>
        /// returns the Scope Object of an given ID
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IScope GetScope(CanonicalName name)
        {
            Initialize();
            return GetScope(name.FullId);
        }
        /// <summary>
        /// create a scope in the scope tree by id
        /// a.b.c -> will lead to create a with b as sub and c as sub to b
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IScope CreateScope(string id)
        {
            return CreateScope(new CanonicalName(id));
        }
        public IScope CreateScope(CanonicalName name)
        {
            Initialize();
            IScope aScope;
            // todo: error condition
            if (!GlobalScope.AddScope(name)) return null;

            aScope = GlobalScope.GetScope(name);
            this.DataObjectRepositoryAdded += aScope.Scope_DataObjectRepositoryAdded;
           
            // register all scopes 
            do
            {
                // add the known data object repositories
                foreach (IDataObjectRepository aR in this._dataobjectEngines)
                    aScope.Repository.RegisterDataObjectRepository(aR);
                // next scope
                aScope = GlobalScope.GetScope(aScope.Name.Pop());
            } while (aScope != GlobalScope && aScope != null);
            // return the original scope
            return GlobalScope.GetScope(name);
        }
        /// <summary>
        /// returns true if scope has sub scope of id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasSubScope(string id)
        {
            Initialize();
            return _subscopes.Where(x => String.Compare(id, x.Id, ignoreCase: true) == 0).FirstOrDefault() != null;
        }
        /// <summary>
        /// returns the subscope of id or null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IScope GetSubScope(string id)
        {
            Initialize();
            return _subscopes.Where(x => String.Compare(id, x.Id, ignoreCase: true) == 0).FirstOrDefault();
        }
        /// <summary>
        /// adds subscope
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IScope AddSubScope(string id)
        {
            Initialize();
            // add the scope
            if (!HasSubScope(id))
            {
                this.SubScopes.Add(CreateScope(id));
            }
            // return the last scope
            return this.GetSubScope(id);
        }
        /// <summary>
        /// adds the scope to the scope of this scope
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public bool AddScope(IScope scope)
        {
            Initialize();
            if (!HasScope(scope.Name))
            {
                IScope aSub;
                CanonicalName normalized = scope.Name.Reduce(((IScope) this).Name);
                if (!HasSubScope(normalized.IDs.First()))
                    aSub = CreateScope(CanonicalName.Push(this.Id, normalized.IDs.First()));
                else aSub = GetSubScope(normalized.IDs.First());

                return aSub.AddScope(scope);
            }
            return false;
        }
        /// <summary>
        /// creates an scope object of given id and adds it to this scope
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool AddScope(string id)
        {
            Initialize();
            return AddScope(NewScope(id));
        }
        /// <summary>
        /// creates an scope object of given name and adds it to this scope
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        public bool AddScope(CanonicalName name)
        {
            Initialize();
            return AddScope(NewScope(name));
        }
        /// <summary>
        /// gets the root scope
        /// </summary>
        /// <returns></returns>
        public IScope GetRoot()
        {
            return this;
        }
        /// <summary>
        /// return a new Scope object
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IScope NewScope(string id)
        {
            return new Scope(engine: this, id: id);
        }
        /// <summary>
        /// return a new Scope object
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IScope NewScope(CanonicalName name)
        {
            return new Scope(engine: this, name: name);
        }
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
                        aScope.Engine = this;
                    }
                }
        }
        /// <summary>
        /// event handler for adding data object repositories
        /// </summary>
        /// <param name="sender"></param>

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
            if ((args.Engine == null || args.Engine == this) && String.Compare(args.DataType.Name.ModuleId, this.Id, ignoreCase: true) == 00)
                this.Repository.Add(args.DataType);
        }
        /// <summary>
        ///  event Handling routine of Datatype On Removal Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public virtual void Scope_DataTypeOnRemoval(object sender, Core.DataType.EventArgs args)
        {
            // remove
            if ((args.Engine == null || args.Engine == this) && String.Compare(args.DataType.Name.ModuleId, this.Id, true) == 00)
                this.Repository.Remove(args.DataType.Signature);
        }
        /// <summary>
        /// register a data Object Repository with the engine
        /// </summary>
        /// <param name="dataObjectRepository"></param>
        /// <returns></returns>
        public bool RegisterDataObjectRepository(IDataObjectRepository dataObjectRepository)
        {
            Initialize();

            // define Visitor and return the REsult
            var aVisitor = new Scope.Visitor<bool>();
            Scope.Visitor<bool>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.RegisterDataObjectRepository(dataObjectRepository))
                    e.Result = true;
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);

            // return
            return aVisitor.Result;
        }
        /// <summary>
        /// deregister a data object repository with the engine
        /// </summary>
        /// <param name="dataObjectRepository"></param>
        /// <returns></returns>
        public bool DeRegisterDataObjectRepository(IDataObjectRepository dataObjectRepository)
        {
            Initialize();

            // define Visitor and return the REsult
            var aVisitor = new Scope.Visitor<bool>();
            Scope.Visitor<bool>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.DeRegisterDataObjectRepository(dataObjectRepository))
                    e.Result = true;
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);

            // return
            return aVisitor.Result;
        }
        /// <summary>
        /// add the ISigned object to the top level repository
        /// </summary>
        /// <param name="signed"></param>
        /// <returns></returns>
        public bool Add(ISigned signed)
        {
            Initialize();

            // add a scope to the scope
            if (signed.GetType().GetInterface(name: typeof(IScope).Name) != null)
                this.AddScope((IScope)signed);

            // add to repository
            return _repository.Add(signed);
        }
        /// <summary>
        ///  returns true if an  object  exists in the repositories  with the given signature
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signature"></param>
        /// <returns></returns>
        public bool Has(ISignature signature)
        {
            Initialize();

            // check the global
            if (_repository.Has(signature)) return true;

            // define Visitor and return the REsult
            var aVisitor = new Scope.Visitor<bool>();
            Scope.Visitor<bool>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.Has(signature)) e.Result = true;
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);

            // return
            return aVisitor.Result;
        }
        /// <summary>
        ///  returns true if an ISigned derived object of T exists in the repositories optional with the given signature
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signature"></param>
        /// <returns></returns>
        public bool Has<T>(ISignature signature = null) where T : ISigned
        {
            Initialize();

            // check the global
            if (_repository.Has<T>(signature)) return true;

            // define Visitor and return the REsult
            var aVisitor = new Scope.Visitor<bool>();
            Scope.Visitor<bool>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.Has<T>(signature)) e.Result = true;
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);

            // return
            return aVisitor.Result;
        }
        /// <summary>
        /// returns true if an object with the canonical name exists in the repositories
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Has(CanonicalName name)
        {
            Initialize();

            // check the global
            if (_repository.Has(name: name)) return true;

            // check the scope path
            var aScopeName = new CanonicalName(name.Pop());
            if (aScopeName != null && aScopeName != CanonicalName.GlobalName && this.HasScope(aScopeName))
                if (this.GetScope(aScopeName).Has(name)) return true;

            // define Visitor and return the REsult
            var aVisitor = new Scope.Visitor<bool>();
            Scope.Visitor<bool>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.Has(name)) e.Result = true;
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);

            // return
            return aVisitor.Result;
        }
        /// <summary>
        /// returns true if an Isigned derived object T exists in the engine by name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Has<T>(CanonicalName name) where T : ISigned
        {
            Initialize();

            // check the global
            if (_repository.Has<T>(name: name)) return true;

            // check the scope path
            var aScopeName = new CanonicalName(name.Pop());
            if (aScopeName != null && aScopeName != CanonicalName.GlobalName && this.HasScope(aScopeName))
                if (this.GetScope(aScopeName).Has<T>(name)) return true;

            // define Visitor and return the REsult
            var aVisitor = new Scope.Visitor<bool>();
            Scope.Visitor<bool>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.Has<T>(name)) e.Result = true;
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);

            // return
            return aVisitor.Result;
        }
        /// <summary>
        /// gets a list of all Isigned objects by signature in the engine
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        public IList<ISigned> Get(ISignature signature)
        {
            Initialize();

            IList<ISigned> aList = new List<ISigned>();

            // define Visitor and return the REsult
            var aVisitor = new Scope.Visitor<List<ISigned>>();
            Scope.Visitor<List<ISigned>>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.Has<ISigned>(signature))
                    e.Result.AddRange(e.Current.Get<ISigned>(signature));
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);

            // return
            return aVisitor.Result;
        }
        /// <summary>
        /// gets a list of Isigned derived objects from type T with the optional signature
        /// in the engine
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="signature"></param>
        /// <returns></returns>
        public IList<T> Get<T>(ISignature signature = null) where T : ISigned
        {
            Initialize();

            IList<T> aList = new List<T>();

            // define Visitor and return the REsult
            var aVisitor = new Scope.Visitor<List<T>>();
            Scope.Visitor<List<T>>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.Has<T>(signature))
                    e.Result.AddRange(e.Current.Get<T>(signature));
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);

            // return
            return aVisitor.Result;
        }
        /// <summary>
        /// gets an list of Isigned derived objects from the repositories by canonical name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public IList<T> Get<T>(CanonicalName name) where T : ISigned
        {
            Initialize();

            IList<T> aList = new List<T>();

            // check the scope path
            var aScopeName = new CanonicalName(name.Pop());
            if (aScopeName != null && aScopeName != CanonicalName.GlobalName && this.HasScope(aScopeName))
                if (this.GetScope(aScopeName).Has<T>(name)) return this.GetScope(aScopeName).Get<T>(name);

            // define Visitor and return the REsult
            var aVisitor = new Scope.Visitor<List<T>>();
            Scope.Visitor<List<T>>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.Has<T>(name))
                    e.Result.AddRange(e.Current.Get<T>(name));
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);

            // return
            return aVisitor.Result;
        }
        /// <summary>
        /// remove all isigned objects from the repositories
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        public bool Remove(ISignature signature)
        {
            Initialize();

            // define Visitor and return the REsult
            var aVisitor = new Scope.Visitor<bool>();
            Scope.Visitor<bool>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.Remove(signature))
                    e.Result = true;
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);

            // return
            return aVisitor.Result;
        }
        #endregion
    }

    /// <summary>
    /// runtime Context for storing variables etc.
    /// </summary>
    public class Context
    {
        private readonly Engine _engine; // reference engine
        private readonly Dictionary<String, object> _heap = new Dictionary<string, object>();
        private readonly Stack<Object> _stack = new Stack<Object>();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="engine"></param>
        public Context(Engine engine = null)
        {
            if (engine != null) _engine = engine;
            else _engine = Rules.Engine;
        }

        /// <summary>
        /// gets the Stack of the Context
        /// </summary>
        public Stack<Object> Stack { get { return _stack; } }
        /// <summary>
        /// pop from stack
        /// </summary>
        /// <returns></returns>
        public Object Pop()
        {
            if (_stack.Count > 0) return _stack.Pop();
            throw new RulezException(RulezException.Types.StackUnderFlow, arguments: new Object[] { 1, _stack.Count });
        }
        /// <summary>
        /// pops no arguments from the stack as an array
        /// </summary>
        /// <param name="no"></param>
        /// <returns></returns>
        public Object[] PopParameters(uint no)
        {
            if (no > _stack.Count)
            {
                Object[] arr = { };
                Array.Resize<object>(ref arr, (int) no);
                for (uint i = no; i > 0; i--) arr[i - 1] = _stack.Pop();
                return arr;
            }
            else throw new RulezException(RulezException.Types.StackUnderFlow, arguments: new Object[] { no, _stack.Count });

        }
        /// <summary>
        /// push an array on the stack - item by item
        /// </summary>
        /// <param name="no"></param>
        /// <returns></returns>
        public void PushParameters(object[] parameters)
        {
            if (parameters == null) return;
            for (Int16 i = 0; i < parameters.Length; i++) _stack.Push(parameters[i]);
        }
        /// <summary>
        /// push element on stack
        /// </summary>
        /// <param name="item"></param>
        public void Push(object item)
        {
            _stack.Push(item);
        }
        /// <summary>
        /// returns true if the heap has the handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool HasItem(string id)
        {
            return _heap.ContainsKey(id);
        }
        /// <summary>
        /// returns true if item was added 
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool AddItem(string id, object value)
        {
            if (this.HasItem(id)) return false;
            _heap.Add(id, value);
            return true;
        }
        /// <summary>
        /// returns true if item was added 
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public Object Item(string id)
        {
            if (!this.HasItem(id)) return null;
            return _heap[id];
        }
        /// <summary>
        /// returns true if item was replaced 
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool ReplaceItem(string id, object value)
        {
            if (!this.HasItem(id)) return false;
            this.RemoveItem(id);
            _heap.Add(id, value);
            return true;
        }
        /// <summary>
        /// returns true if the item by handle was removed
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool RemoveItem(string id)
        {
            if (this.HasItem(id)) return false;
            _heap.Remove(id);
            return true;
        }

        /// <summary>
        /// return the item names
        /// </summary>
        public List<String> Itemnames { get { return _heap.Keys.ToList(); } }
        /// <summary>
        /// return the item values
        /// </summary>
        public List<Object> Itemvalues { get { return _heap.Values.ToList(); } }
    }

    /// <summary>
    /// theCode bit is an executable object to run a rule
    /// </summary>
    public class CodeBit : ICodeBit
    {
        protected string _handle;
        protected object _tag;
        protected Func<Context, bool> _code;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="handle"></param>
        public CodeBit(string handle = null)
        {
            if (handle == null) _handle = new Guid().ToString();
        }
        /// <summary>
        /// gets the unique handle of the codebit
        /// </summary>
        public string Handle { get { return _handle; } set { _handle = value; } }

        /// <summary>
        /// gets and set an arbitary object for the theCode generator
        /// </summary>
        public object Tag { get { return _tag; } set { _tag = value; } }

        /// <summary>
        /// gets and sets the executable theCode
        /// </summary>
        public Func<Context, bool> Code { get { return _code; } set { _code = value; } }

    }
}