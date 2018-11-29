using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    [InfrastructureTest]
    public class PurgeApplicationDataTest
	{
		public ICurrentUnitOfWorkFactory UnitOfWorkFactory;
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public IPersonRepository PersonRepository;
		public IActivityRepository ActivityRepository;
		public IScenarioRepository ScenarioRepository;
		
		[Test]
		public void ShouldDeleteOldAssignments()
		{
			var longTimeAgo = new DateOnly(1800, 1, 1);
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var ass = new PersonAssignment(agent, scenario, longTimeAgo)
				.WithLayer(activity, new TimePeriod(8, 17));

			using (var uow = UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				ScenarioRepository.Add(scenario);
				PersonRepository.Add(agent);
				ActivityRepository.Add(activity);
				PersonAssignmentRepository.Add(ass);
				uow.PersistAll();
			}
			using (UnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var target = new PurgeApplicationData(UnitOfWorkFactory.Current());
				target.Execute();

				PersonAssignmentRepository.LoadAll()
					.Should().Be.Empty();
			}
		}
    }
}
