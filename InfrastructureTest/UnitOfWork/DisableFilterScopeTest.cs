using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	[Category("LongRunning")]
	public class DisableFilterScopeTest : DatabaseTest
	{
		private IPerson deletedPerson;

		protected override void SetupForRepositoryTest()
		{
			deletedPerson = new Person();
			deletedPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Local);
			((IDeleteTag)deletedPerson).SetDeleted();
			PersistAndRemoveFromUnitOfWork(deletedPerson);
		}

		[Test]
		public void ShouldNotEnableFilterWhenNested()
		{
			var personRep = new PersonRepository(new ThisUnitOfWork(UnitOfWork));
			personRep.LoadAll().Should().Not.Contain(deletedPerson);
			using (UnitOfWork.DisableFilter(QueryFilter.Deleted))
			{
				personRep.LoadAll().Should().Contain(deletedPerson);
				using (UnitOfWork.DisableFilter(QueryFilter.Deleted))
				{
					personRep.LoadAll().Should().Contain(deletedPerson);					
				}
				personRep.LoadAll().Should().Contain(deletedPerson);
			}
			personRep.LoadAll().Should().Not.Contain(deletedPerson);
		}

		[Test]
		public void ShouldWorkWithCurrentUnitOfWork()
		{
			var personRep = new PersonRepository(new ThisUnitOfWork(UnitOfWork));
			personRep.LoadAll().Should().Not.Contain(deletedPerson);
			using (UnitOfWorkFactory.Current.CurrentUnitOfWork().DisableFilter(QueryFilter.Deleted))
			{
				personRep.LoadAll().Should().Contain(deletedPerson);
				using (UnitOfWorkFactory.Current.CurrentUnitOfWork().DisableFilter(QueryFilter.Deleted))
				{
					personRep.LoadAll().Should().Contain(deletedPerson);
				}
				personRep.LoadAll().Should().Contain(deletedPerson);
			}
			personRep.LoadAll().Should().Not.Contain(deletedPerson);
		}
	}
}