using System.Configuration;
using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Core.Startup.Booter;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(100)]
	public class TenantStartupTask : IBootstrapperTask
	{
		private readonly CheckTenantConnectionstrings _checker;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;

		public TenantStartupTask(CheckTenantConnectionstrings checker, ITenantUnitOfWork tenantUnitOfWork)
		{
			_checker = checker;
			_tenantUnitOfWork = tenantUnitOfWork;
		}

		public Task Execute(IAppBuilder application)
		{
			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenantConnString = ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString;
				_checker.CheckEm(tenantConnString);
			}
			
			return null;
		}
	}
}