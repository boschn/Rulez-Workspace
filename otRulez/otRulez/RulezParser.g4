/***
  ** Rulez Parser language definition
  **
  ** this ANLTR .g4 file defines the par
  **
  ** (C) by Boris Schneider 2015
  **/
parser grammar RulezParser;
options {
    tokenVocab=RulezLexer;
}

/* Rulez -> entry rule for parsing
 */

rulez
    : SELECTION IDENTIFIER (LPAREN parameters RPAREN)? AS selection EOS
    ;

/* Parameterdefinition
 */
parameters
    : parameterdefinition (COLON parameterdefinition)*
    ;

parameterdefinition
    : IDENTIFIER ( AS valuetype )?
    ;

/* Selection expression
 *
 * e.g. deliverables[109] = deliverables[uid=109] -> returns a list with one member which is primary key #1 (UID)
 *		deliverables[(109|110|120)] = deliverables[uid=109 OR uid=110 OR uid = 120] -> returns list with 
 *		deliverables[109, category = "DOC"] = deliverables[UID = 109 AND CATEGORY = "DOC"]
 *		deliverables[(109|110|120), created >= #10.12.2015#] = deliverables[(UID = 109 OR UID = 110 OR UID = 120) AND CREATED >= 10.12.2015]
 */

selection
    :   dataObjectClass L_SQUARE_BRACKET selectConditionExpression+ R_SQUARE_BRACKET 
    ;

selectConditionExpression
    :	selectCondition (logicalOperator selectCondition)*
    ;

 selectCondition
    :	(dataObjectEntryName? compareOperator)? expression
    ;

 logicalOperator
    : AND
    | OR
    | XOR
    | NOT
    ;


/* Expressions
 */

expression
    : literal 
    | parametername
    | dataObjectEntryName
    | ( PLUS | MINUS ) expression
    | expression logicalOperator expression
    | expression arithmeticOperator expression
    | LPAREN expression RPAREN
    ;


arithmeticOperator
    : PLUS | MINUS | DIV | MULT | MODULO | POW
    ;

/* Words and so on
 */

compareOperator
    : EQ | NEQ | GT | GE | LE | LT
    ;

/* Identifier 
 */
valuetype
    : LONG
    | NUMERIC
    | DATE
    | TIMESTAMP
    | TEXT
    | MEMO
    | LIST 
    ;
// Object Class
dataObjectClass
    : IDENTIFIER
    ;
// Object Entry Name
dataObjectEntryName
    : (dataObjectClass DOT)? IDENTIFIER
    ;
// parametername
parametername
    : IDENTIFIER
    ;

/* Literals
 */
literal
	: STRINGLITERAL
    | DECIMALLITERAL
    | DATELITERAL
    | NUMBERLITERAL
    | NOTHING
    | FALSE
    | TRUE
	;