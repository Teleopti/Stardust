using System;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class PersonAssignmentRepositoryWithRandomLatency : PersonAssignmentRepository
	{
		private readonly Random _random = new Random();

		public PersonAssignmentRepositoryWithRandomLatency(IUnitOfWork unitOfWork) : base(unitOfWork)
		{
		}

		public PersonAssignmentRepositoryWithRandomLatency(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public override ICollection<IPersonAssignment> Find(IEnumerable<IPerson> persons, DateOnlyPeriod period, IScenario scenario)
		{
			var personAssignments = base.Find(persons, period, scenario);
			Thread.Sleep(_random.Next(0, 200));
			return personAssignments;
		}

		public override DateTime GetScheduleLoadedTime()
		{
			Thread.Sleep(_random.Next(0, 200));
			return base.GetScheduleLoadedTime();
		}
	}
}