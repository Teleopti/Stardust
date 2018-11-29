using System;
using System.Drawing;
using System.IO;
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
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ExportSchedule
{
	[TestFixture, DomainTest]
	public class ExportScheduleServiceTest : IIsolateSystem, IExtendSystem
	{
		public ExportScheduleService Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeScenarioRepository ScenarioRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonFinderReadOnlyRepository PersonFinder;
		public FakeTeamRepository TeamRepository;
		public FakeGroupingReadOnlyRepository GroupingReadOnlyRepository;
		public FakeOptionalColumnRepository OptionalColumnRepository;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<ExportScheduleService>();
		}
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<PeopleSearchProvider>().For<IPeopleSearchProvider>();
			isolate.UseTestDouble<UserTextTranslator>().For<IUserTextTranslator>();
			isolate.UseTestDouble<PermissionProvider>().For<IPermissionProvider>();
		}

		[Test]
		public void ShouldGeneratePersonScheduleSummaryInContentRow()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			ScenarioRepository.Has(scenario);
			var team = TeamFactory.CreateSimpleTeam("myTeam").WithId();
			team.Site = SiteFactory.CreateSimpleSite("mySite").WithId();
			var person = PersonFactory.CreatePerson("ashley", "andeen").WithId();
			person.SetEmploymentNumber("1234");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(scheduleDate.AddDays(-1)), PersonContractFactory.CreatePersonContract(), team ));
			PersonFinder.Has(person);
			PersonRepository.Has(person);
			var shift = ShiftCategoryFactory.CreateShiftCategory("Day");
			var period = new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario,
				ActivityFactory.CreateActivity("Phone"), period,
				shift);
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));
			PersonAssignmentRepository.Add(pa);

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
			scheduleData.Cells[3].StringCellValue.Should().Be.EqualTo(shift.Description.ShortName + " " + period.TimePeriod(TimeZoneInfo.Utc).ToShortTimeString());

		}
		
		[Test]
		public void ShouldSortPersonScheduleSummaryInContentRow()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			ScenarioRepository.Has(scenario);
			var team = TeamFactory.CreateSimpleTeam("myTeam").WithId();
			team.Site = SiteFactory.CreateSimpleSite("mySite").WithId();
			var person = PersonFactory.CreatePerson("ashley", "andeen").WithId();
			var person1 = PersonFactory.CreatePerson("b", "b").WithId();
			person.SetEmploymentNumber("1234");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(scheduleDate.AddDays(-1)), PersonContractFactory.CreatePersonContract(), team ));
			
			PersonFinder.Has(person1);
			PersonFinder.Has(person);
			
			PersonRepository.Has(person1);
			PersonRepository.Has(person);
			

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

			sheet.GetRow(9).Cells[0].StringCellValue.Should().Be.EqualTo("ashley@andeen");
			sheet.GetRow(10).Cells[0].StringCellValue.Should().Be.EqualTo("b@b");
		}
		
		[Test]
		public void ShouldDisplayOptionalColumnHeaderInScheduleSummaryInContentRow()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			ScenarioRepository.Has(scenario);
			var team = TeamFactory.CreateSimpleTeam("myTeam").WithId();
			team.Site = SiteFactory.CreateSimpleSite("mySite").WithId();
			var person = PersonFactory.CreatePerson("ashley", "andeen").WithId();
			person.SetEmploymentNumber("1234");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(scheduleDate.AddDays(-1)), PersonContractFactory.CreatePersonContract(), team ));
			PersonFinder.Has(person);
			PersonRepository.Has(person);
			
			var optionalColumn = new OptionalColumn("opt1").WithId();
			var optionalColumnValue1 = new OptionalColumnValue("value1").WithId();
			var optionalColumnValue2 = new OptionalColumnValue("value2").WithId();
			
			person.SetOptionalColumnValue(optionalColumnValue1, optionalColumn);
			OptionalColumnRepository.Add(optionalColumn);
			OptionalColumnRepository.AddPersonValues(optionalColumnValue1);
			OptionalColumnRepository.AddPersonValues(optionalColumnValue2);
			
			
			var input = new ExportScheduleForm
			{
				StartDate = new DateOnly(scheduleDate),
				EndDate = new DateOnly(scheduleDate),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				SelectedGroups = new SearchGroupIdsData
				{
					SelectedGroupIds = new[] {Guid.NewGuid().ToString()}
				},
				OptionalColumnIds = new []{optionalColumn.Id.GetValueOrDefault()}
			};

			var byteArray = Target.ExportToExcel(input).Data;
			var workbook = new XSSFWorkbook(new MemoryStream(byteArray));
			var sheet = workbook.GetSheetAt(0);

			var headerRow = sheet.GetRow(8);
			headerRow.Cells[3].StringCellValue.Should().Be.EqualTo("opt1");
		}

		[Test]
		public void ShouldDisplayOptionalColumnValuesInScheduleSummaryInContentRow()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			ScenarioRepository.Has(scenario);
			var team = TeamFactory.CreateSimpleTeam("myTeam").WithId();
			team.Site = SiteFactory.CreateSimpleSite("mySite").WithId();
			var person = PersonFactory.CreatePerson("ashley", "andeen").WithId();
			person.SetEmploymentNumber("1234");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(scheduleDate.AddDays(-1)), PersonContractFactory.CreatePersonContract(), team ));
			PersonFinder.Has(person);
			PersonRepository.Has(person);
			
			var optionalColumn = new OptionalColumn("opt1").WithId();
			var optionalColumnValue1 = new OptionalColumnValue("value1").WithId();
			var optionalColumnValue2 = new OptionalColumnValue("value2").WithId();
			
			person.SetOptionalColumnValue(optionalColumnValue1, optionalColumn);
			OptionalColumnRepository.Add(optionalColumn);
			OptionalColumnRepository.AddPersonValues(optionalColumnValue1);
			OptionalColumnRepository.AddPersonValues(optionalColumnValue2);
			
			
			var input = new ExportScheduleForm
			{
				StartDate = new DateOnly(scheduleDate),
				EndDate = new DateOnly(scheduleDate),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				SelectedGroups = new SearchGroupIdsData
				{
					SelectedGroupIds = new[] {Guid.NewGuid().ToString()}
				},
				OptionalColumnIds = new []{optionalColumn.Id.GetValueOrDefault()}
			};

			var byteArray = Target.ExportToExcel(input).Data;
			var workbook = new XSSFWorkbook(new MemoryStream(byteArray));
			var sheet = workbook.GetSheetAt(0);

			var scheduleData = sheet.GetRow(9);
			scheduleData.Cells[3].StringCellValue.Should().Be.EqualTo("value1");
			
		}

		[Test]
		public void ShouldDisplayCorrectOrderForOptionalColumnHeadersAndValuesInScheduleSummaryInContentRow()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var scenario = ScenarioFactory.CreateScenarioWithId("default", true);
			ScenarioRepository.Has(scenario);
			var team = TeamFactory.CreateSimpleTeam("myTeam").WithId();
			team.Site = SiteFactory.CreateSimpleSite("mySite").WithId();
			var person = PersonFactory.CreatePerson("ashley", "andeen").WithId();
			person.SetEmploymentNumber("1234");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(scheduleDate.AddDays(-1)), PersonContractFactory.CreatePersonContract(), team));
			PersonFinder.Has(person);
			PersonRepository.Has(person);

			var optionalColumn = new OptionalColumn("opt1").WithId();
			var optionalColumnValue1 = new OptionalColumnValue("opt1 value1").WithId();

			var optionalColumnAnother = new OptionalColumn("opt2").WithId();
			var optionalColumnAnotherValue1 = new OptionalColumnValue("opt2 value1").WithId();

			person.SetOptionalColumnValue(optionalColumnValue1, optionalColumn);
			OptionalColumnRepository.Add(optionalColumn);
			OptionalColumnRepository.AddPersonValues(optionalColumnValue1);

			person.SetOptionalColumnValue(optionalColumnAnotherValue1, optionalColumnAnother);
			OptionalColumnRepository.Add(optionalColumnAnother);
			OptionalColumnRepository.AddPersonValues(optionalColumnAnotherValue1);

			var input = new ExportScheduleForm
			{
				StartDate = new DateOnly(scheduleDate),
				EndDate = new DateOnly(scheduleDate),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				SelectedGroups = new SearchGroupIdsData
				{
					SelectedGroupIds = new[] { Guid.NewGuid().ToString() }
				},
				OptionalColumnIds = new[] {
					optionalColumnAnother.Id.GetValueOrDefault(),
					optionalColumn.Id.GetValueOrDefault()
				}
			};

			var byteArray = Target.ExportToExcel(input).Data;
			var workbook = new XSSFWorkbook(new MemoryStream(byteArray));
			var sheet = workbook.GetSheetAt(0);

			var headerRow = sheet.GetRow(8);
			headerRow.Cells[3].StringCellValue.Should().Be.EqualTo("opt1");
			headerRow.Cells[4].StringCellValue.Should().Be.EqualTo("opt2");

			var scheduleData = sheet.GetRow(9);
			scheduleData.Cells[3].StringCellValue.Should().Be.EqualTo("opt1 value1");
			scheduleData.Cells[4].StringCellValue.Should().Be.EqualTo("opt2 value1");

		}


		[Test]
		public void ShouldDisplayEmptyOptionalColumnValuesInScheduleSummaryWhenNoValueInContentRow()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			ScenarioRepository.Has(scenario);
			var team = TeamFactory.CreateSimpleTeam("myTeam").WithId();
			team.Site = SiteFactory.CreateSimpleSite("mySite").WithId();
			var person = PersonFactory.CreatePerson("ashley", "andeen").WithId();
			person.SetEmploymentNumber("1234");
			person.AddPersonPeriod(new PersonPeriod(new DateOnly(scheduleDate.AddDays(-1)), PersonContractFactory.CreatePersonContract(), team ));
			PersonFinder.Has(person);
			PersonRepository.Has(person);
			
			var optionalColumn = new OptionalColumn("opt1").WithId();
			var optionalColumnValue = new OptionalColumnValue("value2").WithId();
			
			OptionalColumnRepository.Add(optionalColumn);
			OptionalColumnRepository.AddPersonValues(optionalColumnValue);
			var shift = ShiftCategoryFactory.CreateShiftCategory("Day");
			var period = new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 17);
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario,
				ActivityFactory.CreateActivity("Phone"), period,
				shift);
			pa.AddActivity(ActivityFactory.CreateActivity("activity1", new Color()),
				new DateTimePeriod(2020, 1, 1, 8, 2020, 1, 1, 9));
			pa.AddActivity(ActivityFactory.CreateActivity("activity2", new Color()),
				new DateTimePeriod(2020, 1, 1, 9, 2020, 1, 1, 11));
			PersonAssignmentRepository.Add(pa);
			
			var input = new ExportScheduleForm
			{
				StartDate = new DateOnly(scheduleDate),
				EndDate = new DateOnly(scheduleDate),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				SelectedGroups = new SearchGroupIdsData
				{
					SelectedGroupIds = new[] {Guid.NewGuid().ToString()}
				},
				OptionalColumnIds = new []{optionalColumn.Id.GetValueOrDefault()}
			};

			var byteArray = Target.ExportToExcel(input).Data;
			var workbook = new XSSFWorkbook(new MemoryStream(byteArray));
			var sheet = workbook.GetSheetAt(0);

			var scheduleData = sheet.GetRow(9);
			scheduleData.Cells[3].StringCellValue.Should().Be.EqualTo("");
		}
		
		[Test]
		public void ShouldReturnFailureWhenPeopleToExportMoreThan1000()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			ScenarioRepository.Has(scenario);
			var team = TeamFactory.CreateSimpleTeam("myTeam").WithId();
			team.Site = SiteFactory.CreateSimpleSite("mySite").WithId();
			for (int i = 0; i < 1001; i++)
			{
				var person = PersonFactory.CreatePerson("ashley" + i, "andeen").WithId();
				person.AddPersonPeriod(new PersonPeriod(new DateOnly(scheduleDate.AddDays(-1)), PersonContractFactory.CreatePersonContract(), team ));
				PersonFinder.Has(person);
				PersonRepository.Has(person);
			}

	
			var input = new ExportScheduleForm
			{
				StartDate = new DateOnly(scheduleDate),
				EndDate = new DateOnly(scheduleDate),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				SelectedGroups = new SearchGroupIdsData
				{
					SelectedGroupIds = new[] {Guid.NewGuid().ToString()}
				},
			};

			var result = Target.ExportToExcel(input);

			result.FailReason.Should().Be.EqualTo(string.Format(Resources.MaximumAgentToExport,1001));
			
		}


		[Test, Ignore("Check internally for development")]
		public void ShouldExportExcelWithCorrectFormat()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
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
			PersonAssignmentRepository.Add(pa);

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

		[Test]
		public void ShouldDisplaySelectedTeamsInHeaderRow()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			ScenarioRepository.Has(scenario);
			var team = TeamFactory.CreateSimpleTeam("myTeam").WithId();
			team.Site = SiteFactory.CreateSimpleSite("mySite").WithId();
			TeamRepository.Has(team);

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
		
		[Test]
		public void ShouldDisplaySelectedGroupsInHeaderRow()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			ScenarioRepository.Has(scenario);
			var team = TeamFactory.CreateSimpleTeam("myTeam").WithId();
			team.Site = SiteFactory.CreateSimpleSite("mySite").WithId();
			TeamRepository.Has(team);
			var groupPage = new ReadOnlyGroupPage
			{
				PageId = Guid.NewGuid(),
				PageName = "my page"
			};
			var groupDetail = new ReadOnlyGroupDetail
			{
				GroupId = Guid.NewGuid(),
				PageId = groupPage.PageId,
				GroupName = "test value"
			};

			GroupingReadOnlyRepository.Has(new [] {groupPage}, new [] {groupDetail});

			var input = new ExportScheduleForm
			{
				StartDate = new DateOnly(scheduleDate),
				EndDate = new DateOnly(scheduleDate),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				SelectedGroups = new SearchGroupIdsData
				{
					SelectedGroupPageId = groupPage.PageId,
					SelectedGroupIds = new[] { groupDetail.GroupId.ToString() }
				}
			};
			var byteArray = Target.ExportToExcel(input).Data;
			var workbook = new XSSFWorkbook(new MemoryStream(byteArray));
			var sheet = workbook.GetSheetAt(0);

			sheet.GetRow(0).Cells[1].StringCellValue.Should().Be.EqualTo("my page/test value");
		}
		[Test]
		public void ShouldDisplayTranslatedSelectedBuiltInGroupsInHeaderRow()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			ScenarioRepository.Has(scenario);
			var team = TeamFactory.CreateSimpleTeam("myTeam").WithId();
			team.Site = SiteFactory.CreateSimpleSite("mySite").WithId();
			TeamRepository.Has(team);
			var groupPage = new ReadOnlyGroupPage
			{
				PageId = Guid.NewGuid(),
				PageName = "xxContract"
			};
			var groupDetail = new ReadOnlyGroupDetail
			{
				GroupId = Guid.NewGuid(),
				PageId = groupPage.PageId,
				GroupName = "test value"
			};

			GroupingReadOnlyRepository.Has(new [] {groupPage}, new [] {groupDetail});

			var input = new ExportScheduleForm
			{
				StartDate = new DateOnly(scheduleDate),
				EndDate = new DateOnly(scheduleDate),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				SelectedGroups = new SearchGroupIdsData
				{
					SelectedGroupPageId = groupPage.PageId,
					SelectedGroupIds = new[] { groupDetail.GroupId.ToString() }
				}
			};
			var byteArray = Target.ExportToExcel(input).Data;
			var workbook = new XSSFWorkbook(new MemoryStream(byteArray));
			var sheet = workbook.GetSheetAt(0);

			sheet.GetRow(0).Cells[1].StringCellValue.Should().Be.EqualTo("Contract/test value");
		}
		
		[Test]
		public void ShouldDisplaySelectedDynamicGroupsInHeaderRow()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			ScenarioRepository.Has(scenario);
			var team = TeamFactory.CreateSimpleTeam("myTeam").WithId();
			team.Site = SiteFactory.CreateSimpleSite("mySite").WithId();
			TeamRepository.Has(team);
			var dynamicGroupPage = new OptionalColumn("my page")
			{
				AvailableAsGroupPage = true
			}.WithId();
			var groupValue = new OptionalColumnValue("my value");
			var person = PersonFactory.CreatePerson("ashley", "andeen").WithId();
			person.SetOptionalColumnValue(groupValue, dynamicGroupPage);
			OptionalColumnRepository.Add(dynamicGroupPage);
			OptionalColumnRepository.AddPersonValues(groupValue);

			var input = new ExportScheduleForm
			{
				StartDate = new DateOnly(scheduleDate),
				EndDate = new DateOnly(scheduleDate),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				SelectedGroups = new SearchGroupIdsData
				{
					SelectedGroupPageId = dynamicGroupPage.Id.GetValueOrDefault(),
					SelectedGroupIds = new[] { groupValue.Description }
				}
			};
			var byteArray = Target.ExportToExcel(input).Data;
			var workbook = new XSSFWorkbook(new MemoryStream(byteArray));
			var sheet = workbook.GetSheetAt(0);

			sheet.GetRow(0).Cells[1].StringCellValue.Should().Be.EqualTo("my page/my value");
		}

		[Test]
		public void ShouldDisplaySelectedOptionalColumnsInHeaderRow()
		{
			var scheduleDate = new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var scenario = ScenarioFactory.CreateScenarioWithId("test", true);
			ScenarioRepository.Has(scenario);
			
			var optionalCol1 = new OptionalColumn("opt1").WithId();
			var optionalCol2 = new OptionalColumn("opt2").WithId();
			OptionalColumnRepository.Add(optionalCol1);
			OptionalColumnRepository.Add(optionalCol2);
			
			var input = new ExportScheduleForm
			{
				StartDate = new DateOnly(scheduleDate),
				EndDate = new DateOnly(scheduleDate),
				ScenarioId = scenario.Id.GetValueOrDefault(),
				TimezoneId = TimeZoneInfo.Utc.Id,
				SelectedGroups = new SearchGroupIdsData
				{
					SelectedGroupIds = new[] { Guid.NewGuid().ToString() }
				},
				OptionalColumnIds = new []{optionalCol1.Id.GetValueOrDefault()}
			};
			
			var byteArray = Target.ExportToExcel(input).Data;
			var workbook = new XSSFWorkbook(new MemoryStream(byteArray));
			var sheet = workbook.GetSheetAt(0);

			sheet.GetRow(4).Cells[1].StringCellValue.Should().Be.EqualTo("opt1");
		}
	}
}
