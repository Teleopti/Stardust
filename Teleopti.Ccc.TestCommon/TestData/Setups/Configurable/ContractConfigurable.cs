using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
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

		public IContract Contract;

		public ContractConfigurable()
		{
			EmploymentType = EmploymentType.FixedStaffNormalWorkTime;
			AverageWorkTimePerDay = WorkTime.DefaultWorkTime.AvgWorkTimePerDay.Hours + ":00";
		}

		public void Apply(IUnitOfWork uow)
		{
			if (Name == null)
			{
				Name = DefaultName.Make();
			}
			Contract = new Contract(Name)
			{
				EmploymentType = EmploymentType,
				WorkTime = new WorkTime(TimeSpan.Parse(AverageWorkTimePerDay)),
				PositiveDayOffTolerance = PositiveDayOffTolerance,
				NegativeDayOffTolerance = NegativeDayOffTolerance
			};
			if (PositiveTargetTolerance != null)
				Contract.PositivePeriodWorkTimeTolerance = TimeSpan.Parse(PositiveTargetTolerance);
			if (NegativeTargetTolerance != null)
				Contract.NegativePeriodWorkTimeTolerance = TimeSpan.Parse(NegativeTargetTolerance);
			new ContractRepository(uow).Add(Contract);
		}
	}
}