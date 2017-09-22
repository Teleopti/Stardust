using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public class AbsenceTypesProvider : IAbsenceTypesProvider
	{
		private readonly IAbsenceRepository _absenceRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public AbsenceTypesProvider(IAbsenceRepository absenceRepository, ILoggedOnUser loggedOnUser)
		{
			_absenceRepository = absenceRepository;
			_loggedOnUser = loggedOnUser;
		}

		public IEnumerable<IAbsence> GetRequestableAbsences()
		{
			return _absenceRepository.LoadRequestableAbsence();
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