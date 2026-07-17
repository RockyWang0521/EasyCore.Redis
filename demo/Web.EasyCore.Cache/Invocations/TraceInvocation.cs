using EasyCore.Invocation;

namespace Web.EasyCore.Cache.Invocations;

/// <summary>Minimal Invocation wrapper so Redis demo can exercise EasyCore.Invocation.</summary>
public sealed class TraceInvocation : IInvocation
{
    public async ValueTask<object?> InvokeAsync(InvocationContext context, InvocationDelegate next)
    {
        Console.WriteLine($"[Trace/Invocation] Before {context.TargetType.Name}.{context.MethodName}");
        try
        {
            var result = await next();
            Console.WriteLine($"[Trace/Invocation] After  {context.TargetType.Name}.{context.MethodName}");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Trace/Invocation] Error  {context.MethodName}: {ex.Message}");
            throw;
        }
    }
}
