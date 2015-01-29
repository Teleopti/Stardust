using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Authentication.Services
{
	public class LogLogonAttemptTest
	{
		//copied from old authenticator test - will be removed alltogether later so just keep as is
		[Test]
		public void ShouldSaveAuthenticateResult()
		{
			var repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			var modelFactory = MockRepository.GenerateStub<ILoginAttemptModelFactory>();
			var target = new LogLogonAttempt(modelFactory, repositoryFactory);

			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var personRep = MockRepository.GenerateMock<IPersonRepository>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var result = new AuthenticateResult { Successful = false, DataSource = dataSource };

			var model = new LoginAttemptModel
			{
				ClientIp = "",
				Provider = "Application",
				UserCredentials = "hej",
				Result = "LogonSuccess"
			};

			modelFactory.Create("hej", null, result.Successful);
			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRep);
			personRep.Stub(x => x.SaveLoginAttempt(model)).IgnoreArguments().Return(1);

			target.SaveAuthenticateResult("hej", result);
		} 
	}
}