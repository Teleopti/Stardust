using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
	public class SkillStaffPeriodFake : IValidatePeriod
	{
		public SkillStaffPeriodFake(double relativeDifference, DateTimePeriod dateTimePeriod = new DateTimePeriod())
		{
			RelativeDifference = relativeDifference;
			DateTimePeriod = dateTimePeriod;
		}
		
		public double RelativeDifference { get; }
		public DateTimePeriod DateTimePeriod { get; }
	}
}