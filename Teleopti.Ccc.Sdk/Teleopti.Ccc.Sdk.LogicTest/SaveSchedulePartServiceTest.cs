using System.Collections.Generic;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest
{
	[TestFixture]
	public class SaveSchedulePartServiceTest
	{
		private MockRepository mocks;
		private IScheduleDictionarySaver scheduleDictionarySaver;
		private IScheduleRepository scheduleRepository;
		private ISaveSchedulePartService target;
		private IPersonAbsenceAccountRepository personAbsenceAccountRepository;
	    private ICurrentUnitOfWork currentUnitOfWork;

	    [SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			scheduleDictionarySaver = mocks.DynamicMock<IScheduleDictionarySaver>();
			scheduleRepository = mocks.DynamicMock<IScheduleRepository>();
			personAbsenceAccountRepository = mocks.DynamicMock<IPersonAbsenceAccountRepository>();
			currentUnitOfWork = mocks.DynamicMock<ICurrentUnitOfWork>();
			target = new SaveSchedulePartService(scheduleDictionarySaver, scheduleRepository, personAbsenceAccountRepository, currentUnitOfWork);
		}

		[Test]
		public void ShouldSaveSchedulePart()
		{
			var unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			var scheduleDay = mocks.DynamicMock<IScheduleDay>();
			var differenceCollectionItems = mocks.DynamicMock<IDifferenceCollection<IPersistableScheduleData>>();
			var response = new List<IBusinessRuleResponse>();
			var dictionary = mocks.DynamicMock<IReadOnlyScheduleDictionary>();

			using (mocks.Record())
			{
				Expect.Call(currentUnitOfWork.Current()).Return(unitOfWork);
				Expect.Call(dictionary.DifferenceSinceSnapshot()).Return(differenceCollectionItems);
				Expect.Call(scheduleDay.Owner).Return(dictionary);
				Expect.Call(dictionary.Modify(ScheduleModifier.Scheduler, scheduleDay, null, null, null)).IgnoreArguments().Return(response);
				Expect.Call(scheduleDictionarySaver.MarkForPersist(unitOfWork, scheduleRepository, differenceCollectionItems)).Return(null);
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
	}
}
