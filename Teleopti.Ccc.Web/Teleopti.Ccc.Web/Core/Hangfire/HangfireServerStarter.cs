using System;
using Autofac;
using Hangfire;
using Owin;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[CLSCompliant(false)]
	public class HangfireServerStarter : IHangfireServerStarter
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IHangfireServerStorageConfiguration _storageConfiguration;

		public HangfireServerStarter(ILifetimeScope lifetimeScope, IHangfireServerStorageConfiguration storageConfiguration)
		{
			_lifetimeScope = lifetimeScope;
			_storageConfiguration = storageConfiguration;
		}

		public void Start(IAppBuilder application)
		{
			application.UseHangfire(c =>
			{
				_storageConfiguration.ConfigureStorage(c);
				c.UseAutofacActivator(_lifetimeScope);
				c.UseServer(1);// cant have more than 1 yet even though we use distrubuted locks
			});
		}
	}

}