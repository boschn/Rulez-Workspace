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
    internal class ObjectEntryDefinition : iObjectEntryDefinition 
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="objectdefinition"></param>
        /// <param name="entryname"></param>
        /// <param name="datatype"></param>
        /// <param name="isNull"></param>
        public ObjectEntryDefinition (iObjectDefinition objectdefinition, String entryname, otDataType datatype, bool isNull )
        {
            this.Objectname = objectdefinition.Objectname;
            this.Entryname = entryname;
            this.TypeId = datatype ;
            this.IsNullable = isNull;
            this.ObjectDefinition = objectdefinition ;
        }
        public bool IsMapped { get ; set; }

        public long? LowerRangeValue { get ; set; }
       

        public long? UpperRangeValue { get ; set; }
      
        public List<string> PossibleValues { get ; set; }
        

        public string Description{ get ; set; }
        

        public string Objectname { get ; set; }

        public string Entryname { get ; set; }

        public otDataType TypeId { get ; set; }

        public long Version { get ; set; }
       
        public string Title { get ; set; }

        public object DefaultValue { get ; set; }
        
        public bool IsNullable { get ; set; }
        

        public long PrimaryKeyOrdinal { get ; set; }
       
        public otDataType? InnerDatatype { get ; set; }
       

        public long Ordinal { get ; set; }
       

        public bool IsReadonly { get ; set; }
       

        public bool IsActive { get ; set; }
       

        public iObjectDefinition ObjectDefinition { get ; set; }
       
    }

    /// <summary>
    /// Test ObjectDefinition
    /// </summary>
    internal class ObjectDefinition : iObjectDefinition
    {
        private String _id;
        private Dictionary <String, iObjectEntryDefinition> _entries = new Dictionary<String, iObjectEntryDefinition>();
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="id"></param>
        public ObjectDefinition(String id)
        {
            _id = id;
        }

        public bool AddEntry(iObjectEntryDefinition entry)
        {
            if (_entries.ContainsKey (entry.Entryname)) _entries .Remove (entry.Entryname );

            _entries .Add(entry.Entryname, entry);
            return true;
        }
        public string Objectname
        {
            get
            {
                return _id;
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

        public string Modulename
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

        public string Classname
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
                // TODO: Implement this property getter
                throw new NotImplementedException();
            }
            set
            {
                // TODO: Implement this property setter
                throw new NotImplementedException();
            }
        }

        public IList<iObjectEntryDefinition> iObjectEntryDefinitions
        {
            get
            {
                return _entries.Values.ToList();
            }
        }

        public IList<string> Entrynames(bool onlyActive = true)
        {
            return _entries.Keys.ToList();
        }

        public iObjectEntryDefinition GetiEntryDefinition(string entryname)
        {
            if (this.HasEntry(entryname)) return _entries[entryname];
            return null;
        }

        public bool HasEntry(string entryname)
        {
           return _entries.ContainsKey(entryname);
        }
    }

   
    /// <summary>
    /// Test DataObject Repository
    /// </summary>
    internal  class DataObjectRepository : iDataObjectRepository
    {
        // add
        private Dictionary<string, iObjectDefinition> _objects = new Dictionary<string, iObjectDefinition>();
        private Dictionary<Type, String> _types = new Dictionary<Type, String>();

        private List<iDataObjectProvider> _providers = new List<iDataObjectProvider>();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DataObjectRepository()
        {
            ObjectDefinition aDefinition = new ObjectDefinition("deliverables");
            aDefinition.AddEntry(new ObjectEntryDefinition(aDefinition, "uid", otDataType.Number, false));
            _objects.Add(aDefinition.Objectname, aDefinition);

            _providers.Add(new DataObjectProvider());
        }

        /// <summary>
        /// GetIObjectDefinition of an ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public iObjectDefinition GetIObjectDefinition(string id)
        {
            if (_objects.ContainsKey(id))
                return _objects[id];
            return null;
        }

        public iObjectDefinition GetIObjectDefinition(Type type)
        {
            throw new NotImplementedException();
        }

        public string GetObjectname(Type type)
        {
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
            if (_objects.ContainsKey(id))
                return true;
            return false;
        }

        public bool HasObjectDefinition(Type type)
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<iObjectDefinition> IObjectDefinitions
        {
            get
            {
                return _objects.Values;
            }
        }

        public System.Collections.Generic.IEnumerable<iDataObjectProvider> DataObjectProviders
        {
            get
            {
                return _providers;
            }
        }
    }

    /// <summary>
    /// Test DataObject Provider
    /// </summary>
    internal class DataObjectProvider : iDataObjectProvider
    {
        /// <summary>
        /// constructor
        /// </summary>
        public DataObjectProvider ()
        {

        }
        public iDataObject NewDataObject(Type type)
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

        public iDataObjectRepository DataObjectRepository
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

        public iDataObject Create(string objectid, iKey key)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public IEnumerable<iDataObject> RetrieveAll(string objectid)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public IEnumerable<iDataObject> Retrieve(SelectionRule rule)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public iDataObject Retrieve(string objectid, iKey key)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public bool Persist(iDataObject obj, DateTime? timestamp = null)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public bool Delete(iDataObject obj, DateTime? timestamp = null)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public bool UnDelete(iDataObject obj)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }

        public iDataObject Clone(iDataObject obj, iKey key = null)
        {
            // TODO: Implement this method
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// DataObjectEngine for Testing
    /// </summary>
    internal class DataObjectEngine : iDataObjectEngine
    {
        DataObjectRepository _objects = new DataObjectRepository();

        public DataObjectEngine (string id = null)
        {
            if (id == null) id = (new Guid()).ToString();
            this.ID = id;
           
        }
        public string ID { get; set; }

        public iDataObjectRepository Objects
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
            throw new NotImplementedException();
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
        String[] SyntaxTest =
        {
            "selection s1 as deliverables[100];",
            "selection s2 (p1 as number) as deliverables[p1];",
            "selection s3 (p1 as number ? default 100 ) as deliverables[.uid=p1];",
            "selection s4 as deliverables[.uid=p1 as number];"
        };

        [TestMethod]
        public void AllSyntax()
        {
            try
            {
                // data context
                _engine.AddDataEngine(new DataObjectEngine("test"));

                for (uint i = 0; i < SyntaxTest.GetUpperBound(0); i++)
                {
                    RunSpecificTest(i);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
       
        [TestMethod]
        public void RunSpecificTest(uint no = 2)
        {
            try
            {
                Assert.IsFalse(_engine.Generate(SyntaxTest[no]), "Failed: " + SyntaxTest[no]);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}