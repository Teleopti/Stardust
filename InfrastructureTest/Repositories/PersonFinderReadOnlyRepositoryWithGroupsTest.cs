using NUnit.Framework;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


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
		public IOptionalColumnRepository OptionalColumnRepository;

		[Test]
		public void ShouldMatchAllValuesInGivenGroupsWithGivenCriteria()
		{
			var scheduleDate = new DateOnly(2000, 1, 1);
			var personToTest = PersonFactory.CreatePerson(new Name("dummyAgent1", "dummy"));

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

			createAndSaveReadModel(PersonFinderField.FirstName, personToTest.Id.Value, personToTest.Name.FirstName, team.Id.Value, team.Site.Id.Value, team.Site.BusinessUnit.Id.Value, scheduleDate.Date);

			WithUnitOfWork.Do(() =>
			{
				var result = Target.FindPersonIdsInGroupsBasedOnPersonPeriod(new DateOnlyPeriod(scheduleDate, scheduleDate), new[] { personContract.Contract.Id.Value },
				new Dictionary<PersonFinderField, string> {
					{ PersonFinderField.FirstName, "dummyAgent1"}
				});

				result.Count.Should().Be.EqualTo(1);
				result.Single().Should().Be.EqualTo(personToTest.Id.Value);
			});

			personToTest.SetEmploymentNumber("137545");
			createAndSaveReadModel(PersonFinderField.EmploymentNumber, personToTest.Id.Value, personToTest.EmploymentNumber, team.Id.Value, team.Site.Id.Value, team.Site.BusinessUnit.Id.Value, scheduleDate.Date);

			WithUnitOfWork.Do(() =>
			{
				var result = Target.FindPersonIdsInGroupsBasedOnPersonPeriod(new DateOnlyPeriod(scheduleDate, scheduleDate), new[] { personContract.Contract.Id.Value },
				new Dictionary<PersonFinderField, string> {
					{ PersonFinderField.FirstName, "dummyAgent1"},
					{PersonFinderField.EmploymentNumber, "137545"}
				});

				result.Count.Should().Be.EqualTo(1);
			});

			createAndSaveReadModel(PersonFinderField.LastName, personToTest.Id.Value, personToTest.Name.LastName, team.Id.Value, team.Site.Id.Value, team.Site.BusinessUnit.Id.Value, scheduleDate.Date);

			WithUnitOfWork.Do(() =>
			{
				var result = Target.FindPersonIdsInGroupsBasedOnPersonPeriod(new DateOnlyPeriod(scheduleDate, scheduleDate), new[] { personContract.Contract.Id.Value },
				new Dictionary<PersonFinderField, string> {
					{ PersonFinderField.FirstName, "dummyAgent1"},
					{PersonFinderField.LastName, "dummy" }
				});

				result.Count.Should().Be.EqualTo(1);
			});

		}

		[Test]
		public void ShouldMatchAllValuesInGivenDynamicGroupWithGivenCriteria()
		{
			var scheduleDate = new DateOnly(2000, 1, 1);
			var optionColumn = new OptionalColumn("Test")
			{
				TableName = "Person",
				AvailableAsGroupPage = true
			};

			var personToTest = PersonFactory.CreatePerson(new Name("dummyAgent1", "dummy"));
			personToTest.SetOptionalColumnValue(new OptionalColumnValue("test value"), optionColumn);

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(scheduleDate,
				personContract,
				team);
			personToTest.AddPersonPeriod(personPeriod);

			WithUnitOfWork.Do(() =>
			{
				OptionalColumnRepository.Add(optionColumn);
			});
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

			createAndSaveReadModel(PersonFinderField.FirstName, personToTest.Id.Value, personToTest.Name.FirstName, team.Id.Value, team.Site.Id.Value, team.Site.BusinessUnit.Id.Value, scheduleDate.Date);

			WithUnitOfWork.Do(() =>
			{
				var result = Target.FindPersonIdsInDynamicOptionalGroupPages(new DateOnlyPeriod(scheduleDate, scheduleDate), optionColumn.Id.Value, new[] { "test value" },
					new Dictionary<PersonFinderField, string> {
						{ PersonFinderField.FirstName, "dummyAgent1"}
					});

				result.Count.Should().Be.EqualTo(1);
				result.Single().Should().Be.EqualTo(personToTest.Id.Value);
			});
		}

		[Test]
		public void ShouldNotFindPersonIdsInGivenDynamicGroupWhenGivenCriteriaIsNotMatchedInPeriod()
		{
			var scheduleDate = new DateOnly(2000, 1, 1);
			var optionColumn = new OptionalColumn("Test")
			{
				TableName = "Person",
				AvailableAsGroupPage = true
			};

			var personToTest = PersonFactory.CreatePerson(new Name("dummyAgent1", "dummy"));
			personToTest.SetOptionalColumnValue(new OptionalColumnValue("test value"), optionColumn);

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");

			var personContract = PersonContractFactory.CreatePersonContract("contract1", "contract schedule1", "1");
			var personPeriod = new PersonPeriod(scheduleDate, personContract, team);
			personToTest.AddPersonPeriod(personPeriod);

			var anotherContract = PersonContractFactory.CreatePersonContract("contract2", "contract schedule2", "1");
			var anotherPersonPeriod = new PersonPeriod(scheduleDate.AddDays(30), anotherContract, team);
			personToTest.AddPersonPeriod(anotherPersonPeriod);

			WithUnitOfWork.Do(() =>
			{
				OptionalColumnRepository.Add(optionColumn);
			});
			WithUnitOfWork.Do(() =>
			{
				SiteRepository.Add(team.Site);
				TeamRepository.Add(team);
				ContractRepository.Add(personContract.Contract);
				ContractScheduleRepository.Add(personContract.ContractSchedule);
				PartTimePercentageRepository.Add(personContract.PartTimePercentage);
				ContractRepository.Add(anotherContract.Contract);
				ContractScheduleRepository.Add(anotherContract.ContractSchedule);
				PartTimePercentageRepository.Add(anotherContract.PartTimePercentage);
				PersonRepository.Add(personToTest);
			});

			WithUnitOfWork.Do(() =>
			{
				GroupingReadonly.UpdateGroupingReadModel(new List<Guid> { Guid.Empty });
			});

			createAndSaveReadModel(PersonFinderField.Contract, personToTest.Id.Value, "contract1", team.Id.Value, team.Site.Id.Value, team.Site.BusinessUnit.Id.Value, scheduleDate.Date);
			createAndSaveReadModel(PersonFinderField.Contract, personToTest.Id.Value, "contract2", team.Id.Value, team.Site.Id.Value, team.Site.BusinessUnit.Id.Value, scheduleDate.AddDays(30).Date);

			WithUnitOfWork.Do(() =>
			{
				var result = Target.FindPersonIdsInDynamicOptionalGroupPages(new DateOnlyPeriod(scheduleDate, scheduleDate), optionColumn.Id.Value, new[] { "test value" },
				new Dictionary<PersonFinderField, string> {
						{ PersonFinderField.Contract, "contract2"}
				});

				result.Count.Should().Be.EqualTo(0);
			});
		}

		[Test]
		public void ShouldNotFindPersonIdsInGivenDynamicGroupWhenPersonIsLeft()
		{
			var scheduleDate = new DateOnly(2000, 1, 1);
			var optionColumn = new OptionalColumn("Test")
			{
				TableName = "Person",
				AvailableAsGroupPage = true
			};

			var personToTest = PersonFactory.CreatePerson(new Name("dummyAgent1", "dummy"));
			personToTest.SetOptionalColumnValue(new OptionalColumnValue("test value"), optionColumn);
			personToTest.TerminatePerson(scheduleDate.AddDays(-1), new PersonAccountUpdaterDummy());

			var team = TeamFactory.CreateTeam("Dummy Site", "Dummy Team");

			var personContract = PersonContractFactory.CreatePersonContract();
			var personPeriod = new PersonPeriod(scheduleDate,
				personContract,
				team);
			personToTest.AddPersonPeriod(personPeriod);

			WithUnitOfWork.Do(() =>
			{
				OptionalColumnRepository.Add(optionColumn);
			});
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

			createAndSaveReadModel(PersonFinderField.FirstName, personToTest.Id.Value, personToTest.Name.FirstName, team.Id.Value, team.Site.Id.Value, team.Site.BusinessUnit.Id.Value, scheduleDate.Date, scheduleDate.Date.AddDays(-1));

			WithUnitOfWork.Do(() =>
			{
				var result = Target.FindPersonIdsInDynamicOptionalGroupPages(new DateOnlyPeriod(scheduleDate, scheduleDate), optionColumn.Id.Value, new[] { "test value" },
					new Dictionary<PersonFinderField, string>());

				result.Count.Should().Be.EqualTo(0);

			});
		}
		private void createAndSaveReadModel(PersonFinderField searchType, Guid personId, string searchValue,
			 Guid teamId, Guid siteId, Guid businessUnitId, DateTime startDateTime, DateTime? terminalDate = null)
		{
			WithUnitOfWork.Do(uow =>
			{
				uow.Current().FetchSession().CreateSQLQuery(
			  "Insert into [ReadModel].[FindPerson] (PersonId,  FirstName,  LastName,  EmploymentNumber,Note,TerminalDate,SearchValue, SearchValueId                        ,SearchType        , TeamId, SiteId,BusinessUnitId, StartDateTime, EndDateTime)" +
			  " Values (                            :personId, '',          '',        '137545'        ,''  ,:terminalDate,:searchValue,'11610FE4-0130-4568-97DE-9B5E015B2564',:searchType,:teamId, :siteId,:businessUnitId, :startDateTime, NULL)")
			  .SetString("searchType", searchType.ToString())
			  .SetGuid("personId", personId)
			  .SetDateTime("terminalDate", terminalDate ?? DateTime.MaxValue)
			  .SetString("searchValue", searchValue)
			  .SetDateTime("startDateTime", startDateTime)
			  .SetGuid("businessUnitId", businessUnitId)
			  .SetGuid("teamId", teamId)
			  .SetGuid("siteId", siteId)
			  .ExecuteUpdate();
			});
		}


	}

}
