using System;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class VisualLayerTest
    {
        private VisualLayer target;
        private DateTimePeriod period;
        private IActivity activity;
        private IVisualLayerFactory layerFactory;
    	private IPerson person;

    	[SetUp]
        public void Setup()
        {
            layerFactory = new VisualLayerFactory();
            period = new DateTimePeriod(2000, 1, 1, 2001, 1, 1);
			activity = ActivityFactory.CreateActivity("df"); 
			person = PersonFactory.CreatePerson();
            target = (VisualLayer)layerFactory.CreateShiftSetupLayer(activity, period, person);
        }

        [Test]
        public void VerifyPersonIsSentToDisplayMethods()
        {
            MockRepository mocks = new MockRepository();
            var act = mocks.StrictMock<IActivity>();
            target = (VisualLayer)layerFactory.CreateShiftSetupLayer(act, period, person);

            Color c = Color.Red;
            Description d = new Description("sdfsdf");
            using(mocks.Record())
            {
                Expect.Call(act.ConfidentialDescription(target.Person))
                    .Return(d);
				Expect.Call(act.ConfidentialDisplayColor(target.Person))
                    .Return(c);
            }
            using(mocks.Playback())
            {
                Assert.AreEqual(c, target.DisplayColor());
                Assert.AreEqual(d, target.DisplayDescription());
            }
        }

        [Test]
        public void CanCreate()
        {
            Assert.IsNotNull(target);
            Assert.AreSame(activity, target.Payload);
            Assert.AreSame(activity, target.HighestPriorityActivity);
            Assert.AreEqual(period, target.Period);
            Assert.IsNull(target.HighestPriorityAbsence);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyUnderlyingActivityMustNotBeNull()
        {
            layerFactory.CreateShiftSetupLayer(null, period, person);
        }

			[Test]
			public void ShouldCloneWithNewPeriod()
			{
				var newPeriod = new DateTimePeriod(2000, 5, 6, 2000, 7, 8);
				var res = (VisualLayer)target.CloneWithNewPeriod(newPeriod);
				res.Period.Should().Be.EqualTo(newPeriod);
				res.Payload.Should().Be.SameInstanceAs(target.Payload);
				res.HighestPriorityAbsence.Should().Be.SameInstanceAs(target.HighestPriorityAbsence);
				res.HighestPriorityActivity.Should().Be.SameInstanceAs(target.HighestPriorityActivity);
				res.DefinitionSet.Should().Be.SameInstanceAs(target.DefinitionSet);
			}
    }
}
