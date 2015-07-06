/*
 * otCSV Grammar for parsing CSV files
 */

grammar otCSV;

tokens { TEXT } 

/*
 * Parser Rules
 */

csvbuffer: header row+ ;
header : row ;

row : field (DELIMITER field)* '\r'? '\n'  ;

field
    : TEXT		# text
    | STRING	# string
    |			# empty
    ;


/*
 * Lexer Rules
 */

//Comments first and check if the '#' is really in the first column
SINGLE_LINE_COMMENT 
  :   (
     '#'  {isFirst()}? Input_character* NEW_LINE_CHARACTER+
	 ) -> channel(HIDDEN)
  ;

STRING : '"' ('""'|~'"')* '"' ; // quote-quote is an escaped quote
TEXT:  {tryText();}  ; // dynamic since delimiter is also dynamic
DELIMITER:  ~["\n\r] {isDelimiter()}?  ; // dynamic Delimiter Token which is set by the property of the LExer

fragment Input_characters
  : Input_character+
  ;
fragment Input_character 
  : ~([\u000D\u000A\u0085\u2028\u2029]) //'<Any Unicode Character Except A NEW_LINE_CHARACTER>'
  ;
fragment NEW_LINE_CHARACTER 
  : '\u000D' //'<Carriage Return Character (U+000D)>'
  | '\u000A' //'<Line Feed Character (U+000A)>'
  | '\u0085' //'<Next Line Character (U+0085)>'
  | '\u2028' //'<Line Separator Character (U+2028)>'
  | '\u2029' //'<Paragraph Separator Character (U+2029)>'
  ;