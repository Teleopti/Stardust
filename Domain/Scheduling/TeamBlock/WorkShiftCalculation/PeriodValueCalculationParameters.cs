using Teleopti.Ccc.Secrets.WorkShiftCalculator;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public class PeriodValueCalculationParameters
	{
		public PeriodValueCalculationParameters(WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons, bool useMaximumPersons)
		{
			LengthFactor = lengthFactor;
			UseMinimumPersons = useMinimumPersons;
			UseMaximumPersons = useMaximumPersons;
		}

		public WorkShiftLengthHintOption LengthFactor { get; }
		public bool UseMinimumPersons { get; }
		public bool UseMaximumPersons { get; }
	}
}
