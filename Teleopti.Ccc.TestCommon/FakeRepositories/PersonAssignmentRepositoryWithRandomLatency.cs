using System;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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