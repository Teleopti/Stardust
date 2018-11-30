using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Toggle;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public class AbsenceTypesProvider : IAbsenceTypesProvider
	{
		private readonly IAbsenceRepository _absenceRepository;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IToggleManager _toggleManager;
		private readonly INow _now;

		public AbsenceTypesProvider(IAbsenceRepository absenceRepository, ILoggedOnUser loggedOnUser,
			IToggleManager toggleManager, INow now)
		{
			_absenceRepository = absenceRepository;
			_loggedOnUser = loggedOnUser;
			_toggleManager = toggleManager;
			_now = now;
		}

		public IEnumerable<IAbsence> GetRequestableAbsences()
		{
			if (_toggleManager.IsEnabled(Toggles.MyTimeWeb_AbsenceRequest_LimitAbsenceTypes_77446))
			{
				var currentUser = _loggedOnUser?.CurrentUser();
				if (currentUser?.WorkflowControlSet?.AbsenceRequestOpenPeriods == null)
				{
					return new List<IAbsence>();
				}
				var timeZone = currentUser.PermissionInformation.DefaultTimeZone();
				var agentToday = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), timeZone));
				return currentUser.WorkflowControlSet.AbsenceRequestOpenPeriods.Where(openPeriod =>
					openPeriod.Absence.Requestable
					&& openPeriod.OpenForRequestsPeriod.Contains(agentToday)).Select(a => a.Absence).Distinct().ToList();
			}
			return _absenceRepository.LoadRequestableAbsence().ToList();
		}

		public IEnumerable<IAbsence> GetReportableAbsences()
		{
			var currentUser = _loggedOnUser?.CurrentUser();
			if (currentUser?.WorkflowControlSet?.AllowedAbsencesForReport == null)
			{
				return new List<IAbsence>();
			}

			return currentUser.WorkflowControlSet.AllowedAbsencesForReport;
		}
	}
}