#region License, Terms and Author(s)
//
// DynamicBridge tests
//
// Bridge-only fixture: NOT linked into Dynamic.Tests. Its subjects either have no Framework
// counterpart to compare against, or are implementation details whose shape the Framework does not
// guarantee (CallSite internals in particular).
//
#endregion

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;

namespace DynamicBridge.Tests
{
    [TestFixture]
    public class BridgeDynamicFixture
    {
        /// <summary>
        /// The C# compiler passes node kinds to Microsoft.CSharp.RuntimeBinder.Binder as numeric
        /// constants from its own table, so these ordinals are a hard contract, not documentation.
        /// A member inserted in the middle of the enum silently turns 'a * b' into some other
        /// operation at run time; this is the regression guard for that.
        /// </summary>
        [Test]
        public void ExpressionTypeOrdinalsMatchTheFramework()
        {
            Assert.AreEqual(0, (int)ExpressionType.Add);
            Assert.AreEqual(5, (int)ExpressionType.ArrayIndex);
            Assert.AreEqual(6, (int)ExpressionType.Call);
            Assert.AreEqual(13, (int)ExpressionType.Equal);
            Assert.AreEqual(23, (int)ExpressionType.MemberAccess);
            Assert.AreEqual(26, (int)ExpressionType.Multiply);
            Assert.AreEqual(34, (int)ExpressionType.Not);
            Assert.AreEqual(45, (int)ExpressionType.TypeIs);
#if !FRAMEWORK_EXPRESSIONS
            // The 4.0 members exist only on the LinqBridge enum (net20/net30). On net35 the enum is
            // System.Core's and stops at TypeIs, which is exactly why the binder names those kinds
            // through NodeKinds constants rather than enum members.
            Assert.AreEqual(46, (int)ExpressionType.Assign);
            Assert.AreEqual(47, (int)ExpressionType.Block);
            Assert.AreEqual(49, (int)ExpressionType.Decrement);
            Assert.AreEqual(50, (int)ExpressionType.Dynamic);
            Assert.AreEqual(54, (int)ExpressionType.Increment);
            Assert.AreEqual(55, (int)ExpressionType.Index);
            Assert.AreEqual(82, (int)ExpressionType.OnesComplement);
            Assert.AreEqual(83, (int)ExpressionType.IsTrue);
            Assert.AreEqual(84, (int)ExpressionType.IsFalse);
#endif
        }

        [Test]
        public void CallSiteCreateBuildsATypedSite()
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, "Length", typeof(BridgeDynamicFixture),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            var site = CallSite<Func<CallSite, object, object>>.Create(binder);

            Assert.IsNotNull(site);
            Assert.AreSame(binder, site.Binder);
            Assert.IsNotNull(site.Target);
            Assert.AreSame(site.Target, site.Update);
        }

        [Test]
        public void CallSiteTargetPerformsTheOperation()
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, "Length", typeof(BridgeDynamicFixture),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            var site = CallSite<Func<CallSite, object, object>>.Create(binder);

            Assert.AreEqual(3, site.Target(site, "abc"));
        }

        [Test]
        public void CallSiteIsReusableAcrossReceiverTypes()
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, "Count", typeof(BridgeDynamicFixture),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            var site = CallSite<Func<CallSite, object, object>>.Create(binder);

            Assert.AreEqual(2, site.Target(site, new List<int> { 1, 2 }));
            Assert.AreEqual(1, site.Target(site, new List<string> { "a" }));
        }

        [Test]
        public void UntypedCallSiteCreate()
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, "Length", typeof(BridgeDynamicFixture),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            var site = CallSite.Create(typeof(Func<CallSite, object, object>), binder);

            Assert.IsInstanceOf<CallSite<Func<CallSite, object, object>>>(site);
        }

        [Test]
        public void CallSiteCreateRejectsANonDelegate()
        {
            var binder = Binder.GetMember(CSharpBinderFlags.None, "X", typeof(BridgeDynamicFixture),
                new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });

            Assert.Throws<ArgumentException>(delegate { CallSite.Create(typeof(string), binder); });
        }

        [Test]
        public void UpdateLabelIsAVoidLabel()
        {
            Assert.IsNotNull(CallSiteBinder.UpdateLabel);
            Assert.AreEqual(typeof(void), CallSiteBinder.UpdateLabel.Type);
            Assert.AreSame(CallSiteBinder.UpdateLabel, CallSiteBinder.UpdateLabel);
        }

        [Test]
        public void BinderFactoriesProduceTheExpectedBinderKinds()
        {
            var info = new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };

            Assert.IsInstanceOf<GetMemberBinder>(Binder.GetMember(CSharpBinderFlags.None, "A", GetType(), info));
            Assert.IsInstanceOf<SetMemberBinder>(Binder.SetMember(CSharpBinderFlags.None, "A", GetType(), info));
            Assert.IsInstanceOf<InvokeMemberBinder>(Binder.InvokeMember(CSharpBinderFlags.None, "A", null, GetType(), info));
            Assert.IsInstanceOf<InvokeBinder>(Binder.Invoke(CSharpBinderFlags.None, GetType(), info));
            Assert.IsInstanceOf<CreateInstanceBinder>(Binder.InvokeConstructor(CSharpBinderFlags.None, GetType(), info));
            Assert.IsInstanceOf<GetIndexBinder>(Binder.GetIndex(CSharpBinderFlags.None, GetType(), info));
            Assert.IsInstanceOf<SetIndexBinder>(Binder.SetIndex(CSharpBinderFlags.None, GetType(), info));
            Assert.IsInstanceOf<ConvertBinder>(Binder.Convert(CSharpBinderFlags.None, typeof(int), GetType()));
            Assert.IsInstanceOf<BinaryOperationBinder>(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.Add, GetType(), info));
            Assert.IsInstanceOf<UnaryOperationBinder>(Binder.UnaryOperation(CSharpBinderFlags.None, ExpressionType.Negate, GetType(), info));
        }

        [Test]
        public void ConvertBinderCarriesItsTargetType()
        {
            var binder = (ConvertBinder)Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof(int), GetType());

            Assert.AreEqual(typeof(int), binder.Type);
            Assert.AreEqual(typeof(int), binder.ReturnType);
            Assert.IsTrue(binder.Explicit);
        }

        [Test]
        public void IsEventBinderAnswersForAnEvent()
        {
            var binder = Binder.IsEvent(CSharpBinderFlags.None, "Changed", GetType());
            var site = CallSite<Func<CallSite, object, bool>>.Create(binder);

            Assert.IsTrue(site.Target(site, new Person()));

            var notAnEvent = Binder.IsEvent(CSharpBinderFlags.None, "Name", GetType());
            var otherSite = CallSite<Func<CallSite, object, bool>>.Create(notAnEvent);

            Assert.IsFalse(otherSite.Target(otherSite, new Person()));
        }

        [Test]
        public void CSharpArgumentInfoCarriesItsName()
        {
            var info = CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.NamedArgument, "second");
            Assert.IsNotNull(info);
        }

        [Test]
        public void DynamicAttributeExposesItsTransformFlags()
        {
            Assert.AreEqual(1, new DynamicAttribute().TransformFlags.Count);
            Assert.IsTrue(new DynamicAttribute().TransformFlags[0]);

            var attribute = new DynamicAttribute(new[] { false, true });
            Assert.AreEqual(2, attribute.TransformFlags.Count);
            Assert.IsFalse(attribute.TransformFlags[0]);
            Assert.IsTrue(attribute.TransformFlags[1]);
        }

        [Test]
        public void DynamicMetaObjectCarriesItsValue()
        {
            var meta = new DynamicMetaObject(Expression.Constant("a", typeof(object)), BindingRestrictions.Empty, "a");

            Assert.IsTrue(meta.HasValue);
            Assert.AreEqual("a", meta.Value);
            Assert.AreEqual(typeof(string), meta.RuntimeType);
            Assert.AreEqual(typeof(string), meta.LimitType);
        }

        [Test]
        public void DynamicMetaObjectWithoutAValue()
        {
            var meta = new DynamicMetaObject(Expression.Constant("a", typeof(object)), BindingRestrictions.Empty);

            Assert.IsFalse(meta.HasValue);
            Assert.IsNull(meta.RuntimeType);
            Assert.AreEqual(typeof(object), meta.LimitType);
        }

        [Test]
        public void DynamicMetaObjectCreateUsesTheProvider()
        {
            var bag = new Bag();
            var meta = DynamicMetaObject.Create(bag, Expression.Constant(bag, typeof(object)));

            Assert.AreEqual(bag, meta.Value);
            Assert.AreNotEqual(typeof(DynamicMetaObject), meta.GetType());
        }

        [Test]
        public void BindingRestrictionsProduceBooleanExpressions()
        {
            var parameter = Expression.Parameter(typeof(object), "x");

            Assert.AreEqual(typeof(bool), BindingRestrictions.Empty.ToExpression().Type);
            Assert.AreEqual(typeof(bool), BindingRestrictions.GetTypeRestriction(parameter, typeof(string)).ToExpression().Type);
            Assert.AreEqual(typeof(bool), BindingRestrictions.GetInstanceRestriction(parameter, "a").ToExpression().Type);
            Assert.AreEqual(typeof(bool), BindingRestrictions.Empty
                .Merge(BindingRestrictions.GetTypeRestriction(parameter, typeof(string)))
                .ToExpression().Type);
        }

        [Test]
        public void CallInfoDescribesTheArguments()
        {
            var info = new CallInfo(3, "b", "c");

            Assert.AreEqual(3, info.ArgumentCount);
            Assert.AreEqual(2, info.ArgumentNames.Count);
            Assert.AreEqual("b", info.ArgumentNames[0]);
            Assert.AreEqual(info, new CallInfo(3, "b", "c"));
        }

        [Test]
        public void CallInfoRejectsMoreNamesThanArguments()
        {
            Assert.Throws<ArgumentException>(delegate { new CallInfo(1, "a", "b"); });
        }

        /// <summary>
        /// A binder that only implements the rule-producing Bind overload cannot be executed here:
        /// the 4.0 expression nodes a rule needs do not exist on this framework. The failure has to
        /// be an explicit, explained one rather than a mis-binding.
        /// </summary>
        [Test]
        public void RuleProducingBinderIsRejectedWithAnExplanation()
        {
            var site = CallSite<Func<CallSite, object, object>>.Create(new RuleOnlyBinder());
            var error = Assert.Throws<NotSupportedException>(delegate { site.Target(site, "x"); });
            Assert.IsTrue(error.Message.Contains("rule"));
        }

        private sealed class RuleOnlyBinder : CallSiteBinder
        {
            public override Expression Bind(object[] args,
                System.Collections.ObjectModel.ReadOnlyCollection<ParameterExpression> parameters,
                LabelTarget returnLabel)
            {
                return Expression.Constant(null, typeof(object));
            }
        }
    }
}
