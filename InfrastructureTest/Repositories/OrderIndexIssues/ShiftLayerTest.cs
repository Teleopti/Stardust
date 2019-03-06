using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Repositories.Audit;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.OrderIndexIssues
{
	[Ignore("Same strack trace as the old order index bug - related to #81337")]
	public class ShiftLayerTest : AuditTest //audittest is not needed but easier setup that way
	{
		[Test]
		public void MakesNormalSchedulesGoBanana()
		{
			using (var uow =UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddActivity(PersonAssignment.ShiftLayers.First().Payload, new TimePeriod(10, 12));
				//uown känner inte till assignment, bara lager. När händer detta?
				PersonAssignment.ShiftLayers.ForEach(x => uow.FetchSession().Merge<ShiftLayer>(x));
				///////////////////////////////////////////
				uow.PersistAll();
			}
			
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repo = new PersonAssignmentRepository(CurrentUnitOfWork.Make(), CurrentBusinessUnit.Make(), new Lazy<IUpdatedBy>(UpdatedBy.Make));
				Assert.DoesNotThrow(() =>
				{
					repo.Get(PersonAssignment.Id.Value).ShiftLayers.Count();
				});
			}
		}
	}
}