using System;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
    [TestFixture]
    public class DayLayerModelTest
    {
        private DateTimePeriod period;
        private MockRepository mocks;
        private IPerson person;
        private ITeam team;
        private DateTime timeStamp = new DateTime(2008, 11, 17, 0, 0, 0, DateTimeKind.Utc);
        private DayLayerModel target;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            
            period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 12, 8, 0, 0, 0, DateTimeKind.Utc), 0);
            person = PersonFactory.CreatePerson("Kalle", "Kula");
            person.EmploymentNumber = "10";
            team = mocks.StrictMock<ITeam>();
            var layerCollection = new LayerViewModelCollection();
            var commonAgentName = new CommonNameDescriptionSetting {AliasFormat = CommonNameDescriptionSetting.LastName};
            target = new DayLayerModel(person, period, team, layerCollection, commonAgentName);
        }

        [Test]
        public void APinnedAdapterIsPinned()
        {
            bool isPinnedSet = false;
            target.PropertyChanged += (sender, e) => isPinnedSet = true;
            target.IsPinned = true;
            Assert.IsTrue(isPinnedSet);
        }

        [Test]
        public void VerifyShowNextActivityLayer()
        {
            Assert.IsFalse(target.ShowNextActivity);
        }

        [Test]
        public void ShouldGetAlarmDescription()
        {
            target.AlarmLayer = new AlarmSituation(new AlarmType(new Description("Alarma!"), Color.DimGray, TimeSpan.Zero,
                                       AlarmTypeMode.UserDefined, 1), period, person);
            Assert.AreEqual("Alarma!", target.AlarmDescription);
        }

        [Test]
        public void ShouldGetCommonNameDescription()
        {
            Assert.AreEqual("Kula", target.CommonNameDescription);
        }

        [Test]
        public void ShouldGetCurrentStateDescription()
        {
            IRtaStateGroup g1 = new RtaStateGroup("sdf1", false, true);
            g1.AddState("AUX1", "sdfsdf 1", Guid.NewGuid());

            IRtaStateGroup g2 = new RtaStateGroup("sdf2", false, true);
            g2.AddState("AUX2", "sdfsdf 2", Guid.NewGuid());

            IRtaVisualLayer newRtaVisualLayer = new RtaVisualLayer(g1.StateCollection[0],
                                                                new DateTimePeriod(timeStamp, timeStamp.AddHours(5)),
                                                                new Activity("Phone"), person);
            target.CurrentState = newRtaVisualLayer;
            Assert.AreEqual("sdf1", target.CurrentStateDescription);
        }

        [Test]
        public void ShouldGetCurrentActivityDescription()
        {
            var layerFactory = new VisualLayerFactory();
            target.CurrentActivityLayer = layerFactory.CreateShiftSetupLayer(new Activity("Phone"), period,person);
            Assert.AreEqual("Phone", target.CurrentActivityDescription);
        } 
        
        [Test]
        public void ShouldGetNextActivityDescription()
        {
            var layerFactory = new VisualLayerFactory();
            target.NextActivityLayer = layerFactory.CreateShiftSetupLayer(new Activity("Phone1"), period,person);
            Assert.AreEqual("Phone1", target.NextActivityDescription);
        }

        [Test]
        public void ShouldGetNextActivityStartTime()
        {
            var layerFactory = new VisualLayerFactory();
            target.NextActivityLayer = layerFactory.CreateShiftSetupLayer(new Activity("Phone1"), period, person);
            Assert.AreEqual("00:00", target.NextActivityStartDateTime);
        }

        [Test]
        public void ShouldGetSortTime()
        {
            Assert.AreEqual(TimeSpan.Zero, target.SortTime);

            target.AlarmLayer =
                new AlarmSituation(new AlarmType(new Description("Alarma!"), Color.DimGray, TimeSpan.Zero,
                                                 AlarmTypeMode.UserDefined, 1), period, person);
            Assert.AreEqual(TimeSpan.FromDays(1), target.SortTime);
        }

        [Test]
        public void ShouldGetTeam()
        {
            Assert.AreEqual(team, target.Team);
        }
    }
}