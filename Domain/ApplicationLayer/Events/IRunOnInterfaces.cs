using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public interface IRunOnHangfire
	{
	}

	[Obsolete("We're phasing out the Rhino ESB, use Hangfire or Stardust instead.")]
	public interface IRunOnServiceBus
	{
	}

	public interface IRunOnStardust
	{
	}

	public interface IRunInSyncInFatClientProcess
	{
	}
}