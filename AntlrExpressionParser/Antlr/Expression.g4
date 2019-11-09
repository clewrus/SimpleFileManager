grammar Expression;

@parser::members
{
    protected const int EOF = Eof;
}
 
@lexer::members
{
    protected const int EOF = Eof;
    protected const int HIDDEN = Hidden;
}
 
/*
 * Parser Rules
 */
 
prog: expr+ ;
 
expr : left = expr op=('*'|'/') right = expr   # MulDiv
     | left = expr op=('+'|'-') right = expr   # AddSub
     | INT                  # int
     | '(' expr ')'         # parens
     ;
 
/*
 * Lexer Rules
 */
INT : [0-9]+;
MUL : '*';
DIV : '/';
ADD : '+';
SUB : '-';
EQU : '=';
WS
    :   (' ' | '\r' | '\n') -> channel(HIDDEN)
    ;