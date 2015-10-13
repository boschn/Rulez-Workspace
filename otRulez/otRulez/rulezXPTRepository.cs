/**
 *  ONTRACK RULEZ ENGINE
 *  
 *  Intermediate repository for generating rules while generating
 * 
 * Version: 1.0
 * Created: 2015-10-14
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
using System.Text;

using OnTrack.Core;
using OnTrack.Rulez.eXPressionTree;

namespace OnTrack.Rulez
{
    /// <summary>
    /// XPT Generation Scope Definition
    /// this Scope is intended while generating new XPT Nodes
    /// the search function includes a vertical search first in this scope, than in the engine scope (on that level) and then upwards 
    /// </summary>
    public class XPTScope : Scope
    {
        private XPTRepository _xptrepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="XPTScope" /> class.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <param name="id">The id.</param>
        public XPTScope(Engine engine, string id=null) : base(engine, id)
        {
            _xptrepository = new XPTRepository (engine);
        }

        /// <summary>
        /// gets the scope of the engine with same id
        /// </summary>
        public IScope EngineScope
        {
            get
            {
                if (Engine.HasScope(this.Id))
                    return Engine.GetScope(this.Id);
                return null;
            }
        }
        /// <summary>
        /// gets the repository
        /// </summary>
        public override IRepository Repository
        {
            get { return _xptrepository; }
        }
        /// <summary>
        /// returns true if the Children have an ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override bool HasSubScope(string id)
        {
            if (this.Children.Where(x => String.Compare(x.Id, id, true) == 00).FirstOrDefault() != null)
                return true;
            else
                return (EngineScope!=null) ? EngineScope.HasSubScope(id) : false;
        }

        /// <summary>
        /// returns a Subscope of an given id or null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override IScope GetSubScope(string id)
        {
            IScope aScope = this.Children.Where(x => String.Compare(x.Id, id, true) == 00).FirstOrDefault();

            if (aScope != null) return aScope;
            return (EngineScope!=null) ? EngineScope.GetSubScope(id) :null;
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
                this.Children.Add(new Scope(engine: this.Engine, id: id));
            }
            // return the last scope
            return this.GetSubScope(id);
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
            else
                /// look into Engine Scope
                if (EngineScope!=null && EngineScope.Repository.HasSelectionRule(id)) 
                    return EngineScope.Repository.GetSelectionRule(id);
            else 
                    if (Parent != null && Parent.HasSelectionRule(id))
                        return Parent.GetSelectionRule(id);
            


            // create a selection rule and return
            SelectionRule aRule = new SelectionRule(id);
            Repository.AddSelectionRule(aRule.ID, aRule);
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
            else
                if (EngineScope != null && EngineScope.Repository.HasSelectionRule(id)) return true;
                else
                    if (Parent != null)
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
            else if (EngineScope != null && EngineScope.Repository.HasOperator(id)) 
                return EngineScope.Repository.GetOperator(id);
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
            else if (EngineScope != null && EngineScope.Repository.HasOperator(id))
                return true;
            else if (Parent != null)
                return Parent.Repository.HasOperator(id);
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
            else if (EngineScope != null && EngineScope.Repository.HasFunction(id)) 
                return EngineScope.GetFunction(id);
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
            else if (EngineScope!=null && EngineScope.Repository.HasFunction(id))
                return true;
            else if (Parent != null)
                return Parent.Repository.HasFunction(id);
            return false;
        }

        /// <summary>
        /// gets the Operator definition for the ID
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public virtual iObjectDefinition GetDataObjectDefinition(string id)
        {
            if (Repository.HasDataObjectDefinition(id))
                return Repository.GetDataObjectDefinition(id);
            else if (EngineScope != null && EngineScope.Repository.HasDataObjectDefinition(id))
                return EngineScope.Repository.GetDataObjectDefinition(id);
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
            else if (EngineScope != null && EngineScope.Repository.HasDataObjectDefinition(new ObjectName(moduleid: this.Id, objectid: id))) 
                return true;
            else if (Parent != null)
                return Parent.HasDataObjectDefinition(id);
            return false;
        }
    }
    /// <summary>
    /// XPT Generation Repository
    /// implement a normal repository but with a fake data object repository
    /// </summary>
    internal class XPTRepository : Repository
    {
        // static dataobjectrepository
        static XPTDataObjectRepository _datarepository = new XPTDataObjectRepository();

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="id"></param>
        public XPTRepository (Engine engine, string id = null) : base(engine, id)
        {
            // add the static engine
            this.RegisterDataObjectRepository(_datarepository);
        }
        
    }
    /// <summary>
    /// the XPT DataObjectRepository to handle the intermediate data objects
    /// </summary>
    internal class XPTDataObjectRepository : iDataObjectRepository
    {
        private Dictionary <ObjectName,iObjectDefinition> _objects = new Dictionary<ObjectName, iObjectDefinition>();

        public iObjectDefinition GetIObjectDefinition(ObjectName name)
        {
            if (HasObjectDefinition(name)) return _objects[name];
            return null;
        }

        public iObjectDefinition GetIObjectDefinition(string name)
        {
            ObjectName aName = new ObjectName(name);
            if (HasObjectDefinition(aName)) return _objects[aName];
            return null;
        }

        public iObjectDefinition GetIObjectDefinition(Type type)
        {
            return _objects.Values.Where(x => x.ObjectType == type).FirstOrDefault();
        }

        public string GetObjectId(Type type)
        {
            return (GetIObjectDefinition(type) == null) ? null : GetIObjectDefinition (type).Id;
        }

        public Type GetObjectType(ObjectName name)
        {
            throw new NotImplementedException();
        }

        public bool HasObjectDefinition(ObjectName objectname)
        {
            return _objects.ContainsKey(objectname);
        }

        public bool HasObjectDefinition(string id)
        {
            ObjectName aName = new ObjectName(id);
            return HasObjectDefinition(aName);
        }

        public bool HasObjectDefinition(Type type)
        {
            return _objects.Values.Where(x => x.ObjectType == type).FirstOrDefault() != null;
        }

        public IEnumerable<iObjectDefinition> IObjectDefinitions
        {
            get { return _objects.Values; }
        }

        public IEnumerable<iDataObjectProvider> DataObjectProviders
        {
            get { return new List<iDataObjectProvider>(); }
        }

        public IEnumerable<CanonicalName> ModuleNames
        {
            get 
            { 
                SortedSet<CanonicalName > aList = new SortedSet<CanonicalName> ();
                foreach (string anID in _objects.Values.Select(x => x.ModuleId))
                    aList.Add(new CanonicalName(anID));
                return aList.ToList<CanonicalName>();

            }
        }
    }
    /// <summary>
    /// XPT data object definition - simply a fake
    /// </summary>
    internal class XPTDataObjectDefinition : iObjectDefinition
    {
        private  ObservableCollection<iObjectEntryDefinition> _entries = new ObservableCollection<iObjectEntryDefinition> ();
        /// <summary>
        /// constructor
        /// </summary>
        public XPTDataObjectDefinition()
        {
            _entries.CollectionChanged += _entries_CollectionChanged;
        }
        /// <summary>
        /// event handler for entries add
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _entries_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add )
            {
                foreach (iObjectEntryDefinition item in e.NewItems)
                {
                    if (item is XPTDataObjectEntryDefinition) ((XPTDataObjectEntryDefinition)item).ObjectDefinition  = this;
                }
            }
        }
        ///
        /// Properties
        /// 
        #region Properties
        /// <summary>
        /// gets the object name
        /// </summary>
        /// <value></value>
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// gets the System.Type of the object implementation class
        /// </summary>
        /// <value></value>
        public Type ObjectType
        {
            get;
            set;
        }

        /// <summary>
        /// gets the module id space
        /// </summary>
        /// <value></value>
        public string ModuleId
        {
            get;
            set;
        }
        /// <summary>
        /// gets the .net class name
        /// </summary>
        /// <value></value>
        public string Classname
        {
            get;
            set;
        }
        /// <summary>
        /// gets the description
        /// </summary>
        /// <value></value>
        public string Description
        {
            get;
            set;
        }
        /// <summary>
        /// gets or sets the Properties of the object
        /// </summary>
        /// <value></value>
        public string[] Properties
        {
            get;
            set;
        }
        /// <summary>
        /// gets or sets the Version of the object
        /// </summary>
        /// <value></value>
        public long Version
        {
            get;
            set;
        }
        /// <summary>
        /// gets or sets the active / enabled flag
        /// </summary>
        /// <value></value>
        public bool IsActive
        {
            get;
            set;
        }
        /// <summary>
        /// gets or sets the unique key entry names
        /// </summary>
        /// <value></value>
        public string[] Keys
        {
            get 
            {
                return _entries.Where(x => x.PrimaryKeyOrdinal.HasValue && x.PrimaryKeyOrdinal.Value > 0).OrderBy(x => x.PrimaryKeyOrdinal.Value).Select(x => x.EntryId).ToArray();
            }
            set
            {
                // set key according to array
                long anOrdinal = 1;
                foreach(iObjectEntryDefinition anEntry in _entries)
                {
                    bool found = false;
                    foreach (string anId in value)
                        if (String.Compare(anEntry.EntryId, anId) == 00)
                        {
                            anEntry.PrimaryKeyOrdinal = anOrdinal;
                            anOrdinal++;
                            found = true;
                        }
                    // set to null of not found
                    if (!found) anEntry.PrimaryKeyOrdinal = null;
                }
            }
        }
        /// <summary>
        /// returns a List of iObjectEntryDefinitions
        /// </summary>
        /// <value></value>
        public IList<iObjectEntryDefinition> iObjectEntryDefinitions
        {
            get { return _entries; }
        }
        /// <summary>
        /// gets the Entries
        /// </summary>
        public ObservableCollection<iObjectEntryDefinition> Entries
        {
            get { return _entries; }
        }
#endregion
        /// <summary>
        /// returns the (active) names of the Entries
        /// </summary>
        /// <param name="onlyActive"></param>
        /// <returns></returns>
        public IList<string> EntryNameIds(bool onlyActive = true)
        {
            return _entries.Select(x => x.EntryId).ToList ();
        }
        /// <summary>
        /// returns an EntryDefinition or null
        /// </summary>
        /// <param name="entryNameId"></param>
        /// <returns></returns>
        public iObjectEntryDefinition GetiEntryDefinition(string entryNameId)
        {
            return _entries.Where(x => String.Compare(x.EntryId, entryNameId, true) == 00).FirstOrDefault();
        }
        /// <summary>
        /// returns true if the entry name exists
        /// </summary>
        /// <param name="entryNameId"></param>
        /// <returns></returns>
        public bool HasEntry(string entryNameId)
        {
            return _entries.Where(x => String.Compare(x.EntryId, entryNameId, true)==00).FirstOrDefault() != null;
        }
    }
    /// <summary>
    /// XPT data object entry definition
    /// </summary>
    internal class XPTDataObjectEntryDefinition : iObjectEntryDefinition
    {
        #region Properties
        /// <summary>
        /// returns true if the Entry is mapped to a class member field
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <value></value>
        public bool IsMapped
        {
            get;
            set;
        }

        /// <summary>
        /// gets the lower range Value
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <value></value>
        public long? LowerRangeValue
        {
            get;
            set;
        }

        /// <summary>
        /// gets the upper range Value
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <value></value>
        public long? UpperRangeValue
        {
            get;
            set;
        }

        /// <summary>
        /// gets the list of possible values
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <value></value>
        public List<string> PossibleValues
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// sets or gets the object name of the entry
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <value></value>
        public string ObjectId
        {
            get { return this.ObjectDefinition.Id; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// returns the name of the entry
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <value></value>
        public string EntryId
        {
            get;
            set;
        }

        /// <summary>
        /// returns the field data type
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <value></value>
        public otDataType TypeId
        {
            get;
            set;
        }

        /// <summary>
        /// returns the datatype
        /// </summary>
        /// <value></value>
        public IDataType DataType
        {
            get;
            set;
        }

        /// <summary>
        /// returns version
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <value></value>
        public long Version
        {
            get;
            set;
        }

        /// <summary>
        /// returns Title (Column Header)
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <value></value>
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// sets or gets the default value for the object entry
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <value></value>
        public object DefaultValue
        {
            get;
            set;
        }
        /// <summary>
        /// set or gets true if the entry value is nullable
        /// </summary>
        /// <value></value>
        public bool IsNullable
        {
            get;
            set;
        }
        /// <summary>
        /// gets or sets the Primary key Ordinal of the Object Entry (if set this is part of a key)
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <value></value>
        public long? PrimaryKeyOrdinal
        {
            get;
            set;
        }
        /// <summary>
        /// returns the ordinal
        /// </summary>
        /// <value></value>
        public long Ordinal
        {
            get;
            set;
        }

        /// <summary>
        /// get or sets true if the entry is readonly
        /// </summary>
        /// <value></value>
        public bool IsReadonly
        {
            get;
            set;
        }

        /// <summary>
        /// get or sets true if the entry is active
        /// </summary>
        /// <value></value>
        public bool IsActive
        {
            get;
            set;
        }

        /// <summary>
        /// gets the ObjectDefinition
        /// </summary>
        /// <returns></returns>
        /// <value></value>
        public iObjectDefinition ObjectDefinition
        {
            get;
            set;
        }
        #endregion
        
    }
}
