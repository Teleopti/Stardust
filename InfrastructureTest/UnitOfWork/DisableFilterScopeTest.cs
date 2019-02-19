using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	[Category("BucketB")]
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
			var personRep = new PersonRepository(new ThisUnitOfWork(UnitOfWork), null, null);
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
			var personRep = new PersonRepository(new ThisUnitOfWork(UnitOfWork), null, null);
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

		[Test]
		public void ShouldDoNothingIfNoFilterFromBeginningWithCurrentUnitOfWork()
		{
			Session.DisableFilter(QueryFilter.Deleted.Name);

			var personRep = new PersonRepository(new ThisUnitOfWork(UnitOfWork), null, null);
			personRep.LoadAll().Should().Contain(deletedPerson);
			using (UnitOfWorkFactory.Current.CurrentUnitOfWork().DisableFilter(QueryFilter.Deleted))
			{
				personRep.LoadAll().Should().Contain(deletedPerson);
			}
			personRep.LoadAll().Should().Contain(deletedPerson);
		}

		[Test]
		public void ShouldDoNothingIfNoFilterFromBeginning()
		{
			Session.DisableFilter(QueryFilter.Deleted.Name);

			var personRep = new PersonRepository(new ThisUnitOfWork(UnitOfWork), null, null);
			personRep.LoadAll().Should().Contain(deletedPerson);
			using (UnitOfWork.DisableFilter(QueryFilter.Deleted))
			{
				personRep.LoadAll().Should().Contain(deletedPerson);
			}
			personRep.LoadAll().Should().Contain(deletedPerson);
		}
	}
}