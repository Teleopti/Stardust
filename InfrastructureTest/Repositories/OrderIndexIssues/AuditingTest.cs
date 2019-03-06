using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Repositories.Audit;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.OrderIndexIssues
{
	[Ignore("Same strack trace as #81337")]
	public class AuditingTest : AuditTest
	{
		[Test]
		public void MakesAuditingGoBanana()
		{
			var target = new ScheduleAuditTrailReport(new CurrentUnitOfWork(CurrentUnitOfWorkFactory.Make()), new FakeUserTimeZone(), new GlobalSettingDataRepository(CurrentUnitOfWork.Make()));
			
			using (var uow =UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				PersonAssignment.AddActivity(PersonAssignment.ShiftLayers.First().Payload, new TimePeriod(10, 12));
				//uown känner inte till assignment, bara lager. När händer detta?
				PersonAssignment.ShiftLayers.ForEach(x => uow.FetchSession().Merge(x));
				//////////////////////////////////
				uow.PersistAll();
			}
			
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				Assert.DoesNotThrow(() =>
				{
					target.Report(null,
						new DateOnlyPeriod(new DateOnly(Today), new DateOnly(Today).AddDays(1)),
						PersonAssignment.Period.ToDateOnlyPeriod(TimeZoneInfo.Utc), 100, new List<IPerson> {PersonAssignment.Person});
				});
			}
		}
	}
}