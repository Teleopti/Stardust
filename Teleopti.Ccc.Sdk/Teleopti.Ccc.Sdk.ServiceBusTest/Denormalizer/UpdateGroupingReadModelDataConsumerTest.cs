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
    public class UpdateGroupingReadModelDataConsumerTest
    {
        private UpdateGroupingReadModelDataConsumer  _target;
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
            _target = new UpdateGroupingReadModelDataConsumer(_groupReadOnlyRepository, _currentUnitOfWorkFactory);
        }


        [Test]
        public void GroupingReadModelDataTest()
        {
            var skillTest = SkillFactory.CreateSkill("Test3");
            var tempGuid = Guid.NewGuid();
            skillTest.SetId(tempGuid);

            var ids = new[] { tempGuid };
            var message = new PersonPeriodChangedMessage();
            message.SetPersonIdCollection(ids);

            using (_mocks.Record())
            {
				Expect.Call(() => _groupReadOnlyRepository.UpdateGroupingReadModelData(ids));
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