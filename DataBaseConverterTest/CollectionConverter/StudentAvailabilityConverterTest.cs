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
    public class StudentAvailabilityConverterTest
    {
        private MockRepository _mocks;
        private IUnitOfWork _uow;
        private MappedObjectPair _mappedObjectPair;
        private Mapper<IStudentAvailabilityDay, global::Domain.AgentDay> _mapper;
        private StudentAvailabilityConverter _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _uow = _mocks.StrictMock<IUnitOfWork>();
            _mappedObjectPair = new MappedObjectPair();
            _mapper = _mocks.StrictMock<Mapper<IStudentAvailabilityDay, global::Domain.AgentDay>>(_mappedObjectPair, null);
            _target = new StudentAvailabilityConverter(_uow, _mapper);
        }

        [Test]
        public void VerifyRepositoryGetterWorks()
        {
            Assert.IsNotNull(_target.Repository);
        }
        [Test]
        public void VerifyOnPersisted()
        {
            var pairList = new ObjectPairCollection<global::Domain.AgentDay, IStudentAvailabilityDay>();
            ConverterEventHelper.ExecuteOnPersisted(_target, pairList);
            Assert.AreSame(pairList, _target.Mapper.MappedObjectPair.StudentAvailabilityDay);
        }
        //private MockRepository _mocks;
        //private IUnitOfWork _uow;
        //private MappedObjectPair _mappedObjectPair;
        //private Mapper<IStudentAvailabilityDay, DataRow> _mapper;
        //private StudentAvailabilityConverter _target;

        //[SetUp]
        //public void Setup()
        //{
        //    _mocks = new MockRepository();
        //    _uow = _mocks.StrictMock<IUnitOfWork>();
        //    _mappedObjectPair = new MappedObjectPair();
        //    _mapper = _mocks.StrictMock<Mapper<IStudentAvailabilityDay, DataRow>>(_mappedObjectPair, null);
        //    _target = new StudentAvailabilityConverter(_uow, _mapper);
        //}

        //[Test]
        //public void VerifyRepositoryGetterWorks()
        //{
        //    Assert.IsNotNull(_target.Repository);
        //}
        //[Test]
        //public void VerifyOnPersisted()
        //{
        //    var pairList = new ObjectPairCollection<DataRow, IStudentAvailabilityDay>();
        //    ConverterEventHelper.ExecuteOnPersisted(_target, pairList);
        //    Assert.AreSame(pairList, _target.Mapper.MappedObjectPair.StudentAvailabilityDay);
        //}
    }
}
