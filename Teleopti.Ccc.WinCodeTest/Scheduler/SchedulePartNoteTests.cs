using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[DomainTest, UseIocForFatClient]
	public class SchedulePartNoteTests
	{
		public ISchedulerStateHolder StateHolder;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldUpdateNoteWithoutUndoRedoContainer()
		{
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
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
				NullScheduleTag.Instance, new UndoRedoContainer());

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
			BusinessUnitRepository.Has(ServiceLocator_DONTUSE.CurrentBusinessUnit.Current());
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
				NullScheduleTag.Instance, new UndoRedoContainer());

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
	}
}