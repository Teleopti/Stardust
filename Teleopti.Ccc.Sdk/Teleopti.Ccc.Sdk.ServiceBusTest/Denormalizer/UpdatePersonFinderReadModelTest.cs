using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Denormalizer;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Denormalizer
{
	[TestFixture]
	public class UpdatePersonFinderReadModelTest
	{
		private MockRepository _mocks;
		private UpdatePersonFinderReadModel _target;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IPersonFinderReadOnlyRepository _finderReadOnlyRep;
		private IStatelessUnitOfWork _uow;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_finderReadOnlyRep = _mocks.StrictMock<IPersonFinderReadOnlyRepository>();
			_uow = _mocks.StrictMock<IStatelessUnitOfWork>();

			_target = new UpdatePersonFinderReadModel(_unitOfWorkFactory,_finderReadOnlyRep);
		}

		[Test]
		public void ShouldCallRepToUpdatePersons()
		{
			Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(_uow);
			Expect.Call(() => _finderReadOnlyRep.UpdateFindPerson("IDS"));
			Expect.Call(_uow.Dispose);
			_mocks.ReplayAll();
			_target.Execute(true, "IDS");
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldCallRepToUpdatePage()
		{
			Expect.Call(_unitOfWorkFactory.CreateAndOpenStatelessUnitOfWork()).Return(_uow);
			Expect.Call(() => _finderReadOnlyRep.UpdateFindPersonData("IDS"));
			Expect.Call(_uow.Dispose);
			_mocks.ReplayAll();
			_target.Execute(false, "IDS");
			_mocks.VerifyAll();
		}
	}

}