using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
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

		public void Apply(IUnitOfWork uow)
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
			new ContractRepository(uow).Add(Contract);
		}
	}
}