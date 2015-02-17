﻿using System;
using System.Collections.Generic;
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
	public class MultiTenancyIdentityLogonTest
	{
		private MultiTenancyWindowsLogon _target;
		private IRepositoryFactory _repositoryFactory;
		private IAuthenticationQuerier _authenticationQuerier;
		private IDataSource _dataSource;
		private DataSourceContainer _dataSourceCont;
		private ILogonModel _model;
		private IApplicationData _appData;
		private IWindowsUserProvider _windowsUserProvider;
		private const string userAgent = "something";

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			_authenticationQuerier = MockRepository.GenerateMock<IAuthenticationQuerier>();
			_windowsUserProvider = MockRepository.GenerateMock<IWindowsUserProvider>();
			_dataSource = MockRepository.GenerateMock<IDataSource>();
			_appData = MockRepository.GenerateStub<IApplicationData>();
			_dataSource.Expect(x => x.DataSourceName).Return("Teleopti WFM");
			_dataSourceCont = new DataSourceContainer(_dataSource, _repositoryFactory, null, AuthenticationTypeOption.Application);
			_model = new logonModel { DataSourceContainers = new List<IDataSourceContainer> { _dataSourceCont }, UserName = "kalle", Password = "kula" };
			_target = new MultiTenancyWindowsLogon(_repositoryFactory, _authenticationQuerier, _windowsUserProvider);
		}

		[Test]
		public void ShouldReturnSuccessOnSuccess()
		{
			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var personId = Guid.NewGuid();
			var person = new Person();
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_windowsUserProvider.Stub(x => x.DomainName).Return("TOPTINET");
			_windowsUserProvider.Stub(x => x.UserName).Return("KULA");
			_authenticationQuerier.Stub(x => x.TryIdentityLogon("TOPTINET\\KULA", userAgent))
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
			_windowsUserProvider.Stub(x => x.DomainName).Return("TOPTINET");
			_windowsUserProvider.Stub(x => x.UserName).Return("KULA");
			_authenticationQuerier.Stub(x => x.TryIdentityLogon("TOPTINET\\KULA", userAgent)).Return(new AuthenticationQueryResult { Success = false });

			var result = _target.Logon(_model, _appData, userAgent);

			result.Successful.Should().Be.False();
			_model.SelectedDataSourceContainer.Should().Be.Null();
		}
	}
	class logonModel : ILogonModel
	{
		public bool GetConfigFromWebService { get; set; }
		public IList<IDataSourceContainer> DataSourceContainers { get; set; }
		public IDataSourceContainer SelectedDataSourceContainer { get; set; }
		public IList<string> Sdks { get; set; }
		public string SelectedSdk { get; set; }
		public IList<IBusinessUnit> AvailableBus { get; set; }
		public IBusinessUnit SelectedBu { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public bool HasValidLogin()
		{
			throw new NotImplementedException();
		}

		public AuthenticationTypeOption AuthenticationType { get; set; }
		public string Warning { get; set; }
	}
}