using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface IAbsenceCommandConverter
	{
		AbsenceCreatorInfo GetCreatorInfoForFullDayAbsence(AddFullDayAbsenceCommand command);
		AbsenceCreatorInfo GetCreatorInfoForIntradayAbsence(AddIntradayAbsenceCommand command);
		DateTimePeriod GetFullDayAbsencePeriod(IPerson person, DateTime start, DateTime end);
	}
}
