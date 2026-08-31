using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace System.Linq.Expressions
{
    [__DynamicallyInvokable]
    [DebuggerTypeProxy(typeof(Expression.IndexExpressionProxy))]
    public sealed class IndexExpression : Expression, IArgumentProvider
    {
        private readonly Expression _instance;

        private readonly PropertyInfo _indexer;

        private IList<Expression> _arguments;

        [__DynamicallyInvokable]
        public ReadOnlyCollection<Expression> Arguments
        {
            [__DynamicallyInvokable]
            get
            {
                return Expression.ReturnReadOnly<Expression>(ref this._arguments);
            }
        }

        [__DynamicallyInvokable]
        public PropertyInfo Indexer
        {
            [__DynamicallyInvokable]
            get
            {
                return this._indexer;
            }
        }

        [__DynamicallyInvokable]
        public sealed override ExpressionType NodeType
        {
            [__DynamicallyInvokable]
            get
            {
                return ExpressionType.Index;
            }
        }

        [__DynamicallyInvokable]
        public Expression Object
        {
            [__DynamicallyInvokable]
            get
            {
                return this._instance;
            }
        }

        [__DynamicallyInvokable]
        int System.Linq.Expressions.IArgumentProvider.ArgumentCount
        {
            [__DynamicallyInvokable]
            get
            {
                return this._arguments.Count;
            }
        }

        [__DynamicallyInvokable]
        public sealed override Type Type
        {
            [__DynamicallyInvokable]
            get
            {
                if (this._indexer != null)
                {
                    return this._indexer.PropertyType;
                }
                return this._instance.Type.GetElementType();
            }
        }

        public bool CanReduce { get; set; }
        public string DebugView { get; set; }

        [__DynamicallyInvokable]
        Expression System.Linq.Expressions.IArgumentProvider.GetArgument(int index)
        {
            return this._arguments[index];
        }

        public IndexExpression(ExpressionType nodeType, Type type) : base(nodeType, type)
        {

        }
    }
}