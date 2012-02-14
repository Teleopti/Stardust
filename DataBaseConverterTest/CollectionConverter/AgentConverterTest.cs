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
    public class AgentConverterTest
    {
        private AgentConverter target;
        private IUnitOfWork uow;
        private MappedObjectPair mappedObjectPair;
        private Mapper<IPerson, global::Domain.Agent> mapper;
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
            mapper = mocks.StrictMock<Mapper<IPerson, global::Domain.Agent>>(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Local));
            target = new AgentConverter(uow, mapper);
        }

        [Test]
        public void VerifyRepositoryGetterWorks()
        {
            Assert.IsNotNull(target.Repository);
        }

        [Test]
        public void VerifyOnPersisted()
        {
            ObjectPairCollection<global::Domain.Agent, IPerson> pairList = new ObjectPairCollection<global::Domain.Agent, IPerson>();
            ConverterEventHelper.ExecuteOnPersisted(target, pairList);
            Assert.AreSame(pairList, target.Mapper.MappedObjectPair.Agent);
        }
    }
}
