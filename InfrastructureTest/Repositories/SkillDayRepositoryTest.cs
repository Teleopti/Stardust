using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class SkillDayRepositoryTest : RepositoryTest<ISkillDay>
	{
		private IScenario _scenario;
		private ISkillType _skillType;
		private IActivity _activity;
		private ISkill _skill;
		private IWorkload _workload;
		private DateTime _date;

		/// <summary>
		/// Runs every test. Implemented by repository's concrete implementation.
		/// </summary>
		protected override void ConcreteSetup()
		{
			_scenario = ScenarioFactory.CreateScenarioAggregate();
			PersistAndRemoveFromUnitOfWork(_scenario);

			_skillType = SkillTypeFactory.CreateSkillTypePhone();
			_skill = SkillFactory.CreateSkill("dummy", _skillType, 15);
			_activity = new Activity("dummyActivity");
			_skill.Activity = _activity;

			PersistAndRemoveFromUnitOfWork(_skillType);

			var timeZoneInfo = TimeZoneInfoFactory.GmtTimeZoneInfo();
			_skill.TimeZone = timeZoneInfo;
			PersistAndRemoveFromUnitOfWork(_activity);
			PersistAndRemoveFromUnitOfWork(_skill);

			_workload = WorkloadFactory.CreateWorkloadWithFullOpenHours(_skill);
			PersistAndRemoveFromUnitOfWork(_workload);

			_date = timeZoneInfo.SafeConvertTimeToUtc(new DateTime(2008, 1, 8, 0, 0, 0));
		}

		/// <summary>
		/// Creates an aggreagte using the Bu of logged in user.
		/// Should be a "full detailed" aggregate
		/// </summary>
		/// <returns></returns>
		protected override ISkillDay CreateAggregateWithCorrectBusinessUnit()
		{
			IList<TimePeriod> openHourPeriods = new List<TimePeriod>();
			openHourPeriods.Add(new TimePeriod("12:30-17:30"));

			WorkloadDay workloadDay = new WorkloadDay();
			var dateOnly = new DateOnly(_date);
			workloadDay.Create(dateOnly, _workload, openHourPeriods);
			workloadDay.Tasks = 7*20;
			workloadDay.AverageTaskTime = TimeSpan.FromSeconds(22);
			workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(233);

			IList<ISkillDataPeriod> skillDataPeriods = new List<ISkillDataPeriod>();
			skillDataPeriods.Add(
				new SkillDataPeriod(
					ServiceAgreement.DefaultValues(),
					new SkillPersonData(2, 5),
					new DateTimePeriod(_date, _date.Add(TimeSpan.FromHours(12)))));
			skillDataPeriods.Add(
				new SkillDataPeriod(
					skillDataPeriods[0].ServiceAgreement,
					skillDataPeriods[0].SkillPersonData,
					skillDataPeriods[0].Period.MovePeriod(TimeSpan.FromHours(12))));

			SkillDay skillDay = new SkillDay(dateOnly, _skill, _scenario,
				new List<IWorkloadDay> {workloadDay}, skillDataPeriods);

			_date = _date.AddDays(1);

			return skillDay;
		}

		/// <summary>
		/// Verifies the aggregate graph properties.
		/// </summary>
		/// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
		protected override void VerifyAggregateGraphProperties(ISkillDay loadedAggregateFromDatabase)
		{
			ISkillDay skillDay = CreateAggregateWithCorrectBusinessUnit();
			loadedAggregateFromDatabase.SetupSkillDay();
			Assert.AreEqual(skillDay.Scenario.Description, loadedAggregateFromDatabase.Scenario.Description);
			Assert.AreEqual(skillDay.Skill.Name, loadedAggregateFromDatabase.Skill.Name);
			Assert.AreEqual(skillDay.WorkloadDayCollection.Count, loadedAggregateFromDatabase.WorkloadDayCollection.Count);
			Assert.AreEqual(skillDay.WorkloadDayCollection[0].AverageTaskTime,
				loadedAggregateFromDatabase.WorkloadDayCollection[0].AverageTaskTime);
			Assert.AreEqual(skillDay.WorkloadDayCollection[0].TotalTasks,
				loadedAggregateFromDatabase.WorkloadDayCollection[0].TotalTasks);
		}
		
		[Test]
		public void CanGetUpdateInfoWhenWorkloadDayIsSaved()
		{
			ISkillDay skillDay = CreateAggregateWithCorrectBusinessUnit();
			ISkillDay skillDay2 = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(skillDay);
			PersistAndRemoveFromUnitOfWork(skillDay2);

			SkillDayCalculator calc = new SkillDayCalculator(skillDay2.Skill, new List<ISkillDay> {skillDay},
				new DateOnlyPeriod(2009, 1, 1, 2009, 12, 31));
			skillDay2.SkillDayCalculator = calc;
			skillDay2.WorkloadDayCollection[0].Close();

			PersistAndRemoveFromUnitOfWork(skillDay2);

			SetUpdatedOnForSkillDay(skillDay, -1);

			SkillDayRepository skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			ICollection<ISkillDay> skillDays =
				skillDayRepository.FindRange(
					new DateOnlyPeriod(skillDay.CurrentDate.AddDays(-10), skillDay.CurrentDate.AddDays(30)),
					_skill, _scenario);

			Assert.IsTrue(skillDays.Count == 2);
			ISkillDay result = skillDayRepository.FindLatestUpdated(_skill, _scenario, false);
			Assert.AreEqual(skillDay2, result);

			ISkillDay result2 = skillDayRepository.FindLatestUpdated(_skill, _scenario, true);
			Assert.IsNull(result2);
		}

		private void SetUpdatedOnForSkillDay(ISkillDay skillDay, int minutes)
		{
			Session.CreateSQLQuery("UPDATE dbo.SkillDay SET UpdatedOn = DATEADD(mi,:Minutes,UpdatedOn) WHERE Id=:Id;").SetGuid(
				"Id", skillDay.Id.GetValueOrDefault()).SetInt32("Minutes", minutes).ExecuteUpdate();
		}

		[Test]
		public void CanGetLastUpdatedWithLongtermTemplate()
		{
			ISkillDay skillDay = CreateAggregateWithCorrectBusinessUnit();
			ISkillDay skillDay2 = CreateAggregateWithCorrectBusinessUnit();
			SkillDayCalculator calc = new SkillDayCalculator(skillDay2.Skill, new List<ISkillDay> {skillDay},
				new DateOnlyPeriod(2009, 1, 1, 2009, 12, 31));
			skillDay2.SkillDayCalculator = calc;
			PersistAndRemoveFromUnitOfWork(skillDay2);

			IWorkloadDay workloadDay = skillDay2.WorkloadDayCollection[0];
			WorkloadDayTemplateReference templateReference = new WorkloadDayTemplateReference(Guid.Empty, 1,
				TemplateReference.LongtermTemplateKey, null, _workload);
			typeof (WorkloadDay).GetField("_templateReference", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(
				workloadDay, templateReference);
			skillDay2.AddWorkloadDay(workloadDay);
			PersistAndRemoveFromUnitOfWork(skillDay2);

			SetUpdatedOnForSkillDay(skillDay2, -1);

			PersistAndRemoveFromUnitOfWork(skillDay);
			SkillDayRepository skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);

			ISkillDay result = skillDayRepository.FindLatestUpdated(_skill, _scenario, true);

			Assert.AreEqual(skillDay2, result);
		}

		[Test]
		public void CanGetUpdatedSinceGivenDateTime()
		{
			ISkillDay skillDay = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(skillDay);

			ISkillDay skillDay2 = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(skillDay2);

			SetUpdatedOnForSkillDay(skillDay, -2);

			SkillDayRepository skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);

			var result = skillDayRepository.FindUpdatedSince(_scenario, skillDay.UpdatedOn.GetValueOrDefault().AddMinutes(-1));

			result.Single().Should().Be.EqualTo(skillDay2);
		}

		/// <summary>
		/// Determines whether this instance [can find skill days].
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-01-23
		/// </remarks>
		[Test]
		public void CanFindSkillDays()
		{
			ISkillDay skillDay = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(skillDay);

			SkillDayRepository skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			ICollection<ISkillDay> skillDays =
				skillDayRepository.FindRange(new DateOnlyPeriod(skillDay.CurrentDate, skillDay.CurrentDate.AddDays(1)),
					_skill, _scenario);

			Assert.AreEqual(1, skillDays.Count);
			ISkillDay sd = new List<ISkillDay>(skillDays)[0];
			Assert.IsTrue(LazyLoadingManager.IsInitialized(sd.SkillDataPeriodCollection));
			Assert.AreEqual(2, sd.SkillDataPeriodCollection.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(sd.WorkloadDayCollection));
			Assert.AreEqual(1, sd.WorkloadDayCollection.Count);
			Assert.IsTrue(LazyLoadingManager.IsInitialized(sd.WorkloadDayCollection[0].OpenHourList));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(sd.WorkloadDayCollection[0].TaskPeriodList));
			Assert.IsTrue(LazyLoadingManager.IsInitialized(sd.Scenario));
			Assert.IsTrue(
				LazyLoadingManager.IsInitialized(sd.WorkloadDayCollection[0].Workload.TemplateWeekCollection[0].TaskPeriodList[0]));
			Assert.IsTrue(
				LazyLoadingManager.IsInitialized(sd.WorkloadDayCollection[0].Workload.TemplateWeekCollection[0].OpenHourList[0]));
		}

		/// <summary>
		/// Verifies the get all skill days work without adding to repository.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-15
		/// </remarks>
		[Test]
		public void VerifyGetAllSkillDaysWorkWithoutAddingToRepository()
		{
			SkillDayRepository skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			var dateOnly = new DateOnly(_date);
			ICollection<ISkillDay> skillDays =
				skillDayRepository.GetAllSkillDays(new DateOnlyPeriod(dateOnly, dateOnly.AddDays(1)), new List<ISkillDay>(), _skill,
					_scenario, _ => { });
			Assert.AreEqual(2, skillDays.Count);
			Assert.IsNull(skillDays.ElementAt(0).Id);
			Assert.IsNull(skillDays.ElementAt(1).Id);
		}

		[Test]
		public void ShouldSetCorrectLocalDateForSkillDayInJordanStandardTime()
		{
			SkillDayRepository skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			_skill.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time"));

			ICollection<ISkillDay> skillDays =
				skillDayRepository.GetAllSkillDays(new DateOnlyPeriod(new DateOnly(2011, 3, 30), new DateOnly(2011, 4, 3)),
					new List<ISkillDay>(), _skill, _scenario, _ => { });
			skillDays.FirstOrDefault(s => s.CurrentDate == new DateOnly(2011, 4, 1)).Should().Not.Be.Null();
		}

		[Test]
		public void VerifyGetLatestSkillDayDateWorks()
		{
			SkillDayRepository skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			DateOnly defaultLastSkillDayDate = skillDayRepository.FindLastSkillDayDate(_workload, _scenario);

			Assert.AreEqual(new DateOnly(DateTime.Today.AddMonths(-1)), defaultLastSkillDayDate);

			SkillDay skillDay = createSkillDay(new DateOnly(_date), _workload, _skill, _scenario);
			PersistAndRemoveFromUnitOfWork(skillDay);

			SkillDay skillDay1 = createSkillDay(skillDay.CurrentDate.AddDays(-1), _workload, _skill, _scenario);
			PersistAndRemoveFromUnitOfWork(skillDay1);

			SkillDay skillDay2 = createSkillDay(skillDay.CurrentDate.AddDays(-2), _workload, _skill, _scenario);
			PersistAndRemoveFromUnitOfWork(skillDay2);

			var lastSkillDayDate = skillDayRepository.FindLastSkillDayDate(skillDay.Skill.WorkloadCollection.First(), _scenario);

			Assert.AreEqual(skillDay.CurrentDate, lastSkillDayDate);
		}

		[Test]
		public void ShouldInitializeWorkloadForWorkloadDaysForPerformanceReasons()
		{
			var dateTime = new DateOnly(_date);
			DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDate(dateTime, dateTime.AddDays(1),
				TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);

			IList<IWorkloadDay> workloadDays = WorkloadDayFactory.GetWorkloadDaysForTest(_date, _date, _workload);
			SkillPersonData skillPersonData = new SkillPersonData(0, 0);
			ISkillDataPeriod skillDataPeriod = new SkillDataPeriod(ServiceAgreement.DefaultValues(), skillPersonData,
				TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
					dateTimePeriod.StartDateTime,
					dateTimePeriod.EndDateTime, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone));
			IList<ISkillDataPeriod> skillDataPeriods = new List<ISkillDataPeriod> {skillDataPeriod};
			ISkillDay skillDay = new SkillDay(dateTime, _skill, _scenario, workloadDays, skillDataPeriods);

			PersistAndRemoveFromUnitOfWork(skillDay);
			UnitOfWork.Clear();

			var skillRepository = SkillRepository.DONT_USE_CTOR(UnitOfWork);
			var skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			var orgSkillDays =
				skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(dateTime, dateTime.AddDays(2)), skillRepository.LoadAll(),
					_scenario);

			orgSkillDays
				.All(s => LazyLoadingManager.IsInitialized(s.WorkloadDayCollection) &&
						  s.WorkloadDayCollection.All(w => LazyLoadingManager.IsInitialized(w.Workload))).Should().Be.True();
		}

		[Test]
		public void VerifyWorkloadDaysParentAreSet()
		{
			var dateTime = new DateOnly(_date);
			DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDate(dateTime, dateTime.AddDays(1),
				TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);

			IList<IWorkloadDay> workloadDays = WorkloadDayFactory.GetWorkloadDaysForTest(_date, _date, _workload);
			SkillPersonData skillPersonData = new SkillPersonData(0, 0);
			ISkillDataPeriod skillDataPeriod = new SkillDataPeriod(ServiceAgreement.DefaultValues(), skillPersonData,
				TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
					dateTimePeriod.StartDateTime,
					dateTimePeriod.EndDateTime, TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone));
			IList<ISkillDataPeriod> skillDataPeriods = new List<ISkillDataPeriod> { skillDataPeriod };
			ISkillDay skillDay = new SkillDay(dateTime, _skill, _scenario, workloadDays, skillDataPeriods);

			PersistAndRemoveFromUnitOfWork(skillDay);
			SkillDayRepository skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			ICollection<ISkillDay> orgSkillDays =
				skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(dateTime, dateTime.AddDays(2)), new List<ISkill> { _skill },
					_scenario);
			IList<ISkillDay> skillDays = new List<ISkillDay>(orgSkillDays);

			WorkloadDay workloadDay = (WorkloadDay)skillDays[0].WorkloadDayCollection[0];
			Assert.AreEqual(1, workloadDay.Parents.Count);
			Assert.AreEqual(skillDays[0], workloadDay.Parents[0]);
		}

		[Test,Ignore("Just here temporarily to measure performance")]
		public void ShouldReadTaskPeriodListFaster()
		{
			var startDate = new DateOnly(_date);
			PersistAndRemoveFromUnitOfWork(Enumerable.Range(0, 2100).Select(i =>
			{
				var skillDay = CreateAggregateWithCorrectBusinessUnit();
				skillDay.SkillDayCalculator = new SkillDayCalculator(_skill,new [] {skillDay},new DateOnlyPeriod(skillDay.CurrentDate,skillDay.CurrentDate));
				skillDay.WorkloadDayCollection[0].MakeOpen24Hours();
				return skillDay;
			}));
			var endDate = new DateOnly(_date);

			var timer = new Stopwatch();
			timer.Start();
			SkillDayRepository skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			ICollection<ISkillDay> orgSkillDays =
				skillDayRepository.FindReadOnlyRange(new DateOnlyPeriod(startDate, endDate), new List<ISkill> { _skill },
					_scenario);
			timer.Stop();

			var numberOfTaskPeriodsRead =
				orgSkillDays.SelectMany(s => s.WorkloadDayCollection).SelectMany(w => w.TaskPeriodList).Count();
			Console.WriteLine("{0} read in {1}",numberOfTaskPeriodsRead,timer.Elapsed);
		}

		[Test]
		public void VerifyIntervalsAreRemovedWhenSplittingAndMerging()
		{
			ISkillDay skillDay = CreateAggregateWithCorrectBusinessUnit();
			skillDay.SetupSkillDay();
			PersistAndRemoveFromUnitOfWork(skillDay);

			Assert.AreEqual(2, skillDay.SkillDataPeriodCollection.Count);

			skillDay.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> {skillDay},
				new DateOnlyPeriod());
			skillDay.SplitSkillDataPeriods(new List<ISkillDataPeriod>(skillDay.SkillDataPeriodCollection));
			skillDay.MergeSkillDataPeriods(new List<ISkillDataPeriod>(skillDay.SkillDataPeriodCollection));
			skillDay.SplitSkillDataPeriods(new List<ISkillDataPeriod>(skillDay.SkillDataPeriodCollection));

			Assert.AreEqual(96, skillDay.SkillDataPeriodCollection.Count);

			PersistAndRemoveFromUnitOfWork(skillDay);

			IRepository<ISkillDay> skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			skillDay = skillDayRepository.Get(skillDay.Id.Value);
			skillDay.SetupSkillDay();

			Assert.AreEqual(96, skillDay.SkillDataPeriodCollection.Count);

			skillDay.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> {skillDay},
				new DateOnlyPeriod());
			skillDay.MergeSkillDataPeriods(new List<ISkillDataPeriod>(skillDay.SkillDataPeriodCollection));
			PersistAndRemoveFromUnitOfWork(skillDay);

			skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			skillDay = skillDayRepository.Get(skillDay.Id.Value);
			skillDay.SetupSkillDay();

			Assert.AreEqual(1, skillDay.SkillDataPeriodCollection.Count);
		}

		[Test]
		public void VerifyIntervalsAreRemovedWhenSplitting()
		{
			ISkillDay skillDay = CreateAggregateWithCorrectBusinessUnit();
			skillDay.SetupSkillDay();
			skillDay.SkillDayCalculator = new SkillDayCalculator(_skill, new List<ISkillDay> {skillDay},
				new DateOnlyPeriod());

			skillDay.SplitSkillDataPeriods(new List<ISkillDataPeriod>(skillDay.SkillDataPeriodCollection));
			SkillDayRepository.DONT_USE_CTOR(UnitOfWork).Add(skillDay);

			var skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			skillDay = skillDayRepository.Get(skillDay.Id.Value);

			Assert.AreEqual(96, skillDay.SkillDataPeriodCollection.Count);
		}

		private static SkillDay createSkillDay(DateOnly skillDate, IWorkload workload, ISkill skill, IScenario scenario)
		{
			IList<TimePeriod> openHourPeriods = new List<TimePeriod>();
			openHourPeriods.Add(new TimePeriod("12:30-17:30"));

			WorkloadDay workloadDay = new WorkloadDay();
			workloadDay.Create(skillDate, workload, openHourPeriods);
			workloadDay.Tasks = 7*20;
			workloadDay.AverageTaskTime = TimeSpan.FromSeconds(22);
			workloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(233);

			IList<ISkillDataPeriod> skillDataPeriods = new List<ISkillDataPeriod>();
			skillDataPeriods.Add(
				new SkillDataPeriod(
					ServiceAgreement.DefaultValues(),
					new SkillPersonData(2, 5),
					DateTimeFactory.CreateDateTimePeriod(DateTime.SpecifyKind(skillDate.Date, DateTimeKind.Utc), 1)
						.ChangeEndTime(TimeSpan.FromHours(-12))));
			skillDataPeriods.Add(
				new SkillDataPeriod(
					skillDataPeriods[0].ServiceAgreement,
					skillDataPeriods[0].SkillPersonData,
					skillDataPeriods[0].Period.ChangeStartTime(TimeSpan.FromHours(12))));

			SkillDay skillDay = new SkillDay(skillDate, skill, scenario,
				new List<IWorkloadDay> {workloadDay}, skillDataPeriods);

			return skillDay;
		}

		[Test]
		public void ShouldCreateSkillDayWithCurrentDateAsDateAndNoTime()
		{
			_skill = SkillFactory.CreateSiteSkill("bbb");
			SkillDayRepository skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			var startDateTime = new DateOnly(2011, 4, 1);
			DateOnlyPeriod period = new DateOnlyPeriod(startDateTime, startDateTime.AddDays(1));
			ICollection<ISkillDay> skilldays = skillDayRepository.GetAllSkillDays(period, new Collection<ISkillDay>(),
				_skill, _scenario, _ => { });
			Assert.AreEqual(2, skilldays.Count);
			ISkillDay skillDay1 = skilldays.FirstOrDefault();
			Assert.AreEqual(new DateOnly(2011, 4, 1), skillDay1.CurrentDate);
		}

		[Test]
		public void ShouldCreateWorkloadDayForNewlyAddedWorkload()
		{
			_skill = SkillFactory.CreateSiteSkill("bbb");
			SkillDayRepository skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			var startDateTime = new DateOnly(2011, 4, 1);
			DateOnlyPeriod period = new DateOnlyPeriod(startDateTime, startDateTime);
			ICollection<ISkillDay> skilldays = skillDayRepository.GetAllSkillDays(period, new Collection<ISkillDay>(),
				_skill, _scenario, _ => { }).ToArray();

			skilldays.First().WorkloadDayCollection.Count.Should().Be.EqualTo(0);
			_skill.AddWorkload(WorkloadFactory.CreateWorkload("New WL", _skill));

			skilldays = skillDayRepository.GetAllSkillDays(period, skilldays, _skill, _scenario, _ => { });
			skilldays.First().WorkloadDayCollection.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnFalseIfNoSkillDays()
		{
			var skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			var businessUnitRepository = BusinessUnitRepository.DONT_USE_CTOR(UnitOfWork);
			var bu = businessUnitRepository.LoadAll().First();
			skillDayRepository.HasSkillDaysWithinPeriod(DateOnly.MinValue, DateOnly.MaxValue, bu, _scenario).Should().Be.False();
		}

		[Test]
		public void ShouldReturnTrueIfSkillDays()
		{
			var skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			var businessUnitRepository = BusinessUnitRepository.DONT_USE_CTOR(UnitOfWork);

			ISkillDay skillDay = CreateAggregateWithCorrectBusinessUnit();
			ISkillDay skillDay2 = CreateAggregateWithCorrectBusinessUnit();
			SkillDayCalculator calc = new SkillDayCalculator(skillDay2.Skill, new List<ISkillDay> { skillDay },
				new DateOnlyPeriod(2009, 1, 1, 2009, 12, 31));
			skillDay2.SkillDayCalculator = calc;
			PersistAndRemoveFromUnitOfWork(skillDay);
			PersistAndRemoveFromUnitOfWork(skillDay2);

			var bu = businessUnitRepository.LoadAll().First();
			skillDayRepository.HasSkillDaysWithinPeriod(DateOnly.MinValue, DateOnly.MaxValue, bu, _scenario).Should().Be.True();
		}

		[Test]
		public void ShouldLoadSkillDaysByGivenIds()
		{
			ISkillDay skillDay1 = CreateAggregateWithCorrectBusinessUnit();
			ISkillDay skillDay2 = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(skillDay1);
			PersistAndRemoveFromUnitOfWork(skillDay2);

			SkillDayRepository skillDayRepository = SkillDayRepository.DONT_USE_CTOR(UnitOfWork);
			ICollection<ISkillDay> skillDays =
				skillDayRepository.LoadSkillDays(new List<Guid> { skillDay1.Id.Value, skillDay2.Id.Value });

			Assert.AreEqual(2, skillDays.Count);
			skillDays.Any(s => s.CurrentDate == skillDay1.CurrentDate);
			skillDays.Any(s => s.CurrentDate == skillDay2.CurrentDate);
		}

		protected override Repository<ISkillDay> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return SkillDayRepository.DONT_USE_CTOR_asdasd(currentUnitOfWork);
		}
	}
}