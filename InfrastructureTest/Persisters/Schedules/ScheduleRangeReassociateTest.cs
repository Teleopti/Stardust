using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Scenario = Teleopti.Ccc.Domain.Common.Scenario;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Schedules
{
	public class ScheduleRangeReassociateTest : DatabaseTest
	{
		[Test]
		public void ShouldReassociateScheduleData()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var agent = PersonFactory.CreatePerson();
			var pa = new PersonAssignment(agent, scenario, new DateOnly(2000,1,1));
			PersistAndRemoveFromUnitOfWork(scenario);
			PersistAndRemoveFromUnitOfWork(activity);
			PersistAndRemoveFromUnitOfWork(agent);
			PersistAndRemoveFromUnitOfWork(pa);

			var scheduleDic = new ScheduleRepository(UnitOfWork).FindSchedulesForPersonOnlyInGivenPeriod(agent, new ScheduleDictionaryLoadOptions(false, false), new DateOnlyPeriod(2000,1,1, 2001,1,1), scenario);

			UnitOfWork.Clear();
			scheduleDic[agent].Reassociate(UnitOfWork);
			UnitOfWork.FetchSession().Statistics.EntityKeys.Select(x => x.Identifier).Should().Contain(pa.Id.Value);
		}
	}
}