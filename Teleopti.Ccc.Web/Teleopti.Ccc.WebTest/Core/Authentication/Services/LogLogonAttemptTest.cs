using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Authentication.Services
{
	public class LogLogonAttemptTest
	{
		//copied from old authenticator test
		[Test]
		public void ShouldSaveAuthenticateResult()
		{
			var repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			var tokenIdentityProvider = MockRepository.GenerateMock<ITokenIdentityProvider>();
			var ipFinder = MockRepository.GenerateMock<IIpAddressResolver>();
			var target = new LogLogonAttempt(tokenIdentityProvider, repositoryFactory, ipFinder);

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

			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			ipFinder.Stub(x => x.GetIpAddress()).IgnoreArguments().Return("");
			repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRep);
			personRep.Stub(x => x.SaveLoginAttempt(model)).IgnoreArguments().Return(1);

			target.SaveAuthenticateResult("hej", result);
		} 
	}
}