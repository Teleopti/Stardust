namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAlreadyAbsentValidator
	{
		bool Validate(IAbsenceRequest absenceRequest, IScheduleRange scheduleRange);
	}
}