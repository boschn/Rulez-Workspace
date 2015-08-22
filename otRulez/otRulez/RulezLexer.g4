/***
  ** Rulez Lexer definition
  **
  **
  **
  ** (C) by Boris Schneider 2015
  **/

lexer grammar RulezLexer;


// Comments
BlockComment
    :   '/*' .*? '*/'
        -> channel(HIDDEN)
    ;

LineComment
    :   '//' ~[\r\n]*
        -> channel(HIDDEN)
    ;


// keywords
SELECTION : S E L E C T I O N ;
AS : A S ;
DEFAULT: D E F A U L T;
NULLABLE: N U L L A B L E;
OF: O F;
MATCH: M A T C H;
WITH: W I T H;
RETURN: R E T U R N;
DO: D O;

// datatypes
NUMBER : N U M B E R ;
DECIMAL : D E C I M A L ;
TEXT : T E X T;
MEMO : M E M O ;
DATE : D A T E;
TIMESTAMP : T I M E S T A M P;
LIST : L I S T;
TIMESPAN: T I M E S P A N;

// literals
STRINGLITERAL : '"' (~["\r\n] | '""')* '"';
DATELITERAL : '#' (~[#\r\n])* '#';
NUMBERLITERAL : ('+' | '-')? ('0'..'9')+ ;
DECIMALLITERAL :  ('+' | '-')? ('0'..'9')* '.' ('0'..'9')+ ;
TRUE : T R U E ;
FALSE : F A L S E ;
NULL :  N U L L ;

// assignments
ASSIGN : ':=';
MINUS_EQ : '-=';
PLUS_EQ : '+=';

// others
 COLON :	',' ;
 STROKE : '|' ;
 HASH : '#' ;
 DOT : '.' ;
 QUESTIONMARK : '?';
// parenthesis
 LPAREN : '(';
 RPAREN : ')';
 L_SQUARE_BRACKET : '[';
 R_SQUARE_BRACKET : ']';
 L_BRACKET: '{' ;
 R_BRACKET: '}';

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


// End of Statement
EOS : ( ';' )  ;

// identifier
IDENTIFIER : LETTER (LETTERORDIGIT)*;

// letters
fragment LETTER : [a-zA-Z_äöüÄÖÜ];
fragment LETTERORDIGIT : [a-zA-Z0-9_äöüÄÖÜ];

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

// whitespace, line breaks, comments, ...
Newline
    :   (   '\r' '\n'?
        |   '\n'
        )
        -> skip
    ;

WS : [ \t]+  -> skip;


