using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("BucketB")]
	[DatabaseTest]
	public class PersonSelectorReadOnlyRepositoryTest
	{
		private PersonSelectorReadOnlyRepository _target;

		[Test]
		public void ShouldLoadGroupPages()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_target = new PersonSelectorReadOnlyRepository(UnitOfWorkFactory.Current);
				_target.GetUserDefinedTabs();
			}
		}

		[Test]
		public void ShouldLoadOrganization()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_target = new PersonSelectorReadOnlyRepository(UnitOfWorkFactory.Current);
				var date = new DateOnly(2012, 1, 27);
				_target.GetOrganization(new DateOnlyPeriod(date,date), true );
			}
		}

		[Test]
		public void ShouldLoadBuiltIn()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_target = new PersonSelectorReadOnlyRepository(UnitOfWorkFactory.Current);
				var date = new DateOnly(2012, 1, 27);
				_target.GetBuiltIn(new DateOnlyPeriod(date,date), PersonSelectorField.Contract, Guid.Empty);
			}
		}

		[Test]
		public void ShouldLoadUserTabs()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_target = new PersonSelectorReadOnlyRepository(UnitOfWorkFactory.Current);
				_target.GetUserDefinedTab(new DateOnly(2012, 1, 27), Guid.NewGuid());
			}
		}

		[Test]
		public void ShouldLoadOptionalColumnTabs()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_target = new PersonSelectorReadOnlyRepository(UnitOfWorkFactory.Current);
				_target.GetOptionalColumnTabs();
			}
		}
	}
}