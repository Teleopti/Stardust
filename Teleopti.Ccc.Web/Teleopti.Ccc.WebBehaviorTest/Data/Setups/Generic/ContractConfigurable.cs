using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.WebBehaviorTest.Bindings;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class ContractConfigurable : IDataSetup
	{
		public string Name { get; set; }
		public EmploymentType EmploymentType { get; set; }
		public int PositiveDayOffTolerance { get; set; }
		public int NegativeDayOffTolerance { get; set; }
		public string PositiveTargetTolerance { get; set; }
		public string NegativeTargetTolerance { get; set; }
		public string AverageWorkTimePerDay { get; set; }

		public ContractConfigurable()
		{
			EmploymentType = EmploymentType.FixedStaffNormalWorkTime;
			AverageWorkTimePerDay = WorkTime.DefaultWorkTime.AvgWorkTimePerDay.Hours + ":00";
		}

		public void Apply(IUnitOfWork uow)
		{
			var contract = ContractFactory.CreateContract(Name);
			contract.EmploymentType = EmploymentType;
			contract.WorkTime = new WorkTime(Transform.ToTimeSpan(AverageWorkTimePerDay));
			contract.PositiveDayOffTolerance = PositiveDayOffTolerance;
			contract.NegativeDayOffTolerance = NegativeDayOffTolerance;
			if (PositiveTargetTolerance != null)
				contract.PositivePeriodWorkTimeTolerance = Transform.ToTimeSpan(PositiveTargetTolerance);
			if (NegativeTargetTolerance != null)
				contract.NegativePeriodWorkTimeTolerance = Transform.ToTimeSpan(NegativeTargetTolerance);
			new ContractRepository(uow).Add(contract);
		}
	}
}