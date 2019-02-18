using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.ReadModel;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	[TestFixture]
	public class GroupPagePersonTransformerTest
	{

		private GroupPagePersonTransformer _target;
		private ICommonStateHolder _groupPageDataProvider;
		CultureInfo _englishCulture;

		[SetUp]
		public void Setup()
		{
			_groupPageDataProvider = new GroupingsProviderForTest();
			_target = new GroupPagePersonTransformer(() => _groupPageDataProvider);
			_englishCulture = CultureInfo.GetCultureInfo("en-GB");
		}

		[Test]
		public void ShouldReturnUserDefinedGroupingsWithOnlyOneLevelOfGroups()
		{
			/* Original user defined grouping */
			//GroupPage1
			//   GroupA
			//      GroupAB
			//         GroupABC
			//            Person4
			//         Person1
			//      Person2
			//   GroupB
			//      Person3
			//GroupPage2
			//   GroupC
			//      Person1

			/* User defined grouping after compressed to only one level of groups */
			//GroupPage1
			//   GroupA
			//      Person4
			//      Person1
			//      Person2
			//   GroupB
			//      Person3
			//GroupPage2
			//   GroupC
			//      Person1

			var firstGrouping = _groupPageDataProvider.UserDefinedGroupings(null).First();
			IPerson person1 = firstGrouping.RootGroupCollection[0].ChildGroupCollection[0].PersonCollection[0];
			IPerson person2 = firstGrouping.RootGroupCollection[0].PersonCollection[0];
			IPerson person3 = firstGrouping.RootGroupCollection[1].PersonCollection[0];
			IPerson person4 =
				 firstGrouping.RootGroupCollection[0].ChildGroupCollection[0].
					  ChildGroupCollection[0].PersonCollection[0];

			IRootPersonGroup groupA = firstGrouping.RootGroupCollection[0];
			IRootPersonGroup groupB = firstGrouping.RootGroupCollection[1];
			IRootPersonGroup groupC = _groupPageDataProvider.UserDefinedGroupings(null).ElementAt(1).RootGroupCollection[0];
			IGroupPage groupPage1 = firstGrouping;
			IGroupPage groupPage2 = _groupPageDataProvider.UserDefinedGroupings(null).ElementAt(1);

			// Get the user defined group pages and compress to only one level of groups
			var comressedUserDefinedGroupings = _target.UserDefinedGroupings;


			// Check GroupPage
			Assert.AreEqual(2, comressedUserDefinedGroupings.Count());
			Assert.AreEqual(2, groupPage1.RootGroupCollection.Count);
			Assert.AreEqual(1, groupPage2.RootGroupCollection.Count);
			// Check that obsolete groups are removed
			Assert.IsEmpty(groupA.ChildGroupCollection);
			Assert.IsEmpty(groupB.ChildGroupCollection);
			// Check that the root groups contains the correct agents
			Assert.AreEqual(3, groupA.PersonCollection.Count);
			Assert.IsTrue(groupA.PersonCollection.Contains(person1));
			Assert.IsTrue(groupA.PersonCollection.Contains(person2));
			Assert.IsTrue(groupA.PersonCollection.Contains(person4));
			Assert.AreEqual(1, groupB.PersonCollection.Count);
			Assert.IsTrue(groupB.PersonCollection.Contains(person3));
			Assert.IsTrue(groupC.PersonCollection.Contains(person1));
			Assert.AreEqual(1, groupC.PersonCollection.Count);
		}

		[Test]
		public void ShouldReturnBuiltInGroupPagesIncludingOptionalColumnsWithValues()
		{

			IList<IGroupPage> dynamicGroupPages = _target.BuiltInGroupPages;

			Assert.IsNotNull(dynamicGroupPages);
			Assert.AreEqual(7, dynamicGroupPages.Count);
		}

		[Test]
		public void ShouldBeACertainNumberOfRowsAfterTransformation()
		{
			IEnumerable<IGroupPage> userDefinedGroupings = _target.UserDefinedGroupings;
			IList<IGroupPage> builtInGroupings = _target.BuiltInGroupPages;

			using (DataTable dataTable = new DataTable())
			{
				dataTable.Locale = Thread.CurrentThread.CurrentCulture;
				GroupPagePersonInfrastructure.AddColumnsToDataTable(dataTable);
				_target.Transform(builtInGroupings, userDefinedGroupings, dataTable);

				Assert.IsNotNull(dataTable);
				Assert.AreEqual(16, dataTable.Rows.Count);
			}
		}

		[Test]
		public void ShouldSetIdOnBuiltInGroupPageInTransformation()
		{
			var userDefinedGroupings = _target.UserDefinedGroupings;
			IList<IGroupPage> builtInGroupings = _target.BuiltInGroupPages;

			Assert.IsNull(builtInGroupings[0].Id);

			using (DataTable dataTable = new DataTable())
			{
				dataTable.Locale = Thread.CurrentThread.CurrentCulture;
				GroupPagePersonInfrastructure.AddColumnsToDataTable(dataTable);
				_target.Transform(builtInGroupings, userDefinedGroupings, dataTable);

				foreach (DataRow row in dataTable.Rows)
				{
					Assert.IsTrue(row["group_page_code"] != DBNull.Value);
				}
			}
		}


		[Test]
		public void ShouldNotSetOptionalColumnGroupPageAsCustom()
		{
			IList<IGroupPage> builtInGroupings = _target.BuiltInGroupPages;
			var optionalColumn = _groupPageDataProvider.OptionalColumnCollectionAvailableAsGroupPage.First();

			Assert.IsNull(builtInGroupings[0].Id);

			using (DataTable dataTable = new DataTable())
			{
				dataTable.Locale = Thread.CurrentThread.CurrentCulture;
				GroupPagePersonInfrastructure.AddColumnsToDataTable(dataTable);
				_target.Transform(builtInGroupings, new List<IGroupPage>(), dataTable);

				foreach (DataRow row in dataTable.Rows)
				{
					if((Guid)row["group_page_code"] == optionalColumn.Id.Value)
						Assert.IsFalse((bool)row["group_is_custom"]);
				}
			}
		}

		[Test]
		public void ShouldTransformBuiltInGroupPagesCorrectly()
		{
			IList<IGroupPage> builtInGroupings = _target.BuiltInGroupPages;
			DataRow row;

			using (DataTable dataTable = new DataTable())
			{
				dataTable.Locale = Thread.CurrentThread.CurrentCulture;
				GroupPagePersonInfrastructure.AddColumnsToDataTable(dataTable);
				_target.Transform(builtInGroupings, new List<IGroupPage>(), dataTable);
				row = dataTable.Rows[0];
			}
			//Teleopti.Ccc.UserTexts.Resources.ResourceManager.GetString()
			//ResourceManager resourceManager = new ResourceManager("Teleopti.Ccc.UserTexts", Assembly.GetExecutingAssembly());
			//resourceManager.

			IGroupPage gp = builtInGroupings[0];
			GroupPage gpCasted = gp as GroupPage;
			Assert.IsNotNull(gpCasted);

			Assert.AreEqual(gp.Id, row["group_page_code"]);
			//Assert.AreEqual(DBNull.Value, row["group_page_name"]);
			Assert.AreNotEqual(DBNull.Value, row["group_page_name"]);
			Assert.AreEqual(gp.DescriptionKey, row["group_page_name_resource_key"]);
			Assert.AreEqual(gp.RootGroupCollection[0].Id, row["group_code"]);
			Assert.AreEqual(gp.RootGroupCollection[0].Name, row["group_name"]);
			Assert.IsFalse((bool)row["group_is_custom"]);
			Assert.AreEqual(gp.RootGroupCollection[0].PersonCollection[0].Id, row["person_code"]);
			Assert.AreEqual(gpCasted.BusinessUnit.Id, row["business_unit_code"]);
			Assert.AreEqual(gpCasted.BusinessUnit.Description.Name, row["business_unit_name"]);
			Assert.AreEqual(1, row["datasource_id"]);
		}

		[Test]
		public void ShouldTransformUserDefinedGroupPagesCorrectly()
		{
			var userDefinedGroupings = _target.UserDefinedGroupings;
			DataRow row;

			using (DataTable dataTable = new DataTable())
			{
				dataTable.Locale = Thread.CurrentThread.CurrentCulture;
				GroupPagePersonInfrastructure.AddColumnsToDataTable(dataTable);
				_target.Transform(new List<IGroupPage>(), userDefinedGroupings, dataTable);
				row = dataTable.Rows[0];
			}

			IGroupPage gp = userDefinedGroupings.First();
			GroupPage gpCasted = gp as GroupPage;
			Assert.IsNotNull(gpCasted);

			Assert.AreEqual(gp.Id, row["group_page_code"]);
			Assert.AreEqual(gp.Description.Name, row["group_page_name"]);
			Assert.AreEqual(DBNull.Value, row["group_page_name_resource_key"]);
			Assert.AreEqual(gp.RootGroupCollection[0].Id, row["group_code"]);
			Assert.AreEqual(gp.RootGroupCollection[0].Name, row["group_name"]);
			Assert.IsTrue((bool)row["group_is_custom"]);
			Assert.AreEqual(gp.RootGroupCollection[0].PersonCollection[0].Id, row["person_code"]);
			Assert.AreEqual(gpCasted.BusinessUnit.Id, row["business_unit_code"]);
			Assert.AreEqual(gpCasted.BusinessUnit.Description.Name, row["business_unit_name"]);
			Assert.AreEqual(1, row["datasource_id"]);
		}

		[Test]
		public void ShouldTranslateBuiltInGroupPageNamesToEnglish()
		{
			IList<IGroupPage> builtInGroupings = _target.BuiltInGroupPages;
			DataRow contractsRow;
			DataRow contractScheduleRow;
			DataRow partTimepercentagesRow;
			DataRow noteRow;
			DataRow ruleSetBagRow;

			using (DataTable dataTable = new DataTable())
			{
				dataTable.Locale = Thread.CurrentThread.CurrentCulture;
				GroupPagePersonInfrastructure.AddColumnsToDataTable(dataTable);
				_target.Transform(builtInGroupings, new List<IGroupPage>(), dataTable);

				contractsRow = dataTable.Rows[0];
				contractScheduleRow = dataTable.Rows[2];
				partTimepercentagesRow = dataTable.Rows[4];
				noteRow = dataTable.Rows[6];
				ruleSetBagRow = dataTable.Rows[8];

			}

			string contracts = TranslateToEnglish(builtInGroupings[0].DescriptionKey);
			string contractSchedule = TranslateToEnglish(builtInGroupings[1].DescriptionKey);
			string partTimepercentages = TranslateToEnglish(builtInGroupings[2].DescriptionKey);
			string noteGroupPage = TranslateToEnglish(builtInGroupings[3].DescriptionKey);
			string ruleSetBagGroupPage = TranslateToEnglish(builtInGroupings[4].DescriptionKey);

			Assert.AreEqual(contracts, contractsRow["group_page_name"]);
			Assert.AreEqual(contractSchedule, contractScheduleRow["group_page_name"]);
			Assert.AreEqual(partTimepercentages, partTimepercentagesRow["group_page_name"]);
			Assert.AreEqual(noteGroupPage, noteRow["group_page_name"]);
			Assert.AreEqual(ruleSetBagGroupPage, ruleSetBagRow["group_page_name"]);
		}

		[Test]
		public void AllBuildInGroupPageShouldHaveId()
		{
			var builtInGroupings = _target.BuiltInGroupPages;
			Assert.IsTrue(builtInGroupings.All(d => d.RootGroupCollection.All(g => g.Id.HasValue)));
		}

		private string TranslateToEnglish(string key)
		{
			return Resources.ResourceManager.GetString(key, _englishCulture);
		}
	}

	internal class GroupingsProviderForTest : ICommonStateHolder
	{
		private IList<IPerson> _personCollection;
		private IList<IContract> _contractCollection;
		private IList<IContractSchedule> _contractScheduleCollection;
		private IList<IPartTimePercentage> _partTimePercentageCollection;
		private IList<IRuleSetBag> _ruleSetBagCollection;
		private List<IGroupPage> _userDefinedGroupings;
		private List<IOptionalColumn> _optionalColumnsAsGroupPage;

		public IEnumerable<IPerson> PersonCollection
		{
			get
			{
				if (_personCollection == null)
				{
					// Create two persons with person periods set to different contracts etc
					// The person period start date is dynamic due to the test using it will check for "Today".

					DateOnly periodStartDate = DateOnly.Today;

					//Person 1
					IPerson person1 = PersonFactory.CreatePerson("Greg", "Hancock");
					person1.SetId(Guid.NewGuid());
					person1.Note = "Greg the great";
					IPersonContract personContract1 = PersonContractFactory.CreatePersonContract(ContractCollection.First(),
																						  PartTimePercentageCollection.First(),
																						  ContractScheduleCollection.First());
					ITeam team1 = TeamFactory.CreateSimpleTeam("Strängnäs");
					IPersonPeriod pp1 = PersonPeriodFactory.CreatePersonPeriod(periodStartDate, personContract1, team1);
					pp1.RuleSetBag = RuleSetBagCollection.First();
					person1.AddPersonPeriod(pp1);
					person1.SetOptionalColumnValue(new OptionalColumnValue("group1"), OptionalColumnCollectionAvailableAsGroupPage.First());

					//Person 2
					IPerson person2 = PersonFactory.CreatePerson("Billy", "Hamill");
					person2.SetId(Guid.NewGuid());
					person2.Note = "Billy the bullet";
					IPersonContract personContract2 = PersonContractFactory.CreatePersonContract(ContractCollection.ElementAt(1),
																						  PartTimePercentageCollection.ElementAt(1),
																						  ContractScheduleCollection.ElementAt(1));
					ITeam team2 = TeamFactory.CreateSimpleTeam("Stockholm");
					IPersonPeriod pp2 = PersonPeriodFactory.CreatePersonPeriod(periodStartDate, personContract2, team2);
					pp2.RuleSetBag = RuleSetBagCollection.ElementAt(1);
					person2.AddPersonPeriod(pp2);

					_personCollection = new List<IPerson> { person1, person2 };
				}
				return _personCollection;
			}
		}

		public IList<IScenario> ScenarioCollection { get; }
		public IScenario DefaultScenario { get; }
		public IList<TimeZoneInfo> TimeZonesUsedByClient { get; }
		public IList<TimeZoneInfo> TimeZonesUsedByDataSources { get; }
		public IList<TimeZonePeriod> PeriodToLoadBridgeTimeZone { get; }
		public IScheduleDictionary GetSchedules(DateTimePeriod period, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public ICollection<ISkillDay> GetSkillDaysCollection(DateTimePeriod period, IList<ISkill> skills, IScenario scenario,
			IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade)
		{
			throw new NotImplementedException();
		}

		public ICollection<ISkillDay> GetSkillDaysCollection(IScenario scenario, DateTime lastCheck,
			IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade)
		{
			throw new NotImplementedException();
		}

		public IDictionary<ISkill, IEnumerable<ISkillDay>> GetSkillDaysDictionary(DateTimePeriod period, IList<ISkill> skills, IScenario scenario,
			IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade)
		{
			throw new NotImplementedException();
		}

		public IList<IPerson> UserCollection { get; }
		public IList<IActivity> ActivityCollection { get; }
		public IList<IAbsence> AbsenceCollection { get; }
		public IList<IDayOffTemplate> DayOffTemplateCollection { get; }
		public IList<IShiftCategory> ShiftCategoryCollection { get; }
		public IList<IApplicationFunction> ApplicationFunctionCollection { get; }
		public IList<IApplicationRole> ApplicationRoleCollection { get; }
		public IList<IAvailableData> AvailableDataCollection { get; }
		public IList<IScenario> ScenarioCollectionDeletedExcluded { get; }
		public IList<IScheduleDay> GetSchedulePartPerPersonAndDate(IScheduleDictionary scheduleDictionary)
		{
			throw new NotImplementedException();
		}

		public IList<IMultiplicatorDefinitionSet> MultiplicatorDefinitionSetCollection { get; }
		public IList<IScheduleDay> LoadSchedulePartsPerPersonAndDate(DateTimePeriod period, IScenario scenario)
		{
			throw new NotImplementedException();
		}
		
		public IDictionary<DateOnly, IScheduleDictionary> GetSchedules(HashSet<IStudentAvailabilityDay> days, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public IDictionary<DateTimePeriod, IScheduleDictionary> GetScheduleCache()
		{
			throw new NotImplementedException();
		}

		public IList<IPerson> PersonsWithIds(List<Guid> ids)
		{
			throw new NotImplementedException();
		}

		public IScheduleDay GetSchedulePartOnPersonAndDate(IPerson person, DateOnly restrictionDate, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public void SetThisTime(ILastChangedReadModel lastTime, string step)
		{
			throw new NotImplementedException();
		}

		public void UpdateThisTime(string step, IBusinessUnit businessUnit)
		{
			throw new NotImplementedException();
		}

		public bool PermissionsMustRun()
		{
			throw new NotImplementedException();
		}

		public void SetLoadBridgeTimeZonePeriod(DateTimePeriod period, string timeZoneCode)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IContract> ContractCollection
		{
			get
			{
				if (_contractCollection == null)
				{
					IContract contract1 = ContractFactory.CreateContract("Contract Fixed Staff");
					IContract contract2 = ContractFactory.CreateContract("Contract name duplicate");
					IContract contract3 = ContractFactory.CreateContract("Contract name duplicate");
					contract1.SetId(Guid.NewGuid());
					contract2.SetId(Guid.NewGuid());
					contract3.SetId(Guid.NewGuid());
					_contractCollection = new List<IContract> { contract1, contract2, contract3 };
				}
				return _contractCollection;
			}
		}

		public IEnumerable<IContractSchedule> ContractScheduleCollection
		{
			get
			{
				if (_contractScheduleCollection == null)
				{
					IContractSchedule contractSchedule1 = ContractScheduleFactory.CreateContractSchedule("Contract Schedule Fixed Staff");
					IContractSchedule contractSchedule2 = ContractScheduleFactory.CreateContractSchedule("Contract Schedule Part Time 75%");
					contractSchedule1.SetId(Guid.NewGuid());
					contractSchedule2.SetId(Guid.NewGuid());
					_contractScheduleCollection = new List<IContractSchedule> { contractSchedule1, contractSchedule2 };
				}
				return _contractScheduleCollection;
			}
		}

		public IEnumerable<IPartTimePercentage> PartTimePercentageCollection
		{
			get
			{
				if (_partTimePercentageCollection == null)
				{
					IPartTimePercentage partTimePercentage1 = PartTimePercentageFactory.CreatePartTimePercentage("PTP 100%");
					IPartTimePercentage partTimePercentage2 = PartTimePercentageFactory.CreatePartTimePercentage("PTP 50%");
					partTimePercentage1.SetId(Guid.NewGuid());
					partTimePercentage2.SetId(Guid.NewGuid());
					_partTimePercentageCollection = new List<IPartTimePercentage> { partTimePercentage1, partTimePercentage2 };
				}
				return _partTimePercentageCollection;
			}
		}

		public IEnumerable<IRuleSetBag> RuleSetBagCollection
		{
			get
			{
				if (_ruleSetBagCollection == null)
				{
					IRuleSetBag ruleSetBag1 = new RuleSetBag { Description = new Description("Strängnäs 100%") };
					IRuleSetBag ruleSetBag2 = new RuleSetBag { Description = new Description("Stockholm 50%") };
					ruleSetBag1.SetId(Guid.NewGuid());
					ruleSetBag2.SetId(Guid.NewGuid());
					_ruleSetBagCollection = new List<IRuleSetBag> { ruleSetBag1, ruleSetBag2 };
				}
				return _ruleSetBagCollection;
			}
		}

		public IEnumerable<IGroupPage> UserDefinedGroupings(IScheduleDictionary schedules)
		{
			if (_userDefinedGroupings == null)
			{
				//GroupPage1
				//   GroupA
				//      GroupAB
				//         GroupABC
				//            Person4
				//         Person1
				//      Person2
				//   GroupB
				//      Person3
				//GroupPage2
				//   GroupC
				//      Person1

				IGroupPage groupPage1 = new GroupPage("GroupPage1");
				groupPage1.SetId(Guid.NewGuid());
				IGroupPage groupPage2 = new GroupPage("GroupPage2");
				groupPage2.SetId(Guid.NewGuid());
				IRootPersonGroup groupA = new RootPersonGroup("GroupA");
				groupA.SetId(Guid.NewGuid());
				IRootPersonGroup groupB = new RootPersonGroup("GroupB");
				groupB.SetId(Guid.NewGuid());
				IRootPersonGroup groupC = new RootPersonGroup("GroupC");
				groupC.SetId(Guid.NewGuid());
				IChildPersonGroup groupAb = new ChildPersonGroup("GroupAB");
				IChildPersonGroup groupAbc = new ChildPersonGroup("GroupABC");
				IPerson person1 = PersonFactory.CreatePerson("Person1");
				person1.SetId(Guid.NewGuid());
				IPerson person2 = PersonFactory.CreatePerson("Person2");
				person2.SetId(Guid.NewGuid());
				IPerson person3 = PersonFactory.CreatePerson("Person3");
				person3.SetId(Guid.NewGuid());
				IPerson person4 = PersonFactory.CreatePerson("Person4");
				person4.SetId(Guid.NewGuid());

				groupPage1.AddRootPersonGroup(groupA);
				groupPage1.AddRootPersonGroup(groupB);
				groupPage2.AddRootPersonGroup(groupC);
				groupA.AddChildGroup(groupAb);
				groupAb.AddChildGroup(groupAbc);
				groupA.AddPerson(person2);
				groupB.AddPerson(person3);
				groupAb.AddPerson(person1);
				groupC.AddPerson(person1);
				groupAbc.AddPerson(person4);

				_userDefinedGroupings = new List<IGroupPage> {groupPage1, groupPage2};
			}
			return _userDefinedGroupings;
		}

		public IBusinessUnit BusinessUnit
		{
			get { return BusinessUnitUsedInTests.BusinessUnit; }
		}

		public DateOnlyPeriod SelectedPeriod
		{
			get { return new DateOnlyPeriod(DateOnly.Today, DateOnly.Today); }
		}

		public IList<ISkill> SkillCollection
		{
			get
			{
				ISkill skill1 = SkillFactory.CreateSkill("skill1");
				ISkill skill2 = SkillFactory.CreateSkill("skill2");
				skill1.SetId(Guid.NewGuid());
				skill2.SetId(Guid.NewGuid());

				return new List<ISkill> { skill1, skill2 };
			}
		}
		public IList<IOptionalColumn> OptionalColumnCollectionAvailableAsGroupPage
		{
			get
			{
				if (_optionalColumnsAsGroupPage == null)
				{
					_optionalColumnsAsGroupPage = new List<IOptionalColumn>() {new OptionalColumn("opt column").WithId(), new OptionalColumn("opt column with no values for any persons").WithId()};

				}
				return _optionalColumnsAsGroupPage;
			}
		}
	}
}
