using System;
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
	public class MultiTenancyApplicationLogonTest
	{
		private MultiTenancyApplicationLogon _target;
		private IRepositoryFactory _repositoryFactory;
		private IAuthenticationQuerier _authenticationQuerier;
		private IDataSource _dataSource;
		private DataSourceContainer _dataSourceCont;
		private logonModel _model;

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			_authenticationQuerier = MockRepository.GenerateMock<IAuthenticationQuerier>();
			_dataSource = MockRepository.GenerateMock<IDataSource>();
			_dataSource.Expect(x => x.DataSourceName).Return("Teleopti WFM");
			_dataSourceCont = new DataSourceContainer(_dataSource, _repositoryFactory, null, AuthenticationTypeOption.Application);
			_model = new logonModel { DataSourceContainers = new List<IDataSourceContainer> { _dataSourceCont }, UserName = "kalle", Password = "kula" };

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
			_authenticationQuerier.Stub(x => x.TryLogon("kalle", "kula"))
				.Return(new AuthenticationQueryResult { PersonId = personId, Success = true, Tennant = "Teleopti WFM" });
			_dataSource.Stub(x => x.Application).Return(uowFactory);
			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			_repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRepository);
			personRepository.Stub(x => x.LoadOne(personId)).Return(person);
			var result = _target.Logon(_model);

			result.Successful.Should().Be.True();
			_model.SelectedDataSourceContainer.Should().Be.EqualTo(_dataSourceCont);
			_model.UserName.Should().Be.EqualTo("kalle");
		}

		[Test]
		public void ShouldReturnFailureOnNoSuccess()
		{
			_authenticationQuerier.Stub(x => x.TryLogon("kalle", "kula"))
				.Return(new AuthenticationQueryResult { Success = false });

			var result = _target.Logon(_model);

			result.Successful.Should().Be.False();
			_model.SelectedDataSourceContainer.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnFailureIfNoMatchingDatasource()
		{
			_authenticationQuerier.Stub(x => x.TryLogon("kalle", "kula"))
				.Return(new AuthenticationQueryResult { PersonId = Guid.NewGuid(), Success = true, Tennant = "Wrong tennant" });
			var result = _target.Logon(_model);

			result.Successful.Should().Be.False();
			_model.SelectedDataSourceContainer.Should().Be.Null();
		}

		class logonModel :ILogonModel
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
		}
	}
}