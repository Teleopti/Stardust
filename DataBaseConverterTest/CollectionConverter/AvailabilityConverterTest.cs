using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter
{
    [TestFixture]
    public class AvailabilityConverterTest
    {
        private MockRepository mocks;
        private IUnitOfWork uow;
        private MappedObjectPair mappedObjectPair;
        private Mapper<IAvailabilityRotation, DataRow> mapper;
        private AvailabilityConverter target;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            uow = mocks.StrictMock<IUnitOfWork>();
            mappedObjectPair = new MappedObjectPair();
            mapper = mocks.StrictMock<Mapper<IAvailabilityRotation, DataRow>>(mappedObjectPair, null);
            target = new AvailabilityConverter(uow, mapper);
        }

        [Test]
        public void VerifyRepositoryGetterWorks()
        {
            Assert.IsNotNull(target.Repository);
        }
        [Test]
        public void VerifyOnPersisted()
        {
            ObjectPairCollection<DataRow, IAvailabilityRotation> pairList = new ObjectPairCollection<DataRow, IAvailabilityRotation>();
            ConverterEventHelper.ExecuteOnPersisted(target, pairList);
            Assert.AreSame(pairList, target.Mapper.MappedObjectPair.Availability);
        }
    }
}
