using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Text = Rhino.Mocks.Constraints.Text;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class ExportToScenarioResultTest
	{
		private IExportToScenarioResultView view;
		private MockRepository mocks;
		private Presenter target;
		private IScheduleStorage scheduleStorage;
		private IMoveDataBetweenSchedules moveSvc;
		private IScenario exportScenario;
		private IScenario orginalScenario;
		private IList<IScheduleDay> partsToMove;
		private IScheduleDictionaryPersister scheduleDictionaryBatchPersister;
		private IUnitOfWorkFactory uowFactory;
		private List<IPerson> persons;
		private IReassociateDataForSchedules callback;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			view = mocks.StrictMock<IExportToScenarioResultView>();
			scheduleStorage = mocks.StrictMock<IScheduleStorage>();
			moveSvc = mocks.StrictMock<IMoveDataBetweenSchedules>();
			uowFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			scheduleDictionaryBatchPersister = mocks.DynamicMock<IScheduleDictionaryPersister>();
			callback = mocks.DynamicMock<IReassociateDataForSchedules>();
			partsToMove = new List<IScheduleDay>();
			persons = new List<IPerson>();
			exportScenario = new Scenario("export");
			orginalScenario = new Scenario("original");
			target = new Presenter(uowFactory, view, scheduleStorage, moveSvc, callback, persons, partsToMove, exportScenario, scheduleDictionaryBatchPersister);
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
				Expect.Call(() => view.DisableInteractions());
				Expect.Call(() => view.EnableInteractions());
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(null);
				callback.ReassociateDataForAllPeople();
				Expect.Call(scheduleStorage.FindSchedulesForPersons(null, null, null, null, null)).IgnoreArguments()
					.Return(dictionaryToExportTo);
				LastCall.IgnoreArguments();
				Expect.Call(moveSvc.CopySchedulePartsToAnotherDictionary(dictionaryToExportTo, partsToMove)).Return(warnings);
				verifyScenarioText();
				verifyAgentText("1");
				verifyWarningsAreSetWithArrayCount();
			}
			using (mocks.Playback())
			{
				Initialize(target);
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
				Expect.Call(() => view.DisableInteractions());
				Expect.Call(() => view.EnableInteractions());
				Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(null);
				callback.ReassociateDataForAllPeople();
				Expect.Call(scheduleStorage.FindSchedulesForPersons(null, null, null, null, null)).Return(dictionaryToExportTo);
				LastCall.IgnoreArguments();
				Expect.Call(moveSvc.CopySchedulePartsToAnotherDictionary(dictionaryToExportTo, partsToMove)).Return(warnings);
				verifyScenarioText();
				verifyAgentText("1");
				view.DisableBodyText();
			}
			using (mocks.Playback())
			{
				Initialize(target);
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
			Assert.Throws<ArgumentException>(() => Initialize(target)); //annat exception här?
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

			var export = target.CommitExport();
			target.ExportFinished(export);

			mocks.VerifyAll();
		}

		[Test]
		public void Initialize_WhenWarnings_ShouldSetWarningsOnTheView()
		{
			var moveDataBetweenSchedules = MockRepository.GenerateMock<IMoveDataBetweenSchedules>();
			var exportToScenarioView = MockRepository.GenerateMock<IExportToScenarioResultView>();

			moveDataBetweenSchedules.Expect(m => m.CopySchedulePartsToAnotherDictionary(null, null))
									.IgnoreArguments()
									.Return(threeRuleResponsesTwoWithSameMessageAndPerson());

			var exportPresenter = new exportToScenarioResultPresenterStubFactory() { View = exportToScenarioView, MoveSchedules = moveDataBetweenSchedules }.Create();

			Initialize(exportPresenter);

			exportToScenarioView.AssertWasCalled(e => e.SetWarningText(null), o => o.IgnoreArguments());
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

		[Test]
		public void Initialize_WhenFinishedSuccesfully_ShouldEnableControlsOnTheView()
		{
			var exportToScenarioView = MockRepository.GenerateMock<IExportToScenarioResultView>();
			var exportPresenter = new exportToScenarioResultPresenterStubFactory() { View = exportToScenarioView }.Create();

			exportPresenter.ExportFinished(true);

			exportToScenarioView.AssertWasCalled(e => e.EnableInteractions());
		}

		[Test]
		public void Initialize_BeforeLoadingStarts_ShouldDisableInteractions()
		{
			var exportToScenarioView = MockRepository.GenerateMock<IExportToScenarioResultView>();

			var exportPresenter = new exportToScenarioResultPresenterStubFactory() { View = exportToScenarioView }.Create();

			exportPresenter.PrepareForExport();
			exportToScenarioView.AssertWasCalled(e => e.DisableInteractions());
		}

		#region helpers
		private static void Initialize(ExportToScenarioResultPresenter presenter)
		{
			presenter.PrepareForExport();
			var exportSuccessful = presenter.CommitExport();
			presenter.ExportFinished(exportSuccessful);
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
			return ExtractedSchedule.CreateScheduleDay(tempDictionary, person, new DateOnly(2000, 1, 1));
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
			public Presenter(IUnitOfWorkFactory uowFactory, IExportToScenarioResultView view, IScheduleStorage scheduleStorage, IMoveDataBetweenSchedules moveSchedules, IReassociateDataForSchedules callback, IEnumerable<IPerson> persons, IEnumerable<IScheduleDay> schedulePartsToExport, IScenario exportScenario, IScheduleDictionaryPersister scheduleDictionaryBatchPersister)
				: base(uowFactory, view, scheduleStorage, moveSchedules, callback, persons, schedulePartsToExport, exportScenario, scheduleDictionaryBatchPersister)
			{
			}

			public void SetPersistingDic(IScheduleDictionary dic)
			{
				SetScheduleDictionaryToPersist(dic);
			}
		}

		private class exportToScenarioResultPresenterStubFactory
		{
			public IUnitOfWorkFactory UowFactory { get; set; }
			public IExportToScenarioResultView View { get; set; }
			public IScheduleStorage ScheduleStorage { get; set; }
			public IMoveDataBetweenSchedules MoveSchedules { get; set; }
			public IReassociateDataForSchedules Callback { get; set; }
			public IEnumerable<IPerson> FullyLoadedPersonsToMove { get; set; }
			public IEnumerable<IScheduleDay> SchedulePartsToExport { get; set; }
			public IScenario ExportScenario { get; set; }
			public IScheduleDictionaryPersister ScheduleDictionaryBatchPersister { get; set; }

			public ExportToScenarioResultPresenter Create()
			{
				var fakeFactory = new MockRepository();
				if (UowFactory == null)
				{
					UowFactory = fakeFactory.Stub<IUnitOfWorkFactory>();
				}
				if (View == null)
				{
					View = fakeFactory.Stub<IExportToScenarioResultView>();
				}
				if (ScheduleStorage == null)
				{
					ScheduleStorage = fakeFactory.Stub<IScheduleStorage>();
				}
				if (Callback == null)
				{
					Callback = fakeFactory.Stub<IReassociateDataForSchedules>();
				}
				if (MoveSchedules == null)
				{
					MoveSchedules = fakeFactory.DynamicMock<IMoveDataBetweenSchedules>();
					MoveSchedules.Expect(m => m.CopySchedulePartsToAnotherDictionary(null, null))
								 .IgnoreArguments()
								 .Return(new List<IBusinessRuleResponse>());
				}
				if (SchedulePartsToExport == null)
				{
					SchedulePartsToExport = new List<IScheduleDay>()
						                        {
							                        new SchedulePartFactoryForDomain().CreatePartWithMainShift()
						                        };
				}
				if (FullyLoadedPersonsToMove == null)
				{
					FullyLoadedPersonsToMove = from s in SchedulePartsToExport
											   select s.Person;
				}
				if (ExportScenario == null)
				{
					ExportScenario = ScenarioFactory.CreateScenarioWithId("whatever", false);
				}

				if (ScheduleDictionaryBatchPersister == null)
				{
					ScheduleDictionaryBatchPersister = fakeFactory.Stub<IScheduleDictionaryPersister>();
				}
				fakeFactory.ReplayAll();
				return new ExportToScenarioResultPresenter(UowFactory, View, ScheduleStorage, MoveSchedules, Callback, FullyLoadedPersonsToMove, SchedulePartsToExport, ExportScenario, ScheduleDictionaryBatchPersister);
			}
		}
		#endregion
	}
}