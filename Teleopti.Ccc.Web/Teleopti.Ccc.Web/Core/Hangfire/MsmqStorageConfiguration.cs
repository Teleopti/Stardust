using System;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.SqlServer.Msmq;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[CLSCompliant(false)]
	public class MsmqStorageConfiguration : IHangfireServerStorageConfiguration
	{
		private readonly IConfigReader _config;

		public MsmqStorageConfiguration(IConfigReader config)
		{
			_config = config;
		}

		public void ConfigureStorage(IBootstrapperConfiguration configuration)
		{
			configuration.UseSqlServerStorage(
				_config.ConnectionStrings["Hangfire"].ConnectionString,
				new SqlServerStorageOptions
				{
					PrepareSchemaIfNecessary = false
				}
				).UseMsmqQueues(_config.AppSettings["Hangfire.MSMQ"])
				;
		}

		// How I made MSMQ work with Hangfire:
		// put @"TELEOPTI710\PRIVATE$\hangfire-{0}" as queue name
		// in windows features, select MSMQ. Do not select MSMQ Active Directory Domain Services Integration
		//		Programs and Features -> Turn Windows features on or off -> Microsoft Message Queue (MSMQ) Server
		// in computer management, add the private queue "hangfire-default". Check Transaictional!
		//		Computer Management -> Services and Applications -> Message Queueuing -> Private Queues -> 
		//		Right click -> New -> Private Queue ...
		// permit everyone full access to the queue by selecting Full Control to all users
		//		Computer Management -> Services and Applications -> Message Queueuing -> Private Queues -> 
		//		hangfire-default -> Right click -> Properties -> Security ...

	}
}