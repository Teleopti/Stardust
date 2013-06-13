using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.Authentication
{
	[TestFixture]
	public class LogonLoggerTest
	{
		private MockRepository _mocks;
		IRepositoryFactory _repositoryFactory;
		IUnitOfWorkFactory _unitOfWorkFactory;
		private LogonLogger _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_repositoryFactory = _mocks.DynamicMock<IRepositoryFactory>();
			_unitOfWorkFactory = _mocks.DynamicMock<IUnitOfWorkFactory>();
			_target = new LogonLogger(_repositoryFactory);
		}

		[Test]
		public void ShouldSaveLogonAttempt()
		{
			var uow = _mocks.DynamicMock<IUnitOfWork>();
			var rep = _mocks.DynamicMock<IPersonRepository>();
			var model = new LoginAttemptModel();

			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
			Expect.Call(_repositoryFactory.CreatePersonRepository(uow)).Return(rep);
			Expect.Call(rep.SaveLoginAttempt(model)).Return(1);
			Expect.Call(uow.PersistAll());
			Expect.Call(uow.Dispose);
			_mocks.ReplayAll();
			_target.SaveLogonAttempt(model, _unitOfWorkFactory);
			_mocks.VerifyAll();
		}
	}

	
}