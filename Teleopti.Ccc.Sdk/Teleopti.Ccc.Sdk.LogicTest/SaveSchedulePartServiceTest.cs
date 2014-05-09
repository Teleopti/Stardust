using System.Collections.Generic;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest
{
	[TestFixture]
	public class SaveSchedulePartServiceTest
	{
		private MockRepository mocks;
		private IScheduleDifferenceSaver scheduleDictionarySaver;
		private ISaveSchedulePartService target;
		private IPersonAbsenceAccountRepository personAbsenceAccountRepository;

	    [SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			scheduleDictionarySaver = mocks.DynamicMock<IScheduleDifferenceSaver>();
			personAbsenceAccountRepository = mocks.DynamicMock<IPersonAbsenceAccountRepository>();
			target = new SaveSchedulePartService(scheduleDictionarySaver, personAbsenceAccountRepository);
		}

		[Test]
		public void ShouldSaveSchedulePart()
		{
			var scheduleDay = mocks.DynamicMock<IScheduleDay>();
			var differenceCollectionItems = mocks.DynamicMock<IDifferenceCollection<IPersistableScheduleData>>();
			var response = new List<IBusinessRuleResponse>();
			var dictionary = mocks.DynamicMock<IReadOnlyScheduleDictionary>();

			using (mocks.Record())
			{
				Expect.Call(dictionary.DifferenceSinceSnapshot()).Return(differenceCollectionItems);
				Expect.Call(scheduleDay.Owner).Return(dictionary);
				Expect.Call(dictionary.Modify(ScheduleModifier.Scheduler, scheduleDay, null, null, null)).IgnoreArguments().Return(response);
				Expect.Call(() => scheduleDictionarySaver.SaveChanges(differenceCollectionItems, null)).IgnoreArguments();
				Expect.Call(dictionary.MakeEditable);
			}
			using (mocks.Playback())
			{
				target.Save(scheduleDay,null, new ScheduleTag());
			}
		}

		[Test]
		public void ShouldThrowFaultExceptionOnBrokenBusinessRules()
		{
			var scheduleDay = mocks.DynamicMock<IScheduleDay>();
			var response = new List<IBusinessRuleResponse>{mocks.DynamicMock<IBusinessRuleResponse>()};
			var dictionary = mocks.StrictMock<IReadOnlyScheduleDictionary>();

			using (mocks.Record())
			{
				Expect.Call(scheduleDay.Owner).Return(dictionary);
				Expect.Call(dictionary.Modify(ScheduleModifier.Scheduler, scheduleDay, null, null, null)).IgnoreArguments().Return(response);
				Expect.Call(dictionary.MakeEditable);
			}
			using (mocks.Playback())
			{
				Assert.Throws<FaultException>(()=>target.Save(scheduleDay,null, new ScheduleTag()));
			}
		}

		[Test]
		public void ShouldNotThrowFaultExceptionOnBrokenBusinessRulesWhenOverriden()
		{
			var scheduleDay = mocks.DynamicMock<IScheduleDay>();
			var businessRuleResponse = mocks.DynamicMock<IBusinessRuleResponse>();
			var response = new List<IBusinessRuleResponse> { businessRuleResponse };
			var dictionary = mocks.DynamicMock<IReadOnlyScheduleDictionary>();
			var differenceCollectionItems = mocks.DynamicMock<IDifferenceCollection<IPersistableScheduleData>>();

			using (mocks.Record())
			{
				Expect.Call(businessRuleResponse.Overridden).Return(true);
				Expect.Call(scheduleDay.Owner).Return(dictionary);
				Expect.Call(dictionary.Modify(ScheduleModifier.Scheduler, scheduleDay, null, null, null)).IgnoreArguments().Return(response);
				Expect.Call(dictionary.MakeEditable);

				Expect.Call(dictionary.DifferenceSinceSnapshot()).Return(differenceCollectionItems);
				Expect.Call(() => scheduleDictionarySaver.SaveChanges(differenceCollectionItems, null)).IgnoreArguments();
			}
			using (mocks.Playback())
			{
				target.Save(scheduleDay, null, new ScheduleTag());
			}
		}
	}
}
