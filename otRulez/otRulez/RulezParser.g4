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
// Linq Extensions
using System.Linq;
}

/* Rulez -> entry rule for parsing
 */


rulezUnit
returns [OnTrack.Rulez.eXPressionTree.Unit XPTreeNode ]
@init { $XPTreeNode = new OnTrack.Rulez.eXPressionTree.Unit(engine:this.Engine);  RegisterMessages($XPTreeNode);}
@after {  DeRegisterMessages($XPTreeNode);}

    : oneRulez ( EOS+ oneRulez )* EOS* EOF
    ;

/* One Rulez
 */
oneRulez
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
    : selectionRulez
	| typeDeclaration 
	| moduleDeclaration
    ;

/*
 * namespace / Module declaration declaration
 */
moduleDeclaration
returns [ OnTrack.Rulez.IScope Scope, OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
	: MODULE canonicalName
	;
/*
 * type Definition
 */
typeDeclaration
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
locals [  OnTrack.Rulez.IScope Scope ]
	: TYPE typeid AS typeDefinition [$ctx.typeid().GetText()]
	
	;

typeid
	: identifier
	;

/*
 * different type definition
 */
typeDefinition [string id]
	:
		dataType [$id]
	;

/* datatype declaration
 * 
 * if name = null then anonymous name else name is the name the type is saved under
 *
 */
dataType [string id]
returns [ Core.IDataType datatype]
	: primitiveType { $datatype = Rulez.PrimitiveType.GetPrimitiveType($ctx.primitiveType().typeId);}
	| datastructureType [$id] {$datatype = $ctx.datastructureType().datatype;}
	| compositeType [$id] {$datatype = $ctx.compositeType().datatype;}

	// defined data types by name such as data objects, if the save name is null
	|  {$id == null && IsDataType($ctx.GetText())}? identifier { $datatype = this.Engine.Get<IDataType>(new CanonicalName($ctx.identifier().GetText())).FirstOrDefault();}
	;

/* structure types
 * LIST? of DATE, LIST of deliverables
 */
datastructureType [string id]
returns [ Core.IDataType datatype ]
locals [ bool isnullable = false]
	: LIST (isNullable {$isnullable = true;})? OF dataType[null] 
	{ $datatype = Rulez.ListType.GetDataType (innerDataType:$ctx.dataType().datatype, id: id, engine: this.Engine, isNullable: $isnullable);}
	;

/*
 * complex types 
 * if name = null then anonymous name else name is the name the type is saved under
 *
 */
compositeType  [string id]
returns [ Core.IDataType datatype ]
    : symbolTypeDeclaration  [$id] 
	| decimalUnitDeclaration [$id]
	| languageTextDeclaration [$id]
    ;

/*
 * anonymous decimal unit declaration
 * if name = null then anonymous name else name is the name the type is saved under
 *
 * DecimalUnit of Currency 
 * DecimalUnit of ( EUR | USD | CHF )
 */
decimalUnitDeclaration [string id]
returns [ Core.IDataType datatype ]
	: DECIMALUNIT OF symboldecl=symbolTypeDeclaration[null] {$datatype = Rulez.DecimalUnitType.GetDataType( unit: (SymbolType) $ctx.symboldecl.datatype, id: id, engine: this.Engine);  }
	 {IsDataType($ctx.identifier().GetText(),otDataType.Symbol)}? LANGUAGETEXT OF identifier 
		{$datatype = Rulez.DecimalUnitType.GetDataType(unit: (SymbolType) this.Engine.Get<IDataType>(new CanonicalName($ctx.identifier().GetText())), id: id, engine: this.Engine);}

	;

/*
 * anonymous language text declaration
 * if name = null then anonymous name else name is the name the type is saved under
 *
 * LanguageText of Cultural
 * LanguageText of ( DE_de | EN_en )
 */
languageTextDeclaration [string id]
returns [ Core.IDataType datatype ]
	: LANGUAGETEXT OF symboldecl=symbolTypeDeclaration[null] {$datatype = Rulez.LanguageTextType.GetDataType( cultural: (SymbolType) $ctx.symboldecl.datatype, id: id, engine: this.Engine);  }
	| {IsDataType($ctx.identifier().GetText(), otDataType.Symbol)}? LANGUAGETEXT OF identifier 
		{$datatype = Rulez.LanguageTextType.GetDataType(cultural: (SymbolType) this.Engine.Get<IDataType>(new CanonicalName($ctx.identifier().GetText())), id: id,engine: this.Engine);}
	;
/*
 * anonymous canonicalName declaration
 * if name = null then anonymous name else name is the name the type is saved under
 *
 * ( orange | apple | peach )
 */
symbolTypeDeclaration [string id]
returns [ Core.IDataType datatype ]
locals  [uint pos = 1]
@init {$datatype = Rulez.SymbolType.GetDataType(id:$id, innerTypeId: otDataType.Number, engine: this.Engine); } 
	: 
	  LPAREN symbolDeclaration[(SymbolType)$datatype, $pos] ( OR {$pos ++;} symbolDeclaration[(SymbolType)$datatype,$pos] )* RPAREN
	;

/* canonicalName declaration
 *
 *
 */
symbolDeclaration [Rulez.SymbolType datatype, uint pos]
	: identifier {datatype.AddSymbol($ctx.GetText().ToUpper(), Core.DataType.To($pos, otDataType.Number));}
	;

/* base types
 */
primitiveType
locals [ otDataType typeId]
    : ( 
	  NUMBER {$typeId = otDataType.Number;}
    | DECIMAL {$typeId = otDataType.Decimal;}
    | DATE {$typeId = otDataType.Date;}
    | TIMESTAMP {$typeId = otDataType.Timestamp;}
    | TEXT {$typeId = otDataType.Text;}
    | MEMO {$typeId = otDataType.Memo;}
	) (isNullable {$typeId ^= otDataType.IsNullable;})?
    ;

/* nullable
 */
isNullable
    : QUESTIONMARK
    | NULLABLE
    ;


// Selection Rule with local rule -> in Context
/*
*	selection s (p1 as number ? default 100) as deliverables[p1];
*	selection s as deliverables[uid=p1 as number? default 100]; -> implicit defines a parameter p1 
*/
selectionRulez
returns [  OnTrack.Rulez.IScope Scope, OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
locals [ 
		// parameters
		 Dictionary<string,ParameterDefinition> names = new Dictionary<string,ParameterDefinition>() ]
@init{  RegisterMessages($XPTreeNode);}
@after { DeRegisterMessages($XPTreeNode);}

    : SELECTION id = ruleid {$XPTreeNode = new SelectionRule(id:$ctx.id.GetText());} (LPAREN parameters RPAREN)? AS ( selectStatementBlock | selection ) 
	
    ;
// rulename
ruleid
    : canonicalName 
	{ CheckUniqueSelectionRuleId ($ctx.GetText()); }
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
    : identifier AS dataType[null] ( DEFAULT defaultvalue=literal )? 
		{AddParameter($ctx.identifier().GetText(), $pos, $ctx.dataType().datatype, $ctx.defaultvalue,$ctx);}
    ;

/* SelectStatementBlock
 */
selectStatementBlock
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode,  OnTrack.Rulez.IScope Scope ]
locals [
		 // local variables
		Dictionary<string,VariableDefinition> names = new Dictionary<string,VariableDefinition>() 
		]
@init {$XPTreeNode = new OnTrack.Rulez.eXPressionTree.SelectionStatementBlock(); 
	   RegisterMessages($XPTreeNode);}
@after {  DeRegisterMessages($XPTreeNode); }

	: L_BRACKET selectStatement (EOS+ selectStatement)* R_BRACKET
	
	;

/* selectStatement in a Select Block
 */
selectStatement
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
	: selection 
	| variableDeclaration
	| assignment  
	| match 
	| return 
	;

/* Assignment
 */
assignment
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]

	: canonicalName EQ selectExpression
	
	;

/* Variable Declaration
 */
variableDeclaration
locals [  OnTrack.Rulez.IScope Scope ]
	: identifier AS dataType[null] ( DEFAULT literal )? 
		{AddVariable($ctx.identifier().GetText(), $ctx.dataType().datatype, $ctx.literal(), $ctx);}
	;
	
/* MATCH 
 */
match
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]

	: MATCH (canonicalName) WITH matchcase ( OR matchcase )* 
	
	;

matchcase
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]

	: selectExpression DO selectStatement
	;

/* return
 */

return
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]

	: RETURN selectExpression
	
	;

/* Selection of data objects
 *
 * e.g. deliverables[109] = deliverables[uid=109] -> returns a list with one member which is primary key #1 (UID)
 *		deliverables[(109|110|120)] = deliverables[uid=109 OR uid=110 OR uid = 120] -> returns list with 
 *		deliverables[109, category = "DOC"] = deliverables[UID = 109 AND CATEGORY = "DOC"]
 *		deliverables[(109|110|120), created >= #10.12.2015#] = deliverables[(UID = 109 OR UID = 110 OR UID = 120) AND CREATED >= 10.12.2015]
 *		deliverables[] -> all
 *
 *		deliverables[100].desc
 *		deliverables [created >= #01.01.2015#].[uid,desc,created]

 */

selection
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
locals [ string ClassName ]
	// Note: $ClassName will be used from GetDefaultClassname () as workaround for providing the classname by rule argument
	//
    :   dataObject=objectName {$ClassName = $ctx.dataObject.GetText();}  L_SQUARE_BRACKET (Conditions=selectConditions[1])?  R_SQUARE_BRACKET  resultSelection [$ClassName]
	;

/* data object entry selection of the results
 *
 */
resultSelection [string ClassName]
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
	:
	   DOT identifier // -> one data Object Entry Name
	|  (DOT)? L_SQUARE_BRACKET ( identifier ( AND identifier )* )? R_SQUARE_BRACKET
	| // rule is optional -> means that the full object is the result
	;

/* all selection conditions 
 * e.g. 
 * uid = 100, category = "test" 
 *
 * add position counting for keys
 * to enable things like this
 * 100, "test" -> uid = 100 AND category = "test" (uid, category keys)
 * 100 | 101, "test" -> (uid = 100 OR uid = 101) AND category = "test"
 * 100 | category = "Test",  created >= #10.09.2015#
 * ( 100 | category = "Test" ),  created >= #10.09.2015#
 */
selectConditions[uint keypos] // argument keypos as keyposition
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
    :	L_SQUARE_BRACKET  R_SQUARE_BRACKET // all
	|	( NOT )? selectCondition [$keypos] (logicalOperator_2 { incIncreaseKeyNo($ctx); } ( NOT )? selectCondition [$keypos])* 
	    ;

/* selection condition with position 
 * 
 * argument keypos is the 1 ... based counter for anonymous key naming
 *
 * nested [] -> keyposition will be counted to act like a tuple selection
 *	[[100,2] | [101,2] | created > #20.05.2015#] -> (uid = 100 and ver =2) OR (uid = 101 and ver =2) OR created > #20.05.2015#
 *
 * nested () -> are for setting priorities on logical expressions
 * [ uid=100 OR (created > #20.05.2015# and desc = "test")]
 *
 */
 selectCondition [uint keypos]
 returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
    :	 
	    ( NOT )? L_SQUARE_BRACKET {$keypos= 1;} selectConditions [$keypos]  R_SQUARE_BRACKET
	|   ( NOT )? LPAREN  selectConditions [$keypos]  RPAREN
	// check if entryname is preceded with an object class here else we are not getting into this rule
//	|	( { IsDataObjectEntry(CurrentToken.Text, $ctx) | IsDataObjectClass(CurrentToken.Text, $ctx) }? dataObjectEntry=dataObjectEntryName Operator=compareOperator)? select=selectExpression 
	|   ( dataObjectEntry=canonicalName Operator=compareOperator )? select = selectExpression
    ;

/*
 * Logical Operator
 */
 logicalOperator_1
 returns [ OnTrack.Rulez.eXPressionTree.IOperatorDefinition Operator  ]
    : NOT { $ctx.Operator = Operator.GetOperator(new Token(Token.NOT));}
    ;

 logicalOperator_2
 returns [ OnTrack.Rulez.eXPressionTree.IOperatorDefinition Operator  ]
    : AND { $ctx.Operator = Operator.GetOperator(new Token(Token.ANDALSO));}
    | OR  { $ctx.Operator = Operator.GetOperator(new Token(Token.ORELSE));}
	;



/* Select Expressions
 */

selectExpression  
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
locals [ string defaultClassName ]

    : literal 
    | {IsParameterName(CurrentToken.Text, $ctx)}? parameterName
	| {IsVariableName(CurrentToken.Text, $ctx)}? variableName
    | ( PLUS | MINUS ) selectExpression 
	| logicalOperator_1 selectExpression 
	| LPAREN selectExpression RPAREN
    | selectExpression (arithmeticOperator selectExpression)+
	| selection
	// last resort -> unresolved canonical name
	| canonicalName  { $ctx.XPTreeNode = $ctx.canonicalName().XPTreeNode; } // who knows why we need that
	
    ;


/* Arithmetic Operators
 */
arithmeticOperator
returns [ OnTrack.Rulez.eXPressionTree.IOperatorDefinition Operator  ]
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
returns [ OnTrack.Rulez.eXPressionTree.IOperatorDefinition Operator  ]
    : EQ { $ctx.Operator = Operator.GetOperator(new Token(Token.EQ));}
	| NEQ { $ctx.Operator = Operator.GetOperator(new Token(Token.NEQ));}
	| GT  { $ctx.Operator = Operator.GetOperator(new Token(Token.GT));}
	| GE { $ctx.Operator = Operator.GetOperator(new Token(Token.GE));}
	| LE  { $ctx.Operator = Operator.GetOperator(new Token(Token.LE));}
	| LT  { $ctx.Operator = Operator.GetOperator(new Token(Token.LT));}
    ;


// Object Class
//
objectName
returns [  OnTrack.Rulez.eXPressionTree.INode XPTreeNode , ObjectName Name ]
@after{ $Name = GetCanonicalObjectName($ctx, $ctx.identifier()) ;}
    :   (identifier DOT )* identifier
    ;

// Object Entry Name
//
// ClassName will be handled in BuildXPTNode
dataObjectEntryName 
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode , EntryName Name  ]
@after{ $Name = GetCanonicalEntryName($ctx, $ctx.identifier()) ;}
    : 
	(identifier DOT )* identifier
    ;

// parameter name
//
// IsParameterName(CurrentToken.Text,$ctx)
parameterName
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
    :   identifier 
    ;

// variable name
//
// IsVariableName(CurrentToken.Text,$ctx)
variableName
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
    :    canonicalName 
    ;

/* Literals
 */
literal
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode ]
    : TEXTLITERAL { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal($ctx.GetText(), otDataType.Text); }
	| LANGUAGETEXTLITERAL { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal($ctx.GetText(), otDataType.LanguageText); }
	| SYMBOLLITERAL  { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal($ctx.GetText(), otDataType.Symbol); }
    | DECIMALLITERAL  { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal($ctx.GetText(), otDataType.Decimal); }
	| DECIMALUNITLITERAL  { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal($ctx.GetText(), otDataType.DecimalUnit); }
    | DATELITERAL  { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal($ctx.GetText(), otDataType.Date); }
    | NUMBERLITERAL  { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal($ctx.GetText(), otDataType.Number); }
    | NOTHING  { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal(null, otDataType.Null); }
    | FALSE  { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal(false, otDataType.Bool); }
    | TRUE  { $ctx.XPTreeNode = new OnTrack.Rulez.eXPressionTree.Literal(true, otDataType.Bool); }
    ;

/*
 * define a canonicalName which might be a parametername, variablename, dataobjectentry name
 *
 */
canonicalName
returns [ OnTrack.Rulez.eXPressionTree.INode XPTreeNode , CanonicalName Name  ]
@after{ $Name = GetCanonicalEntryName($ctx, $ctx.identifier()) ;}
	:
	 identifier (DOT identifier)*
	;

/*
 * identifier parsing rule
 */
 identifier
    :
	// keywords
	  TYPE | SELECTION | AS | DEFAULT | NULLABLE | OF | MATCH | WITH | RETURN | DO  | MODULE
	// types
	| NUMBER | DECIMAL | TEXT | MEMO | TIMESTAMP | LIST | DATE | DECIMALUNIT | LANGUAGETEXT | SYMBOL
	// named literals
	| TRUE | FALSE | NULL
	// logical operators 
	| AND | OR | NOT | XOR
	// identifier
	| IDENTIFIER // finally
	;


