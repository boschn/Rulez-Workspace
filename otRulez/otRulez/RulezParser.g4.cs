
/**
 *  ONTRACK RULEZ ENGINE
 *  
 * rulez parser extensions
 * 
 * Version: 1.0
 * Created: 2015-08-14
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
using Antlr4.Runtime;

namespace OnTrack.Rulez
{

    /// <summary>
    /// extensions to RulezParser class (which is the actual parser)
    /// </summary>
    partial class RulezParser
    {
        /// <summary>
        /// add a parametername from the current context to a context which has names
        /// </summary>
        /// <returns></returns>
        bool AddVariableName(string name, uint pos, RuleContext context)
        {
            RuleContext root = GetRootContext(context, typeof(SelectStatementBlockContext));
            if (root != null)
            {
                ((SelectStatementBlockContext)root).names.Add(name, pos);
                return true;
            }

            return false;
        }
        /// <summary>
        /// add a parametername from the current context to a context which has names
        /// </summary>
        /// <returns></returns>
        bool IsVariableName(string name, RuleContext context)
        {
            RuleContext root = GetRootContext(context, typeof(SelectStatementBlockContext));
            if (root != null)
            {
                return ((SelectStatementBlockContext)root).names.ContainsKey(name);
            }

            return false;
        }
        /// <summary>
        /// add a parametername from the current context to a context which has names
        /// </summary>
        /// <returns></returns>
        bool AddParameterName(string name, uint pos, RuleContext context)
        {
            RuleContext root = GetRootContext(context, typeof(SelectionRulezContext));
            if (root != null)
            {
                ((SelectionRulezContext)root).names.Add(name, pos);
                return true;
            }

            return false;
        }
        /// <summary>
        /// add a parametername from the current context to a context which has names
        /// </summary>
        /// <returns></returns>
        bool IsParameterName(string name, RuleContext context)
        {
            RuleContext root = GetRootContext(context, typeof(SelectionRulezContext));
            if (root != null)
            {
                return ((SelectionRulezContext)root).names.ContainsKey(name);
            }

            return false;
        }
        /// <summary>
        /// returns the ancestor node context of a certain type of root from this context
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        RuleContext GetRootContext(RuleContext context, System.Type type)
        {
            if (context.GetType() == type) return context;
            if (context.Parent == null) return null;

            return GetRootContext(context.Parent, type);
        }
        /// <summary>
        /// returns the ancestor node context of a certain type of root from this context
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetDefaultClassName(RuleContext context)
        {
            if (context.GetType() == typeof(SelectionContext)) return ((SelectionContext) context).dataObjectClass ().GetText ();
            if (context.Parent == null) return null;

            return GetDefaultClassName(context.Parent);
        }
    }
}
