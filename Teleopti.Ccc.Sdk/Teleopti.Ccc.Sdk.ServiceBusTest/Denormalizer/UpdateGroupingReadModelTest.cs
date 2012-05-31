using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class UpdateGroupingReadModelTest
	{
		private MockRepository _mocks;
		private UpdateGroupingReadModel _target;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IGroupingReadOnlyRepository _groupingReadOnlyRep;
		private IStatelessUnitOfWork _uow;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_groupingReadOnlyRep = _mocks.StrictMock<IGroupingReadOnlyRepository>();
			_uow = _mocks.StrictMock<IStatelessUnitOfWork>();
			_target = new UpdateGroupingReadModel(_unitOfWorkFactory,_groupingReadOnlyRep);
		}

        [Test]
        public void ShouldCallRepToUpdatePersons()
        {
            Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(_uow);
            Expect.Call(() => _groupingReadOnlyRep.UpdateGroupingReadModel(new Guid[0]));
            Expect.Call(_uow.Dispose);
            _mocks.ReplayAll();
            _target.Execute(1, new Guid[0]);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallRepToUpdatePage()
        {
            Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(_uow);
            Expect.Call(() => _groupingReadOnlyRep.UpdateGroupingReadModelGroupPage(new Guid[0]));
            Expect.Call(_uow.Dispose);
            _mocks.ReplayAll();
            _target.Execute(2, new Guid[0]);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallRepToUpdateData()
        {
            Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(_uow);
            Expect.Call(() => _groupingReadOnlyRep.UpdateGroupingReadModelData(new Guid[0]));
            Expect.Call(_uow.Dispose);
            _mocks.ReplayAll();
            _target.Execute(3, new Guid[0]);
            _mocks.VerifyAll();
        }
	}
}