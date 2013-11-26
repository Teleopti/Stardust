using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Domain;
using Infrastructure;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class WorkShiftRuleSetMapperTest : MapperTest<ShiftClass>
    {
        WorkShiftRuleSetMapper _workShiftRuleSetMapper;
        ShiftClass _oldShiftClass;
        ShiftClass _oldShiftClassNegativeId;
        ShiftClass _oldShiftClassDeletedCategory;
        IWorkShiftRuleSet _workShiftRuleSet;
        private readonly DateTime _excludedDate = new DateTime(2008, 11, 12);
        private MappedObjectPair _mappedObjectPair;
        private const DayOfWeek _excludedDayOfWeek = DayOfWeek.Tuesday;

        protected override int NumberOfPropertiesToConvert
        {
            get { return 15; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]//Zoë is guilty...
        public void Setup()
        {
            string oldName = "oldName";
            Unit oldUnit = new Unit(1, "oldUnit", false, false, null, null, false);
            Site newSite = new Site("oldUnit");
            ShiftCategory oldShiftCategory = new ShiftCategory(1, "oldShiftCategory", "oldSC", Color.Blue, true, true, 0);
            ShiftCategory oldDeletedShiftCategory = new ShiftCategory(1, "oldDeletedShiftCategory", "oldSC", Color.Blue, true, false, 0);
            IShiftCategory newShiftCategory = new Domain.Scheduling.ShiftCategory("oldShiftCategory");
            IShiftCategory newDeletedShiftCategory = new Domain.Scheduling.ShiftCategory("oldDeletedShiftCategory");

            ObjectPairCollection<ShiftCategory, IShiftCategory> shiftCatPairList = new ObjectPairCollection<ShiftCategory, IShiftCategory>();
            shiftCatPairList.Add(oldShiftCategory, newShiftCategory);
            shiftCatPairList.Add(oldDeletedShiftCategory, newDeletedShiftCategory);

            ObjectPairCollection<Unit, ISite> sitePairList = new ObjectPairCollection<Unit, ISite>();
            sitePairList.Add(oldUnit, newSite);

            global::Domain.EmploymentType empType =
                new global::Domain.EmploymentType(-1, "Test employment type", 1, new TimeSpan(11, 0, 0), new TimeSpan(24, 0, 0),
                                   new TimeSpan(2, 0, 0));

            StartLengthDefinition startLengthDefinition =
                new StartLengthDefinition(new TimeSpan(7, 30, 0), new TimeSpan(8, 30, 0),
                                                         new TimeSpan(0, 5, 0), new TimeSpan(7, 30, 0),
                                                         new TimeSpan(8, 30, 0), new TimeSpan(0, 5, 0));


            AgentDayFactory agdFactory = new AgentDayFactory();
            ICccListCollection<ActivityLayer> activityLayerCollection =
                new CccListCollection<ActivityLayer>(agdFactory.ActivityLayerList(), CollectionType.Changeable);

            Activity activity = agdFactory.ActivityLayerList()[0].LayerActivity;

            //absolute
            ShiftClassActivity shiftClassActivity1 = new ShiftClassActivity(activity, new TimeSpan(8, 0, 0),
                                                                                           new TimeSpan(8, 0, 0),
                                                                                           new TimeSpan(0, 15, 0),
                                                                                           new TimeSpan(0, 15, 0),
                                                                                           new TimeSpan(0, 15, 0),
                                                                                           new TimeSpan(0, 15, 0),
                                                                                           false, false, 1);
            //relative
            ShiftClassActivity shiftClassActivity2 = new ShiftClassActivity(activity, new TimeSpan(0, 15, 0),
                                                                                           new TimeSpan(0, 45, 0),
                                                                                           new TimeSpan(0, 15, 0),
                                                                                           new TimeSpan(0, 15, 0),
                                                                                           new TimeSpan(0, 15, 0),
                                                                                           new TimeSpan(0, 15, 0),
                                                                                           true, false, 1);
            //autopos
            ShiftClassActivity shiftClassActivity3 = new ShiftClassActivity(activity, new TimeSpan(8, 0, 0),
                                                                                           new TimeSpan(8, 0, 0),
                                                                                           new TimeSpan(0, 0, 0),
                                                                                           new TimeSpan(0, 15, 0),
                                                                                           new TimeSpan(0, 15, 0),
                                                                                           new TimeSpan(0, 15, 0),
                                                                                           false, true, 1);

            ICccListCollection<ShiftClassActivity> shiftClassActivityCollection =
                new CccListCollection<ShiftClassActivity>();

            ICccListCollection<DateTime> datesExludedCollection = new CccListCollection<DateTime>();
            ICccListCollection<DayOfWeek> daysOfWeekExcludedCollection = new CccListCollection<DayOfWeek>();
            ICccListCollection<ShiftClassActivityMinPeriod> shiftClassActivityMinPeriods =
                new CccListCollection<ShiftClassActivityMinPeriod>();
            

            datesExludedCollection.Add(_excludedDate);
            daysOfWeekExcludedCollection.Add(_excludedDayOfWeek);
            shiftClassActivityMinPeriods.Add(new ShiftClassActivityMinPeriod(shiftClassActivity2.Activity, TimeSpan.FromMinutes(45)));
            shiftClassActivityMinPeriods.FinishReadingFromDatabase(CollectionType.Locked);
            shiftClassActivityCollection.Add(shiftClassActivity1);
            shiftClassActivityCollection.Add(shiftClassActivity2);
            shiftClassActivityCollection.Add(shiftClassActivity3);

            _oldShiftClass = new ShiftClass(12, oldName, ShiftType.Good, oldUnit, oldShiftCategory, empType, datesExludedCollection,
                              daysOfWeekExcludedCollection, shiftClassActivityCollection, shiftClassActivityMinPeriods, startLengthDefinition, new TimeSpan(16, 30, 0), new TimeSpan(17, 30, 0), new TimeSpan(0, 5, 0), activity);

            _oldShiftClassNegativeId = new ShiftClass(-1, oldName, ShiftType.Good, oldUnit, oldShiftCategory, empType, null,
                              null, shiftClassActivityCollection, null, startLengthDefinition, new TimeSpan(16, 30, 0), new TimeSpan(17, 30, 0), new TimeSpan(0, 5, 0), activity);

            _oldShiftClassDeletedCategory = new ShiftClass(12, oldName, ShiftType.Good, oldUnit, oldDeletedShiftCategory, empType, datesExludedCollection,
                              daysOfWeekExcludedCollection, shiftClassActivityCollection, shiftClassActivityMinPeriods, startLengthDefinition, new TimeSpan(16, 30, 0), new TimeSpan(17, 30, 0), new TimeSpan(0, 5, 0), activity);




            WorkShift oldWorkShift1 =
               new WorkShift(-1, "AA1500", _oldShiftClass, activityLayerCollection, false, oldShiftCategory);

            Collection<WorkShift> oldWorkShiftCollection = new Collection<WorkShift>();
            oldWorkShiftCollection.Add(oldWorkShift1);

            Contract newContract = new Contract("NewContract");
            ObjectPairCollection<WorktimeType, IContract> contractPairList =
                new ObjectPairCollection<WorktimeType, IContract>();
            contractPairList.Add((WorktimeType)1, newContract);

            _mappedObjectPair = new MappedObjectPair();
            _mappedObjectPair.Site = sitePairList;
            _mappedObjectPair.Activity = agdFactory.ActPairList;
            _mappedObjectPair.ShiftCategory = shiftCatPairList;
            _mappedObjectPair.Contract = contractPairList;

            _workShiftRuleSetMapper = new WorkShiftRuleSetMapper(_mappedObjectPair, (TimeZoneInfo.Utc));
            _workShiftRuleSet = _workShiftRuleSetMapper.Map(_oldShiftClass);
        }

        [Test]
        public void CanMapOldShiftClass()
        {
            Assert.IsNotNull(_workShiftRuleSet);
        }

        [Test]
        public void CanMapOldShiftClassWithOvertimeActivity()
        {
            _mappedObjectPair.OvertimeUnderlyingActivity.Add(_mappedObjectPair.Activity.Obj1Collection().First(),
                                                             _mappedObjectPair.Activity.Obj1Collection().Last());
            var theActivity = _mappedObjectPair.Activity.Obj2Collection().Last();
            _mappedObjectPair.Activity = new ObjectPairCollection<Activity, IActivity>();
            _mappedObjectPair.Activity.Add(_mappedObjectPair.OvertimeUnderlyingActivity.Obj2Collection().First(), theActivity);

            _workShiftRuleSet = _workShiftRuleSetMapper.Map(_oldShiftClass);
            Assert.IsNotNull(_workShiftRuleSet);
            Assert.AreEqual(theActivity, _workShiftRuleSet.TemplateGenerator.BaseActivity);
            Assert.AreEqual(theActivity,_workShiftRuleSet.ExtenderCollection[0].ExtendWithActivity);
            Assert.AreEqual(theActivity, _workShiftRuleSet.ExtenderCollection[1].ExtendWithActivity);
            Assert.AreEqual(theActivity, _workShiftRuleSet.ExtenderCollection[2].ExtendWithActivity);
            Assert.AreEqual(theActivity, ((ActivityTimeLimiter) _workShiftRuleSet.LimiterCollection[0]).Activity);
        }

        [Test]
        public void VerifyMappedName()
        {
            Assert.AreEqual(_oldShiftClass.Name, _workShiftRuleSet.Description.Name);
        }

        [Test]
        public void VerifyMappedShiftCategory()
        {
            Assert.AreEqual(_oldShiftClass.Category.Name, _workShiftRuleSet.TemplateGenerator.Category.Description.Name);
        }

        [Test]
        public void VerifyMappedActivity()
        {
            Assert.AreEqual("xyz", _workShiftRuleSet.TemplateGenerator.BaseActivity.Description.Name);
        }

        [Test]
        public void VerifyStartPeriod()
        {
            Assert.AreEqual(_oldShiftClass.StartAndLengthDefinition.EarliestStartTime,
                            _workShiftRuleSet.TemplateGenerator.StartPeriod.Period.StartTime);

            Assert.AreEqual(_oldShiftClass.StartAndLengthDefinition.LatestStartTime,
                            _workShiftRuleSet.TemplateGenerator.StartPeriod.Period.EndTime);
        }

        [Test]
        public void VerifyEndPeriod()
        {

            TimeSpan earliestEndTime = _oldShiftClass.EarliestEnd;
            TimeSpan latestEndTime = _oldShiftClass.LatestEnd;
            
            Assert.AreEqual(earliestEndTime, _workShiftRuleSet.TemplateGenerator.EndPeriod.Period.StartTime);
            Assert.AreEqual(latestEndTime, _workShiftRuleSet.TemplateGenerator.EndPeriod.Period.EndTime);
        }

        [Test]
        public void VerifySegments()
        {
            Assert.AreEqual(_oldShiftClass.StartAndLengthDefinition.StartTimeSegment,
                                _workShiftRuleSet.TemplateGenerator.StartPeriod.Segment);

            Assert.AreEqual(_oldShiftClass.EndSegment,
                                _workShiftRuleSet.TemplateGenerator.EndPeriod.Segment);
        }

        [Test]
        public void VerifyExtenders()
        {
            Assert.IsTrue(typeof(ActivityAbsoluteStartExtender) == _workShiftRuleSet.ExtenderCollection[0].GetType());
            Assert.IsTrue(typeof(ActivityRelativeStartExtender) == _workShiftRuleSet.ExtenderCollection[1].GetType());
            Assert.IsTrue(typeof(AutoPositionedActivityExtender) == _workShiftRuleSet.ExtenderCollection[2].GetType());
        }

        [Test]
        public void VerifyOrderIndex()
        {
            Assert.AreEqual(1, _workShiftRuleSet.ExtenderCollection[0].Priority());
            Assert.AreEqual(2, _workShiftRuleSet.ExtenderCollection[1].Priority());
            Assert.AreEqual(3, _workShiftRuleSet.ExtenderCollection[2].Priority());
        }

        [Test]
        public void VerifyNullOnNegativeId()
        {
            _workShiftRuleSetMapper = new WorkShiftRuleSetMapper(new MappedObjectPair(), (TimeZoneInfo.Utc));
            Assert.IsNull(_workShiftRuleSetMapper.Map(_oldShiftClassNegativeId));
        }
        [Test]
        public void VerifyDoNotConvertWithDeletedShiftCategory()
        {
            _workShiftRuleSetMapper = new WorkShiftRuleSetMapper(_mappedObjectPair, (TimeZoneInfo.Utc));
            _workShiftRuleSet = _workShiftRuleSetMapper.Map(_oldShiftClassDeletedCategory);
            Assert.IsNull(_workShiftRuleSetMapper.Map(_oldShiftClassDeletedCategory));
        }

        [Test]
        public void VerifyDoNotConvertAbsenceActivity()
        {
            //will get no hit it in mapper
            MappedObjectPair pair = new MappedObjectPair();
            pair.Activity = new ObjectPairCollection<Activity, IActivity>();
            _workShiftRuleSetMapper = new WorkShiftRuleSetMapper(pair, (TimeZoneInfo.Utc));
            Assert.IsNull(_workShiftRuleSetMapper.Map(_oldShiftClass));
        }

        [Test]
        public void VerifyExclusions()
        {
            Assert.AreEqual(1, _oldShiftClass.ExcludedDateCollection.Count);
            Assert.AreEqual(1,_oldShiftClass.ExcludedWeekDayCollection.Count);

            Assert.AreEqual(1,_workShiftRuleSet.AccessibilityDates.Count());
            Assert.AreEqual(1,_workShiftRuleSet.AccessibilityDaysOfWeek.Count());
        }

        [Test]
        public void VerifyActivityLengthLimiter()
        {
            Assert.AreEqual(1, _oldShiftClass.ActivityMinTimeCollection.Count);
            Assert.AreEqual(2, _workShiftRuleSet.LimiterCollection.Count);

            ActivityTimeLimiter activityTimeLimiter = _workShiftRuleSet.LimiterCollection[0] as ActivityTimeLimiter;
            Assert.IsNotNull(activityTimeLimiter);
            Assert.AreEqual(_oldShiftClass.ActivityMinTimeCollection[0].MinTime,
                            activityTimeLimiter.TimeLimit);
            Assert.AreEqual(OperatorLimiter.GreaterThenEquals, activityTimeLimiter.TimeLimitOperator);
        }
    }
}
