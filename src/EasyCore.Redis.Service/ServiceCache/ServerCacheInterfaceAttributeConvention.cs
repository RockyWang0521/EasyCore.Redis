using System.Reflection;
using EasyCore.Redis.Service.Attribute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace EasyCore.Redis.Service;

/// <summary>
/// Copies <see cref="ServerCacheAttribute"/> from implemented interfaces onto MVC actions
/// (Controllers / EasyCoreAppService contracts).
/// </summary>
internal sealed class ServerCacheInterfaceAttributeConvention : IApplicationModelConvention
{
    public void Apply(ApplicationModel application)
    {
        ArgumentNullException.ThrowIfNull(application);

        foreach (var controller in application.Controllers)
        {
            var controllerType = controller.ControllerType.AsType();
            if (!typeof(ControllerBase).IsAssignableFrom(controllerType))
            {
                continue;
            }

            foreach (var action in controller.Actions)
            {
                ApplyAction(controllerType, action);
            }
        }
    }

    private static void ApplyAction(Type controllerType, ActionModel action)
    {
        var existing = new HashSet<ServerCacheAttribute>(ReferenceEqualityComparer.Instance);
        foreach (var filter in action.Filters.OfType<ServerCacheAttribute>())
        {
            existing.Add(filter);
        }

        foreach (var filter in action.Controller.Filters.OfType<ServerCacheAttribute>())
        {
            existing.Add(filter);
        }

        foreach (var attr in controllerType.GetCustomAttributes<ServerCacheAttribute>(inherit: true))
        {
            existing.Add(attr);
        }

        if (action.ActionMethod is null)
        {
            return;
        }

        foreach (var attr in action.ActionMethod.GetCustomAttributes<ServerCacheAttribute>(inherit: true))
        {
            existing.Add(attr);
        }

        foreach (var attr in ServerCacheAttributeLocator.GetInterfaceAttributes(controllerType, action.ActionMethod))
        {
            if (existing.Add(attr))
            {
                action.Filters.Add(attr);
            }
        }
    }
}
