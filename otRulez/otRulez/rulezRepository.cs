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
            /// <summary>
            /// static - must be ascending and discrete ! (do not leave one out !!)
            /// </summary>
            public  const uint POS = 0;
            public  const uint AND = 1;
            public  const uint ANDALSO = 2;
            public  const uint OR = 3;
            public  const uint ORELSE = 4;
            public  const uint NOT = 5;

            public  const uint EQ = 10;
            public  const uint NEQ = 11;
            public  const uint GT = 12;
            public  const uint GE = 13;
            public  const uint LT = 14;
            public  const uint LE = 15;

            public  const uint PLUS = 16;
            public  const uint MINUS = 17;
            public  const uint MULT = 18;
            public  const uint DIV = 19;
            public  const uint MOD = 20;
            public  const uint CONCAT = 21; // Concat must be the last one for functions to be found

            public  const uint BEEP = 22;

            private static string[] _ids = {"POS", "AND", "ANDALSO", "OR", "ORELSE", "NOT", "","","","",
                                            "=", "!=", "GT", "GE", "LT", "LE", "+", "-", "*", "/", "MOD", "CONCAT", 
                                            "BEEP"};
            /// <summary>
            /// variable
            /// </summary>
            private uint _token;

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
                if (((object)a == null) || ((object)b == null))
                {
                    return false;
                }

                // Return true if the fields match:
                return a.ToUint == b.ToUint ;
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
                    return this.CompareTo ((Token) obj) == 0;
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
                if ( this.ToUint <= _ids.GetUpperBound (0) ) return  "'" + _ids[this.ToUint ]+"'";
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
        private static List<@Function> _buildInFunctions = new List<@Function>();
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
        private Token _token;
        private string _signature;
        private IDataType _returntype;
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
            _signature  = signature;
            _returntype = returnType;
        }
        public @Function(uint tokenID, string signature, IDataType returnType)
        {
            _token = new Token(tokenID);
            _signature  = signature;
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
        public IDataType ReturnType { get { return _returntype; } set { _returntype = value; } }
        #endregion
        /// <summary>
        /// implementation of comparable
        /// </summary>
        /// <param name="obj"></param>
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
            return (int)this.Token.GetHashCode();
        }
        /// <summary>
        /// To string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Token.ToString()+"<" + this.Signature + ">";
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
                return this.CompareTo((@Function)obj) == 0;
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
        private static Operator[] _buildInOperators = {

                                                  // logical Operations
                                                  new Operator(Token.POS,1,7,otDataType .Bool ,  otOperatorType.Logical  ) ,
                                                  new Operator(Token.AND,2,5,  otDataType .Bool , otOperatorType.Logical ) ,
                                                  new Operator(Token.ANDALSO,2,5 ,  otDataType .Bool, otOperatorType.Logical ) ,
                                                  new Operator(Token.OR,2,6,  otDataType .Bool , otOperatorType.Logical ) ,
                                                  new Operator(Token.ORELSE,2,6,  otDataType .Bool , otOperatorType.Logical ) ,
                                                  new Operator(Token.NOT,1,7, otDataType .Bool, otOperatorType.Logical   ) ,
                                                  new Operator(Token.EQ,2,8,  otDataType .Bool , otOperatorType.Compare ) ,
                                                  new Operator(Token.NEQ,2,8,  otDataType .Bool , otOperatorType.Compare ) ,
                                                  new Operator(Token.GT,2,8,  otDataType .Bool , otOperatorType.Compare ) ,
                                                  new Operator(Token.GE,2,8,  otDataType .Bool, otOperatorType.Compare  ) ,
                                                  new Operator(Token.LT,2,8,  otDataType .Bool , otOperatorType.Compare ) ,
                                                  new Operator(Token.LE,2,8,  otDataType .Bool , otOperatorType.Compare ) ,

                                                  // Arithmetic - null means return type is determined by the operands
                                                  new Operator(Token.PLUS,2,2,  null , otOperatorType.Arithmetic ) ,
                                                  new Operator(Token.MINUS,2,2,  null , otOperatorType.Arithmetic ) ,
                                                  new Operator(Token.MULT,2,1,  null , otOperatorType.Arithmetic ) ,
                                                  new Operator(Token.DIV,2,1,  null , otOperatorType.Arithmetic ) ,
                                                  new Operator(Token.MOD,2,1,  null , otOperatorType.Arithmetic ) ,
                                                  new Operator(Token.CONCAT,2,1,  null , otOperatorType.Arithmetic ) ,
                                               
        };

        /// <summary>
        /// inner variables
        /// </summary>
        private Token _token;
        private UInt16 _arguments;
        private UInt16 _priority;
        private IDataType  _returntype;
        private otOperatorType _type;

        /// <summary>
        /// returns a List of BuildInFunctions
        /// </summary>
        /// <returns></returns>
        public static List<Operator> BuildInOperators()
        {
            return _buildInOperators.ToList();
        }
        /// <summary>
        /// return the Operator Definition
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Operator GetOperator(Token token)
        {
            Operator o =  _buildInOperators.Where( x => x.Token == token).FirstOrDefault();
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
            if (tokenid < _buildInOperators.Length) return _buildInOperators[tokenid];
            throw new RulezException(RulezException.Types.OutOfArraySize, arguments: new object[] { tokenid, _buildInOperators.Length });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static string CreateSignature(Token token, UInt16 arguments, UInt16 priority, IDataType returnType)
        {
            return token.ToString() + "<" + arguments.ToString() + "," + priority.ToString() + "," + (returnType == null ? returnType.ToString() :"*") + ">";
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="arguments"></param>
        /// <param name="priority"></param>
        public Operator(Token token, UInt16 arguments, UInt16 priority, otDataType? returnTypeId, otOperatorType type)
        {
            _token = token;
            _arguments = arguments;
            _priority = priority;
            if (returnTypeId.HasValue ) _returntype = DataType.GetDataType (returnTypeId.Value);
            _type = type;
        }
        public Operator(uint tokenID, UInt16 arguments, UInt16 priority, otDataType? returnTypeId, otOperatorType type)
        {
            _token = new Token(tokenID);
            _arguments = arguments;
            _priority = priority;
            if (returnTypeId.HasValue)  _returntype = DataType.GetDataType(returnTypeId.Value); ;
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
        /// gets or sets the return type of the operation
        /// </summary>
        public otDataType? ReturnTypeId { get { return _returntype != null ? _returntype.TypeId : new otDataType? () ; } 
            set { if (value.HasValue )_returntype = DataType.GetDataType(value.Value); } }
        public IDataType ReturnType { get { return _returntype; } set { _returntype = value; } }
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
            return (int)this.Token.GetHashCode();
        }
        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (obj == null || !(obj is Operator))
                return false;
            else
                return this.CompareTo((Operator)obj) == 0;
        }
        /// <summary>
        /// implementation of comparable
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public  int CompareTo(Operator obj)
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
            if (((object)a == null) || ((object)b == null))
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
    /// a repository for the rulez engine
    /// </summary>
    public class Repository 
    {
        private string _id; // ID of the Repository
        private Engine _engine; // my engine
        // Dictionary of operators
        private Dictionary<Token, Operator> _Operators = new Dictionary<Token, Operator>();
        // Dictionary of functions
        private Dictionary<Token, @Function> _Functions = new Dictionary<Token, @Function>();
        // Dictionary of the rule rules
        private Dictionary<String, SelectionRule> _selectionrules = new Dictionary<string, SelectionRule>();
        // Stack of dataObject Repositories
        private List<iDataObjectRepository> _dataobjectRepositories = new List<iDataObjectRepository> ();
        // dictionary of types
        private Dictionary<string, IDataType> _datatypes = new Dictionary<string, IDataType>();
        private Dictionary<string, List<IDataType>> _datatypesSignature = new Dictionary<string, List<IDataType>>();

        // initialize Flag
        private bool _IsInitialized = false;

        /// <summary>
        /// constructor of an engine
        /// </summary>
        public Repository(Engine engine, string id = null)
        {
            if (id == null) _id = Guid.NewGuid().ToString();
            else _id = id;
            _engine = engine;
        }


        #region "Properties"
        /// <summary>
        /// gets the unique handle of the engine
        /// </summary>
        public string Id { get { return _id; } }
        /// <summary>
        /// returns the Engine
        /// </summary>
        public Engine Engine { get { return _engine;  } }
        /// <summary>
        /// gets all the rule rules in the repository
        /// </summary>
        public List<SelectionRule> SelectionRules { get { return _selectionrules.Values.ToList() ; } }
        /// <summary>
        /// gets all rule rule IDs in the repository
        /// </summary>
        public List<String> SelectionRuleIDs { get { return _selectionrules.Keys.ToList (); } }

        /// <summary>
        /// gets all the operators in the repository
        /// </summary>
        public List<Operator> Operators { get { return _Operators.Values.ToList(); } }
        /// <summary>
        /// gets all operator tokens rule IDs in the repository
        /// </summary>
        public List<Token> OperatorTokens { get { return _Operators.Keys.ToList(); } }
        /// <summary>
        /// return true if initialized
        /// </summary>
        public bool IsInitialized { get { return _IsInitialized; } }

        #endregion

        /// <summary>
        /// register the DataObjectEntrySymbol Repository
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public bool RegisterDataObjectRepository(iDataObjectRepository repository)
        {
            _dataobjectRepositories.Add(repository);
            return true;
        }
        /// <summary>
        /// register the DataObjectEntrySymbol Repository
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public bool DeRegisterDataObjectRepository(iDataObjectRepository repository)
        {
            _dataobjectRepositories.Remove(repository);
            return true;
        }
        /// <summary>
        /// lazy initialize
        /// </summary>
        /// <returns></returns>
        private bool Initialize()
        {
            if (_IsInitialized) return false;
            // Register with Data types
            DataType.OnCreation += Repository_DataTypeOnCreation;
            DataType.OnRemoval += Repository_DataTypeOnRemoval;
            // operator
            foreach (Operator anOperator in Operator.BuildInOperators())
            {
                if (! _Operators.ContainsKey(anOperator.Token)) 
                    _Operators.Add(anOperator.Token, anOperator);
            }

            // Functions
            foreach (@Function aFunction in @Function.BuildInFunctions())
            {
                if (!_Functions.ContainsKey(aFunction.Token))
                    _Functions.Add(aFunction.Token, aFunction);
            }
            // primitve Datatypes
            foreach (IDataType aDatatype in PrimitiveType.DataTypes)
            {
                if (!_datatypes.ContainsKey(aDatatype.Name.ToUpper()))
                {
                    _datatypes.Add(aDatatype.Name.ToUpper(), aDatatype);
                    if (! _datatypesSignature.ContainsKey(aDatatype.Signature.ToUpper())) _datatypesSignature.Add(aDatatype.Signature.ToUpper(), new List<IDataType>());
                    List<IDataType> aList = _datatypesSignature[aDatatype.Signature.ToUpper()];
                    // remove all existing
                    aList.RemoveAll(x => x.Name == aDatatype.Name);
                    aList.Add(aDatatype);
                }
            }
            _IsInitialized = true;
            return _IsInitialized;
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
            if (this.HasSelectionRule (id)) return _selectionrules[id];
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
            if (this.HasSelectionRule(id)) _selectionrules.Remove(id);
            _selectionrules.Add(id, rule);
            return true;
        }
        /// <summary>
        /// adds a rule rule to the repository by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public bool RemoveSelectionRule(string id)
        {
            Initialize();
            if (this.HasSelectionRule(id)) return _selectionrules.Remove(id);
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
            return _Functions.ContainsKey(id);
        }
        /// <summary>
        /// returns the function by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public @Function GetFunction(Token id)
        {
            Initialize();
            if (this.HasFunction(id)) return _Functions[id];
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
            if (this.HasFunction(function.Token)) _Functions.Remove(function.Token);
            _Functions.Add(function.Token, function);
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
            if (id != null) return _Operators.ContainsKey(id);
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
            if (this.HasOperator(id)) return _Operators[id];
            throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[]{id.ToString (), "Operator"});
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
            if (this.HasOperator(Operator.Token)) _Operators.Remove(Operator.Token);
            _Operators.Add(Operator.Token, Operator);
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
            if (this.HasOperator(id)) return _Operators.Remove(id);
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
            return _datatypes.ContainsKey(name.ToUpper() );
        }
        /// <summary>
        /// returns true if the repository has the function
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool HasDataType(IDataType datatype)
        {
            return HasDataType(datatype.Name);
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
            if (this.HasDataType(Name)) return _datatypes[Name.ToUpper()];
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
            if (this.HasDataTypeSignature(signature)) return _datatypesSignature[signature.ToUpper()];
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
            if (this.HasDataType(datatype.Name)) _datatypes.Remove(datatype.Name.ToUpper());
            _datatypes.Add(datatype.Name.ToUpper(), datatype);
            if (!this.HasDataTypeSignature(datatype.Signature)) _datatypesSignature.Add(datatype.Signature.ToUpper(), new List<IDataType>());
            List<IDataType> aList = _datatypesSignature[datatype.Signature.ToUpper()];
            // remove all existing
            aList.RemoveAll(x => x.Name.ToUpper() == datatype.Name.ToUpper());
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
            if (this.HasDataType(datatype.Name)) _datatypes.Remove(datatype.Name.ToUpper());
            if (this.HasDataTypeSignature(datatype.Signature))
            {
                List<IDataType> aList = _datatypesSignature[datatype.Signature.ToUpper()];
                // remove all existing
                aList.RemoveAll(x => x.Name.ToUpper() == datatype.Name.ToUpper());
            }
            return true;
        }
        /// <summary>
        /// returns true if the id exists in the Repository
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasDataObjectDefinition(string id)
        {
            Initialize();
            foreach (iDataObjectRepository aRepository in _dataobjectRepositories)
            {
                if (aRepository.HasObjectDefinition(id)) return true;
            }
            return false;
        }
        /// <summary>
        /// returns the selectionrule by handle
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public iObjectDefinition GetDataObjectDefinition(String id)
        {
            Initialize();
            foreach (iDataObjectRepository aRepository in _dataobjectRepositories)
            {
                iObjectDefinition aDefinition = aRepository.GetIObjectDefinition(id);
                if (aDefinition != null) return aDefinition;
            }
            throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { id, "DataObjectEntrySymbol Repositories" });
        }
        /// <summary>
        /// event Handling routine of Datatype On Creation Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Repository_DataTypeOnCreation(object sender, Core.DataType.EventArgs args)
        {
            if (( args.Engine == null || args.Engine == this.Engine) && !this.HasDataType (args.DataType)) this.AddDataType(args.DataType);
        }
        /// <summary>
        ///  event Handling routine of Datatype On Removal Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Repository_DataTypeOnRemoval(object sender, Core.DataType.EventArgs args)
        {
            if ((args.Engine == null || args.Engine == this.Engine) && this.HasDataType(args.DataType)) this.RemoveDataType(args.DataType);
        }
    }
}