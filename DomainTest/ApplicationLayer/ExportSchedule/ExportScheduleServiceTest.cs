using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using NPOI.XSSF.UserModel;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ExportSchedule;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ExportSchedule
{
	[TestFixture, DomainTest]
	public class ExportScheduleServiceTest : ISetup
	{
		public FakeCurrentScenario CurrentScenario;
		public ExportScheduleService Target;
		public FakeScheduleStorage ScheduleStorage;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonFinderReadOnlyRepository PersonFinder;
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<ExportScheduleService>();
			system.UseTestDouble<FakeCurrentScenario>().For<ICurrentScenario>();
			system.UseTestDouble<PeopleSearchProvider>().For<IPeopleSearchProvider>();
			system.UseTestDouble<UserTextTranslator>().For<IUserTextTranslator>();
			system.UseTestDouble<FakeScheduleStorage>().For<IScheduleStorage>();
			system.UseTestDouble<FakePersonFinderReadOnlyRepository>().For<IPersonFinderReadOnlyRepository>();
			system.UseTestDouble<FakeScenarioRepository>().For<IScenarioRepository>();
			system.UseTestDouble<FakePersonRepository>().For<IPersonRepository>();
			system.UseTestDouble<PermissionProvider>().For<IPermissionProvider>();
		}

		[Test]
		public void ShouldGeneratePersonScheduleSummaryInContentRow()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			ScenarioRepository.Has(scenario);
			var team = TeamFactory.CreateSimpleTeam("myTeam").WithId();
			team.Site = SiteFactory.CreateSimpleSite("mySite").WithId();
			var person = PersonFactory.CreatePerson("ashley", "andeen").WithId();
			person.SetEmploymentNumber("1234");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(scheduleDate.AddDays(-1)), PersonContractFactory.CreatePersonContract(), team ));
			PersonFinder.Has(person);
			PersonRepository.Has(person);
			var shift = ShiftCategoryFactory.CreateShiftCategory("Day");
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario,
				ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17),
				shift);
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));
			ScheduleStorage.Add(pa);

			var input = new ExportScheduleForm
			{
				StartDate = new DateOnly(scheduleDate),
				EndDate = new DateOnly(scheduleDate),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				SelectedGroups = new SearchGroupIdsData
				{
					SelectedGroupIds = new[] {Guid.NewGuid().ToString()}
				}
			};

			var byteArray = Target.ExportToExcel(input).Data;
			var workbook = new XSSFWorkbook(new MemoryStream(byteArray));
			var sheet = workbook.GetSheetAt(0);

			var scheduleData = sheet.GetRow(9);
			scheduleData.Cells[0].StringCellValue.Should().Be.EqualTo("ashley@andeen");
			scheduleData.Cells[1].StringCellValue.Should().Be.EqualTo("1234");
			scheduleData.Cells[2].StringCellValue.Should().Be.EqualTo("mySite/myTeam");
			scheduleData.Cells[3].StringCellValue.Should().Be.EqualTo("Da 8:00 - 17:00");

		}

		[Test, Ignore("Check internally for development")]
		public void ShouldExportExcelWithCorrectFormat()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			ScenarioRepository.Has(scenario);
			var team = TeamFactory.CreateSimpleTeam("myTeam").WithId();
			team.Site = SiteFactory.CreateSimpleSite("mySite").WithId();
			var person = PersonFactory.CreatePerson("ashley", "andeen").WithId();
			person.SetEmploymentNumber("1234");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(scheduleDate.AddDays(-1)), PersonContractFactory.CreatePersonContract(), team));
			PersonFinder.Has(person);
			PersonRepository.Has(person);
			var shift = ShiftCategoryFactory.CreateShiftCategory("Day");
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario,
				ActivityFactory.CreateActivity("Phone"), new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17),
				shift);
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));
			ScheduleStorage.Add(pa);

			var input = new ExportScheduleForm
			{
				StartDate = new DateOnly(scheduleDate),
				EndDate = new DateOnly(scheduleDate),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				SelectedGroups = new SearchGroupIdsData
				{
					SelectedGroupIds = new[] { Guid.NewGuid().ToString() }
				}
			};

			var byteArray = Target.ExportToExcel(input).Data;

			File.WriteAllBytes(@"C:\schedule.xlsx", byteArray);
		}

		[Test, Ignore("fix it later")]
		public void ShouldDisplaySelectedGroupsInHeaderRow()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			CurrentScenario.FakeScenario(scenario);
			ScenarioRepository.Has(scenario);
			var team = TeamFactory.CreateSimpleTeam("myTeam").WithId();
			team.Site = SiteFactory.CreateSimpleSite("mySite").WithId();

			var input = new ExportScheduleForm
			{
				StartDate = new DateOnly(scheduleDate),
				EndDate = new DateOnly(scheduleDate),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				SelectedGroups = new SearchGroupIdsData
				{
					SelectedGroupIds = new[] { team.Id.ToString() }
				}
			};
			var byteArray = Target.ExportToExcel(input).Data;
			var workbook = new XSSFWorkbook(new MemoryStream(byteArray));
			var sheet = workbook.GetSheetAt(0);

			sheet.GetRow(0).Cells[1].StringCellValue.Should().Be.EqualTo("mySite/myTeam");


		}
	}

	public class FakePersonFinderReadOnlyRepository : IPersonFinderReadOnlyRepository
	{
		private IList<IPerson> _personList = new List<IPerson>();
		public void Has(IPerson person)
		{
			_personList.Add(person);
		}
		public void Find(IPersonFinderSearchCriteria personFinderSearchCriteria)
		{
			throw new NotImplementedException();
		}

		public void FindInTeams(IPersonFinderSearchCriteria personFinderSearchCriteria, Guid[] teamIds)
		{
			throw new NotImplementedException();
		}

		public void FindPeople(IPeoplePersonFinderSearchCriteria personFinderSearchCriteria)
		{
			throw new NotImplementedException();
		}

		public void UpdateFindPerson(ICollection<Guid> ids)
		{
			throw new NotImplementedException();
		}

		public void UpdateFindPersonData(ICollection<Guid> ids)
		{
			throw new NotImplementedException();
		}

		public List<Guid> FindPersonIdsInTeams(DateOnly date, Guid[] teamIds, IDictionary<PersonFinderField, string> searchCriteria)
		{
			throw new NotImplementedException();
		}

		public List<Guid> FindPersonIdsInTeamsBasedOnPersonPeriod(DateOnlyPeriod period, Guid[] teamIds, IDictionary<PersonFinderField, string> searchCriteria)
		{
			throw new NotImplementedException();
		}

		public List<Guid> FindPersonIdsInGroupsBasedOnPersonPeriod(DateOnlyPeriod period, Guid[] groupIds, IDictionary<PersonFinderField, string> searchCriteria)
		{
			return _personList.Select(p => p.Id.GetValueOrDefault()).ToList();
		}

		public List<Guid> FindPersonIdsInDynamicOptionalGroupPages(DateOnlyPeriod period, Guid groupPageId, string[] dynamicValues,
			IDictionary<PersonFinderField, string> searchCriteria)
		{
			throw new NotImplementedException();
		}
	}
}
