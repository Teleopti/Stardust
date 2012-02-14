using System.Data;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter
{
    [TestFixture]
    public class RotationConverterTest
    {
        private MockRepository mocks;
        private IUnitOfWork uow;
        private MappedObjectPair mappedObjectPair;
        private Mapper<IRotation, DataRow> mapper;
        private RotationsConverter target;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            uow = mocks.StrictMock<IUnitOfWork>();
            mappedObjectPair = new MappedObjectPair();
            mapper = mocks.StrictMock<Mapper<IRotation, DataRow>>(mappedObjectPair, null);
            target = new RotationsConverter(uow, mapper);
        }

        [Test]
        public void VerifyRepositoryGetterWorks()
        {
            Assert.IsNotNull(target.Repository);
        }
        [Test]
        public void VerifyOnPersisted()
        {
            ObjectPairCollection<DataRow, IRotation> pairList = new ObjectPairCollection<DataRow, IRotation>();
            ConverterEventHelper.ExecuteOnPersisted(target, pairList);
            Assert.AreSame(pairList, target.Mapper.MappedObjectPair.Rotations);
        }

    }
}
