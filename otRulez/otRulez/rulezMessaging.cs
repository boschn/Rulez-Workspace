﻿/**
 *  ONTRACK RULEZ ENGINE
 *  
 * rulez messaging and exceptions
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
using System.Text;

using OnTrack.Rulez.Resources;
using System.Globalization;
using System.Resources;
using System.Diagnostics;

namespace OnTrack.Rulez
{
    /// <summary>
    /// type of messages
    /// </summary>
    public enum MessageType : uint
    {
        Error = 1,
        Warning
    }
    /// <summary>
    /// structure for erors
    /// </summary>
    public struct Message
    {
        public DateTime Timestamp;
        public MessageType Type;
        public int Line;
        public int Pos;
        public string Text;
        public string ID;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param id="type"></param>
        /// <param id="line"></param>
        /// <param id="pos"></param>
        /// <param id="message"></param>
        public Message(MessageType type = MessageType.Error, int line = 0, int pos = 0, string id = null, string message = null)
        {
            this.Timestamp = DateTime.Now;
            this.Type = type;
            this.Line = line;
            this.Pos = pos;
            this.ID = id;
            if (String.IsNullOrEmpty(message)) message = String.Empty;
            if (ID != null && !String.IsNullOrEmpty(Messages.ResourceManager.GetString (id))) 
                message = Messages.ResourceManager.GetString(id) + " " + message;
                
            this.Text = message;
            
        }
        /// <summary>
        /// convert to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0:s}: {1} [Line {2}, Position {3}] {4}", this.Timestamp, this.Type.ToString(), this.Line, this.Pos, this.Text);
        }
    }
    /// <summary>
    /// defines the exception
    /// </summary>
    public class RulezException : Exception
    {
        /// <summary>
        /// RulezException Types
        /// </summary>
        public enum Types
        {
            None = 0,
            NullArgument = 1,
            InvalidOperandNodeType,
            OutOfArraySize,
            OperatorNotDefined,
            OperandsNotEqualOperatorDefinition,
            OperandNull,
            IdNotFound,
            IdExists,
            OperatorTypeNotExpected,
            ValueNotConvertible,
            InvalidNodeType,
            GenerateFailed,
            RunFailed,
            NoDataEngineAvailable,
            StackUnderFlow,
            StackOverFlow,
            HandleNotDefined,
            InvalidNumberOfArguments,
            InvalidCode,
            DataTypeNotImplementedByCase,
            MessagesNotFound,
            DataTypeNotImplementedByClass,
            DataTypeNotFound,
        }
        /// <summary>
        /// fall back messages
        /// </summary>
        private static string[] _messages = {
                                         // None
                                         String.Empty,
                                         // NullArgument
                                         "invalid null argument",
                                         // InvalidOperandNodeType
                                         "invalid type of operand '{0}' - should be implementing INode or IExpression",
                                         // OutOfArraySize
                                         "index '{0}' greater than array size of '{1}'",
                                         // OperatorNotDefined
                                         "operator '{0}' is not defined",
                                         // OperandsNotEqualOperatorDefinition
                                         "for operator '{0}' are '{1}' operands necessary - '{2}' are supplied",
                                         // OperandsNotEqualOperatorDefinition
                                         "for operator '{0}' {1} operand must not be null",
                                         // IdNotFound
                                         "handle '{0}' was not found in context '{1}'",
                                         // IdExits
                                         "handle '{0}' is already defined in context '{1}'",
                                         // OperatorType not Expected
                                         "operator '{0}' is not of expected type {1}",
                                         // ValueNotConvertible
                                         "value '{0}' is not convertible to {1}",
                                          // InvalidNodeType
                                         "invalid type of node '{0}' - expected is {1}",
                                         // GenerateFailed
                                         "Generating rule theCode failed - see inner exception", 
                                         // RunFailed
                                         "Running a rule failed - see inner exception",
                                         // NoDataEngineAvailable
                                         "No data engine for object names '{0}' available",
                                         // Stack Underflow
                                         "Context Stack Underflow Error - no of elements on Stack {0} but {1} elements to be popped off",
                                         // Stack Overflow
                                         "Context Stack Overflow Error",
                                         // Handle not defined
                                         "Code handle for rule '{0}' is not existing in the engine or is null",
                                         // Invalid Number of Arguments
                                         "Rule '{1}' of type '{0}' is expecting {2} arguments - {3} supplied",
                                         // Invalid Code
                                         "Code of handle '{1}' of Rule '{0}' is invalid",
                                         // DataType not Implemented
                                         "Data type with name '{0}' is not implemented in case condition of routine",
                                         // Message Resource file not found
                                         "Messages resource file was not found",
                                         // invalid data type of class
                                         "Data type with name '{0}' cannot be implemented by class '{1}'",
                                          // Datatype not found
                                         "Data type with name '{0}' not found'"
                                         };

        /// <summary>
        /// variables
        /// </summary>
        private Types _id;
        private String _message;
        private String _category;
        private String _Tag;
        private Exception _innerException;
        // static
        private static ResourceManager _ResourceMessages;
        const string MessagePrefix = "REM_"; // prefix for Rulez Exception Messages
        /// <summary>
        /// static constructor
        /// </summary>
        static RulezException ()
        {
           try
           {
               _ResourceMessages = new ResourceManager("OnTrack.Rulez.Resources.Messages", typeof(Messages).Assembly);
           }
           catch (System.Exception ex)
           {
               throw new RulezException(Types.MessagesNotFound, "OnTrack.Rulez.Resources.Messages", inner: ex);
           }
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="message"></param>
        public RulezException(Types id = 0, String message = null, string category = null, string Tag = null, Exception inner = null,  params object[] arguments)
        {
            _id = id;
            _category = category;
            _Tag = Tag;
            if (message != null) _message = message;
            else
            {
                _message = (_ResourceMessages != null) ? _ResourceMessages.GetString(MessagePrefix+id.ToString()) : _messages[(int)id];
                // fall back
                if (_message == null) _message = _messages[(int)id];
            }
            _message = (_message != null ) ? string.Format(_message, arguments) : OnTrack.Core.Converter.Array2StringList (arguments);
            _innerException = inner;

            // log it 
            System.Diagnostics.Debug.Print("Exception " + DateTime.Now.ToString("s") + ":" + ID.ToString() + " " + _message);
        }
        /// <summary>
        /// gets the message string of the exception
        /// </summary>
        public override string Message { get { return _message; } }
        /// <summary>
        /// gets the exception handle
        /// </summary>
        public Types ID { get { return _id; } }
        /// <summary>
        /// gets the category of the exception
        /// </summary>
        public string Category { get { return _category; } }
        /// <summary>
        /// gets the Tag string of the exception
        /// </summary>
        public string Tag { get { return _Tag; } }
    }
}
