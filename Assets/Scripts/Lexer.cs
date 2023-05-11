using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lexer
{
    int line = 1;
    int startIndex = 0;
    int currentIndex = 0;
    string code;

    List<Token> tokens = new List<Token>();

    static Dictionary<string, TokenType> keyWords = new Dictionary<string, TokenType>()
    {
        {"else", TokenType.ELSE},
        {"false", TokenType.FALSE},
        {"function", TokenType.FUNC},
        {"if", TokenType.IF},
        {"break", TokenType.BREAK},
        {"continue", TokenType.CONTINUE},
        {"return", TokenType.RETURN},
        {"true", TokenType.TRUE},
        {"int", TokenType.INT},
        {"float", TokenType.FLOAT},
        {"bool", TokenType.BOOL},
        {"while", TokenType.WHILE}
    };

    public List<Token> ScanCode(string rawCode)
    {
        code = rawCode.ToLower();
        while (currentIndex < code.Length)
        {
            startIndex = currentIndex;
            ScanToken();
        }
        AddToken(TokenType.EOF);
        return tokens;
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            case '(':
                AddToken(TokenType.LEFT_PAREN);
                break;
            case ')':
                AddToken(TokenType.RIGHT_PAREN);
                break;
            case '{':
                AddToken(TokenType.LEFT_BRACE);
                break;
            case '}':
                AddToken(TokenType.RIGHT_BRACE);
                break;
            case '[':
                AddToken(TokenType.LEFT_SQUAREBRACKET);
                break;
            case ']':
                AddToken(TokenType.RIGHT_SQUAREBRACKET);
                break;
            case ',':
                AddToken(TokenType.COMMA);
                break;
            case '.':
                AddToken(TokenType.DOT);
                break;
            case '-':
                AddToken(TokenType.MINUS);
                break;
            case '+':
                AddToken(TokenType.PLUS);
                break;
            case ';':
                AddToken(TokenType.SEMICOLON);
                break;
            case '/':
                AddToken(TokenType.SLASH);
                break;
            case '*':
                AddToken(TokenType.STAR);
                break;
            case '%':
                AddToken(TokenType.MOD);
                break;
            case '!':
                AddToken(Match('=') ? TokenType.NOT_EQUAL : TokenType.NOT);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                break;
            case '&':
                if (Match('&'))
                {
                    AddToken(TokenType.AND);
                }
                else
                {
                    Debug.Log("Unexpected character.");
                }
                break;
            case '|':
                if (Match('|'))
                {
                    AddToken(TokenType.OR);
                }
                else
                {
                    Debug.Log("Unexpected character.");
                }
                break;
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                line++;
                break;
            default:
                if (char.IsDigit(c))
                {
                    Number();
                }
                else if (char.IsLetter(c))
                {
                    Identifier();
                }
                else
                {
                    Debug.Log("Unexpected character.");
                }
                break;
        }
    }

    private char Advance()
    {
        currentIndex++;
        return code[currentIndex - 1];
    }

    private bool Match(char expected)
    {
        if (currentIndex >= code.Length)
        {
            return false;
        }
        if (code[currentIndex] != expected)
        {
            return false;
        }
        currentIndex++;
        return true;
    }

    private char Peek()
    {
        if (currentIndex >= code.Length)
        {
            return '\0';
        }
        return code[currentIndex];
    }

    private char PeekNext()
    {
        if (currentIndex + 1 >= code.Length)
        {
            return '\0';
        }
        return code[currentIndex + 1];
    }

    private void Number()
    {
        while (char.IsDigit(Peek()))
        {
            Advance();
        }
        // Look for a fractional part.
        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            // Consume the "."
            Advance();
            while (char.IsDigit(Peek()))
            {
                Advance();
            }
        }
        AddToken(TokenType.NUMBER);
    }

    private void Identifier()
    {
        while (char.IsLetterOrDigit(Peek()))
        {
            Advance();
        }
        string text = code.Substring(startIndex, currentIndex - startIndex);
        TokenType type;
        bool isKeyword = keyWords.TryGetValue(text, out type);
        if (!isKeyword)
        {
            type = TokenType.IDENTIFIER;
        }
        AddToken(type);
    }

    private void AddToken(TokenType type)
    {
        string text = code.Substring(startIndex, currentIndex - startIndex);
        tokens.Add(new Token { type = type, line = line, startIndex = startIndex, value = text });
    }
}
