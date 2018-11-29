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
			var absencesForRequest = new List<IAbsence>();

			if (_toggleManager.IsEnabled(Toggles.MyTimeWeb_AbsenceRequest_LimitAbsenceTypes_77446))
			{
				if (_loggedOnUser == null ||
					_loggedOnUser.CurrentUser() == null ||
					_loggedOnUser.CurrentUser().WorkflowControlSet == null ||
					_loggedOnUser.CurrentUser().WorkflowControlSet.AbsenceRequestOpenPeriods == null)
				{
					return new List<IAbsence>();
				}
				var timeZone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
				var agentToday = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), timeZone));
				foreach (var openPeriod in _loggedOnUser.CurrentUser().WorkflowControlSet.AbsenceRequestOpenPeriods)
				{
					if (openPeriod.Absence.Requestable
						&& openPeriod.OpenForRequestsPeriod.Contains(agentToday) 
						&& !absencesForRequest.Contains(openPeriod.Absence) ) absencesForRequest.Add(openPeriod.Absence);
				}
			}
			else absencesForRequest = _absenceRepository.LoadRequestableAbsence().ToList();

			return absencesForRequest;
		}

		public IEnumerable<IAbsence> GetReportableAbsences()
		{
			if (_loggedOnUser == null ||
				_loggedOnUser.CurrentUser() == null ||
				_loggedOnUser.CurrentUser().WorkflowControlSet == null ||
				_loggedOnUser.CurrentUser().WorkflowControlSet.AllowedAbsencesForReport == null)
			{
				return new List<IAbsence>();
			}

			return _loggedOnUser.CurrentUser().WorkflowControlSet.AllowedAbsencesForReport;
		}
	}
}