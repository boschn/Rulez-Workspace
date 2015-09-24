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
    public class CanonicalName : string
    {
#region Static

        public static char ConstDot = '.';

        /// <summary>
        /// returns true if the name is in canonical form
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsCanonical(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return name.IndexOf(ConstDot)>=0;
        }
        /// <summary>
        /// returns the entry name of a string or the string if the name is not in canonical form
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string EntryName(string name)
        {
            if (IsCanonical(name))
            {
                string [] split = name.Split (ConstDot);
                return split[split.GetUpperBound (0)];
            }
            return name;
        }
        /// <summary>
        /// returns the classname if one exits else String.Empty - if not canonical return name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ClassName(string name)
        {
            if (IsCanonical(name))
            {
                string [] split = name.Split (ConstDot);
                if (split.GetUpperBound (0)> 1) return split[split.GetUpperBound (0) -1 ];
                return string.Empty;
            }
            return name;
        }
         /// <summary>
        /// returns the modulename if one exists - if not then String.Empty
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ModuleName(string name)
        {
            if (IsCanonical(name))
            {
                string [] split = name.Split (ConstDot);
                if (split.GetUpperBound (0)> 2) 
                { 
                    string modulename = string.Empty;
                    for (uint i = 0; i < split.GetUpperBound (0)-2; i++) 
                        if (i==0) modulename = split[i];
                        else modulename += ConstDot + split[i];
                    return modulename;
                }
                return string.Empty;
            }
            return name;
        }
#endregion
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
        public string Name {get;set;}
        /// <summary>
        /// gets the EntryName
        /// </summary>
        public string EntryName () { return CanonicalName.EntryName(this.Name);}
         /// <summary>
        /// gets the ClassName
        /// </summary>
        public string ClassName () {  return CanonicalName.ClassName(this.Name);}
         /// <summary>
        /// gets the ModuleName
        /// </summary>
        public string ModuleName() {  return CanonicalName.ModuleName(this.Name);}
        /// <summary>
        /// returns true if the name is in canonical Form
        /// </summary>
        /// <returns></returns>
        public bool IsCanonical() { return CanonicalName.IsCanonical(this.Name); }
    }
}
