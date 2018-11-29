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
		public void ShouldGetTeamsAndSitesExcludingUnusedUnderCurrentBu()
		{
			var bu = BusinessUnitFactory.CreateSimpleBusinessUnit("bu");
			WithUnitOfWork.Do(() =>
			{
				BusinessUnitRepository.Add(bu);
			});
			CurrentBU.OnThisThreadUse(bu);

			ISite site = SiteFactory.CreateSimpleSite("d");
			ITeam team = TeamFactory.CreateSimpleTeam("Team");
			bu.AddSite(site);
			team.Site = site;
			team.SetDescription(new Description("sdf"));
			WithUnitOfWork.Do(() =>
			{
				SitesRepository.Add(site);
				TeamsRepository.Add(team);
			});
			IPerson per = PersonFactory.CreatePerson("Ashley", "Ardeen");
			per.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));

			WithUnitOfWork.Do(() =>
			{
				PersonsRepository.Add(per);
			});

			var bu1 = BusinessUnitFactory.CreateSimpleBusinessUnit("bu1");
			WithUnitOfWork.Do(() =>
			{
				BusinessUnitRepository.Add(bu1);
			});
			CurrentBU.OnThisThreadUse(bu1);

			ISite site1 = SiteFactory.CreateSimpleSite("d");
			ITeam team1 = TeamFactory.CreateSimpleTeam("Team");
			team1.Site = site1;
			team1.SetDescription(new Description("sdf"));
			bu1.AddSite(site1);
			WithUnitOfWork.Do(() =>
			{
				SitesRepository.Add(site1);
				TeamsRepository.Add(team1);
			});
			IPerson per1 = PersonFactory.CreatePerson("Ashley", "Ardeen");
			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2010, 1, 1), createPersonContract(), team1));
			WithUnitOfWork.Do(() =>
			{
				PersonsRepository.Add(per1);
			});

			CurrentBU.OnThisThreadUse(bu);
			var result = WithUnitOfWork.Get(() => new PersonSelectorReadOnlyRepository(CurrentUnitOfWork)
					.GetOrganizationForWeb(new DateOnlyPeriod(new DateOnly(2011, 1, 1), new DateOnly(2011, 1, 1)))
					.Single());
			result.TeamId.Value.Should().Equals(team.Id);
		}

		[Test]
		public void ShouldGetTeamsAndSitesExcludingUnused()
		{
			
			ISite site = SiteFactory.CreateSimpleSite("d");
			ITeam team = TeamFactory.CreateSimpleTeam("Team");
			team.Site = site;
			team.SetDescription(new Description("sdf"));
			WithUnitOfWork.Do(() =>
			{
				SitesRepository.Add(site);
				TeamsRepository.Add(team);
			});

			IPerson per = PersonFactory.CreatePerson("Ashley", "Ardeen");

			per.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));

			WithUnitOfWork.Do(() =>
			{
				PersonsRepository.Add(per);
			});

			ISite site1 = SiteFactory.CreateSimpleSite("d");
			ITeam team1 = TeamFactory.CreateSimpleTeam("Team");
			team1.Site = site1;
			team1.SetDescription(new Description("sdf"));
			WithUnitOfWork.Do(() =>
			{
				SitesRepository.Add(site1);
				TeamsRepository.Add(team1);
			});

			IPerson per1 = PersonFactory.CreatePerson("Ashley", "Ardeen");

			per1.AddPersonPeriod(new PersonPeriod(new DateOnly(2010, 1, 1), createPersonContract(), team1));

			WithUnitOfWork.Do(() =>
			{
				PersonsRepository.Add(per1);
			});

			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var result = new PersonSelectorReadOnlyRepository(CurrentUnitOfWork)
					.GetOrganizationForWeb(new DateOnlyPeriod(new DateOnly(2010, 1, 1), new DateOnly(2010, 1, 1)))
					.Single();
				result.TeamId.Value.Should().Equals(team1.Id);
			}

			
		}

		private IPersonContract createPersonContract(IBusinessUnit otherBusinessUnit = null)
		{
			var pContract = PersonContractFactory.CreatePersonContract();
			if (otherBusinessUnit != null)
			{
				pContract.Contract.SetBusinessUnit(otherBusinessUnit);
				pContract.ContractSchedule.SetBusinessUnit(otherBusinessUnit);
				pContract.PartTimePercentage.SetBusinessUnit(otherBusinessUnit);
			}
			WithUnitOfWork.Do(() =>
			{
				ContractsRepository.Add(pContract.Contract);
				ContractSchedulesRepository.Add(pContract.ContractSchedule);
				PartTimePercentagesRepository.Add(pContract.PartTimePercentage);
			});

			return pContract;
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