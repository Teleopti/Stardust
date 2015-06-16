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
    public class UpdateFindPersonDataConsumerTest
    {
        private MockRepository _mocks;
        private UpdateFindPersonDataConsumer _target;
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

            _target = new UpdateFindPersonDataConsumer(_finderReadOnlyRep, _currentUnitOfWorkFactory);
        }

        [Test]
        public void ShouldDenormalizeNotPersons()
        {
            var skill = SkillFactory.CreateSkill("TestID");
            var tempGuid = Guid.NewGuid();
            skill.SetId(tempGuid);

            var ids = new[] {tempGuid};
            var mess = new PersonPeriodChangedMessage();
            mess.SetPersonIdCollection(ids);

            using (_mocks.Record())
            {
                Expect.Call(() => _finderReadOnlyRep.UpdateFindPersonData(ids));
                Expect.Call(_currentUnitOfWorkFactory.Current()).Return(_unitOfWorkFactory);
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
            }
            using (_mocks.Playback())
            {
                _target.Consume(mess);
            }
        }
    }
}