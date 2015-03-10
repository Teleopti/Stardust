//using System;
//using System.Collections.Generic;
//using NUnit.Framework;
//using Rhino.Mocks;
//using SharpTestsEx;
//using Teleopti.Ccc.Domain.Common;
//using Teleopti.Ccc.Domain.Repositories;
//using Teleopti.Ccc.Domain.Security.Authentication;
//using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
//using Teleopti.Ccc.Infrastructure.MultiTenancy;
//using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
//using Teleopti.Ccc.Infrastructure.UnitOfWork;
//using Teleopti.Interfaces.Domain;
//using Teleopti.Interfaces.Infrastructure;

//namespace Teleopti.Ccc.DomainTest.Security.MultiTenancyAuthentication
//{
//	[TestFixture]
//	public class MultiTenancyIdentityLogonTest
//	{
//		private MultiTenancyWindowsLogon _target;
//		private IRepositoryFactory _repositoryFactory;
//		private IAuthenticationQuerier _authenticationQuerier;
//		private IDataSource _dataSource;
//		private ILogonModel _model;
//		private IDataSourcesFactory _dataSourcesFactory;
//		private IWindowsUserProvider _windowsUserProvider;
//		private const string userAgent = "something";

//		[SetUp]
//		public void Setup()
//		{
//			_repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
//			_authenticationQuerier = MockRepository.GenerateMock<IAuthenticationQuerier>();
//			_windowsUserProvider = MockRepository.GenerateMock<IWindowsUserProvider>();
//			_dataSource = MockRepository.GenerateMock<IDataSource>();
//			_dataSourcesFactory = MockRepository.GenerateMock<IDataSourcesFactory>();
		
			
//			_dataSource.Expect(x => x.DataSourceName).Return("Teleopti WFM");
//			_model = new LogonModel { UserName = "kalle", Password = "kula" };
//			_target = new MultiTenancyWindowsLogon(_repositoryFactory, _authenticationQuerier, _windowsUserProvider, _dataSourcesFactory);
//		}

//		[Test]
//		public void ShouldReturnSuccessOnSuccess()
//		{
//			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
//			var uow = MockRepository.GenerateMock<IUnitOfWork>();
//			var personId = Guid.NewGuid();
//			var person = new Person();
//			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
//			var dataSourceCfg = new DataSourceConfig
//			{
//				AnalyticsConnectionString = "",
//				ApplicationNHibernateConfig = new Dictionary<string, string>()
//			};
//			_windowsUserProvider.Stub(x => x.DomainName).Return("TOPTINET");
//			_windowsUserProvider.Stub(x => x.UserName).Return("KULA");
//			_authenticationQuerier.Stub(x => x.TryIdentityLogon("TOPTINET\\KULA", userAgent))
//				.Return(new AuthenticationQueryResult { PersonId = personId, Success = true, Tenant = "Teleopti WFM", DataSourceConfiguration = dataSourceCfg });
//			_dataSourcesFactory.Stub(x => x.Create(dataSourceCfg.ApplicationNHibernateConfig, dataSourceCfg.AnalyticsConnectionString)).Return(_dataSource);
//			_dataSource.Stub(x => x.Application).Return(uowFactory);
//			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
//			_repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRepository);
//			personRepository.Stub(x => x.LoadOne(personId)).Return(person);
//			var result = _target.Logon(_model, userAgent);

//			result.Successful.Should().Be.True();
//			_model.SelectedDataSourceContainer.User.Should().Be.EqualTo(person);
			
//		}

//		[Test]
//		public void ShouldReturnFailureOnNoSuccess()
//		{
//			_windowsUserProvider.Stub(x => x.DomainName).Return("TOPTINET");
//			_windowsUserProvider.Stub(x => x.UserName).Return("KULA");
//			_authenticationQuerier.Stub(x => x.TryIdentityLogon("TOPTINET\\KULA", userAgent)).Return(new AuthenticationQueryResult { Success = false });

//			var result = _target.Logon(_model, userAgent);

//			result.Successful.Should().Be.False();
//			_model.SelectedDataSourceContainer.Should().Be.Null();
//		}
//	}
	
//}