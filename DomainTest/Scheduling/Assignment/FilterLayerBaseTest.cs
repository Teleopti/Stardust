﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{

    public abstract class FilterLayerBaseTest
    {
        private IVisualLayerCollection target;
        private IList<IVisualLayer> internalCollection;
        private IVisualLayerFactory layerFactory;

        [SetUp]
        public void Setup()
        {
            layerFactory = new VisualLayerFactory();
            internalCollection = new List<IVisualLayer>();
            target = new VisualLayerCollection(null, internalCollection, new ProjectionPayloadMerger());
            Optimizer = CreateOptimizer();
            target.PeriodOptimizer = Optimizer;
        }

        protected abstract IFilterOnPeriodOptimizer CreateOptimizer();

        protected IFilterOnPeriodOptimizer Optimizer { get; private set; }

        [Test]
        public void VerifyFilterPeriod()
        {
            internalCollection.Add(createLayer(10, 11));
            internalCollection.Add(createLayer(12, 13));
            internalCollection.Add(createLayer(14, 15));
            internalCollection.Add(createLayer(16, 17));
            internalCollection.Add(createLayer(17, 18));
            Assert.AreEqual(3, target.FilterLayers(createPeriod(12, 17)).Count());
            Assert.AreEqual(1, target.FilterLayers(createPeriod(17, 18)).Count()); //opt
            Assert.AreEqual(1, target.FilterLayers(createPeriod(8, 12)).Count()); //non opt
        }

        [Test]
        public void VerifyDoNotCrashZeroHits()
        {
            internalCollection.Add(createLayer(10, 12));
            internalCollection.Add(createLayer(13, 14));
            Assert.AreEqual(0, target.FilterLayers(createPeriod(9, 10)).Count());
            Assert.AreEqual(1, target.FilterLayers(createPeriod(11, 12)).Count());
            Assert.AreEqual(1, target.FilterLayers(createPeriod(13, 14)).Count());
            Assert.AreEqual(0, target.FilterLayers(createPeriod(14, 15)).Count());
            Assert.AreEqual(0, target.FilterLayers(createPeriod(14, 15)).Count());

            Assert.AreEqual(1, target.FilterLayers(createPeriod(11, 12)).Count());
        }

        [Test, Explicit("Keep this for perf test")]
        public void PerformanceTestToRunWithDotTrace()
        {
            internalCollection.Add(createLayer(4, 7));
            internalCollection.Add(createLayer(7, 10));
            internalCollection.Add(createLayer(11, 14));

            internalCollection.Add(createLayer(18, 21));
            internalCollection.Add(createLayer(21, 24));
            internalCollection.Add(createLayer(24, 27));

            Assert.AreEqual(0, target.FilterLayers(createPeriod(2, 3)).Count());
            Assert.AreEqual(0, target.FilterLayers(createPeriod(3, 4)).Count());
            Assert.AreEqual(1, target.FilterLayers(createPeriod(4, 5)).Count());
            Assert.AreEqual(1, target.FilterLayers(createPeriod(5, 6)).Count());
            Assert.AreEqual(1, target.FilterLayers(createPeriod(6, 7)).Count());
            Assert.AreEqual(1, target.FilterLayers(createPeriod(7, 8)).Count());
            Assert.AreEqual(1, target.FilterLayers(createPeriod(8, 9)).Count());
            Assert.AreEqual(1, target.FilterLayers(createPeriod(9, 10)).Count());
            Assert.AreEqual(0, target.FilterLayers(createPeriod(10, 11)).Count());
            Assert.AreEqual(1, target.FilterLayers(createPeriod(11, 12)).Count());
            Assert.AreEqual(1, target.FilterLayers(createPeriod(12, 13)).Count());
            Assert.AreEqual(1, target.FilterLayers(createPeriod(13, 14)).Count());
            Assert.AreEqual(0, target.FilterLayers(createPeriod(14, 15)).Count());
            Assert.AreEqual(0, target.FilterLayers(createPeriod(15, 16)).Count());
            Assert.AreEqual(0, target.FilterLayers(createPeriod(16, 17)).Count());
            Assert.AreEqual(0, target.FilterLayers(createPeriod(17, 18)).Count());
            Assert.AreEqual(1, target.FilterLayers(createPeriod(18, 19)).Count());
            Assert.AreEqual(1, target.FilterLayers(createPeriod(19, 20)).Count());

            Assert.AreEqual(0, target.FilterLayers(createPeriod(30, 31)).Count());
            Assert.AreEqual(0, target.FilterLayers(createPeriod(31, 32)).Count());


            Assert.AreEqual(2, target.FilterLayers(createPeriod(11, 19)).Count());

        }

        private IVisualLayer createLayer(int startHour, int endHour)
        {
            return layerFactory.CreateShiftSetupLayer(new Activity("sdf"), createPeriod(startHour, endHour));
        }

        private static DateTimePeriod createPeriod(int startHour, int endHour)
        {
            DateTime date = TimeZoneHelper.ConvertToUtc(new DateTime(2000, 1, 1));
            return new DateTimePeriod(date.AddHours(startHour), date.AddHours(endHour));
        }
    }
}
