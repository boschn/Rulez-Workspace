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
using System.Data;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;
using OnTrack.Core;
using OnTrack.Rulez.Resources;
using OnTrack.Rulez.eXPressionTree;

namespace OnTrack.Rulez
{

    /// <summary>
    /// extensions to RulezParser class (which is the actual parser)
    /// </summary>
    partial class RulezParser
    {
       
        /// <summary> 
        /// structure to hold a parameter definition
        /// </summary>
        public struct ParameterDefinition
        {
            public uint pos;
            public IDataType datatype;
            public string name;
            public IExpression defaultvalue;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="name"></param>
            /// <param name="datatype"></param>
            /// <param name="pos"></param>
            /// <param id="defaultvalue"></param>
            public ParameterDefinition(string name, IDataType datatype, uint pos, IExpression defaultvalue = null)
            { this.name = name; this.datatype = datatype; this.pos = pos; this.defaultvalue = defaultvalue; }

        }
        /// <summary>
        /// structure to hold a variable definition
        /// </summary>
        public struct VariableDefinition
        {
            public IDataType datatype;
            public string Id;
            public INode defaultvalue;
            public VariableDefinition(string name, IDataType datatype, INode defaultvalue = null)
            { this.Id = name; this.datatype = datatype; this.defaultvalue = defaultvalue; }
        }
        /// <summary>
        /// gets or sets the current XPTScope
        /// </summary>
        public XPTScope CurrentScope { get; set; }
        /// <summary>
        /// gets or sets the Engine for the parser
        /// </summary>
        public Engine Engine { get; set; }
        /// <summary>
        /// add a parametername from the current context to a context which has names
        /// </summary>
        /// <returns></returns>
        bool AddVariable(string name, IDataType datatype, LiteralContext literal, RuleContext context)
        {
            RuleContext root = GetRootContext(context, typeof(SelectStatementBlockContext));
            VariableDefinition def = new VariableDefinition(name: name, datatype: datatype, defaultvalue: literal.XPTreeNode);
            if (root != null)
            {
                if (!((SelectStatementBlockContext)root).names.ContainsKey(name))
                {
                    ((SelectStatementBlockContext)root).names.Add(name, def);
                    ((StatementBlock)((SelectStatementBlockContext)root).XPTreeNode).AddNewVariable(name, datatype);
                }
                else { this.NotifyErrorListeners(String.Format(Messages.RCM_1, name, "SelectStatementBlock")); return false; }
                return true;
            }
            this.NotifyErrorListeners(String.Format(Messages.RCM_2, name, "SelectStatementBlock"));
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
                // if defined here then return true else if a parent than take next nested Statement Block
                if (((SelectStatementBlockContext)root).names.ContainsKey(name) == true) return true;
                else if (root.Parent != null) return IsVariableName(name, root.Parent);
                return false;

            }
            // this.NotifyErrorListeners(String.Format(Messages.RCM_2, name, "SelectStatementBlock"));
            return false;
        }
        /// <summary>
        /// checks if the name is a variable name and throws an error
        /// </summary>
        /// <returns></returns>
        bool CheckVariableName(string name, RuleContext context)
        {
            RuleContext root = GetRootContext(context, typeof(SelectStatementBlockContext));
            if (root != null)
            {
                if (!((SelectStatementBlockContext)root).names.ContainsKey(name)) 
                    { this.NotifyErrorListeners(String.Format(Messages.RCM_5, name, "SelectStatementBlock")); return false; }
                return true;
            }
            this.NotifyErrorListeners(String.Format(Messages.RCM_2, name, "SelectStatementBlock"));
            return false;
        }
        /// <summary>
        /// add a parametername from the current context to a context which has names
        /// </summary>
        /// <returns></returns>
        bool AddParameter(string name, uint pos, IDataType datatype, LiteralContext defaultvalue, RuleContext context)
        {
            RuleContext root = GetRootContext(context, typeof(SelectionRulezContext));
            INode theDefaultValue = null;

            if (defaultvalue != null) theDefaultValue = defaultvalue.XPTreeNode;

            ParameterDefinition def = new ParameterDefinition(name: name, pos: pos, datatype: datatype, defaultvalue: (IExpression) theDefaultValue);
            if (root != null)
            {
                if (!((SelectionRulezContext)root).names.ContainsKey(name))
                {
                    ((SelectionRulezContext)root).names.Add(name, def);
                    ((SelectionRule)((SelectionRulezContext)root).XPTreeNode).AddNewParameter(name, datatype);
                }
                else 
                { this.NotifyErrorListeners(String.Format(Messages.RCM_3, name, "SelectionRule")); 
                       return false; 
                }

                return true;
            }
            this.NotifyErrorListeners(String.Format(Messages.RCM_4, name, "SelectionRule"));
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

                // if defined here then return true else if a parent than take next nested Statement Block
                if (((SelectStatementBlockContext)root).names.ContainsKey(name) == true) return true;
                else if (root.Parent != null) return IsParameterName(name, root.Parent);
                return false;
            }
            // this.NotifyErrorListeners(String.Format(Messages.RCM_4, id, "SelectionRule"));
            return false;
        }
        /// <summary>
        /// checks if the id is a parameter id and throws an error
        /// </summary>
        /// <returns></returns>
        bool CheckParameterName(string name, RuleContext context)
        {
            RuleContext root = GetRootContext(context, typeof(SelectionRulezContext));
            if (root != null)
            {
                if (!((SelectionRulezContext)root).names.ContainsKey(name))
                { this.NotifyErrorListeners(String.Format(Messages.RCM_6, name, "SelectionRule")); return false; }
                return true;
            }
            this.NotifyErrorListeners(String.Format(Messages.RCM_4, name, "SelectionRule"));
            return false;
        }
        /// <summary>
        /// returns true if this a Data Object class
        /// </summary>
        /// <returns></returns>
        bool IsDataObjectClass(string id, RuleContext context)
        {
            // check the name might be a full name
            return Engine.Has<IObjectDefinition>(name: new ObjectName(id));
        }
        /// <summary>
        /// returns true if this a Data Object class
        /// </summary>
        /// <returns></returns>
        bool IsDataObjectEntry(string id, RuleContext context)
        {
            // check the name might be a full name
            var anEntryName = new EntryName(id);
            string aClassId =  anEntryName.ObjectId ;
            string anEntryId = anEntryName.Id;

            // if we are in the right context
            if (context is DataObjectEntryNameContext)
            {
                var ctx = (DataObjectEntryNameContext)context;
                if (!ctx.Name.IsEntryName() || String.IsNullOrEmpty (ctx.Name.ObjectId )) aClassId = GetDefaultClassName(context);
                else
                {
                    // if classname differs than it is not allowed
                    if (! String.IsNullOrEmpty (aClassId) && string.Compare(ctx.Name.ObjectId, aClassId, true) != 00)
                        this.NotifyErrorListeners(String.Format (Messages.RCM_12, ctx.Name.ObjectId));
                    else aClassId = ctx.Name.ObjectId;
                }
            }
            else if (context is SelectExpressionContext)
            {
                var ctx = (SelectExpressionContext)context;
                string aDefaultname = GetDefaultClassName(ctx);
                if (!(String.IsNullOrEmpty(aDefaultname))) aClassId = aDefaultname;
            }
            else if (context is SelectConditionContext)
            {
                var ctx = (SelectConditionContext)context;
                string aDefaultname = GetDefaultClassName(ctx);
                if (!(String.IsNullOrEmpty(aDefaultname))) aClassId = aDefaultname;
            }
            else if (context is ResultSelectionContext)
            {
                var ctx = (ResultSelectionContext)context;
                string aDefaultname = GetDefaultClassName(ctx);
                if (string.IsNullOrEmpty(ctx.ClassName)) aClassId = GetDefaultClassName(context);
                else if (!String.IsNullOrWhiteSpace(ctx.ClassName)) aClassId = ctx.ClassName;
            }

            // check if DataObjectEntry is there
            if (!string.IsNullOrWhiteSpace(aClassId) && Engine.Has<IObjectDefinition>(anEntryName.ObjectName))
                return (Engine.Has<IObjectEntryDefinition>(anEntryName));
            // no way to get classname and entryname
            return false;
        }
        /// <summary>
        /// checks if the name is a unique rule id and throws an error
        /// </summary>
        /// <returns></returns>
        public bool CheckUniqueSelectionRuleId(string id)
        {
            if (Engine.Has<ISelectionRule> (new ObjectName(id)))
            {
                this.NotifyErrorListeners(String.Format(Messages.RCM_7, id)); 
                return false;
            }
            return true;
        }
        /// <summary>
        /// returns the ancestor node context of a certain type of root from this context
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static RuleContext GetRootContext(RuleContext context, System.Type type)
        {
            if (context.GetType() == type) return context;
            if (context.Parent == null) return null;

            return GetRootContext(context.Parent, type);
        }
        /// <summary>
        /// returns an Objectname from an array of contexts texts
        /// </summary>
        /// <param name="contexts"></param>
        /// <returns></returns>
        public CanonicalName GetCanonicalName(RuleContext current, RuleContext[] contexts)
        {
            string anID = String.Empty;
            if (contexts != null)
                for (uint i = 0; i < contexts.Length; i++)
                {
                    if (String.IsNullOrEmpty(anID)) anID = contexts[i].GetText();
                    else anID += CanonicalName.ConstDelimiter + contexts[i].GetText();
                }

            return new CanonicalName(anID);
        }
        /// <summary>
        /// returns an Objectname from an array of contexts texts
        /// </summary>
        /// <param name="contexts"></param>
        /// <returns></returns>
        public ObjectName GetCanonicalObjectName(RuleContext current,RuleContext[] contexts)
        {
            string anID = String.Empty;
           
            if (contexts != null)
            for ( uint i = 0; i < contexts.Length; i++)
            {
                if (String.IsNullOrEmpty(anID)) anID = contexts[i].GetText();
                else anID += CanonicalName.ConstDelimiter + contexts[i].GetText();
            }

            var aName = new ObjectName(anID);
            if (aName.IsObjectName()) return aName;
            // we need a modulename
            string aModuleID = GetCurrentScopeID(current);
            return new ObjectName (moduleid:aModuleID, objectid: aName.FullId);
        }
        /// <summary>
        /// returns an Objectname from an array of contexts texts
        /// </summary>
        /// <param name="contexts"></param>
        /// <returns></returns>
        public EntryName GetCanonicalEntryName(RuleContext current, RuleContext[] contexts)
        {
            string anID = String.Empty;
            if (contexts != null)
                for (uint i = 0; i < contexts.Length; i++)
                {
                    if (String.IsNullOrEmpty(anID)) anID = contexts[i].GetText();
                    else anID += CanonicalName.ConstDelimiter + contexts[i].GetText();
                }
            var aName = new EntryName(anID);
            if (aName.IsEntryName()) return aName;
            // we need a modulename
            string aModuleID = GetCurrentScopeID(current);
            return new EntryName(objectid: aName.FullId, entryid: anID);
        }
        /// <summary>
        /// returns the ancestor node context of a certain type of root from this context
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetDefaultClassName(RuleContext context)
        {
            if (context.GetType() == typeof(SelectionContext)) return ((SelectionContext) context).dataObject.GetText ();
            if (context.Parent == null) return null;

            return GetDefaultClassName(context.Parent);
        }
        /// <summary>
        /// returns the ancestor node context of a certain type of root from this context
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetCurrentScopeID(RuleContext context)
        {
            // select a context where a scope id can be found 
            // if (context.GetType() == typeof(SelectionContext)) return ((SelectionContext)context).dataObject.GetText();
            if (context.Parent == null) return CanonicalName.GlobalID;

            return GetCurrentScopeID(context.Parent);
        }
        /// <summary>
        /// returns true if the id is a data type name of an optional given typeid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool IsDataType(string id, otDataType? typeid = null)
        {
            var aName = new CanonicalName(id);
            if (this.Engine != null)
            {
                if (this.Engine.Has<IDataType>(aName))
                    if (typeid.HasValue)
                    {
                        foreach (IDataType aDatatype in this.Engine.Get<IDataType>(aName))
                            if (aDatatype.TypeId == typeid.Value) return true;
                    }
                    else return true;
            }
            return false;
        }
        /// <summary>
        /// increase the key position depending on the logical Operator
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
         public uint incIncreaseKeyNo(SelectConditionsContext ctx)
         {
             
             // increase only if the last operator was an AND/ANDALSO
             if (ctx.logicalOperator_2() != null)
             {
                 
                 // if there is an AND or ANDALSO
                 if (ctx.logicalOperator_2().Last().Operator.Token.ToUint == Token.AND || ctx.logicalOperator_2().Last().Operator.Token.ToUint == Token.ANDALSO)
                 {
                     // check if the last named entry is a key -> reposition for unamed defaults
                     // (100,200,uid=150,250) -> keypos 1,2,1,2
                     if (ctx.selectCondition() != null  )
                     {
                         if (ctx.selectCondition().Last().dataObjectEntry != null)
                         {
                             DataObjectEntrySymbol aSymbol = (DataObjectEntrySymbol)ctx.selectCondition().Last().dataObjectEntry.XPTreeNode;
                             if (aSymbol.CheckValidity().HasValue && aSymbol.IsValid.Value == true)
                                 if (aSymbol.ObjectDefinition.Keys.Contains(aSymbol.Entryname))
                                 {
                                     int pos = Array.FindIndex(aSymbol.ObjectDefinition.Keys, x => String.Compare(x, aSymbol.Entryname.ToUpper(), true) == 0);
                                     if (pos >= 0) ctx.keypos = (uint)pos+1; // keypos is starting from 1 ... -> will be increased to next key lower down
                                 }
                         }
                     }

                     // simply increase and return
                     return ctx.keypos++;
                 }
                
             }
                         

             return ctx.keypos;

         }
       
        /// <summary>
        /// register a Messagehandler to the node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool RegisterMessages(INode node)
        {
            bool result = false;
            // register at the listener our event listener
            EventHandler<RulezParser.MessageListener.EventArgs> handler = (s,e) => node.Messages.Add(e.Message);
            foreach (var aListener in this.ErrorListeners )
                if (aListener is MessageListener)
                { ((MessageListener)aListener).OnMessageAdded += handler; result = true; 
                }
            return result;
        }
        /// <summary>
        /// register a Messagehandler to the node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool DeRegisterMessages(INode node)
        {
            bool result = false;
            // register at the listener our event listener
            EventHandler<RulezParser.MessageListener.EventArgs> handler = (s,e) => node.Messages.Add(e.Message);
            foreach (var aListener in this.ErrorListeners)
                if (aListener is MessageListener) 
                {
                    ((MessageListener)aListener).ClearOnMessageAddedEvents();
                    result = true;
                }
            return result;
        }

        /// <summary>
        /// Rulez build-in Message Listener
        /// </summary>
        public class MessageListener : Antlr4.Runtime.BaseErrorListener
        {
            /// <summary>
            /// event args
            /// </summary>
            public class EventArgs: System.EventArgs
            {
                public Message Message;

                /// <summary>
                /// constructor
                /// </summary>
                /// <param id="message"></param>
                public EventArgs(Message message)
                {
                    this.Message = message;
                }
            }
       
            /// <summary>
            /// list of errors
            /// </summary>
            private List<Message> _errors = new List<Message>();

            // the OnMessageAdded Event
            public event EventHandler<EventArgs> OnMessageAdded;

            /// <summary>
            /// constructor
            /// </summary>
            public MessageListener()
            {
            }

            /// <summary>
            /// get the errors
            /// </summary>
            public IEnumerable<Message> Errors
            {
                get
                {
                    return _errors.Where(x => x.Type == MessageType.Error);
                }
            }

            /// <summary>
            /// get the warnings
            /// </summary>
            public IEnumerable<Message> Warnings
            {
                get
                {
                    return _errors.Where(x => x.Type == MessageType.Warning);
                }
            }

            public override void ReportAmbiguity(Antlr4.Runtime.Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact, BitSet ambigAlts, ATNConfigSet configs)
            {
            }

            public override void ReportAttemptingFullContext(Parser recognizer, DFA dfa, int startIndex, int stopIndex, BitSet conflictingAlts, SimulatorState conflictState)
            {
            }

            public override void ReportContextSensitivity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, int prediction, SimulatorState acceptState)
            {
            }

            /// <summary>
            /// process the SyntaxError
            /// </summary>
            /// <param name="recognizer"></param>
            /// <param name="offendingSymbol"></param>
            /// <param name="line"></param>
            /// <param name="charPositionInLine"></param>
            /// <param name="msg"></param>
            /// <param name="e"></param>
            public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                // publish the message
                string text = String.Empty;
                if (e is FailedPredicateException) text = String.Format(Messages.RCM_11, offendingSymbol.Text );
                else text = msg;

                Message message = new Message(type: MessageType.Error, line: line, pos: charPositionInLine, message: text);
                if (OnMessageAdded != null) OnMessageAdded(this, new EventArgs(message));
                _errors.Add(message);

                if (charPositionInLine != 00)
                    Console.Out.WriteLine(String.Format("ERROR <{0},{1:D2}>:{2}", line, charPositionInLine, text));
                else
                    Console.Out.WriteLine(String.Format("ERROR <line {0}>:{1}", line, text));
            }
            /// <summary>
            /// clear all events
            /// </summary>
            public void ClearOnMessageAddedEvents()
            {
                foreach (EventHandler<MessageListener.EventArgs> aHandler in this.OnMessageAdded.GetInvocationList())
                    this.OnMessageAdded += aHandler;
            }
        }
    }

   
}
