using System;
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
    [TestFixture]
    public class WorkloadConverterTest
    {
        private WorkloadConverter target;
        private IUnitOfWork uow;
        private MappedObjectPair mappedObjectPair;
        private Mapper<IWorkload, global::Domain.Forecast> mapper;
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
            mapper = mocks.StrictMock<Mapper<IWorkload, global::Domain.Forecast>>(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Local));
            target = new WorkloadConverter(uow, mapper);
        }

        [Test]
        public void VerifyRepositoryGetterWorks()
        {
            Assert.IsNotNull(target.Repository);
        }

        [Test]
        public void VerifyOnPersisted()
        {
            ObjectPairCollection<global::Domain.Forecast, IWorkload> pairList = new ObjectPairCollection<global::Domain.Forecast, IWorkload>();
            ConverterEventHelper.ExecuteOnPersisted(target, pairList);
            Assert.AreSame(pairList, target.Mapper.MappedObjectPair.Workload);
        }
    }
}
