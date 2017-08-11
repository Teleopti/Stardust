using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("BucketB")]
	[DatabaseTest]
	public class PersonFinderReadOnlyRepositoryWithGroupsTest
	{
		public IGroupingReadOnlyRepository GroupingReadonly;
		public IPersonFinderReadOnlyRepository Target;
		public WithUnitOfWork WithUnitOfWork;
		public IPersonRepository PersonRepository;
		public ITeamRepository TeamRepository;
		public ISiteRepository SiteRepository;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IActivityRepository ActivityRepository;
		public IContractRepository ContractRepository;
		public IContractScheduleRepository ContractScheduleRepository;
		public IPartTimePercentageRepository PartTimePercentageRepository;

		[Test]
		public void ShouldMatchAllValuesInGivenGroupsWithGivenCriteria()
		{
			var scheduleDate = new DateOnly(2000, 1, 1);
			var personToTest = PersonFactory.CreatePerson("dummyAgent1");

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(scheduleDate,
												personContract,
												team);
			personToTest.AddPersonPeriod(personPeriod);

			WithUnitOfWork.Do(() =>
			{
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				ContractRepository.Add(personContract.Contract);
				ContractScheduleRepository.Add(personContract.ContractSchedule);
				PartTimePercentageRepository.Add(personContract.PartTimePercentage);
				PersonRepository.Add(personToTest);
			});
			WithUnitOfWork.Do(() =>
			{
				GroupingReadonly.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });
			});

			createAndSaveReadModel(personToTest.Id.Value, personToTest.Name.FirstName, personToTest.Name.LastName, team.Id.Value, team.Site.Id.Value, team.Site.BusinessUnit.Id.Value, scheduleDate.Date);
			WithUnitOfWork.Do(() =>
			{
				var result = Target.FindPersonIdsInGroupsBasedOnPersonPeriod(new DateOnlyPeriod(scheduleDate, scheduleDate), new[] { personContract.Contract.Id.Value },
				new Dictionary<PersonFinderField, string> {
					{ PersonFinderField.FirstName, "dummyAgent1" }
				});

				result.Count.Should().Be.EqualTo(1);
				result.Single().Should().Be.EqualTo(personToTest.Id.Value);
			});
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
