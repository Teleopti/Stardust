using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using Owin;
using Stardust.Manager.Models;

namespace Stardust.Manager
{
    public static class AppBuilderExtension
    {
        public static void UseStardustManager(this IAppBuilder appBuilder, ManagerConfiguration managerConfiguration)
        {
            appBuilder.Map(
                managerConfiguration.Route,
                inner =>
                {
                    var config = new HttpConfiguration();

                    config.MapHttpAttributeRoutes();
					
                    config.Services.Add(typeof (IExceptionLogger),
                        new GlobalExceptionLogger());

                    inner.UseAutofacWebApi(config);
                    inner.UseWebApi(config);
                });
        }
    }
}