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

// add the OnTrack eXpression Tree

@header {
// add the eXpression Tree
using OnTrack.Rulez.eXPressionTree;
// add core for datatypes
using OnTrack.Core;
// add Dictionary
using System.Collections.Generic;
}

/* Rulez -> entry rule for parsing
 */


rulezUnit
    : oneRulez ( EOS+ oneRulez )* EOS* EOF
    ;

/* One Rulez
 */
oneRulez
    : selectionRulez
    ;

// Selection Rule with local rule -> in Context
/*
*	selection s (p1 as number ? default 100) as deliverables[p1];
*	selection s as deliverables[uid=p1 as number? default 100]; -> implicit defines a parameter p1 
*/
selectionRulez 
locals [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode , 
		 Dictionary<string,uint> names = new Dictionary<string,uint>() ]
    : SELECTION ruleid (LPAREN parameters RPAREN)? AS ( selectStatementBlock | selection ) 
    ;

// rulename
ruleid
    : IDENTIFIER
    ;

/* Parameterdefinition
 * defines a position no for each paramterdefinition
 */
parameters
locals [ uint pos = 1 ]
    : parameterdefinition [$pos] ( {$pos ++;} COLON parameterdefinition [$pos] )*
    ;

// parameter definition with a default value 
parameterdefinition  [uint pos]
    : IDENTIFIER AS dataType ( DEFAULT literal )? 
		{AddParameterName($ctx.IDENTIFIER().GetText(), $pos, $ctx);}
    ;

/* SelectStatementBlock
 */
selectStatementBlock
locals [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode , 
		 Dictionary<string,uint> names = new Dictionary<string,uint>() ]
	: L_BRACKET selectStatement (EOS+ selectStatement)* R_BRACKET
	;

/* selectStatement in a Select Block
 */
selectStatement
	: selection 
	| variableDeclaration
	| assignment 
	| match 
	| return 
	;

/* Assignment
 */
assignment
	: variableName EQ selectExpression
	| dataObjectEntryName EQ selectExpression
	;

/* Variable Declaration
 */
variableDeclaration
	: IDENTIFIER AS dataType ( DEFAULT literal )? 
		{AddVariableName($ctx.IDENTIFIER().GetText(), 0, $ctx);}
	;
	
/* MATCH 
 */
match
	: MATCH (variableName | parameterName | dataObjectEntryName ) WITH matchcase ( OR matchcase )* 
	;

matchcase
	: selectExpression DO selectStatement
	;

/* return
 */

return
	: RETURN selectExpression
	;

/* Selection of data objects
 *
 * e.g. deliverables[109] = deliverables[uid=109] -> returns a list with one member which is primary key #1 (UID)
 *		deliverables[(109|110|120)] = deliverables[uid=109 OR uid=110 OR uid = 120] -> returns list with 
 *		deliverables[109, category = "DOC"] = deliverables[UID = 109 AND CATEGORY = "DOC"]
 *		deliverables[(109|110|120), created >= #10.12.2015#] = deliverables[(UID = 109 OR UID = 110 OR UID = 120) AND CREATED >= 10.12.2015]
 */

selection
locals [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode,
		 string ClassName ]
    :   dataObjectClass {$ClassName = $ctx.dataObjectClass().GetText();} L_SQUARE_BRACKET selectConditions+ R_SQUARE_BRACKET 
    ;

/* all selection conditions 
 * e.g. 
 * uid = 100, category = "test" 
 *
 * add position counting for keys
 * to enable things like this
 * 100, "test" -> uid = 100 AND category = "test" (uid, category keys)
 * 100 | 101, "test" -> (uid = 100 OR uid = 101) AND category = "test"
 */
selectConditions
locals [  uint pos = 1, OnTrack.Rulez.eXPressionTree.INode XPTreeNode  ]
    :	selectCondition [$pos] (logicalOperator {if ($ctx.logicalOperator($ctx.logicalOperator().GetUpperBound(0)).AND() != null) $pos ++;} selectCondition [$pos] )*
    ;


/* selection condition with position 
 *
 */
 selectCondition [uint pos]
 locals [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
    :	(dataObjectEntryName? compareOperator)? selectExpression
    ;

 logicalOperator
 locals [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode,
		  OnTrack.Rulez.Operator Operator  ]
    : AND { $ctx.Operator = Operator.GetOperator(new Token(Token.ANDALSO));}
    | OR  { $ctx.Operator = Operator.GetOperator(new Token(Token.ORELSE));}
  //| XOR { $ctx.Operator = Operator.GetOperator(new Token(Token.XOR));}
    | NOT { $ctx.Operator = Operator.GetOperator(new Token(Token.NOT));}
    ;


/* Select Expressions
 */

selectExpression
locals [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
    : literal 
    | parameterName
	| variableName
    | dataObjectEntryName
    | ( PLUS | MINUS ) selectExpression
    | selectExpression logicalOperator selectExpression
    | selectExpression arithmeticOperator selectExpression
    | LPAREN selectExpression RPAREN
    ;

/* Arithmetic Operators
 */
arithmeticOperator
locals [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ,
		  OnTrack.Rulez.Operator Operator  ]
    : PLUS { $ctx.Operator = Operator.GetOperator(new Token(Token.PLUS));}
	| MINUS { $ctx.Operator = Operator.GetOperator(new Token(Token.MINUS));}
	| DIV { $ctx.Operator = Operator.GetOperator(new Token(Token.DIV));}
	| MULT { $ctx.Operator = Operator.GetOperator(new Token(Token.MULT));}
	| MODULO { $ctx.Operator = Operator.GetOperator(new Token(Token.MOD));}
	| CONCAT { $ctx.Operator = Operator.GetOperator(new Token(Token.CONCAT));}
    ;

/* Comparison Operators
 */

compareOperator
locals [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ,
		  OnTrack.Rulez.Operator Operator  ]
    : EQ { $ctx.Operator = Operator.GetOperator(new Token(Token.EQ));}
	| NEQ { $ctx.Operator = Operator.GetOperator(new Token(Token.NEQ));}
	| GT  { $ctx.Operator = Operator.GetOperator(new Token(Token.GT));}
	| GE { $ctx.Operator = Operator.GetOperator(new Token(Token.GE));}
	| LE  { $ctx.Operator = Operator.GetOperator(new Token(Token.LE));}
	| LT  { $ctx.Operator = Operator.GetOperator(new Token(Token.LT));}
    ;

/* datatype definition
 * 
 */
dataType
locals []
	: valueType
	| structuredType
	| dataObjectClass
	;

/* structure types
 */
structuredType
locals []
	: LIST OF valueType
	| LIST OF structuredType
	;

/* value types
 */
valueType
locals [ otDataType type]
    : ( NUMBER {$type = otDataType.Number;}
    | DECIMAL {$type = otDataType.Decimal;}
    | DATE {$type = otDataType.Date;}
    | TIMESTAMP {$type = otDataType.Timestamp;}
    | TEXT {$type = otDataType.Text;}
    | MEMO {$type = otDataType.Memo;}
	) (isNullable {$type ^= otDataType.isNullable;})?
    ;

/* nullable
 */
isNullable
    : QUESTIONMARK
    | NULLABLE
    ;

// Object Class
dataObjectClass
locals [ string ClassName ]
    : IDENTIFIER { $ClassName = $ctx.IDENTIFIER().GetText() ;}
    ;

// Object Entry Name
dataObjectEntryName
locals [ string entryname ]
    : (dataObjectClass DOT)?  IDENTIFIER 
	{ if ($ctx.dataObjectClass() == null) $entryname = GetDefaultClassName($ctx) + "." + $ctx.IDENTIFIER().GetText();
	  else $entryname = $ctx.dataObjectClass().ClassName + "." + $ctx.IDENTIFIER().GetText();
	}
    ;

// parameter name
parameterName
    : {IsParameterName($ctx.GetText(),$ctx)}? IDENTIFIER
    ;

// variable name
variableName
    : {IsVariableName($ctx.GetText(),$ctx)}? IDENTIFIER
    ;

/* Literals
 */
literal
locals [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
    : STRINGLITERAL { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal($ctx.GetText(), otDataType.Text); }
    | DECIMALLITERAL  { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal($ctx.GetText(), otDataType.Decimal); }
    | DATELITERAL  { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal($ctx.GetText(), otDataType.Date); }
    | NUMBERLITERAL  { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal($ctx.GetText(), otDataType.Number); }
    | NOTHING  { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal(null, otDataType.Void); }
    | FALSE  { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal(false, otDataType.Bool); }
    | TRUE  { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal(true, otDataType.Bool); }
    ;