using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider
{
	public class AbsenceTypesProvider : IAbsenceTypesProvider
	{
		private readonly IAbsenceRepository _absenceRepository;

		public AbsenceTypesProvider(IAbsenceRepository absenceRepository)
		{
			_absenceRepository = absenceRepository;
		}

		public IEnumerable<IAbsence> GetRequestableAbsences()
		{
			return _absenceRepository.LoadRequestableAbsence();
		}
	}
}