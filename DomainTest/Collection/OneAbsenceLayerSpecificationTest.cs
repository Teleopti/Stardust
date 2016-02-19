﻿using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.DomainTest.Collection
{
    [TestFixture]
    public class OneAbsenceLayerSpecificationTest
    {
        private ISpecification<IVisualLayerCollection> target;
        private IVisualLayerFactory visualLayerFactory;
    	private IPerson person;

    	[SetUp]
        public void Setup()
        {
        	person = PersonFactory.CreatePerson();
            target = VisualLayerCollectionSpecification.OneAbsenceLayer;
            visualLayerFactory = new VisualLayerFactory();
        }

        [Test]
        public void VerifySameInstance()
        {
            Assert.AreSame(target, VisualLayerCollectionSpecification.OneAbsenceLayer);
        }

        [Test]
        public void VerifyEmptyReturnFalse()
        {
            VisualLayerCollection coll = new VisualLayerCollection(null, new List<IVisualLayer>(), new ProjectionPayloadMerger());
            Assert.IsFalse(target.IsSatisfiedBy(coll));
        }

        [Test]
        public void VerifyMoreThanOneReturnFalse()
        {
            IList<IVisualLayer> vList = new List<IVisualLayer> {correctAbsenceLayer(), correctAbsenceLayer()};
            Assert.IsFalse(target.IsSatisfiedBy(new VisualLayerCollection(null, vList, new ProjectionPayloadMerger())));
        }

        [Test]
        public void VerifyMoreThanOneReturnButOnlySameAbsence()
        {
            var layerOne = correctAbsenceLayer();
            IList<IVisualLayer> vList = new List<IVisualLayer> { layerOne, layerOne};
            Assert.IsTrue(target.IsSatisfiedBy(new VisualLayerCollection(null, vList, new ProjectionPayloadMerger())));
        }

        [Test]
        public void VerifyOneActivityLayerReturnFalse()
        {
            IVisualLayer actLayer = visualLayerFactory.CreateShiftSetupLayer(new Activity("sdf"), new DateTimePeriod(2000, 1, 1, 2001, 1, 1),person);
            IList<IVisualLayer> vList = new List<IVisualLayer> { actLayer };
            Assert.IsFalse(target.IsSatisfiedBy(new VisualLayerCollection(null, vList, new ProjectionPayloadMerger())));
        }

        [Test]
        public void VerifyTrue()
        {
            IList<IVisualLayer> vList = new List<IVisualLayer> {correctAbsenceLayer()};
            Assert.IsTrue(target.IsSatisfiedBy(new VisualLayerCollection(null, vList, new ProjectionPayloadMerger())));
        }

	    private IVisualLayer correctAbsenceLayer()
	    {
		    var actLayer = visualLayerFactory.CreateShiftSetupLayer(new Activity("sdf"),
			    new DateTimePeriod(2000, 1, 1, 2001, 1, 1), person);
		    return visualLayerFactory.CreateAbsenceSetupLayer(new Absence(), actLayer,
			    new DateTimePeriod(2000, 1, 1, 2001, 1, 1), actLayer.PersonAbsenceId);
	    }
    }
}
