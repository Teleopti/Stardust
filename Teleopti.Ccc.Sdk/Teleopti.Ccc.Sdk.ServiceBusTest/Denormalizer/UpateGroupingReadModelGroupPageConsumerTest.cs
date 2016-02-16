using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
    [TestFixture]
    public class UpdateGroupingReadModelGroupPageConsumerTest
    {
        private UpdateGroupingReadModelGroupPageConsumer _target;
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
            _target = new UpdateGroupingReadModelGroupPageConsumer(_groupReadOnlyRepository, _currentUnitOfWorkFactory);
        }

        [Test]
        public void GroupingReadModelGroupPageTest()
        {
            //const string ids = "IDS";
            var TempGuid = Guid.NewGuid();

            Guid[] ids = { TempGuid };

            var message = new GroupPageChangedMessage();
            message.SetGroupPageIdCollection( ids);

            using (_mocks.Record())
            {
				Expect.Call(() => _groupReadOnlyRepository.UpdateGroupingReadModelGroupPage(ids));
				Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
				Expect.Call(() => _unitOfWork.PersistAll());
            }
            using (_mocks.Playback())
            {
                _target.Consume(message);
            }
        }

       
    }
}