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

        bool tryDelimiter()
        {
            // See if `text` is ahead in the CharStream.
            for (int i = 0; i < this.Delimiter.Length; i++)
            {
                if (_input.La(i + 1) != this.Delimiter[i])
                {
                    // Nope, we didn't find `text`.
                    return false;
                }
            }

            // Since we found the text, increase the CharStream's index.
            // _input.Seek(_input.Index + this.Delimiter .Length );

            return true;
        }
        /// <summary>
        /// check if the Comment is in first column of line
        /// </summary>
        /// <returns></returns>
        bool isFirst()
        {
            if (this.Line == 102)
                System.Console.Write("STOP");
            // return if '#' was not recognized
            if (this.Text == null || (char)this.Text[0] != '#') 
                return false;
             // first character We are here after the '#'
            if (_input.Index ==1) 
                return true;

           
            // NEW_LINE_CHARACTER  ?? -> 2 back sind one back is the '#'
            switch ((int)_input.La(-2))
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
                // NEW_LINE_CHARACTER 
                switch ((int)_input.La(1 + i))
                {
                    case (int) '#': // comment ?!
                        if (i == 0)
                        {
                            // NEW_LINE_CHARACTER  ?? 
                            switch ((int)_input.La(-1))
                            {
                                case (int)'\n': //'<Line Feed Character (U+000A)>'
                                case (int)'\r'://'<Carriage Return Character (U+000D)>'
                                case (int)'\u0085': //'<Next Line Character (U+0085)>'
                                case (int)'\u2028': //'<Line Separator Character (U+2028)>'
                                case (int)'\u2029': //'<Paragraph Separator Character (U+2029)>'
                                    return false;
                                    break;
                            }
                        }
                        break;
                    case (int)'\n': //'<Line Feed Character (U+000A)>'
                    case (int)'\r'://'<Carriage Return Character (U+000D)>'
                    case (int)'\u0085': //'<Next Line Character (U+0085)>'
                    case (int)'\u2028': //'<Line Separator Character (U+2028)>'
                    case (int)'\u2029': //'<Paragraph Separator Character (U+2029)>'
                    case (int)'"':
                        // return false
                        if (i == 0) return false;
                        else
                        {
                            // Since we found the text, increase the CharStream's index.
                            _input.Seek(_input.Index + i  );
                            return true;
                        }
                        break;
                    default:
                        // delimiter ?!
                        if ((char)_input.La(1 + i) == (char)this.Delimiter[0])
                        {
                            // return false
                            if (i == 0)
                                return false;
                            else
                            {
                                // Since we found the text, increase the CharStream's index.
                                _input.Seek(_input.Index + i);
                                return true;
                            }
                        }
                        break;
                }
            }

            // Since we found the text, increase the CharStream's index.
            _input.Seek(_input.Size);
            return true;
        }
    }
}