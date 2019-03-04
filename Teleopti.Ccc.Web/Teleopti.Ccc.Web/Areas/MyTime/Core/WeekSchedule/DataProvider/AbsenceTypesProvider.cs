using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public class AbsenceTypesProvider : IAbsenceTypesProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly INow _now;

		public AbsenceTypesProvider(ILoggedOnUser loggedOnUser, INow now)
		{
			_loggedOnUser = loggedOnUser;
			_now = now;
		}

		public IEnumerable<IAbsence> GetRequestableAbsences()
		{
			var currentUser = _loggedOnUser?.CurrentUser();
			if (currentUser?.WorkflowControlSet?.AbsenceRequestOpenPeriods == null)
			{
				return new List<IAbsence>();
			}
			var timeZone = currentUser.PermissionInformation.DefaultTimeZone();
			var agentToday = _now.CurrentLocalDate(timeZone);
			return currentUser.WorkflowControlSet.AbsenceRequestOpenPeriods.Where(openPeriod =>
				openPeriod.Absence.Requestable
				&& openPeriod.OpenForRequestsPeriod.Contains(agentToday)).Select(a => a.Absence).Distinct().ToList();
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