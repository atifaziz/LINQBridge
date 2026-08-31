using System.Dynamic;

namespace DynamicSample.Models;

public class Bag : DynamicObject
{
    private readonly System.Collections.Generic.Dictionary<string, object> _items =
        new System.Collections.Generic.Dictionary<string, object>();

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        return _items.TryGetValue(binder.Name, out result);
    }

    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        _items[binder.Name] = value;
        return true;
    }

    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
        result = binder.Name + "(" + args.Length + " args)";
        return true;
    }
}