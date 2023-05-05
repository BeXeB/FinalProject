using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Expression
{
    public interface Visitor<T>
    {
        public T VisitBinaryExpression(Binary expression);
    }

    public abstract T Accept<T>(Visitor<T> visitor);

    public class Binary : Expression
    {
        public Binary(Expression left, Token op, Expression right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }
        public override T Accept<T>(Visitor<T> visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }
    
        readonly Expression left;
        readonly Token op;
        readonly Expression right;
    }
}
