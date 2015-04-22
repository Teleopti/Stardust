using System;
using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.Startup
{
	[TaskPriority(16)]
	public class ReportTextsStartupTask : IBootstrapperTask
	{
		private readonly Lazy<IApplicationData> _applicationData;

		public ReportTextsStartupTask(Lazy<IApplicationData> applicationData)
		{
			_applicationData = applicationData;
		}

		public Task Execute(IAppBuilder application)
		{
			_applicationData.Value.DoOnAllTenants_AvoidUsingThis(tenant =>
			{
				var connString = tenant.Statistic.ConnectionString;
				TextLoader.LoadAllTextsToDatabase(connString);
			});
			return null;
		}
	}
}