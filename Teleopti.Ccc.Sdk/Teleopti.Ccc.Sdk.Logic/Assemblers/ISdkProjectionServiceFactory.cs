using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public interface ISdkProjectionServiceFactory
	{
		IProjectionService CreateProjectionService(IScheduleDay scheduleDay, string specialProjection, ICccTimeZoneInfo timeZoneInfo);
	}
}