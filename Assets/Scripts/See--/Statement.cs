using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Statement
{
    public interface IStatementVisitor<T>
    {
        T VisitBlockStatement(BlockStatement statement);
        T VisitExpressionStatement(ExpressionStatement statement);
        T VisitFunctionStatement(FunctionStatement statement);
        T VisitIfStatement(IfStatement statement);
        T VisitReturnStatement(ReturnStatement statement);
        T VisitBreakStatement(BreakStatement statement);
        T VisitContinueStatement(ContinueStatement statement);
        T VisitIntStatement(IntStatement statement);
        T VisitFloatStatement(FloatStatement statement);
        T VisitBoolStatement(BoolStatement statement);
        T VisitArrayStatement(ArrayStatement statement);
        T VisitWhileStatement(WhileStatement statement);
    }

    public abstract T Accept<T>(IStatementVisitor<T> visitor);

    public class BlockStatement : Statement
    {
        public BlockStatement(List<Statement> statements) 
        {
            this.statements = statements;
        }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitBlockStatement(this);
        }

        public readonly List<Statement> statements;
    }

    public class ExpressionStatement : Statement
    {
        public ExpressionStatement(Expression expression)
        {
            this.expression = expression;
        }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }

        public readonly Expression expression;
    }

    public class FunctionStatement : Statement
    {
        public FunctionStatement(Token name, List<Token> parameters, List<Statement> body, List<SeeMMType> argumentTypes, SeeMMType returnType)
        {
            this.name = name;
            this.parameters = parameters;
            this.body = body;
            this.argumentTypes = argumentTypes;
            this.returnType = returnType;
        }
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitFunctionStatement(this);
        }

        public readonly SeeMMType returnType;
        public readonly Token name;
        public readonly List<Token> parameters;
        public readonly List<SeeMMType> argumentTypes;
        public readonly List<Statement> body;
    }

    public class IfStatement : Statement
    {
        public IfStatement(Expression condition, Statement thenBranch, Statement elseBranch, Token keyword)
        {
            this.condition = condition;
            this.thenBranch = thenBranch;
            this.elseBranch = elseBranch;
            this.keyword = keyword;
        }
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitIfStatement(this);
        }

        public readonly Expression condition;
        public readonly Statement thenBranch;
        public readonly Statement elseBranch;
        public readonly Token keyword;
    }

    public class ReturnStatement : Statement
    {
        public ReturnStatement(Token keyword, Expression value)
        {
            this.keyword = keyword;
            this.value = value;
        }
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitReturnStatement(this);
        }

        public readonly Token keyword;
        public readonly Expression value;
    }

    public class BreakStatement : Statement
    {
        public BreakStatement(Token keyword)
        {
            this.keyword = keyword;
        }
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitBreakStatement(this);
        }

        public readonly Token keyword;
    }

    public class ContinueStatement : Statement
    {
        public ContinueStatement(Token keyword)
        {
            this.keyword = keyword;
        }
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitContinueStatement(this);
        }

        public readonly Token keyword;
    }

    public class IntStatement : Statement
    {
        public IntStatement(Token name, Expression initializer)
        {
            this.name = name;
            this.initializer = initializer;
        }
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitIntStatement(this);
        }

        public readonly Token name;
        public readonly Expression initializer;
    }

    public class FloatStatement : Statement
    {
        public FloatStatement(Token name, Expression initializer)
        {
            this.name = name;
            this.initializer = initializer;
        }
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitFloatStatement(this);
        }

        public readonly Token name;
        public readonly Expression initializer;
    }

    public class BoolStatement : Statement
    {
        public BoolStatement(Token name, Expression initializer)
        {
            this.name = name;
            this.initializer = initializer;
        }
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitBoolStatement(this);
        }

        public readonly Token name;
        public readonly Expression initializer;
    }
    
    public class ArrayStatement : Statement
    {
        public ArrayStatement(Token name, SeeMMType type, Expression[] initializer, int size)
        {
            this.name = name;
            this.type = type;
            this.initializer = initializer;
            this.size = size;
        }
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitArrayStatement(this);
        }

        public readonly Token name;
        public readonly SeeMMType type;
        public readonly Expression[] initializer;
        public readonly int size;
    }

    public class WhileStatement : Statement
    {
        public WhileStatement(Expression condition, Statement body, Token keyword)
        {
            this.condition = condition;
            this.body = body;
            this.keyword = keyword;
        }
        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitWhileStatement(this);
        }

        public readonly Expression condition;
        public readonly Statement body;
        public readonly Token keyword;
    }
}
