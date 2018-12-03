using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.SaveSchedulePart
{
	[TestFixture]
	public class SaveSchedulePartServiceTest
	{
		private IScheduleDifferenceSaver scheduleDifferenceSaver;
		private ISaveSchedulePartService target;
		private IPersonAbsenceAccountRepository personAbsenceAccountRepository;

		[SetUp]
		public void Setup()
		{
			scheduleDifferenceSaver = MockRepository.GenerateMock<IScheduleDifferenceSaver>();
			personAbsenceAccountRepository = MockRepository.GenerateMock<IPersonAbsenceAccountRepository>();
			target = new SaveSchedulePartService(scheduleDifferenceSaver, personAbsenceAccountRepository, new DoNothingScheduleDayChangeCallBack());
		}

		[Test]
		public void ShouldSaveSchedulePart()
		{
			var period = new DateOnlyAsDateTimePeriod(new DateOnly(2017, 5, 3), TimeZoneInfo.Utc);
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var differenceCollectionItems = MockRepository.GenerateMock<IDifferenceCollection<IPersistableScheduleData>>();
			var response = new List<IBusinessRuleResponse>();
			var dictionary = MockRepository.GenerateMock<IReadOnlyScheduleDictionary>();

			dictionary.Stub(x => x.DifferenceSinceSnapshot()).Return(differenceCollectionItems);
			scheduleDay.Stub(x => x.Owner).Return(dictionary);
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(period);
			dictionary.Stub(x => x.ModifiedPersonAccounts).Return(new List<IPersonAbsenceAccount>());
			dictionary.Stub(x => x.Modify(ScheduleModifier.Scheduler, new[] { scheduleDay }, null, null, null)).IgnoreArguments().Return(response);
			scheduleDifferenceSaver.Stub(x => x.SaveChanges(differenceCollectionItems, null)).IgnoreArguments();
			dictionary.Stub(x => x.MakeEditable());

			target.Save(scheduleDay, null, new ScheduleTag());
		}

		[Test]
		public void ShouldReturnErrorMessage()
		{
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var response = new List<IBusinessRuleResponse> { MockRepository.GenerateMock<IBusinessRuleResponse>() };
			var dictionary = MockRepository.GenerateStrictMock<IReadOnlyScheduleDictionary>();

			scheduleDay.Stub(x => x.Owner).Return(dictionary);
			scheduleDay.Stub(x => x.DateOnlyAsPeriod)
				.Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfo.Utc));
			dictionary.Stub(x => x.Modify(ScheduleModifier.Scheduler, new[] { scheduleDay }, null, null, null)).IgnoreArguments().Return(response);
			dictionary.Stub(x => x.MakeEditable());

			var result = target.Save(scheduleDay, null, new ScheduleTag());
			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldNotThrowBusinessRuleExceptionOnBrokenBusinessRulesWhenOverriden()
		{
			var period = new DateOnlyAsDateTimePeriod(new DateOnly(2017, 5, 3), TimeZoneInfo.Utc);
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var businessRuleResponse = MockRepository.GenerateMock<IBusinessRuleResponse>();
			var response = new List<IBusinessRuleResponse> { businessRuleResponse };
			var dictionary = MockRepository.GenerateMock<IReadOnlyScheduleDictionary>();
			var differenceCollectionItems = MockRepository.GenerateMock<IDifferenceCollection<IPersistableScheduleData>>();

			businessRuleResponse.Stub(x => x.Overridden).Return(true);
			scheduleDay.Stub(x => x.Owner).Return(dictionary);
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(period);
			dictionary.Stub(x => x.Modify(ScheduleModifier.Scheduler, new[] { scheduleDay }, null, null, null)).IgnoreArguments().Return(response);
			dictionary.Stub(x => x.MakeEditable());
			dictionary.Stub(x => x.ModifiedPersonAccounts).Return(new List<IPersonAbsenceAccount>());
			dictionary.Stub(x => x.DifferenceSinceSnapshot()).Return(differenceCollectionItems);
			scheduleDifferenceSaver.Stub(x => x.SaveChanges(differenceCollectionItems, null)).IgnoreArguments();

			target.Save(scheduleDay, null, new ScheduleTag());
		}
	}

	[DomainTest]
	public class SaveSchedulePartServiceNoMocksTest: IIsolateSystem
	{
		public ISaveSchedulePartService Target;
		public FakeScenarioRepository ScenarioRepository;
		public FakeActivityRepository ActivityRepository;
		public FakeSkillRepository SkillRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public FakeIntervalLengthFetcher IntervalLengthFetcher;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public MutableNow Now;
		public IScheduleStorage ScheduleStorage;
		public ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory;
		public FakeRepositoryFactory RepositoryFactory;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<ScheduleDayDifferenceSaver>().For<IScheduleDayDifferenceSaver>();
			isolate.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
		}

		[Test]
		public void ShouldSaveDeltasIfDifferentAbsenceAfter()
		{
			const int resolution = 60;
			IntervalLengthFetcher.Has(resolution);
			var now = new DateTime(2017, 5, 1, 0, 0, 0).Utc();
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = resolution;
			var person = PersonRepository.Has(skill);

			var assignmentPeriod1 = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 10);
			var absencePeriod = new DateTimePeriod(2017, 5, 1, 9, 2017, 5, 1, 10);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod1, new ShiftCategory("category")).WithId();
			PersonAssignmentRepository.Add(assignment);
			var dic = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario);
			var day = dic[person].ScheduledDay(new DateOnly(2017, 5, 1));
			day.CreateAndAddAbsence(new AbsenceLayer(new Absence(), absencePeriod));
			
			Target.Save(day, new FakeNewBusinessRuleCollection(),
				new NullScheduleTag());
			var resources = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 10)).ToList();
			resources.Count.Should().Be.EqualTo(1);
			resources.First().Resource.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldSaveDeltasIfRemovedAbsenceAfter()
		{
			const int resolution = 60;
			IntervalLengthFetcher.Has(resolution);
			var now = new DateTime(2017, 5, 1, 0, 0, 0).Utc();
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = resolution;
			var person = PersonRepository.Has(skill);

			var assignmentPeriod1 = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 10);
			var absencePeriod = new DateTimePeriod(2017, 5, 1, 9, 2017, 5, 1, 10);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod1, new ShiftCategory("category")).WithId();
			PersonAssignmentRepository.Add(assignment);
			var personAbs = new PersonAbsence(person,scenario, new AbsenceLayer(new Absence(), absencePeriod)).WithId();
			PersonAbsenceRepository.Add(personAbs);
			var dic = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario);
			var day = dic[person].ScheduledDay(new DateOnly(2017, 5, 1));
			day.Remove(personAbs);
			
			Target.Save(day, new FakeNewBusinessRuleCollection(),
				new NullScheduleTag());
			var resources = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 10)).ToList();
			resources.Count.Should().Be.EqualTo(1);
			resources.First().Resource.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldSaveDeltasIfAddedActivity()
		{
			const int resolution = 60;
			IntervalLengthFetcher.Has(resolution);
			var now = new DateTime(2017, 5, 1, 0, 0, 0, DateTimeKind.Utc);
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = resolution;
			var person = PersonRepository.Has(skill);

			var assignmentPeriod1 = new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 10);
			var assignmentPeriod2 = new DateTimePeriod(2017, 5, 1, 10, 2017, 5, 1, 12);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod1, new ShiftCategory("category")).WithId();
			PersonAssignmentRepository.Add(assignment);
			var dic = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario);
			var day = dic[person].ScheduledDay(new DateOnly(2017, 5, 1));
			day.CreateAndAddActivity(activity, assignmentPeriod2);

			Target.Save(day, new FakeNewBusinessRuleCollection(),
				new NullScheduleTag());
			var resources = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(2017, 5, 1, 8, 2017, 5, 1, 12)).ToList();
			resources.Count.Should().Be.EqualTo(2);
			resources.First().Resource.Should().Be.EqualTo(1);
		}

		[Test, Ignore("Needs to know the setting used")]
		public void ShouldOnlyPersistDeltasForReadModelPeriod()
		{
			const int resolution = 60;
			IntervalLengthFetcher.Has(resolution);
			var now = new DateTime(2017, 5, 1, 0, 0, 0).Utc();
			Now.Is(now);

			var scenario = ScenarioRepository.Has("scenario");
			var activity = ActivityRepository.Has("activity");
			var skill = SkillRepository.Has("skill", activity).WithId();
			skill.DefaultResolution = resolution;
			var person = PersonRepository.Has(skill);

			var assignmentPeriod1 = new DateTimePeriod(2017, 5, 20, 8, 2017, 5, 20, 10);
			var absencePeriod = new DateTimePeriod(2017, 5, 20, 9, 2017, 5, 20, 10);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, assignmentPeriod1, new ShiftCategory("category")).WithId();
			PersonAssignmentRepository.Add(assignment);
			var dic = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
																			  new ScheduleDictionaryLoadOptions(false, false), assignmentPeriod1, scenario);
			var day = dic[person].ScheduledDay(new DateOnly(2017, 5, 20));
			day.CreateAndAddAbsence(new AbsenceLayer(new Absence(), absencePeriod));
			((IReadOnlyScheduleDictionary)dic).MakeEditable();
			dic.Modify(day, new FakeNewBusinessRuleCollection());

			Target.Save(day, new FakeNewBusinessRuleCollection(),
						new NullScheduleTag());
			var resources = SkillCombinationResourceRepository.LoadSkillCombinationResources(new DateTimePeriod(2017, 5, 20, 8, 2017, 5, 20, 10)).ToList();
			resources.Count.Should().Be.EqualTo(0);
		}
	}
}
