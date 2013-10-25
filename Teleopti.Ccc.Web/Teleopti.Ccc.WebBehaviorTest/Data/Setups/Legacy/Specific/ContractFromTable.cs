using System;
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

		public IContract Contract { get; set; }

		public ContractFromTable()
		{
			EmploymentType = EmploymentType.FixedStaffNormalWorkTime;
			AverageWorkTimePerDay = WorkTime.DefaultWorkTime.AvgWorkTimePerDay.Hours;
		}

		public void Apply(IUnitOfWork uow)
		{
			Contract = ContractFactory.CreateContract("Contract from table");
			Contract.EmploymentType = EmploymentType;
			Contract.WorkTime = new WorkTime(TimeSpan.FromHours(AverageWorkTimePerDay));
			Contract.PositiveDayOffTolerance = PositiveDayOffTolerance;
			Contract.NegativeDayOffTolerance = NegativeDayOffTolerance;
			Contract.PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(PositiveTargetToleranceHours);
			Contract.NegativePeriodWorkTimeTolerance = TimeSpan.FromHours(NegativeTargetToleranceHours);
			new ContractRepository(uow).Add(Contract);
		}
	}
}