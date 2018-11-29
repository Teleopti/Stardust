using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AlreadyAbsentValidator : IAlreadyAbsentValidator
	{
		private IGlobalSettingDataRepository _globalSettingsDataRepository;

		public AlreadyAbsentValidator(IGlobalSettingDataRepository globalSettingsDataRepository)
		{
			_globalSettingsDataRepository = globalSettingsDataRepository;
		}

		public bool Validate(IAbsenceRequest absenceRequest, IScheduleRange scheduleRange)
		{
			var timeZone = absenceRequest.Person.PermissionInformation.DefaultTimeZone();
			var period = FullDayAbsenceRequestPeriodUtil.AdjustFullDayAbsencePeriodIfRequired(absenceRequest.Period,
				absenceRequest.Person,
				scheduleRange.ScheduledDay(new DateOnly(absenceRequest.Period.StartDateTimeLocal(timeZone)))
				, scheduleRange.ScheduledDay(new DateOnly(absenceRequest.Period.EndDateTimeLocal(timeZone))), _globalSettingsDataRepository);

			var scheduleDays = scheduleRange.ScheduledDayCollection(
				period.ToDateOnlyPeriod(absenceRequest.Person.PermissionInformation.DefaultTimeZone()).Inflate(1));
			foreach (var scheduleDay in scheduleDays)
			{
				var projection = scheduleDay.ProjectionService().CreateProjection();
				var absenceLayers = projection.FilterLayers(period).FilterLayers<IAbsence>();
				if (absenceLayers.Any()) return true;
			}
			return false;
		}
	}
}