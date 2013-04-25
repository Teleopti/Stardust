using System;
using System.Reflection;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Infrastructure;

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
			res.Should().Contain(new Tuple<Guid, TimeZoneInfo>(p1.Id.Value, p1.PermissionInformation.DefaultTimeZone()));
			res.Should().Contain(new Tuple<Guid, TimeZoneInfo>(p2.Id.Value, p2.PermissionInformation.DefaultTimeZone()));
		}

		protected override void TeardownForRepositoryTest()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var s = fetchSession(uow);
				s.CreateQuery("update Person set IsDeleted=1").ExecuteUpdate();
				uow.PersistAll();
			}
		}

		private static ISession fetchSession(IUnitOfWork uow)
		{
			return (ISession)typeof(NHibernateUnitOfWork).GetProperty("Session", BindingFlags.Instance | BindingFlags.NonPublic)
															.GetValue(uow, null);
		}
	}
}