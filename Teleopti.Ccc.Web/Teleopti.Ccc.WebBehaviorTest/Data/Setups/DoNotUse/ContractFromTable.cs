using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class ContractFromTable : IDataSetup
	{
		public EmploymentType EmploymentType { get; set; }
		public int PositiveDayOffTolerance { get; set; }
		public int NegativeDayOffTolerance { get; set; }
		public int PositiveTargetToleranceHours { get; set; }
		public int NegativeTargetToleranceHours { get; set; }
		public int AverageWorkTimePerDay { get; set; }
		public int MaxHoursPerWeek { get; set; }
		public int MinHoursPerWeek { get; set; }

		public IContract Contract { get; set; }

		public ContractFromTable()
		{
			EmploymentType = EmploymentType.FixedStaffNormalWorkTime;
			AverageWorkTimePerDay = WorkTime.DefaultWorkTime.AvgWorkTimePerDay.Hours;
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var workTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(MinHoursPerWeek), TimeSpan.FromHours(MaxHoursPerWeek), new TimeSpan(), new TimeSpan());
			Contract = new Contract("Contract from table")
				{
					EmploymentType = EmploymentType,
					WorkTime = new WorkTime(TimeSpan.FromHours(AverageWorkTimePerDay)),
					PositiveDayOffTolerance = PositiveDayOffTolerance,
					NegativeDayOffTolerance = NegativeDayOffTolerance,
					PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(PositiveTargetToleranceHours),
					NegativePeriodWorkTimeTolerance = TimeSpan.FromHours(NegativeTargetToleranceHours),
					WorkTimeDirective = workTimeDirective
				};
			ContractRepository.DONT_USE_CTOR(currentUnitOfWork, null, null).Add(Contract);
		}
	}
}