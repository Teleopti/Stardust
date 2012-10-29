﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter
{
    /// <summary>
    /// Tests for DayOffConverter
    /// </summary>
    [TestFixture]
    public class DayOffConverterTest
    {
        private DayOffConverter target;
        private IUnitOfWork uow;
        private MappedObjectPair mappedObjectPair;
        private Mapper<IDayOffTemplate, global::Domain.Absence> mapper;
        private MockRepository mocks;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            uow = mocks.StrictMock<IUnitOfWork>();
            mappedObjectPair = new MappedObjectPair();
            mapper = mocks.StrictMock<Mapper<IDayOffTemplate, global::Domain.Absence>>(mappedObjectPair, (TimeZoneInfo.Local));
            target = new DayOffConverter(uow, mapper);
        }

        [Test]
        public void VerifyRepositoryGetterWorks()
        {
            Assert.IsNotNull(target.Repository);
        }

        [Test]
        public void VerifyOnPersisted()
        {
            ObjectPairCollection<global::Domain.Absence, IDayOffTemplate> pairList = new ObjectPairCollection<global::Domain.Absence, IDayOffTemplate>();
            ConverterEventHelper.ExecuteOnPersisted(target, pairList);
            Assert.AreSame(pairList, target.Mapper.MappedObjectPair.DayOff);
        }
    }
}
