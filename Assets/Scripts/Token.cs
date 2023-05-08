public struct Token
{
    public TokenType type;
    public int line;
    public int startIndex;
    public string value;
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
    ELSE, FALSE, FUNC, IF, BREAK, CONTINUE,
    RETURN, TRUE, INT, FLOAT, BOOL, WHILE,

    EOF
}

