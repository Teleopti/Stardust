using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

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
            target = new LayerViewModelCollection(null,new CreateLayerViewModelService());
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
                Expect.Call(emptyPart.HasProjection).Return(false).Repeat.Times(3);
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
           IScheduleDay part = _partFactory
               .AddAbsence()
               .AddMainShiftLayer()
               .AddMeeting()
               .AddOvertime()
               .AddPersonalLayer()
               .CreatePart();
           target.AddFromSchedulePart(part);

       
           Assert.IsTrue(target.OfType<AbsenceLayerViewModel>().Count()==1);
           Assert.IsTrue(target.OfType<OvertimeLayerViewModel>().Count()==1);
           Assert.IsTrue(target.OfType<MeetingLayerViewModel>().Count()==1);
           Assert.IsTrue(target.OfType<MainShiftLayerViewModel>().Count()==1);
           Assert.IsTrue(target.OfType<PersonalShiftLayerViewModel>().Count() == 1);

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
        public void VerifyTotalDateTimePeriodWithNoItems()
        {
            IScheduleDay part = new SchedulePartFactoryForDomain().CreatePart();
            target.AddFromSchedulePart(part);
            Assert.AreEqual(part.Period, target.TotalDateTimePeriod(true));
            Assert.AreEqual(part.Period, target.TotalDateTimePeriod(false));
            mocks.VerifyAll();
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
            var layer = mocks.StrictMock<ILayer<IActivity>>();
            IShift shift = mocks.StrictMock<IShift>();
            using (mocks.Record())
            {
                Expect.Call(layer.Period).Return(period).Repeat.Any();
                layer.Period = period.MovePeriod(toMove);

            }
            using (mocks.Playback())
            {
                ILayerViewModel model1 = new MainShiftLayerViewModel(target, layer, shift,null);
                ILayerViewModel model2 = new MainShiftLayerViewModel(target, layer, shift,null);
                model1.CanMoveAll = true;
                model2.CanMoveAll = true;
                target.Add(model1);
                target.Add(model2);
                model1.MoveLayer(toMove);
                Assert.AreEqual(period.StartDateTime.Add(toMove), model2.Period.StartDateTime);
                Assert.AreEqual(period.EndDateTime.Add(toMove), model2.Period.EndDateTime);
            }
        }

        [Test]
        public void VerifyViewsAndFilters()
        {
            target = new LayerViewModelCollection(null,new CreateLayerViewModelService());
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
            target = new LayerViewModelCollection(null,new CreateLayerViewModelService());
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
            ActivityLayer layer = new EditorActivityLayer(ActivityFactory.CreateActivity("test"), period);
            mainShift.LayerCollection.Add(layer);
            using (mocks.Record())
            {
                IScheduleDay part = createPart(person, new DateOnly(mainShift.LayerCollection.Period().Value.StartDateTime));
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
            var layer = new EditorActivityLayer(ActivityFactory.CreateActivity("test"), period);
            mainShift.LayerCollection.Add(layer);
            using (mocks.Record())
            {
                IScheduleDay part = createPart(person, new DateOnly(mainShift.LayerCollection.Period().Value.StartDateTime));
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
            var layer = new ActivityLayer(new Activity("for test"), new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
            ILayerViewModel model1 = new MainShiftLayerViewModel(null,layer,null,null) { Interval = TimeSpan.FromMinutes(12) };
            ILayerViewModel model2 = new MainShiftLayerViewModel(null, layer, null,null) { Interval = TimeSpan.FromMinutes(14) };
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
            IVisualLayerCollection newProjection = VisualLayerCollectionFactory.CreateForWorkShift(part.Person, TimeSpan.FromHours(8), TimeSpan.FromHours(12));
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
                                                         Assert.IsTrue(newProjection.Contains((IVisualLayer)layer.Layer),"Make sure its from the new projection");
                                                     }
                                                 }

                                             });


            using (mocks.Record())
            {
                Expect.Call(newPart.ProjectionService()).Return(projectionService);
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
            MainShift shift = MainShiftFactory.CreateMainShiftWithThreeActivityLayers();
            var firstLayer =
                (from l in shift.LayerCollection
                 orderby l.OrderIndex
                 select l);

	        var multi = mocks.DynamicMock<IMultiplicatorDefinitionSet>();
			var overtime = new OvertimeShiftActivityLayer(ActivityFactory.CreateActivity("activity"), period, multi);
	        var personal = new PersonalShiftActivityLayer(ActivityFactory.CreateActivity("activity"), period);
            ActivityLayer fakeActivityLayer = new ActivityLayer(ActivityFactory.CreateActivity("activity"), period);
            AbsenceLayer absenceLayer = new AbsenceLayer(AbsenceFactory.CreateAbsence("absence"), period);


            MainShiftLayerViewModel mainShiftModel1 = new MainShiftLayerViewModel(null, firstLayer.First(), shift, null);
            MainShiftLayerViewModel mainShiftModel2 = new MainShiftLayerViewModel(null, firstLayer.Last(), shift, null);
			OvertimeLayerViewModel overtimeLayerViewModel = new OvertimeLayerViewModel(null, overtime, null);
			PersonalShiftLayerViewModel personalShiftLayerViewModel = new PersonalShiftLayerViewModel(null,personal, null, null);
            AbsenceLayerViewModel absenceLayerViewModel = new AbsenceLayerViewModel(null, absenceLayer,null);
	        var meetingPerson = new MeetingPerson(new Person(), false);
						Meeting meeting = new Meeting(new Person(), new[]{meetingPerson }, "subject", "location", "description", ActivityFactory.CreateActivity("activity"), ScenarioFactory.CreateScenarioAggregate());
						PersonMeeting personMeeting = new PersonMeeting(meeting, meetingPerson, new DateTimePeriod(2001, 1, 1, 2001, 1, 2));
            MeetingLayerViewModel meetingLayerViewModel = new MeetingLayerViewModel(null, personMeeting, null);
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
                                                                           absenceLayerViewModel
                                                                       }
                                                                       orderby m.VisualOrderIndex
                                                                       select m));
            
            //Verify that the order is what its intended for the visual layout
            Assert.AreEqual(absenceLayerViewModel, stack.Pop());
            Assert.AreEqual(meetingLayerViewModel, stack.Pop());
            Assert.AreEqual(personalShiftLayerViewModel, stack.Pop());
            Assert.AreEqual(overtimeLayerViewModel, stack.Pop());
            Assert.AreEqual(mainShiftModel2, stack.Pop());
            Assert.AreEqual(mainShiftModel1, stack.Pop()); 



        }

        [Test]
        public void VerifyRemoveServiceIsCreated()
        {
            Assert.IsNotNull(target.RemoveService,"Verify RemoveService is created");
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
            Assert.IsTrue(view.CurrentItem as LayerGroupViewModel<AbsenceLayerViewModel> != null, "Absence");

        }


        private IScheduleDay createPart(IPerson person, DateOnly dateOnly)
        {
            IScheduleDictionary dictionaryNotUsed = new ScheduleDictionaryForTest(ScenarioFactory.CreateScenarioAggregate(),
                                                                                  new ScheduleDateTimePeriod(period),
                                                                                  new Dictionary
                                                                                      <IPerson, IScheduleRange>());
            return ExtractedSchedule.CreateScheduleDay(dictionaryNotUsed, person, dateOnly);
        }

        [TearDown]
        public void Teardown()
        {
            mocks.VerifyAll();
        }
    }
}
