using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Common
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), TestFixture]
    public class LayerViewModelCollectionTest
    {
        private MockRepository mocks;
        private LayerViewModelCollection target;
        private DateTimePeriod period;
        private SchedulePartFactoryForDomain _partFactory;

		[SetUp]
		public void Setup()
		{
			_partFactory = new SchedulePartFactoryForDomain();
			period = new DateTimePeriod(new DateTime(2008, 12, 5, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2008, 12, 6, 0, 0, 0, DateTimeKind.Utc));
			target = new LayerViewModelCollection(null, new CreateLayerViewModelService(),
				new RemoveLayerFromSchedule(), null, new FullPermission());
			mocks = new MockRepository();
		}

		[Test]
        public void VerifyInstanceCreated()
        {
            Assert.IsNotNull(target);
        }

        #region remove
      

        [Test]
        public void VerifyAddFromProjection()
        {
            //Just check that it has created from the projection (and from the projection only)

            IScheduleRange range = mocks.StrictMock<IScheduleRange>();
            IScheduleDay part = _partFactory.CreatePartWithMainShiftWithDifferentActivities();
            IScheduleDay emptyPart = mocks.StrictMock<IScheduleDay>();
            part.Person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
            using(mocks.Record())
            {
                Expect.Call(range.Person).Return(part.Person);
				Expect.Call(range.ScheduledDay(new DateOnly(2000, 12, 31))).Return(emptyPart);
                Expect.Call(range.ScheduledDay(new DateOnly(2001, 1, 1))).Return(part);
                Expect.Call(range.ScheduledDay(new DateOnly(2001, 1, 2))).Return(emptyPart);
                Expect.Call(range.ScheduledDay(new DateOnly(2001, 1, 3))).Return(emptyPart);
                Expect.Call(emptyPart.HasProjection()).Return(false).Repeat.Times(3);
            }
            using (mocks.Playback())
            {
                target.AddFromProjection(range, _partFactory.CurrentPeriod);

                Assert.IsTrue(target.All(l => _partFactory.CurrentPeriod.Contains(l.Period)));
                Assert.AreEqual(target.Count(l => !l.IsProjectionLayer), 0,
                                "Only projectionlayers should have been created");
            }
        }
        #endregion
        
       [Test]
       public void VerifyAddFromSchedulePart()
		{
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				IScheduleDay part = _partFactory
					.AddAbsence()
					.AddMainShiftLayer()
					.AddMeeting()
					.AddOvertime()
					.AddPersonalLayer()
					.CreatePart();
				target.AddFromSchedulePart(part);


				Assert.IsTrue(target.OfType<AbsenceLayerViewModel>().Count() == 1);
				Assert.IsTrue(target.OfType<OvertimeLayerViewModel>().Count() == 1);
				Assert.IsTrue(target.OfType<MeetingLayerViewModel>().Count() == 1);
				Assert.IsTrue(target.OfType<MainShiftLayerViewModel>().Count() == 1);
				Assert.IsTrue(target.OfType<PersonalShiftLayerViewModel>().Count() == 1);
			}
		}

        [Test]
        public void VerifySchedulePartIsSet()
        {
            IScheduleDay part = _partFactory
             .AddAbsence()
             .AddMainShiftLayer()
             .AddMeeting()
             .AddOvertime()
             .AddPersonalLayer()
             .CreatePart();
            target.AddFromSchedulePart(part);

            foreach (ILayerViewModel model in target)
            {
                Assert.AreEqual(part, model.SchedulePart);
            }
        }

        [Test]
        public void VerifyCanSupplyNullAsSchedulePart()
        {
            target.AddFromSchedulePart(null);
            Assert.AreEqual(0, target.Count);
        }

        [Test]
        public void VerifyMoveAll()
        {

            TimeSpan movedTimeSpan = TimeSpan.FromMinutes(15);
            ILayerViewModel model1 = mocks.StrictMock<ILayerViewModel>();
            ILayerViewModel model2 = mocks.StrictMock<ILayerViewModel>();
            ILayerViewModel model3 = mocks.StrictMock<ILayerViewModel>();
            target.Add(model1);
            target.Add(model2);
            target.Add(model3);

            using (mocks.Record())
            {

                Expect.Call(model2.Period).Return(period);
                Expect.Call(model1.CanMoveAll).Return(true); //Will not move (sender)
                Expect.Call(model2.CanMoveAll).Return(true);
                Expect.Call(model3.CanMoveAll).Return(false); //Will not move (MultiMove = false)
                Expect.Call(model1.ShouldBeIncludedInGroupMove(model1)).Return(false); //Will not move (sender)
                Expect.Call(model2.ShouldBeIncludedInGroupMove(model1)).Return(true); //Will not move (sender)
                model2.Period = period.MovePeriod(movedTimeSpan);
            }
            using (mocks.Playback())
            {
                target.MoveAllLayers(model1, movedTimeSpan);
            }
        }

        [Test]
        public void VerifyMoveAllTriggersParentCollection()
        {
            TimeSpan toMove = TimeSpan.FromHours(2);
            var layer = new MainShiftLayer(new Activity("_"), period);
	       
            

                ILayerViewModel model1 = new MainShiftLayerViewModel(target, layer, new PersonAssignment(new Person(), new Scenario(), DateOnly.Today), null);
                ILayerViewModel model2 = new MainShiftLayerViewModel(target, layer, new PersonAssignment(new Person(), new Scenario(), DateOnly.Today),null);
                model1.CanMoveAll = true;
                model2.CanMoveAll = true;
                target.Add(model1);
                target.Add(model2);
                model1.MoveLayer(toMove);
                Assert.AreEqual(period.StartDateTime.Add(toMove), model2.Period.StartDateTime);
                Assert.AreEqual(period.EndDateTime.Add(toMove), model2.Period.EndDateTime);
        }

        [Test]
        public void VerifyViewsAndFilters()
        {
			target = new LayerViewModelCollection(null, new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission());
            ILayerViewModel modelFromProjection = mocks.StrictMock<ILayerViewModel>();
            ILayerViewModel modelFromPart = mocks.StrictMock<ILayerViewModel>();
            target.Add(modelFromProjection);
            target.Add(modelFromPart);
            using (mocks.Record())
            {
                Expect.Call(modelFromProjection.IsProjectionLayer).Return(true).Repeat.Any();
                Expect.Call(modelFromPart.IsProjectionLayer).Return(false).Repeat.Any();
            }
            using (mocks.Playback())
            {
                Assert.IsTrue(target.VisualLayers.Contains(modelFromProjection));
                Assert.IsTrue(target.ScheduleLayers.Contains(modelFromPart));
                Assert.IsFalse(target.VisualLayers.Contains(modelFromPart));
                Assert.IsFalse(target.ScheduleLayers.Contains(modelFromProjection));
            }
        }

        [Test]
        public void VerifySortDescriptionOfScheduleLayers()
        {
            //Atleast on description Should be sorted ascending by VisualOrderIndex  
            //Verify schedule is sorted by VisualOrderIndex:
			target = new LayerViewModelCollection(null, new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), null, new FullPermission());
            target.CreateViewModels(new SchedulePartFactoryForDomain().CreatePartWithMainShift());
            bool isSatisfied = false;
                foreach (var sortDesc in  target.ScheduleLayers.SortDescriptions)
                {
                   if(sortDesc.PropertyName=="VisualOrderIndex" && sortDesc.Direction == ListSortDirection.Ascending)
                       isSatisfied = true;
                }
            Assert.IsTrue(isSatisfied);
        }

        [Test]
        public void VerifyCreatesProjectionAndScheduleLayersInTheSameCollection()
        {

            IPerson person = new Person();

            var mainShift = new EditableShift(ShiftCategoryFactory.CreateShiftCategory("for test"));
            var layer = new EditableShiftLayer(ActivityFactory.CreateActivity("test"), period);
            mainShift.LayerCollection.Add(layer);
            using (mocks.Record())
            {
                IScheduleDay part = createPart(person, new DateOnly(mainShift.LayerCollection.OuterPeriod().Value.StartDateTime));
                part.AddMainShift(mainShift);
                target.CreateViewModels(part);
            }

            using (mocks.Playback())
            {
                Assert.AreEqual(1, target.Count(l => l.IsProjectionLayer), "Created from projection");
                Assert.AreEqual(1, target.Count(l => !l.IsProjectionLayer), "Created from schedulePart");

            }
        }

        [Test]
        public void VerifySetsIntervalWhenCreatingLayers()
        {
            target.Interval = TimeSpan.FromMinutes(7);
            IPerson person = new Person();

            var mainShift = new EditableShift(ShiftCategoryFactory.CreateShiftCategory("for test"));
            var layer = new EditableShiftLayer(ActivityFactory.CreateActivity("test"), period);
            mainShift.LayerCollection.Add(layer);
            using (mocks.Record())
            {
                IScheduleDay part = createPart(person, new DateOnly(mainShift.LayerCollection.OuterPeriod().Value.StartDateTime));
                part.AddMainShift(mainShift);
                target.CreateViewModels(part); //Creates both projection and normal
            }

            using (mocks.Playback())
            {
                Assert.AreEqual(target.Interval, target.First(l => l.IsProjectionLayer).Interval, "Interval is sat when projectionlayerviewmodel created");
                Assert.AreEqual(target.Interval, target.First(l => !l.IsProjectionLayer).Interval, "Interval is sat when normal viewmodel created");
            }
        }

        [Test]
        public void VerifyChangingTheIntervalChangesAllTheModelsInterval()
        {
            TimeSpan interval = TimeSpan.FromMinutes(5);
            ILayerViewModel model1 = new MainShiftLayerViewModel(new VisualLayer(new Activity("df"),new DateTimePeriod(), new Activity("sdf")  ), new Person()) { Interval = TimeSpan.FromMinutes(12) };
						ILayerViewModel model2 = new MainShiftLayerViewModel(new VisualLayer(new Activity("df"), new DateTimePeriod(), new Activity("sdf")), new Person()) { Interval = TimeSpan.FromMinutes(14) };
            target.Add(model1);
            target.Add(model2);
            target.Interval = interval;
            Assert.AreEqual(interval, model1.Interval);
            Assert.AreEqual(interval, model2.Interval);
        }

        [Test]
        public void VerifyCreateProjectionLayers()
        {
            #region setup
            IScheduleDay newPart = mocks.StrictMock<IScheduleDay>();
            IProjectionService projectionService = mocks.StrictMock<IProjectionService>();
            IScheduleDay part = new SchedulePartFactoryForDomain().CreatePartWithMainShift();
            IVisualLayerCollection newProjection = VisualLayerCollectionFactory.CreateForWorkShift(TimeSpan.FromHours(8), TimeSpan.FromHours(12));
            target.CreateViewModels(part);
            IList<ILayerViewModel> projectionLayers = target.Where(l => l.IsProjectionLayer).ToList();
            IList<ILayerViewModel> deletedLayers = new List<ILayerViewModel>(); //For holding the deleted layers
            #endregion

            //Check that old projectionlayers are removed and new added by listening to the CollectionChanged
            //Make sure no Layers that arent from the projection is added or removed
            target.CollectionChanged += (delegate(object sender, NotifyCollectionChangedEventArgs e)
                                             {
                                                 if (e.OldItems != null)
                                                 {
                                                     foreach (object removed in e.OldItems)
                                                     {

                                                         ILayerViewModel layer = (ILayerViewModel)removed;
                                                         Assert.IsTrue(layer.IsProjectionLayer, "Should only remove from projection");
                                                         deletedLayers.Add(layer);
                                                     }
                                                 }

                                                 if (e.NewItems != null)
                                                 {
                                                     foreach (object added in e.NewItems)
                                                     {
                                                         ILayerViewModel layer = (ILayerViewModel)added;
                                                         Assert.IsTrue(layer.IsProjectionLayer, "Should only add from new projection");
                                                         Assert.IsTrue(newProjection.Any(l=>l.Payload.Equals(layer.Payload)),"Make sure its from the new projection");
                                                     }
                                                 }

                                             });


            using (mocks.Record())
            {
                Expect.Call(newPart.ProjectionService()).Return(projectionService);
                Expect.Call(newPart.Person).Return(new Person());
                Expect.Call(projectionService.CreateProjection()).Return(newProjection);
            }

            using (mocks.Playback())
            {
                target.CreateProjectionViewModels(newPart);
            }
            
            foreach (var layer in projectionLayers)
            {
                Assert.IsTrue(deletedLayers.Contains(layer),"Check that all layers are removed (should be inserted in the deleted list)");
            }
          

        }

        [Test]
        public void VerifyVisualOrderIndex()
        {
            //Create a model of each type, check that the VisualOrderIndex are based on that type
            //This is how we want the NOT PROJECTED layers to appear in the gui
            #region setup
            var assignment = PersonAssignmentFactory.CreateAssignmentWithThreeMainshiftLayers();
						var multi = mocks.DynamicMock<IMultiplicatorDefinitionSet>();
						assignment.AddOvertimeActivity(ActivityFactory.CreateActivity("activity"), period, multi);
						assignment.AddPersonalActivity(ActivityFactory.CreateActivity("activity"), period);
            AbsenceLayer absenceLayer = new AbsenceLayer(AbsenceFactory.CreateAbsence("absence"), period);


            MainShiftLayerViewModel mainShiftModel1 = new MainShiftLayerViewModel(null, assignment.MainActivities().First(), assignment, null);
            MainShiftLayerViewModel mainShiftModel2 = new MainShiftLayerViewModel(null, assignment.MainActivities().Last(), assignment, null);
			OvertimeLayerViewModel overtimeLayerViewModel = new OvertimeLayerViewModel(null, assignment.OvertimeActivities().Single(), assignment, null);
			PersonalShiftLayerViewModel personalShiftLayerViewModel = new PersonalShiftLayerViewModel(null,assignment.PersonalActivities().Single(), assignment, null);
            AbsenceLayerViewModel absenceLayerViewModel = new AbsenceLayerViewModel(null, new PersonAbsence(new Person(), new Scenario(), absenceLayer),null);
	        var meetingPerson = new MeetingPerson(new Person(), false);
			Meeting meeting = new Meeting(new Person(), new[]{meetingPerson }, "subject", "location", "description", ActivityFactory.CreateActivity("activity"), ScenarioFactory.CreateScenarioAggregate());
			PersonMeeting personMeeting = new PersonMeeting(meeting, meetingPerson, new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
            MeetingLayerViewModel meetingLayerViewModel = new MeetingLayerViewModel(null, personMeeting, null);
			var externalMeeting = new ExternalMeetingLayerViewModel(null, new MeetingShiftLayer(ActivityFactory.CreateActivity("activity"), new DateTimePeriod(2001, 1, 1, 2001, 1, 2), new ExternalMeeting().WithId(Guid.NewGuid())), null, new Person());
            #endregion
			mocks.ReplayAll();
            Stack<ILayerViewModel> stack = new Stack<ILayerViewModel>((from m in 
                                                                       new List<ILayerViewModel>()
                                                                       {
                                                                           mainShiftModel1,
                                                                           meetingLayerViewModel,
                                                                           mainShiftModel2,
                                                                           overtimeLayerViewModel,
                                                                           personalShiftLayerViewModel,
                                                                           absenceLayerViewModel,
																		   externalMeeting
                                                                       }
                                                                       orderby m.VisualOrderIndex
                                                                       select m));
            
            //Verify that the order is what its intended for the visual layout
            Assert.AreEqual(absenceLayerViewModel, stack.Pop());
            Assert.AreEqual(externalMeeting, stack.Pop());
			Assert.AreEqual(meetingLayerViewModel, stack.Pop());
            Assert.AreEqual(personalShiftLayerViewModel, stack.Pop());
            Assert.AreEqual(overtimeLayerViewModel, stack.Pop());
            Assert.AreEqual(mainShiftModel2, stack.Pop());
            Assert.AreEqual(mainShiftModel1, stack.Pop()); 



        }

        [Test]
        public void RemoveService_WhenMainShiftLayerIsRemoved_ShouldBeCalledWithThatLayer()
        {
			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var removeService = MockRepository.GenerateStrictMock<IRemoveLayerFromSchedule>();

				target = new LayerViewModelCollection(new EventAggregator(), new CreateLayerViewModelService(),
					removeService, null, new FullPermission());

				IScheduleDay part = _partFactory
					.AddMainShiftLayer()
					.CreatePart();
				target.AddFromSchedulePart(part);

				var theLayer = part.PersonAssignment().MainActivities().Single();
				var theLayerViewModel = target.Single();

				removeService.Expect(r => r.Remove(part, theLayer));

				target.RemoveActivity(theLayerViewModel, theLayer, part);
				removeService.VerifyAllExpectations();
			}
		}

		[Test]
		public void RemoveService_WhenAbsenceLayerIsRemoved_ShouldBeCalledWithThatAbsenceLayer()
		{

			var removeService = MockRepository.GenerateStrictMock<IRemoveLayerFromSchedule>();

			target = new LayerViewModelCollection(new EventAggregator(), new CreateLayerViewModelService(), removeService, null, new FullPermission());

			IScheduleDay part = _partFactory
			  .AddAbsence()
			  .CreatePart();
			target.AddFromSchedulePart(part);

#pragma warning disable 612,618
			var theLayer = part.PersonAbsenceCollection().Single().Layer;
#pragma warning restore 612,618

			var theLayerViewModel = target.Single();

			removeService.Expect(r => r.Remove(part, theLayer));

			target.RemoveAbsence(theLayerViewModel, theLayer, part);
			removeService.VerifyAllExpectations();
		}

        [Test]
        public void VerifySetsPersonMeeting()
        {
            IScheduleDay part = _partFactory
             .AddMainShiftLayer()
             .AddMeeting()
             .CreatePart();
            target.AddFromSchedulePart(part);
            IPersonMeeting personMeeting = part.PersonMeetingCollection().First();
            MeetingLayerViewModel model = target.OfType<MeetingLayerViewModel>().First();
            Assert.IsNotNull(model);
            Assert.AreEqual(model.PersonMeeting,personMeeting);          
        }


        [Test]
        public  void VerifyElectingALayerSetsTheCurrentIfItExists()
        {
            target.AddFromSchedulePart(_partFactory.AddMainShiftLayer().CreatePartWithMainShift());

            //just get the first viewmodels....
            var model = target.First();
            var model2 = target.First(m=>m!=model);

            target.SelectLayer(model);

            Assert.IsTrue(model.IsSelected,"The model should have been selected");
            Assert.AreEqual(target.ScheduleLayers.CurrentItem as ILayerViewModel,model,"The selected model should be in sync with the currentitem of the schedulelayers");

            Assert.IsNotNull(model2);
            target.SelectLayer(model2);   //Select the second model

            Assert.IsTrue(model2.IsSelected);
            Assert.AreEqual(target.ScheduleLayers.CurrentItem as ILayerViewModel, model2);

            Assert.IsFalse(model.IsSelected, "The first model should no longer be selected");

            //recreate new models:
            target.AddFromSchedulePart(_partFactory.CreatePartWithMainShiftWithDifferentActivities());

            target.SelectLayer(model);
            Assert.AreNotEqual(model, target.ScheduleLayers.CurrentItem as ILayerViewModel, "try to select the old model (that does no longer exist in the collection)");

        }

        [Test]
        public void VerifyHasCreatedGroupsForTheLayers()
        {
            Assert.AreEqual(UserTexts.Resources.Absence,target.AbsenceLayers.Description);
            Assert.AreEqual(UserTexts.Resources.Activities, target.MainShiftLayers.Description);
            Assert.AreEqual(UserTexts.Resources.Meeting, target.MeetingLayers.Description);
            Assert.AreEqual(UserTexts.Resources.Meeting, target.ExternalMeetingLayers.Description);
            Assert.AreEqual(UserTexts.Resources.PersonalShifts, target.PersonalLayers.Description);
            Assert.AreEqual(UserTexts.Resources.Overtime, target.OvertimeLayers.Description);

            //Verify that they are connected to the collection:
            foreach (LayerGroupViewModel model in target.Groups)
            {
                Assert.AreEqual(target,model.Layers.SourceCollection);
            }

        }

        [Test]
        public void VerifySortOrderOfGroups()
        {
          
            var view = CollectionViewSource.GetDefaultView(target.Groups);
            view.MoveCurrentToFirst();

            Assert.IsTrue(view.CurrentItem as LayerGroupViewModel<MainShiftLayerViewModel>!=null,"MainShift");
            view.MoveCurrentToNext();
            Assert.IsTrue(view.CurrentItem as LayerGroupViewModel<PersonalShiftLayerViewModel> != null, "PersonalShift");
            view.MoveCurrentToNext();
            Assert.IsTrue(view.CurrentItem as LayerGroupViewModel<OvertimeLayerViewModel> != null, "Overtime");
            view.MoveCurrentToNext();
            Assert.IsTrue(view.CurrentItem as LayerGroupViewModel<MeetingLayerViewModel> != null, "Meeting");
			view.MoveCurrentToNext();
			Assert.IsTrue(view.CurrentItem is LayerGroupViewModel<ExternalMeetingLayerViewModel>, "Meeting");
            view.MoveCurrentToNext();
            Assert.IsTrue(view.CurrentItem as LayerGroupViewModel<AbsenceLayerViewModel> != null, "Absence");

        }

		[Test]
		public void TotalDateTimePeriod_WhenScheduleHasAbsence_ShouldReturnTheTotalPeriodFromAllLayerViewModelsExceptAbsence()
		{
			IScheduleDay part = _partFactory
			 .AddAbsence()
			 .AddMainShiftLayer()
			 .CreatePart();
			target.AddFromSchedulePart(part);

			var expectedStart = part.PersonAssignment().Period.StartDateTime;
			var expectedEnd = part.PersonAssignment().Period.EndDateTime;
			var totalPeriod = target.TotalDateTimePeriod();

			Assert.That(totalPeriod.StartDateTime, Is.EqualTo(expectedStart));
			Assert.That(totalPeriod.EndDateTime, Is.EqualTo(expectedEnd));
		}

		[Test]
		public void TotalDateTimePeriod_WhenNoLayers_ShouldBeEqualToThePartsPeriod()
		{
			var schedule = new SchedulePartFactoryForDomain().CreatePart();
			target.AddFromSchedulePart(schedule);
			Assert.AreEqual(schedule.Period, target.TotalDateTimePeriod());
		}

		[Test]
		public void TotalDateTimePeriod_WhenOnlyAbsence_ShouldreturnTheperiodOfTheSchedule()
		{
			var schedule = new SchedulePartFactoryForDomain()
				.AddAbsence()
				.AddAbsence()
				.CreatePart();
			target.AddFromSchedulePart(schedule);

			Assert.AreEqual(schedule.Period, target.TotalDateTimePeriod());
		}

		[Test]
		public void ReplaceScheduleLayer_WhenMainshiftLayerIsChanged_ShouldBeCalledWithThatLayer()
		{
			var replaceService = MockRepository.GenerateStrictMock<IReplaceLayerInSchedule>();

			target = new LayerViewModelCollection(new EventAggregator(), new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), replaceService, new FullPermission());

			var part = _partFactory
			  .AddMainShiftLayer()
			  .CreatePart();
			target.AddFromSchedulePart(part);

			var theLayer = part.PersonAssignment().MainActivities().Single();
			var theLayerViewModel = target.Single();

			var newPayload = ActivityFactory.CreateActivity("new");
			var newPeriod = period.MovePeriod(TimeSpan.FromMinutes(5));

			theLayerViewModel.Payload = newPayload;
			theLayerViewModel.Period = newPeriod;

			replaceService.Expect(r => r.Replace(part, theLayer, newPayload, newPeriod));

			target.ReplaceActivity(theLayerViewModel, theLayer, part);
			
			replaceService.VerifyAllExpectations();
		}

		[Test]
		public void ReplaceScheduleLayer_WhenAbsenceLayerIsChanged_ShouldBeCalledWithThatLayer()
		{
			var replaceService = MockRepository.GenerateStrictMock<IReplaceLayerInSchedule>();

			target = new LayerViewModelCollection(new EventAggregator(), new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), replaceService, new FullPermission());

			IScheduleDay part = _partFactory
			  .AddAbsence()
			  .CreatePart();
			target.AddFromSchedulePart(part);

#pragma warning disable 612,618
			var theLayer = part.PersonAbsenceCollection().Single().Layer;
#pragma warning restore 612,618

			var theLayerViewModel = target.Single();

			var newAbsence = AbsenceFactory.CreateAbsence("new");
			var newPeriod = theLayerViewModel.Period.MovePeriod(TimeSpan.FromMinutes(15));

			theLayerViewModel.Period = newPeriod;
			theLayerViewModel.Payload = newAbsence;

			replaceService.Expect(r => r.Replace(part, theLayer, newAbsence, newPeriod));

			target.ReplaceAbsence(theLayerViewModel, theLayer, part);

			replaceService.VerifyAllExpectations();
		}

		[Test]
		public void MoveAllLayers_WhenMultipleLayersHasBeenMoved_ShouldReplaceAllLayersThatHasBeenMoved()
		{
			var replaceService = MockRepository.GenerateStrictMock<IReplaceLayerInSchedule>();

			target = new LayerViewModelCollection(new EventAggregator(), new CreateLayerViewModelService(), new RemoveLayerFromSchedule(), replaceService, new FullPermission());

			IScheduleDay part = _partFactory.CreatePartWithMainShiftWithDifferentActivities();
			target.AddFromSchedulePart(part);

			var firstViewModel = target.First();
			var secondViewModel = target.Skip(1).First();

			var firstModel = part.PersonAssignment().MainActivities().First(l => l.Period == firstViewModel.Period);
			var secondModel = part.PersonAssignment().MainActivities().First(l => l.Period == secondViewModel.Period);
			var firstPeriod = firstModel.Period;
			var secondPeriod = secondModel.Period;

			var firstPayload = firstModel.Payload;
			var secondPayload = secondModel.Payload;

			firstViewModel.CanMoveAll = true;
			secondViewModel.CanMoveAll = true;
			firstViewModel.MoveLayer(TimeSpan.Zero);

			replaceService.Expect(r => r.Replace(part, firstModel, firstPayload, firstPeriod));
			replaceService.Expect(r => r.Replace(part, secondModel, secondPayload, secondPeriod));

			firstViewModel.UpdatePeriod();

			replaceService.VerifyAllExpectations();
		}

		[Test]
		public void ShouldClearLayersThatShouldBeUpdatedWhenANewSchedulePartIsLoaded()
		{
			var layerViewModel = MockRepository.GenerateMock<ILayerViewModel>();
			target.ShouldBeUpdated(layerViewModel);

			target.CreateViewModels(new SchedulePartFactoryForDomain().CreatePart());

			target.UpdateAllMovedLayers();

			layerViewModel.AssertWasNotCalled(x => x.UpdateModel());
		}

		[Test]
		public void ShouldClearLayersThatShouldBeUpdatedWhenANewSchedulePartIsLoaded_DontKnowWhyNeededThisExtraCreateViewModels()
		{
			var layerViewModel = MockRepository.GenerateMock<ILayerViewModel>();
			layerViewModel.Expect(x => x.SchedulePart).Return(new SchedulePartFactoryForDomain().CreatePart());
			target.ShouldBeUpdated(layerViewModel);

			target.CreateViewModels(new LayerViewModelSelector(layerViewModel), new SchedulePartFactoryForDomain().CreatePart());

			target.UpdateAllMovedLayers();

			layerViewModel.AssertWasNotCalled(x => x.UpdateModel());
		}

		[Test]
		public void UpdateAllLayers_IfALayerIsMarkedForUpdateButNotPresent_ShouldNotUpdateThatLayer()
		{
			var schedule =
				new SchedulePartFactoryForDomain().CreatePartWithMainShiftWithDifferentActivities();
			target = new LayerViewModelCollection(new EventAggregator(), new CreateLayerViewModelService(),
													  new RemoveLayerFromSchedule(), new ReplaceLayerInSchedule(), new FullPermission());
			target.CreateViewModels(schedule);
			var first = target.First(l => !l.IsProjectionLayer);
			target.ShouldBeUpdated(first);
			first.Delete();

			Assert.DoesNotThrow(target.UpdateAllMovedLayers);
		}

		[Test]
		public void UpdateAllLayers_Always_ShouldUnmarkLayersForUpdate()
		{
			var vm = MockRepository.GenerateMock<ILayerViewModel>();
			target.Add(vm);
			target.ShouldBeUpdated(vm);

			target.UpdateAllMovedLayers();
			target.UpdateAllMovedLayers();

			vm.AssertWasCalled(l => l.UpdateModel(), l => l.Repeat.Once());
		}

		private IScheduleDay createPart(IPerson person, DateOnly dateOnly)
        {
            IScheduleDictionary dictionaryNotUsed = new ScheduleDictionaryForTest(ScenarioFactory.CreateScenarioAggregate(),
                                                                                  new ScheduleDateTimePeriod(period),
                                                                                  new Dictionary
                                                                                      <IPerson, IScheduleRange>());
            return ExtractedSchedule.CreateScheduleDay(dictionaryNotUsed, person, dateOnly, new FullPermission());
        }

        [TearDown]
        public void Teardown()
        {
            mocks.VerifyAll();
        }
    }

}
