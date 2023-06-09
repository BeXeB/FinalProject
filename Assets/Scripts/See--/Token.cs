public struct Token
{
    public TokenType type;
    public int line;
    public int startIndex;
    public object literal;
    public string textValue;
    public SeeMMType seeMMType;
}
public enum TokenType
{
    // Single-character tokens.
    LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE, LEFT_SQUAREBRACKET, RIGHT_SQUAREBRACKET,
    COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR, MOD,

    // One or two character tokens. 
    NOT, NOT_EQUAL,
    EQUAL, EQUAL_EQUAL,
    GREATER, GREATER_EQUAL,
    LESS, LESS_EQUAL,
    AND, OR,

    // Literals.
    IDENTIFIER, NUMBER, 

    // Keywords.
    ELSE, FALSE, IF, BREAK, CONTINUE,
    RETURN, TRUE, INT, FLOAT, BOOL, VOID, WHILE,

    EOF
}

public enum SeeMMType
{
    INT, FLOAT, BOOL, VOID,
    FLOAT_ARRAY, INT_ARRAY, BOOL_ARRAY,
    
    ANY, NONE
}

