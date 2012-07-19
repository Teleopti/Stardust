using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class HasLayerWithPeriodAndActivityThatIsEqualToTest
    {
        private IActivity _activity1;
        private IActivity _activity2;
        private DateTimePeriod _period1;
        private DateTimePeriod _period2;
    	private IPerson _person;

    	[SetUp]
        public void Setup()
        {
            _activity1 = ActivityFactory.CreateActivity("activity1");
            _activity2 = ActivityFactory.CreateActivity("activity2");
            _period1 = new DateTimePeriod(2001,1,1,2001,1,2);
            _period2 = _period1.MovePeriod(TimeSpan.FromHours(5));
        	_person = PersonFactory.CreatePerson();
        }

        [Test]
        public void VerifyThatIsSatisfiedByALayerWithSameActivityAndPeriod()
        {
            IList<IVisualLayer> layers = new List<IVisualLayer>();
            VisualLayerFactory factory = new VisualLayerFactory();
            var originalLayer = factory.CreateShiftSetupLayer(_activity1, _period1, _person);
            var layerEqualToOriginalLayer = factory.CreateShiftSetupLayer(originalLayer.Payload as IActivity, originalLayer.Period, _person);
            var layerWithDifferentActivity = factory.CreateShiftSetupLayer(_activity2, _period1, _person);
            var layerWithDifferentPeriod = factory.CreateShiftSetupLayer(_activity1, _period2, _person);
            layers.Add(originalLayer);
           
            var target = new HasLayerWithPeriodAndActivityThatIsEqualTo(layers);


            Assert.IsTrue(target.IsSatisfiedBy(layerEqualToOriginalLayer), "There is a layer has the same activity and period");
            Assert.IsFalse(target.IsSatisfiedBy(layerWithDifferentActivity),"Should not be satisfied because the Activity is different");
            Assert.IsFalse(target.IsSatisfiedBy(layerWithDifferentPeriod), "Should not be satisfied because the Period is different");

            //Should return true after adding layers that satisfies the specification:
            layers.Add(factory.CreateShiftSetupLayer(_activity2, _period1, _person));
            layers.Add(factory.CreateShiftSetupLayer(_activity1, _period2, _person));

            Assert.IsTrue(target.IsSatisfiedBy(layerWithDifferentActivity));
            Assert.IsTrue(target.IsSatisfiedBy(layerWithDifferentPeriod));
        }
    }
}