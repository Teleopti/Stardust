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
    /// <summary>
    /// Tests GroupingConverter
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 7/6/2008
    /// </remarks>
    [TestFixture]
    public class GroupingConverterTest
    {
        private GroupingConverter target;
        private IUnitOfWork uow;
        private MappedObjectPair mappedObjectPair;
        private Mapper<IGroupPage, global::Domain.Grouping> mapper;
        private MockRepository mocks;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/6/2008
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            uow = mocks.StrictMock<IUnitOfWork>();
            mappedObjectPair = new MappedObjectPair();
            mapper = mocks.StrictMock<Mapper<IGroupPage, global::Domain.Grouping>>(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Local));
            target = new GroupingConverter(uow, mapper);
        }

        /// <summary>
        /// Verifies the repository getter works.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/6/2008
        /// </remarks>
        [Test]
        public void VerifyRepositoryGetterWorks()
        {
            Assert.IsNotNull(target.Repository);
        }

        /// <summary>
        /// Verifies the on persisted.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 7/6/2008
        /// </remarks>
        [Test]
        public void VerifyOnPersisted()
        {
            ObjectPairCollection<global::Domain.Grouping, IGroupPage> pairList = new ObjectPairCollection<global::Domain.Grouping, IGroupPage>();
            ConverterEventHelper.ExecuteOnPersisted(target, pairList);
            Assert.AreSame(pairList, target.Mapper.MappedObjectPair.Grouping);
        }

    }
  }
