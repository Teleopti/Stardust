using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Text=Rhino.Mocks.Constraints.Text;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class ExportToScenarioResultTest
	{
		private IExportToScenarioResultView view;
		private MockRepository mocks;
		private Presenter target;
		private IScheduleRepository scheduleRepository;
		private IMoveDataBetweenSchedules moveSvc;
		private IScenario exportScenario;
		private IScenario orginalScenario;
		private IList<IScheduleDay> partsToMove;
		private IScheduleDictionaryBatchPersister scheduleDictionaryBatchPersister;
		private IUnitOfWorkFactory uowFactory;
		private List<IPerson> persons;
		private IReassociateDataForSchedules callback;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			view = mocks.StrictMock<IExportToScenarioResultView>();
			scheduleRepository = mocks.StrictMock<IScheduleRepository>();
			moveSvc = mocks.StrictMock<IMoveDataBetweenSchedules>();
			uowFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			scheduleDictionaryBatchPersister = mocks.DynamicMock<IScheduleDictionaryBatchPersister>();
			callback = mocks.DynamicMock<IReassociateDataForSchedules>();
			partsToMove = new List<IScheduleDay>();
			persons = new List<IPerson>();
			exportScenario = new Scenario("export");
			orginalScenario = new Scenario("original");
			target = new Presenter(uowFactory, view, scheduleRepository, moveSvc, callback, persons, partsToMove, exportScenario, scheduleDictionaryBatchPersister);
		}

		[Test]
		public void VerifyInitializeWhenCorrectPersons()
		{
			var person = new Person();
			partsToMove.Add(createDummyPart(person));
			persons.Add(person);
			var dictionaryToExportTo = mocks.StrictMock<IScheduleDictionary>();
			var warnings = threeRuleResponsesTwoWithSameMessageAndPerson();
			using (mocks.Record())
			{
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(null);
				callback.ReassociateDataForAllPeople();
				Expect.Call(scheduleRepository.FindSchedulesForPersons(null, null, null, null, null)).IgnoreArguments()
					.Return(dictionaryToExportTo);
				LastCall.IgnoreArguments();
				Expect.Call(moveSvc.CopySchedulePartsToAnotherDictionary(dictionaryToExportTo, partsToMove)).Return(warnings);
				verifyScenarioText();
				verifyAgentText("1");
				verifyWarningsAreSetWithArrayCount();
			}
			using (mocks.Playback())
			{
				target.Initialize();
			}
			Assert.AreSame(dictionaryToExportTo, target.ScheduleDictionaryToPersist);
		}

		[Test]
		public void VerifyInitializeWhenNoWarnings()
		{
			var person = new Person();
			partsToMove.Add(createDummyPart(person));
			persons.Add(person);
			var dictionaryToExportTo = mocks.StrictMock<IScheduleDictionary>();
			var warnings = new List<IBusinessRuleResponse>();
			using (mocks.Record())
			{
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(null);
				callback.ReassociateDataForAllPeople();
				Expect.Call(scheduleRepository.FindSchedulesForPersons(null, null, null, null, null)).Return(dictionaryToExportTo);
				LastCall.IgnoreArguments();
				Expect.Call(moveSvc.CopySchedulePartsToAnotherDictionary(dictionaryToExportTo, partsToMove)).Return(warnings);
				verifyScenarioText();
				verifyAgentText("1");
				view.DisableBodyText();
			}
			using (mocks.Playback())
			{
				target.Initialize();
			}
			Assert.AreSame(dictionaryToExportTo, target.ScheduleDictionaryToPersist);
		}

		[Test]
		public void VerifyDoNothingIfNoScheduleParts()
		{
			using (mocks.Record())
			{
				view.CloseForm();
			}
			using (mocks.Playback())
			{
				target.Initialize();
			}
		}

		[Test]
		public void VerifyInitializeWhenPersonIsMissingForSchedule()
		{
			partsToMove.Add(createDummyPart(new Person()));
			Assert.Throws<ArgumentException>(target.Initialize); //annat exception här?
		}

		[Test]
		public void VerifyCancel()
		{
			using (mocks.Record())
			{
				view.CloseForm();
			}
			using (mocks.Playback())
			{
				target.OnCancel();
			}
		}

		[Test]
		public void VerifyPersistAndClose()
		{
			var uow = mocks.DynamicMock<IUnitOfWork>();
			var dic = new ScheduleDictionary(orginalScenario, new ScheduleDateTimePeriod(new DateTimePeriod()));
			target.SetPersistingDic(dic);
			using (mocks.Record())
			{
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(uow).Repeat.Any();
				scheduleDictionaryBatchPersister.Persist(target.ScheduleDictionaryToPersist);
				view.CloseForm();
			}
			using (mocks.Playback())
			{
				target.OnConfirm();
			}
		}

        [Test]
        public void ShouldCallViewToShowErrorOnDataSourceException()
        {
            var dic = new ScheduleDictionary(orginalScenario, new ScheduleDateTimePeriod(new DateTimePeriod()));
            target.SetPersistingDic(dic);
            var err = new DataSourceException();

            Expect.Call(() => scheduleDictionaryBatchPersister.Persist(target.ScheduleDictionaryToPersist)).Throw(err);
            Expect.Call(() => view.ShowDataSourceException(err));
            Expect.Call(() => view.CloseForm());
            mocks.ReplayAll();
            target.OnConfirm();
            mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallViewToShowErrorOnDataSourceExceptionInInit()
        {
            var person = new Person();
            partsToMove.Add(createDummyPart(person));
            persons.Add(person);
            var err = new DataSourceException();
            Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(null);
            Expect.Call(() => callback.ReassociateDataForAllPeople()).Throw(err);
            Expect.Call(() => view.ShowDataSourceException(err));
            Expect.Call(() => view.CloseForm());
            mocks.ReplayAll();

            target.Initialize();
            
            mocks.VerifyAll();
        }

		[Test]
		public void ExportToScenarioEqualsOperators()
		{
			var data1 = new ExportToScenarioWarningData("dfsf", "sdfggg");
			var data2 = new ExportToScenarioWarningData("dfsf", "sdfggsdfsdfg");
			var data3 = new ExportToScenarioWarningData("dfsf", "sdfggsdfsdfg");

			Assert.IsFalse(data1 == data2);
			Assert.IsTrue(data3 == data2);
			Assert.IsFalse(data3 != data2);
			Assert.IsFalse(data3.Equals(new object()));
		}

		private void verifyAgentText(string textToSearchFor)
		{
			view.SetAgentText(null);
			LastCall.Constraints(Text.Contains(textToSearchFor));
		}

		private void verifyWarningsAreSetWithArrayCount()
		{
			view.SetWarningText(null);
			LastCall.IgnoreArguments(); // I trust the guys impl business rules
		}

		private void verifyScenarioText()
		{
			view.SetScenarioText(null);
			LastCall.Constraints(Text.Contains(orginalScenario.Description.Name) && Text.Contains(orginalScenario.Description.Name));
		}

		private IScheduleDay createDummyPart(IPerson person)
		{
			var tempDictionary = new ScheduleDictionaryForTest(orginalScenario,
															   new ScheduleDateTimePeriod(new DateTimePeriod(2000, 1, 1,
																											 2000, 1, 2)),
															   new Dictionary<IPerson, IScheduleRange>());
			return ExtractedSchedule.CreateScheduleDay(tempDictionary, person, new DateOnly(2000,1,1));
		}

		private static IEnumerable<IBusinessRuleResponse> threeRuleResponsesTwoWithSameMessageAndPerson()
		{
			IPerson sharedPerson = new Person { Name = new Name("Roger", "Moore") };
			const string sharedMessage = "EnGång";
			return new List<IBusinessRuleResponse>
					   {
						   new BusinessRuleResponse(typeof (int), sharedMessage, false, false, new DateTimePeriod(), sharedPerson, new DateOnlyPeriod(new DateOnly(),new DateOnly() )),
						   new BusinessRuleResponse(typeof (object), sharedMessage, true, true, new DateTimePeriod(), sharedPerson, new DateOnlyPeriod(new DateOnly(),new DateOnly() )),
						   new BusinessRuleResponse(typeof (object), "unik", true, true, new DateTimePeriod(), sharedPerson, new DateOnlyPeriod(new DateOnly(),new DateOnly() ))
					   };
		}

		private class Presenter : ExportToScenarioResultPresenter
		{
			public Presenter(IUnitOfWorkFactory uowFactory, IExportToScenarioResultView view, IScheduleRepository scheduleRepository, IMoveDataBetweenSchedules moveSchedules, IReassociateDataForSchedules callback, IEnumerable<IPerson> persons, IEnumerable<IScheduleDay> schedulePartsToExport, IScenario exportScenario, IScheduleDictionaryBatchPersister scheduleDictionaryBatchPersister)
				: base(uowFactory, view, scheduleRepository, moveSchedules, callback, persons, schedulePartsToExport, exportScenario, scheduleDictionaryBatchPersister)
			{
			}

			public void SetPersistingDic(IScheduleDictionary dic)
			{
				SetScheduleDictionaryToPersist(dic);
			}
		}
	}
}