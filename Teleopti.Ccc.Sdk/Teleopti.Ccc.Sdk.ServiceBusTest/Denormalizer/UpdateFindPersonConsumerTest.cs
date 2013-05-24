using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
    [TestFixture]
    public class UpdateFindPersonConsumerTest
    {
        private MockRepository _mocks;
        private UpdateFindPersonConsumer _target;
        private IPersonFinderReadOnlyRepository _finderReadOnlyRep;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IUnitOfWork _unitOfWork;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _finderReadOnlyRep = _mocks.StrictMock<IPersonFinderReadOnlyRepository>();
            _currentUnitOfWorkFactory = _mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
            _unitOfWorkFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
            _unitOfWork = _mocks.DynamicMock<IUnitOfWork>();

            _target = new UpdateFindPersonConsumer(_finderReadOnlyRep, _currentUnitOfWorkFactory);
        }

        [Test]
        public void UpdateFindPersonTestCall()
        {
            var person = PersonFactory.CreatePerson();
            var tempGuid = Guid.NewGuid();
            person.SetId(tempGuid);

            var ids = new[] {tempGuid};
            var mess = new PersonChangedMessage();
            mess.SetPersonIdCollection(ids);
            using (_mocks.Record())
            {
                Expect.Call(() => _finderReadOnlyRep.UpdateFindPerson(ids));
                Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
            }
            using (_mocks.Playback())
            {
                _target.Consume(mess);
            }
            Assert.That(mess.Identity, Is.Not.Null);
        }
    }
}