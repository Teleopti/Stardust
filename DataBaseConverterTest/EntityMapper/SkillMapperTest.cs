using System.Drawing;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Time;
using System;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    [TestFixture]
    public class SkillMapperTest : MapperTest<global::Domain.Skill>
    {
        string _oldName;
        string _oldDescription;
        string _timeZoneId;
        private SkillMapper _skillMapper;
        private global::Domain.Activity _oldActivity;
        private global::Domain.ICccListCollection<global::Domain.Forecast> _forecastPeriods;

        [SetUp]
        public void Setup()
        {
            //WorkLoad = Workload
            _oldName = "123";
            _oldDescription = "Desc";
            _timeZoneId = "W. Europe Standard Time";
            ISkill skill = SkillFactory.CreateSkill("TestSkill");

            //Skill Mapper need Workload pair and SkillType par
            ObjectPairCollection<global::Domain.Forecast, IWorkload> workloadCollection = new
                ObjectPairCollection<global::Domain.Forecast, IWorkload>();

            global::Domain.Forecast oldForecast = new global::Domain.Forecast(1, _oldName, _oldDescription);


            IWorkload newWorkload = new Workload(skill);

            workloadCollection.Add(oldForecast, newWorkload);

            ISkillType newSkillType = SkillTypeFactory.CreateSkillType();

            ObjectPairCollection<global::Domain.SkillType, ISkillType> skillTypeCollection =
                new ObjectPairCollection<global::Domain.SkillType, ISkillType>();

            skillTypeCollection.Add(global::Domain.SkillType.InboundTelephony, newSkillType);

            ObjectPairCollection<global::Domain.Activity, IActivity> activityCollection = new
               ObjectPairCollection<global::Domain.Activity, IActivity>();

            _oldActivity = new global::Domain.Activity(1, "Whom", Color.LemonChiffon, false, true, false, false, true, false, false, false);
            activityCollection.Add(_oldActivity, new Activity("Whom"));

            MappedObjectPair mapper = new MappedObjectPair();

            mapper.Workload = workloadCollection;
            mapper.SkillType = skillTypeCollection;
            mapper.Activity = activityCollection;

            _skillMapper = new SkillMapper(mapper, new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById(_timeZoneId)), 15);
            _forecastPeriods = new global::Infrastructure.CccListCollection<global::Domain.Forecast>();
            _forecastPeriods.Add(oldForecast);
        }

        /// <summary>
        /// Determines whether this instance [can map work load6x].
        /// </summary>
        [Test]
        public void CanMapWorkload6X()
        {
            //CreateProjection old Skill
            global::Domain.Skill oldSkill =
                new global::Domain.Skill(1, global::Domain.SkillType.InboundTelephony,
                    _oldName, _oldDescription, Color.DodgerBlue, _oldActivity, null, _forecastPeriods, false,
                          true, null, null,3);

            ISkill newSkill = _skillMapper.Map(oldSkill);

            Assert.AreEqual(_oldName, newSkill.Name);
            Assert.AreEqual(_oldDescription, newSkill.Description);
            Assert.AreEqual(Color.DodgerBlue.ToArgb(), newSkill.DisplayColor.ToArgb());
            Assert.AreEqual("My Phone skill type", newSkill.SkillType.Description.Name);
            Assert.AreEqual(_oldActivity.Name, newSkill.Activity.Description.Name);
            Assert.AreEqual(_timeZoneId, newSkill.TimeZone.Id);
            Assert.IsNotNull(newSkill.Activity);
        }

        [Test]
        public void CanShrinkSkillName()
        {
            string oldName = "123456789012345678901234567890123456789012345678901234567890";
            global::Domain.Skill oldSkill =
               new global::Domain.Skill(1, global::Domain.SkillType.InboundTelephony,
                   oldName, "", Color.DodgerBlue, _oldActivity, null, _forecastPeriods, false,
                         true, null, null, 3);

            ISkill newSkill = _skillMapper.Map(oldSkill);

            Assert.AreEqual(50, newSkill.Name.Length);
        }

        [Test]
        public void CanShrinkSkillDescription()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("a"[0], 1050);
            string oldDescription = sb.ToString();
            global::Domain.Skill oldSkill =
               new global::Domain.Skill(1, global::Domain.SkillType.InboundTelephony,
                   "q", oldDescription, Color.DodgerBlue, _oldActivity, null, _forecastPeriods, false,
                         true, null, null, 3);
            ISkill newSkill = _skillMapper.Map(oldSkill);

            Assert.AreEqual(1024, newSkill.Description.Length);
        }

        [Test]
        public void CanHandleDeleted()
        {
            string oldName = "123456";
            global::Domain.Skill oldSkill =
               new global::Domain.Skill(1, global::Domain.SkillType.InboundTelephony,
                   oldName, _oldDescription, Color.DodgerBlue, _oldActivity, null, _forecastPeriods, true,
                         true, null, null, 3);
            ISkill newSkill = _skillMapper.Map(oldSkill);

            Assert.AreEqual(true, ((IDeleteTag)newSkill).IsDeleted);
        }

        [Test]
        public void CanHandleOvertimeActivityAsSkillActivity()
        {
            string oldName = "123456";
            global::Domain.Skill oldSkill =
               new global::Domain.Skill(1, global::Domain.SkillType.InboundTelephony,
                   oldName, _oldDescription, Color.DodgerBlue, _oldActivity, null, _forecastPeriods, false,
                         true, null, null, 3);

            var newActivity = new global::Domain.Activity(2, "Who", Color.FloralWhite, false, true, false, false, true, false, false, false);
            var resultingActivity = new Activity("Da new");
            _skillMapper.MappedObjectPair.OvertimeUnderlyingActivity.Add(_oldActivity,newActivity);
            _skillMapper.MappedObjectPair.Activity = new ObjectPairCollection<global::Domain.Activity, IActivity>();
            _skillMapper.MappedObjectPair.Activity.Add(newActivity,resultingActivity);
            ISkill newSkill = _skillMapper.Map(oldSkill);

            Assert.AreEqual(resultingActivity,newSkill.Activity);
        }

        [Test]
        public void CanHandleEmptySkillName()
        {
            string oldName = "";
            global::Domain.Skill oldSkill =
               new global::Domain.Skill(1, global::Domain.SkillType.InboundTelephony,
                   oldName, _oldDescription, Color.DodgerBlue, _oldActivity, null, _forecastPeriods, true,
                         true, null, null, 3);

            ISkill newSkill = _skillMapper.Map(oldSkill);
            Assert.AreNotEqual("", newSkill.Name);
        }

        [Test]
        public void ConvertsPriority()
        {
            string oldName = "OldSkill";
            global::Domain.Skill oldSkill =
               new global::Domain.Skill(1, global::Domain.SkillType.InboundTelephony,
                   oldName, _oldDescription, Color.DodgerBlue, _oldActivity, null, _forecastPeriods, true,
                         true, null, null, 5);

            ISkill newSkill = _skillMapper.Map(oldSkill);
            Assert.AreEqual(oldSkill.Priority +1, newSkill.Priority);
        }

        /// <summary>
        /// All are not converted
        /// </summary>
        protected override int NumberOfPropertiesToConvert
        {
            get { return 16; }
        }
    }
}