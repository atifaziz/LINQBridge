namespace System.Linq.Expressions
{
    internal static class Strings
    {
        internal static string OwningTeam => StringResources.GetString(nameof(OwningTeam));

        internal static string UserDefinedOperatorMustBeStatic(object p0) => StringResources.GetString(nameof(UserDefinedOperatorMustBeStatic), p0);

        internal static string UserDefinedOperatorMustNotBeVoid(object p0) => StringResources.GetString(nameof(UserDefinedOperatorMustNotBeVoid), p0);

        internal static string CoercionOperatorNotDefined(object p0, object p1) => StringResources.GetString(nameof(CoercionOperatorNotDefined), p0, p1);

        internal static string UnaryOperatorNotDefined(object p0, object p1) => StringResources.GetString(nameof(UnaryOperatorNotDefined), p0, p1);

        internal static string BinaryOperatorNotDefined(object p0, object p1, object p2) => StringResources.GetString(nameof(BinaryOperatorNotDefined), p0, p1, p2);

        internal static string OperandTypesDoNotMatchParameters(object p0, object p1) => StringResources.GetString(nameof(OperandTypesDoNotMatchParameters), p0, p1);

        internal static string ArgumentMustBeArray => StringResources.GetString(nameof(ArgumentMustBeArray));

        internal static string ArgumentMustBeBoolean => StringResources.GetString(nameof(ArgumentMustBeBoolean));

        internal static string ArgumentMustBeComparable => StringResources.GetString(nameof(ArgumentMustBeComparable));

        internal static string ArgumentMustBeConvertible => StringResources.GetString(nameof(ArgumentMustBeConvertible));

        internal static string ArgumentMustBeFieldInfoOrPropertInfo => StringResources.GetString(nameof(ArgumentMustBeFieldInfoOrPropertInfo));

        internal static string ArgumentMustBeFieldInfoOrPropertInfoOrMethod => StringResources.GetString(nameof(ArgumentMustBeFieldInfoOrPropertInfoOrMethod));

        internal static string ArgumentMustBeInstanceMember => StringResources.GetString(nameof(ArgumentMustBeInstanceMember));

        internal static string ArgumentMustBeInteger => StringResources.GetString(nameof(ArgumentMustBeInteger));

        internal static string ArgumentMustBeInt32 => StringResources.GetString(nameof(ArgumentMustBeInt32));

        internal static string ArgumentMustBeCheckable => StringResources.GetString(nameof(ArgumentMustBeCheckable));

        internal static string ArgumentMustBeArrayIndexType => StringResources.GetString(nameof(ArgumentMustBeArrayIndexType));

        internal static string ArgumentMustBeIntegerOrBoolean => StringResources.GetString(nameof(ArgumentMustBeIntegerOrBoolean));

        internal static string ArgumentMustBeNumeric => StringResources.GetString(nameof(ArgumentMustBeNumeric));

        internal static string ArgumentMustBeSingleDimensionalArrayType => StringResources.GetString(nameof(ArgumentMustBeSingleDimensionalArrayType));

        internal static string ArgumentTypesMustMatch => StringResources.GetString(nameof(ArgumentTypesMustMatch));

        internal static string CannotAutoInitializeValueTypeElementThroughProperty(object p0) => StringResources.GetString(nameof(CannotAutoInitializeValueTypeElementThroughProperty), p0);

        internal static string CannotAutoInitializeValueTypeMemberThroughProperty(object p0) => StringResources.GetString(nameof(CannotAutoInitializeValueTypeMemberThroughProperty), p0);

        internal static string CannotCastTypeToType(object p0, object p1) => StringResources.GetString(nameof(CannotCastTypeToType), p0, p1);

        internal static string IncorrectTypeForTypeAs(object p0) => StringResources.GetString(nameof(IncorrectTypeForTypeAs), p0);

        internal static string CoalesceUsedOnNonNullType => StringResources.GetString(nameof(CoalesceUsedOnNonNullType));

        internal static string ExpressionTypeCannotInitializeCollectionType(object p0, object p1) => StringResources.GetString(nameof(ExpressionTypeCannotInitializeCollectionType), p0, p1);

        internal static string ExpressionTypeCannotInitializeArrayType(object p0, object p1) => StringResources.GetString(nameof(ExpressionTypeCannotInitializeArrayType), p0, p1);

        internal static string ExpressionTypeDoesNotMatchArrayType(object p0, object p1) => StringResources.GetString(nameof(ExpressionTypeDoesNotMatchArrayType), p0, p1);

        internal static string ExpressionTypeDoesNotMatchConstructorParameter(object p0, object p1) => StringResources.GetString(nameof(ExpressionTypeDoesNotMatchConstructorParameter), p0, p1);

        internal static string ArgumentTypeDoesNotMatchMember(object p0, object p1) => StringResources.GetString(nameof(ArgumentTypeDoesNotMatchMember), p0, p1);

        internal static string ArgumentMemberNotDeclOnType(object p0, object p1) => StringResources.GetString(nameof(ArgumentMemberNotDeclOnType), p0, p1);

        internal static string ExpressionTypeDoesNotMatchMethodParameter(
          object p0,
          object p1,
          object p2)
        {
            return StringResources.GetString(nameof(ExpressionTypeDoesNotMatchMethodParameter), p0, p1, p2);
        }

        internal static string ExpressionTypeDoesNotMatchParameter(object p0, object p1) => StringResources.GetString(nameof(ExpressionTypeDoesNotMatchParameter), p0, p1);

        internal static string ExpressionTypeDoesNotMatchReturn(object p0, object p1) => StringResources.GetString(nameof(ExpressionTypeDoesNotMatchReturn), p0, p1);

        internal static string ExpressionTypeNotInvocable(object p0) => StringResources.GetString(nameof(ExpressionTypeNotInvocable), p0);

        internal static string FieldNotDefinedForType(object p0, object p1) => StringResources.GetString(nameof(FieldNotDefinedForType), p0, p1);

        internal static string IncorrectNumberOfIndexes => StringResources.GetString(nameof(IncorrectNumberOfIndexes));

        internal static string IncorrectNumberOfLambdaArguments => StringResources.GetString(nameof(IncorrectNumberOfLambdaArguments));

        internal static string IncorrectNumberOfLambdaDeclarationParameters => StringResources.GetString(nameof(IncorrectNumberOfLambdaDeclarationParameters));

        internal static string IncorrectNumberOfMethodCallArguments(object p0) => StringResources.GetString(nameof(IncorrectNumberOfMethodCallArguments), p0);

        internal static string IncorrectNumberOfConstructorArguments => StringResources.GetString(nameof(IncorrectNumberOfConstructorArguments));

        internal static string IncorrectNumberOfMembersForGivenConstructor => StringResources.GetString(nameof(IncorrectNumberOfMembersForGivenConstructor));

        internal static string IncorrectNumberOfArgumentsForMembers => StringResources.GetString(nameof(IncorrectNumberOfArgumentsForMembers));

        internal static string LambdaParameterNotInScope => StringResources.GetString(nameof(LambdaParameterNotInScope));

        internal static string LambdaTypeMustBeDerivedFromSystemDelegate => StringResources.GetString(nameof(LambdaTypeMustBeDerivedFromSystemDelegate));

        internal static string MemberNotFieldOrProperty(object p0) => StringResources.GetString(nameof(MemberNotFieldOrProperty), p0);

        internal static string MethodContainsGenericParameters(object p0) => StringResources.GetString(nameof(MethodContainsGenericParameters), p0);

        internal static string MethodIsGeneric(object p0) => StringResources.GetString(nameof(MethodIsGeneric), p0);

        internal static string MethodNotPropertyAccessor(object p0, object p1) => StringResources.GetString(nameof(MethodNotPropertyAccessor), p0, p1);

        internal static string PropertyDoesNotHaveGetter(object p0) => StringResources.GetString(nameof(PropertyDoesNotHaveGetter), p0);

        internal static string PropertyDoesNotHaveSetter(object p0) => StringResources.GetString(nameof(PropertyDoesNotHaveSetter), p0);

        internal static string NotAMemberOfType(object p0, object p1) => StringResources.GetString(nameof(NotAMemberOfType), p0, p1);

        internal static string OperatorNotImplementedForType(object p0, object p1) => StringResources.GetString(nameof(OperatorNotImplementedForType), p0, p1);

        internal static string ParameterExpressionNotValidAsDelegate(object p0, object p1) => StringResources.GetString(nameof(ParameterExpressionNotValidAsDelegate), p0, p1);

        internal static string ParameterNotCaptured => StringResources.GetString(nameof(ParameterNotCaptured));

        internal static string PropertyNotDefinedForType(object p0, object p1) => StringResources.GetString(nameof(PropertyNotDefinedForType), p0, p1);

        internal static string MethodNotDefinedForType(object p0, object p1) => StringResources.GetString(nameof(MethodNotDefinedForType), p0, p1);

        internal static string TypeContainsGenericParameters(object p0) => StringResources.GetString(nameof(TypeContainsGenericParameters), p0);

        internal static string TypeIsGeneric(object p0) => StringResources.GetString(nameof(TypeIsGeneric), p0);

        internal static string TypeMissingDefaultConstructor(object p0) => StringResources.GetString(nameof(TypeMissingDefaultConstructor), p0);

        internal static string ListInitializerWithZeroMembers => StringResources.GetString(nameof(ListInitializerWithZeroMembers));

        internal static string ElementInitializerMethodNotAdd => StringResources.GetString(nameof(ElementInitializerMethodNotAdd));

        internal static string ElementInitializerMethodNoRefOutParam(object p0, object p1) => StringResources.GetString(nameof(ElementInitializerMethodNoRefOutParam), p0, p1);

        internal static string ElementInitializerMethodWithZeroArgs => StringResources.GetString(nameof(ElementInitializerMethodWithZeroArgs));

        internal static string ElementInitializerMethodStatic => StringResources.GetString(nameof(ElementInitializerMethodStatic));

        internal static string TypeNotIEnumerable(object p0) => StringResources.GetString(nameof(TypeNotIEnumerable), p0);

        internal static string TypeParameterIsNotDelegate(object p0) => StringResources.GetString(nameof(TypeParameterIsNotDelegate), p0);

        internal static string UnexpectedCoalesceOperator => StringResources.GetString(nameof(UnexpectedCoalesceOperator));

        internal static string InvalidCast(object p0, object p1) => StringResources.GetString(nameof(InvalidCast), p0, p1);

        internal static string UnhandledCall(object p0) => StringResources.GetString(nameof(UnhandledCall), p0);

        internal static string UnhandledBinary(object p0) => StringResources.GetString(nameof(UnhandledBinary), p0);

        internal static string UnhandledBinding => StringResources.GetString(nameof(UnhandledBinding));

        internal static string UnhandledBindingType(object p0) => StringResources.GetString(nameof(UnhandledBindingType), p0);

        internal static string UnhandledConvert(object p0) => StringResources.GetString(nameof(UnhandledConvert), p0);

        internal static string UnhandledConvertFromDecimal(object p0) => StringResources.GetString(nameof(UnhandledConvertFromDecimal), p0);

        internal static string UnhandledConvertToDecimal(object p0) => StringResources.GetString(nameof(UnhandledConvertToDecimal), p0);

        internal static string UnhandledExpressionType(object p0) => StringResources.GetString(nameof(UnhandledExpressionType), p0);

        internal static string UnhandledMemberAccess(object p0) => StringResources.GetString(nameof(UnhandledMemberAccess), p0);

        internal static string UnhandledUnary(object p0) => StringResources.GetString(nameof(UnhandledUnary), p0);

        internal static string UnknownBindingType => StringResources.GetString(nameof(UnknownBindingType));

        internal static string LogicalOperatorMustHaveConsistentTypes(object p0, object p1) => StringResources.GetString(nameof(LogicalOperatorMustHaveConsistentTypes), p0, p1);

        internal static string LogicalOperatorMustHaveBooleanOperators(object p0, object p1) => StringResources.GetString(nameof(LogicalOperatorMustHaveBooleanOperators), p0, p1);

        internal static string MethodDoesNotExistOnType(object p0, object p1) => StringResources.GetString(nameof(MethodDoesNotExistOnType), p0, p1);

        internal static string MethodWithArgsDoesNotExistOnType(object p0, object p1) => StringResources.GetString(nameof(MethodWithArgsDoesNotExistOnType), p0, p1);

        internal static string MethodWithMoreThanOneMatch(object p0, object p1) => StringResources.GetString(nameof(MethodWithMoreThanOneMatch), p0, p1);

        internal static string IncorrectNumberOfTypeArgsForFunc => StringResources.GetString(nameof(IncorrectNumberOfTypeArgsForFunc));

        internal static string IncorrectNumberOfTypeArgsForAction => StringResources.GetString(nameof(IncorrectNumberOfTypeArgsForAction));

        internal static string ExpressionMayNotContainByrefParameters => StringResources.GetString(nameof(ExpressionMayNotContainByrefParameters));

        internal static string ArgumentCannotBeOfTypeVoid => StringResources.GetString(nameof(ArgumentCannotBeOfTypeVoid));
    }
}
