using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Expression
{
    public interface Visitor<T>
    {
        T VisitBinaryExpression(Binary expression);
    }

    public class Binary : Expression
    {
        Binary(Expression left, Token op, Expression right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }
        T Accept(Visitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }

        readonly Expression left;
        readonly Token op;
        readonly Expression right;
    }
}
