using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Collection
{
    [TestFixture]
    public class OneSpecificLayerSpecificationTest
    {
        private ISpecification<IVisualLayerCollection> target;
        private IActivity payload;
        private IVisualLayerFactory visualLayerFactory;
    	private IPerson person;

    	[SetUp]
        public void Setup()
        {
            visualLayerFactory = new VisualLayerFactory();
            payload = ActivityFactory.CreateActivity("sdfsdf");
        	person = PersonFactory.CreatePerson();
            target = VisualLayerCollectionSpecification.OneSpecificLayer(payload);
        }

        [Test]
        public void VerifyEmptyReturnFalse()
        {
            VisualLayerCollection coll = new VisualLayerCollection(person, new List<IVisualLayer>(), new ProjectionPayloadMerger());
            Assert.IsFalse(target.IsSatisfiedBy(coll));
        }

        [Test]
        public void VerifyMoreThanOneReturnFalse()
        {
            IList<IVisualLayer> vList = new List<IVisualLayer> { createLayer(payload), createLayer(payload) };
            Assert.IsFalse(target.IsSatisfiedBy(new VisualLayerCollection(person, vList, new ProjectionPayloadMerger())));
        }

        [Test]
        public void VerifyWrongPayload()
        {
            IList<IVisualLayer> vList = new List<IVisualLayer> { createLayer(payload), createAbsenceLayer(new Absence()) };
            Assert.IsFalse(target.IsSatisfiedBy(new VisualLayerCollection(person, vList, new ProjectionPayloadMerger())));
        }

        [Test]
        public void VerifyCorrectPayload()
        {
            IList<IVisualLayer> vList = new List<IVisualLayer> { createLayer(payload) };
            Assert.IsTrue(target.IsSatisfiedBy(new VisualLayerCollection(person, vList, new ProjectionPayloadMerger())));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PayloadMustBeSet()
        {
            target = VisualLayerCollectionSpecification.OneSpecificLayer(null);
        }

        private IVisualLayer createLayer(IActivity activity)
        {
            return visualLayerFactory.CreateShiftSetupLayer(activity, new DateTimePeriod(2000, 1, 1, 2001, 1, 1), person);
        }

        private IVisualLayer createAbsenceLayer(IAbsence abs)
        {
            var actLayer = createLayer(payload);
	        return visualLayerFactory.CreateAbsenceSetupLayer(abs, actLayer, new DateTimePeriod(2000, 1, 1, 2001, 1, 1),
		        actLayer.PersonAbsenceId);
        }
    }
}
