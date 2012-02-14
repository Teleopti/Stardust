using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter
{
    [TestFixture]
    public class NoteConverterTest
    {
        private NoteConverter _target;
        private IUnitOfWork _uow;
        private MappedObjectPair _mappedObjectPair;
        private Mapper<INote, global::Domain.AgentDay> _mapper;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _uow = _mocks.StrictMock<IUnitOfWork>();
            _mappedObjectPair = new MappedObjectPair();
            _mapper = _mocks.StrictMock<Mapper<INote, global::Domain.AgentDay>>(_mappedObjectPair, new CccTimeZoneInfo(TimeZoneInfo.Local));
            _target = new NoteConverter(_uow, _mapper);
        }

        [Test]
        public void VerifyRepositoryGetterWorks()
        {
            Assert.IsNotNull(_target.Repository);
        }
    }
}
