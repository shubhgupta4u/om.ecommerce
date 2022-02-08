using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace om.shared.api.common.Extensions
{
    public static class ControllerBaseExtensions
    {
        public static string GetRemoteIpAddress(this ControllerBase controller)
        {
            if (controller.Request.Headers.ContainsKey("X-Forwarded-For"))
                return controller.Request.Headers["X-Forwarded-For"];
            else
                return controller.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
        public static T GetLoggedInUserId<T>(this ControllerBase controller)
        {
            var principal = controller.User;
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            var loggedInUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(loggedInUserId, typeof(T));
            }
            else if (typeof(T) == typeof(int) || typeof(T) == typeof(long))
            {
                return loggedInUserId != null ? (T)Convert.ChangeType(loggedInUserId, typeof(T)) : (T)Convert.ChangeType(0, typeof(T));
            }
            else
            {
                throw new Exception("Invalid type provided");
            }
        }

        public static string GetLoggedInUserName(this ControllerBase controller)
        {
            var principal = controller.User;
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirstValue(ClaimTypes.Name);
        }

        public static string GetLoggedInUserEmail(this ControllerBase controller)
        {
            var principal = controller.User;
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            return principal.FindFirstValue(ClaimTypes.Email);
        }
    }
}
