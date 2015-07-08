/***
  ** Rulez Lexer definition
  **
  **
  **
  ** (C) by Boris Schneider 2015
  **/

lexer grammar RulezLexer;

// End of Statement
fragment EOS
	:	';' | EOF
	;

// keywords
SELECTION : S E L E C T I O N ;
AS : A S ;

// datatypes
LONG : L O N G ;
NUMERIC : N U M E R I C ;
TEXT : T E X T;
MEMO : M E M O ;
DATE : D A T E;
TIMESTAMP : T I M E S T A M P;
LIST : L I S T;

// assignments
ASSIGN : ':=';
MINUS_EQ : '-=';
PLUS_EQ : '+=';
// others
AMPERSAND : '&' ;
COLON :	',' ;
STROKE : '|' ;
HASH : '#' ;
DOT : '.' ;
// parenthesis
LPAREN : '(';
RPAREN : ')';
L_SQUARE_BRACKET : '[';
R_SQUARE_BRACKET : ']';
// logical operators
AND :	(A N D | COLON) ;
OR :	(O R | STROKE) ;
NOT:	(N O T | '!') ;
XOR :	(X O R | '&');
// compare operators
EQ : '=';
NEQ : '<>';
GE : '>=';
GT : '>';
LE : '<=';
LT : '<';
// arithmetic Operation
DIV :  '/';
MINUS : '-';
MULT : '*';
PLUS : '+';
POW : '^';
MODULO: '%';
// literals
STRINGLITERAL : '"' (~["\r\n] | '""')* '"';
DATELITERAL : '#' (~[#\r\n])* '#';
//COLORLITERAL : '&H' [0-9A-F]+ '&'?;
LONGLITERAL : (PLUS|MINUS)? ('0'..'9')+ ('#' | '&')?;
NUMERICLITERAL : (PLUS|MINUS)? ('0'..'9')* '.' ('0'..'9')+ ('#' | '&')?;
TRUE : T R U E ;
FALSE : F A L S E ;
NOTHING : N O T H I N G | N U L L ;
// identifier
IDENTIFIER : LETTER (LETTERORDIGIT)*;

// whitespace, line breaks, comments, ...
Newline
    :   (   '\r' '\n'?
        |   '\n'
        )
        -> skip
    ;

WS : [ \t]+  -> skip;

BlockComment
    :   '/*' .*? '*/'
        -> channel(HIDDEN)
    ;

LineComment
    :   '//' ~[\r\n]*
        -> channel(HIDDEN)
    ;

// letters
fragment LETTER : [a-zA-Z_‰ˆ¸ƒ÷‹];
fragment LETTERORDIGIT : [a-zA-Z0-9_‰ˆ¸ƒ÷‹];

// case insensitive chars
fragment A:('a'|'A');
fragment B:('b'|'B');
fragment C:('c'|'C');
fragment D:('d'|'D');
fragment E:('e'|'E');
fragment F:('f'|'F');
fragment G:('g'|'G');
fragment H:('h'|'H');
fragment I:('i'|'I');
fragment J:('j'|'J');
fragment K:('k'|'K');
fragment L:('l'|'L');
fragment M:('m'|'M');
fragment N:('n'|'N');
fragment O:('o'|'O');
fragment P:('p'|'P');
fragment Q:('q'|'Q');
fragment R:('r'|'R');
fragment S:('s'|'S');
fragment T:('t'|'T');
fragment U:('u'|'U');
fragment V:('v'|'V');
fragment W:('w'|'W');
fragment X:('x'|'X');
fragment Y:('y'|'Y');
fragment Z:('z'|'Z');
