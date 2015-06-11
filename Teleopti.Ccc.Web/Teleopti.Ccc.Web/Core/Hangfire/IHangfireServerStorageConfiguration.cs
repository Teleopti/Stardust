using System;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[CLSCompliant(false)]
	public interface IHangfireServerStorageConfiguration
	{
		void ConfigureStorage();
	}
}