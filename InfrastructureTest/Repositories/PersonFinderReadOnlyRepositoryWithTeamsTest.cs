using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	
	[TestFixture, Category("BucketB")]
	[DatabaseTest]
	public class PersonFinderReadOnlyRepositoryWithTeamsTest
	{
		public IPersonFinderReadOnlyRepository Target;
		public ISiteRepository Sites;
		public ITeamRepository Teams;
		public IPersonRepository Persons;
		public IContractRepository Contracts;
		public IContractScheduleRepository ContractSchedules;
		public IPartTimePercentageRepository PartTimePercentages;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldBatchQueryForTooManyTeams()
		{
			ISite site = SiteFactory.CreateSimpleSite("d");
			ITeam team = TeamFactory.CreateSimpleTeam("Team");
			team.Site = site;
			team.SetDescription(new Description("sdf"));
			WithUnitOfWork.Do(() =>
			{
				Sites.Add(site);
				Teams.Add(team);
			});

			IPerson per = PersonFactory.CreatePerson("Ashley", "Ardeen");

			per.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));

			WithUnitOfWork.Do(() =>
			{
				Persons.Add(per);
			});

			createAndSaveReadModel(per.Id.Value, per.Name.FirstName, per.Name.LastName, team.Id.Value, site.Id.Value, site.BusinessUnit.Id.Value, new DateTime(2011, 1, 1));

			var crit = new PersonFinderSearchCriteria(new Dictionary<PersonFinderField, string>(), 10,
				new DateOnly(2020, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));

			var largeTeams = new List<Guid> { team.Id.Value };
			largeTeams.AddRange(Enumerable.Range(0, 8000).Select(i => Guid.NewGuid()).ToArray());

			var result = WithUnitOfWork.Get(() =>
			{
				Target.FindInTeams(crit, largeTeams.ToArray());
				return crit.TotalRows;
			});

			Assert.That(result, Is.EqualTo(1));
		}

		[Test]
		public void ShouldMatchAllValuesInGivenTeamsWithEmptyCriteria()
		{
			ISite site = SiteFactory.CreateSimpleSite("d");
			ITeam team = TeamFactory.CreateSimpleTeam("Team");
			team.Site = site;
			team.SetDescription(new Description("sdf"));
			WithUnitOfWork.Do(() =>
			{
				Sites.Add(site);
				Teams.Add(team);
			});

			IPerson per = PersonFactory.CreatePerson("Ashley", "Ardeen");

			per.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));

			WithUnitOfWork.Do(() =>
			{
				Persons.Add(per);
			});

			createAndSaveReadModel(per.Id.Value, per.Name.FirstName, per.Name.LastName, team.Id.Value, site.Id.Value, site.BusinessUnit.Id.Value, new DateTime(2011, 1, 1));

			var crit = new PersonFinderSearchCriteria(new Dictionary<PersonFinderField, string>(), 10,
				new DateOnly(2020, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));

			var result = WithUnitOfWork.Get(() =>
			{
				Target.FindInTeams(crit, new[] { team.Id.Value });
				return crit.TotalRows;
			});


			Assert.That(result, Is.EqualTo(1));
		}

		[Test]
		public void ShouldMatchAllValuesInGivenTeamsWithGivenCriteria()
		{
			ISite site = SiteFactory.CreateSimpleSite("d");
			ITeam team = TeamFactory.CreateSimpleTeam("Team");
			team.Site = site;
			team.SetDescription(new Description("sdf"));
			WithUnitOfWork.Do(() =>
			{
				Sites.Add(site);
				Teams.Add(team);
			});

			IPerson per = PersonFactory.CreatePerson("Ashley", "Ardeen");

			per.AddPersonPeriod(new PersonPeriod(new DateOnly(2011, 1, 1), createPersonContract(), team));

			WithUnitOfWork.Do(() =>
			{
				Persons.Add(per);
			});

			createAndSaveReadModel(per.Id.Value, per.Name.FirstName, per.Name.LastName, team.Id.Value, site.Id.Value, site.BusinessUnit.Id.Value, new DateTime(2011, 1, 1));

			var crit = new PersonFinderSearchCriteria(PersonFinderField.All, "Ashley", 10,
				new DateOnly(2020, 1, 1), new Dictionary<string, bool>(), new DateOnly(2011, 12, 1));

			var result = WithUnitOfWork.Get(() =>
			{
				Target.FindInTeams(crit, new[] { team.Id.Value });
				return crit.TotalRows;
			});


			Assert.That(result, Is.EqualTo(1));
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
				Contracts.Add(pContract.Contract);
				ContractSchedules.Add(pContract.ContractSchedule);
				PartTimePercentages.Add(pContract.PartTimePercentage);
			});

			return pContract;
		}

		private void createAndSaveReadModel(Guid personId, string firstName, string lastName, Guid teamId, Guid siteId, Guid businessUnitId, DateTime startDateTime)
		{

			WithUnitOfWork.Do(uow =>
			{
				uow.Current().FetchSession().CreateSQLQuery(
			  "Insert into [ReadModel].[FindPerson] (PersonId,  FirstName,  LastName,  EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId                        ,SearchType        , TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
			  " Values (                            :personId, :firstName, :lastName,   '137545'           ,''  ,NULL ,  :searchValue   ,'11610FE4-0130-4568-97DE-9B5E015B2564','FirstName',:teamId, :siteId,:businessUnitId, :startDateTime, NULL)")
			  .SetGuid("personId", personId)
			  .SetString("firstName", firstName)
			  .SetString("lastName", lastName)
			  .SetString("searchValue", firstName)
			  .SetDateTime("startDateTime", startDateTime)
			  .SetGuid("businessUnitId", businessUnitId)
			  .SetGuid("teamId", teamId)
			  .SetGuid("siteId", siteId)
			  .ExecuteUpdate();
			});
		}


	}
}