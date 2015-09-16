/**
 *  ONTRACK DATABASE
 *  
 *  DATATYPE ROUTINES
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;

namespace OnTrack.Core
{
    
    /// <summary>
    /// static class Datatype
    /// </summary>
    public abstract class DataType : IDataType
    {
        public const Char ConstDelimiter = '|';
        public const String ConstNullTimestampString = "1900-01-01T00:00:00";
        
        // magic numbers
        protected const uint PrimitiveTypeMaxRange = 15; // magic number as the maximum of the otDataType Enumeration for Value Types
        // Instance data
        private otDataType _type = otDataType.@Null; // datatype
        private Rulez.Engine _engine;
        private object _defaultvalue;
        private string _name;
        /// <summary>
        /// DataType Event Args
        /// </summary>
        public class EventArgs : System.EventArgs
        {
            public EventArgs(IDataType datatype, OnTrack.Rulez.Engine engine)
            {
               this.DataType = datatype;
               this.Engine = engine;
            }
            /// <summary>
            /// get or sets the Datatype
            /// </summary>
            public IDataType DataType
            {
                get;
                private set;
            }
            /// <summary>
            /// get or sets the Engine
            /// </summary>
            public OnTrack.Rulez.Engine Engine
            {
                get;
                private set;
            }
        }
        //////////////////////////////////////////////////////////////////////////
        // Static
        //////////////////////////////////////////////////////////////////////////
        #region "Static"

        public static EventHandler<EventArgs> OnCreation;
        public static EventHandler<EventArgs> OnRemoval;

        /// <summary>
        /// static constructor
        /// </summary>
        static DataType()
        {

        }
        
        /// <summary>
        /// return the category for a otDataType ID
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public static otDataTypeCategory GetCategory(otDataType typeId)
        {
            // extract nullable
            if ((typeId & otDataType.IsNullable) ==  otDataType.IsNullable) typeId ^= otDataType.IsNullable ;
            
            switch (typeId)
            {
                // primitives
                case otDataType.Bool:
                case otDataType.Binary:
                case otDataType.Date:
                case otDataType.Decimal:
                case otDataType.Memo:
                case otDataType.Number:
                case otDataType.Null:
                case otDataType.Text:
                case otDataType.Timespan:
                case otDataType.Timestamp:
                    return otDataTypeCategory.Primitive;
                // complex
                case otDataType.DecimalUnit:
                case otDataType.LanguageText:
                case otDataType.Symbol:
                    return otDataTypeCategory.Composite;
                // data structure
                case otDataType.DataObject:
                case otDataType.List:
                    return otDataTypeCategory.DataStructure;
                // not found
                default:
                    throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByCase, arguments: new object[] { typeId.ToString(), "DataType.GetCategory" });
            }
        }
        /// <summary>
        /// returns a data type object for a typeid
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public static IDataType GetDataType(otDataType typeId)
        {
            switch (GetCategory(typeId))
            {
                case otDataTypeCategory.Primitive:
                    return (IDataType) Rulez.PrimitiveType.GetPrimitiveType(typeId);
                case otDataTypeCategory.Composite:
                    return (IDataType) Rulez.CompositeType.GetCompositeType(typeId);
                case otDataTypeCategory.DataStructure:
                    return (IDataType) Rulez.DataStructureType.GetStructuredType(typeId);
                default:
                    throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByClass, arguments: new object[] { typeId.ToString(), "DataType.GetDataType" });
            }
        }
        /// <summary>
        /// returns the best fit System.Type for a OnTrack Datatype
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public static System.Type GetNativeType(otDataType typeId)
        {
            switch(GetCategory(typeId))
            {
                case otDataTypeCategory.Primitive:
                     return Rulez.PrimitiveType.GetNativeType(typeId);
                case otDataTypeCategory.Composite:
                     return Rulez.CompositeType.GetNativeType(typeId);
                case otDataTypeCategory.DataStructure:
                     return Rulez.DataStructureType.GetNativeType(typeId);
                default:
                     throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByClass, arguments: new object[] { typeId.ToString(), "DataType.GetNativeType" });
            }
        }
        /// <summary>
        /// returns a default value for the OnTrack Datatypes
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public static object GetDefaultValue(otDataType typeId)
        {
            switch (GetCategory(typeId))
            {
                case otDataTypeCategory.Primitive:
                    return Rulez.PrimitiveType.GetDefaultValue(typeId);
                case otDataTypeCategory.Composite:
                    return Rulez.CompositeType.GetDefaultValue(typeId);
                case otDataTypeCategory.DataStructure:
                    return Rulez.DataStructureType.GetDefaultValue(typeId);
                default:
                    throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByClass, arguments: new object[] { typeId.ToString(), "DataType.GetDefaultValue" });
            }
         }
        /// <summary>
        /// returns true if the value is convertible to the datatype
        /// </summary>
        /// <param name="value"></param>
        /// <param name="outvalue"></param>
        /// <param name="datatype"></param>
        /// <returns></returns>
        public static bool Is(object value, otDataType typeId)
        {
            switch (GetCategory(typeId))
            {
                case otDataTypeCategory.Primitive:
                    return Rulez.PrimitiveType.Is(value,typeId);
                default:
                    throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByClass, arguments: new object[] { typeId.ToString(), "DataType.Is" });
            }
        }
        public static bool Is(object value, IDataType datatype)
        {
            switch (datatype.Category)
            {
                case otDataTypeCategory.Primitive:
                    return Rulez.PrimitiveType.Is(value, datatype);
                default:
                    throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByClass, arguments: new object[] { datatype.ToString(), "DataType.Is" });
            }
        }
        /// <summary>
        /// converts a value to an representing value of the outvalue
        /// </summary>
        /// <param name="value"></param>
        /// <param name="outvalue"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public static object To(object value, otDataType typeId)
        {
            switch (GetCategory(typeId))
            {
                case otDataTypeCategory.Primitive:
                    return Rulez.PrimitiveType.To(value, typeId);
                default:
                    throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByClass, arguments: new object[] { typeId.ToString(), "DataType.To" });
            }
        }
        /// <summary>
        /// converts a value to an representing value of the outvalue
        /// </summary>
        /// <param name="value"></param>
        /// <param name="outvalue"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public static object To(object value, IDataType datatype)
        {
            switch (datatype.Category)
            {
                case otDataTypeCategory.Primitive:
                    return Rulez.PrimitiveType.To(value, datatype);
                default:
                    throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByClass, arguments: new object[] { datatype.ToString(), "DataType.To" });
            }
        }
         /// <summary>
         /// returns true if the value is of otDataType.bool
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static bool IsBool(object value)
         {
             return Rulez.PrimitiveType.IsBool(value);
         }
         /// <summary>
         /// convert a value to otDataType.Bool and return the value
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static bool ToBool(object value)
         {
             return Rulez.PrimitiveType.ToBool(value);
         }
         /// <summary>
         /// returns true if the value is of otDataType.Date
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static bool IsBinary(object value)
         {
             return Rulez.PrimitiveType.IsBinary(value);
         }
         /// <summary>
         /// convert a value to otDataType.Date and return the value
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static byte[] ToBinary(object value)
         {
             return Rulez.PrimitiveType.ToBinary(value);
         }
         /// <summary>
         /// returns true if the value is of otDataType.Date
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static bool IsDate(object value)
         {
             return Rulez.PrimitiveType.IsDate(value);
         }
         /// <summary>
         /// convert a value to otDataType.Date and return the value
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static DateTime ToDate(object value)
         {
             return Rulez.PrimitiveType.ToDate(value);
         }
         /// <summary>
         /// returns true if the value is of otDataType.Timespan
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static bool IsTimespan(object value)
         {
             return Rulez.PrimitiveType.IsTimespan(value);
         }
         /// <summary>
         /// convert a value to otDataType.Timespan and return the value
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static TimeSpan ToTimespan(object value)
         {
             return Rulez.PrimitiveType.ToTimespan(value);
         }
         /// <summary>
         /// returns true if the value is of otDataType.TimeStamp
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static bool IsTimeStamp(object value)
         {
             return Rulez.PrimitiveType.IsTimeStamp(value);
         }
         /// <summary>
         /// convert a value to otDataType.Timestamp and return the value
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static DateTime ToTimeStamp(object value)
         {
            return Rulez.PrimitiveType.ToTimeStamp(value);
         }
         /// <summary>
         /// returns true if the value is of otDataType.Decimal
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static bool IsDecimal(object value)
         {
             return Rulez.PrimitiveType.IsDecimal(value);
         }
         /// <summary>
         /// convert a value to otDataType.Double and return the value
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static Double ToDecimal(object value)
         {
             return Rulez.PrimitiveType.ToDecimal(value);
         }
         /// <summary>
         /// returns true if the value is of otDataType.Number
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static bool IsNumber(object value)
         {
             return Rulez.PrimitiveType.IsNumber(value);
         }
         /// <summary>
         /// convert a value to otDataType.Number and return the value
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static long ToNumber(object value)
         {
             return Rulez.PrimitiveType.ToNumber(value);
         }
         /// <summary>
         /// returns true if the value is of otDataType.Text
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static bool IsText(object value)
         {
             return Rulez.PrimitiveType.IsText(value);
         }
         /// <summary>
         /// convert a value to otDataType.Text and return the value
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static String ToText(object value)
         {
             return Rulez.PrimitiveType.ToText(value);
         }
         /// <summary>
         /// returns true if the value is of otDataType.Text
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static bool IsMemo(object value)
         {
             return Rulez.PrimitiveType.IsMemo(value);
         }
         /// <summary>
         /// convert a value to otDataType.Text and return the value
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static String ToMemo(object value)
         {
             return Rulez.PrimitiveType.ToMemo(value);
         }
         /// <summary>
         /// returns true if the value is of otDataType.Text
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static bool IsList(object value)
         {
             // if it is a type anyway
             if (value != null && (value.GetType().IsArray || value.GetType().IsAssignableFrom (typeof(List<>)))) return true;
             

             // toString
             if (value != null && value is String && ((String)value).Contains('|')) return true;

             return false; // not convertible
         }
         /// <summary>
         /// convert a value to otDataType.Text and return the value
         /// </summary>
         /// <param name="value"></param>
         /// <returns></returns>
         public static List<String> ToList(object value)
         {
             // try to convert 
             if (value != null)
             {
                 if (value.GetType().IsAssignableFrom(typeof(List<>))) return ((IEnumerable)value).Cast<object>().Select(x => x.ToString()).ToList(); ;
                 if (value.GetType().IsArray ) return ((IEnumerable ) value).Cast<object>().Select(x => x.ToString()).ToList();
                 return DataType.ToList(value);
             }

             if (value == null) value = "(null)";
             // throw exception
             throw new Rulez.RulezException(Rulez.RulezException.Types.ValueNotConvertible, arguments: new object[] { value, "list" });
         }
        /// <summary>
        /// converts a string of "|aa|bb|" to an array {"aa", "bb"}
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
         public  static String[] ToArray(String input)
        {
            if (String.IsNullOrWhiteSpace (input))
            {
                return new String[0];
            }
            else
            {
                return  input.Split (ConstDelimiter ).Where(x =>  !String.IsNullOrEmpty (x) && !x.Contains (ConstDelimiter)  ).ToArray ();
            }
        }
        /// <summary>
        /// converts a string of "|aa|bb|" to a list {"aa", "bb"}
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<String> ToList(String input)
        {
            if (String.IsNullOrWhiteSpace (input))
            {
                return new List<String> ();
            }
            else
            {
                return  input.Split (ConstDelimiter ).Where(x =>  !String.IsNullOrEmpty (x) && !x.Contains (ConstDelimiter)  ).ToList ();
            }
        }
        /// <summary>
        /// returns a string representation of an enumerable in "|aa|bb|cc|"
        /// returns String.Empty if IEnumerable is empty
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static String ToString(IEnumerable input)
        {
            String result = String.Empty + ConstDelimiter ;
            foreach (var e in input)
            {
                if (e != null) result += e.ToString();
            }
            result += ConstDelimiter;

            if (result != String.Empty + ConstDelimiter + ConstDelimiter) return result;
            return String.Empty;
        }
        #endregion
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="typeId"></param>
        protected DataType(otDataType typeId, bool isNullable = false, object defaultvalue = null, string name = null, Rulez.Engine engine = null)
        {
            _type = isNullable ? typeId | otDataType.IsNullable : typeId;
            _name = (String.IsNullOrWhiteSpace(name)) ? this.Signature : name;
            _engine = engine;
            _defaultvalue = defaultvalue;
        }
        #region "Properties"
        /// <summary>
        /// returns the typeid of the DataType
        /// </summary>
        public otDataType TypeId
        {
            get { return _type; }
        }
        /// <summary>
        /// returns the engine or null for all
        /// </summary>
        public Rulez.Engine Engine
        {
            get { return _engine; }
        }
        /// <summary>
        /// gets the Name of the  Datatype
        /// </summary>
        public string Name
        {
            get { return _name; }
            protected set { _name = value!= null ? value.ToUpper () : System.Guid.NewGuid().ToString ();}
        }
        /// <summary>
        /// gets the Signature of the  Datatype
        /// </summary>
        public abstract string Signature
        {
            get;
        }
        /// <summary>
        /// return true if the type is Nullable
        /// </summary>
        public bool IsNullable 
        { 
            get 
            { 
                    if ((_type & otDataType.IsNullable) == otDataType.IsNullable) return true; 
                    return false; 
            } 
        }
        /// <summary>
        /// gets the default value
        /// </summary>
        public Object DefaultValue 
        { 
            get { return _defaultvalue; }
            protected set { _defaultvalue = value;}
        }
        /// <summary>
        /// gets the Category
        /// </summary>
        public abstract otDataTypeCategory Category { get; }
        /// <summary>
        /// gets the native Type
        /// </summary>
        public abstract System.Type NativeType { get; }
        #endregion
        #region "Events"
        /// <summary>
        /// raise the event on Creation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="datatype"></param>
        protected void RaiseOnCreation(object sender, IDataType datatype, Rulez.Engine engine = null)
        {
            if (engine == null) engine = this.Engine;
            OnCreation(sender, new EventArgs(datatype: datatype, engine: engine));
        }
        /// <summary>
        /// raise the event on Removal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="datatype"></param>
        protected void RaiseOnRemoval(object sender, IDataType datatype, Rulez.Engine engine = null)
        {
            if (engine == null) engine = this.Engine;
            OnRemoval(sender, new EventArgs(datatype: datatype, engine: engine));
        }
        #endregion
        #region "IEqualComparer"
        /// <summary>
        /// Equality Comparer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
         public  bool  Equals(IDataType y)
        {
            return Equals(this, y);
        }
         /// <summary>
         /// Equality Comparer
         /// </summary>
         /// <param name="x"></param>
         /// <param name="y"></param>
         /// <returns></returns>
           bool Equals(object y)
         {
             if (y.GetType().GetInterface(typeof(IDataType).Name, false) == null) return false;
             return Equals(this, (IDataType) y);
         }
        /// <summary>
         /// compares 2 types by name 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
         bool System.Collections.Generic.IEqualityComparer<IDataType>.Equals(IDataType x,IDataType y)
         {
             return String.Compare(x.Signature, y.Signature, ignoreCase: true) == 0;
         }
         /// <summary>
         /// Gets the hash code.
         /// </summary>
         /// <param name="obj">The obj.</param>
         /// <returns></returns>
          int System.Collections.Generic.IEqualityComparer<IDataType>.GetHashCode(IDataType obj)
         {
             return this.Name.GetHashCode();
         }
          /// <summary>
          /// == comparerer on datatypes
          /// </summary>
          /// <param name="a"></param>
          /// <param name="b"></param>
          /// <returns></returns>
          public static bool operator ==(DataType a, DataType b)
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
              return String.Compare(a.Signature, b.Signature, ignoreCase: true) == 0;
          }
          /// <summary>
          /// != comparer
          /// </summary>
          /// <param name="a"></param>
          /// <param name="b"></param>
          /// <returns></returns>
          public static bool operator !=(DataType a, DataType b)
          {
              return !(a == b);
          }
        #endregion
        /// <summary>
        /// to String Method
        /// </summary>
        /// <returns></returns>
        public override string  ToString()
        {
            return this.Signature ;
        }
    }
    /// <summary>
    /// ConverterHelpers
    /// </summary>
    public class Converter
    {
        /// <summary>
        /// converts an array to an a listed string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string Array2StringList(object[] input, char delimiter = ',') 
        {
            int i;
            // Warning!!! Optional parameters not supported
            if (input != null)
            {
                string aStrValue = String.Empty;
                for (i = 0; (i <= input.GetUpperBound(1)); i++)
                {
                    if ((i == 0))
                    {
                        aStrValue = input[i].ToString();
                    }
                    else
                    {
                        aStrValue += delimiter + input[i].ToString();
                    }
                }
                return aStrValue;
            }
            else
            {
                return String.Empty;
            }
        }

        public static string Enumerable2StringList(IEnumerable input, char delimiter =',')
        {
            string aStrValue = String.Empty;
            // Warning!!! Optional parameters not supported
            if ((input == null))
            {
                return String.Empty;
            }
            foreach (var anElement in input)
            {
                string s;
                if ((anElement == null))
                {
                    s = String.Empty;
                }
                else
                {
                    s = anElement.ToString();
                }
                if ((aStrValue == String.Empty))
                {
                    aStrValue = s;
                }
                else
                {
                    aStrValue += delimiter + s;
                }
            }
            return aStrValue;
        }
        /// <summary>
        /// converts to String - if an array then to a a listed string
        /// </summary>
        /// <param name="anObject"></param>
        /// <returns></returns>
        public static String ToString(object anObject)
        {
            if (anObject == null) return String.Empty;

            // convert inenumerables and arrays
            if ((anObject.GetType().IsArray) || (anObject.GetType().GetInterface(typeof(IEnumerable).Name, false) != null))
            {
                String aString = String.Empty + DataType.ConstDelimiter  ;
                foreach (Object anItem in (anObject as IEnumerable )) if (anItem != null) aString += anItem.ToString();
                aString += DataType.ConstDelimiter;
                return aString;
            }

            // convert all others
            return anObject.ToString();
        }
        /// <summary>
        /// return a timestamp in the localTime
        /// </summary>
        /// <param name="datevalue"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string DateTime2LocaleDateTimeString(DateTime datevalue)
        {
            string formattimestamp = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " + System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
            return datevalue.ToString (formattimestamp);
        }

        /// <summary>
        /// return a date in the date localTime
        /// </summary>
        /// <param name="datevalue"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string DateTime2UniversalDateTimeString(DateTime datevalue)
        {
            string formattimestamp = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.UniversalSortableDateTimePattern;
            return datevalue.ToString(formattimestamp);
        }
        /// <summary>
        /// return a date in the date localTime
        /// </summary>
        /// <param name="datevalue"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Date2LocaleShortDateString(System.DateTime datevalue)
        {
            string formattimestamp = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            return datevalue.ToString (formattimestamp);
        }
        /// <summary>
        /// return a date in the date localTime
        /// </summary>
        /// <param name="datevalue"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Time2LocaleShortTimeString(DateTime timevalue)
        {
            string formattimestamp = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
            return timevalue.ToString(formattimestamp);
        }
        /// <summary>
        /// translates an hex integer to argb presentation integer RGB(FF,00,00) = FF but integer = FF0000
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static long Int2ARGB(long value)
        {
            long red = 0;
            long green = 0;
            long blue = 0;
            blue = value & 0xffL;
            green = value / 0x100L & 0xffL;
            red = value / 0x10000 & 0xffL;
            return blue * Convert.ToUInt32 (Math.Pow(255, 2)) + green * 255 + red;
        }

        /// <summary>
        /// returns a color value in rgb to system.drawing.color
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static System.Drawing.Color RGB2Color(long value)
        {
            long red = 0;
            long green = 0;
            long blue = 0;
            red = value & 0xffL;
            green = value / 0x100L & 0xffL;
            blue = value / 0x10000 & 0xffL;
            return System.Drawing.Color.FromArgb(red: Convert.ToInt32(red), green: Convert.ToInt32(green), blue: Convert.ToInt32(blue));
        }

        /// <summary>
        /// returns a color value to hexadecimal (bgr of rgb) 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static long Color2RGB(System.Drawing.Color color)
        {
            long red = 0;
            long green = 0;
            long blue = 0;
            blue = color.B;
            green = color.G;
            red = color.R;
            return blue * Convert.ToUInt32 (Math.Pow(255, 2)) + green * 255 + red;
        }
    }
}
