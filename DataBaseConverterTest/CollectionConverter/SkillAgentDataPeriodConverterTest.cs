using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter
{
    [TestFixture]
    public class SkillAgentDataPeriodConverterTest
    {
        private SkillAgentDataPeriodConverter target;
        private IUnitOfWork uow;
        private MappedObjectPair mappedObjectPair;
        private Mapper<SkillPersonDataPeriod, global::Domain.SkillData> mapper;
        private MockRepository mocks;

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            uow = mocks.CreateMock<IUnitOfWork>();
            mappedObjectPair = new MappedObjectPair();
            mapper = mocks.CreateMock<Mapper<SkillPersonDataPeriod, global::Domain.SkillData>>(mappedObjectPair, TimeZoneInfo.Local);
            target = new SkillAgentDataPeriodConverter(uow, mapper);
        }

        [Test]
        public void VerifyRepositoryGetterWorks()
        {
            Assert.IsNotNull(target.Repository);
        }
    }
}
