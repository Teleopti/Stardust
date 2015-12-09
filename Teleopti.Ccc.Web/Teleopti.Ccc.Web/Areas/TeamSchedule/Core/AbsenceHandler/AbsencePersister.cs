using System.Collections.Generic;
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

		public AddAbsenceFailResult PersistFullDayAbsence(AddFullDayAbsenceCommand command)
		{
			var absenceCreatorInfo = _absenceCommandConverter.GetCreatorInfoForFullDayAbsence(command);
			var createResult = _personAbsenceCreator.Create(absenceCreatorInfo, true, true) as IList<string>;
			if (createResult != null)
			{
				return new AddAbsenceFailResult()
				{
					PersonName = absenceCreatorInfo.Person.Name.ToString(),
					Message = createResult
				};
			}
			return null;
		}

		public AddAbsenceFailResult PersistIntradayAbsence(AddIntradayAbsenceCommand command)
		{
			var absenceCreatorInfo = _absenceCommandConverter.GetCreatorInfoForIntradayAbsence(command);
			var createResult = _personAbsenceCreator.Create(absenceCreatorInfo, false, true) as IList<string>;
			if (createResult != null)
			{
				return  new AddAbsenceFailResult()
				{
					PersonName = absenceCreatorInfo.Person.Name.ToString(),
					Message = createResult
				};
			}
			return null;
		}
	}
}