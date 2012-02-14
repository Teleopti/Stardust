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
    public class SkillConverterTest
    {
        private SkillConverter target;
        private IUnitOfWork uow;
        private MappedObjectPair mappedObjectPair;
        private Mapper<ISkill, global::Domain.Skill> mapper;
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
            mapper = mocks.StrictMock<Mapper<ISkill, global::Domain.Skill>>(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Local));
            target = new SkillConverter(uow, mapper);
        }

        [Test]
        public void VerifyRepositoryGetterWorks()
        {
            Assert.IsNotNull(target.Repository);
        }

        [Test]
        public void VerifyOnPersisted()
        {
            ObjectPairCollection<global::Domain.Skill, ISkill> pairList = new ObjectPairCollection<global::Domain.Skill, ISkill>();
            ConverterEventHelper.ExecuteOnPersisted(target, pairList);
            Assert.AreSame(pairList, target.Mapper.MappedObjectPair.Skill);
        }
    }
}
