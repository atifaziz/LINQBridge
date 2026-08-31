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

namespace System.Dynamic
{
    /// <summary>Represents the dynamic get member operation at the call site, providing the binding semantic and the details about the operation.</summary>
    public abstract class GetMemberBinder : DynamicMetaObjectBinder
    {
        private readonly string _name;
        private readonly bool _ignoreCase;

        /// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.GetMemberBinder" /> class.</summary>
        protected GetMemberBinder(string name, bool ignoreCase)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            _name = name;
            _ignoreCase = ignoreCase;
        }

        /// <summary>Gets the name of the member to obtain.</summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>Gets the value indicating if the string comparison should ignore the case of the member name.</summary>
        public bool IgnoreCase
        {
            get { return _ignoreCase; }
        }

        /// <summary>Performs the binding of the dynamic get member operation.</summary>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (target == null) throw new ArgumentNullException("target");
            return target.BindGetMember(this);
        }

        /// <summary>Performs the binding of the dynamic get member operation if the target dynamic object cannot bind.</summary>
        public DynamicMetaObject FallbackGetMember(DynamicMetaObject target)
        {
            return FallbackGetMember(target, null);
        }

        /// <summary>When overridden in the derived class, performs the binding of the dynamic get member operation if the target dynamic object cannot bind.</summary>
        public abstract DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion);
    }

    /// <summary>Represents the dynamic set member operation at the call site, providing the binding semantic and the details about the operation.</summary>
    public abstract class SetMemberBinder : DynamicMetaObjectBinder
    {
        private readonly string _name;
        private readonly bool _ignoreCase;

        /// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.SetMemberBinder" /> class.</summary>
        protected SetMemberBinder(string name, bool ignoreCase)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            _name = name;
            _ignoreCase = ignoreCase;
        }

        /// <summary>Gets the name of the member to obtain.</summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>Gets the value indicating if the string comparison should ignore the case of the member name.</summary>
        public bool IgnoreCase
        {
            get { return _ignoreCase; }
        }

        /// <summary>Performs the binding of the dynamic set member operation.</summary>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (args == null || args.Length != 1) throw new ArgumentException("SetMember takes exactly one value", "args");
            return target.BindSetMember(this, args[0]);
        }

        /// <summary>Performs the binding of the dynamic set member operation if the target dynamic object cannot bind.</summary>
        public DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value)
        {
            return FallbackSetMember(target, value, null);
        }

        /// <summary>When overridden in the derived class, performs the binding of the dynamic set member operation if the target dynamic object cannot bind.</summary>
        public abstract DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion);
    }

    /// <summary>Represents the dynamic delete member operation at the call site, providing the binding semantic and the details about the operation.</summary>
    public abstract class DeleteMemberBinder : DynamicMetaObjectBinder
    {
        private readonly string _name;
        private readonly bool _ignoreCase;

        /// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.DeleteMemberBinder" /> class.</summary>
        protected DeleteMemberBinder(string name, bool ignoreCase)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            _name = name;
            _ignoreCase = ignoreCase;
        }

        /// <summary>Gets the name of the member to delete.</summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>Gets the value indicating if the string comparison should ignore the case of the member name.</summary>
        public bool IgnoreCase
        {
            get { return _ignoreCase; }
        }

        /// <summary>The result type of the operation.</summary>
        public override Type ReturnType
        {
            get { return typeof(void); }
        }

        /// <summary>Performs the binding of the dynamic delete member operation.</summary>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (target == null) throw new ArgumentNullException("target");
            return target.BindDeleteMember(this);
        }

        /// <summary>Performs the binding of the dynamic delete member operation if the target dynamic object cannot bind.</summary>
        public DynamicMetaObject FallbackDeleteMember(DynamicMetaObject target)
        {
            return FallbackDeleteMember(target, null);
        }

        /// <summary>When overridden in the derived class, performs the binding of the dynamic delete member operation if the target dynamic object cannot bind.</summary>
        public abstract DynamicMetaObject FallbackDeleteMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion);
    }

    /// <summary>Represents the dynamic get index operation at the call site, providing the binding semantic and the details about the operation.</summary>
    public abstract class GetIndexBinder : DynamicMetaObjectBinder
    {
        private readonly CallInfo _callInfo;

        /// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.GetIndexBinder" /> class.</summary>
        protected GetIndexBinder(CallInfo callInfo)
        {
            if (callInfo == null)
                throw new ArgumentNullException("callInfo");
            _callInfo = callInfo;
        }

        /// <summary>Gets the signature of the arguments at the call site.</summary>
        public CallInfo CallInfo
        {
            get { return _callInfo; }
        }

        /// <summary>Performs the binding of the dynamic get index operation.</summary>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (args == null || args.Length == 0) throw new ArgumentException("GetIndex needs at least one index", "args");
            return target.BindGetIndex(this, args);
        }

        /// <summary>Performs the binding of the dynamic get index operation if the target dynamic object cannot bind.</summary>
        public DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes)
        {
            return FallbackGetIndex(target, indexes, null);
        }

        /// <summary>When overridden in the derived class, performs the binding of the dynamic get index operation if the target dynamic object cannot bind.</summary>
        public abstract DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion);
    }

    /// <summary>Represents the dynamic set index operation at the call site, providing the binding semantic and the details about the operation.</summary>
    public abstract class SetIndexBinder : DynamicMetaObjectBinder
    {
        private readonly CallInfo _callInfo;

        /// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.SetIndexBinder" /> class.</summary>
        protected SetIndexBinder(CallInfo callInfo)
        {
            if (callInfo == null)
                throw new ArgumentNullException("callInfo");
            _callInfo = callInfo;
        }

        /// <summary>Gets the signature of the arguments at the call site.</summary>
        public CallInfo CallInfo
        {
            get { return _callInfo; }
        }

        /// <summary>Performs the binding of the dynamic set index operation.</summary>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (args == null || args.Length < 2) throw new ArgumentException("SetIndex needs at least one index and a value", "args");

            var indexes = new DynamicMetaObject[args.Length - 1];
            Array.Copy(args, indexes, indexes.Length);
            return target.BindSetIndex(this, indexes, args[args.Length - 1]);
        }

        /// <summary>Performs the binding of the dynamic set index operation if the target dynamic object cannot bind.</summary>
        public DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            return FallbackSetIndex(target, indexes, value, null);
        }

        /// <summary>When overridden in the derived class, performs the binding of the dynamic set index operation if the target dynamic object cannot bind.</summary>
        public abstract DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion);
    }

    /// <summary>Represents the dynamic delete index operation at the call site, providing the binding semantic and the details about the operation.</summary>
    public abstract class DeleteIndexBinder : DynamicMetaObjectBinder
    {
        private readonly CallInfo _callInfo;

        /// <summary>Initializes a new instance of the <see cref="T:System.Dynamic.DeleteIndexBinder" /> class.</summary>
        protected DeleteIndexBinder(CallInfo callInfo)
        {
            if (callInfo == null)
                throw new ArgumentNullException("callInfo");
            _callInfo = callInfo;
        }

        /// <summary>Gets the signature of the arguments at the call site.</summary>
        public CallInfo CallInfo
        {
            get { return _callInfo; }
        }

        /// <summary>The result type of the operation.</summary>
        public override Type ReturnType
        {
            get { return typeof(void); }
        }

        /// <summary>Performs the binding of the dynamic delete index operation.</summary>
        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (args == null || args.Length == 0) throw new ArgumentException("DeleteIndex needs at least one index", "args");
            return target.BindDeleteIndex(this, args);
        }

        /// <summary>Performs the binding of the dynamic delete index operation if the target dynamic object cannot bind.</summary>
        public DynamicMetaObject FallbackDeleteIndex(DynamicMetaObject target, DynamicMetaObject[] indexes)
        {
            return FallbackDeleteIndex(target, indexes, null);
        }

        /// <summary>When overridden in the derived class, performs the binding of the dynamic delete index operation if the target dynamic object cannot bind.</summary>
        public abstract DynamicMetaObject FallbackDeleteIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion);
    }
}
