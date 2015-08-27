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
    public class ValueType : DataType
    {
        /// <summary>
        /// static constructor
        /// </summary>
        static ValueType ()
        {

        }
        /// <summary>
        /// constructor
        /// </summary
        private ValueType(otDataType type, bool isNullable = false, object defaultvalue = null) : base( type,  isNullable,  defaultvalue )
        {
            // check on type
            if ((uint) type <= 16) throw new RulezException(RulezException.Types.DataTypeNotImplementedByClass,arguments: new object[]  {type.ToString (), "ValueType"});

        }
    }
}
