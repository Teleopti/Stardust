namespace Teleopti.Interfaces.Domain
{
	public interface ICheckingPersonalAccountDaysProvider
	{
		DateOnlyPeriod GetDays(IAbsence absence, IPerson person, DateTimePeriod period);
	}
}
