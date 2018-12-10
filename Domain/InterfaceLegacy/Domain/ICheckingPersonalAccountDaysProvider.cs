namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ICheckingPersonalAccountDaysProvider
	{
		DateOnlyPeriod GetDays(IAbsence absence, IPerson person, DateTimePeriod period);
	}
}
