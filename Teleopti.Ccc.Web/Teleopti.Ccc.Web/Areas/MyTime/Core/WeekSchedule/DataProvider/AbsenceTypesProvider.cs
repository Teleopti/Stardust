using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

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
			return _loggedOnUser.CurrentUser().WorkflowControlSet.AllowedReportAbsences;
		}
	}
}