/**
 *  ONTRACK DATABASE
 *  
 *  Data Types Definition
 * 
 * Version: 1.0
 * Created: 2015-08-26
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

using OnTrack.Core;
using OnTrack.Rulez.eXPressionTree;

namespace OnTrack.Rulez
{
    /// <summary>
    /// defines a value data type object
    /// </summary>
    public class PrimitiveType : DataType, IDataType
    {
  
        //////////////////////////////////////////////////////////////////////////
        /// Static
        //////////////////////////////////////////////////////////////////////////
 #region "Static"

        // Internal
        static readonly Dictionary<uint, PrimitiveType> _primitives = new Dictionary<uint, PrimitiveType>();
         /// <summary>
        /// static constructor
        /// </summary>
        static PrimitiveType ()
        {
            // create all the build in data type descriptions
            foreach (uint value in Enum.GetValues (typeof(otDataType)))
            {
                /// create only until 16
                if (value <= PrimitiveTypeMaxRange)
                    if (!_primitives.ContainsKey(value))
                    {
                        // add the non-nullable value type
                        _primitives.Add(value, new PrimitiveType((otDataType)Enum.ToObject(typeof(otDataType), value), isNullable: false));
                        // add nullable value type (but not for Null itself)
                        if (value != 0) _primitives.Add((value | (uint) otDataType.IsNullable), new PrimitiveType((otDataType)Enum.ToObject(typeof(otDataType), value), isNullable: true));
                    }
            }
        }
        /// <summary>
        /// gets all primitive Types
        /// </summary>
        public static System.Collections.Generic.IEnumerable<IDataType> DataTypes
        {
            get { return _primitives.Values.ToList<IDataType>(); } 
        }
   
        /// <summary>
        /// get the Value Type Data Type object
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="isNullable"></param>
        /// <returns></returns>
        public static PrimitiveType GetPrimitiveType(otDataType typeId, bool isNullable = false)
        {
           typeId = isNullable ? typeId | otDataType.IsNullable : typeId;
           // check if in store
           if (!_primitives .ContainsKey ((uint)typeId)) 
                throw new RulezException(RulezException.Types.DataTypeNotImplementedByClass, arguments: new object[] { typeId.ToString(), "ValueType" });
           // return 
           return _primitives[(uint)typeId];
        }
        /// <summary>
        /// returns the best fit System.Type for a OnTrack Datatype
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public new static System.Type GetNativeType(otDataType typeId)
        {
            switch (typeId)
            {
                case otDataType.@Null:
                    return typeof(void);
                case otDataType.Date:
                    return typeof(DateTime);
                case otDataType.Bool:
                    return typeof(bool);
                case otDataType.Number:
                    return typeof(long);
                case otDataType.Memo:
                    return typeof(string);
                case otDataType.Text:
                    return typeof(string);
                case otDataType.Timespan:
                    return typeof(TimeSpan);
                case otDataType.Decimal:
                    return typeof(double);
                case otDataType.Timestamp:
                    return typeof(DateTime);
                case otDataType.Binary:
                    return typeof(byte[]);
                default:
                    if ((uint)typeId < PrimitiveTypeMaxRange)
                        throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByCase, arguments: new object[] { typeId.ToString(), "Core.DataType.GetTypeFor" });
                    else throw new RulezException(RulezException.Types.DataTypeNotImplementedByClass, arguments: new object[] { typeId.ToString(), "ValueType" });
   
            }
        }
        /// <summary>
        /// returns a default value for the OnTrack Datatypes
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public new static object GetDefaultValue(otDataType typeId)
        {
            // if nullable than return the null
            if ((typeId & otDataType.IsNullable ) == otDataType.IsNullable) return null;
           
            // make a case
            switch (typeId)
            {
                 case otDataType.Null:
                    return null;
                case otDataType.Bool:
                    return false;
                case otDataType.Date:
                    return DateTime.Parse(ConstNullTimestampString).Date;
                /*case otDataType.List:
                    /// To do implement inner Type or accept Object()
                    List<string> aValue = new List<string>();
                    return aValue.ToArray();*/
                case otDataType.Number:
                    return (long)0;
                case otDataType.Memo:
                    return string.Empty;
                case otDataType.Decimal:
                    return (double)0;
                case otDataType.Text:
                    return string.Empty;
                case otDataType.Timespan:
                    return new TimeSpan();
                case otDataType.Timestamp:
                    return DateTime.Parse(DataType.ConstNullTimestampString);
                case otDataType.Binary:
                    return new byte[]{0x00};
                
                default:
                    if ((uint)typeId < PrimitiveTypeMaxRange)
                        throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByCase, arguments: new object[] { typeId.ToString(), "Core.DataType.GetTypeFor" });
                    else throw new RulezException(RulezException.Types.DataTypeNotImplementedByClass, arguments: new object[] { typeId.ToString(), "PrimitiveType" });
   
            }

        }
        /// <summary>
        /// returns true if the value is convertible to the datatype
        /// </summary>
        /// <param name="value"></param>
        /// <param name="outvalue"></param>
        /// <param name="datatype"></param>
        /// <returns></returns>
        public new static bool Is(object value, otDataType typeId)
        {
            switch (typeId)
            {
                case otDataType.Bool:
                    return IsBool(value);
                case otDataType.Date:
                    return IsDate(value);
                case otDataType.List:
                    return IsList(value);
                case otDataType.Number:
                    return IsNumber(value);
                case otDataType.Memo:
                    return IsMemo(value);
                case otDataType.Decimal:
                    return IsDecimal(value);
                case otDataType.Text:
                    return IsText(value);
                case otDataType.Timespan:
                    return IsTimespan(value);
                case otDataType.Timestamp:
                    return IsTimeStamp(value);
                case otDataType.Binary:
                    return IsBinary(value);
                default:
                    throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByCase, arguments: new object[] { typeId.ToString(), "PrimitiveType.Is" });
            }
        }
        /// <summary>
        /// returns true if the value is convertible to the datatype
        /// </summary>
        /// <param name="value"></param>
        /// <param name="outvalue"></param>
        /// <param name="datatype"></param>
        /// <returns></returns>
        public new static bool Is(object value, IDataType datatype)
        {
            return PrimitiveType.Is(value, datatype.TypeId);
        }
        /// <summary>
        /// converts a value to an representing value of the outvalue
        /// </summary>
        /// <param name="value"></param>
        /// <param name="outvalue"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public new static object To(object value, otDataType typeId)
        {
            switch (typeId)
            {
                case otDataType.Null:
                    return null;
                case otDataType.Bool:
                    return ToBool(value);
                case otDataType.Date:
                    return ToDate(value);
                case otDataType.List:
                    return ToList(value);
                case otDataType.Number:
                    return ToNumber(value);
                case otDataType.Memo:
                    return ToMemo(value);
                case otDataType.Decimal:
                    return ToDecimal(value);
                case otDataType.Text:
                    return ToText(value);
                case otDataType.Timespan:
                    return ToTimespan(value);
                case otDataType.Timestamp:
                    return ToTimeStamp(value);
                case otDataType.Binary:
                    return ToBinary(value);
                default:
                    throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByCase, arguments: new object[] { typeId.ToString(), "PrimitiveType.To" });
            }
        }
        // <summary>
        /// converts a value to an representing value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="outvalue"></param>
        /// <param name="datatype"></param>
        /// <returns></returns>
        public new static object To(object value, IDataType datatype)
        {
            return PrimitiveType.To(value, datatype.TypeId);
        }
        /// <summary>
        /// returns true if the value is of otDataType.bool
        /// </summary>
        /// <param id="value"></param>
        /// <returns></returns>
        public new  static bool IsBool(object value)
        {
            // if it is a bool anyway
            if (value != null && (value.GetType() == typeof(bool) || value.GetType() == typeof(Boolean))) return true;

            // try to convert to number if that works -> convertible
            if (value != null)
            {
                bool bvalue;
                if (bool.TryParse(value.ToString(), out bvalue)) return true;
                float fvalue;
                if (float.TryParse(value.ToString(), out fvalue)) return true;
            }

            return false; // not convertible
        }
        /// <summary>
        /// convert a value to otDataType.Bool and return the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new  static bool ToBool(object value)
        {
            // if it is a bool anyway
            if (value != null && (value.GetType() == typeof(bool) || value.GetType() == typeof(Boolean))) return (bool)value;

            // try to convert to number if that works -> convertible
            if (value != null)
            {
                // convert True, False to bool
                bool bvalue;
                if (bool.TryParse(value.ToString(), out bvalue)) return bvalue;
                // every numeric value except 0 is regarded as true
                float fvalue;
                if (float.TryParse(value.ToString(), out fvalue))
                {
                    if (fvalue == 0) return false;
                    else return true;
                }
            }

            if (value == null) value = "(null)";
            // throw exception
            throw new Rulez.RulezException(Rulez.RulezException.Types.ValueNotConvertible, arguments: new object[] { value, "bool" });
        }
        /// <summary>
        /// returns true if the value is of otDataType.Binary
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static bool IsBinary(object value)
        {
            // if it is a bool anyway
            if (value != null && (value.GetType() == typeof(byte) || value.GetType() == typeof(Byte )) && value.GetType().IsArray) return true;

            return false; // not convertible
        }
        /// <summary>
        /// convert a value to otDataType.Binary and return the value
        /// </summary>
        /// <param id="value"></param>
        /// <returns></returns>
        public new static byte[] ToBinary(object value)
        {
            // if it is a bool anyway
            if (IsBinary(value)) return (byte[]) value;

            // try to convert
            if (value is Int16) return BitConverter.GetBytes((Int16) value);
            if (value is Int32) return BitConverter.GetBytes((Int32) value);
            if (value is Int64) return BitConverter.GetBytes((Int64) value);
            if (value is bool) return BitConverter.GetBytes((bool)value);
            if (value is Decimal || value is Double) return BitConverter.GetBytes((Double)value);
            if (value is long) return BitConverter.GetBytes((long)value);
            if (value is DateTime) return BitConverter.GetBytes(((DateTime)value).ToBinary());
            if (value is TimeSpan) return BitConverter.GetBytes(((TimeSpan)value).Ticks);
            if (value is string)
            {
                System.Text.Encoding en = System.Text.Encoding.UTF8;
                return en.GetBytes(value.ToString());
            }

            // throw exception
            throw new Rulez.RulezException(Rulez.RulezException.Types.ValueNotConvertible, arguments: new object[] { value, "bool" });
        }
        /// <summary>
        /// returns true if the value is of otDataType.Date
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new  static bool IsDate(object value)
        {
            // if it is a type anyway
            if (value != null && value.GetType() == typeof(DateTime)) return true;

            // try to convert to number if that works -> convertible
            if (value != null)
            {
                DateTime dtvalue;
                // if this is time (no date -> converted to today) then check with second expression
                // 21.05.2015 10:00 -> is Timestamp not a date !
                if ((DateTime.TryParse(value.ToString(), out dtvalue)) && (dtvalue.TimeOfDay == dtvalue.Date.TimeOfDay)) return true;
            }

            return false; // not convertible
        }
        /// <summary>
        /// convert a value to otDataType.Date and return the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new  static DateTime ToDate(object value)
        {
            // if it is a type anyway
            if (value != null && value.GetType() == typeof(DateTime)) return (DateTime)value;

            // try to convert to datetime
            if (value != null)
            {
                // convert just the date component of the value
                DateTime dtvalue;
                if (DateTime.TryParse(value.ToString(), out dtvalue))
                {
                    return dtvalue.Date;
                }
            }

            if (value == null) value = "(null)";
            // throw exception
            throw new Rulez.RulezException(Rulez.RulezException.Types.ValueNotConvertible, arguments: new object[] { value, "date" });
        }
        /// <summary>
        /// returns true if the value is of otDataType.Timespan
        /// </summary>
        /// <param id="value"></param>
        /// <returns></returns>
        public new static bool IsTimespan(object value)
        {
            // if it is a type anyway
            if (value != null && value.GetType() == typeof(DateTime) || value.GetType() == typeof(TimeSpan)) return true;

            // try to convert to number if that works -> convertible
            if (value != null)
            {
                TimeSpan tsvalue;
                if (TimeSpan.TryParse(value.ToString(), out tsvalue)) return true;
                DateTime dtvalue;
                // if this is time (no date -> converted to today) then check with second expression
                // 21.05.2015 10:00 -> is Timestamp not a timespan !
                if ((DateTime.TryParse(value.ToString(), out dtvalue)) && (dtvalue.TimeOfDay != dtvalue.Date.TimeOfDay)) return true;
            }

            return false; // not convertible
        }
        /// <summary>
        /// convert a value to otDataType.Timespan and return the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new  static TimeSpan ToTimespan(object value)
        {
            // if it is anyway the right type
            if (value != null && value.GetType() == typeof(TimeSpan)) return ((TimeSpan)value);
            if (value.GetType() == typeof(DateTime)) return ((DateTime)value).TimeOfDay;

            // try to convert to datetime
            if (value != null)
            {
                // convert just the timespan
                TimeSpan tsvalue;
                if (TimeSpan.TryParse(value.ToString(), out tsvalue)) return tsvalue;

                // if this is time (no date -> converted to today) then check with second expression
                // 21.05.2015 10:00 -> is Timestamp not a timespan !
                DateTime dtvalue;
                if ((DateTime.TryParse(value.ToString(), out dtvalue)) && (dtvalue.TimeOfDay != dtvalue.Date.TimeOfDay)) return dtvalue.TimeOfDay;
            }

            if (value == null) value = "(null)";
            // throw exception
            throw new Rulez.RulezException(Rulez.RulezException.Types.ValueNotConvertible, arguments: new object[] { value, "time" });
        }
        /// <summary>
        /// returns true if the value is of otDataType.TimeStamp
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new  static bool IsTimeStamp(object value)
        {
            // if it is a type anyway
            if (value != null && value.GetType() == typeof(DateTime)) return true;

            // try to convert to number if that works -> convertible
            if (value != null)
            {
                DateTime dtvalue;
                // if this is time (no date -> converted to today) then check with second expression
                // 21.05.2015 10:00 -> is Timestamp not a timespan !
                if ((DateTime.TryParse(value.ToString(), out dtvalue))) return true;
            }

            return false; // not convertible
        }
        /// <summary>
        /// convert a value to otDataType.Timestamp and return the value
        /// </summary>
        /// <param id="value"></param>
        /// <returns></returns>
        public new static DateTime ToTimeStamp(object value)
        {
            // if it is anyway the right type
            if (value != null && value.GetType() == typeof(DateTime)) return ((DateTime)value);

            // try to convert to datetime
            if (value != null)
            {

                // if this is time (no date -> converted to today) then check with second expression
                // 21.05.2015 10:00 -> is Timestamp not a timespan !
                DateTime dtvalue;
                if ((DateTime.TryParse(value.ToString(), out dtvalue))) return dtvalue;
            }

            if (value == null) value = "(null)";
            // throw exception
            throw new Rulez.RulezException(Rulez.RulezException.Types.ValueNotConvertible, arguments: new object[] { value, "timestamp" });
        }
        /// <summary>
        /// returns true if the value is of otDataType.Decimal
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static bool IsDecimal(object value)
        {
            // if it is a type anyway
            if (value != null && (value.GetType() == typeof(Double) || value.GetType() == typeof(float) || value.GetType() == typeof(Single)
                || value.GetType() == typeof(long) || value.GetType() == typeof(int))) return true;

            // try to convert to number if that works -> convertible
            if (value != null)
            {
                Double dvalue;
                if (Double.TryParse(value.ToString(), out dvalue)) return true;
            }

            return false; // not convertible
        }
        /// <summary>
        /// convert a value to otDataType.Double and return the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static Double ToDecimal(object value)
        {
            // if it is anyway the right type
            if (value != null && value.GetType() == typeof(Double)) return ((Double)value);
            if (value != null && value.GetType() == typeof(Single)) return ((Double)value);
            if (value != null && value.GetType() == typeof(Decimal)) return ((Double)value);

            // try to convert to datetime
            if (value != null)
            {
                Double dvalue;
                if (Double.TryParse(value.ToString(), out dvalue)) return dvalue;
            }

            if (value == null) value = "(null)";
            // throw exception
            throw new Rulez.RulezException(Rulez.RulezException.Types.ValueNotConvertible, arguments: new object[] { value, "numeric" });
        }
        /// <summary>
        /// returns true if the value is of otDataType.Number
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static bool IsNumber(object value)
        {
            // if it is a type anyway
            if (value != null && (value.GetType() == typeof(long) || value.GetType() == typeof(int))) return true;

            // try to convert to number if that works -> convertible
            if (value != null)
            {
                long lvalue;
                if (long.TryParse(value.ToString(), out lvalue)) return true;
            }

            return false; // not convertible
        }
        /// <summary>
        /// convert a value to otDataType.Number and return the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new  static long ToNumber(object value)
        {
            // if it is anyway the right type
            if (value != null && value.GetType() == typeof(long)) return ((long)value);

            // try to convert to datetime
            if (value != null)
            {
                // convert to long
                long lvalue;
                if (long.TryParse(value.ToString(), out lvalue)) return lvalue;
                // loose
                decimal dvalue;
                if (decimal.TryParse(value.ToString(), out dvalue))
                {
                    return (long)Math.Round(dvalue);
                }
            }

            if (value == null) value = "(null)";
            // throw exception
            throw new Rulez.RulezException(Rulez.RulezException.Types.ValueNotConvertible, arguments: new object[] { value, "long" });
        }
        /// <summary>
        /// returns true if the value is of otDataType.Text
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new  static bool IsText(object value)
        {
            // if it is a type anyway
            if (value != null && value.GetType() == typeof(String)) return true;

            // toString
            if (value != null) return true;


            return false; // not convertible
        }
        /// <summary>
        /// convert a value to otDataType.Text and return the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static String ToText(object value)
        {
            // if it is anyway the right type
            if (value != null && value.GetType() == typeof(String)) return ((String)value);

            // try to convert 
            if (value != null)
            {
                // convert to long
                return value.ToString();
            }

            if (value == null) value = "(null)";
            // throw exception
            throw new Rulez.RulezException(Rulez.RulezException.Types.ValueNotConvertible, arguments: new object[] { value, "text" });
        }
        /// <summary>
        /// returns true if the value is of otDataType.Text
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static bool IsMemo(object value)
        {
            // if it is a type anyway
            if (value != null && value.GetType() == typeof(String)) return true;

            // toString
            if (value != null) return true;


            return false; // not convertible
        }
        /// <summary>
        /// convert a value to otDataType.Text and return the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static String ToMemo(object value)
        {
            // if it is anyway the right type
            if (value != null && value is String) return ((String)value);

            // try to convert 
            if (value != null)
            {
                // convert to long
                return value.ToString();
            }

            if (value == null) value = "(null)";
            // throw exception
            throw new Rulez.RulezException(Rulez.RulezException.Types.ValueNotConvertible, arguments: new object[] { value, "text" });
        }
#endregion

        /// <summary>
        /// constructor
        /// </summary
        private PrimitiveType(otDataType typeId, bool isNullable = false) : base(typeId, isNullable, defaultvalue:GetDefaultValue(typeId), id: null, engine:null)
        {
            // check on type
            if ((uint) typeId > PrimitiveTypeMaxRange) 
                throw new RulezException(RulezException.Types.DataTypeNotImplementedByClass,arguments: new object[]  {typeId.ToString (), "ValueType"});
            // no event for Primitives
        }

        /// <summary>
        /// gets the Category
        /// </summary>
        public override otDataTypeCategory Category { get { return otDataTypeCategory.Primitive; } }
        /// <summary>
        /// gets the Signature
        /// </summary>
        public override ISignature Signature
        {
            get
            {
              return new TypeSignature (this.TypeId);
            }
            protected set
            {
                _signature = value;
            }
        }

        /// <summary>
        /// gets the native Type
        /// </summary>
        public override System.Type NativeType { get { return PrimitiveType.GetNativeType(this.TypeId); } }
 }
    /// <summary>
    /// defines a complex data type such as decimal unit or
    /// </summary>
    public abstract class CompositeType : DataType
    {
        // additional combined data types
        protected readonly Dictionary<string, IDataType> _structure = new Dictionary<string, IDataType>();
        /// <summary>
        /// returns the best fit System.Type for a OnTrack Datatype
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public new static System.Type GetNativeType(otDataType typeId)
        {

            // strip off
            if ((typeId & otDataType.IsNullable) == otDataType.IsNullable) typeId ^= otDataType.IsNullable;

            switch (typeId)
            {
                case otDataType.Symbol:
                    return typeof(string);
                case otDataType.DecimalUnit:
                    return typeof(DecimalUnitType.DecimalUnit);
                case otDataType.LanguageText:
                    return typeof(LanguageTextType.LanguageText);
                case otDataType.Tuple:
                    return typeof(TupleType.Tuple);
                default:
                    throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByCase, arguments: new object[] { typeId.ToString(), "Core.DataType.GetNativeType" });

            }
        }
        /// <summary>
        /// returns a default value for the OnTrack Datatypes
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public new static object GetDefaultValue(otDataType typeId)
        {
            // if nullable than return the null
            if ((typeId & otDataType.IsNullable) == otDataType.IsNullable) return null;
           
            switch (typeId)
            {
             case otDataType.Symbol:
                    return String.Empty;
             case otDataType.DecimalUnit:
                    return new DecimalUnitType.DecimalUnit ();
             case otDataType.LanguageText:
                    return new LanguageTextType.LanguageText();
                case otDataType.Tuple:
                    return new TupleType.Tuple();
             default:
                    throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByCase, arguments: new object[] { typeId.ToString(), "Core.DataType.GetDefaultValue" });

            }

        }
        /// <summary>
        /// create a signature
        /// </summary>
        /// <param id="typename"></param>
        /// <param id="structure"></param>
        /// <param id="id"></param>
        /// <returns></returns>
        public static string CreateSignature(IEnumerable<IDataType> structure, bool isNullable = false, string typename = null, string id = null)
        {
            if (!String.IsNullOrWhiteSpace(id)) return id.ToUpper();
            string sig = String.Empty;
            foreach (IDataType dt in structure)
            {
                if (sig != String.Empty) sig += ",";
                sig += dt.Signature;
            }
            return (!String.IsNullOrWhiteSpace(typename) ? typename.ToUpper() : "COMPOSITE") + (isNullable ?  "?" : String.Empty ) + "<" + sig + ">";
        }
        /// <summary>
        /// returns a composite type of typeId if possible to make one
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public new static IDataType GetCompositeType(otDataType typeId)
        {
            switch (typeId)
            {
                case otDataType.Symbol:
                    return SymbolType.GetDataType(engine: Rules.Engine);
                case otDataType.DecimalUnit:
                    // return DecimalUnitType.GetDataType(unit: unit,engine: Rules.Engine);
                case otDataType.LanguageText:
                // return LanguageTextType.GetDataType (cultural: cultural, engine: Rules.Engine);
                case otDataType.Tuple:
                    // not possible without a name
                default:
                   throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByCase, arguments: new object[] { typeId.ToString(), "Core.CompositeType.GetCompositeType" });
                    

            }
        }
        /// <summary>
        /// constructor
        /// </summary
        public CompositeType(string id, otDataType typeId, Boolean isNullable, Engine engine = null, object defaultvalue = null) : base(typeId, isNullable:isNullable, defaultvalue: null, engine: engine, id: id)
        {
        }
        /// <summary>
        /// gets the Category
        /// </summary>
        public override otDataTypeCategory Category { get { return otDataTypeCategory.Composite; } }
        /// <summary>
        /// gets or sets the subtype name
        /// </summary>
        public string ComplexTypeName { get; set; }
        /// <summary>
        /// gets the Signature
        /// </summary>
        public override ISignature Signature 
        {
            get 
            {
                if (_signature == null)
                    _signature = new StructuredTypeSignature(typename: ComplexTypeName, structure: _structure.Values, id: this.Id);
                return _signature;
            } 
        }
        /// <summary>
        /// gets the native Type
        /// </summary>
        public override System.Type NativeType { get { return CompositeType.GetNativeType(this.TypeId); } }
        /// <summary>
        /// returns true if the member by id is part of the structure
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasMember(string id)
        {
            return _structure.ContainsKey (id.ToUpper());
        }
        /// <summary>
        /// adds a Member
        /// </summary>
        /// <param name="id"></param>
        /// <param name="datatype"></param>
        /// <returns></returns>
        protected bool AddMember(string id, IDataType datatype)
        {
            if (_structure.ContainsKey(id.ToUpper())) _structure.Remove(id.ToUpper());
            _structure.Add(id.ToUpper(), datatype);
            return true;
        }
        /// <summary>
        /// returns a Member
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected IDataType GetMember(string id)
        {
            if (HasMember(id.ToUpper())) return _structure[id.ToUpper()];
            return null;
        }
    }
    /// <summary>
    /// defines a decimal with an unit
    /// </summary>
    public class SymbolType : CompositeType
    {
        public const string ConstValue = "VALUE";
        public const string ConstSYMBOL = "SYMBOL";
        public const string ConstTypeName = "SYMBOL";

        // additional combined data types
        protected Dictionary<string, object> _allowedSymboles = new Dictionary<string, object>();
        /// <summary>
        /// returns or creates an anonymous List type from the engine
        /// </summary>
        public static SymbolType GetDataType(Engine engine, otDataType innerTypeId = otDataType.Number, string id = null, bool isNullable = false)
        {
            ISignature sig = new StructuredTypeSignature(structure: new IDataType[] { PrimitiveType.GetPrimitiveType(innerTypeId) }, isNullable: isNullable);
            if (!String.IsNullOrEmpty(id))
            {
                var aName = new ObjectName(id);
                IDataType aDatatype = null;
                if (aName.IsObjectName())
                {
                    // if we have the scope
                    if (engine.HasScope (aName.ModuleId))
                        aDatatype = engine.GetScope(aName.ModuleId).Repository.Get<IDataType>(aName).FirstOrDefault();
                }

                if (aDatatype != null && aDatatype.TypeId == otDataType.Symbol) return (SymbolType)aDatatype;
                // throw error
                if (aDatatype != null) throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { id, aDatatype.Id });
            }
            if (engine.Repository.Has<IDataType>(sig)) return (SymbolType)engine.Repository.Get<IDataType>(sig).FirstOrDefault();
            // create new one
            return new SymbolType(innerTypeId: innerTypeId, isNullable: isNullable, id: id, engine: engine);
        }

        /// <summary>
        /// constructor
        /// </summary
        public SymbolType( otDataType innerTypeId = otDataType.Number, Boolean isNullable= false, Engine engine = null, string id = null) : base(typeId: otDataType.Symbol, isNullable: isNullable, defaultvalue: null, engine: engine, id: id)
        {
            this.ComplexTypeName = ConstTypeName;
            // anonymous name
            if (String.IsNullOrEmpty(id)) _name = new ObjectName(Guid.NewGuid().ToString());
            // define the structure
            AddMember(ConstSYMBOL, DataType.GetDataType(otDataType.Text));
            AddMember(ConstValue, DataType.GetDataType(innerTypeId));
            _signature = new StructuredTypeSignature(_structure.Values, isNullable: isNullable, typename: ConstTypeName, id: id);
            // raise event
            RaiseOnCreation(this, datatype: this, engine: engine);
        }
        /// <summary>
        /// returns all symbols
        /// </summary>
        public IList<String> AllowedSymbols
        {
            get { return _allowedSymboles.Keys.ToList<String>(); }
        }
        /// <summary>
        /// gets the native type of the value
        /// </summary>
        public System.Type NativeTypeValue
        {
            get { return GetNativeType(GetMember(ConstValue).TypeId);}
        }
        /// <summary>
        /// gets the type of the value
        /// </summary>
        public IDataType TypeValue
        {
            get { return GetMember(ConstValue); }
        }
        /// <summary>
        /// returns True if the symbl by Id is allowed in this Type
        /// </summary>
        /// <param name="symbolId"></param>
        /// <returns></returns>
        public bool HasSymbol(string symbolId)
        {
            return _allowedSymboles.ContainsKey(symbolId.ToUpper());
        }
        /// <summary>
        /// add a symbol by ID and the corresponding value (try to convert)
        /// </summary>
        /// <param name="symbolId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddSymbol (string symbolId, object value)
        {
            
            if (HasSymbol(symbolId)) _allowedSymboles.Remove(symbolId.ToUpper());
            _allowedSymboles.Add(symbolId.ToUpper(), DataType.To(value, _structure[ConstValue].TypeId));
            return true;
        }
        /// <summary>
        /// add a symbol by ID and the corresponding value (try to convert)
        /// </summary>
        /// <param name="symbolId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool RemoveSymbol(string symbolId)
        {

            if (HasSymbol(symbolId)) _allowedSymboles.Remove(symbolId.ToUpper());
            return true;
        }
    }
    /// <summary>
    /// defines a decimal with an unit
    /// </summary>
    public class DecimalUnitType : CompositeType
    {
        public const string ConstValue = "VALUE";
        public const string ConstUnit = "UNIT";
        public const string ConstTypeName = "DECIMALUNIT";
        
        /// <summary>
        /// structure to hold native a DecimalUnitType Value
        /// </summary>
        public struct DecimalUnit
        {
            public decimal value;
            public long unit;
            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="value"></param>
            /// <param name="unit"></param>
            DecimalUnit(decimal value, long unit)
            {
                this.value = value;
                this.unit = unit;
            }
        }

        #region "Static"
        /// <summary>
        /// returns or creates a type from the engine
        /// </summary>
        public static DecimalUnitType GetDataType(SymbolType unit, Engine engine, string id = null, bool isNullable = false)
        {
            ISignature sig = new StructuredTypeSignature(structure: new IDataType[] { PrimitiveType.GetPrimitiveType(otDataType.DecimalUnit), unit }, isNullable: isNullable);
            if (!String.IsNullOrEmpty(id))
            {
                var aName = new ObjectName(id);
                IDataType aDatatype = null;
                if (aName.IsObjectName())
                {
                    // if we have the scope
                    if (engine.HasScope(aName.ModuleId))
                        aDatatype = engine.GetScope(aName.ModuleId).Repository.Get<IDataType>(aName).FirstOrDefault();
                }

                if (aDatatype != null && aDatatype.TypeId == otDataType.DecimalUnit) return (DecimalUnitType)aDatatype;
                // throw error
                if (aDatatype != null) throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { id, aDatatype.Id });
            }
            // get it by signature
            if (engine.Repository.Has<IDataType>(sig)) return (DecimalUnitType)engine.Repository.Get<IDataType>(sig).FirstOrDefault();
            // create new one
            return new DecimalUnitType(unit: unit, isNullable: isNullable, id: id, engine: engine);
        }

        #endregion
        /// <summary>
        /// constructor
        /// </summary
        public DecimalUnitType(SymbolType unit, string id = null, Boolean isNullable= false, Engine engine = null) : base(typeId: otDataType.DecimalUnit, isNullable:isNullable, defaultvalue: null, engine: engine, id: id)
        {
            // anonymous name
            if (String.IsNullOrEmpty(id)) this.Id = Guid.NewGuid().ToString();
            this.ComplexTypeName = ConstTypeName;
;
            AddMember(ConstValue, PrimitiveType.GetPrimitiveType(otDataType.Decimal));
            AddMember(ConstUnit, unit);
            _signature = new StructuredTypeSignature(_structure.Values, isNullable: isNullable, typename: ConstTypeName, id: id);
            // raise event
            RaiseOnCreation(this, datatype: this, engine: engine);
        }
    }
    /// <summary>
    /// defines a structure - a defined tuple
    /// </summary>
    public class TupleType : CompositeType
    {
        public const string ConstTypeName = "TUPLE";
        /// <summary>
        /// structure to hold native a TupleType Value
        /// </summary>
        public struct Tuple
        {
            object[] _values;
        }

        #region "Static"
        /// <summary>
        /// returns or creates a type from the engine
        /// </summary>
        public new static TupleType GetDataType(IDataType[] structure, Engine engine, string[] memberNames = null, string id = null, bool isNullable = false)
        {
            ISignature sig = new StructuredTypeSignature(structure: structure, typename: ConstTypeName, isNullable: isNullable);
            var aName = new ObjectName(id);
            if (!String.IsNullOrEmpty(id) && engine.Repository.Has<IDataType>(aName))
            {
                IDataType aDatatype = engine.Repository.Get<IDataType>(aName).FirstOrDefault();
                if (aDatatype.TypeId == otDataType.Tuple) return (TupleType)aDatatype;
                throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { id, aDatatype.Id });
            }

            if (engine.Repository.Has<IDataType>(sig)) return (TupleType)engine.Repository.Get<IDataType>(sig).FirstOrDefault();
            // create new one
            return new TupleType(structure: structure, isNullable: isNullable, memberNames: memberNames, id: id, engine: engine);
        }

        #endregion
        /// <summary>
        /// constructor
        /// </summary
        public TupleType(IDataType[] structure, string[] memberNames = null, string id = null, Boolean isNullable = false, Engine engine = null) : base(typeId: otDataType.Tuple, isNullable: isNullable, defaultvalue: null, engine: engine, id: id)
        {
            // anonymous name
            if (String.IsNullOrEmpty(id)) this.Id = Guid.NewGuid().ToString();
            this.ComplexTypeName = ConstTypeName;
            uint i = 0;
            foreach (IDataType aType in structure)
            {
                string anId;
                if (memberNames != null && i <= memberNames.GetUpperBound(0)) anId = memberNames[i];
                else anId = i.ToString();
                AddMember(anId, aType);
                i++;
            }
            _signature = new StructuredTypeSignature(structure: structure, isNullable: isNullable, typename: ConstTypeName, id: id);
            // raise event
            RaiseOnCreation(this, datatype: this, engine: engine);
        }
    }
    /// <summary>
    /// defines a text with several culturals
    /// </summary>
    public class LanguageTextType : CompositeType
    {
        public const string ConstText = "TEXT";
        public const string ConstCultural = "CULTURAL";
        public const string ConstTypeName = "LANGUAGETEXT";
        /// <summary>
        /// structure to hold native a DecimalUnitType Value
        /// </summary>
        public struct LanguageText
        {
            string value;
            long cultural;
            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="value"></param>
            /// <param name="unit"></param>
            LanguageText(string value, long cultural)
            {
                this.value = value;
                this.cultural = cultural;
            }
        }
        /// <summary>
        /// returns or creates an anonymous type from the engine
        /// </summary>
        public static LanguageTextType GetDataType(SymbolType cultural, Engine engine, string id = null, bool isNullable = false)
        {
            ISignature sig = new StructuredTypeSignature(structure: new IDataType[] { PrimitiveType.GetPrimitiveType(otDataType.Text), cultural }, isNullable: isNullable);
            if (!String.IsNullOrEmpty(id))
            {
                ObjectName aName = new ObjectName(id);
                IDataType aDatatype = engine.Get<IDataType>(aName).FirstOrDefault();

                if (aDatatype != null && aDatatype.TypeId == otDataType.LanguageText) return (LanguageTextType)aDatatype;
                // throw error
                if (aDatatype == null) throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { id, aDatatype.Id });
            }
            // get it by signature
            if (engine.Repository.Has<IDataType>(sig)) return (LanguageTextType)engine.Repository.Get<IDataType>(sig).FirstOrDefault();
            // create new one
            return new LanguageTextType(cultural: cultural, isNullable: isNullable, name:id, engine: engine);
        }
        /// <summary>
        /// constructor
        /// </summary
        public LanguageTextType(SymbolType cultural, string name = null,Boolean isNullable=false, Engine engine = null) : base(typeId: otDataType.LanguageText, isNullable: isNullable, defaultvalue: null, engine: engine, id: name)
        {
            this.ComplexTypeName = ConstTypeName;
            AddMember(ConstText, PrimitiveType.GetPrimitiveType(otDataType.Text));
            AddMember(ConstCultural, cultural);
            _signature = new StructuredTypeSignature(_structure.Values, isNullable: isNullable, typename: ConstTypeName, id: name);
            // raise event
            RaiseOnCreation(this, datatype: this, engine: engine);
        }
    }
    /// <summary>
    /// defines a data structure type such as lists
    /// </summary>
    public abstract class DataStructureType : DataType
    {
        protected List<IDataType> _innerTypes = new List<IDataType>();
#region "Static"
        /// <summary>
        /// Get native datatype
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public new static System.Type GetNativeType(otDataType typeId)
        {
            switch (typeId)
            {
                case otDataType.List:
                    return typeof(List<>);
                case otDataType.DataObject:
                    return typeof(IDataObject);
                default:
                    if (typeId != otDataType.List)
                        throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByCase,
                            arguments: new object[] { typeId.ToString(), "Rulez.StructuredType.GetTypeFor" });
                    else throw new RulezException(RulezException.Types.DataTypeNotImplementedByClass,
                        arguments: new object[] { typeId.ToString(), "Structured" });

            }
        }
        /// <summary>
        /// returns a default value for the OnTrack Datatypes
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public new static object GetDefaultValue(otDataType typeId)
        {
            // if nullable than return the null
            if ((typeId & otDataType.IsNullable) == otDataType.IsNullable) return null;
            
            switch (typeId)
            {
                case otDataType.List:
                    return new List<string>();
                case otDataType.DataObject:
                    // return String.Empty;
                default:
                    throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByCase, arguments: new object[] { typeId.ToString(), "Core.StructuredType.GetDefaultValue" });
            }
        }
        /// <summary>
        /// returns a complex type of typeId if possible to make one
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public static IDataType GetStructuredType(otDataType typeId)
        {
            // strip off
            if ((typeId & otDataType.IsNullable) == otDataType.IsNullable) typeId ^= otDataType.IsNullable ;
            switch (typeId)
            {
                case otDataType.List:
                    return ListType.GetDataType (engine: Rules.Engine);
                case otDataType.DataObject:
                    // not implementable
                default:
                    throw new Rulez.RulezException(Rulez.RulezException.Types.DataTypeNotImplementedByCase, arguments: new object[] { typeId.ToString(), "Core.StructuredType.GetStructuredType" });


            }
        }
        /// <summary>
        /// create a signature
        /// </summary>
        /// <param id="typename"></param>
        /// <param id="types"></param>
        /// <param id="id"></param>
        /// <returns></returns>
        public static string CreateSignature(IEnumerable<IDataType> types, bool isNullable = false, string typename = null, string id = null)
        {
            if (!String.IsNullOrWhiteSpace(id)) return id.ToUpper();
            string sig = String.Empty;
            foreach (IDataType dt in types)
            {
                if (sig != String.Empty) sig += ",";
                sig += dt.Signature;
            }
            return (!String.IsNullOrWhiteSpace(typename) ? typename.ToUpper() : "STRUCTURE") + (isNullable ? "?" : String.Empty) + "<" + sig + ">";
        }
        /// <summary>
        /// returns true if the value is of otDataType.Text
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static bool IsList(object value)
        {
            return ListType.IsList(value);
        }
        /// <summary>
        /// convert a value to otDataType.Text and return the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static List<String> ToList(object value)
        {
            return ListType.ToList(value);
        }
        /// <summary>
        /// converts a string of "|aa|bb|" to an array {"aa", "bb"}
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public new static String[] ToArray(String input)
        {
            return ListType.ToArray(input);
        }
        /// <summary>
        /// converts a string of "|aa|bb|" to a list {"aa", "bb"}
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public new static List<String> ToList(String input)
        {
            return ListType.ToList(input);
        }
#endregion
         /// <summary>
        /// constructor
        /// </summary
        public DataStructureType(otDataType typeId, string id = null, bool isNullable = false, Engine engine = null, object defaultvalue = null) : base(typeId: typeId, isNullable: isNullable, defaultvalue: defaultvalue, id: id, engine: engine)
        {
         }
         /// <summary>
         /// gets the Category
         /// </summary>
         public override otDataTypeCategory Category { get { return otDataTypeCategory.DataStructure; } }
         /// <summary>
         /// gets or sets the subtype name
         /// </summary>
         public string StructureTypeName { get; set; }
         /// <summary>
         /// gets the native Type
         /// </summary>
         public override System.Type NativeType { get { return DataStructureType.GetNativeType(this.TypeId); } }
         /// <summary>
         /// gets the Signature
         /// </summary>
         public override ISignature Signature
         {
             get
             {
                if (_signature == null)                   
                    _signature = new StructuredTypeSignature(typename: StructureTypeName, structure: _innerTypes, id: Id);
                return _signature;
             }
         }
    }
    /// <summary>
    /// defines a listed structured type
    /// </summary>
    public class ListType: DataStructureType
    {

       
        // constants
        const string ConstTypeName = "LIST";

#region "Static"
        /// <summary>
        /// returns or creates an anonymous List type from the engine
        /// </summary>
        public static ListType GetDataType(IDataType innerDataType, Engine engine, string id = null, bool isNullable = false)
        {
            ISignature sig = CreateSignature(innerDataType, isNullable);
            if (!String.IsNullOrEmpty(id))
            {
                var aName = new ObjectName(id);
                IDataType aDatatype = engine.Get<IDataType>(aName).FirstOrDefault(); 
           
                if (aDatatype != null && aDatatype.TypeId == otDataType.List) return (ListType)aDatatype;
                // throw error
                if (aDatatype == null) throw new RulezException(RulezException.Types.IdNotFound, arguments: new object[] { id, aDatatype.Id });
                else throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { id, aDatatype.Id });
            }
            if (engine.Repository.Has<IDataType>(sig))
                return (ListType)engine.Repository.Get<IDataType>(sig).FirstOrDefault();
            // create new one
            return new ListType(innerDataType: innerDataType, isNullable: isNullable, id: id, engine: engine);
        }
        
        /// <summary>
        /// returns a stored or new ListType object from otDataType
        /// </summary>
        /// <param id="innerTypeId"></param>
        /// <param id="engine"></param>
        /// <param id="id"></param>
        /// <param id="isNullable"></param>
        /// <returns></returns>
        public static ListType GetDataType(Engine engine, otDataType innerTypeId = otDataType.Text, string id = null, bool isNullable = false)
        {
            IDataType innerDataType = PrimitiveType.GetPrimitiveType(innerTypeId);
            var aName = new ObjectName(id);
            ISignature sig = CreateSignature(innerDataType, isNullable);
            if (!String.IsNullOrEmpty(id) && engine.Repository.Has<IDataType>(aName))
            {
                IDataType aDatatype = engine.Repository.Get<IDataType>(aName).FirstOrDefault();
                if (aDatatype.TypeId == otDataType.List) return (ListType)aDatatype;
                throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { id, aDatatype.Id });
            }
            if (engine.Repository.Has<IDataType>(sig)) return (ListType)engine.Repository.Get<IDataType>(sig).FirstOrDefault();
            // create new one
            return new ListType(innerDataType: innerDataType, isNullable: isNullable, id: id, engine: engine);
        }

        /// <summary>
        /// returns the signature of the ListType
        /// </summary>
        /// <param name="innerDataType"></param>
        /// <param name="isNullable"></param>
        /// <returns></returns>
        public static TypeSignature CreateSignature(IDataType innerDataType,  bool isNullable = false)
        {
            return new TypeSignature( ConstTypeName + (isNullable ? "?" : "") + "<" + innerDataType.Signature + ">"); 
        }
        /// <summary>
        /// returns true if the value is of otDataType.Text
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static bool IsList(object value)
        {
            // if it is a type anyway
            if (value != null && (value.GetType().IsArray || value.GetType().IsAssignableFrom(typeof(List<>)))) return true;
            // toString
            if (value != null && value is String && ((String)value).Contains('|')) return true;
            return false; // not convertible
        }
        /// <summary>
        /// convert a value to otDataType.Text and return the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public new static List<String> ToList(object value)
        {
            // try to convert 
            if (value != null)
            {
                //if (value.GetType().IsAssignableFrom(typeof(List<>))) return ((IEnumerable)value).Cast<object>().Select(x => x.ToString()).ToList(); ;
                if (value.GetType().IsAssignableFrom(typeof(List<>))) return ((IEnumerable)value).Cast<object>().Select(x => x.ToString()).ToList(); ;
                //if (value.GetType().IsArray) return ((IEnumerable)value).Cast<object>().Select(x => x.ToString()).ToList();
                if (value.GetType().IsArray) return ((IEnumerable)value).Cast<object>().Select(x => x.ToString()).ToList();
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
        public new static String[] ToArray(string input)
        {
            if (String.IsNullOrWhiteSpace(input))
            {
                return new String[0];
            }
            else
            {
                return input.Split(ConstDelimiter).Where(x => !String.IsNullOrEmpty(x) && !x.Contains(ConstDelimiter)).ToArray();
            }
        }
        /// <summary>
        /// converts a string of "|aa|bb|" to a list {"aa", "bb"}
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public new static List<string> ToList(string input)
        {
            if (String.IsNullOrWhiteSpace(input))
            {
                return new List<String>();
            }
            else
            {
                return input.Split(ConstDelimiter).Where(x => !String.IsNullOrEmpty(x) && !x.Contains(ConstDelimiter)).ToList();
            }
        }
#endregion
        /// <summary>
        /// constructor
        /// </summary
        public ListType(IDataType innerDataType, string id = null, bool isNullable = false, Engine engine = null) : base(typeId: otDataType.List, isNullable: isNullable, id: id, engine: engine, defaultvalue: null)
        {
            // check on type
            this.StructureTypeName = ConstTypeName;
            this.InnerDataType = innerDataType;
            this.DefaultValue = GetDefaultValue(otDataType.List);
            this.Signature = new TypeSignature(StructureTypeName + (IsNullable ? "?" : "") + "<" + innerDataType.Signature + ">");
            // raise event !
            RaiseOnCreation(this, datatype: this, engine: engine);
        }
        /// <summary>
        /// set or get the inner data type
        /// </summary>
        public IDataType InnerDataType
        {
            get
            {
                return _innerTypes.FirstOrDefault ();
            }
            private set
            {
                if (_innerTypes.Count() == 0) _innerTypes.Add(value);
                else _innerTypes[0] = value;
            }
        }
        
     }
    
    /// <summary>
    /// defines a data object type
    /// </summary>
    public class DataObjectType: DataStructureType
    {
        private Core.IObjectDefinition _objectdefinition;
        /// <summary>
        /// returns or creates an anonymous type from the engine
        /// </summary>
        public static DataObjectType GetDataType(string id, Engine engine)
        {
            if (!String.IsNullOrEmpty(id))
            {
                var aName = new ObjectName(id);
                IDataType aDatatype = engine.Get<IDataType> (aName).FirstOrDefault();
                // throw error
                if (aDatatype == null)
                    throw new RulezException(RulezException.Types.IdExists, arguments: new object[] { id, "not a data object type" });
                

                if (aDatatype.TypeId == otDataType.DataObject || aDatatype.TypeId == (otDataType.IsNullable | otDataType.DataObject))
                    return (DataObjectType)aDatatype;
               
            }
            
            // create new one
            return new DataObjectType(id, engine: engine);
        }
       
        /// <summary>
        /// constructor
        /// </summary
        public DataObjectType(string id, Engine engine=null) : base(otDataType.DataObject, isNullable:true, defaultvalue: null, engine: engine, id: id)
        {
            //_signature = CreateSignature(_structure.Values, isNullable: isNullable, typename: ConstTypeName, name: name);
            // raise event !
            RaiseOnCreation(this, datatype: this, engine: engine);
        }
       
        /// <summary>
        /// gets the Signature
        /// </summary>
        public override ISignature Signature { get { return new TypeSignature(this.Id); } }
        /// <summary>
        /// returns true if the data object exists in Engine
        /// has no value if was not looked up via ObjectDefinition property
        /// </summary>
        public bool? ExistsInEngine { get; private set; }
        /// <summary>
        /// gets the object definition
        /// </summary>
        public Core.IObjectDefinition ObjectDefinition 
        { 
            get 
            {
                if (!ExistsInEngine.HasValue && this.Engine != null)
                {
                    _objectdefinition = this.Engine.Get<IObjectDefinition>(this.Name).FirstOrDefault();
                    this.ExistsInEngine = (_objectdefinition != null) ? true : false;
                }
                return _objectdefinition;
            }
        }
        /// <summary>
        /// gets the native Type
        /// </summary>
        public override System.Type NativeType { get { if (_objectdefinition == null) return DataStructureType.GetNativeType(this.TypeId); return _objectdefinition.ObjectType; } }
    }
}
