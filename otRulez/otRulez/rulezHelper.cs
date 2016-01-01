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
    /// class for working with canonical names of the  form id { '.' id }
    /// </summary>
    public class CanonicalName : IEqualityComparer<CanonicalName>, IEquatable<CanonicalName>
    {
        public const string GlobalID = "";
        public const char ConstDelimiter = '.';

        #region Static
        /// <summary>
        /// static Property Global (for GlobalName)
        /// </summary>
        public static CanonicalName GlobalName { get { return new CanonicalName(GlobalID); } }
        /// <summary>
        /// returns true if the id is in canonical form
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        public static bool IsCanonical(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            return id.IndexOf(ConstDelimiter)>=0;
        }
        /// <summary>
        /// pushes a id on a canonical id and returns it
        /// </summary>
        /// <param id="canonicalID"></param>
        /// <param id="id"></param>
        /// <returns></returns>
        public static string Push (string canonicalID, string id)
        {
            if (String.IsNullOrEmpty(canonicalID) || string.Compare (id, GlobalID, ignoreCase: true) == 0)
                return id;
            return canonicalID + ConstDelimiter + id;
        }
        /// <summary>
        /// pops an id of a canonical id and returns the rest and the id in the ref parameter
        /// </summary>
        /// <param id="canonicalID"></param>
        /// <param id="id"></param>
        /// <returns></returns>
        public static string Pop(string canonicalID, ref string id)
        {
            if (IsCanonical(canonicalID))
            {
                string[] split = canonicalID.Split(ConstDelimiter);
                if (split.GetUpperBound(0) > 0)
                {
                    id = split[split.GetUpperBound(0) - 1];
                    string result = String.Empty;
                    // get the result string
                    for (uint i = 0; i <= split.GetUpperBound(0); i++)
                        if (i > 0) result += ConstDelimiter + split[i];
                        else result = split[i];
                    return result;
                }
            }
            id = canonicalID;
            return String.Empty;
        }
        /// <summary>
        /// pops of an id on a canonical id and returns the rest
        /// </summary>
        /// <param id="canonicalID"></param>
        /// <param id="id"></param>
        /// <returns></returns>
        public static string Pop(string canonicalID)
        {
            if (IsCanonical(canonicalID))
            {
                string[] split = canonicalID.Split(ConstDelimiter);
                if (split.GetUpperBound(0) > 0)
                {
                    string result = String.Empty;
                    // get the result string
                    for (uint i = 0; i <= split.GetUpperBound(0); i++)
                        if (i > 0) result += ConstDelimiter + split[i];
                        else result = split[i];
                    return result;
                }
            }
            return String.Empty;
        }
        /// <summary>
        /// returns from canonicalID a reduced id string (without id)  or the canonicalID if not doesnot contain
        /// </summary>
        /// <param name="canonicalID"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string Reduce(string canonicalID,string id)
        {
            // if the canonicalID fully contains the id
            if (String.Compare (canonicalID,1,id, id.Length, 1, true)==00)
            {
                // remove heading '.'
                if (canonicalID.ElementAt(id.Length) == ConstDelimiter)
                    return canonicalID.Substring(id.Length + 1);
                // else the rest of the string
                return canonicalID.Substring(id.Length);
            }
            // else
            return canonicalID;
        }
        /// <summary>
        /// build a canonical String out of an array
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static string StringFrom(string [] ids)
        {
            string result = String.Empty;

            if (ids != null)
                foreach (string aName in ids)
                    if (!String.IsNullOrEmpty(result) && !String.IsNullOrEmpty(aName)) result += ConstDelimiter + aName.ToUpper();
                    else result += aName.ToUpper();

            return result;
        }
        public static string StringFrom(IEnumerable<string> ids)
        {
            string result = String.Empty;

            if (ids != null)
                foreach (string aName in ids)
                    if (!String.IsNullOrEmpty(result) && !String.IsNullOrEmpty(aName)) result += ConstDelimiter + aName;
                    else result += aName;

            return result;
        }
#endregion
        // hold the names in an array
        protected readonly string[] _ids = (new List<string>()).ToArray();
        /// <summary>
        /// empty constructor - forbidden
        /// </summary>
        private CanonicalName()
        {

        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param id="id"></param>
        public CanonicalName (string id)
        {
            if (!String.IsNullOrEmpty (id)) _ids = id.ToUpper().Split(ConstDelimiter);
        }
        /// <summary>
        /// gets the individual parts of the canonical name
        /// </summary>
        public virtual string[] IDs { get { return _ids;  } }
        /// <summary>
        /// get or sets the Id
        /// </summary>
        public virtual string FullId
        {
            get
            {
                return StringFrom(_ids);
            }
        }
        /// <summary>
        /// returns true if the name is in canonical Form
        /// </summary>
        /// <returns></returns>
        public virtual bool IsCanonical() { return CanonicalName.IsCanonical(this.FullId); }
        /// <summary>
        /// pushes a name on a canonical name and returns it
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual string Push(string id)
        {
            return CanonicalName.Push(this.FullId, id.ToUpper());
        }
        /// <summary>
        /// pops a id on a canonical id and returns it
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        public virtual string Pop(ref string id)
        {
            return CanonicalName.Pop(this.FullId, ref id);
        }
        /// <summary>
        /// pops the last canonical id and resturns the rest
        /// </summary>
        /// <returns></returns>
        public virtual string Pop()
        {
            return CanonicalName.Pop(this.FullId);
        }
        /// <summary>
        /// strips a heading id from the canonical Name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual CanonicalName Reduce(string id)
        {
            return new CanonicalName(CanonicalName.Reduce(this.FullId, id));
        }
        /// <summary>
        /// strips a heading name from this canonical name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual CanonicalName Reduce(CanonicalName name)
        {
            return Reduce(name.FullId);
        }
        /// <summary>
        /// toString()
        /// </summary>
        /// <returns></returns>
        public override string ToString()        { return this.FullId; }
        #region IEqualComparer
        /// <summary>
        /// Equality Comparer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(CanonicalName y)
        {
            return String.Compare (this.FullId, y.FullId, ignoreCase:true)==0;
        }
        /// <summary>
        /// Equality Comparer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override bool Equals(object y)
        {
            if (! (y is CanonicalName)) return false;
            return Equals(this, (CanonicalName)y);
        }
        /// <summary>
        /// compares 2 types by name 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        bool System.Collections.Generic.IEqualityComparer<CanonicalName>.Equals(CanonicalName x, CanonicalName y)
        {
            return String.Compare(x.FullId, y.FullId, ignoreCase: true) == 0;
        }
        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        int System.Collections.Generic.IEqualityComparer<CanonicalName>.GetHashCode(CanonicalName obj)
        {
            return this.IDs.GetHashCode();
        }
        /// <summary>
        /// returns Hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.IDs.GetHashCode();
        }
        /// <summary>
        /// == comparerer on datatypes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(CanonicalName a, CanonicalName b)
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
            return String.Compare(a.FullId, b.FullId, ignoreCase: true) == 0;
        }
        /// <summary>
        /// != comparer
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(CanonicalName a, CanonicalName b)
        {
            return !(a == b);
        }
        #endregion
    }
    /// <summary>
    /// class for working objectnames Modulename = [ID ( . ID )* .] OBJECTID with canonical names
    /// </summary>
    public class ObjectName : CanonicalName
    {
        
        #region Static
        /// <summary>
        /// returns true if the id is in canonical form
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        public new static bool IsCanonical(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            return id.IndexOf(ConstDelimiter) >= 1;
        }
        /// <summary>
        /// returns a Objectname object
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        public static ObjectName From(string id)
        {
            return new ObjectName(id);
        }
        /// <summary>
        /// returns the modulename if one exists - if not then String.Empty
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        public static string GetModuleID(string id)
        {
            if (IsCanonical(id))
            {
                string[] split = id.Split(ConstDelimiter);
                if (split.GetUpperBound(0) > 1)
                {
                    split[split.GetUpperBound(0)] = String.Empty; // use empty feature
                    return StringFrom(split);
                }
                return CanonicalName.GlobalID;
            }
            return id;
        }
        /// <summary>
        /// returns the classname if one exits else String.Empty - if not canonical return id
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        public static string GetObjectID(string id)
        {
            if (IsCanonical(id))
            {
                string[] split = id.Split(ConstDelimiter);
                if (split.GetUpperBound(0) > 1) return split[split.GetUpperBound(0) - 1].ToUpper();
                return string.Empty;
            }
            return id;
        }
        #endregion
        /// <summary>
        /// constructor
        /// </summary>
        /// <param id="id"></param>
        public ObjectName(string id):base(id)
        {
        }
        public ObjectName(string moduleid, string objectid)
            : base((String.IsNullOrEmpty(moduleid) ? (moduleid + ConstDelimiter) : String.Empty)+ objectid)
        {
        }
        /// <summary>
        /// get or sets the Id
        /// </summary>
        public override string FullId
        {
            get
            {
                return base.FullId;
            }
            /*
            set
            {
                if (value == null) base.FullId = String.Empty;
                else
                if (IsCanonical(value)) base.FullId = value;
                else 
                {
                    List<string> names = value.ToUpper().Split(ConstDelimiter).ToList<string>();
                  if (names.Count() < 1) names.Insert(0,String.Empty);
                  _ids = names.ToArray();
                }
            }
            */
        }
        
        /// <summary>
        /// gets or sets the ModuleName
        /// </summary>
        public virtual string ModuleId
        {
            get
            {
                var names = (string[]) _ids.Clone();
                names[names.GetUpperBound(0)] = string.Empty;
                return StringFrom (names);
            }
            /*
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    List<string> names = value.ToUpper().Split(ConstDelimiter).ToList<string>();
                    names.Add(FullId);
                    _ids = names.ToArray();
                }
                else
                {
                    string[] n = { String.Empty, FullId };
                    _ids = n;
                }
            }
            */
        }
        /// <summary>
        /// gets the Objectname itself
        /// </summary>
        public virtual string Id
        {
            get { return _ids[_ids.GetUpperBound(0)]; } set { _ids[_ids.GetUpperBound(0)] = value.ToUpper(); }
        }
        /// <summary>
        /// returns true if the name is in canonical Form
        /// </summary>
        /// <returns></returns>
        public virtual bool IsObjectName() { return IsCanonical(this.FullId); }
        /// <summary>
        /// pushes a id on the Moduleside of the Canonical Id and returns the full id
        /// </summary>
        /// <param id="canonicalName"></param>
        /// <param id="id"></param>
        /// <returns></returns>
        public string PushModule(string id)
        {
            return CanonicalName.Push(ModuleId, id);
        }
        /// <summary>
        /// pops a modulename from the canonical id and returns the rest
        /// </summary>
        /// <param id="canonicalName"></param>
        /// <param id="id"></param>
        /// <returns></returns>
        public string PopModule(ref string id)
        {
            return CanonicalName.Pop(ModuleId, ref id);
        }
    }
    /// <summary>
    /// class for working objectnames with canonical names
    /// </summary>
    public class EntryName : ObjectName
    {
        private readonly string _entryID;

        #region Static
        /// <summary>
        /// returns true if the id is in canonical form
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        public new static bool IsCanonical(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            return id.IndexOf(ConstDelimiter) >= 2;
        }
        /// <summary>
        /// returns a Objectname object
        /// </summary>
        /// <param fullname="fullname"></param>
        /// <returns></returns>
        public new static EntryName From(string fullname)
        {
            return new EntryName(fullname);
        }
        /// <summary>
        /// returns the modulename if one exists - if not then String.Empty
        /// </summary>n
        /// <param id="id"></param>
        /// <returns></returns>
        public new static string GetModuleID(string id)
        {
            if (IsCanonical(id))
            {
                string[] split = id.Split(ConstDelimiter);
                if (split.GetUpperBound(0) > 2)
                {
                    split[split.GetUpperBound(0)] = String.Empty; // use empty feature
                    split[split.GetUpperBound(0)-1] = String.Empty; // use empty feature
                    return StringFrom(split).ToUpper();
                }
                return CanonicalName.GlobalID;
            }
            return id.ToUpper();
        }
        /// <summary>
        /// returns the classname if one exits else String.Empty - if not canonical return id
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        public new static string GetObjectID(string id)
        {
            if (IsCanonical(id))
            {
                string[] split = id.Split(ConstDelimiter);
                if (split.GetUpperBound(0) > 1) return split[split.GetUpperBound(0) - 1].ToUpper();
                return string.Empty;
            }
            return id.ToUpper();
        }
        /// <summary>
        /// returns the classname if one exits else String.Empty - if not canonical return id
        /// </summary>
        /// <param id="id"></param>
        /// <returns></returns>
        public static string GetEntryID(string id)
        {
            if (IsCanonical(id))
            {
                string[] split = id.Split(ConstDelimiter);
                return split[split.GetUpperBound(0)].ToUpper();
            }
            return id.ToUpper();
        }
        #endregion
        /// <summary>
        /// constructor
        /// </summary>
        /// <param id="id"></param>
        public EntryName(string id)
            : base(id)
        {
           
        }
        public EntryName(string moduleid, string objectid, string entryid) 
            : base((!String.IsNullOrEmpty(moduleid) ? (moduleid + ConstDelimiter) : String.Empty) +
                   (!String.IsNullOrEmpty(objectid) ? (objectid + ConstDelimiter) : String.Empty) + 
                   entryid)
        {
            
        }
        /// <summary>
        /// constructor with objectid (including moduleID)
        /// </summary>
        /// <param name="objectid"></param>
        /// <param name="entryid"></param>
        public EntryName(string objectid, string entryid):
            base(!String.IsNullOrEmpty (objectid) ? (objectid + ConstDelimiter + entryid) : entryid)
        {
        }

        /// <summary>
        /// get or sets the Id
        /// </summary>
        public override string FullId
        {
            get
            {
                return base.FullId;
            }
            /*
            set
            {
                if (value == null) base.FullId = String.Empty;
                else
                    if (IsCanonical(value)) base.FullId = value;
                    else
                    {
                        // make sure t add 2 or 1 
                        List<string> names = value.ToUpper().Split(ConstDelimiter).ToList<string>();
                        if (names.Count()== 1){ names.Insert(0, String.Empty);names.Insert(1, String.Empty);}
                        else if (names.Count() == 2) { names.Insert(0, String.Empty);  }
                        _ids = names.ToArray();
                    }
            }
            */
        }
        /// <summary>
        /// gets or sets the ModuleName
        /// </summary>
        public override string ModuleId
        {
            get
            {
                var names = (string[])_ids.Clone();
                names[names.GetUpperBound(dimension:0)] = string.Empty;
                names[names.GetUpperBound(dimension:0)-1] = string.Empty;
                return StringFrom(names);
            }
            /*
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    List<string> names = value.ToUpper().Split(ConstDelimiter).ToList<string>();
                    names.Add(this.ObjectId);
                    names.Add(this.Id);
                    _ids = names.ToArray();
                }
                else
                {
                    string[] n = { String.Empty, ObjectId , this.Id};
                    _ids = n;
                }
            }
            */
        }
        /// <summary>
        /// gets or sets the entry name
        /// </summary>
        public virtual string ObjectId { get { return _ids[_ids.GetUpperBound(0)-1]; } set { _ids[_ids.GetUpperBound(0)-1] = value.ToUpper(); } }
        /// <summary>
        /// gets te obectname object
        /// </summary>
        public virtual ObjectName ObjectName
        {
            get { return new ObjectName(String.IsNullOrEmpty(ModuleId) ? this.ObjectId : (this.ModuleId + ConstDelimiter + ObjectId)); }
        }
        /// <summary>
        /// gets or sets the entry name
        /// </summary>
        public override string Id
        {
            get { return _ids[_ids.GetUpperBound(0)]; } set { _ids[_ids.GetUpperBound(0)] = value.ToUpper(); }
        }
        /// <summary>
        /// returns true if the name is in canonical Form
        /// </summary>
        /// <returns></returns>
        public bool IsEntryName() { return IsCanonical(this.FullId); }
    }
}
