using System;

namespace System.Linq.Expressions
{
    [__DynamicallyInvokable]
    public interface IArgumentProvider
    {
        [__DynamicallyInvokable]
        int ArgumentCount
        {
            [__DynamicallyInvokable]
            get;
        }

        [__DynamicallyInvokable]
        Expression GetArgument(int index);
    }
}