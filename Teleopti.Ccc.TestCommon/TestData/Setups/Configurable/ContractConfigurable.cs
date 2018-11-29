using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;


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
		public string MultiplicatorDefinitionSet { get; set; }

		public IContract Contract;

		public ContractConfigurable()
		{
			EmploymentType = EmploymentType.FixedStaffNormalWorkTime;
			AverageWorkTimePerDay = WorkTime.DefaultWorkTime.AvgWorkTimePerDay.Hours + ":00";
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			if (Name == null)
			{
				Name = RandomName.Make();
			}
			Contract = new Contract(Name)
			{
				EmploymentType = EmploymentType,
				WorkTime = new WorkTime(TimeSpan.Parse(AverageWorkTimePerDay)),
				PositiveDayOffTolerance = PositiveDayOffTolerance,
				NegativeDayOffTolerance = NegativeDayOffTolerance,
				WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.FromHours(60), TimeSpan.FromHours(11), TimeSpan.Zero)
			};
			if (PositiveTargetTolerance != null)
				Contract.PositivePeriodWorkTimeTolerance = TimeSpan.Parse(PositiveTargetTolerance);
			if (NegativeTargetTolerance != null)
				Contract.NegativePeriodWorkTimeTolerance = TimeSpan.Parse(NegativeTargetTolerance);
			if (MultiplicatorDefinitionSet != null)
			{
				var multiplicatorDefinitionSet = new MultiplicatorDefinitionSetRepository(currentUnitOfWork).LoadAll().FirstOrDefault(m => m.Name == MultiplicatorDefinitionSet);
				Contract.AddMultiplicatorDefinitionSetCollection(multiplicatorDefinitionSet);
			}

			new ContractRepository(currentUnitOfWork).Add(Contract);
		}
	}
}