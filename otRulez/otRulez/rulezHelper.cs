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
    /// class for working with canonical names
    /// </summary>
    public class CanonicalName 
    {
        public const string Global = "";

        #region Static

        public static char ConstDelimiter = '.';

        /// <summary>
        /// returns true if the name is in canonical form
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsCanonical(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return name.IndexOf(ConstDelimiter)>=0;
        }
        /// <summary>
        /// pushes a name on a canonical name and returns it
        /// </summary>
        /// <param name="canonicalName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string Push (string canonicalName, string name)
        {
            if (String.IsNullOrEmpty(canonicalName) || string.Compare (name, Global, true) == 0) return name;
            return canonicalName + ConstDelimiter + name;
        }
        /// <summary>
        /// pops a name on a canonical name and returns it
        /// </summary>
        /// <param name="canonicalName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string Pop(string canonicalName, ref string name)
        {
            if (IsCanonical(canonicalName))
            {
                string[] split = canonicalName.Split(ConstDelimiter);
                if (split.GetUpperBound(0) > 0)
                {
                    name = split[split.GetUpperBound(0) - 1];
                    string result = String.Empty;
                    // get the result string
                    for (uint i = 0; i <= split.GetUpperBound(0); i++)
                        if (i > 0) name += ConstDelimiter + split[i];
                        else name = split[i];
                    return result;
                }
            }
            name = canonicalName;
            return String.Empty;
        }
        /// <summary>
        /// build a canonical String out of an array
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public static string StringFrom(string [] names)
        {
            string result = String.Empty;

            if (names != null)
                foreach (string aName in names)
                    if (!String.IsNullOrEmpty(result) && !String.IsNullOrEmpty(aName)) result += ConstDelimiter + aName;
                    else result += aName;

            return result;
        }
        public static string StringFrom(IEnumerable<string> names)
        {
            string result = String.Empty;

            if (names != null)
                foreach (string aName in names)
                    if (!String.IsNullOrEmpty(result) && !String.IsNullOrEmpty(aName)) result += ConstDelimiter + aName;
                    else result += aName;

            return result;
        }
#endregion
        // hold the names in an array
        protected string[] _names;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name"></param>
        public CanonicalName (string name)
        {
            this.Name = name;
        }
        /// <summary>
        /// get or sets the Name
        /// </summary>
        public string Name
        {
            get
            {
                return StringFrom(_names);
            }
            set
            {
                _names = value.Split(ConstDelimiter);
            }
        }
        /// <summary>
        /// returns true if the name is in canonical Form
        /// </summary>
        /// <returns></returns>
        public bool IsCanonical() { return CanonicalName.IsCanonical(this.Name); }
        /// <summary>
        /// pushes a name on a canonical name and returns it
        /// </summary>
        /// <param name="canonicalName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string Push(string name)
        {
            return CanonicalName.Push(this.Name, name);
        }
        /// <summary>
        /// pops a name on a canonical name and returns it
        /// </summary>
        /// <param name="canonicalName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public  string Pop(ref string name)
        {
            return CanonicalName.Pop(this.Name, ref name);
        }
        /// <summary>
        /// toString()
        /// </summary>
        /// <returns></returns>
        public string ToString()
        { return this.Name; }
    }
    /// <summary>
    /// class for working objectnames Modulename = [ID ( . ID )* .] ID with canonical names
    /// </summary>
    public class ObjectName : CanonicalName
    {
        
        #region Static
        /// <summary>
        /// returns true if the name is in canonical form
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new static bool IsCanonical(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return name.IndexOf(ConstDelimiter) >= 1;
        }
        /// <summary>
        /// returns the modulename if one exists - if not then String.Empty
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetModuleName(string name)
        {
            if (IsCanonical(name))
            {
                string[] split = name.Split(ConstDelimiter);
                if (split.GetUpperBound(0) > 1)
                {
                    split[split.GetUpperBound(0)] = String.Empty; // use empty feature
                    return StringFrom(split);
                }
                return CanonicalName.Global;
            }
            return name;
        }
        /// <summary>
        /// returns the classname if one exits else String.Empty - if not canonical return name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetObjectName(string name)
        {
            if (IsCanonical(name))
            {
                string[] split = name.Split(ConstDelimiter);
                if (split.GetUpperBound(0) > 1) return split[split.GetUpperBound(0) - 1];
                return string.Empty;
            }
            return name;
        }
        #endregion
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name"></param>
        public ObjectName(string name) : base(name)
        {
            this.Name = name;
        }
        /// <summary>
        /// get or sets the Name
        /// </summary>
        public string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                if (value == null) base.Name = String.Empty;
                else
                if (IsCanonical(value)) base.Name = value;
                else 
                {
                  List<string> names = value.Split(ConstDelimiter).ToList<string>();
                  names.Insert(0,String.Empty);
                  _names = names.ToArray();
                }
            }
        }
        /// <summary>
        /// gets or sets the ModuleName
        /// </summary>
        public string ModuleName
        {
            get
            {
                string[] names = (string[]) _names.Clone();
                names[names.GetUpperBound(0)] = string.Empty;
                return StringFrom (names);
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    List<string> names = value.Split(ConstDelimiter).ToList<string>();
                    names.Add(ObjectName);
                    _names = names.ToArray();
                }
                else
                {
                    string[] n = { String.Empty, ObjectName };
                    _names = n;
                }
            }
        }
        /// <summary>
        /// gets the ClassName
        /// </summary>
        public string ObjectName { get { return _names[_names.GetUpperBound(0)]; } set { _names[_names.GetUpperBound(0)] = value; } }
        /// <summary>
        /// returns true if the name is in canonical Form
        /// </summary>
        /// <returns></returns>
        public bool IsObjectName() { return IsCanonical(this.Name); }
        /// <summary>
        /// pushes a name on the Moduleside of the Canonical Name and returns the full name
        /// </summary>
        /// <param name="canonicalName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string PushModule(string name)
        {
            this.ModuleName = CanonicalName.Push(ModuleName, name);
            return this.Name;
        }
        /// <summary>
        /// pops a modulename from the canonical name and returns the rest
        /// </summary>
        /// <param name="canonicalName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string PopModule(ref string name)
        {
            this.ModuleName = CanonicalName.Pop(ModuleName, ref name);
            return this.Name;
        }
    }
    /// <summary>
    /// class for working objectnames with canonical names
    /// </summary>
    public class EntryName : ObjectName
    {

        #region Static
        /// <summary>
        /// returns true if the name is in canonical form
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new static bool IsCanonical(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return name.IndexOf(ConstDelimiter) >= 2;
        }
        /// <summary>
        /// returns the modulename if one exists - if not then String.Empty
        /// </summary>n
        /// <param name="name"></param>
        /// <returns></returns>
        public new static string GetModuleName(string name)
        {
            if (IsCanonical(name))
            {
                string[] split = name.Split(ConstDelimiter);
                if (split.GetUpperBound(0) > 2)
                {
                    split[split.GetUpperBound(0)] = String.Empty; // use empty feature
                    split[split.GetUpperBound(0)-1] = String.Empty; // use empty feature
                    return StringFrom(split);
                }
                return CanonicalName.Global;
            }
            return name;
        }
        /// <summary>
        /// returns the classname if one exits else String.Empty - if not canonical return name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetObjectName(string name)
        {
            if (IsCanonical(name))
            {
                string[] split = name.Split(ConstDelimiter);
                if (split.GetUpperBound(0) > 1) return split[split.GetUpperBound(0) - 1];
                return string.Empty;
            }
            return name;
        }
        /// <summary>
        /// returns the classname if one exits else String.Empty - if not canonical return name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetEntryName(string name)
        {
            if (IsCanonical(name))
            {
                string[] split = name.Split(ConstDelimiter);
                return split[split.GetUpperBound(0)];
            }
            return name;
        }
        #endregion
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name"></param>
        public EntryName(string name)
            : base(name)
        {
            this.Name = name;
        }
        /// <summary>
        /// get or sets the Name
        /// </summary>
        public string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                if (value == null) base.Name = String.Empty;
                else
                    if (IsCanonical(value)) base.Name = value;
                    else
                    {
                        // make sure t add 2 or 1 
                        List<string> names = value.Split(ConstDelimiter).ToList<string>();
                        if (names.Count()== 1){ names.Insert(0, String.Empty);names.Insert(1, String.Empty);}
                        else if (names.Count() == 2) { names.Insert(0, String.Empty);  }
                        _names = names.ToArray();
                    }
            }
        }
        /// <summary>
        /// gets or sets the ModuleName
        /// </summary>
        public string ModuleName
        {
            get
            {
                string[] names = (string[])_names.Clone();
                names[names.GetUpperBound(0)] = string.Empty;
                names[names.GetUpperBound(0)-1] = string.Empty;
                return StringFrom(names);
            }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    List<string> names = value.Split(ConstDelimiter).ToList<string>();
                    names.Add(ObjectName);
                    names.Add(EntryName);
                    _names = names.ToArray();
                }
                else
                {
                    string[] n = { String.Empty, ObjectName , EntryName};
                    _names = n;
                }
            }
        }
        /// <summary>
        /// gets or sets the entry name
        /// </summary>
        public string ObjectName { get { return _names[_names.GetUpperBound(0)-1]; } set { _names[_names.GetUpperBound(0)-1] = value; } }
        /// <summary>
        /// gets or sets the entry name
        /// </summary>
        public string EntryName { get { return _names[_names.GetUpperBound(0)]; } set { _names[_names.GetUpperBound(0)] = value; } }
        /// <summary>
        /// returns true if the name is in canonical Form
        /// </summary>
        /// <returns></returns>
        public bool IsEntryName() { return IsCanonical(this.Name); }
    }
}
