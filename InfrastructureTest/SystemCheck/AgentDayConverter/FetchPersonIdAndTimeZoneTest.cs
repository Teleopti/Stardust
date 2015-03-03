using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	public class FetchPersonIdAndTimeZoneTest : DatabaseTestWithoutTransaction
	{
		[Test]
		public void ShouldFetchPersons()
		{
			var p1 = new Person();
			p1.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
			PersistAndRemoveFromUnitOfWork(p1);
			var p2 = new Person();
			p2.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PersistAndRemoveFromUnitOfWork(p2);
			UnitOfWork.PersistAll();

			var res = new FetchPersonIdAndTimeZone(UnitOfWorkFactory.Current.ConnectionString).ForAllPersons();
			res.First(x => x.Item1 == p1.Id.Value).Item2.Id
				.Should().Be.EqualTo(p1.PermissionInformation.DefaultTimeZone().Id);
			res.First(x => x.Item1 == p2.Id.Value).Item2.Id
				.Should().Be.EqualTo(p2.PermissionInformation.DefaultTimeZone().Id);
		}

	}
}