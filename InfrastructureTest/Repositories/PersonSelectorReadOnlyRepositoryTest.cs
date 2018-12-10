using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("BucketB")]
	[DatabaseTest]
	public class PersonSelectorReadOnlyRepositoryTest: IIsolateSystem
	{
		private PersonSelectorReadOnlyRepository _target;
		public WithUnitOfWork WithUnitOfWork;
		public ISiteRepository SitesRepository;
		public ITeamRepository TeamsRepository;
		public IPersonRepository PersonsRepository;
		public IContractRepository ContractsRepository;
		public IContractScheduleRepository ContractSchedulesRepository;
		public IPartTimePercentageRepository PartTimePercentagesRepository;
		public CurrentBusinessUnit CurrentBU;
		public IBusinessUnitRepository BusinessUnitRepository;
		public ICurrentUnitOfWork CurrentUnitOfWork;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<CurrentBusinessUnit>().For<ICurrentBusinessUnit>();
		}

		[Test]
		public void ShouldLoadGroupPages()
		{
			WithUnitOfWork.Get(() =>
			{
				_target = new PersonSelectorReadOnlyRepository(CurrentUnitOfWork);
				return _target.GetUserDefinedTabs();
			});
	
		}

		[Test]
		public void ShouldLoadOrganization()
		{
			WithUnitOfWork.Get(() =>
			{
				_target = new PersonSelectorReadOnlyRepository(CurrentUnitOfWork);
				var date = new DateOnly(2012, 1, 27);
				return _target.GetOrganization(new DateOnlyPeriod(date, date), true);
			});
		}

		[Test]
		public void ShouldLoadBuiltIn()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_target = new PersonSelectorReadOnlyRepository(CurrentUnitOfWork);
				var date = new DateOnly(2012, 1, 27);
				_target.GetBuiltIn(new DateOnlyPeriod(date, date), PersonSelectorField.Contract, Guid.Empty);
			}
		}

		[Test]
		public void ShouldLoadUserTabs()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_target = new PersonSelectorReadOnlyRepository(CurrentUnitOfWork);
				_target.GetUserDefinedTab(new DateOnly(2012, 1, 27), Guid.NewGuid());
			}
		}

		[Test]
		public void ShouldLoadOptionalColumnTabs()
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				_target = new PersonSelectorReadOnlyRepository(CurrentUnitOfWork);
				_target.GetOptionalColumnTabs();
			}
		}
	}
}