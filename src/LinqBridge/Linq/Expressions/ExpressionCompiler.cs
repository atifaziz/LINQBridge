using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace System.Linq.Expressions
{
    internal class ExpressionCompiler
    {
        private List<ExpressionCompiler.LambdaInfo> lambdas;
        private List<object> globals;
        private ExpressionCompiler.CompileScope scope;

        internal ExpressionCompiler()
        {
            this.lambdas = new List<ExpressionCompiler.LambdaInfo>();
            this.globals = new List<object>();
        }

        public D Compile<D>(Expression<D> lambda) where D:Delegate
        {
            if (!typeof(Delegate).IsAssignableFrom(typeof(D)))
                throw Error.TypeParameterIsNotDelegate((object)typeof(D));
            return (D)this.Compile((LambdaExpression)lambda);
        }

        public Delegate Compile(LambdaExpression lambda) => this.CompileDynamicLambda(lambda);

        private Delegate CompileDynamicLambda(LambdaExpression lambda)
        {
            this.lambdas = new List<ExpressionCompiler.LambdaInfo>();
            this.globals = new List<object>();
            ExpressionCompiler.LambdaInfo lambda1 = this.lambdas[this.GenerateLambda(lambda)];
            ExecutionScope target = new ExecutionScope((ExecutionScope)null, lambda1, this.globals.ToArray(), (object[])null);
            return ((DynamicMethod)lambda1.Method).CreateDelegate(lambda.Type, (object)target);
        }

        private static void GenerateLoadExecutionScope(ILGenerator gen) => gen.Emit(OpCodes.Ldarg_0);

        private void GenerateLoadHoistedLocals(ILGenerator gen) => gen.Emit(OpCodes.Ldloc, this.scope.HoistedLocalsVar);

        private int GenerateLambda(LambdaExpression lambda)
        {
            this.scope = new ExpressionCompiler.CompileScope(this.scope, lambda);
            MethodInfo method1 = lambda.Type.GetMethod("Invoke");
            new ExpressionCompiler.Hoister().Hoist(this.scope);
            DynamicMethod dynamicMethod = new DynamicMethod("lambda_method", method1.ReturnType, this.GetParameterTypes(method1), true);
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            MethodInfo method2 = (MethodInfo)dynamicMethod;
            this.GenerateInitHoistedLocals(ilGenerator);
            int num = (int)this.Generate(ilGenerator, lambda.Body, ExpressionCompiler.StackType.Value);
            if (method1.ReturnType == typeof(void) && lambda.Body.Type != typeof(void))
                ilGenerator.Emit(OpCodes.Pop);
            ilGenerator.Emit(OpCodes.Ret);
            int count = this.lambdas.Count;
            this.lambdas.Add(new ExpressionCompiler.LambdaInfo(lambda, method2, this.scope.HoistedLocals, this.lambdas));
            this.scope = this.scope.Parent;
            return count;
        }

        private void GenerateInitHoistedLocals(ILGenerator gen)
        {
            if (this.scope.HoistedLocals.Count == 0)
                return;
            this.scope.HoistedLocalsVar = gen.DeclareLocal(typeof(object[]));
            ExpressionCompiler.GenerateLoadExecutionScope(gen);
            gen.Emit(OpCodes.Callvirt, typeof(ExecutionScope).GetMethod("CreateHoistedLocals", BindingFlags.Instance | BindingFlags.Public));
            gen.Emit(OpCodes.Stloc, this.scope.HoistedLocalsVar);
            int count = this.scope.Lambda.Parameters.Count;
            for (int index = 0; index < count; ++index)
            {
                ParameterExpression parameter = this.scope.Lambda.Parameters[index];
                if (this.IsHoisted(parameter))
                {
                    this.PrepareInitLocal(gen, parameter);
                    int argAccess = (int)this.GenerateArgAccess(gen, index + 1, ExpressionCompiler.StackType.Value);
                    this.GenerateInitLocal(gen, parameter);
                }
            }
        }

        private bool IsHoisted(ParameterExpression p) => this.scope.HoistedLocals.ContainsKey(p);

        private void PrepareInitLocal(ILGenerator gen, ParameterExpression p)
        {
            int num;
            if (this.scope.HoistedLocals.TryGetValue(p, out num))
            {
                this.GenerateLoadHoistedLocals(gen);
                this.GenerateConstInt(gen, num);
            }
            else
            {
                LocalBuilder localBuilder = gen.DeclareLocal(p.Type);
                this.scope.Locals.Add(p, localBuilder);
            }
        }

        private static Type MakeStrongBoxType(Type type) => typeof(StrongBox<>).MakeGenericType(type);

        private void GenerateInitLocal(ILGenerator gen, ParameterExpression p)
        {
            if (this.scope.HoistedLocals.TryGetValue(p, out int _))
            {
                ConstructorInfo constructor = ExpressionCompiler.MakeStrongBoxType(p.Type).GetConstructor(new Type[1]
                {
          p.Type
                });
                gen.Emit(OpCodes.Newobj, constructor);
                gen.Emit(OpCodes.Stelem_Ref);
            }
            else
            {
                LocalBuilder local;
                if (!this.scope.Locals.TryGetValue(p, out local))
                    throw Error.NotSupported();
                gen.Emit(OpCodes.Stloc, local);
            }
        }

        private Type[] GetParameterTypes(MethodInfo mi)
        {
            ParameterInfo[] parameters = mi.GetParameters();
            Type[] parameterTypes = new Type[parameters.Length + 1];
            int index = 0;
            for (int length = parameters.Length; index < length; ++index)
                parameterTypes[index + 1] = parameters[index].ParameterType;
            parameterTypes[0] = typeof(ExecutionScope);
            return parameterTypes;
        }

        private ExpressionCompiler.StackType Generate(
          ILGenerator gen,
          Expression node,
          ExpressionCompiler.StackType ask)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.ArrayIndex:
                case ExpressionType.Coalesce:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return this.GenerateBinary(gen, (BinaryExpression)node, ask);
                case ExpressionType.ArrayLength:
                case ExpressionType.Negate:
                case ExpressionType.UnaryPlus:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.TypeAs:
                    return this.GenerateUnary(gen, (UnaryExpression)node, ask);
                case ExpressionType.Call:
                    return this.GenerateMethodCall(gen, (MethodCallExpression)node, ask);
                case ExpressionType.Conditional:
                    return this.GenerateConditional(gen, (ConditionalExpression)node);
                case ExpressionType.Constant:
                    return this.GenerateConstant(gen, (ConstantExpression)node, ask);
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    this.GenerateConvert(gen, (UnaryExpression)node);
                    return ExpressionCompiler.StackType.Value;
                case ExpressionType.Invoke:
                    return this.GenerateInvoke(gen, (InvocationExpression)node, ask);
                case ExpressionType.Lambda:
                    this.GenerateCreateDelegate(gen, (LambdaExpression)node);
                    return ExpressionCompiler.StackType.Value;
                case ExpressionType.ListInit:
                    return this.GenerateListInit(gen, (ListInitExpression)node);
                case ExpressionType.MemberAccess:
                    return this.GenerateMemberAccess(gen, (MemberExpression)node, ask);
                case ExpressionType.MemberInit:
                    return this.GenerateMemberInit(gen, (MemberInitExpression)node);
                case ExpressionType.New:
                    return this.GenerateNew(gen, (NewExpression)node, ask);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    this.GenerateNewArray(gen, (NewArrayExpression)node);
                    return ExpressionCompiler.StackType.Value;
                case ExpressionType.Parameter:
                    return this.GenerateParameterAccess(gen, (ParameterExpression)node, ask);
                case ExpressionType.Quote:
                    this.GenerateQuote(gen, (UnaryExpression)node);
                    return ExpressionCompiler.StackType.Value;
                case ExpressionType.TypeIs:
                    this.GenerateTypeIs(gen, (TypeBinaryExpression)node);
                    return ExpressionCompiler.StackType.Value;
                default:
                    throw Error.UnhandledExpressionType((object)node.NodeType);
            }
        }

        private ExpressionCompiler.StackType GenerateNew(
          ILGenerator gen,
          NewExpression nex,
          ExpressionCompiler.StackType ask)
        {
            LocalBuilder local = (LocalBuilder)null;
            if (nex.Type.IsValueType)
                local = gen.DeclareLocal(nex.Type);
            if (nex.Constructor != null)
            {
                ParameterInfo[] parameters = nex.Constructor.GetParameters();
                this.GenerateArgs(gen, parameters, nex.Arguments);
                gen.Emit(OpCodes.Newobj, nex.Constructor);
                if (nex.Type.IsValueType)
                    gen.Emit(OpCodes.Stloc, local);
            }
            else if (nex.Type.IsValueType)
            {
                gen.Emit(OpCodes.Ldloca, local);
                gen.Emit(OpCodes.Initobj, nex.Type);
            }
            else
            {
                ConstructorInfo constructor = nex.Type.GetConstructor(Type.EmptyTypes);
                gen.Emit(OpCodes.Newobj, constructor);
            }
            return nex.Type.IsValueType ? this.ReturnFromLocal(gen, ask, local) : ExpressionCompiler.StackType.Value;
        }

        private ExpressionCompiler.StackType GenerateInvoke(
          ILGenerator gen,
          InvocationExpression invoke,
          ExpressionCompiler.StackType ask)
        {
            LambdaExpression lambdaExpression = invoke.Expression.NodeType == ExpressionType.Quote ? (LambdaExpression)((UnaryExpression)invoke.Expression).Operand : invoke.Expression as LambdaExpression;
            if (lambdaExpression != null)
            {
                int index = 0;
                for (int count = invoke.Arguments.Count; index < count; ++index)
                {
                    ParameterExpression parameter = lambdaExpression.Parameters[index];
                    this.PrepareInitLocal(gen, parameter);
                    int num = (int)this.Generate(gen, invoke.Arguments[index], ExpressionCompiler.StackType.Value);
                    this.GenerateInitLocal(gen, parameter);
                }
                return this.Generate(gen, lambdaExpression.Body, ask);
            }
            Expression instance = invoke.Expression;
            if (typeof(LambdaExpression).IsAssignableFrom(instance.Type))
                instance = (Expression)Expression.Call(instance, instance.Type.GetMethod("Compile", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            Expression node = (Expression)Expression.Call(instance, instance.Type.GetMethod("Invoke"), (IEnumerable<Expression>)invoke.Arguments);
            return this.Generate(gen, node, ask);
        }

        private void GenerateQuote(ILGenerator gen, UnaryExpression quote)
        {
            ExpressionCompiler.GenerateLoadExecutionScope(gen);
            int iGlobal = this.AddGlobal(typeof(Expression), (object)quote.Operand);
            int globalAccess = (int)this.GenerateGlobalAccess(gen, iGlobal, typeof(Expression), ExpressionCompiler.StackType.Value);
            if (this.scope.HoistedLocalsVar != null)
                this.GenerateLoadHoistedLocals(gen);
            else
                gen.Emit(OpCodes.Ldnull);
            MethodInfo method = typeof(ExecutionScope).GetMethod("IsolateExpression", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            gen.Emit(OpCodes.Callvirt, method);
            Type type = quote.Operand.GetType();
            if (type == typeof(Expression))
                return;
            gen.Emit(OpCodes.Castclass, type);
        }

        private void GenerateBinding(ILGenerator gen, MemberBinding binding, Type objectType)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    this.GenerateMemberAssignment(gen, (MemberAssignment)binding, objectType);
                    break;
                case MemberBindingType.MemberBinding:
                    this.GenerateMemberMemberBinding(gen, (MemberMemberBinding)binding);
                    break;
                case MemberBindingType.ListBinding:
                    this.GenerateMemberListBinding(gen, (MemberListBinding)binding);
                    break;
                default:
                    throw Error.UnknownBindingType();
            }
        }

        private void GenerateMemberAssignment(
          ILGenerator gen,
          MemberAssignment binding,
          Type objectType)
        {
            int num = (int)this.Generate(gen, binding.Expression, ExpressionCompiler.StackType.Value);
            if (binding.Member is FieldInfo member1)
            {
                gen.Emit(OpCodes.Stfld, member1);
            }
            else
            {
                PropertyInfo member = binding.Member as PropertyInfo;
                MethodInfo setMethod = member.GetSetMethod(true);
                if (member == null)
                    throw Error.UnhandledBinding();
                if (this.UseVirtual(setMethod))
                {
                    if (objectType.IsValueType)
                        gen.Emit(OpCodes.Constrained, objectType);
                    gen.Emit(OpCodes.Callvirt, setMethod);
                }
                else
                    gen.Emit(OpCodes.Call, setMethod);
            }
        }

        private void GenerateMemberMemberBinding(ILGenerator gen, MemberMemberBinding binding)
        {
            Type memberType = this.GetMemberType(binding.Member);
            if (binding.Member is PropertyInfo && memberType.IsValueType)
                throw Error.CannotAutoInitializeValueTypeMemberThroughProperty((object)binding.Member);
            ExpressionCompiler.StackType ask = memberType.IsValueType ? ExpressionCompiler.StackType.Address : ExpressionCompiler.StackType.Value;
            if (this.GenerateMemberAccess(gen, binding.Member, ask) != ask && memberType.IsValueType)
            {
                LocalBuilder local = gen.DeclareLocal(memberType);
                gen.Emit(OpCodes.Stloc, local);
                gen.Emit(OpCodes.Ldloca, local);
            }
            if (binding.Bindings.Count == 0)
                gen.Emit(OpCodes.Pop);
            else
                this.GenerateMemberInit(gen, binding.Bindings, false, memberType);
        }

        private void GenerateMemberListBinding(ILGenerator gen, MemberListBinding binding)
        {
            Type memberType = this.GetMemberType(binding.Member);
            if (binding.Member is PropertyInfo && memberType.IsValueType)
                throw Error.CannotAutoInitializeValueTypeElementThroughProperty((object)binding.Member);
            ExpressionCompiler.StackType ask = memberType.IsValueType ? ExpressionCompiler.StackType.Address : ExpressionCompiler.StackType.Value;
            if (this.GenerateMemberAccess(gen, binding.Member, ask) != ExpressionCompiler.StackType.Address && memberType.IsValueType)
            {
                LocalBuilder local = gen.DeclareLocal(memberType);
                gen.Emit(OpCodes.Stloc, local);
                gen.Emit(OpCodes.Ldloca, local);
            }
            this.GenerateListInit(gen, binding.Initializers, false, memberType);
        }

        private ExpressionCompiler.StackType GenerateMemberInit(
          ILGenerator gen,
          MemberInitExpression init)
        {
            int num = (int)this.Generate(gen, (Expression)init.NewExpression, ExpressionCompiler.StackType.Value);
            LocalBuilder local = (LocalBuilder)null;
            if (init.NewExpression.Type.IsValueType && init.Bindings.Count > 0)
            {
                local = gen.DeclareLocal(init.NewExpression.Type);
                gen.Emit(OpCodes.Stloc, local);
                gen.Emit(OpCodes.Ldloca, local);
            }
            this.GenerateMemberInit(gen, init.Bindings, local == null, init.NewExpression.Type);
            if (local != null)
                gen.Emit(OpCodes.Ldloc, local);
            return ExpressionCompiler.StackType.Value;
        }

        private void GenerateMemberInit(
          ILGenerator gen,
          ReadOnlyCollection<MemberBinding> bindings,
          bool keepOnStack,
          Type objectType)
        {
            int index = 0;
            for (int count = bindings.Count; index < count; ++index)
            {
                if (keepOnStack || index < count - 1)
                    gen.Emit(OpCodes.Dup);
                this.GenerateBinding(gen, bindings[index], objectType);
            }
        }

        private ExpressionCompiler.StackType GenerateListInit(
          ILGenerator gen,
          ListInitExpression init)
        {
            int num = (int)this.Generate(gen, (Expression)init.NewExpression, ExpressionCompiler.StackType.Value);
            LocalBuilder local = (LocalBuilder)null;
            if (init.NewExpression.Type.IsValueType)
            {
                local = gen.DeclareLocal(init.NewExpression.Type);
                gen.Emit(OpCodes.Stloc, local);
                gen.Emit(OpCodes.Ldloca, local);
            }
            this.GenerateListInit(gen, init.Initializers, local == null, init.NewExpression.Type);
            if (local != null)
                gen.Emit(OpCodes.Ldloc, local);
            return ExpressionCompiler.StackType.Value;
        }

        private void GenerateListInit(
          ILGenerator gen,
          ReadOnlyCollection<ElementInit> initializers,
          bool keepOnStack,
          Type objectType)
        {
            int index = 0;
            for (int count = initializers.Count; index < count; ++index)
            {
                if (keepOnStack || index < count - 1)
                    gen.Emit(OpCodes.Dup);
                this.GenerateMethodCall(gen, initializers[index].AddMethod, initializers[index].Arguments, objectType);
                if (initializers[index].AddMethod.ReturnType != typeof(void))
                    gen.Emit(OpCodes.Pop);
            }
        }

        private Type GetMemberType(MemberInfo member)
        {
            switch (member)
            {
                case FieldInfo fieldInfo:
                    return fieldInfo.FieldType;
                case PropertyInfo propertyInfo:
                    return propertyInfo.PropertyType;
                default:
                    throw Error.MemberNotFieldOrProperty((object)member);
            }
        }

        private void GenerateNewArray(ILGenerator gen, NewArrayExpression nex)
        {
            Type elementType = nex.Type.GetElementType();
            if (nex.NodeType == ExpressionType.NewArrayInit)
            {
                this.GenerateConstInt(gen, nex.Expressions.Count);
                gen.Emit(OpCodes.Newarr, elementType);
                int index = 0;
                for (int count = nex.Expressions.Count; index < count; ++index)
                {
                    gen.Emit(OpCodes.Dup);
                    this.GenerateConstInt(gen, index);
                    int num = (int)this.Generate(gen, nex.Expressions[index], ExpressionCompiler.StackType.Value);
                    this.GenerateArrayAssign(gen, elementType);
                }
            }
            else
            {
                Type[] types = new Type[nex.Expressions.Count];
                int index1 = 0;
                for (int length = types.Length; index1 < length; ++index1)
                    types[index1] = typeof(int);
                int index2 = 0;
                for (int count = nex.Expressions.Count; index2 < count; ++index2)
                {
                    Expression expression = nex.Expressions[index2];
                    int num = (int)this.Generate(gen, expression, ExpressionCompiler.StackType.Value);
                    if (expression.Type != typeof(int))
                        this.GenerateConvertToType(gen, expression.Type, typeof(int), true);
                }
                if (nex.Expressions.Count > 1)
                {
                    int[] numArray = new int[nex.Expressions.Count];
                    ConstructorInfo constructor = Array.CreateInstance(elementType, numArray).GetType().GetConstructor(types);
                    gen.Emit(OpCodes.Newobj, constructor);
                }
                else
                    gen.Emit(OpCodes.Newarr, elementType);
            }
        }

        private void GenerateConvert(ILGenerator gen, UnaryExpression u)
        {
            if (u.Method != null)
            {
                if (u.IsLifted && (!u.Type.IsValueType || !u.Operand.Type.IsValueType))
                {
                    ParameterInfo[] parameters = u.Method.GetParameters();
                    Type parameterType = parameters[0].ParameterType;
                    if (parameterType.IsByRef)
                        parameterType.GetElementType();
                    Expression node = (Expression)Expression.Convert((Expression)Expression.Call((Expression)null, u.Method, (Expression)Expression.Convert(u.Operand, parameters[0].ParameterType)), u.Type);
                    int num = (int)this.Generate(gen, node, ExpressionCompiler.StackType.Value);
                }
                else
                {
                    int unaryMethod = (int)this.GenerateUnaryMethod(gen, u, ExpressionCompiler.StackType.Value);
                }
            }
            else
            {
                int num = (int)this.Generate(gen, u.Operand, ExpressionCompiler.StackType.Value);
                this.GenerateConvertToType(gen, u.Operand.Type, u.Type, u.NodeType == ExpressionType.ConvertChecked);
            }
        }

        private void GenerateCreateDelegate(ILGenerator gen, LambdaExpression lambda)
        {
            int lambda1 = this.GenerateLambda(lambda);
            ExpressionCompiler.GenerateLoadExecutionScope(gen);
            this.GenerateConstInt(gen, lambda1);
            if (this.scope.HoistedLocalsVar != null)
                this.GenerateLoadHoistedLocals(gen);
            else
                gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Callvirt, typeof(ExecutionScope).GetMethod("CreateDelegate", BindingFlags.Instance | BindingFlags.Public));
            gen.Emit(OpCodes.Castclass, lambda.Type);
        }

        private ExpressionCompiler.StackType GenerateMethodCall(
          ILGenerator gen,
          MethodCallExpression mc,
          ExpressionCompiler.StackType ask)
        {
            ExpressionCompiler.StackType methodCall = ExpressionCompiler.StackType.Value;
            MethodInfo method = mc.Method;
            if (!mc.Method.IsStatic)
            {
                ExpressionCompiler.StackType ask1 = mc.Object.Type.IsValueType ? ExpressionCompiler.StackType.Address : ExpressionCompiler.StackType.Value;
                if (this.Generate(gen, mc.Object, ask1) != ask1)
                {
                    LocalBuilder local = gen.DeclareLocal(mc.Object.Type);
                    gen.Emit(OpCodes.Stloc, local);
                    gen.Emit(OpCodes.Ldloca, local);
                }
                if (ask == ExpressionCompiler.StackType.Address && mc.Object.Type.IsArray && method == mc.Object.Type.GetMethod("Get", BindingFlags.Instance | BindingFlags.Public))
                {
                    method = mc.Object.Type.GetMethod("Address", BindingFlags.Instance | BindingFlags.Public);
                    methodCall = ExpressionCompiler.StackType.Address;
                }
            }
            this.GenerateMethodCall(gen, method, mc.Arguments, mc.Object == null ? (Type)null : mc.Object.Type);
            return methodCall;
        }

        private void GenerateMethodCall(
          ILGenerator gen,
          MethodInfo mi,
          ReadOnlyCollection<Expression> args,
          Type objectType)
        {
            ParameterInfo[] parameters = mi.GetParameters();
            List<ExpressionCompiler.WriteBack> args1 = this.GenerateArgs(gen, parameters, args);
            OpCode opcode = this.UseVirtual(mi) ? OpCodes.Callvirt : OpCodes.Call;
            if (opcode == OpCodes.Callvirt && objectType.IsValueType)
                gen.Emit(OpCodes.Constrained, objectType);
            if (mi.CallingConvention == CallingConventions.VarArgs)
            {
                Type[] optionalParameterTypes = new Type[args.Count];
                int index = 0;
                for (int length = optionalParameterTypes.Length; index < length; ++index)
                    optionalParameterTypes[index] = args[index].Type;
                gen.EmitCall(opcode, mi, optionalParameterTypes);
            }
            else
                gen.Emit(opcode, mi);
            foreach (ExpressionCompiler.WriteBack writeback in args1)
                this.GenerateWriteBack(gen, writeback);
        }

        private List<ExpressionCompiler.WriteBack> GenerateArgs(
          ILGenerator gen,
          ParameterInfo[] pis,
          ReadOnlyCollection<Expression> args)
        {
            List<ExpressionCompiler.WriteBack> args1 = new List<ExpressionCompiler.WriteBack>();
            int index = 0;
            for (int length = pis.Length; index < length; ++index)
            {
                ParameterInfo pi = pis[index];
                Expression node = args[index];
                ExpressionCompiler.StackType ask = pi.ParameterType.IsByRef ? ExpressionCompiler.StackType.Address : ExpressionCompiler.StackType.Value;
                ExpressionCompiler.StackType stackType = this.Generate(gen, node, ask);
                if (ask == ExpressionCompiler.StackType.Address && stackType != ExpressionCompiler.StackType.Address)
                {
                    LocalBuilder localBuilder = gen.DeclareLocal(node.Type);
                    gen.Emit(OpCodes.Stloc, localBuilder);
                    gen.Emit(OpCodes.Ldloca, localBuilder);
                    if (args[index] is MemberExpression)
                        args1.Add(new ExpressionCompiler.WriteBack(localBuilder, args[index]));
                }
            }
            return args1;
        }

        private ExpressionCompiler.StackType GenerateLift(
          ILGenerator gen,
          ExpressionType nodeType,
          Type resultType,
          MethodCallExpression mc,
          IEnumerable<ParameterExpression> parameters,
          IEnumerable<Expression> arguments)
        {
            ReadOnlyCollection<ParameterExpression> readOnlyCollection1 = parameters.ToReadOnlyCollection<ParameterExpression>();
            ReadOnlyCollection<Expression> readOnlyCollection2 = arguments.ToReadOnlyCollection<Expression>();
            switch (nodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    if (resultType != Expression.GetNullableType(mc.Type))
                    {
                        Label label1 = gen.DefineLabel();
                        Label label2 = gen.DefineLabel();
                        Label label3 = gen.DefineLabel();
                        LocalBuilder local1 = gen.DeclareLocal(typeof(bool));
                        LocalBuilder local2 = gen.DeclareLocal(typeof(bool));
                        gen.Emit(OpCodes.Ldc_I4_0);
                        gen.Emit(OpCodes.Stloc, local1);
                        gen.Emit(OpCodes.Ldc_I4_1);
                        gen.Emit(OpCodes.Stloc, local2);
                        int index = 0;
                        for (int count = readOnlyCollection1.Count; index < count; ++index)
                        {
                            ParameterExpression p = readOnlyCollection1[index];
                            Expression node = readOnlyCollection2[index];
                            this.PrepareInitLocal(gen, p);
                            if (ExpressionCompiler.IsNullable(node.Type))
                            {
                                if (this.Generate(gen, node, ExpressionCompiler.StackType.Address) == ExpressionCompiler.StackType.Value)
                                {
                                    LocalBuilder local3 = gen.DeclareLocal(node.Type);
                                    gen.Emit(OpCodes.Stloc, local3);
                                    gen.Emit(OpCodes.Ldloca, local3);
                                }
                                gen.Emit(OpCodes.Dup);
                                this.GenerateHasValue(gen, node.Type);
                                gen.Emit(OpCodes.Ldc_I4_0);
                                gen.Emit(OpCodes.Ceq);
                                gen.Emit(OpCodes.Dup);
                                gen.Emit(OpCodes.Ldloc, local1);
                                gen.Emit(OpCodes.Or);
                                gen.Emit(OpCodes.Stloc, local1);
                                gen.Emit(OpCodes.Ldloc, local2);
                                gen.Emit(OpCodes.And);
                                gen.Emit(OpCodes.Stloc, local2);
                                this.GenerateGetValueOrDefault(gen, node.Type);
                            }
                            else
                            {
                                int num = (int)this.Generate(gen, node, ExpressionCompiler.StackType.Value);
                                if (!node.Type.IsValueType)
                                {
                                    gen.Emit(OpCodes.Dup);
                                    gen.Emit(OpCodes.Ldnull);
                                    gen.Emit(OpCodes.Ceq);
                                    gen.Emit(OpCodes.Dup);
                                    gen.Emit(OpCodes.Ldloc, local1);
                                    gen.Emit(OpCodes.Or);
                                    gen.Emit(OpCodes.Stloc, local1);
                                    gen.Emit(OpCodes.Ldloc, local2);
                                    gen.Emit(OpCodes.And);
                                    gen.Emit(OpCodes.Stloc, local2);
                                }
                                else
                                {
                                    gen.Emit(OpCodes.Ldc_I4_0);
                                    gen.Emit(OpCodes.Stloc, local2);
                                }
                            }
                            this.GenerateInitLocal(gen, p);
                        }
                        gen.Emit(OpCodes.Ldloc, local2);
                        gen.Emit(OpCodes.Brtrue, label2);
                        gen.Emit(OpCodes.Ldloc, local1);
                        gen.Emit(OpCodes.Brtrue, label3);
                        int num1 = (int)this.Generate(gen, (Expression)mc, ExpressionCompiler.StackType.Value);
                        if (ExpressionCompiler.IsNullable(resultType) && resultType != mc.Type)
                        {
                            ConstructorInfo constructor = resultType.GetConstructor(new Type[1]
                            {
                mc.Type
                            });
                            gen.Emit(OpCodes.Newobj, constructor);
                        }
                        gen.Emit(OpCodes.Br_S, label1);
                        gen.MarkLabel(label2);
                        bool flag1 = nodeType == ExpressionType.Equal;
                        int constant1 = (int)this.GenerateConstant(gen, Expression.Constant((object)flag1), ExpressionCompiler.StackType.Value);
                        gen.Emit(OpCodes.Br_S, label1);
                        gen.MarkLabel(label3);
                        bool flag2 = nodeType == ExpressionType.NotEqual;
                        int constant2 = (int)this.GenerateConstant(gen, Expression.Constant((object)flag2), ExpressionCompiler.StackType.Value);
                        gen.MarkLabel(label1);
                        return ExpressionCompiler.StackType.Value;
                    }
                    break;
            }
            Label label4 = gen.DefineLabel();
            Label label5 = gen.DefineLabel();
            LocalBuilder local4 = gen.DeclareLocal(typeof(bool));
            int index1 = 0;
            for (int count = readOnlyCollection1.Count; index1 < count; ++index1)
            {
                ParameterExpression p = readOnlyCollection1[index1];
                Expression node = readOnlyCollection2[index1];
                if (ExpressionCompiler.IsNullable(node.Type))
                {
                    this.PrepareInitLocal(gen, p);
                    if (this.Generate(gen, node, ExpressionCompiler.StackType.Address) == ExpressionCompiler.StackType.Value)
                    {
                        LocalBuilder local5 = gen.DeclareLocal(node.Type);
                        gen.Emit(OpCodes.Stloc, local5);
                        gen.Emit(OpCodes.Ldloca, local5);
                    }
                    gen.Emit(OpCodes.Dup);
                    this.GenerateHasValue(gen, node.Type);
                    gen.Emit(OpCodes.Ldc_I4_0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Stloc, local4);
                    this.GenerateGetValueOrDefault(gen, node.Type);
                    this.GenerateInitLocal(gen, p);
                }
                else
                {
                    this.PrepareInitLocal(gen, p);
                    int num = (int)this.Generate(gen, node, ExpressionCompiler.StackType.Value);
                    if (!node.Type.IsValueType)
                    {
                        gen.Emit(OpCodes.Dup);
                        gen.Emit(OpCodes.Ldnull);
                        gen.Emit(OpCodes.Ceq);
                        gen.Emit(OpCodes.Stloc, local4);
                    }
                    this.GenerateInitLocal(gen, p);
                }
                gen.Emit(OpCodes.Ldloc, local4);
                gen.Emit(OpCodes.Brtrue, label5);
            }
            int num2 = (int)this.Generate(gen, (Expression)mc, ExpressionCompiler.StackType.Value);
            if (ExpressionCompiler.IsNullable(resultType) && resultType != mc.Type)
            {
                ConstructorInfo constructor = resultType.GetConstructor(new Type[1]
                {
          mc.Type
                });
                gen.Emit(OpCodes.Newobj, constructor);
            }
            gen.Emit(OpCodes.Br_S, label4);
            gen.MarkLabel(label5);
            if (resultType == Expression.GetNullableType(mc.Type))
            {
                if (resultType.IsValueType)
                {
                    LocalBuilder local6 = gen.DeclareLocal(resultType);
                    gen.Emit(OpCodes.Ldloca, local6);
                    gen.Emit(OpCodes.Initobj, resultType);
                    gen.Emit(OpCodes.Ldloc, local6);
                }
                else
                    gen.Emit(OpCodes.Ldnull);
            }
            else
            {
                switch (nodeType)
                {
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                        gen.Emit(OpCodes.Ldc_I4_0);
                        break;
                }
            }
            gen.MarkLabel(label4);
            return ExpressionCompiler.StackType.Value;
        }

        private ExpressionCompiler.StackType GenerateMemberAccess(
          ILGenerator gen,
          MemberExpression m,
          ExpressionCompiler.StackType ask)
        {
            return this.GenerateMemberAccess(gen, m.Expression, m.Member, ask);
        }

        private ExpressionCompiler.StackType GenerateMemberAccess(
          ILGenerator gen,
          Expression expression,
          MemberInfo member,
          ExpressionCompiler.StackType ask)
        {
            switch (member)
            {
                case FieldInfo fieldInfo:
                    if (!fieldInfo.IsStatic)
                    {
                        ExpressionCompiler.StackType ask1 = expression.Type.IsValueType ? ExpressionCompiler.StackType.Address : ExpressionCompiler.StackType.Value;
                        if (this.Generate(gen, expression, ask1) != ask1)
                        {
                            LocalBuilder local = gen.DeclareLocal(expression.Type);
                            gen.Emit(OpCodes.Stloc, local);
                            gen.Emit(OpCodes.Ldloca, local);
                        }
                    }
                    return this.GenerateMemberAccess(gen, member, ask);
                case PropertyInfo propertyInfo:
                    if (!propertyInfo.GetGetMethod(true).IsStatic)
                    {
                        ExpressionCompiler.StackType ask2 = expression.Type.IsValueType ? ExpressionCompiler.StackType.Address : ExpressionCompiler.StackType.Value;
                        if (this.Generate(gen, expression, ask2) != ask2)
                        {
                            LocalBuilder local = gen.DeclareLocal(expression.Type);
                            gen.Emit(OpCodes.Stloc, local);
                            gen.Emit(OpCodes.Ldloca, local);
                        }
                    }
                    return this.GenerateMemberAccess(gen, member, ask);
                default:
                    throw Error.UnhandledMemberAccess((object)member);
            }
        }

        private void GenerateWriteBack(ILGenerator gen, ExpressionCompiler.WriteBack writeback)
        {
            if (!(writeback.arg is MemberExpression memberExpression))
                return;
            this.GenerateMemberWriteBack(gen, memberExpression.Expression, memberExpression.Member, writeback.loc);
        }

        private void GenerateMemberWriteBack(
          ILGenerator gen,
          Expression expression,
          MemberInfo member,
          LocalBuilder loc)
        {
            switch (member)
            {
                case FieldInfo field:
                    if (!field.IsStatic)
                    {
                        ExpressionCompiler.StackType ask = expression.Type.IsValueType ? ExpressionCompiler.StackType.Address : ExpressionCompiler.StackType.Value;
                        int num = (int)this.Generate(gen, expression, ask);
                        gen.Emit(OpCodes.Ldloc, loc);
                        gen.Emit(OpCodes.Stfld, field);
                        break;
                    }
                    gen.Emit(OpCodes.Ldloc, loc);
                    gen.Emit(OpCodes.Stsfld, field);
                    break;
                case PropertyInfo propertyInfo:
                    MethodInfo setMethod = propertyInfo.GetSetMethod(true);
                    if (setMethod == null)
                        break;
                    if (!setMethod.IsStatic)
                    {
                        ExpressionCompiler.StackType ask = expression.Type.IsValueType ? ExpressionCompiler.StackType.Address : ExpressionCompiler.StackType.Value;
                        int num = (int)this.Generate(gen, expression, ask);
                    }
                    gen.Emit(OpCodes.Ldloc, loc);
                    gen.Emit(this.UseVirtual(setMethod) ? OpCodes.Callvirt : OpCodes.Call, setMethod);
                    break;
                default:
                    throw Error.UnhandledMemberAccess((object)member);
            }
        }

        private bool UseVirtual(MethodInfo mi) => !mi.IsStatic && !mi.DeclaringType.IsValueType;

        private void GenerateFieldAccess(
          ILGenerator gen,
          FieldInfo fi,
          ExpressionCompiler.StackType ask)
        {
            ExpressionCompiler.StackType stackType;
            if (fi.IsLiteral)
            {
                stackType = this.GenerateConstant(gen, fi.FieldType, fi.GetRawConstantValue(), ask);
            }
            else
            {
                OpCode opcode;
                if (ask == ExpressionCompiler.StackType.Value || fi.IsInitOnly)
                {
                    opcode = fi.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld;
                    stackType = ExpressionCompiler.StackType.Value;
                }
                else
                {
                    opcode = fi.IsStatic ? OpCodes.Ldsflda : OpCodes.Ldflda;
                    stackType = ExpressionCompiler.StackType.Address;
                }
                gen.Emit(opcode, fi);
            }
            if (ask != ExpressionCompiler.StackType.Address || stackType != ExpressionCompiler.StackType.Value)
                return;
            LocalBuilder local = gen.DeclareLocal(fi.FieldType);
            gen.Emit(OpCodes.Stloc, local);
            gen.Emit(OpCodes.Ldloca, local);
        }

        private ExpressionCompiler.StackType GenerateMemberAccess(
          ILGenerator gen,
          MemberInfo member,
          ExpressionCompiler.StackType ask)
        {
            switch (member)
            {
                case FieldInfo fi:
                    this.GenerateFieldAccess(gen, fi, ask);
                    return ask;
                case PropertyInfo propertyInfo:
                    MethodInfo getMethod = propertyInfo.GetGetMethod(true);
                    gen.Emit(this.UseVirtual(getMethod) ? OpCodes.Callvirt : OpCodes.Call, getMethod);
                    return ExpressionCompiler.StackType.Value;
                default:
                    throw Error.UnhandledMemberAccess((object)member);
            }
        }

        private ExpressionCompiler.StackType GenerateParameterAccess(
          ILGenerator gen,
          ParameterExpression p,
          ExpressionCompiler.StackType ask)
        {
            LocalBuilder local;
            if (this.scope.Locals.TryGetValue(p, out local))
            {
                if (ask == ExpressionCompiler.StackType.Value)
                    gen.Emit(OpCodes.Ldloc, local);
                else
                    gen.Emit(OpCodes.Ldloca, local);
                return ask;
            }
            int hoistIndex;
            if (this.scope.HoistedLocals.TryGetValue(p, out hoistIndex))
            {
                this.GenerateLoadHoistedLocals(gen);
                return this.GenerateHoistedLocalAccess(gen, hoistIndex, p.Type, ask);
            }
            int index = 0;
            for (int count = this.scope.Lambda.Parameters.Count; index < count; ++index)
            {
                if (this.scope.Lambda.Parameters[index] == p)
                    return this.GenerateArgAccess(gen, index + 1, ask);
            }
            ExpressionCompiler.GenerateLoadExecutionScope(gen);
            for (ExpressionCompiler.CompileScope parent = this.scope.Parent; parent != null; parent = parent.Parent)
            {
                if (parent.HoistedLocals.TryGetValue(p, out hoistIndex))
                {
                    gen.Emit(OpCodes.Ldfld, typeof(ExecutionScope).GetField("Locals", BindingFlags.Instance | BindingFlags.Public));
                    return this.GenerateHoistedLocalAccess(gen, hoistIndex, p.Type, ask);
                }
                gen.Emit(OpCodes.Ldfld, typeof(ExecutionScope).GetField("Parent", BindingFlags.Instance | BindingFlags.Public));
            }
            throw Error.LambdaParameterNotInScope();
        }

        private ExpressionCompiler.StackType GenerateConstant(
          ILGenerator gen,
          ConstantExpression c,
          ExpressionCompiler.StackType ask)
        {
            return this.GenerateConstant(gen, c.Type, c.Value, ask);
        }

        private ExpressionCompiler.StackType GenerateConstant(
          ILGenerator gen,
          Type type,
          object value,
          ExpressionCompiler.StackType ask)
        {
            if (value == null)
            {
                if (type.IsValueType)
                {
                    LocalBuilder local = gen.DeclareLocal(type);
                    gen.Emit(OpCodes.Ldloca, local);
                    gen.Emit(OpCodes.Initobj, type);
                    gen.Emit(OpCodes.Ldloc, local);
                }
                else
                    gen.Emit(OpCodes.Ldnull);
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        this.GenerateConstInt(gen, (bool)value ? 1 : 0);
                        break;
                    case TypeCode.SByte:
                        this.GenerateConstInt(gen, (int)(sbyte)value);
                        gen.Emit(OpCodes.Conv_I1);
                        break;
                    case TypeCode.Int16:
                        this.GenerateConstInt(gen, (int)(short)value);
                        gen.Emit(OpCodes.Conv_I2);
                        break;
                    case TypeCode.Int32:
                        this.GenerateConstInt(gen, (int)value);
                        break;
                    case TypeCode.Int64:
                        gen.Emit(OpCodes.Ldc_I8, (long)value);
                        break;
                    case TypeCode.Single:
                        gen.Emit(OpCodes.Ldc_R4, (float)value);
                        break;
                    case TypeCode.Double:
                        gen.Emit(OpCodes.Ldc_R8, (double)value);
                        break;
                    default:
                        int iGlobal = this.AddGlobal(type, value);
                        return this.GenerateGlobalAccess(gen, iGlobal, type, ask);
                }
            }
            return ExpressionCompiler.StackType.Value;
        }

        private ExpressionCompiler.StackType GenerateUnary(
          ILGenerator gen,
          UnaryExpression u,
          ExpressionCompiler.StackType ask)
        {
            if (u.Method != null)
                return this.GenerateUnaryMethod(gen, u, ask);
            if (u.NodeType == ExpressionType.NegateChecked && ExpressionCompiler.IsInteger(u.Operand.Type))
            {
                this.GenerateConstInt(gen, 0);
                this.GenerateConvertToType(gen, typeof(int), u.Operand.Type, false);
                int num = (int)this.Generate(gen, u.Operand, ExpressionCompiler.StackType.Value);
                return this.GenerateBinaryOp(gen, ExpressionType.SubtractChecked, u.Operand.Type, u.Operand.Type, u.Type, false, ask);
            }
            int num1 = (int)this.Generate(gen, u.Operand, ExpressionCompiler.StackType.Value);
            return this.GenerateUnaryOp(gen, u.NodeType, u.Operand.Type, u.Type, ask);
        }

        private static bool IsInteger(Type type)
        {
            type = ExpressionCompiler.GetNonNullableType(type);
            if (type.IsEnum)
                return false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        private ExpressionCompiler.StackType GenerateUnaryMethod(
          ILGenerator gen,
          UnaryExpression u,
          ExpressionCompiler.StackType ask)
        {
            if (u.IsLifted)
            {
                ParameterExpression parameterExpression = Expression.Parameter(Expression.GetNonNullableType(u.Operand.Type), (string)null);
                MethodCallExpression mc = Expression.Call((Expression)null, u.Method, (Expression)parameterExpression);
                Type nullableType = Expression.GetNullableType(mc.Type);
                int lift = (int)this.GenerateLift(gen, u.NodeType, nullableType, mc, (IEnumerable<ParameterExpression>)new ParameterExpression[1]
                {
          parameterExpression
                }, (IEnumerable<Expression>)new Expression[1]
                {
          u.Operand
                });
                this.GenerateConvertToType(gen, nullableType, u.Type, false);
                return ExpressionCompiler.StackType.Value;
            }
            MethodCallExpression node = Expression.Call((Expression)null, u.Method, u.Operand);
            return this.Generate(gen, (Expression)node, ask);
        }

        private ExpressionCompiler.StackType GenerateConditional(
          ILGenerator gen,
          ConditionalExpression b)
        {
            Label label1 = gen.DefineLabel();
            Label label2 = gen.DefineLabel();
            int num1 = (int)this.Generate(gen, b.Test, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Brfalse, label1);
            int num2 = (int)this.Generate(gen, b.IfTrue, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Br, label2);
            gen.MarkLabel(label1);
            int num3 = (int)this.Generate(gen, b.IfFalse, ExpressionCompiler.StackType.Value);
            gen.MarkLabel(label2);
            return ExpressionCompiler.StackType.Value;
        }

        private void GenerateCoalesce(ILGenerator gen, BinaryExpression b)
        {
            if (ExpressionCompiler.IsNullable(b.Left.Type))
            {
                this.GenerateNullableCoalesce(gen, b);
            }
            else
            {
                if (b.Left.Type.IsValueType)
                    throw Error.CoalesceUsedOnNonNullType();
                if (b.Conversion != null)
                    this.GenerateLambdaReferenceCoalesce(gen, b);
                else if (b.Method != null)
                    this.GenerateUserDefinedReferenceCoalesce(gen, b);
                else
                    this.GenerateReferenceCoalesceWithoutConversion(gen, b);
            }
        }

        private void GenerateNullableCoalesce(ILGenerator gen, BinaryExpression b)
        {
            LocalBuilder local = gen.DeclareLocal(b.Left.Type);
            Label label1 = gen.DefineLabel();
            Label label2 = gen.DefineLabel();
            int num1 = (int)this.Generate(gen, b.Left, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Stloc, local);
            gen.Emit(OpCodes.Ldloca, local);
            this.GenerateHasValue(gen, b.Left.Type);
            gen.Emit(OpCodes.Brfalse, label1);
            Type nonNullableType = ExpressionCompiler.GetNonNullableType(b.Left.Type);
            if (b.Method != null)
            {
                if (!b.Method.GetParameters()[0].ParameterType.IsAssignableFrom(b.Left.Type))
                {
                    gen.Emit(OpCodes.Ldloca, local);
                    this.GenerateGetValueOrDefault(gen, b.Left.Type);
                }
                else
                    gen.Emit(OpCodes.Ldloc, local);
                gen.Emit(OpCodes.Call, b.Method);
            }
            else if (b.Conversion != null)
            {
                ParameterExpression parameter = b.Conversion.Parameters[0];
                this.PrepareInitLocal(gen, parameter);
                if (!parameter.Type.IsAssignableFrom(b.Left.Type))
                {
                    gen.Emit(OpCodes.Ldloca, local);
                    this.GenerateGetValueOrDefault(gen, b.Left.Type);
                }
                else
                    gen.Emit(OpCodes.Ldloc, local);
                this.GenerateInitLocal(gen, parameter);
                int num2 = (int)this.Generate(gen, b.Conversion.Body, ExpressionCompiler.StackType.Value);
            }
            else if (b.Type != nonNullableType)
            {
                gen.Emit(OpCodes.Ldloca, local);
                this.GenerateGetValueOrDefault(gen, b.Left.Type);
                this.GenerateConvertToType(gen, nonNullableType, b.Type, true);
            }
            else
            {
                gen.Emit(OpCodes.Ldloca, local);
                this.GenerateGetValueOrDefault(gen, b.Left.Type);
            }
            gen.Emit(OpCodes.Br, label2);
            gen.MarkLabel(label1);
            int num3 = (int)this.Generate(gen, b.Right, ExpressionCompiler.StackType.Value);
            if (b.Right.Type != b.Type)
                this.GenerateConvertToType(gen, b.Right.Type, b.Type, true);
            gen.MarkLabel(label2);
        }

        private void GenerateLambdaReferenceCoalesce(ILGenerator gen, BinaryExpression b)
        {
            Label label1 = gen.DefineLabel();
            Label label2 = gen.DefineLabel();
            int num1 = (int)this.Generate(gen, b.Left, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brfalse, label2);
            gen.Emit(OpCodes.Pop);
            int num2 = (int)this.Generate(gen, b.Right, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Br, label1);
            gen.MarkLabel(label2);
            ParameterExpression parameter = b.Conversion.Parameters[0];
            this.PrepareInitLocal(gen, parameter);
            this.GenerateInitLocal(gen, parameter);
            int num3 = (int)this.Generate(gen, b.Conversion.Body, ExpressionCompiler.StackType.Value);
            gen.MarkLabel(label1);
        }

        private void GenerateUserDefinedReferenceCoalesce(ILGenerator gen, BinaryExpression b)
        {
            Label label1 = gen.DefineLabel();
            Label label2 = gen.DefineLabel();
            int num1 = (int)this.Generate(gen, b.Left, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brfalse, label2);
            gen.Emit(OpCodes.Pop);
            int num2 = (int)this.Generate(gen, b.Right, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Br_S, label1);
            gen.MarkLabel(label2);
            gen.Emit(OpCodes.Call, b.Method);
            gen.MarkLabel(label1);
        }

        private void GenerateReferenceCoalesceWithoutConversion(ILGenerator gen, BinaryExpression b)
        {
            Label label1 = gen.DefineLabel();
            Label label2 = gen.DefineLabel();
            int num1 = (int)this.Generate(gen, b.Left, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brfalse, label2);
            gen.Emit(OpCodes.Pop);
            int num2 = (int)this.Generate(gen, b.Right, ExpressionCompiler.StackType.Value);
            if (b.Right.Type != b.Type)
                gen.Emit(OpCodes.Castclass, b.Type);
            gen.Emit(OpCodes.Br_S, label1);
            gen.MarkLabel(label2);
            if (b.Left.Type != b.Type)
                gen.Emit(OpCodes.Castclass, b.Type);
            gen.MarkLabel(label1);
        }

        private ExpressionCompiler.StackType GenerateUserdefinedLiftedAndAlso(
          ILGenerator gen,
          BinaryExpression b,
          ExpressionCompiler.StackType ask)
        {
            Type type = b.Left.Type;
            Type nonNullableType = ExpressionCompiler.GetNonNullableType(type);
            gen.DefineLabel();
            Label label1 = gen.DefineLabel();
            Label label2 = gen.DefineLabel();
            LocalBuilder local1 = gen.DeclareLocal(type);
            LocalBuilder local2 = gen.DeclareLocal(type);
            LocalBuilder local3 = gen.DeclareLocal(nonNullableType);
            LocalBuilder local4 = gen.DeclareLocal(nonNullableType);
            int num1 = (int)this.Generate(gen, b.Left, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Stloc, local1);
            int num2 = (int)this.Generate(gen, b.Right, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Stloc, local2);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse, label2);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateGetValueOrDefault(gen, type);
            Type[] types1 = new Type[1] { nonNullableType };
            MethodInfo method1 = nonNullableType.GetMethod("op_False", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, types1, (ParameterModifier[])null);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Brtrue, label2);
            gen.Emit(OpCodes.Ldloca, local2);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse, label1);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateGetValueOrDefault(gen, type);
            gen.Emit(OpCodes.Stloc, local3);
            gen.Emit(OpCodes.Ldloca, local2);
            this.GenerateGetValueOrDefault(gen, type);
            gen.Emit(OpCodes.Stloc, local4);
            Type[] types2 = new Type[2]
            {
        nonNullableType,
        nonNullableType
            };
            MethodInfo method2 = nonNullableType.GetMethod("op_BitwiseAnd", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, types2, (ParameterModifier[])null);
            gen.Emit(OpCodes.Ldloc, local3);
            gen.Emit(OpCodes.Ldloc, local4);
            gen.Emit(OpCodes.Call, method2);
            if (method2.ReturnType != type)
                this.GenerateConvertToType(gen, method2.ReturnType, type, true);
            gen.Emit(OpCodes.Stloc, local1);
            gen.Emit(OpCodes.Br, label2);
            gen.MarkLabel(label1);
            gen.Emit(OpCodes.Ldloc, local2);
            gen.Emit(OpCodes.Stloc, local1);
            gen.MarkLabel(label2);
            return this.ReturnFromLocal(gen, ask, local1);
        }

        private ExpressionCompiler.StackType GenerateLiftedAndAlso(
          ILGenerator gen,
          BinaryExpression b,
          ExpressionCompiler.StackType ask)
        {
            Type type = typeof(bool?);
            Label label1 = gen.DefineLabel();
            Label label2 = gen.DefineLabel();
            Label label3 = gen.DefineLabel();
            Label label4 = gen.DefineLabel();
            Label label5 = gen.DefineLabel();
            LocalBuilder local1 = gen.DeclareLocal(type);
            LocalBuilder local2 = gen.DeclareLocal(type);
            int num1 = (int)this.Generate(gen, b.Left, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Stloc, local1);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse, label1);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateGetValueOrDefault(gen, type);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brtrue, label2);
            gen.MarkLabel(label1);
            int num2 = (int)this.Generate(gen, b.Right, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Stloc, local2);
            gen.Emit(OpCodes.Ldloca, local2);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse_S, label3);
            gen.Emit(OpCodes.Ldloca, local2);
            this.GenerateGetValueOrDefault(gen, type);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brtrue_S, label2);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse, label3);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Br_S, label4);
            gen.MarkLabel(label2);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Br_S, label4);
            gen.MarkLabel(label4);
            ConstructorInfo constructor = type.GetConstructor(new Type[1]
            {
        typeof (bool)
            });
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Stloc, local1);
            gen.Emit(OpCodes.Br, label5);
            gen.MarkLabel(label3);
            gen.Emit(OpCodes.Ldloca, local1);
            gen.Emit(OpCodes.Initobj, type);
            gen.MarkLabel(label5);
            return this.ReturnFromLocal(gen, ask, local1);
        }

        private void GenerateMethodAndAlso(ILGenerator gen, BinaryExpression b)
        {
            Label label = gen.DefineLabel();
            int num1 = (int)this.Generate(gen, b.Left, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Dup);
            Type parameterType = b.Method.GetParameters()[0].ParameterType;
            Type[] types = new Type[1] { parameterType };
            MethodInfo method = parameterType.GetMethod("op_False", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, types, (ParameterModifier[])null);
            gen.Emit(OpCodes.Call, method);
            gen.Emit(OpCodes.Brtrue, label);
            int num2 = (int)this.Generate(gen, b.Right, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Call, b.Method);
            gen.MarkLabel(label);
        }

        private void GenerateUnliftedAndAlso(ILGenerator gen, BinaryExpression b)
        {
            int num1 = (int)this.Generate(gen, b.Left, ExpressionCompiler.StackType.Value);
            Label label = gen.DefineLabel();
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Brfalse, label);
            gen.Emit(OpCodes.Pop);
            int num2 = (int)this.Generate(gen, b.Right, ExpressionCompiler.StackType.Value);
            gen.MarkLabel(label);
        }

        private ExpressionCompiler.StackType GenerateAndAlso(
          ILGenerator gen,
          BinaryExpression b,
          ExpressionCompiler.StackType ask)
        {
            if (b.Method != null && !ExpressionCompiler.IsLiftedLogicalBinaryOperator(b.Left.Type, b.Right.Type, b.Method))
            {
                this.GenerateMethodAndAlso(gen, b);
            }
            else
            {
                if (b.Left.Type == typeof(bool?))
                    return this.GenerateLiftedAndAlso(gen, b, ask);
                if (ExpressionCompiler.IsLiftedLogicalBinaryOperator(b.Left.Type, b.Right.Type, b.Method))
                    return this.GenerateUserdefinedLiftedAndAlso(gen, b, ask);
                this.GenerateUnliftedAndAlso(gen, b);
            }
            return ExpressionCompiler.StackType.Value;
        }

        private static bool IsLiftedLogicalBinaryOperator(Type left, Type right, MethodInfo method) => right == left && ExpressionCompiler.IsNullable(left) && method != null && method.ReturnType == ExpressionCompiler.GetNonNullableType(left);

        private ExpressionCompiler.StackType GenerateUserdefinedLiftedOrElse(
          ILGenerator gen,
          BinaryExpression b,
          ExpressionCompiler.StackType ask)
        {
            Type type = b.Left.Type;
            Type nonNullableType = ExpressionCompiler.GetNonNullableType(type);
            gen.DefineLabel();
            Label label1 = gen.DefineLabel();
            Label label2 = gen.DefineLabel();
            LocalBuilder local1 = gen.DeclareLocal(type);
            LocalBuilder local2 = gen.DeclareLocal(type);
            LocalBuilder local3 = gen.DeclareLocal(nonNullableType);
            LocalBuilder local4 = gen.DeclareLocal(nonNullableType);
            int num1 = (int)this.Generate(gen, b.Left, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Stloc, local1);
            int num2 = (int)this.Generate(gen, b.Right, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Stloc, local2);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse, label1);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateGetValueOrDefault(gen, type);
            Type[] types1 = new Type[1] { nonNullableType };
            MethodInfo method1 = nonNullableType.GetMethod("op_True", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, types1, (ParameterModifier[])null);
            gen.Emit(OpCodes.Call, method1);
            gen.Emit(OpCodes.Brtrue, label2);
            gen.Emit(OpCodes.Ldloca, local2);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse, label1);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateGetValueOrDefault(gen, type);
            gen.Emit(OpCodes.Stloc, local3);
            gen.Emit(OpCodes.Ldloca, local2);
            this.GenerateGetValueOrDefault(gen, type);
            gen.Emit(OpCodes.Stloc, local4);
            Type[] types2 = new Type[2]
            {
        nonNullableType,
        nonNullableType
            };
            MethodInfo method2 = nonNullableType.GetMethod("op_BitwiseOr", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, types2, (ParameterModifier[])null);
            gen.Emit(OpCodes.Ldloc, local3);
            gen.Emit(OpCodes.Ldloc, local4);
            gen.Emit(OpCodes.Call, method2);
            if (method2.ReturnType != type)
                this.GenerateConvertToType(gen, method2.ReturnType, type, true);
            gen.Emit(OpCodes.Stloc, local1);
            gen.Emit(OpCodes.Br, label2);
            gen.MarkLabel(label1);
            gen.Emit(OpCodes.Ldloc, local2);
            gen.Emit(OpCodes.Stloc, local1);
            gen.MarkLabel(label2);
            return this.ReturnFromLocal(gen, ask, local1);
        }

        private ExpressionCompiler.StackType GenerateLiftedOrElse(
          ILGenerator gen,
          BinaryExpression b,
          ExpressionCompiler.StackType ask)
        {
            Type type = typeof(bool?);
            Label label1 = gen.DefineLabel();
            Label label2 = gen.DefineLabel();
            Label label3 = gen.DefineLabel();
            Label label4 = gen.DefineLabel();
            Label label5 = gen.DefineLabel();
            LocalBuilder local1 = gen.DeclareLocal(type);
            LocalBuilder local2 = gen.DeclareLocal(type);
            int num1 = (int)this.Generate(gen, b.Left, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Stloc, local1);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse, label1);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateGetValueOrDefault(gen, type);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brfalse, label2);
            gen.MarkLabel(label1);
            int num2 = (int)this.Generate(gen, b.Right, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Stloc, local2);
            gen.Emit(OpCodes.Ldloca, local2);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse_S, label3);
            gen.Emit(OpCodes.Ldloca, local2);
            this.GenerateGetValueOrDefault(gen, type);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brfalse_S, label2);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse, label3);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Br_S, label4);
            gen.MarkLabel(label2);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Br_S, label4);
            gen.MarkLabel(label4);
            ConstructorInfo constructor = type.GetConstructor(new Type[1]
            {
        typeof (bool)
            });
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Stloc, local1);
            gen.Emit(OpCodes.Br, label5);
            gen.MarkLabel(label3);
            gen.Emit(OpCodes.Ldloca, local1);
            gen.Emit(OpCodes.Initobj, type);
            gen.MarkLabel(label5);
            return this.ReturnFromLocal(gen, ask, local1);
        }

        private void GenerateUnliftedOrElse(ILGenerator gen, BinaryExpression b)
        {
            int num1 = (int)this.Generate(gen, b.Left, ExpressionCompiler.StackType.Value);
            Label label = gen.DefineLabel();
            gen.Emit(OpCodes.Dup);
            gen.Emit(OpCodes.Brtrue, label);
            gen.Emit(OpCodes.Pop);
            int num2 = (int)this.Generate(gen, b.Right, ExpressionCompiler.StackType.Value);
            gen.MarkLabel(label);
        }

        private void GenerateMethodOrElse(ILGenerator gen, BinaryExpression b)
        {
            Label label = gen.DefineLabel();
            int num1 = (int)this.Generate(gen, b.Left, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Dup);
            Type parameterType = b.Method.GetParameters()[0].ParameterType;
            Type[] types = new Type[1] { parameterType };
            MethodInfo method = parameterType.GetMethod("op_True", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, types, (ParameterModifier[])null);
            gen.Emit(OpCodes.Call, method);
            gen.Emit(OpCodes.Brtrue, label);
            int num2 = (int)this.Generate(gen, b.Right, ExpressionCompiler.StackType.Value);
            gen.Emit(OpCodes.Call, b.Method);
            gen.MarkLabel(label);
        }

        private ExpressionCompiler.StackType GenerateOrElse(
          ILGenerator gen,
          BinaryExpression b,
          ExpressionCompiler.StackType ask)
        {
            if (b.Method != null && !ExpressionCompiler.IsLiftedLogicalBinaryOperator(b.Left.Type, b.Right.Type, b.Method))
            {
                this.GenerateMethodOrElse(gen, b);
            }
            else
            {
                if (b.Left.Type == typeof(bool?))
                    return this.GenerateLiftedOrElse(gen, b, ask);
                if (ExpressionCompiler.IsLiftedLogicalBinaryOperator(b.Left.Type, b.Right.Type, b.Method))
                    return this.GenerateUserdefinedLiftedOrElse(gen, b, ask);
                this.GenerateUnliftedOrElse(gen, b);
            }
            return ExpressionCompiler.StackType.Value;
        }

        private static bool IsNullConstant(Expression e) => e.NodeType == ExpressionType.Constant && ((ConstantExpression)e).Value == null;

        private ExpressionCompiler.StackType GenerateBinary(
          ILGenerator gen,
          BinaryExpression b,
          ExpressionCompiler.StackType ask)
        {
            switch (b.NodeType)
            {
                case ExpressionType.AndAlso:
                    return this.GenerateAndAlso(gen, b, ask);
                case ExpressionType.Coalesce:
                    this.GenerateCoalesce(gen, b);
                    return ExpressionCompiler.StackType.Value;
                case ExpressionType.OrElse:
                    return this.GenerateOrElse(gen, b, ask);
                default:
                    if (b.Method != null)
                        return this.GenerateBinaryMethod(gen, b, ask);
                    if ((b.NodeType == ExpressionType.Equal || b.NodeType == ExpressionType.NotEqual) && (b.Type == typeof(bool) || b.Type == typeof(bool?)))
                    {
                        if (ExpressionCompiler.IsNullConstant(b.Left) && !ExpressionCompiler.IsNullConstant(b.Right) && ExpressionCompiler.IsNullable(b.Right.Type))
                            return this.GenerateNullEquality(gen, b.NodeType, b.Right, b.IsLiftedToNull);
                        if (ExpressionCompiler.IsNullConstant(b.Right) && !ExpressionCompiler.IsNullConstant(b.Left) && ExpressionCompiler.IsNullable(b.Left.Type))
                            return this.GenerateNullEquality(gen, b.NodeType, b.Left, b.IsLiftedToNull);
                    }
                    int num1 = (int)this.Generate(gen, b.Left, ExpressionCompiler.StackType.Value);
                    int num2 = (int)this.Generate(gen, b.Right, ExpressionCompiler.StackType.Value);
                    return this.GenerateBinaryOp(gen, b.NodeType, b.Left.Type, b.Right.Type, b.Type, b.IsLiftedToNull, ask);
            }
        }

        private ExpressionCompiler.StackType GenerateNullEquality(
          ILGenerator gen,
          ExpressionType op,
          Expression e,
          bool isLiftedToNull)
        {
            int num = (int)this.Generate(gen, e, ExpressionCompiler.StackType.Value);
            if (isLiftedToNull)
            {
                gen.Emit(OpCodes.Pop);
                int constant = (int)this.GenerateConstant(gen, Expression.Constant((object)null, typeof(bool?)), ExpressionCompiler.StackType.Value);
            }
            else
            {
                LocalBuilder local = gen.DeclareLocal(e.Type);
                gen.Emit(OpCodes.Stloc, local);
                gen.Emit(OpCodes.Ldloca, local);
                this.GenerateHasValue(gen, e.Type);
                if (op == ExpressionType.Equal)
                {
                    gen.Emit(OpCodes.Ldc_I4_0);
                    gen.Emit(OpCodes.Ceq);
                }
            }
            return ExpressionCompiler.StackType.Value;
        }

        private ExpressionCompiler.StackType GenerateBinaryMethod(
          ILGenerator gen,
          BinaryExpression b,
          ExpressionCompiler.StackType ask)
        {
            if (b.IsLifted)
            {
                ParameterExpression parameterExpression1 = Expression.Parameter(Expression.GetNonNullableType(b.Left.Type), (string)null);
                ParameterExpression parameterExpression2 = Expression.Parameter(Expression.GetNonNullableType(b.Right.Type), (string)null);
                MethodCallExpression mc = Expression.Call((Expression)null, b.Method, (Expression)parameterExpression1, (Expression)parameterExpression2);
                Type resultType;
                if (b.IsLiftedToNull)
                {
                    resultType = Expression.GetNullableType(mc.Type);
                }
                else
                {
                    switch (b.NodeType)
                    {
                        case ExpressionType.Equal:
                        case ExpressionType.GreaterThan:
                        case ExpressionType.GreaterThanOrEqual:
                        case ExpressionType.LessThan:
                        case ExpressionType.LessThanOrEqual:
                        case ExpressionType.NotEqual:
                            if (mc.Type != typeof(bool))
                                throw Error.ArgumentMustBeBoolean();
                            resultType = typeof(bool);
                            break;
                        default:
                            resultType = Expression.GetNullableType(mc.Type);
                            break;
                    }
                }
                IEnumerable<ParameterExpression> parameters = (IEnumerable<ParameterExpression>)new ParameterExpression[2]
                {
          parameterExpression1,
          parameterExpression2
                };
                IEnumerable<Expression> arguments = (IEnumerable<Expression>)new Expression[2]
                {
          b.Left,
          b.Right
                };
                Expression.ValidateLift(parameters, arguments);
                return this.GenerateLift(gen, b.NodeType, resultType, mc, parameters, arguments);
            }
            MethodCallExpression node = Expression.Call((Expression)null, b.Method, b.Left, b.Right);
            return this.Generate(gen, (Expression)node, ask);
        }

        private void GenerateTypeIs(ILGenerator gen, TypeBinaryExpression b)
        {
            int num = (int)this.Generate(gen, b.Expression, ExpressionCompiler.StackType.Value);
            if (b.Expression.Type == typeof(void))
            {
                gen.Emit(OpCodes.Ldc_I4_0);
            }
            else
            {
                if (b.Expression.Type.IsValueType)
                    gen.Emit(OpCodes.Box, b.Expression.Type);
                gen.Emit(OpCodes.Isinst, b.TypeOperand);
                gen.Emit(OpCodes.Ldnull);
                gen.Emit(OpCodes.Cgt_Un);
            }
        }

        private ExpressionCompiler.StackType GenerateHoistedLocalAccess(
          ILGenerator gen,
          int hoistIndex,
          Type type,
          ExpressionCompiler.StackType ask)
        {
            this.GenerateConstInt(gen, hoistIndex);
            gen.Emit(OpCodes.Ldelem_Ref);
            Type cls = ExpressionCompiler.MakeStrongBoxType(type);
            gen.Emit(OpCodes.Castclass, cls);
            FieldInfo field = cls.GetField("Value", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            if (ask == ExpressionCompiler.StackType.Value)
                gen.Emit(OpCodes.Ldfld, field);
            else
                gen.Emit(OpCodes.Ldflda, field);
            return ask;
        }

        private ExpressionCompiler.StackType GenerateGlobalAccess(
          ILGenerator gen,
          int iGlobal,
          Type type,
          ExpressionCompiler.StackType ask)
        {
            ExpressionCompiler.GenerateLoadExecutionScope(gen);
            gen.Emit(OpCodes.Ldfld, typeof(ExecutionScope).GetField("Globals", BindingFlags.Instance | BindingFlags.Public));
            this.GenerateConstInt(gen, iGlobal);
            gen.Emit(OpCodes.Ldelem_Ref);
            Type cls = ExpressionCompiler.MakeStrongBoxType(type);
            gen.Emit(OpCodes.Castclass, cls);
            FieldInfo field = cls.GetField("Value", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            if (ask == ExpressionCompiler.StackType.Value)
                gen.Emit(OpCodes.Ldfld, field);
            else
                gen.Emit(OpCodes.Ldflda, field);
            return ask;
        }

        private int AddGlobal(Type type, object value)
        {
            int count = this.globals.Count;
            this.globals.Add(Activator.CreateInstance(ExpressionCompiler.MakeStrongBoxType(type), value));
            return count;
        }

        private void GenerateCastToType(ILGenerator gen, Type typeFrom, Type typeTo)
        {
            if (!typeFrom.IsValueType && typeTo.IsValueType)
                gen.Emit(OpCodes.Unbox_Any, typeTo);
            else if (typeFrom.IsValueType && !typeTo.IsValueType)
            {
                gen.Emit(OpCodes.Box, typeFrom);
                if (typeTo == typeof(object))
                    return;
                gen.Emit(OpCodes.Castclass, typeTo);
            }
            else
            {
                if (typeFrom.IsValueType || typeTo.IsValueType)
                    throw Error.InvalidCast((object)typeFrom, (object)typeTo);
                gen.Emit(OpCodes.Castclass, typeTo);
            }
        }

        private void GenerateNullableToNullableConversion(
          ILGenerator gen,
          Type typeFrom,
          Type typeTo,
          bool isChecked)
        {
            Label label1 = new Label();
            Label label2 = new Label();
            LocalBuilder local1 = gen.DeclareLocal(typeFrom);
            gen.Emit(OpCodes.Stloc, local1);
            LocalBuilder local2 = gen.DeclareLocal(typeTo);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateHasValue(gen, typeFrom);
            Label label3 = gen.DefineLabel();
            gen.Emit(OpCodes.Brfalse_S, label3);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateGetValueOrDefault(gen, typeFrom);
            Type nonNullableType1 = ExpressionCompiler.GetNonNullableType(typeFrom);
            Type nonNullableType2 = ExpressionCompiler.GetNonNullableType(typeTo);
            this.GenerateConvertToType(gen, nonNullableType1, nonNullableType2, isChecked);
            ConstructorInfo constructor = typeTo.GetConstructor(new Type[1]
            {
        nonNullableType2
            });
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Stloc, local2);
            Label label4 = gen.DefineLabel();
            gen.Emit(OpCodes.Br_S, label4);
            gen.MarkLabel(label3);
            gen.Emit(OpCodes.Ldloca, local2);
            gen.Emit(OpCodes.Initobj, typeTo);
            gen.MarkLabel(label4);
            gen.Emit(OpCodes.Ldloc, local2);
        }

        private void GenerateNonNullableToNullableConversion(
          ILGenerator gen,
          Type typeFrom,
          Type typeTo,
          bool isChecked)
        {
            LocalBuilder local = gen.DeclareLocal(typeTo);
            Type nonNullableType = ExpressionCompiler.GetNonNullableType(typeTo);
            this.GenerateConvertToType(gen, typeFrom, nonNullableType, isChecked);
            ConstructorInfo constructor = typeTo.GetConstructor(new Type[1]
            {
        nonNullableType
            });
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Stloc, local);
            gen.Emit(OpCodes.Ldloc, local);
        }

        private void GenerateNullableToNonNullableConversion(
          ILGenerator gen,
          Type typeFrom,
          Type typeTo,
          bool isChecked)
        {
            if (typeTo.IsValueType)
                this.GenerateNullableToNonNullableStructConversion(gen, typeFrom, typeTo, isChecked);
            else
                this.GenerateNullableToReferenceConversion(gen, typeFrom);
        }

        private void GenerateNullableToNonNullableStructConversion(
          ILGenerator gen,
          Type typeFrom,
          Type typeTo,
          bool isChecked)
        {
            LocalBuilder local = gen.DeclareLocal(typeFrom);
            gen.Emit(OpCodes.Stloc, local);
            gen.Emit(OpCodes.Ldloca, local);
            this.GenerateGetValue(gen, typeFrom);
            Type nonNullableType = ExpressionCompiler.GetNonNullableType(typeFrom);
            this.GenerateConvertToType(gen, nonNullableType, typeTo, isChecked);
        }

        private void GenerateNullableToReferenceConversion(ILGenerator gen, Type typeFrom) => gen.Emit(OpCodes.Box, typeFrom);

        private void GenerateNullableConversion(
          ILGenerator gen,
          Type typeFrom,
          Type typeTo,
          bool isChecked)
        {
            bool flag1 = ExpressionCompiler.IsNullable(typeFrom);
            bool flag2 = ExpressionCompiler.IsNullable(typeTo);
            if (flag1 && flag2)
                this.GenerateNullableToNullableConversion(gen, typeFrom, typeTo, isChecked);
            else if (flag1)
                this.GenerateNullableToNonNullableConversion(gen, typeFrom, typeTo, isChecked);
            else
                this.GenerateNonNullableToNullableConversion(gen, typeFrom, typeTo, isChecked);
        }

        private void GenerateNumericConversion(
          ILGenerator gen,
          Type typeFrom,
          Type typeTo,
          bool isChecked)
        {
            bool flag = ExpressionCompiler.IsUnsigned(typeFrom);
            ExpressionCompiler.IsFloatingPoint(typeFrom);
            if (typeTo == typeof(float))
            {
                if (flag)
                    gen.Emit(OpCodes.Conv_R_Un);
                gen.Emit(OpCodes.Conv_R4);
            }
            else if (typeTo == typeof(double))
            {
                if (flag)
                    gen.Emit(OpCodes.Conv_R_Un);
                gen.Emit(OpCodes.Conv_R8);
            }
            else
            {
                TypeCode typeCode = Type.GetTypeCode(typeTo);
                if (isChecked)
                {
                    if (flag)
                    {
                        switch (typeCode)
                        {
                            case TypeCode.Char:
                            case TypeCode.UInt16:
                                gen.Emit(OpCodes.Conv_Ovf_U2_Un);
                                break;
                            case TypeCode.SByte:
                                gen.Emit(OpCodes.Conv_Ovf_I1_Un);
                                break;
                            case TypeCode.Byte:
                                gen.Emit(OpCodes.Conv_Ovf_U1_Un);
                                break;
                            case TypeCode.Int16:
                                gen.Emit(OpCodes.Conv_Ovf_I2_Un);
                                break;
                            case TypeCode.Int32:
                                gen.Emit(OpCodes.Conv_Ovf_I4_Un);
                                break;
                            case TypeCode.UInt32:
                                gen.Emit(OpCodes.Conv_Ovf_U4_Un);
                                break;
                            case TypeCode.Int64:
                                gen.Emit(OpCodes.Conv_Ovf_I8_Un);
                                break;
                            case TypeCode.UInt64:
                                gen.Emit(OpCodes.Conv_Ovf_U8_Un);
                                break;
                            default:
                                throw Error.UnhandledConvert((object)typeTo);
                        }
                    }
                    else
                    {
                        switch (typeCode)
                        {
                            case TypeCode.Char:
                            case TypeCode.UInt16:
                                gen.Emit(OpCodes.Conv_Ovf_U2);
                                break;
                            case TypeCode.SByte:
                                gen.Emit(OpCodes.Conv_Ovf_I1);
                                break;
                            case TypeCode.Byte:
                                gen.Emit(OpCodes.Conv_Ovf_U1);
                                break;
                            case TypeCode.Int16:
                                gen.Emit(OpCodes.Conv_Ovf_I2);
                                break;
                            case TypeCode.Int32:
                                gen.Emit(OpCodes.Conv_Ovf_I4);
                                break;
                            case TypeCode.UInt32:
                                gen.Emit(OpCodes.Conv_Ovf_U4);
                                break;
                            case TypeCode.Int64:
                                gen.Emit(OpCodes.Conv_Ovf_I8);
                                break;
                            case TypeCode.UInt64:
                                gen.Emit(OpCodes.Conv_Ovf_U8);
                                break;
                            default:
                                throw Error.UnhandledConvert((object)typeTo);
                        }
                    }
                }
                else if (flag)
                {
                    switch (typeCode)
                    {
                        case TypeCode.Char:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                            gen.Emit(OpCodes.Conv_U2);
                            break;
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                            gen.Emit(OpCodes.Conv_U1);
                            break;
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                            gen.Emit(OpCodes.Conv_U4);
                            break;
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            gen.Emit(OpCodes.Conv_U8);
                            break;
                        default:
                            throw Error.UnhandledConvert((object)typeTo);
                    }
                }
                else
                {
                    switch (typeCode)
                    {
                        case TypeCode.Char:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                            gen.Emit(OpCodes.Conv_I2);
                            break;
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                            gen.Emit(OpCodes.Conv_I1);
                            break;
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                            gen.Emit(OpCodes.Conv_I4);
                            break;
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            gen.Emit(OpCodes.Conv_I8);
                            break;
                        default:
                            throw Error.UnhandledConvert((object)typeTo);
                    }
                }
            }
        }

        private void GenerateConvertToType(
          ILGenerator gen,
          Type typeFrom,
          Type typeTo,
          bool isChecked)
        {
            if (typeFrom == typeTo)
                return;
            bool flag1 = ExpressionCompiler.IsNullable(typeFrom);
            bool flag2 = ExpressionCompiler.IsNullable(typeTo);
            Type nonNullableType1 = ExpressionCompiler.GetNonNullableType(typeFrom);
            Type nonNullableType2 = ExpressionCompiler.GetNonNullableType(typeTo);
            if (typeFrom.IsInterface || typeTo.IsInterface || typeFrom == typeof(object) || typeTo == typeof(object))
                this.GenerateCastToType(gen, typeFrom, typeTo);
            else if (flag1 || flag2)
                this.GenerateNullableConversion(gen, typeFrom, typeTo, isChecked);
            else if ((!ExpressionCompiler.IsConvertible(typeFrom) || !ExpressionCompiler.IsConvertible(typeTo)) && (nonNullableType1.IsAssignableFrom(nonNullableType2) || nonNullableType2.IsAssignableFrom(nonNullableType1)))
                this.GenerateCastToType(gen, typeFrom, typeTo);
            else if (typeFrom.IsArray && typeTo.IsArray)
                this.GenerateCastToType(gen, typeFrom, typeTo);
            else
                this.GenerateNumericConversion(gen, typeFrom, typeTo, isChecked);
        }

        private ExpressionCompiler.StackType ReturnFromLocal(
          ILGenerator gen,
          ExpressionCompiler.StackType ask,
          LocalBuilder local)
        {
            if (ask == ExpressionCompiler.StackType.Address)
                gen.Emit(OpCodes.Ldloca, local);
            else
                gen.Emit(OpCodes.Ldloc, local);
            return ask;
        }

        private ExpressionCompiler.StackType GenerateUnaryOp(
          ILGenerator gen,
          ExpressionType op,
          Type operandType,
          Type resultType,
          ExpressionCompiler.StackType ask)
        {
            bool flag = ExpressionCompiler.IsNullable(operandType);
            if (op == ExpressionType.ArrayLength)
            {
                gen.Emit(OpCodes.Ldlen);
                return ExpressionCompiler.StackType.Value;
            }
            if (flag)
            {
                switch (op)
                {
                    case ExpressionType.Negate:
                    case ExpressionType.UnaryPlus:
                    case ExpressionType.NegateChecked:
                        Label label1 = gen.DefineLabel();
                        Label label2 = gen.DefineLabel();
                        LocalBuilder local1 = gen.DeclareLocal(operandType);
                        gen.Emit(OpCodes.Stloc, local1);
                        gen.Emit(OpCodes.Ldloca, local1);
                        this.GenerateHasValue(gen, operandType);
                        gen.Emit(OpCodes.Brfalse_S, label1);
                        gen.Emit(OpCodes.Ldloca, local1);
                        this.GenerateGetValueOrDefault(gen, operandType);
                        Type nonNullableType1 = ExpressionCompiler.GetNonNullableType(resultType);
                        int unaryOp1 = (int)this.GenerateUnaryOp(gen, op, nonNullableType1, nonNullableType1, ExpressionCompiler.StackType.Value);
                        ConstructorInfo constructor1 = resultType.GetConstructor(new Type[1]
                        {
              nonNullableType1
                        });
                        gen.Emit(OpCodes.Newobj, constructor1);
                        gen.Emit(OpCodes.Stloc, local1);
                        gen.Emit(OpCodes.Br_S, label2);
                        gen.MarkLabel(label1);
                        gen.Emit(OpCodes.Ldloca, local1);
                        gen.Emit(OpCodes.Initobj, resultType);
                        gen.MarkLabel(label2);
                        return this.ReturnFromLocal(gen, ask, local1);
                    case ExpressionType.Not:
                        if (operandType == typeof(bool?))
                        {
                            gen.DefineLabel();
                            Label label3 = gen.DefineLabel();
                            LocalBuilder local2 = gen.DeclareLocal(operandType);
                            gen.Emit(OpCodes.Stloc, local2);
                            gen.Emit(OpCodes.Ldloca, local2);
                            this.GenerateHasValue(gen, operandType);
                            gen.Emit(OpCodes.Brfalse_S, label3);
                            gen.Emit(OpCodes.Ldloca, local2);
                            this.GenerateGetValueOrDefault(gen, operandType);
                            Type nonNullableType2 = ExpressionCompiler.GetNonNullableType(operandType);
                            int unaryOp2 = (int)this.GenerateUnaryOp(gen, op, nonNullableType2, typeof(bool), ExpressionCompiler.StackType.Value);
                            ConstructorInfo constructor2 = resultType.GetConstructor(new Type[1]
                            {
                typeof (bool)
                            });
                            gen.Emit(OpCodes.Newobj, constructor2);
                            gen.Emit(OpCodes.Stloc, local2);
                            gen.MarkLabel(label3);
                            return this.ReturnFromLocal(gen, ask, local2);
                        }
                        goto case ExpressionType.Negate;
                    case ExpressionType.TypeAs:
                        gen.Emit(OpCodes.Box, operandType);
                        gen.Emit(OpCodes.Isinst, resultType);
                        if (ExpressionCompiler.IsNullable(resultType))
                            gen.Emit(OpCodes.Unbox_Any, resultType);
                        return ExpressionCompiler.StackType.Value;
                    default:
                        throw Error.UnhandledUnary((object)op);
                }
            }
            else
            {
                switch (op)
                {
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                        gen.Emit(OpCodes.Neg);
                        break;
                    case ExpressionType.UnaryPlus:
                        gen.Emit(OpCodes.Nop);
                        break;
                    case ExpressionType.Not:
                        if (operandType == typeof(bool))
                        {
                            gen.Emit(OpCodes.Ldc_I4_0);
                            gen.Emit(OpCodes.Ceq);
                            break;
                        }
                        gen.Emit(OpCodes.Not);
                        break;
                    case ExpressionType.TypeAs:
                        if (operandType.IsValueType)
                            gen.Emit(OpCodes.Box, operandType);
                        gen.Emit(OpCodes.Isinst, resultType);
                        if (ExpressionCompiler.IsNullable(resultType))
                        {
                            gen.Emit(OpCodes.Unbox_Any, resultType);
                            break;
                        }
                        break;
                    default:
                        throw Error.UnhandledUnary((object)op);
                }
                return ExpressionCompiler.StackType.Value;
            }
        }

        private ExpressionCompiler.StackType GenerateLiftedBinaryArithmetic(
          ILGenerator gen,
          ExpressionType op,
          Type leftType,
          Type rightType,
          Type resultType,
          ExpressionCompiler.StackType ask)
        {
            bool flag1 = ExpressionCompiler.IsNullable(leftType);
            bool flag2 = ExpressionCompiler.IsNullable(rightType);
            Label label1 = gen.DefineLabel();
            Label label2 = gen.DefineLabel();
            LocalBuilder local1 = gen.DeclareLocal(leftType);
            LocalBuilder local2 = gen.DeclareLocal(rightType);
            LocalBuilder local3 = gen.DeclareLocal(resultType);
            gen.Emit(OpCodes.Stloc, local2);
            gen.Emit(OpCodes.Stloc, local1);
            if (flag1 && flag2)
            {
                gen.Emit(OpCodes.Ldloca, local1);
                this.GenerateHasValue(gen, leftType);
                gen.Emit(OpCodes.Ldloca, local2);
                this.GenerateHasValue(gen, rightType);
                gen.Emit(OpCodes.And);
                gen.Emit(OpCodes.Brfalse_S, label1);
            }
            else if (flag1)
            {
                gen.Emit(OpCodes.Ldloca, local1);
                this.GenerateHasValue(gen, leftType);
                gen.Emit(OpCodes.Brfalse_S, label1);
            }
            else if (flag2)
            {
                gen.Emit(OpCodes.Ldloca, local2);
                this.GenerateHasValue(gen, rightType);
                gen.Emit(OpCodes.Brfalse_S, label1);
            }
            if (flag1)
            {
                gen.Emit(OpCodes.Ldloca, local1);
                this.GenerateGetValueOrDefault(gen, leftType);
            }
            else
                gen.Emit(OpCodes.Ldloc, local1);
            if (flag2)
            {
                gen.Emit(OpCodes.Ldloca, local2);
                this.GenerateGetValueOrDefault(gen, rightType);
            }
            else
                gen.Emit(OpCodes.Ldloc, local2);
            int binaryOp = (int)this.GenerateBinaryOp(gen, op, ExpressionCompiler.GetNonNullableType(leftType), ExpressionCompiler.GetNonNullableType(rightType), ExpressionCompiler.GetNonNullableType(resultType), false, ExpressionCompiler.StackType.Value);
            ConstructorInfo constructor = resultType.GetConstructor(new Type[1]
            {
        ExpressionCompiler.GetNonNullableType(resultType)
            });
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Stloc, local3);
            gen.Emit(OpCodes.Br_S, label2);
            gen.MarkLabel(label1);
            gen.Emit(OpCodes.Ldloca, local3);
            gen.Emit(OpCodes.Initobj, resultType);
            gen.MarkLabel(label2);
            return this.ReturnFromLocal(gen, ask, local3);
        }

        private ExpressionCompiler.StackType GenerateLiftedRelational(
          ILGenerator gen,
          ExpressionType op,
          Type leftType,
          Type rightType,
          Type resultType,
          bool liftedToNull,
          ExpressionCompiler.StackType ask)
        {
            Label label1 = gen.DefineLabel();
            Label label2 = gen.DefineLabel();
            Label label3 = gen.DefineLabel();
            LocalBuilder local1 = gen.DeclareLocal(leftType);
            LocalBuilder local2 = gen.DeclareLocal(rightType);
            gen.Emit(OpCodes.Stloc, local2);
            gen.Emit(OpCodes.Stloc, local1);
            switch (op)
            {
                case ExpressionType.Equal:
                    gen.Emit(OpCodes.Ldloca, local1);
                    this.GenerateHasValue(gen, leftType);
                    gen.Emit(OpCodes.Ldc_I4_0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Ldloca, local2);
                    this.GenerateHasValue(gen, rightType);
                    gen.Emit(OpCodes.Ldc_I4_0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.And);
                    gen.Emit(OpCodes.Dup);
                    if (liftedToNull)
                        gen.Emit(OpCodes.Brtrue_S, label1);
                    else
                        gen.Emit(OpCodes.Brtrue_S, label2);
                    gen.Emit(OpCodes.Pop);
                    gen.Emit(OpCodes.Ldloca, local1);
                    this.GenerateHasValue(gen, leftType);
                    gen.Emit(OpCodes.Ldloca, local2);
                    this.GenerateHasValue(gen, rightType);
                    gen.Emit(OpCodes.And);
                    gen.Emit(OpCodes.Dup);
                    if (liftedToNull)
                        gen.Emit(OpCodes.Brfalse_S, label1);
                    else
                        gen.Emit(OpCodes.Brfalse_S, label2);
                    gen.Emit(OpCodes.Pop);
                    break;
                case ExpressionType.NotEqual:
                    gen.Emit(OpCodes.Ldloca, local1);
                    this.GenerateHasValue(gen, leftType);
                    gen.Emit(OpCodes.Ldloca, local2);
                    this.GenerateHasValue(gen, rightType);
                    gen.Emit(OpCodes.Or);
                    gen.Emit(OpCodes.Dup);
                    if (liftedToNull)
                        gen.Emit(OpCodes.Brfalse_S, label1);
                    else
                        gen.Emit(OpCodes.Brfalse_S, label2);
                    gen.Emit(OpCodes.Pop);
                    gen.Emit(OpCodes.Ldloca, local1);
                    this.GenerateHasValue(gen, leftType);
                    gen.Emit(OpCodes.Ldc_I4_0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Ldloca, local2);
                    this.GenerateHasValue(gen, rightType);
                    gen.Emit(OpCodes.Ldc_I4_0);
                    gen.Emit(OpCodes.Ceq);
                    gen.Emit(OpCodes.Or);
                    gen.Emit(OpCodes.Dup);
                    if (liftedToNull)
                        gen.Emit(OpCodes.Brtrue_S, label1);
                    else
                        gen.Emit(OpCodes.Brtrue_S, label2);
                    gen.Emit(OpCodes.Pop);
                    break;
                default:
                    gen.Emit(OpCodes.Ldloca, local1);
                    this.GenerateHasValue(gen, leftType);
                    gen.Emit(OpCodes.Ldloca, local2);
                    this.GenerateHasValue(gen, rightType);
                    gen.Emit(OpCodes.And);
                    gen.Emit(OpCodes.Dup);
                    if (liftedToNull)
                        gen.Emit(OpCodes.Brfalse_S, label1);
                    else
                        gen.Emit(OpCodes.Brfalse_S, label2);
                    gen.Emit(OpCodes.Pop);
                    break;
            }
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateGetValueOrDefault(gen, leftType);
            gen.Emit(OpCodes.Ldloca, local2);
            this.GenerateGetValueOrDefault(gen, rightType);
            ExpressionCompiler.StackType binaryOp = this.GenerateBinaryOp(gen, op, ExpressionCompiler.GetNonNullableType(leftType), ExpressionCompiler.GetNonNullableType(rightType), ExpressionCompiler.GetNonNullableType(resultType), false, ask);
            gen.MarkLabel(label2);
            if (resultType != ExpressionCompiler.GetNonNullableType(resultType))
                this.GenerateConvertToType(gen, ExpressionCompiler.GetNonNullableType(resultType), resultType, true);
            gen.Emit(OpCodes.Br, label3);
            gen.MarkLabel(label1);
            gen.Emit(OpCodes.Pop);
            gen.Emit(OpCodes.Ldnull);
            gen.Emit(OpCodes.Unbox_Any, resultType);
            gen.MarkLabel(label3);
            return binaryOp;
        }

        private ExpressionCompiler.StackType GenerateLiftedBooleanAnd(
          ILGenerator gen,
          ExpressionCompiler.StackType ask)
        {
            Type type = typeof(bool?);
            Label label1 = gen.DefineLabel();
            Label label2 = gen.DefineLabel();
            Label label3 = gen.DefineLabel();
            Label label4 = gen.DefineLabel();
            Label label5 = gen.DefineLabel();
            LocalBuilder local1 = gen.DeclareLocal(type);
            LocalBuilder local2 = gen.DeclareLocal(type);
            gen.Emit(OpCodes.Stloc, local2);
            gen.Emit(OpCodes.Stloc, local1);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse, label1);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateGetValueOrDefault(gen, type);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brtrue, label2);
            gen.MarkLabel(label1);
            gen.Emit(OpCodes.Ldloca, local2);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse_S, label3);
            gen.Emit(OpCodes.Ldloca, local2);
            this.GenerateGetValueOrDefault(gen, type);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brtrue_S, label2);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse, label3);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Br_S, label4);
            gen.MarkLabel(label2);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Br_S, label4);
            gen.MarkLabel(label4);
            ConstructorInfo constructor = type.GetConstructor(new Type[1]
            {
        typeof (bool)
            });
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Stloc, local1);
            gen.Emit(OpCodes.Br, label5);
            gen.MarkLabel(label3);
            gen.Emit(OpCodes.Ldloca, local1);
            gen.Emit(OpCodes.Initobj, type);
            gen.MarkLabel(label5);
            return this.ReturnFromLocal(gen, ask, local1);
        }

        private ExpressionCompiler.StackType GenerateLiftedBooleanOr(
          ILGenerator gen,
          ExpressionCompiler.StackType ask)
        {
            Type type = typeof(bool?);
            Label label1 = gen.DefineLabel();
            Label label2 = gen.DefineLabel();
            Label label3 = gen.DefineLabel();
            Label label4 = gen.DefineLabel();
            Label label5 = gen.DefineLabel();
            LocalBuilder local1 = gen.DeclareLocal(type);
            LocalBuilder local2 = gen.DeclareLocal(type);
            gen.Emit(OpCodes.Stloc, local2);
            gen.Emit(OpCodes.Stloc, local1);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse, label1);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateGetValueOrDefault(gen, type);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brfalse, label2);
            gen.MarkLabel(label1);
            gen.Emit(OpCodes.Ldloca, local2);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse_S, label3);
            gen.Emit(OpCodes.Ldloca, local2);
            this.GenerateGetValueOrDefault(gen, type);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ceq);
            gen.Emit(OpCodes.Brfalse_S, label2);
            gen.Emit(OpCodes.Ldloca, local1);
            this.GenerateHasValue(gen, type);
            gen.Emit(OpCodes.Brfalse, label3);
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Br_S, label4);
            gen.MarkLabel(label2);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Br_S, label4);
            gen.MarkLabel(label4);
            ConstructorInfo constructor = type.GetConstructor(new Type[1]
            {
        typeof (bool)
            });
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Stloc, local1);
            gen.Emit(OpCodes.Br, label5);
            gen.MarkLabel(label3);
            gen.Emit(OpCodes.Ldloca, local1);
            gen.Emit(OpCodes.Initobj, type);
            gen.MarkLabel(label5);
            return this.ReturnFromLocal(gen, ask, local1);
        }

        private ExpressionCompiler.StackType GenerateLiftedBinaryOp(
          ILGenerator gen,
          ExpressionType op,
          Type leftType,
          Type rightType,
          Type resultType,
          bool liftedToNull,
          ExpressionCompiler.StackType ask)
        {
            switch (op)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Divide:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.LeftShift:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return this.GenerateLiftedBinaryArithmetic(gen, op, leftType, rightType, resultType, ask);
                case ExpressionType.And:
                    return leftType == typeof(bool?) ? this.GenerateLiftedBooleanAnd(gen, ask) : this.GenerateLiftedBinaryArithmetic(gen, op, leftType, rightType, resultType, ask);
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    return this.GenerateLiftedRelational(gen, op, leftType, rightType, resultType, liftedToNull, ask);
                case ExpressionType.Or:
                    return leftType == typeof(bool?) ? this.GenerateLiftedBooleanOr(gen, ask) : this.GenerateLiftedBinaryArithmetic(gen, op, leftType, rightType, resultType, ask);
                default:
                    return ExpressionCompiler.StackType.Value;
            }
        }

        private static void GenerateUnliftedEquality(ILGenerator gen, ExpressionType op, Type type)
        {
            if (!type.IsPrimitive && type.IsValueType && !type.IsEnum)
                throw Error.OperatorNotImplementedForType((object)op, (object)type);
            gen.Emit(OpCodes.Ceq);
            if (op != ExpressionType.NotEqual)
                return;
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Ceq);
        }

        private ExpressionCompiler.StackType GenerateUnliftedBinaryOp(
          ILGenerator gen,
          ExpressionType op,
          Type leftType,
          Type rightType)
        {
            if (op == ExpressionType.Equal || op == ExpressionType.NotEqual)
            {
                ExpressionCompiler.GenerateUnliftedEquality(gen, op, leftType);
                return ExpressionCompiler.StackType.Value;
            }
            if (!leftType.IsPrimitive)
                throw Error.OperatorNotImplementedForType((object)op, (object)leftType);
            switch (op)
            {
                case ExpressionType.Add:
                    gen.Emit(OpCodes.Add);
                    break;
                case ExpressionType.AddChecked:
                    LocalBuilder local1 = gen.DeclareLocal(leftType);
                    LocalBuilder local2 = gen.DeclareLocal(rightType);
                    gen.Emit(OpCodes.Stloc, local2);
                    gen.Emit(OpCodes.Stloc, local1);
                    gen.Emit(OpCodes.Ldloc, local1);
                    gen.Emit(OpCodes.Ldloc, local2);
                    if (ExpressionCompiler.IsFloatingPoint(leftType))
                    {
                        gen.Emit(OpCodes.Add);
                        break;
                    }
                    if (ExpressionCompiler.IsUnsigned(leftType))
                    {
                        gen.Emit(OpCodes.Add_Ovf_Un);
                        break;
                    }
                    gen.Emit(OpCodes.Add_Ovf);
                    break;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    gen.Emit(OpCodes.And);
                    break;
                case ExpressionType.Divide:
                    if (ExpressionCompiler.IsUnsigned(leftType))
                    {
                        gen.Emit(OpCodes.Div_Un);
                        break;
                    }
                    gen.Emit(OpCodes.Div);
                    break;
                case ExpressionType.ExclusiveOr:
                    gen.Emit(OpCodes.Xor);
                    break;
                case ExpressionType.GreaterThan:
                    if (ExpressionCompiler.IsUnsigned(leftType))
                    {
                        gen.Emit(OpCodes.Cgt_Un);
                        break;
                    }
                    gen.Emit(OpCodes.Cgt);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    Label label1 = gen.DefineLabel();
                    Label label2 = gen.DefineLabel();
                    if (ExpressionCompiler.IsUnsigned(leftType))
                        gen.Emit(OpCodes.Bge_Un_S, label1);
                    else
                        gen.Emit(OpCodes.Bge_S, label1);
                    gen.Emit(OpCodes.Ldc_I4_0);
                    gen.Emit(OpCodes.Br_S, label2);
                    gen.MarkLabel(label1);
                    gen.Emit(OpCodes.Ldc_I4_1);
                    gen.MarkLabel(label2);
                    break;
                case ExpressionType.LeftShift:
                    Type nonNullableType1 = ExpressionCompiler.GetNonNullableType(rightType);
                    if (nonNullableType1 != typeof(int))
                        this.GenerateConvertToType(gen, nonNullableType1, typeof(int), true);
                    gen.Emit(OpCodes.Shl);
                    break;
                case ExpressionType.LessThan:
                    if (ExpressionCompiler.IsUnsigned(leftType))
                    {
                        gen.Emit(OpCodes.Clt_Un);
                        break;
                    }
                    gen.Emit(OpCodes.Clt);
                    break;
                case ExpressionType.LessThanOrEqual:
                    Label label3 = gen.DefineLabel();
                    Label label4 = gen.DefineLabel();
                    if (ExpressionCompiler.IsUnsigned(leftType))
                        gen.Emit(OpCodes.Ble_Un_S, label3);
                    else
                        gen.Emit(OpCodes.Ble_S, label3);
                    gen.Emit(OpCodes.Ldc_I4_0);
                    gen.Emit(OpCodes.Br_S, label4);
                    gen.MarkLabel(label3);
                    gen.Emit(OpCodes.Ldc_I4_1);
                    gen.MarkLabel(label4);
                    break;
                case ExpressionType.Modulo:
                    if (ExpressionCompiler.IsUnsigned(leftType))
                    {
                        gen.Emit(OpCodes.Rem_Un);
                        break;
                    }
                    gen.Emit(OpCodes.Rem);
                    break;
                case ExpressionType.Multiply:
                    gen.Emit(OpCodes.Mul);
                    break;
                case ExpressionType.MultiplyChecked:
                    LocalBuilder local3 = gen.DeclareLocal(leftType);
                    LocalBuilder local4 = gen.DeclareLocal(rightType);
                    gen.Emit(OpCodes.Stloc, local4);
                    gen.Emit(OpCodes.Stloc, local3);
                    gen.Emit(OpCodes.Ldloc, local3);
                    gen.Emit(OpCodes.Ldloc, local4);
                    if (ExpressionCompiler.IsFloatingPoint(leftType))
                    {
                        gen.Emit(OpCodes.Mul);
                        break;
                    }
                    if (ExpressionCompiler.IsUnsigned(leftType))
                    {
                        gen.Emit(OpCodes.Mul_Ovf_Un);
                        break;
                    }
                    gen.Emit(OpCodes.Mul_Ovf);
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    gen.Emit(OpCodes.Or);
                    break;
                case ExpressionType.RightShift:
                    Type nonNullableType2 = ExpressionCompiler.GetNonNullableType(rightType);
                    if (nonNullableType2 != typeof(int))
                        this.GenerateConvertToType(gen, nonNullableType2, typeof(int), true);
                    if (ExpressionCompiler.IsUnsigned(leftType))
                    {
                        gen.Emit(OpCodes.Shr_Un);
                        break;
                    }
                    gen.Emit(OpCodes.Shr);
                    break;
                case ExpressionType.Subtract:
                    gen.Emit(OpCodes.Sub);
                    break;
                case ExpressionType.SubtractChecked:
                    LocalBuilder local5 = gen.DeclareLocal(leftType);
                    LocalBuilder local6 = gen.DeclareLocal(rightType);
                    gen.Emit(OpCodes.Stloc, local6);
                    gen.Emit(OpCodes.Stloc, local5);
                    gen.Emit(OpCodes.Ldloc, local5);
                    gen.Emit(OpCodes.Ldloc, local6);
                    if (ExpressionCompiler.IsFloatingPoint(leftType))
                    {
                        gen.Emit(OpCodes.Sub);
                        break;
                    }
                    if (ExpressionCompiler.IsUnsigned(leftType))
                    {
                        gen.Emit(OpCodes.Sub_Ovf_Un);
                        break;
                    }
                    gen.Emit(OpCodes.Sub_Ovf);
                    break;
                default:
                    throw Error.UnhandledBinary((object)op);
            }
            return ExpressionCompiler.StackType.Value;
        }

        private ExpressionCompiler.StackType GenerateBinaryOp(
          ILGenerator gen,
          ExpressionType op,
          Type leftType,
          Type rightType,
          Type resultType,
          bool liftedToNull,
          ExpressionCompiler.StackType ask)
        {
            bool flag1 = ExpressionCompiler.IsNullable(leftType);
            bool flag2 = ExpressionCompiler.IsNullable(rightType);
            switch (op)
            {
                case ExpressionType.ArrayIndex:
                    if (flag2)
                    {
                        LocalBuilder local = gen.DeclareLocal(rightType);
                        gen.Emit(OpCodes.Stloc, local);
                        gen.Emit(OpCodes.Ldloca, local);
                        this.GenerateGetValue(gen, rightType);
                    }
                    Type nonNullableType = ExpressionCompiler.GetNonNullableType(rightType);
                    if (nonNullableType != typeof(int))
                        this.GenerateConvertToType(gen, nonNullableType, typeof(int), true);
                    return this.GenerateArrayAccess(gen, leftType.GetElementType(), ask);
                case ExpressionType.Coalesce:
                    throw Error.UnexpectedCoalesceOperator();
                default:
                    return flag1 ? this.GenerateLiftedBinaryOp(gen, op, leftType, rightType, resultType, liftedToNull, ask) : this.GenerateUnliftedBinaryOp(gen, op, leftType, rightType);
            }
        }

        private ExpressionCompiler.StackType GenerateArgAccess(
          ILGenerator gen,
          int iArg,
          ExpressionCompiler.StackType ask)
        {
            if (ask == ExpressionCompiler.StackType.Value)
            {
                switch (iArg)
                {
                    case 0:
                        gen.Emit(OpCodes.Ldarg_0);
                        break;
                    case 1:
                        gen.Emit(OpCodes.Ldarg_1);
                        break;
                    case 2:
                        gen.Emit(OpCodes.Ldarg_2);
                        break;
                    case 3:
                        gen.Emit(OpCodes.Ldarg_3);
                        break;
                    default:
                        if (iArg < 128)
                        {
                            gen.Emit(OpCodes.Ldarg_S, (byte)iArg);
                            break;
                        }
                        gen.Emit(OpCodes.Ldarg, iArg);
                        break;
                }
            }
            else if (iArg < 128)
                gen.Emit(OpCodes.Ldarga_S, (byte)iArg);
            else
                gen.Emit(OpCodes.Ldarga, iArg);
            return ask;
        }

        private void GenerateConstInt(ILGenerator gen, int value)
        {
            switch (value)
            {
                case -1:
                    gen.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    gen.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    gen.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    gen.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    gen.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    gen.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    gen.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    gen.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    gen.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    gen.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    if (value >= -127 && value < 128)
                    {
                        gen.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                        break;
                    }
                    gen.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }

        private void GenerateArrayAssign(ILGenerator gen, Type type)
        {
            if (type.IsEnum)
            {
                gen.Emit(OpCodes.Stelem, type);
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                        gen.Emit(OpCodes.Stelem_I1);
                        break;
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                        gen.Emit(OpCodes.Stelem_I2);
                        break;
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                        gen.Emit(OpCodes.Stelem_I4);
                        break;
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        gen.Emit(OpCodes.Stelem_I8);
                        break;
                    case TypeCode.Single:
                        gen.Emit(OpCodes.Stelem_R4);
                        break;
                    case TypeCode.Double:
                        gen.Emit(OpCodes.Stelem_R8);
                        break;
                    default:
                        if (type.IsValueType)
                        {
                            gen.Emit(OpCodes.Stelem, type);
                            break;
                        }
                        gen.Emit(OpCodes.Stelem_Ref);
                        break;
                }
            }
        }

        private ExpressionCompiler.StackType GenerateArrayAccess(
          ILGenerator gen,
          Type type,
          ExpressionCompiler.StackType ask)
        {
            if (ask == ExpressionCompiler.StackType.Address)
                gen.Emit(OpCodes.Ldelema, type);
            else if (!type.IsValueType)
                gen.Emit(OpCodes.Ldelem_Ref);
            else if (type.IsEnum)
            {
                gen.Emit(OpCodes.Ldelem, type);
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.SByte:
                        gen.Emit(OpCodes.Ldelem_I1);
                        break;
                    case TypeCode.Int16:
                        gen.Emit(OpCodes.Ldelem_I2);
                        break;
                    case TypeCode.Int32:
                        gen.Emit(OpCodes.Ldelem_I4);
                        break;
                    case TypeCode.Int64:
                        gen.Emit(OpCodes.Ldelem_I8);
                        break;
                    case TypeCode.Single:
                        gen.Emit(OpCodes.Ldelem_R4);
                        break;
                    case TypeCode.Double:
                        gen.Emit(OpCodes.Ldelem_R8);
                        break;
                    default:
                        gen.Emit(OpCodes.Ldelem, type);
                        break;
                }
            }
            return ask;
        }

        private void GenerateHasValue(ILGenerator gen, Type nullableType)
        {
            MethodInfo method = nullableType.GetMethod("get_HasValue", BindingFlags.Instance | BindingFlags.Public);
            gen.Emit(OpCodes.Call, method);
        }

        private void GenerateGetValue(ILGenerator gen, Type nullableType)
        {
            MethodInfo method = nullableType.GetMethod("get_Value", BindingFlags.Instance | BindingFlags.Public);
            gen.Emit(OpCodes.Call, method);
        }

        private void GenerateGetValueOrDefault(ILGenerator gen, Type nullableType)
        {
            MethodInfo method = nullableType.GetMethod("GetValueOrDefault", Type.EmptyTypes);
            gen.Emit(OpCodes.Call, method);
        }

        private static bool IsNullable(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

        private static Type GetNonNullableType(Type type) => ExpressionCompiler.IsNullable(type) ? type.GetGenericArguments()[0] : type;

        private static bool IsConvertible(Type type)
        {
            type = ExpressionCompiler.GetNonNullableType(type);
            if (type.IsEnum)
                return true;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsUnsigned(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GetGenericArguments()[0];
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsFloatingPoint(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GetGenericArguments()[0];
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }

        internal class LambdaInfo
        {
            internal LambdaExpression Lambda;
            internal List<ExpressionCompiler.LambdaInfo> Lambdas;
            internal MethodInfo Method;
            internal Dictionary<ParameterExpression, int> HoistedLocals;

            internal LambdaInfo(
              LambdaExpression lambda,
              MethodInfo method,
              Dictionary<ParameterExpression, int> hoistedLocals,
              List<ExpressionCompiler.LambdaInfo> lambdas)
            {
                this.Lambda = lambda;
                this.Method = method;
                this.HoistedLocals = hoistedLocals;
                this.Lambdas = lambdas;
            }
        }

        private class CompileScope
        {
            internal ExpressionCompiler.CompileScope Parent;
            internal LambdaExpression Lambda;
            internal Dictionary<ParameterExpression, LocalBuilder> Locals;
            internal Dictionary<ParameterExpression, int> HoistedLocals;
            internal LocalBuilder HoistedLocalsVar;

            internal CompileScope(ExpressionCompiler.CompileScope parent, LambdaExpression lambda)
            {
                this.Parent = parent;
                this.Lambda = lambda;
                this.Locals = new Dictionary<ParameterExpression, LocalBuilder>();
                this.HoistedLocals = new Dictionary<ParameterExpression, int>();
            }
        }

        private enum StackType
        {
            Value,
            Address,
        }

        private class Hoister : ExpressionVisitor
        {
            private ExpressionCompiler.CompileScope expressionScope;
            private LambdaExpression current;
            private List<ParameterExpression> locals;

            internal Hoister()
            {
            }

            internal void Hoist(ExpressionCompiler.CompileScope scope)
            {
                this.expressionScope = scope;
                this.current = scope.Lambda;
                this.locals = new List<ParameterExpression>((IEnumerable<ParameterExpression>)scope.Lambda.Parameters);
                this.Visit(scope.Lambda.Body);
            }

            internal override Expression VisitParameter(ParameterExpression p)
            {
                if (this.locals.Contains(p) && this.expressionScope.Lambda != this.current && !this.expressionScope.HoistedLocals.ContainsKey(p))
                    this.expressionScope.HoistedLocals.Add(p, this.expressionScope.HoistedLocals.Count);
                return (Expression)p;
            }

            internal override Expression VisitInvocation(InvocationExpression iv)
            {
                if (this.expressionScope.Lambda == this.current)
                {
                    if (iv.Expression.NodeType == ExpressionType.Lambda)
                        this.locals.AddRange((IEnumerable<ParameterExpression>)((LambdaExpression)iv.Expression).Parameters);
                    else if (iv.Expression.NodeType == ExpressionType.Quote && iv.Expression.Type.IsSubclassOf(typeof(LambdaExpression)))
                        this.locals.AddRange((IEnumerable<ParameterExpression>)((LambdaExpression)((UnaryExpression)iv.Expression).Operand).Parameters);
                }
                return base.VisitInvocation(iv);
            }

            internal override Expression VisitLambda(LambdaExpression l)
            {
                LambdaExpression current = this.current;
                this.current = l;
                this.Visit(l.Body);
                this.current = current;
                return (Expression)l;
            }
        }

        private struct WriteBack
        {
            public LocalBuilder loc;
            public Expression arg;

            public WriteBack(LocalBuilder loc, Expression arg)
            {
                this.loc = loc;
                this.arg = arg;
            }
        }
    }
}
