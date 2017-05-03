using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SaveSchedulePart;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.SaveSchedulePart
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
			target = new SaveSchedulePartService(scheduleDictionarySaver, personAbsenceAccountRepository, new DoNothingScheduleDayChangeCallBack(), new EmptyScheduleDayDifferenceSaver());
		}

		[Test]
		public void ShouldSaveSchedulePart()
		{
			var period = new DateOnlyAsDateTimePeriod(new DateOnly(2017, 5, 3), TimeZoneInfo.Utc);
			var scheduleDay = mocks.DynamicMock<IScheduleDay>();
			var differenceCollectionItems = mocks.DynamicMock<IDifferenceCollection<IPersistableScheduleData>>();
			var response = new List<IBusinessRuleResponse>();
			var dictionary = mocks.DynamicMock<IReadOnlyScheduleDictionary>();

			using (mocks.Record())
			{
				Expect.Call(dictionary.DifferenceSinceSnapshot()).Return(differenceCollectionItems);
				Expect.Call(scheduleDay.Owner).Return(dictionary);
				Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(period);
				Expect.Call(dictionary.ModifiedPersonAccounts).Return(new List<IPersonAbsenceAccount>());
				Expect.Call(dictionary.Modify(ScheduleModifier.Scheduler, new [] { scheduleDay }, null, null, null)).IgnoreArguments().Return(response);
				Expect.Call(() => scheduleDictionarySaver.SaveChanges(differenceCollectionItems, null)).IgnoreArguments();
				Expect.Call(dictionary.MakeEditable);
			}
			using (mocks.Playback())
			{
				target.Save(scheduleDay, null, new ScheduleTag());
			}
		}

		[Test]
		public void ShouldReturnErrorMessage()
		{
			var scheduleDay = mocks.DynamicMock<IScheduleDay>();
			var response = new List<IBusinessRuleResponse> { mocks.DynamicMock<IBusinessRuleResponse>() };
			var dictionary = mocks.StrictMock<IReadOnlyScheduleDictionary>();

			using (mocks.Record())
			{
				Expect.Call(scheduleDay.Owner).Return(dictionary);
				Expect.Call(dictionary.Modify(ScheduleModifier.Scheduler, new [] { scheduleDay }, null, null, null)).IgnoreArguments().Return(response);
				Expect.Call(dictionary.MakeEditable);
			}
			using (mocks.Playback())
			{
				var result = target.Save(scheduleDay, null, new ScheduleTag());
				result.Should().Not.Be.Null();
			}
		}

		[Test]
		public void ShouldNotThrowBusinessRuleExceptionOnBrokenBusinessRulesWhenOverriden()
		{
			var period = new DateOnlyAsDateTimePeriod(new DateOnly(2017, 5, 3), TimeZoneInfo.Utc);
			var scheduleDay = mocks.DynamicMock<IScheduleDay>();
			var businessRuleResponse = mocks.DynamicMock<IBusinessRuleResponse>();
			var response = new List<IBusinessRuleResponse> { businessRuleResponse };
			var dictionary = mocks.DynamicMock<IReadOnlyScheduleDictionary>();
			var differenceCollectionItems = mocks.DynamicMock<IDifferenceCollection<IPersistableScheduleData>>();

			using (mocks.Record())
			{
				Expect.Call(businessRuleResponse.Overridden).Return(true);
				Expect.Call(scheduleDay.Owner).Return(dictionary);
				Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(period);
				Expect.Call(dictionary.Modify(ScheduleModifier.Scheduler, new[] { scheduleDay }, null, null, null)).IgnoreArguments().Return(response);
				Expect.Call(dictionary.MakeEditable);
				Expect.Call(dictionary.ModifiedPersonAccounts).Return(new List<IPersonAbsenceAccount>());
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
