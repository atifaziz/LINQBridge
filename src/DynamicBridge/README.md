# Net20.DynamicBridge

Use the C# `dynamic` keyword on .NET Framework 2.0, 3.0 and 3.5.

`dynamic` is a compiler feature: the C# compiler lowers every dynamic operation into a *call site*
that asks a runtime binder what to do. Those types — `System.Runtime.CompilerServices.CallSite`,
`System.Dynamic.*` and `Microsoft.CSharp.RuntimeBinder.*` — first shipped with .NET 4.0, which is the
only reason `dynamic` cannot be used on earlier frameworks. This package supplies them, in their real
namespaces, so the compiler is satisfied and the operations bind at run time.

It is the companion of [`Net20.LinqBridge`](https://github.com/pieroviano/LINQBridge) (LINQ and
expression trees on 2.0/3.0) and `Net4x.AsyncBridge` (`async`/`await` on frameworks without it).

```csharp
dynamic customer = new Customer { Name = "Ada" };

customer.Name = "Grace";                 // set member
var greeting = customer.Greet("hi", 2);  // overload resolution, on a net20 assembly
var first = customer[0];                 // indexer

dynamic expando = new ExpandoObject();
expando.Anything = 42;
```

## Installing

```
dotnet add package Net20.DynamicBridge
```

`<LangVersion>latest</LangVersion>` is required in the consuming project: the framework may be 2.0,
but the *compiler* has to be one that knows the `dynamic` keyword.

| Target | What you get |
| --- | --- |
| net20, net30 | Full implementation. Expression trees come from `Net20.LinqBridge`, which the package depends on. |
| net35 | Full implementation, using the 3.5 `System.Core` for expression trees; no LinqBridge dependency. |
| net40 and later, netstandard2.0 | An empty facade. The Framework's own `Microsoft.CSharp` and `System.Core` provide these types, so referencing this package on a modern target changes nothing and cannot cause an ambiguity. |

That last row is the point of the split: a project that multi-targets `net20;net48` gets the bridge
where the framework lacks `dynamic` and the real thing everywhere else, with no `#if` in your code.

## What works

- Member get and set on properties and fields, instance and static, including inherited and
  interface members.
- Method invocation with overload resolution, generic type inference, explicit type arguments,
  `params` arrays, optional arguments, named arguments, and `ref`/`out` arguments (written back).
- Indexers, arrays and `IList`/`IDictionary` indexing, for both reading and writing.
- Delegate invocation, including delegate-valued fields and properties, and constructor invocation
  with dynamic arguments.
- All binary and unary operators with C# numeric promotion, `checked`/`unchecked` semantics, string
  concatenation, enum and delegate operators, user-defined operators, compound assignment,
  `++`/`--` and the short-circuiting `&&`/`||`.
- Implicit and explicit conversions, including nullable, enum and user-defined conversions.
- `System.Dynamic.DynamicObject` — every `TryGetMember`/`TrySetMember`/`TryInvokeMember`/… override,
  with fallback to the CLR type when an override returns `false`.
- `System.Dynamic.ExpandoObject`, including `IDictionary<string, object>`, insertion-order
  enumeration and `INotifyPropertyChanged`.
- Failures raise `Microsoft.CSharp.RuntimeBinder.RuntimeBinderException`, as on 4.0.

Behaviour is checked by a differential test suite: the same fixtures are compiled once against this
package (net30 and net35) and once against the real `Microsoft.CSharp` (net40), and must pass
identically on all three.

## Limitations

The Framework's binder compiles a *rule* — an expression tree — for each call site and caches it.
This package does not: it has no 4.0 expression compiler to run such a rule on 2.0, so it performs
each operation directly instead. That is invisible for ordinary use of `dynamic`, but it has three
consequences worth knowing:

1. **Custom DLR binders that only implement `CallSiteBinder.Bind`** — the overload returning a rule
   expression — are not supported; calling one throws `NotSupportedException` with an explanation.
   Derive from the `System.Dynamic` binders (`GetMemberBinder`, `InvokeMemberBinder`, …) instead:
   those run the ordinary meta-object protocol and work as they do on 4.0.
2. **No rule caching.** Each dynamic operation resolves its member through reflection every time, so
   a dynamic call is slower than on 4.0. Correctness is unaffected.
3. **Overload resolution uses runtime types.** The Framework binder also considers the *static* type
   of an argument when the compiler recorded one. In the rare case where an overload set can only be
   disambiguated by an argument's compile-time type, the choice here may differ.

Also note that extension methods are invisible to dynamic dispatch — the same as on 4.0, where the
receiver being `dynamic` likewise prevents extension method lookup.

## Licence

New BSD, as the rest of the LINQBridge distribution.
