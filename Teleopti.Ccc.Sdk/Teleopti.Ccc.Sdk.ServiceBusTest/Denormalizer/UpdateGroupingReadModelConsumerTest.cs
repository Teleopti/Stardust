using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
    [TestFixture]
    public class UpdateGroupingReadModelConsumerTest
    {
        private UpdateGroupingReadModelConsumer _target;
        private MockRepository _mocks;
        private IGroupingReadOnlyRepository _groupReadOnlyRepository;
	    private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
	    private IUnitOfWorkFactory _unitOfWorkFactory;
	    private IUnitOfWork _unitOfWork;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
			_groupReadOnlyRepository = _mocks.DynamicMock<IGroupingReadOnlyRepository>();
			_currentUnitOfWorkFactory = _mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			_unitOfWorkFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
			_unitOfWork = _mocks.DynamicMock<IUnitOfWork>();
            _target = new UpdateGroupingReadModelConsumer( _groupReadOnlyRepository, _currentUnitOfWorkFactory);
        }

        [Test]
        public void GroupingReadModelTest()
        {
            var person = PersonFactory.CreatePerson();
            var tempGuid = Guid.NewGuid();
            person.SetId(tempGuid);

            Guid[] ids = {tempGuid};
            var mess = new PersonChangedMessage();
            mess.SetPersonIdCollection(ids);

            using (_mocks.Record())
            {
				Expect.Call(() => _groupReadOnlyRepository.UpdateGroupingReadModel(ids));
				Expect.Call(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
	            Expect.Call(() => _unitOfWork.PersistAll());
            }
            using (_mocks.Playback())
            {
                _target.Consume(mess);
            }
           Assert.That(mess.Identity, Is.Not.Null);
        } 
    }
}