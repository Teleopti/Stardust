using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public interface ISdkProjectionServiceFactory
	{
		IProjectionService CreateProjectionService(IScheduleDay scheduleDay, string specialProjection, TimeZoneInfo timeZoneInfo);
	}
}