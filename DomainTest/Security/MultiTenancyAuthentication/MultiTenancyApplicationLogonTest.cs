using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.MultiTenancyAuthentication
{
	[TestFixture]
	public class MultiTenancyApplicationLogonTest
	{
		private MultiTenancyApplicationLogon _target;
		private IRepositoryFactory _repositoryFactory;
		private IAuthenticationQuerier _authenticationQuerier;
		private IDataSource _dataSource;
		private ILogonModel _model;
		private IApplicationData _appData;
		private const string userAgent = "something";

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			_authenticationQuerier = MockRepository.GenerateMock<IAuthenticationQuerier>();
			_dataSource = MockRepository.GenerateMock<IDataSource>();
			_appData = MockRepository.GenerateStub<IApplicationData>();
			_dataSource.Expect(x => x.DataSourceName).Return("Teleopti WFM");
			_model = new LogonModel { UserName = "kalle", Password = "kula" };
			_target = new MultiTenancyApplicationLogon(_repositoryFactory, _authenticationQuerier);
		}

		[Test]
		public void ShouldReturnSuccessOnSuccess()
		{
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var personId = Guid.NewGuid();
			var person = new Person();
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_authenticationQuerier.Stub(x => x.TryLogon("kalle", "kula", userAgent))
				.Return(new AuthenticationQueryResult { PersonId = personId, Success = true, Tenant = "Teleopti WFM", DataSourceConfiguration = new DataSourceConfig()});
			_appData.Stub(x => x.CreateAndAddDataSource(null, null, null)).Return(_dataSource).IgnoreArguments();
			_dataSource.Stub(x => x.Application).Return(uowFactory);
			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			_repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRepository);
			personRepository.Stub(x => x.LoadOne(personId)).Return(person);
			var result = _target.Logon(_model, _appData, userAgent);

			result.Successful.Should().Be.True();
			_model.SelectedDataSourceContainer.User.Should().Be.EqualTo(person);
			
		}

		[Test]
		public void ShouldReturnSuccessOnSuccessFromFile()
		{
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var personId = Guid.NewGuid();
			var person = new Person();
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_authenticationQuerier.Stub(x => x.TryLogon("kalle", "kula", userAgent))
				.Return(new AuthenticationQueryResult { PersonId = personId, Success = true, Tenant = "Teleopti WFM", DataSourceConfiguration = new DataSourceConfig() });
			_appData.Stub(x => x.CreateAndAddDataSource(null,null,null)).Return(_dataSource).IgnoreArguments();
			_dataSource.Stub(x => x.Application).Return(uowFactory);
			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			_repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRepository);
			personRepository.Stub(x => x.LoadOne(personId)).Return(person);
			var result = _target.Logon(_model, _appData, userAgent);

			result.Successful.Should().Be.True();
			_model.SelectedDataSourceContainer.User.Should().Be.EqualTo(person);

		}

		[Test]
		public void ShouldReturnFailureOnNoSuccess()
		{
			_authenticationQuerier.Stub(x => x.TryLogon("kalle", "kula", userAgent))
				.Return(new AuthenticationQueryResult { Success = false });

			var result = _target.Logon(_model, _appData, userAgent);

			result.Successful.Should().Be.False();
			_model.SelectedDataSourceContainer.Should().Be.Null();
		}
	}
}