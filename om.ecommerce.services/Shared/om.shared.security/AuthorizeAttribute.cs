using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using om.shared.security.Interfaces;
using System;
using System.Linq;
using System.Net;

namespace om.shared.security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private static IAuthService authService=null;
        public string Roles { get; set; }
        public AuthorizeAttribute()
        {
            
        }
        public static void RegisterAuthService(IAuthService _authService)
        {
            AuthorizeAttribute.authService = _authService;
        }
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext != null)
            {
                Microsoft.Extensions.Primitives.StringValues authTokens;
                filterContext.HttpContext.Request.Headers.TryGetValue("Authorization", out authTokens);

                var _token = authTokens.FirstOrDefault();

                if (_token != null && _token.StartsWith("Bearer ",StringComparison.InvariantCultureIgnoreCase))
                {
                    string authToken = _token.Substring("Bearer ".Length);
                    if (authToken != null)
                    {
                        int errorCode=401;
                        if (AuthorizeAttribute.authService !=null && AuthorizeAttribute.authService.ValidateToken(authToken,this.Roles, out errorCode))
                        {
                            filterContext.HttpContext.Response.Headers.Add("authToken", authToken);
                            filterContext.HttpContext.Response.Headers.Add("AuthStatus", "Authorized");

                            filterContext.HttpContext.Response.Headers.Add("storeAccessiblity", "Authorized");

                            return;
                        }
                        else
                        {
                            filterContext.HttpContext.Response.Headers.Add("authToken", authToken);
                            filterContext.HttpContext.Response.Headers.Add("AuthStatus", "NotAuthorized");

                            filterContext.HttpContext.Response.StatusCode = (errorCode == 403)?(int)HttpStatusCode.Forbidden:(int)HttpStatusCode.Unauthorized;
                            filterContext.HttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = (errorCode == 403) ? "Forbidden" : "Not Authorized";
                            filterContext.Result = new JsonResult((errorCode == 403) ? "Forbidden" : "NotAuthorized")
                            {
                                Value = new
                                {
                                    Status = "Error",
                                    Message = (errorCode == 403) ? "User does have permission to access this Api Endpoint": "User is not authorized to access this Api Endpoint"
                                },
                            };
                        }

                    }

                }
                else
                {
                    filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
                    filterContext.HttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Kindly provide valid bearer JWT token";
                    filterContext.Result = new JsonResult("Kindly provide valid bearer JWT token")
                    {
                        Value = new
                        {
                            Status = "Error",
                            Message = "Kindly provide valid bearer JWT token"
                        },
                    };
                }
            }
        }
    }
}
