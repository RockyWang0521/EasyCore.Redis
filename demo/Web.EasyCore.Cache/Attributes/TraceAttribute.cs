using EasyCore.Invocation;
using Web.EasyCore.Cache.Invocations;

namespace Web.EasyCore.Cache.Attributes;

public sealed class TraceAttribute : InvocationAttribute<TraceInvocation>
{
    public TraceAttribute() => Order = 0;
}
