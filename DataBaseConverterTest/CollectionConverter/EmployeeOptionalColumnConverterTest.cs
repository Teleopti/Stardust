#region Imports
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

#endregion

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter
{
    /// <summary>
    /// Tests EmployeeOptionalColumnConverter
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 8/13/2008
    /// </remarks>
    [TestFixture]
    public class EmployeeOptionalColumnConverterTest
    {
        #region Fileds - Instance Members

        private EmployeeOptionalColumnConverter target;
        private IUnitOfWork uow;
        private MappedObjectPair mappedObjectPair;
        private Mapper<IOptionalColumn, global::Domain.EmployeeOptionalColumn> mapper;
        private MockRepository mocks;

        #endregion

        #region Methods - Instance Member - EmployeeOptionalColumnConverterTest Members

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/13/2008
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            uow = mocks.StrictMock<IUnitOfWork>();
            mappedObjectPair = new MappedObjectPair();
            mapper =
                mocks.StrictMock<Mapper<IOptionalColumn, global::Domain.EmployeeOptionalColumn>>(mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Local));
            target = new EmployeeOptionalColumnConverter(uow, mapper);
        }

        #region Test Methods

        /// <summary>
        /// Verifies the repository getter works.
        /// </summary>
        /// <remarks>
        /// Created by: Sachintha Weerasekara
        /// Created date: 8/13/2008
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
        /// Created date: 8/13/2008
        /// </remarks>
        [Test]
        public void VerifyOnPersisted()
        {
            ObjectPairCollection<global::Domain.EmployeeOptionalColumn, IOptionalColumn> pairList =
                new ObjectPairCollection<global::Domain.EmployeeOptionalColumn, IOptionalColumn>();

            ConverterEventHelper.ExecuteOnPersisted(target, pairList);
            Assert.AreSame(pairList, target.Mapper.MappedObjectPair.OptionalColumn);
        }

        #endregion

        #endregion
    }
}
