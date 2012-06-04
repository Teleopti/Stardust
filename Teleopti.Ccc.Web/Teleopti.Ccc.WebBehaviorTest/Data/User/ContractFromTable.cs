using System;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class ContractFromTable : IDataSetup, IContractSetup
	{
		public EmploymentType EmploymentType { get; set; }
		public int PositiveDayOffTolerance { get; set; }
		public int NegativeDayOffTolerance { get; set; }
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
			new ContractRepository(uow).Add(Contract);
		}
	}
}