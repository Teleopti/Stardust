using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public interface ISdkProjectionServiceFactory
	{
		IProjectionService CreateProjectionService(IScheduleDay scheduleDay, string specialProjection, TimeZoneInfo timeZoneInfo);
	}
}