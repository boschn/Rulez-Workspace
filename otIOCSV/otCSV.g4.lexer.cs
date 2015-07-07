using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OnTrack.IO.CSV
{
    partial class otCSVLexer
    {
        private String _delimiter = ","; // DEFAULT Token

        /// <summary>
        /// gets or sets the lexer dynamic Delimiter
        /// </summary>
        public String Delimiter
        {
            get
            {
                return _delimiter;
            }set
            {
                _delimiter = value;
            }
        }

        /// <summary>
        /// Testing function for the token
        /// </summary>
        bool isDelimiter()
        {
            if ((char)this.Text[0] == (char)this.Delimiter[0])
                return true;
            return false;
        }

        /// <summary>
        /// check if n-th la is new line character
        /// </summary>
        /// <returns></returns>
        bool isNewLine(int i)
        {
             // first character We are here after the '#'
            if ((_input.Index + i) < 0) 
                return true;
           
            // NEW_LINE_CHARACTER  ?? -> 2 back sind one back is the '#'
            switch ((int)_input.La(i))
            {
                case (int)'\n': //'<Line Feed Character (U+000A)>'
                case (int)'\r'://'<Carriage Return Character (U+000D)>'
                case (int)'\u0085': //'<Next Line Character (U+0085)>'
                case (int)'\u2028': //'<Line Separator Character (U+2028)>'
                case (int)'\u2029': //'<Paragraph Separator Character (U+2029)>'
                    return true;
                    break;
                default:
                    return false;
                    break;
            }
        }
        /// <summary>
        /// try to match a text unless a new line character or a delimiter comes up
        /// </summary>
        /// <returns></returns>

        bool tryText()
        {
            // See if what is ahead in the CharStream.
            for (int i = 0; i < _input.Size - _input.Index; i++)
            {
                // NEW_LINE_CHARACTER  or delimiter
                if (isNewLine(1 + i) || ((char)_input.La(1 + i) == (char)this.Delimiter[0]))
                {
                    // return false if no text is found but a delimiter like ";;"
                    // evolves to empty
                    if (i == 0) return false;
                    else{
                        // Since we found the text, increase the CharStream's index.
                        _input.Seek(_input.Index + i);
                        return true;
                    }
                }
            }

            // Since we found the text, increase the CharStream's index.
            _input.Seek(_input.Size);
            return true;
        }
    }
}