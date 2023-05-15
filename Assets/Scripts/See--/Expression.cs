using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Expression
{
    public interface IExpressionVisitor<T>
    {
        public T VisitBinaryExpression(BinaryExpression expression);
        public T VisitCallExpression(CallExpression expression);
        public T VisitAssignmentExpression(AssignmentExpression expression);
        public T VisitLiteralExpression(LiteralExpression expression);
        public T VisitGroupingExpression(GroupingExpression expression);
        public T VisitLogicalExpression(LogicalExpression expression);
        public T VisitVariableExpression(VariableExpression expression);
        public T VisitUnaryExpression(UnaryExpression expression);
    }

    public abstract T Accept<T>(IExpressionVisitor<T> visitor);

    public class BinaryExpression : Expression
    {
        public BinaryExpression(Expression left, Token op, Expression right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }
    
        public readonly Expression left;
        public readonly Token op;
        public readonly Expression right;
    }
    public class CallExpression : Expression
    {
        public CallExpression(Expression callee, Token paren, List<Expression> arguments)
        {
            this.callee = callee;
            this.paren = paren;
            this.arguments = arguments;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitCallExpression(this);
        }

        public readonly Expression callee;
        public readonly Token paren;
        public readonly List<Expression> arguments;
    }
    public class AssignmentExpression : Expression
    {
        public AssignmentExpression(Token name, Expression value)
        {
            this.name = name;
            this.value = value;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitAssignmentExpression(this);
        }

        public readonly Token name;
        public readonly Expression value;
    }
    public class LiteralExpression : Expression
    {
        public LiteralExpression(object value)
        {
            this.value = value;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpression(this);
        }

        public readonly object value;
    }
    public class GroupingExpression : Expression
    {
        public GroupingExpression(Expression expression)
        {
            this.expression = expression;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpression(this);
        }

        public readonly Expression expression;
    }
    public class LogicalExpression : Expression
    {
        public LogicalExpression(Expression left, Token op, Expression right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitLogicalExpression(this);
        }

        public readonly Expression left;
        public readonly Token op;
        public readonly Expression right;
    }
    public class VariableExpression : Expression
    {
        public VariableExpression(Token name)
        {
            this.name = name;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitVariableExpression(this);
        }

        public readonly Token name;
    }
    public class UnaryExpression : Expression
    {
        public UnaryExpression(Token op, Expression expression)
        {
            this.op = op;
            this.expression = expression;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }

        public readonly Token op;
        public readonly Expression expression;
    }
}
