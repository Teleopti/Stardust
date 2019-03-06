using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.OrderIndexIssues
{
	[Ignore("Same strack trace as the old order index bug - related to #81337")]
	[DatabaseTest]
	public class ShiftLayerTest
	{
		public IPersonAssignmentRepository PersonAssignmentRepository;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		
		[Test]
		public void MakesNormalSchedulesGoBanana()
		{
			var agent = new Person().InTimeZone(TimeZoneInfo.Utc);
			var scenario = new Scenario();
			var activity = new Activity("_");
			var personAssignment = new PersonAssignment(agent, scenario, new DateOnly(2000, 1, 1));
			
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var session = uow.FetchSession();
				session.Save(agent);
				session.Save(scenario);
				session.Save(activity);
				session.Save(personAssignment);
				
				uow.PersistAll();
			}
			
			using (var uow =UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				personAssignment.AddActivity(activity, new TimePeriod(10, 12));
				//uown känner inte till assignment, bara lager. När händer detta?
				personAssignment.ShiftLayers.ForEach(x => uow.FetchSession().Merge(x));
				///////////////////////////////////////////
				uow.PersistAll();
			}
			
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Assert.DoesNotThrow(() =>
				{
					PersonAssignmentRepository.Get(personAssignment.Id.Value).ShiftLayers.Count();
				});
			}
		}
	}
}