
using Autofac;
using Microsoft.AspNetCore.Builder;


namespace Stardust.Manager
{
	public static class AppBuilderExtension
	{
		public static void UseStardustManager(this IApplicationBuilder applicationBuilder,
		                                      ManagerConfiguration managerConfiguration,
		                                      ILifetimeScope lifetimeScope)
		{
            applicationBuilder.Map(
				managerConfiguration.Route,
				innerConfiguration =>
				{
                    
                    applicationBuilder.UseHttpsRedirection();
                    applicationBuilder.UseRouting();
                    applicationBuilder.UseAuthorization();
                    

                    applicationBuilder.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });

            //to start the timers etc
		//	lifetimeScope.Resolve<ManagerController>();
		}
	}
}