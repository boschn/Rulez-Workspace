using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTrack.Rulez ;
using OnTrack.Core;
using System.Collections.Generic;
using System.Linq;
using OnTrack.Rulez.eXPressionTree;

namespace OnTrack.Testing
{
    /// <summary>
    /// Test Object Entry Definition
    /// </summary>
    internal class ObjectEntryDefinition : IObjectEntryDefinition 
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="objectdefinition"></param>
        /// <param name="entryname"></param>
        /// <param name="typeId"></param>
        /// <param name="isNull"></param>
        public ObjectEntryDefinition (IObjectDefinition objectdefinition, String entryname, otDataType typeid, bool isNull )
        {
            this.ObjectId = objectdefinition.Id;
            this.EntryId = entryname.ToUpper();
            this.DataType = Core.DataType.GetDataType(typeid) ;
            this.IsNullable = isNull;
            this.ObjectDefinition = objectdefinition ;
        }
        public EntryName Name
        {
            get { return new EntryName(objectid: this.ObjectId, entryid: this.EntryId); }
        }
        public bool IsMapped { get ; set; }

        public long? LowerRangeValue { get ; set; }
        public IDataType DataType { get; set; }
        public long? UpperRangeValue { get ; set; }
        public List<string> PossibleValues { get ; set; }
        public string Description{ get ; set; }
        public string ObjectId { get ; set; }
        public string EntryId { get ; set; }
        public otDataType TypeId { get { return DataType.TypeId; } set { DataType = Core.DataType.GetDataType(value); } }
        public long Version { get ; set; }
       
        public string Title { get ; set; }

        public object DefaultValue { get ; set; }
        
        public bool IsNullable { get ; set; }
        

        public long? PrimaryKeyOrdinal { get ; set; }
       
        public otDataType? InnerDatatype { get ; set; }
       

        public long Ordinal { get ; set; }
       

        public bool IsReadonly { get ; set; }
       

        public bool IsActive { get ; set; }
       

        public IObjectDefinition ObjectDefinition { get ; set; }

        public ISignature Signature
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool Equals(ISigned x, ISigned y)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(ISigned obj)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Test ObjectDefinition
    /// </summary>
    internal class ObjectDefinition : IObjectDefinition
    {
        private OnTrack.Rulez.ObjectName _objectname;
        private Dictionary <String, IObjectEntryDefinition> _entries = new Dictionary<String, IObjectEntryDefinition>();
        private List<string> _keynames = new List<string>();
        /// <summary>
        /// constructor with full canonical id
        /// </summary>
        /// <param name="id"></param>
        public ObjectDefinition(String id)
        {
            _objectname = new ObjectName(id);
        }
        /// <summary>
        /// constructor with object name
        /// </summary>
        /// <param name="name"></param>
        public ObjectDefinition(ObjectName name)
        {
            _objectname = name;
        }
        public bool AddEntry(IObjectEntryDefinition entry)
        {
            if (_entries.ContainsKey (entry.EntryId.ToUpper())) _entries .Remove (entry.EntryId.ToUpper() );

            _entries .Add(entry.EntryId.ToUpper(), entry);
            return true;
        }
        /// <summary>
        /// gets the object name
        /// </summary>
        public ObjectName Name
        {
            get
            {
                return _objectname;
            }
        }
        public string Id
        {
            get
            {
                return _objectname.FullId;
            }
        }
        public Type ObjectType
        {
            get
            {
                // TODO: Implement this property getter
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// gets the ModuleID
        /// </summary>
        public string ModuleId
        {
            get
            {
                return _objectname.ModuleId;
            }
            
        }

        public string Classname
        {
            get
            {
                return _objectname.FullId;
            }
          
        }

        public string Description
        {
            get
            {
                // TODO: Implement this property getter
                throw new NotImplementedException();
            }
            set
            {
                // TODO: Implement this property setter
                throw new NotImplementedException();
            }
        }

        public string[] Properties
        {
            get
            {
                // TODO: Implement this property getter
                throw new NotImplementedException();
            }
            set
            {
                // TODO: Implement this property setter
                throw new NotImplementedException();
            }
        }

        public long Version
        {
            get
            {
                // TODO: Implement this property getter
                throw new NotImplementedException();
            }
            set
            {
                // TODO: Implement this property setter
                throw new NotImplementedException();
            }
        }

        public bool IsActive
        {
            get
            {
                // TODO: Implement this property getter
                throw new NotImplementedException();
            }
            set
            {
                // TODO: Implement this property setter
                throw new NotImplementedException();
            }
        }

        public string[] Keys
        {
            get
            {
                return _keynames.ToArray();
            }
            set
            {
                _keynames = new List<string>();
                foreach (string s in value) _keynames.Add(s.ToUpper());
            }
        }

        public IList<IObjectEntryDefinition> IObjectEntryDefinitions
        {
            get
            {
                return _entries.Values.ToList();
            }
        }

        public ISignature Signature
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IList<string> EntryNameIds(bool onlyActive = true)
        {
            return _entries.Keys.ToList();
        }

        public IObjectEntryDefinition GetiEntryDefinition(string entryname)
        {
            if (this.HasEntry(entryname)) return _entries[entryname.ToUpper()];
            return null;
        }

        public bool HasEntry(string entryname)
        {
           return _entries.ContainsKey(entryname.ToUpper());
        }

        public bool Equals(ISigned x, ISigned y)
        {
            throw new NotImplementedException();
        }

        public int GetHashCode(ISigned obj)
        {
            throw new NotImplementedException();
        }
    }

  
    /// <summary>
    /// Test DataObject Repository
    /// </summary>
    internal  class DataObjectRepository : IDataObjectRepository
    {
        // add
        private Dictionary<ObjectName, IObjectDefinition> _objects = new Dictionary<ObjectName, IObjectDefinition>();
        private Dictionary<Type, String> _types = new Dictionary<Type, String>();

        private List<IDataObjectProvider> _providers = new List<IDataObjectProvider>();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DataObjectRepository()
        {
            ObjectDefinition theDeliverables = new ObjectDefinition("deliverables");
            theDeliverables.AddEntry(new ObjectEntryDefinition(theDeliverables, "UID", otDataType.Number, false));
            theDeliverables.AddEntry(new ObjectEntryDefinition(theDeliverables, "DATE", otDataType.Date, true));
            theDeliverables.Keys = new string[] {"uid"};
            _objects.Add(theDeliverables.Name, theDeliverables);
            // testobject1 class
            ObjectDefinition aTestObject = new ObjectDefinition("testobject1");
            aTestObject.AddEntry(new ObjectEntryDefinition(aTestObject, "UID", otDataType.Number, false));
            aTestObject.AddEntry(new ObjectEntryDefinition(aTestObject, "VER", otDataType.Number, false));
            aTestObject.AddEntry(new ObjectEntryDefinition(aTestObject, "CREATED", otDataType.Date, true));
            aTestObject.AddEntry(new ObjectEntryDefinition(aTestObject, "DESC", otDataType.Text, true));
            aTestObject.Keys = new string[] { "UID", "VER" };
            _objects.Add(aTestObject.Name, aTestObject);
            
        }

        /// <summary>
        /// GetIObjectDefinition of an ID
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        public IObjectDefinition GetIObjectDefinition(ObjectName name)
        {
            if (_objects.ContainsKey(name)) return _objects[name];
            return null;
        }

        /// <summary>
        /// retuns an object definition by canonical name in string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IObjectDefinition GetIObjectDefinition(string name)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public IObjectDefinition GetIObjectDefinition(Type type)
        {
            throw new NotImplementedException();
        }

        public string GetObjectId(Type type)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// returns the objectname from a type name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Type GetObjectType(ObjectName name)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public string GetObjectname(string typefullname)
        {
            throw new NotImplementedException();
        }

        public Type GetObjectType(string objectname)
        {
            throw new NotImplementedException();
        }

        public bool HasObjectDefinition(string id)
        {
            if (_objects.ContainsKey(ObjectName.From(id)))  return true;
            return false;
        }
         public bool HasObjectDefinition(ObjectName name)
        {
            if (_objects.ContainsKey(name))  return true;
            return false;
        }
        public bool HasObjectDefinition(Type type)
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

        public IList<T> Get<T>(CanonicalName name) where T : OnTrack.Core.ISigned
        {
            throw new NotImplementedException();
        }

        public bool Remove(ISignature signature)
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<IObjectDefinition> IObjectDefinitions
        {
            get
            {
                return _objects.Values;
            }
        }

        public System.Collections.Generic.IEnumerable<IDataObjectProvider> DataObjectProviders
        {
            get
            {
                return _providers;
            }
        }
        public System.Collections.Generic.IEnumerable<CanonicalName> ModuleNames
        {
            get
            {
                List<CanonicalName> aList = new List<CanonicalName>();
                aList.Add(CanonicalName.GlobalName);
                return aList;
            }
        }

        public string Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// Test DataObject Provider
    /// </summary>
    internal class DataObjectProvider : IDataObjectProvider
    {
        /// <summary>
        /// constructor
        /// </summary>
        public DataObjectProvider ()
        {

        }
        public IDataObject NewDataObject(Type type)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public bool HasObjectID(string objectID)
        {
            return true;
        }

        public bool HasType(Type type)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public bool RegisterObjectID(string objectID)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public IDataObjectRepository DataObjectRepository
        {
            get
            {
                // TODO: Implement this property getter
                throw new NotImplementedException();
            }
        }

        public List<Type> Types
        {
            get
            {
                // TODO: Implement this property getter
                throw new NotImplementedException();
            }
        }

        public List<string> ObjectIDs
        {
            get
            {
                // TODO: Implement this property getter
                throw new NotImplementedException();
            }
        }

        public IDataObject Create(string objectid, IKey key)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public IEnumerable<IDataObject> RetrieveAll(string objectid)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public IEnumerable<IDataObject> Retrieve(SelectionRule rule)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public IDataObject Retrieve(string objectid, IKey key)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public bool Persist(IDataObject obj, DateTime? timestamp = null)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public bool Delete(IDataObject obj, DateTime? timestamp = null)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public bool UnDelete(IDataObject obj)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public IDataObject Clone(IDataObject obj, IKey key = null)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// DataObjectEngine for Testing
    /// </summary>
    internal class DataObjectEngine : IDataObjectEngine
    {
        DataObjectRepository _objects = new DataObjectRepository();

        public DataObjectEngine (string id = null)
        {
            if (id == null) id = Guid.NewGuid().ToString();
            this.Id = id;
           
        }
        public string Id { get; set; }

        public IDataObjectRepository Objects
        {
            get
            {
                return _objects;
            }
        }

        public bool Generate(IRule rule, out ICodeBit result)
        {
            // TODO: Implement this method
            result = null;
            return true;
        }

        public bool Run(string id, Context context)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Testclass for RulezParser
    /// </summary>
    [TestClass]
    public class RulezParserTesting
    {
        private Rulez.Engine _engine = new Rulez.Engine();
        
        // Test-Sources
        String[] postiveSyntaxTest =
        {
            // 1
            "selection s1 as deliverables[100];",
            // 2
            "selection s1a as testobject1[100,1];",
            // 3
            "selection s1b as deliverables[uid = 100|200];",
            // 4
            "selection s1c as testobject1[[ 100|200|300, 2] | created > #10.03.2015# ];", 
            // 5
            "selection s1d as testobject1[[ 100|200|300, 2 ] | [ 150, 3 ]];",
            // 6
            "selection s1e as testobject1[( 100 ) | ( 150 ) , 2];",
            // 7
            "selection s1f as testobject1[ testobject1.uid=100 OR ( created > #20.05.2015# and desc = \"test\" )];",
            // 8
            "selection s1g as testobject1[ uid=ver + 1, 1];",
            // 9
            "selection s1g as testobject1[];",
            // 10
            "selection s2 (p1 as number) as deliverables[p1];",
            // 11
            "selection s3 (p1 as number? default 100 ) as deliverables[uid=p1];",
            // 12
            "selection s12 as deliverables[100].uid ;",
            // 13
            "selection s13 as testobject1[100][desc,created and ver] ;",
            // 14
            "selection s14 (p1 as date) as testobject1[ uid = deliverables[date >= p1].uid, 2];" ,  
             // 15 semantic not possible -> cycle loop
            "selection s14 (p1 as date) as testobject1[ uid = deliverables[date >= testobject1.created].uid, 2];"    
        };
        String[] expectedTree =
        {
            // 1
            "{Unit:{(SelectionRule) s1[]{ResultList:<DataObjectSymbol:DELIVERABLES>}{{(SelectionStatementBlock) LIST<DELIVERABLES?>[]{{Return LIST<DELIVERABLES?> {(SelectionExpression) {ResultList:<DataObjectSymbol:DELIVERABLES>}:{(CompareExpression) '=':<DataObjectSymbol:DELIVERABLES.UID>,<NUMBER:100>}}}}}}}}",
            // 2
            "{Unit:{(SelectionRule) s1a[]{ResultList:<DataObjectSymbol:TESTOBJECT1>}{{(SelectionStatementBlock) LIST<TESTOBJECT1?>[]{{Return LIST<TESTOBJECT1?> {(SelectionExpression) {ResultList:<DataObjectSymbol:TESTOBJECT1>}:{(LogicalExpression) 'ANDALSO':{(CompareExpression) '=':<DataObjectSymbol:testobject1.uid>,<NUMBER:100>},{(CompareExpression) '=':<DataObjectSymbol:testobject1.ver>,<NUMBER:1>}}}}}}}}}",
            // 3
            "{Unit:{(SelectionRule) s1b[]{ResultList:<DataObjectSymbol:DELIVERABLES>}{{(SelectionStatementBlock) LIST<DELIVERABLES?>[]{{Return LIST<DELIVERABLES?> {(SelectionExpression) {ResultList:<DataObjectSymbol:DELIVERABLES>}:{(LogicalExpression) 'ORELSE':{(CompareExpression) '=':<DataObjectSymbol:deliverables.uid>,<NUMBER:100>},{(CompareExpression) '=':<DataObjectSymbol:deliverables.UID>,<NUMBER:200>}}}}}}}}}",
            // 4
            "{Unit:{(SelectionRule) s1c[]{ResultList:<DataObjectSymbol:TESTOBJECT1>}{{(SelectionStatementBlock) LIST<TESTOBJECT1?>[]{{Return LIST<TESTOBJECT1?> {(SelectionExpression) {ResultList:<DataObjectSymbol:TESTOBJECT1>}:{(LogicalExpression) 'ORELSE':{(LogicalExpression) 'ANDALSO':{(LogicalExpression) 'ORELSE':{(LogicalExpression) 'ORELSE':{(CompareExpression) '=':<DataObjectSymbol:testobject1.UID>,<NUMBER:100>},{(CompareExpression) '=':<DataObjectSymbol:testobject1.UID>,<NUMBER:200>}},{(CompareExpression) '=':<DataObjectSymbol:testobject1.UID>,<NUMBER:300>}},{(CompareExpression) '=':<DataObjectSymbol:testobject1.VER>,<NUMBER:2>}},{(CompareExpression) 'GT':<DataObjectSymbol:testobject1.created>,<DATE:10.03.2015 00:00:00>}}}}}}}}}",
            // 5
            "{Unit:{(SelectionRule) s1d[]{ResultList:<DataObjectSymbol:TESTOBJECT1>}{{(SelectionStatementBlock) LIST<TESTOBJECT1?>[]{{Return LIST<TESTOBJECT1?> {(SelectionExpression) {ResultList:<DataObjectSymbol:TESTOBJECT1>}:{(LogicalExpression) 'ORELSE':{(LogicalExpression) 'ANDALSO':{(LogicalExpression) 'ORELSE':{(LogicalExpression) 'ORELSE':{(CompareExpression) '=':<DataObjectSymbol:testobject1.UID>,<NUMBER:100>},{(CompareExpression) '=':<DataObjectSymbol:testobject1.UID>,<NUMBER:200>}},{(CompareExpression) '=':<DataObjectSymbol:testobject1.UID>,<NUMBER:300>}},{(CompareExpression) '=':<DataObjectSymbol:testobject1.VER>,<NUMBER:2>}},{(LogicalExpression) 'ANDALSO':{(CompareExpression) '=':<DataObjectSymbol:testobject1.UID>,<NUMBER:150>},{(CompareExpression) '=':<DataObjectSymbol:testobject1.VER>,<NUMBER:3>}}}}}}}}}}",
            // 6
            "{Unit:{(SelectionRule) s1e[]{ResultList:<DataObjectSymbol:TESTOBJECT1>}{{(SelectionStatementBlock) LIST<TESTOBJECT1?>[]{{Return LIST<TESTOBJECT1?> {(SelectionExpression) {ResultList:<DataObjectSymbol:TESTOBJECT1>}:{(LogicalExpression) 'ANDALSO':{(LogicalExpression) 'ORELSE':{(CompareExpression) '=':<DataObjectSymbol:testobject1.UID>,<NUMBER:100>},{(CompareExpression) '=':<DataObjectSymbol:testobject1.UID>,<NUMBER:150>}},{(CompareExpression) '=':<DataObjectSymbol:testobject1.VER>,<NUMBER:2>}}}}}}}}}",
            // 7
            "{Unit:{(SelectionRule) s1f[]{ResultList:<DataObjectSymbol:TESTOBJECT1>}{{(SelectionStatementBlock) LIST<TESTOBJECT1?>[]{{Return LIST<TESTOBJECT1?> {(SelectionExpression) {ResultList:<DataObjectSymbol:TESTOBJECT1>}:{(LogicalExpression) 'ORELSE':{(CompareExpression) '=':<DataObjectSymbol:testobject1.uid>,<NUMBER:100>},{(LogicalExpression) 'ANDALSO':{(CompareExpression) 'GT':<DataObjectSymbol:testobject1.created>,<DATE:20.05.2015 00:00:00>},{(CompareExpression) '=':<DataObjectSymbol:testobject1.desc>,<TEXT:\"test\">}}}}}}}}}}",
            // 8
            "{Unit:{(SelectionRule) s1g[]{ResultList:<DataObjectSymbol:TESTOBJECT1>}{{(SelectionStatementBlock) LIST<TESTOBJECT1?>[]{{Return LIST<TESTOBJECT1?> {(SelectionExpression) {ResultList:<DataObjectSymbol:TESTOBJECT1>}:{(LogicalExpression) 'ANDALSO':{(CompareExpression) '=':<DataObjectSymbol:testobject1.uid>,{(OperationExpression) '+':<DataObjectSymbol:testobject1.ver>,<NUMBER:1>}},{(CompareExpression) '=':<DataObjectSymbol:testobject1.VER>,<NUMBER:1>}}}}}}}}}",
            // 9
            "{Unit:{(SelectionRule) s1g[]{ResultList:<DataObjectSymbol:TESTOBJECT1>}{{(SelectionStatementBlock) LIST<TESTOBJECT1?>[]{{Return LIST<TESTOBJECT1?> {(SelectionExpression) {ResultList:<DataObjectSymbol:TESTOBJECT1>}:{(LogicalExpression) 'TRUE':}}}}}}}}",
            // 10
            "{Unit:{(SelectionRule) s2[<Variable:p1>]{ResultList:<DataObjectSymbol:DELIVERABLES>}{{(SelectionStatementBlock) LIST<DELIVERABLES?>[]{{Return LIST<DELIVERABLES?> {(SelectionExpression) {ResultList:<DataObjectSymbol:DELIVERABLES>}:{(CompareExpression) '=':<DataObjectSymbol:deliverables.UID>,<Variable:p1>}}}}}}}}",
            // 11
            "{Unit:{(SelectionRule) s3[<Variable:p1>]{ResultList:<DataObjectSymbol:DELIVERABLES>}{{(SelectionStatementBlock) LIST<DELIVERABLES?>[]{{IfThenElse:{(CompareExpression) '=':<Variable:p1>,<NULL:>},{Assignment:<Variable:p1>,<NUMBER:100>}},{Return LIST<DELIVERABLES?> {(SelectionExpression) {ResultList:<DataObjectSymbol:DELIVERABLES>}:{(CompareExpression) '=':<DataObjectSymbol:deliverables.uid>,<Variable:p1>}}}}}}}}",
            // 12
            "{Unit:{(SelectionRule) s12[]{ResultList:<DataObjectSymbol:deliverables.uid>}{{(SelectionStatementBlock) LIST<NUMBER>[]{{Return LIST<NUMBER> {(SelectionExpression) {ResultList:<DataObjectSymbol:deliverables.uid>}:{(CompareExpression) '=':<DataObjectSymbol:deliverables.UID>,<NUMBER:100>}}}}}}}}",
            // 13
            "{Unit:{(SelectionRule) s13[]{ResultList:<DataObjectSymbol:testobject1.desc>,<DataObjectSymbol:testobject1.created>,<DataObjectSymbol:testobject1.ver>}{{(SelectionStatementBlock) LIST<TUPLE<TEXT,DATE,NUMBER>>[]{{Return LIST<TUPLE<TEXT,DATE,NUMBER>> {(SelectionExpression) {ResultList:<DataObjectSymbol:testobject1.desc>,<DataObjectSymbol:testobject1.created>,<DataObjectSymbol:testobject1.ver>}:{(CompareExpression) '=':<DataObjectSymbol:testobject1.UID>,<NUMBER:100>}}}}}}}}",
            // 14
            "{Unit:{(SelectionRule) s14[<Variable:p1>]{ResultList:<DataObjectSymbol:TESTOBJECT1>}{{(SelectionStatementBlock) LIST<TESTOBJECT1?>[]{{Return LIST<TESTOBJECT1?> {(SelectionExpression) {ResultList:<DataObjectSymbol:TESTOBJECT1>}:{(LogicalExpression) 'ANDALSO':{(CompareExpression) '=':<DataObjectSymbol:testobject1.uid>,{(SelectionExpression) {ResultList:<DataObjectSymbol:deliverables.uid>}:{(CompareExpression) 'GE':<DataObjectSymbol:deliverables.date>,<Variable:p1>}}},{(CompareExpression) '=':<DataObjectSymbol:testobject1.VER>,<NUMBER:2>}}}}}}}}}",
            // 15
            "{Unit:{(SelectionRule) s14[<Variable:p1>]{ResultList:<DataObjectSymbol:TESTOBJECT1>}{{(SelectionStatementBlock) LIST<TESTOBJECT1?>[]{{Return LIST<TESTOBJECT1?> {(SelectionExpression) {ResultList:<DataObjectSymbol:TESTOBJECT1>}:{(LogicalExpression) 'ANDALSO':{(CompareExpression) '=':<DataObjectSymbol:testobject1.uid>,{(SelectionExpression) {ResultList:<DataObjectSymbol:deliverables.uid>}:{(CompareExpression) 'GE':<DataObjectSymbol:deliverables.date>,<DataObjectSymbol:testobject1.created>}}},{(CompareExpression) '=':<DataObjectSymbol:testobject1.VER>,<NUMBER:2>}}}}}}}}}",
        };
        String[] negativeSyntaxTest =
        {
            "selection s1 as deliverables[100,1];", // automatic keycount wrong
            "selection s1a as delivarebles[\"100\"];", // wrong type of key
            
        };
        Engine Engine { get { return _engine; }  }
        [TestMethod]
        public void AllSyntax()
        {
            {
                // data context
                Engine.Add(new DataObjectEngine("test"));

                for (uint i = 0; i < postiveSyntaxTest.GetUpperBound(0); i++)
                {
                    RunPositiveSyntaxTest(i+1, postiveSyntaxTest[i], expected: expectedTree[i]);
                }

                for (uint i = 0; i < negativeSyntaxTest.GetUpperBound(0); i++)
                {
                    RunNegativeSyntaxTest(i + 1, negativeSyntaxTest[i]);
                }
            }
            
        }
        [TestMethod]
        public void RunDevelopmentTest()
        {
            // data context
            Engine.Add(new DataObjectEngine("test"));
            uint i = 15;
            RunPositiveSyntaxTest(i, postiveSyntaxTest[i-1], expected: expectedTree[i-1]);
        }
        /// <summary>
        /// run a positive syntax test by statement
        /// </summary>
        /// <param name="statement"></param>
        [TestMethod]
        public void RunPositiveSyntaxTest(ulong id, string statement, string expected = null)
        {
            bool aResult = false;
          
            Console.Out.WriteLine(DateTime.Now.ToString("s") + ": Syntax Test #" + id.ToString() + ":" + statement);
                
            INode theNode = _engine.Verify(statement);

            // check result
            if (theNode == null)
                aResult = false;
            else if (theNode.Messages.Where(x => x.Type == MessageType.Error).Count() == 0)
            {
                if (String.IsNullOrEmpty(expected))
                    aResult = true;
                else if (String.Compare(theNode.ToString(), expected, true) == 00)
                    aResult = true;
                else
                {
                    Console.Out.WriteLine(DateTime.Now.ToString("s") + ": Syntax Test #" + id.ToString() + " expected result :" + expected);
                    aResult = false;
                }
            }
                
            if (!aResult)
            {
                Console.Out.WriteLine(DateTime.Now.ToString("s") + ": Syntax Test #" + id.ToString() + " failed.");
                if (theNode != null)
                {
                    Console.Out.WriteLine("Errors:");
                    foreach (Message anError in theNode.Messages)
                        Console.Out.WriteLine(anError.ToString());
                    Console.Out.WriteLine();
                    Console.Out.WriteLine("Result:");
                    Console.Out.WriteLine(theNode.ToString());
                }
            }
            else
                Console.Out.WriteLine(DateTime.Now.ToString("s") + ": Syntax Test #" + id.ToString() + " succeeded.");

            // assert
            Assert.IsTrue(aResult, DateTime.Now.ToString("s") + ": Syntax Test #" + id.ToString() + " failed.");
            
        }
        /// <summary>
        /// run a positive syntax test by statement
        /// </summary>
        /// <param name="statement"></param>
        [TestMethod]
        public void RunNegativeSyntaxTest(ulong id, string statement, string[] expectedMessages = null)
        {
            bool aResult = false;
            try
            {
                Console.Out.WriteLine(DateTime.Now.ToString("s") + ": Syntax Test #" + id.ToString() + ":" + statement);

                INode theNode = this.Engine.Verify(statement);

                // check result
                if (theNode == null)
                    aResult = false;
                else if (theNode.Messages.Where(x => x.Type == MessageType.Error).Count() != 0)
                {
                    aResult = false;
                    foreach (string msgtext in expectedMessages)
                    {

                    }

                }

                if (!aResult)
                {
                    Console.Out.WriteLine(DateTime.Now.ToString("s") + ": Syntax #" + id.ToString() + " failed as expected.");
                    if (theNode != null)
                    {
                        Console.Out.WriteLine("Errors:");
                        foreach (Message anError in theNode.Messages)
                            Console.Out.WriteLine(anError.ToString());
                        Console.Out.WriteLine();
                        Console.Out.WriteLine("Result:");
                        Console.Out.WriteLine(theNode.ToString());
                    }
                }
                else
                    Console.Out.WriteLine(DateTime.Now.ToString("s") + ": Syntax #" + id.ToString() + " not failed.");

                // assert
                Assert.IsFalse(aResult, DateTime.Now.ToString("s") + ": Syntax Test #" + id.ToString() + " failed.");
            }

            catch (Exception ex)
            {
                if (!(ex is Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException)) Assert.Fail(ex.Message);
            }
        }
    }
}