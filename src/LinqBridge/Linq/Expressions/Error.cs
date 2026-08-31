namespace System.Linq.Expressions
{
    internal static class Error
    {
        internal static Exception UserDefinedOperatorMustBeStatic(object p0) => (Exception)new ArgumentException(Strings.UserDefinedOperatorMustBeStatic(p0));

        internal static Exception UserDefinedOperatorMustNotBeVoid(object p0) => (Exception)new ArgumentException(Strings.UserDefinedOperatorMustNotBeVoid(p0));

        internal static Exception CoercionOperatorNotDefined(object p0, object p1) => (Exception)new InvalidOperationException(Strings.CoercionOperatorNotDefined(p0, p1));

        internal static Exception UnaryOperatorNotDefined(object p0, object p1) => (Exception)new InvalidOperationException(Strings.UnaryOperatorNotDefined(p0, p1));

        internal static Exception BinaryOperatorNotDefined(object p0, object p1, object p2) => (Exception)new InvalidOperationException(Strings.BinaryOperatorNotDefined(p0, p1, p2));

        internal static Exception OperandTypesDoNotMatchParameters(object p0, object p1) => (Exception)new InvalidOperationException(Strings.OperandTypesDoNotMatchParameters(p0, p1));

        internal static Exception ArgumentMustBeArray() => (Exception)new ArgumentException(Strings.ArgumentMustBeArray);

        internal static Exception ArgumentMustBeBoolean() => (Exception)new ArgumentException(Strings.ArgumentMustBeBoolean);

        internal static Exception ArgumentMustBeComparable() => (Exception)new ArgumentException(Strings.ArgumentMustBeComparable);

        internal static Exception ArgumentMustBeConvertible() => (Exception)new ArgumentException(Strings.ArgumentMustBeConvertible);

        internal static Exception ArgumentMustBeFieldInfoOrPropertInfo() => (Exception)new ArgumentException(Strings.ArgumentMustBeFieldInfoOrPropertInfo);

        internal static Exception ArgumentMustBeFieldInfoOrPropertInfoOrMethod() => (Exception)new ArgumentException(Strings.ArgumentMustBeFieldInfoOrPropertInfoOrMethod);

        internal static Exception ArgumentMustBeInstanceMember() => (Exception)new ArgumentException(Strings.ArgumentMustBeInstanceMember);

        internal static Exception ArgumentMustBeInteger() => (Exception)new ArgumentException(Strings.ArgumentMustBeInteger);

        internal static Exception ArgumentMustBeInt32() => (Exception)new ArgumentException(Strings.ArgumentMustBeInt32);

        internal static Exception ArgumentMustBeCheckable() => (Exception)new ArgumentException(Strings.ArgumentMustBeCheckable);

        internal static Exception ArgumentMustBeArrayIndexType() => (Exception)new ArgumentException(Strings.ArgumentMustBeArrayIndexType);

        internal static Exception ArgumentMustBeIntegerOrBoolean() => (Exception)new ArgumentException(Strings.ArgumentMustBeIntegerOrBoolean);

        internal static Exception ArgumentMustBeNumeric() => (Exception)new ArgumentException(Strings.ArgumentMustBeNumeric);

        internal static Exception ArgumentMustBeSingleDimensionalArrayType() => (Exception)new ArgumentException(Strings.ArgumentMustBeSingleDimensionalArrayType);

        internal static Exception ArgumentTypesMustMatch() => (Exception)new ArgumentException(Strings.ArgumentTypesMustMatch);

        internal static Exception CannotAutoInitializeValueTypeElementThroughProperty(object p0) => (Exception)new InvalidOperationException(Strings.CannotAutoInitializeValueTypeElementThroughProperty(p0));

        internal static Exception CannotAutoInitializeValueTypeMemberThroughProperty(object p0) => (Exception)new InvalidOperationException(Strings.CannotAutoInitializeValueTypeMemberThroughProperty(p0));

        internal static Exception CannotCastTypeToType(object p0, object p1) => (Exception)new ArgumentException(Strings.CannotCastTypeToType(p0, p1));

        internal static Exception IncorrectTypeForTypeAs(object p0) => (Exception)new ArgumentException(Strings.IncorrectTypeForTypeAs(p0));

        internal static Exception CoalesceUsedOnNonNullType() => (Exception)new InvalidOperationException(Strings.CoalesceUsedOnNonNullType);

        internal static Exception ExpressionTypeCannotInitializeCollectionType(
          object p0,
          object p1)
        {
            return (Exception)new InvalidOperationException(Strings.ExpressionTypeCannotInitializeCollectionType(p0, p1));
        }

        internal static Exception ExpressionTypeCannotInitializeArrayType(object p0, object p1) => (Exception)new InvalidOperationException(Strings.ExpressionTypeCannotInitializeArrayType(p0, p1));

        internal static Exception ExpressionTypeDoesNotMatchArrayType(object p0, object p1) => (Exception)new InvalidOperationException(Strings.ExpressionTypeDoesNotMatchArrayType(p0, p1));

        internal static Exception ExpressionTypeDoesNotMatchConstructorParameter(
          object p0,
          object p1)
        {
            return (Exception)new ArgumentException(Strings.ExpressionTypeDoesNotMatchConstructorParameter(p0, p1));
        }

        internal static Exception ArgumentTypeDoesNotMatchMember(object p0, object p1) => (Exception)new ArgumentException(Strings.ArgumentTypeDoesNotMatchMember(p0, p1));

        internal static Exception ArgumentMemberNotDeclOnType(object p0, object p1) => (Exception)new ArgumentException(Strings.ArgumentMemberNotDeclOnType(p0, p1));

        internal static Exception ExpressionTypeDoesNotMatchMethodParameter(
          object p0,
          object p1,
          object p2)
        {
            return (Exception)new ArgumentException(Strings.ExpressionTypeDoesNotMatchMethodParameter(p0, p1, p2));
        }

        internal static Exception ExpressionTypeDoesNotMatchParameter(object p0, object p1) => (Exception)new ArgumentException(Strings.ExpressionTypeDoesNotMatchParameter(p0, p1));

        internal static Exception ExpressionTypeDoesNotMatchReturn(object p0, object p1) => (Exception)new ArgumentException(Strings.ExpressionTypeDoesNotMatchReturn(p0, p1));

        internal static Exception ExpressionTypeNotInvocable(object p0) => (Exception)new ArgumentException(Strings.ExpressionTypeNotInvocable(p0));

        internal static Exception FieldNotDefinedForType(object p0, object p1) => (Exception)new ArgumentException(Strings.FieldNotDefinedForType(p0, p1));

        internal static Exception IncorrectNumberOfIndexes() => (Exception)new ArgumentException(Strings.IncorrectNumberOfIndexes);

        internal static Exception IncorrectNumberOfLambdaArguments() => (Exception)new InvalidOperationException(Strings.IncorrectNumberOfLambdaArguments);

        internal static Exception IncorrectNumberOfLambdaDeclarationParameters() => (Exception)new ArgumentException(Strings.IncorrectNumberOfLambdaDeclarationParameters);

        internal static Exception IncorrectNumberOfMethodCallArguments(object p0) => (Exception)new ArgumentException(Strings.IncorrectNumberOfMethodCallArguments(p0));

        internal static Exception IncorrectNumberOfConstructorArguments() => (Exception)new ArgumentException(Strings.IncorrectNumberOfConstructorArguments);

        internal static Exception IncorrectNumberOfMembersForGivenConstructor() => (Exception)new ArgumentException(Strings.IncorrectNumberOfMembersForGivenConstructor);

        internal static Exception IncorrectNumberOfArgumentsForMembers() => (Exception)new ArgumentException(Strings.IncorrectNumberOfArgumentsForMembers);

        internal static Exception LambdaParameterNotInScope() => (Exception)new InvalidOperationException(Strings.LambdaParameterNotInScope);

        internal static Exception LambdaTypeMustBeDerivedFromSystemDelegate() => (Exception)new ArgumentException(Strings.LambdaTypeMustBeDerivedFromSystemDelegate);

        internal static Exception MemberNotFieldOrProperty(object p0) => (Exception)new ArgumentException(Strings.MemberNotFieldOrProperty(p0));

        internal static Exception MethodContainsGenericParameters(object p0) => (Exception)new ArgumentException(Strings.MethodContainsGenericParameters(p0));

        internal static Exception MethodIsGeneric(object p0) => (Exception)new ArgumentException(Strings.MethodIsGeneric(p0));

        internal static Exception MethodNotPropertyAccessor(object p0, object p1) => (Exception)new ArgumentException(Strings.MethodNotPropertyAccessor(p0, p1));

        internal static Exception PropertyDoesNotHaveGetter(object p0) => (Exception)new ArgumentException(Strings.PropertyDoesNotHaveGetter(p0));

        internal static Exception PropertyDoesNotHaveSetter(object p0) => (Exception)new ArgumentException(Strings.PropertyDoesNotHaveSetter(p0));

        internal static Exception NotAMemberOfType(object p0, object p1) => (Exception)new ArgumentException(Strings.NotAMemberOfType(p0, p1));

        internal static Exception OperatorNotImplementedForType(object p0, object p1) => (Exception)new NotImplementedException(Strings.OperatorNotImplementedForType(p0, p1));

        internal static Exception ParameterExpressionNotValidAsDelegate(object p0, object p1) => (Exception)new ArgumentException(Strings.ParameterExpressionNotValidAsDelegate(p0, p1));

        internal static Exception ParameterNotCaptured() => (Exception)new ArgumentException(Strings.ParameterNotCaptured);

        internal static Exception PropertyNotDefinedForType(object p0, object p1) => (Exception)new ArgumentException(Strings.PropertyNotDefinedForType(p0, p1));

        internal static Exception MethodNotDefinedForType(object p0, object p1) => (Exception)new ArgumentException(Strings.MethodNotDefinedForType(p0, p1));

        internal static Exception TypeContainsGenericParameters(object p0) => (Exception)new ArgumentException(Strings.TypeContainsGenericParameters(p0));

        internal static Exception TypeIsGeneric(object p0) => (Exception)new ArgumentException(Strings.TypeIsGeneric(p0));

        internal static Exception TypeMissingDefaultConstructor(object p0) => (Exception)new ArgumentException(Strings.TypeMissingDefaultConstructor(p0));

        internal static Exception ListInitializerWithZeroMembers() => (Exception)new ArgumentException(Strings.ListInitializerWithZeroMembers);

        internal static Exception ElementInitializerMethodNotAdd() => (Exception)new ArgumentException(Strings.ElementInitializerMethodNotAdd);

        internal static Exception ElementInitializerMethodNoRefOutParam(object p0, object p1) => (Exception)new ArgumentException(Strings.ElementInitializerMethodNoRefOutParam(p0, p1));

        internal static Exception ElementInitializerMethodWithZeroArgs() => (Exception)new ArgumentException(Strings.ElementInitializerMethodWithZeroArgs);

        internal static Exception ElementInitializerMethodStatic() => (Exception)new ArgumentException(Strings.ElementInitializerMethodStatic);

        internal static Exception TypeNotIEnumerable(object p0) => (Exception)new ArgumentException(Strings.TypeNotIEnumerable(p0));

        internal static Exception TypeParameterIsNotDelegate(object p0) => (Exception)new InvalidOperationException(Strings.TypeParameterIsNotDelegate(p0));

        internal static Exception UnexpectedCoalesceOperator() => (Exception)new InvalidOperationException(Strings.UnexpectedCoalesceOperator);

        internal static Exception InvalidCast(object p0, object p1) => (Exception)new InvalidOperationException(Strings.InvalidCast(p0, p1));

        internal static Exception UnhandledCall(object p0) => (Exception)new ArgumentException(Strings.UnhandledCall(p0));

        internal static Exception UnhandledBinary(object p0) => (Exception)new ArgumentException(Strings.UnhandledBinary(p0));

        internal static Exception UnhandledBinding() => (Exception)new ArgumentException(Strings.UnhandledBinding);

        internal static Exception UnhandledBindingType(object p0) => (Exception)new ArgumentException(Strings.UnhandledBindingType(p0));

        internal static Exception UnhandledConvert(object p0) => (Exception)new ArgumentException(Strings.UnhandledConvert(p0));

        internal static Exception UnhandledConvertFromDecimal(object p0) => (Exception)new ArgumentException(Strings.UnhandledConvertFromDecimal(p0));

        internal static Exception UnhandledConvertToDecimal(object p0) => (Exception)new ArgumentException(Strings.UnhandledConvertToDecimal(p0));

        internal static Exception UnhandledExpressionType(object p0) => (Exception)new ArgumentException(Strings.UnhandledExpressionType(p0));

        internal static Exception UnhandledMemberAccess(object p0) => (Exception)new ArgumentException(Strings.UnhandledMemberAccess(p0));

        internal static Exception UnhandledUnary(object p0) => (Exception)new ArgumentException(Strings.UnhandledUnary(p0));

        internal static Exception UnknownBindingType() => (Exception)new ArgumentException(Strings.UnknownBindingType);

        internal static Exception LogicalOperatorMustHaveConsistentTypes(object p0, object p1) => (Exception)new ArgumentException(Strings.LogicalOperatorMustHaveConsistentTypes(p0, p1));

        internal static Exception LogicalOperatorMustHaveBooleanOperators(object p0, object p1) => (Exception)new ArgumentException(Strings.LogicalOperatorMustHaveBooleanOperators(p0, p1));

        internal static Exception MethodDoesNotExistOnType(object p0, object p1) => (Exception)new InvalidOperationException(Strings.MethodDoesNotExistOnType(p0, p1));

        internal static Exception MethodWithArgsDoesNotExistOnType(object p0, object p1) => (Exception)new InvalidOperationException(Strings.MethodWithArgsDoesNotExistOnType(p0, p1));

        internal static Exception MethodWithMoreThanOneMatch(object p0, object p1) => (Exception)new InvalidOperationException(Strings.MethodWithMoreThanOneMatch(p0, p1));

        internal static Exception IncorrectNumberOfTypeArgsForFunc() => (Exception)new ArgumentException(Strings.IncorrectNumberOfTypeArgsForFunc);

        internal static Exception IncorrectNumberOfTypeArgsForAction() => (Exception)new ArgumentException(Strings.IncorrectNumberOfTypeArgsForAction);

        internal static Exception ExpressionMayNotContainByrefParameters() => (Exception)new ArgumentException(Strings.ExpressionMayNotContainByrefParameters);

        internal static Exception ArgumentCannotBeOfTypeVoid() => (Exception)new ArgumentException(Strings.ArgumentCannotBeOfTypeVoid);

        internal static Exception ArgumentNull(string paramName) => (Exception)new ArgumentNullException(paramName);

        internal static Exception ArgumentOutOfRange(string paramName) => (Exception)new ArgumentOutOfRangeException(paramName);

        internal static Exception NotImplemented() => (Exception)new NotImplementedException();

        internal static Exception NotSupported() => (Exception)new NotSupportedException();
    }
}
