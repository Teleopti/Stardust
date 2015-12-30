using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.AbsenceHandler
{
	public class AbsencePersister :IAbsencePersister
	{
		private readonly IAbsenceCommandConverter _absenceCommandConverter;
		private readonly IPersonAbsenceCreator _personAbsenceCreator;

		public AbsencePersister(IAbsenceCommandConverter absenceCommandConverter, IPersonAbsenceCreator personAbsenceCreator)
		{
			_absenceCommandConverter = absenceCommandConverter;
			_personAbsenceCreator = personAbsenceCreator;
		}

		public FailActionResult PersistFullDayAbsence(AddFullDayAbsenceCommand command)
		{
			var absenceCreatorInfo = _absenceCommandConverter.GetCreatorInfoForFullDayAbsence(command);
			var createResult = _personAbsenceCreator.Create(absenceCreatorInfo, true);
			if (createResult != null)
			{
				return new FailActionResult()
				{
					PersonName = absenceCreatorInfo.Person.Name.ToString(),
					Message = createResult
				};
			}
			return null;
		}

		public FailActionResult PersistIntradayAbsence(AddIntradayAbsenceCommand command)
		{
			var absenceCreatorInfo = _absenceCommandConverter.GetCreatorInfoForIntradayAbsence(command);
			var createResult = _personAbsenceCreator.Create(absenceCreatorInfo, false);
			if (createResult != null)
			{
				return  new FailActionResult()
				{
					PersonName = absenceCreatorInfo.Person.Name.ToString(),
					Message = createResult
				};
			}
			return null;
		}
	}
}