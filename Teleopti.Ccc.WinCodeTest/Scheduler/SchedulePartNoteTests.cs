using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[DomainTest]
	public class SchedulePartNoteTests : ISetup
	{
		public ISchedulerStateHolder StateHolder;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldUpdateNoteWithoutUndoRedoContainer()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var scenario = new Scenario("Default").WithId();
			
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson().WithId(),
				new DateOnly(2014, 3, 1));
			var personPeriod = person.Period(new DateOnly(2014, 3, 1));
			personPeriod.Team.Site = SiteFactory.CreateSimpleSite();

			var dictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2014, 3, 22, 2014, 4, 4));
			dictionary.UsePermissions(false);

			StateHolder.SetRequestedScenario(scenario);
			StateHolder.SchedulingResultState.Schedules = dictionary;
			StateHolder.SchedulingResultState.UseValidation = true;

			var overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();
			var fakeResponseHandler = new FakeBusinessRulesResponseHandler();
			var view = new FakeScheduleView().WithBusinessRuleResponse(fakeResponseHandler);

			var target = new SchedulePresenterBase(view, StateHolder, new GridlockManager(), new ClipHandler<IScheduleDay>(),
				SchedulePartFilter.None, overriddenBusinessRulesHolder, new DoNothingScheduleDayChangeCallBack(),
				NullScheduleTag.Instance);

			var dayToChange = StateHolder.Schedules[person].ScheduledDay(new DateOnly(2014, 3, 25));
			
			target.LastUnsavedSchedulePart = dayToChange;
			dayToChange.CreateAndAddNote("test");

			target.UpdateNoteFromEditor();

			StateHolder.Schedules[person].ScheduledDay(new DateOnly(2014, 3, 25))
				.NoteCollection()
				.Single()
				.GetScheduleNote(new NoFormatting())
				.Should()
				.Be.EqualTo("test");
		}

		[Test]
		public void ShouldUpdatePublicNoteWithoutUndoRedoContainer()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var scenario = new Scenario("Default").WithId();

			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson().WithId(),
				new DateOnly(2014, 3, 1));
			var personPeriod = person.Period(new DateOnly(2014, 3, 1));
			personPeriod.Team.Site = SiteFactory.CreateSimpleSite();

			var dictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2014, 3, 22, 2014, 4, 4));
			dictionary.UsePermissions(false);

			StateHolder.SetRequestedScenario(scenario);
			StateHolder.SchedulingResultState.Schedules = dictionary;
			StateHolder.SchedulingResultState.UseValidation = true;

			var overriddenBusinessRulesHolder = new OverriddenBusinessRulesHolder();
			var fakeResponseHandler = new FakeBusinessRulesResponseHandler();
			var view = new FakeScheduleView().WithBusinessRuleResponse(fakeResponseHandler);

			var target = new SchedulePresenterBase(view, StateHolder, new GridlockManager(), new ClipHandler<IScheduleDay>(),
				SchedulePartFilter.None, overriddenBusinessRulesHolder, new DoNothingScheduleDayChangeCallBack(),
				NullScheduleTag.Instance);

			var dayToChange = StateHolder.Schedules[person].ScheduledDay(new DateOnly(2014, 3, 25));

			target.LastUnsavedSchedulePart = dayToChange;
			dayToChange.CreateAndAddPublicNote("test");

			target.UpdatePublicNoteFromEditor();

			StateHolder.Schedules[person].ScheduledDay(new DateOnly(2014, 3, 25))
				.PublicNoteCollection()
				.Single()
				.GetScheduleNote(new NoFormatting())
				.Should()
				.Be.EqualTo("test");
		}
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<DesktopOptimizationContext>().For<IFillSchedulerStateHolder>();
		}
	}
}