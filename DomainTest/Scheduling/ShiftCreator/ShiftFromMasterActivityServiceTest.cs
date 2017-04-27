using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [DomainTest]
    public class ShiftFromMasterActivityServiceTest
    {
        public IShiftFromMasterActivityService ShiftFromMasterActivityService;

        IList<IWorkShift> _workShiftList;
        Activity _activity1;
        Activity _activity2;
        Activity _activity3;
        Activity _activity4;
        Activity _activityNotInContractTime;
        Activity _activityDontRequireSkill;
        IWorkShift _workShift;
        DateTimePeriod _period8To17;
        DateTimePeriod _period10To13;
        DateTimePeriod _period15To16;
        DateTimePeriod _period11To12;
        DateTimePeriod _period8To10;
        DateTimePeriod _period13To15;
        DateTimePeriod _period16To17;
        DateTimePeriod _period10To11;
        DateTimePeriod _period12To13;
        DateTimePeriod _period13To17;

        [SetUp]
        public void Setup()
        {
            _activity1 = new Activity("1"){InContractTime = true, RequiresSkill = true};
            _activity2 = new Activity("2") { InContractTime = true, RequiresSkill = true };
            _activity3 = new Activity("3") { InContractTime = true, RequiresSkill = true };
            _activity4 = new Activity("4") { InContractTime = true, RequiresSkill = true };
	        _activityNotInContractTime = new Activity("notInContractTime")
	        {
		        InContractTime = false,
		        RequiresSkill = true
	        };
	        _activityDontRequireSkill = new Activity("dontRequireSkill")
	        {
		        InContractTime = true,
		        RequiresSkill = false
	        };

            _workShift = new WorkShift(new ShiftCategory("shiftCategory"));

            DateTime time8 = new DateTime(2010, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            DateTime time10 = new DateTime(2010, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            DateTime time11 = new DateTime(2010, 1, 1, 11, 0, 0, DateTimeKind.Utc);
            DateTime time12 = new DateTime(2010, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            DateTime time13 = new DateTime(2010, 1, 1, 13, 0, 0, DateTimeKind.Utc);
            DateTime time15 = new DateTime(2010, 1, 1, 15, 0, 0, DateTimeKind.Utc);
            DateTime time16 = new DateTime(2010, 1, 1, 16, 0, 0, DateTimeKind.Utc);
            DateTime time17 = new DateTime(2010, 1, 1, 17, 0, 0, DateTimeKind.Utc);
            
            _period8To17 = new DateTimePeriod(time8,time17);
            _period8To10 = new DateTimePeriod(time8, time10);
            _period10To13 = new DateTimePeriod(time10, time13);
            _period15To16 = new DateTimePeriod(time15, time16);
            _period11To12 = new DateTimePeriod(time11, time12);
            _period13To15 = new DateTimePeriod(time13, time15);
            _period16To17 = new DateTimePeriod(time16, time17);
            _period10To11 = new DateTimePeriod(time10, time11);
            _period12To13 = new DateTimePeriod(time12, time13);
            _period13To17 = new DateTimePeriod(time13, time17);
        }

	    [Test, Ignore("PBI#44134")]
	    public void ShouldAssignFirstUsedActivityInMasterActivityListAsBaseActivityForTheShifts()
	    {
		    var phoneActivity = new Activity { InContractTime = true, RequiresSkill = true }.WithId();
			var emailActivity = new Activity { InContractTime = true, RequiresSkill = true }.WithId();
			var lunchActivity = new Activity { InContractTime = false, RequiresSkill = false }.WithId();
			var masterActivity = new MasterActivity();
			masterActivity.UpdateActivityCollection(new List<IActivity>{phoneActivity, emailActivity});
			var workShift = new WorkShift(new ShiftCategory("shiftCategory"));
		    var layer1 = new WorkShiftActivityLayer(masterActivity, new DateTimePeriod(2017, 4, 27, 8, 2017, 4, 27, 9));
			var layer2 = new WorkShiftActivityLayer(lunchActivity, new DateTimePeriod(2017, 4, 27, 9, 2017, 4, 27, 10));
			var layer3 = new WorkShiftActivityLayer(masterActivity, new DateTimePeriod(2017, 4, 27, 10, 2017, 4, 27, 11));
			workShift.LayerCollection.Add(layer1);
			workShift.LayerCollection.Add(layer2);
			workShift.LayerCollection.Add(layer3);

			var workShifts = ShiftFromMasterActivityService.ExpandWorkShiftsWithMasterActivity(workShift);
			workShifts[0].LayerCollection[0].Period.Should().Be.EqualTo(new DateTimePeriod(2017, 4, 27, 8, 2017, 4, 27, 11));
			workShifts[0].LayerCollection[2].Period.Should().Be.EqualTo(new DateTimePeriod(2017, 4, 27, 10, 2017, 4, 27, 11));
		}

        [Test]
        public void ShouldReturnOriginalWorkShiftWhenNoMasterActivity()
        {
            WorkShiftActivityLayer layer = new WorkShiftActivityLayer(_activity1, new DateTimePeriod());
            _workShift.LayerCollection.Add(layer);

            IList<IWorkShift> workShifts = ShiftFromMasterActivityService.ExpandWorkShiftsWithMasterActivity(_workShift);
	        workShifts.Single().Should().Be.SameInstanceAs(_workShift);
        }

        [Test]
        public void ShouldNotGenerateShiftsForActivitiesNotInContractTimeOrNotRequiresSkill()
        {
			var master = new MasterActivity();
			master.UpdateActivityCollection(new List<IActivity> { _activityNotInContractTime, _activityDontRequireSkill });
            WorkShiftActivityLayer layer = new WorkShiftActivityLayer(master, _period8To17);
 
            _workShift.LayerCollection.Add(layer);

            _workShiftList = ShiftFromMasterActivityService.ExpandWorkShiftsWithMasterActivity(_workShift);

            Assert.AreEqual(0,_workShiftList.Count);
        }

        [Test]
        public void ShouldNotGenerateShiftsForDeletedActivities()
        {
            var master = new MasterActivity();
            var deletedActivity = new Activity("deleted(soon)") {RequiresSkill = true, InContractTime = true};
            deletedActivity.SetDeleted();
            master.UpdateActivityCollection(new List<IActivity>{deletedActivity});
            WorkShiftActivityLayer layer = new WorkShiftActivityLayer(master, _period8To17);

            _workShift.LayerCollection.Add(layer);

            _workShiftList = ShiftFromMasterActivityService.ExpandWorkShiftsWithMasterActivity(_workShift);

            Assert.AreEqual(0, _workShiftList.Count);
        }

        [Test]
        public void ShouldGenerateWorkShiftsWhenOnlyMasterActivities()
        {
			var master1 = new MasterActivity();
			master1.UpdateActivityCollection(new List<IActivity> { _activity2, _activity3 });

			var master2 = new MasterActivity();
			master2.UpdateActivityCollection(new List<IActivity> { _activity2, _activity3 });

			var master3 = new MasterActivity();
			master3.UpdateActivityCollection(new List<IActivity> { _activity2, _activity3 });

			WorkShiftActivityLayer layer1 = new WorkShiftActivityLayer(master1, _period8To10);
            WorkShiftActivityLayer layer2 = new WorkShiftActivityLayer(master2, _period10To13);
            WorkShiftActivityLayer layer3 = new WorkShiftActivityLayer(master3, _period13To15);
            
            _workShift.LayerCollection.Add(layer1);
            _workShift.LayerCollection.Add(layer2);
            _workShift.LayerCollection.Add(layer3);

            _workShiftList = ShiftFromMasterActivityService.ExpandWorkShiftsWithMasterActivity(_workShift);

            Assert.AreEqual(8, _workShiftList.Count);

            assertWorkShiftsWhenOnlyMasterActivities(_workShiftList[0].LayerCollection, 1);
            assertWorkShiftsWhenOnlyMasterActivities(_workShiftList[1].LayerCollection, 2);
            assertWorkShiftsWhenOnlyMasterActivities(_workShiftList[2].LayerCollection, 3);
            assertWorkShiftsWhenOnlyMasterActivities(_workShiftList[3].LayerCollection, 4);
            assertWorkShiftsWhenOnlyMasterActivities(_workShiftList[4].LayerCollection, 5);
            assertWorkShiftsWhenOnlyMasterActivities(_workShiftList[5].LayerCollection, 6);
            assertWorkShiftsWhenOnlyMasterActivities(_workShiftList[6].LayerCollection, 7);
            assertWorkShiftsWhenOnlyMasterActivities(_workShiftList[7].LayerCollection, 8);
        }

        private void assertWorkShiftsWhenOnlyMasterActivities(ILayerCollection<IActivity> layerCollection, int numWorkShift)
        {
            //expected
            //
            //workshift1 8-10[activity2] 10-13[activity2] 13-15[activity2]
            //workshift2 8-10[activity2] 10-13[activity2] 13-15[activity3] 
            //workshift3 8-10[activity2] 10-13[activity3] 13-15[activity2] 
            //workshift4 8-10[activity2] 10-13[activity3] 13-15[activity3] 
            //workshift5 8-10[activity3] 10-13[activity2] 13-15[activity2] 
            //workshift6 8-10[activity3] 10-13[activity2] 13-15[activity3] 
            //workshift7 8-10[activity3] 10-13[activity3] 13-15[activity2] 
            //workshift8 8-10[activity3] 10-13[activity3] 13-15[activity3]

            Assert.AreEqual(3, layerCollection.Count);
            
            int i = 0;
            foreach (ILayer<IActivity> layer in layerCollection)
            {
                IActivity activity = layer.Payload as Activity;

                if(i == 0)
                {
                    if (numWorkShift < 5)
                        Assert.AreEqual(_activity2, activity);
                    else
                        Assert.AreEqual(_activity3, activity);
                    
                }

                if (i == 1)
                {
                    if (numWorkShift == 1 || numWorkShift == 2 || numWorkShift == 5 || numWorkShift == 6)
                        Assert.AreEqual(_activity2, activity);
                    else
                        Assert.AreEqual(_activity3, activity);
                }

                if (i == 2)
                {
                    if (numWorkShift == 1 || numWorkShift == 3 || numWorkShift == 5 || numWorkShift == 7)
                        Assert.AreEqual(_activity2, activity);
                    else
                        Assert.AreEqual(_activity3, activity);
                }

                i++;
            }    
        }

        [Test]
        public void ShouldGenerateWorkShiftsWhenMasterActivitiesAndActivitiesOnWorkShift()
        {
			var master1 = new MasterActivity();
			master1.UpdateActivityCollection(new List<IActivity> { _activity2, _activity3 });

			var master2 = new MasterActivity();
			master2.UpdateActivityCollection(new List<IActivity> { _activity2, _activity3 });

			WorkShiftActivityLayer layer1 = new WorkShiftActivityLayer(_activity1, _period8To17);
            WorkShiftActivityLayer layer2 = new WorkShiftActivityLayer(master1, _period10To13);
            WorkShiftActivityLayer layer3 = new WorkShiftActivityLayer(master2, _period15To16);
            
            _workShift.LayerCollection.Add(layer1);
            _workShift.LayerCollection.Add(layer2);
            _workShift.LayerCollection.Add(layer3);

            _workShiftList = ShiftFromMasterActivityService.ExpandWorkShiftsWithMasterActivity(_workShift);

            Assert.AreEqual(4, _workShiftList.Count);

            assertWorkShiftsWhenMasterActivitiesAndActivitiesOnWorkShift(_workShiftList[0].ProjectionService().CreateProjection(), 1);
            assertWorkShiftsWhenMasterActivitiesAndActivitiesOnWorkShift(_workShiftList[1].ProjectionService().CreateProjection(), 2);
            assertWorkShiftsWhenMasterActivitiesAndActivitiesOnWorkShift(_workShiftList[2].ProjectionService().CreateProjection(), 3);
            assertWorkShiftsWhenMasterActivitiesAndActivitiesOnWorkShift(_workShiftList[3].ProjectionService().CreateProjection(), 4);
        }

        private void assertWorkShiftsWhenMasterActivitiesAndActivitiesOnWorkShift(IVisualLayerCollection visualLayerCollection, int numWorkShift)
        {
            //expected
            //
            //workshift1 8-10[activity1] 10-13[activity2] 13-15[activity1] 15-17[activity2] 16-17[activity1]
            //workshift2 8-10[activity1] 10-13[activity2] 13-15[activity1] 15-17[activity3] 16-17[activity1]
            //workshift3 8-10[activity1] 10-13[activity3] 13-15[activity1] 15-17[activity2] 16-17[activity1]
            //workshift4 8-10[activity1] 10-13[activity3] 13-15[activity1] 15-17[activity3] 16-17[activity1]


            Assert.AreEqual(5, visualLayerCollection.Count());

            int i = 0;
            foreach (IVisualLayer layer in visualLayerCollection)
            {
                IActivity activity = layer.Payload as Activity;

                if (i == 0)
                {
                    Assert.AreEqual(_activity1, activity);
                    Assert.AreEqual(_period8To10, layer.Period);
                }

                if (i == 1)
                {
                    if (numWorkShift == 1 || numWorkShift == 2)
                        Assert.AreEqual(_activity2, activity);
                    else
                        Assert.AreEqual(_activity3, activity);

                    Assert.AreEqual(_period10To13, layer.Period);
                }

                if (i == 2)
                {
                    Assert.AreEqual(_activity1, activity);
                    Assert.AreEqual(_period13To15, layer.Period);
                }

                if (i == 3)
                {
                    if (numWorkShift == 1 || numWorkShift == 3)
                        Assert.AreEqual(_activity2, activity);
                    else
                        Assert.AreEqual(_activity3, activity);

                    Assert.AreEqual(_period15To16, layer.Period);
                }

                if (i == 4)
                {
                    Assert.AreEqual(_activity1, activity);
                    Assert.AreEqual(_period16To17, layer.Period);
                }

                i++;
            }
        }   


        [Test]
        public void ShouldGenerateWorkShiftsWhenActivityOnTopOfMasterActivity()
        {
			var master = new MasterActivity();
			master.UpdateActivityCollection(new List<IActivity> { _activity2, _activity3 });

			WorkShiftActivityLayer layer1 = new WorkShiftActivityLayer(_activity1, _period8To17);
            WorkShiftActivityLayer layer2 = new WorkShiftActivityLayer(master, _period10To13);
            WorkShiftActivityLayer layer3 = new WorkShiftActivityLayer(_activity4, _period11To12);

            _workShift.LayerCollection.Add(layer1);
            _workShift.LayerCollection.Add(layer2);
            _workShift.LayerCollection.Add(layer3);

            _workShiftList = ShiftFromMasterActivityService.ExpandWorkShiftsWithMasterActivity(_workShift);

            Assert.AreEqual(4, _workShiftList.Count);

            assertWorkShiftsWhenActivityOnTopOfMasterActivity(_workShiftList[0].ProjectionService().CreateProjection(), 1);
            assertWorkShiftsWhenActivityOnTopOfMasterActivity(_workShiftList[1].ProjectionService().CreateProjection(), 2);
            assertWorkShiftsWhenActivityOnTopOfMasterActivity(_workShiftList[2].ProjectionService().CreateProjection(), 3);
            assertWorkShiftsWhenActivityOnTopOfMasterActivity(_workShiftList[3].ProjectionService().CreateProjection(), 4);
        }

        private void assertWorkShiftsWhenActivityOnTopOfMasterActivity(IVisualLayerCollection visualLayerCollection, int numWorkShift)
        {
            //expected
            //
            //workshift1 8-10[activity1] 10-11[activity2] 11-12[activity4] 12-13[activity2] 13-17[activity1]
            //workshift2 8-10[activity1] 10-11[activity2] 11-12[activity4] 12-13[activity3] 13-17[activity1]
            //workshift3 8-10[activity1] 10-11[activity3] 11-12[activity4] 12-13[activity2] 13-17[activity1]
            //workshift4 8-10[activity1] 10-11[activity3] 11-12[activity4] 12-13[activity3] 13-17[activity1]

            Assert.AreEqual(5, visualLayerCollection.Count());

            int i = 0;
            foreach (IVisualLayer layer in visualLayerCollection)
            {
                IActivity activity = layer.Payload as Activity;

                if (i == 0)
                {
                    Assert.AreEqual(_activity1, activity);
                    Assert.AreEqual(_period8To10, layer.Period);
                }

                if (i == 1)
                {
                    if (numWorkShift == 1 || numWorkShift == 2)
                        Assert.AreEqual(_activity2, activity);
                    else
                        Assert.AreEqual(_activity3, activity);

                    Assert.AreEqual(_period10To11, layer.Period);
                }

                if (i == 2)
                {
                    Assert.AreEqual(_activity4, activity);
                    Assert.AreEqual(_period11To12, layer.Period);
                }

                if (i == 3)
                {
                    if (numWorkShift == 1 || numWorkShift == 3)
                        Assert.AreEqual(_activity2, activity);
                    else
                        Assert.AreEqual(_activity3, activity);

                    Assert.AreEqual(_period12To13, layer.Period);
                }

                if (i == 4)
                {
                    Assert.AreEqual(_activity1, activity);
                    Assert.AreEqual(_period13To17, layer.Period);
                }

                i++;
            }
        }   
    }
}
