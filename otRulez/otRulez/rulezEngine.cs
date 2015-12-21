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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using OnTrack.Core;
using OnTrack.Rulez.eXPressionTree;

namespace OnTrack.Rulez
{
    /// <summary>
    /// an engine for running rulez
    /// </summary>
    public class Engine
    {
        public class  EventArgs :  System.EventArgs
        {
            private iDataObjectEngine _engine;
            
            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="engine"></param>
            public EventArgs(iDataObjectEngine engine)
            {
                _engine = engine;
            }
            /// <summary>
            /// gets the engine
            /// </summary>
            public iDataObjectEngine Engine { get { return _engine;}}
            /// <summary>
            /// gets the repository
            /// </summary>
            public iDataObjectRepository DataObjectRepository { get { return (_engine != null ) ? _engine.Objects : null; ; } }
        }
        private Scope _globalScope;
        private string _id; // handle of the engine
        private Context _context;
        private Dictionary<String, ICodeBit> _Code; // Code Dictionary
        private List<iDataObjectEngine> _dataobjectEngines; // DataObject Engines for running data object against

        /// events
        public event EventHandler<EventArgs> DataObjectRepositoryAdded;
        public event EventHandler<EventArgs> DataObjectEngineAdded;
        public event EventHandler<EventArgs> DataObjectRepositoryRemoved;
        public event EventHandler<EventArgs> DataObjectEngineRemoved;
        /// <summary>
        /// constructor of an engine
        /// </summary>
        public Engine (string id = null)
        {
            if (id == null) _id = System.Environment.MachineName  + "_" + DateTime.Now.ToString("o");
            else _id = id;
            _globalScope = new Scope(engine: this, id: CanonicalName.GlobalID);
            _context = new Context(this);
            _dataobjectEngines = new List<iDataObjectEngine>();
            _Code = new Dictionary<string, ICodeBit>();
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
        /// returns the Toplevel Repository of the Engine
        /// </summary>
        public IRepository Globals { get { return GlobalScope.Repository; } }
        /// <summary>
        /// returns the Toplevel Scope
        /// </summary>
        public Scope GlobalScope { get { return _globalScope; } }
        /// <summary>
        /// gets the list of data object engines
        /// </summary>
        public IEnumerable<iDataObjectEngine> DataObjectEngines { get { return _dataobjectEngines; } }
        
#endregion

        /// <summary>
        /// Add a data object engine
        /// </summary>
        /// <param name="engine"></param>
        /// <returns></returns>
        public bool AddDataEngine(iDataObjectEngine engine)
        {
            Boolean result = false;

            if  (_dataobjectEngines.Where (x => x.ID == engine.ID).FirstOrDefault () != null)
                throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { engine.ID , "DataEngines"});

            _dataobjectEngines.Add(engine);
            // throw the added event
            if (DataObjectEngineAdded != null) DataObjectEngineAdded(this, new Engine.EventArgs(engine));
            // throw the added event
            if (DataObjectRepositoryAdded != null) DataObjectRepositoryAdded(this, new Engine.EventArgs(engine));
            // add the data object registery
            result &= Globals .RegisterDataObjectRepository (engine.Objects );
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
            Boolean result = false;
            iDataObjectEngine aDataEngine = _dataobjectEngines.Where(x => x.ID == id).FirstOrDefault();

            if (aDataEngine != null)
                throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { id, "DataEngines" });


            result &= _dataobjectEngines.Remove(aDataEngine);
            result &= Globals.DeRegisterDataObjectRepository(aDataEngine.Objects);
            // throw the added event
            if (DataObjectEngineAdded != null) DataObjectEngineAdded(this, new Engine.EventArgs(aDataEngine));
            // throw the added event
            if (DataObjectRepositoryAdded != null) DataObjectRepositoryAdded(this, new Engine.EventArgs(aDataEngine));
            return result;
        }
        /// <summary>
        /// returns true if the scope Entries exists in the Global Scope
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasScope(string id)
        {
            if (String.Compare (GlobalScope.Id, id, true)==00) return true;
            // define Visitor and return the REsult
            Scope.Visitor<Boolean> aVisitor = new Scope.Visitor<Boolean>();
            Scope.Visitor<Boolean>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (String.Compare(id, e.Current.Id , true) == 00) e.Result = true;
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);
            return aVisitor.Result;
        }
         public bool HasScope(CanonicalName name)
        {
            return HasScope(name.FullId);
        }
        /// <summary>
        /// returns the Scope Object of an given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IScope GetScope(string id)
        {
            if (String.Compare(GlobalScope.Id, id, true) == 00) return GlobalScope;
            // define Visitor and return the REsult
            Scope.Visitor<List<IScope>> aVisitor = new Scope.Visitor<List<IScope>>();
            Scope.Visitor<List<IScope>>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (String.Compare(id, e.Current.Id, true) == 00) e.Result.Add(e.Current);
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);
            return aVisitor.Result.FirstOrDefault ();
        }
        /// <summary>
        /// returns the Scope Object of an given ID
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IScope GetScope(CanonicalName name)
        {
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
            IScope aScope;
            // todo: error condition
            if (!GlobalScope.AddScope(name)) return null;

            aScope = GlobalScope.GetScope(name);
            this.DataObjectRepositoryAdded += aScope.Scope_DataObjectRepositoryAdded;

            // register all scopes 
            do
            {
                // add the known data object repositories
                foreach (iDataObjectRepository aR in this._dataobjectEngines)
                  aScope.Repository.RegisterDataObjectRepository(aR);
                // next scope
                aScope = GlobalScope.GetScope(aScope.Name.Pop());
            } while (aScope != GlobalScope && aScope != null);
            // return the original scope
            return GlobalScope.GetScope(name);
        }
        #region Access
        /// <summary>
        /// gets the ICodeBit of an handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        private ICodeBit GetCode(string handle)
        {
            if (_Code.ContainsKey(handle)) return _Code[handle];
            return null;
        }
        /// <summary>
        /// adds or replaces a codebit
        /// </summary>
        /// <param name="theCode"></param>
        /// <returns></returns>
        private bool AddCode(ICodeBit code)
        {
            if (_Code.ContainsKey(code.Handle)) _Code.Remove(code.Handle);
             _Code.Add(key: code.Handle, value: code);
             return true;
        }
        /// <summary>
        /// returns all Rules of a certain id from the scope tree
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public IList <SelectionRule > GetSelectionRules(string id = null)
        {
            if (GlobalScope.HasSelectionRule(id))
            {
                List<SelectionRule> aResult = new List<SelectionRule>();
                aResult.Add(GlobalScope.GetSelectionRule(id));
                return aResult;
            }

           // define Visitor and return the REsult
           Scope.Visitor<List<SelectionRule>> aVisitor = new Scope.Visitor<List<SelectionRule>>();
           Scope.Visitor<List<SelectionRule>>.Eventhandler aVisitingHandling = (o, e) => 
           {
                if (e.Current.HasSelectionRule (id))
                    e.Result.Add(e.Current.GetSelectionRule (id));
           };
           aVisitor.VisitedScope += aVisitingHandling;
           aVisitor.Visit(GlobalScope);
           return aVisitor.Result;
        }
        /// <summary>
        /// returns true if the id exists somewhere in the scope tree
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasSelectionRule(string id)
        {
            if (GlobalScope.HasSelectionRule(id)) return true;
            // define Visitor and return the REsult
            Scope.Visitor<Boolean> aVisitor = new Scope.Visitor<Boolean>();
            Scope.Visitor<Boolean>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.HasSelectionRule(id))  e.Result = true;
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);
            return aVisitor.Result;
        }
        /// <summary>
        /// gets the Operator definition for the Token ID
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public IList<Operator> GetOperators (Token id)
        {
             if (GlobalScope.HasOperator(id))
             {
                 List<Operator> aResult = new List<Operator>();
                 aResult.Add(GlobalScope.GetOperator(id));
                 return aResult;
             }

             // define Visitor and return the REsult
             Scope.Visitor<List<Operator>> aVisitor = new Scope.Visitor<List<Operator>>();
             Scope.Visitor<List<Operator>>.Eventhandler aVisitingHandling = (o, e) =>
             {
                 if (e.Current.HasOperator(id))
                     e.Result.Add(e.Current.GetOperator(id));
             };
             aVisitor.VisitedScope += aVisitingHandling;
             aVisitor.Visit(GlobalScope);
             return aVisitor.Result;
        }
        /// <summary>
        /// returns true if the id exists somewhere in the scope tree
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasOperator(Token id)
        {
            if (GlobalScope.HasOperator(id)) return true;
            // define Visitor and return the REsult
            Scope.Visitor<Boolean> aVisitor = new Scope.Visitor<Boolean>();
            Scope.Visitor<Boolean>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.HasOperator(id)) e.Result = true;
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);
            return aVisitor.Result;
        }
        /// <summary>
        /// gets the Operator definition for the Token ID
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public IList<@Function> GetFunctions(Token id)
        {
            if (GlobalScope.HasFunction(id))
            {
                List<@Function> aResult = new List<@Function>();
                aResult.Add(GlobalScope.GetFunction(id));
                return aResult;
            }

            // define Visitor and return the REsult
            Scope.Visitor<List<@Function>> aVisitor = new Scope.Visitor<List<@Function>>();
            Scope.Visitor<List<@Function>>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.HasFunction(id))
                    e.Result.Add(e.Current.GetFunction(id));
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);
            return aVisitor.Result;
        }
        /// <summary>
        /// returns true if the id exists somewhere in the scope tree
        /// </summary>
        /// <param fullname="id"></param>
        /// <returns></returns>
        public bool HasFunction(Token id)
        {
            if (GlobalScope.HasFunction(id)) return true;
            // define Visitor and return the REsult
            Scope.Visitor<Boolean> aVisitor = new Scope.Visitor<Boolean>();
            Scope.Visitor<Boolean>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.HasFunction(id)) e.Result = true;
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);
            return aVisitor.Result;
        }
        /// <summary>
        /// gets the Operator definition for the Token ID
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public IList<iObjectDefinition> GetDataObjectDefinitions(string id)
        {
            if (GlobalScope.HasDataObjectDefinition (id))
            {
                List<iObjectDefinition> aResult = new List<iObjectDefinition>();
                aResult.Add(GlobalScope.GetDataObjectDefinition(id));
                return aResult;
            }

            // define Visitor and return the REsult
            Scope.Visitor<List<iObjectDefinition>> aVisitor = new Scope.Visitor<List<iObjectDefinition>>();
            Scope.Visitor<List<iObjectDefinition>>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.HasDataObjectDefinition (id))
                    e.Result.Add(e.Current.GetDataObjectDefinition(id));
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);
            return aVisitor.Result;
        }
        /// <summary>
        /// returns true if the id exists somewhere in the scope tree
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasDataObjectDefinition(string id)
        {
            if (GlobalScope.HasDataObjectDefinition(id)) return true;
            // define Visitor and return the REsult
            Scope.Visitor<Boolean> aVisitor = new Scope.Visitor<Boolean>();
            Scope.Visitor<Boolean>.Eventhandler aVisitingHandling = (o, e) =>
            {
                if (e.Current.HasDataObjectDefinition(id)) e.Result = true;
            };
            aVisitor.VisitedScope += aVisitingHandling;
            aVisitor.Visit(GlobalScope);
            return aVisitor.Result;
        }
        #endregion

        /// <summary>
        /// generate from a source string a rule and store it
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool Generate(String source)
        {
            return(this.Generate(new Antlr4.Runtime.AntlrInputStream(source)));
        }
        /// <summary>
        /// Verify a source code and return the Inode
        /// </summary>
        /// <param id="source"></param>
        /// <returns></returns>
        public INode Verify(string source)
        {
            RulezParser.MessageListener aListener = new RulezParser.MessageListener();
            RulezParser.RulezUnitContext aCtx = null;
            try
            {
                RulezLexer aLexer = new RulezLexer(new Antlr4.Runtime.AntlrInputStream(source));
                // wrap a token-stream around the lexer
                Antlr4.Runtime.CommonTokenStream theTokens = new Antlr4.Runtime.CommonTokenStream(aLexer);
                // create the aParser
                RulezParser aParser = new RulezParser(theTokens);
                aParser.Trace = true;
                aParser.Engine = this;
                aParser.AddErrorListener(aListener);
                // step 1: parse
                aCtx = aParser.rulezUnit();
                // step 2: generate the declarations
                XPTDeclGen theDeclGen = new XPTDeclGen(aParser);
                Antlr4.Runtime.Tree.ParseTreeWalker.Default.Walk(theDeclGen, aCtx);
                // step 3: generate the XPTree of the code
                XPTGenerator theXPTGen = new XPTGenerator(aParser);
                Antlr4.Runtime.Tree.ParseTreeWalker.Default.Walk(theXPTGen, aCtx);
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
            RulezParser.MessageListener aListener = new RulezParser.MessageListener();
            RulezParser.RulezUnitContext aCtx = null;
            try
            {
                RulezLexer aLexer = new RulezLexer(input);
                // wrap a token-stream around the lexer
                Antlr4.Runtime.CommonTokenStream theTokens = new Antlr4.Runtime.CommonTokenStream(aLexer);
                // create the aParser
                RulezParser aParser = new RulezParser(theTokens);
                aParser.Trace = true;
                aParser.Engine = this;
                aParser.AddErrorListener(aListener);
                // step 1: parse
                aCtx = aParser.rulezUnit();
                // step 2: generate the declarations
                XPTDeclGen theDeclGen = new XPTDeclGen(aParser);
                Antlr4.Runtime.Tree.ParseTreeWalker.Default.Walk(theDeclGen, aCtx);
                // step 3: generate the XPTree of the code
                XPTGenerator theXPTGen = new XPTGenerator(aParser);
                Antlr4.Runtime.Tree.ParseTreeWalker.Default.Walk(theXPTGen, aCtx);
                // result -> Generate and store from the XPTree
                return Generate((IRule)theXPTGen);

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
        public bool Generate(IRule  rule)
        {
            ICodeBit code=null;
            bool result;
            try
            {
                switch (rule.NodeType)
                {
                    // rule rule
                    case otXPTNodeType.SelectionRule:
                        result= Generate((rule as SelectionRule), out code);
                        break;
                    // no theCode
                    default:
                        throw new RulezException(RulezException.Types.InvalidNodeType, arguments: new object[] { rule.NodeType.ToString(), "IRULE" });
                }

                // if successfull
                if (result) 
                {
                    rule.RuleState = otRuleState.generatedCode ;
                    //  get the handle
                    if (code != null && !String.IsNullOrEmpty(code.Handle)) code.Handle = rule.Handle;
                    // add it to the code base
                    if (code != null &&  !String.IsNullOrEmpty(code.Handle)) AddCode(code);
                    else throw new RulezException(RulezException.Types.HandleNotDefined, arguments: new object[] { rule.ID});
                }
                return result;
               
            } catch (Exception ex )
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
            try
            {
                bool result = true;
                code = null;

                // check if the object to which data engine
                foreach (iDataObjectEngine aDataEngine in _dataobjectEngines.Reverse <iDataObjectEngine > () )
                {
                    foreach (String aName in rule.ResultingObjectnames () ) 
                        result &= aDataEngine.Objects.HasObjectDefinition(ObjectName.From(aName));
                    if (result) 
                        return aDataEngine.Generate((rule as eXPressionTree.IRule), out code);
                }

                // failure
                if (!result)
                {
                    String theNames = DataType.ToString(rule.ResultingObjectnames());
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
        public IEnumerable <iDataObject > RunSelectionRule (string ruleid, params object[] parameters)
        {
            SelectionRule aRule = this.GetSelectionRules(id: ruleid).First();
            // search the rule
            if (aRule == null)
                throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { ruleid, "SelectionRule" });
            // not the required number of arguments
            if (parameters.Length != aRule.Parameters.Count())
                throw new RulezException (RulezException.Types.InvalidNumberOfArguments, arguments: new object[] {"SelectionRule", ruleid, aRule.Parameters .Count(), parameters.Length});
            // get the Codebit
            ICodeBit theCode = this.GetCode(aRule.Handle);
            if (theCode == null) throw new RulezException (RulezException.Types.HandleNotDefined , arguments: new object[] {aRule.ID});
            if (theCode.Code == null) throw new RulezException (RulezException.Types.InvalidCode, arguments: new object[]{aRule.ID, aRule.Handle});
            // push the arguments
            _context.PushParameters(parameters);
            try
            {
                // run the theCode
                if (theCode.Code(_context) == false) return null;
                // pop result
                IEnumerable<iDataObject> result = (_context.Pop() as IEnumerable<iDataObject>);
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

    }

    /// <summary>
    /// runtime Context for storing variables etc.
    /// </summary>
    public class Context
    {
        private Engine _engine ; // reference engine
        private Dictionary <String, object> _heap = new Dictionary<string,object> () ;
        private Stack<Object> _stack = new Stack<Object>();

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
        public Stack<Object> Stack  { get {return _stack;} }
        /// <summary>
        /// pop from stack
        /// </summary>
        /// <returns></returns>
        public Object Pop ()
        {
            if (_stack.Count > 0) return _stack.Pop();
            throw new RulezException(RulezException.Types.StackUnderFlow, arguments: new Object[] { 1, _stack.Count });
        }
        /// <summary>
        /// pops no arguments from the stack as an array
        /// </summary>
        /// <param name="no"></param>
        /// <returns></returns>
        public Object[] PopParameters (uint no)
        {
            if (no > _stack.Count)
            {
                Object[] arr = {};
                Array.Resize<object>(ref arr, (int)no);
                for (uint  i=no;i>0;i--) arr[i-1] = _stack.Pop();
                return arr;
            }else throw new RulezException (RulezException.Types.StackUnderFlow, arguments: new Object[] {no, _stack.Count});
            
        }
        /// <summary>
        /// push an array on the stack - item by item
        /// </summary>
        /// <param name="no"></param>
        /// <returns></returns>
        public void PushParameters (object[] parameters)
        {
            if (parameters==null) return;
            for (Int16 i = 0; i < parameters.Length;i++ ) _stack.Push (parameters[i]);
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
        public bool  HasItem(string id) {
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
        protected Func<Context, Boolean> _code;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="handle"></param>
        public CodeBit (string handle=null)
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
        public Func<Context, Boolean> Code { get { return _code; } set { _code = value; } }
        
    }
}