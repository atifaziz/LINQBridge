#region License, Terms and Author(s)
//
// DynamicBridge
//
// Brings the C# 'dynamic' keyword to CLR 2.0 targets.
//
// This library is free software; you can redistribute it and/or modify it
// under the terms of the New BSD License, a copy of which should have
// been delivered along with this distribution.
//
#endregion

using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Dynamic
{
    /// <summary>Represents a set of binding restrictions on the <see cref="T:System.Dynamic.DynamicMetaObject" /> under which the dynamic binding is valid.</summary>
    /// <remarks>
    /// Restrictions exist so a compiled rule can be reused while it remains applicable. DynamicBridge
    /// binds every operation directly instead of compiling rules, so restrictions are carried for API
    /// compatibility and inspection but do not gate a cache.
    /// </remarks>
    public abstract class BindingRestrictions
    {
        /// <summary>Represents an empty set of binding restrictions.</summary>
        public static readonly BindingRestrictions Empty = new EmptyRestrictions();

        internal BindingRestrictions()
        {
        }

        /// <summary>Merges the set of binding restrictions with the current binding restrictions.</summary>
        public BindingRestrictions Merge(BindingRestrictions restrictions)
        {
            if (restrictions == null)
                throw new ArgumentNullException("restrictions");
            if (this is EmptyRestrictions)
                return restrictions;
            if (restrictions is EmptyRestrictions)
                return this;
            return new MergedRestrictions(this, restrictions);
        }

        /// <summary>Creates the binding restriction that checks the expression for runtime type identity.</summary>
        public static BindingRestrictions GetTypeRestriction(Expression expression, Type type)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (type == null)
                throw new ArgumentNullException("type");
            return new TypeRestriction(expression, type);
        }

        /// <summary>Creates the binding restriction that checks the expression for object instance identity.</summary>
        public static BindingRestrictions GetInstanceRestriction(Expression expression, object instance)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            return new InstanceRestriction(expression, instance);
        }

        /// <summary>Creates the binding restriction that checks the expression for arbitrary immutable properties.</summary>
        public static BindingRestrictions GetExpressionRestriction(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (expression.Type != typeof(bool))
                throw new ArgumentException("Argument must be boolean", "expression");
            return new CustomRestriction(expression);
        }

        /// <summary>Combines binding restrictions from the list of <see cref="T:System.Dynamic.DynamicMetaObject" /> instances into one set of restrictions.</summary>
        public static BindingRestrictions Combine(IList<DynamicMetaObject> contributingObjects)
        {
            var result = Empty;
            if (contributingObjects != null)
            {
                foreach (var metaObject in contributingObjects)
                {
                    if (metaObject != null)
                        result = result.Merge(metaObject.Restrictions);
                }
            }
            return result;
        }

        /// <summary>Creates the <see cref="T:System.Linq.Expressions.Expression" /> representing the binding restrictions.</summary>
        public abstract Expression ToExpression();

        private sealed class EmptyRestrictions : BindingRestrictions
        {
            public override Expression ToExpression()
            {
                return Expression.Constant(true);
            }
        }

        private sealed class TypeRestriction : BindingRestrictions
        {
            private readonly Expression _expression;
            private readonly Type _type;

            internal TypeRestriction(Expression expression, Type type)
            {
                _expression = expression;
                _type = type;
            }

            public override Expression ToExpression()
            {
                return Expression.TypeIs(_expression, _type);
            }
        }

        private sealed class InstanceRestriction : BindingRestrictions
        {
            private readonly Expression _expression;
            private readonly object _instance;

            internal InstanceRestriction(Expression expression, object instance)
            {
                _expression = expression;
                _instance = instance;
            }

            public override Expression ToExpression()
            {
                return Expression.Equal(
                    Expression.Convert(_expression, typeof(object)),
                    Expression.Constant(_instance, typeof(object)));
            }
        }

        private sealed class CustomRestriction : BindingRestrictions
        {
            private readonly Expression _expression;

            internal CustomRestriction(Expression expression)
            {
                _expression = expression;
            }

            public override Expression ToExpression()
            {
                return _expression;
            }
        }

        private sealed class MergedRestrictions : BindingRestrictions
        {
            private readonly BindingRestrictions _left;
            private readonly BindingRestrictions _right;

            internal MergedRestrictions(BindingRestrictions left, BindingRestrictions right)
            {
                _left = left;
                _right = right;
            }

            public override Expression ToExpression()
            {
                return Expression.AndAlso(_left.ToExpression(), _right.ToExpression());
            }
        }
    }
}
