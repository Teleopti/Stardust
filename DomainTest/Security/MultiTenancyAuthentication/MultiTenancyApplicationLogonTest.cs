using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.TestCommon.FakeData;
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
		private IPersonRepository _personRepository;
		private IPerson _person;
		private IDataSourceContainer _dataSourceCont;
		private IDataSource _dataSource;
		private IUnitOfWork _uow;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private logonModel _model;
		private Guid _personId;

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			_authenticationQuerier = MockRepository.GenerateMock<IAuthenticationQuerier>();
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_person = PersonFactory.CreatePerson("kalle");
			_dataSourceCont = MockRepository.GenerateMock<IDataSourceContainer>();
			_dataSource = MockRepository.GenerateMock<IDataSource>();
			_unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_uow = MockRepository.GenerateMock<IUnitOfWork>();
			_model = new logonModel { DataSourceContainers = new List<IDataSourceContainer> { _dataSourceCont }, UserName = "kalle", Password = "kula" };
			_personId = Guid.NewGuid();

			_target = new MultiTenancyApplicationLogon(_repositoryFactory, _authenticationQuerier);
		}

		[Test]
		public void ShouldReturnSuccessOnSuccess()
		{
			_dataSourceCont.Stub(x => x.AuthenticationTypeOption).Return(AuthenticationTypeOption.Application);
			_authenticationQuerier.Stub(x => x.TryLogon("kalle", "kula"))
				.Return(new AuthenticationQueryResult {PersonId = _personId, Success = true, Tennant = "WFM"});
			_dataSourceCont.Stub(x => x.DataSourceName).Return("WFM");
			_dataSourceCont.Stub(x => x.DataSource).Return(_dataSource);
			_dataSource.Stub(x => x.Application).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_uow);
			_repositoryFactory.Stub(x => x.CreatePersonRepository(_uow)).Return(_personRepository);
			_personRepository.Stub(x => x.LoadOne(_personId)).Return(_person);
			_dataSourceCont.Expect(x => x.SetUser(_person));
			var result = _target.Logon(_model);

			result.Successful.Should().Be.True();
			_model.SelectedDataSourceContainer.Should().Be.EqualTo(_dataSourceCont);
			_model.UserName.Should().Be.EqualTo("kalle");
		}

		[Test]
		public void ShouldReturnFailureOnNoSuccess()
		{
			_dataSourceCont.Stub(x => x.AuthenticationTypeOption).Return(AuthenticationTypeOption.Application);
			_authenticationQuerier.Stub(x => x.TryLogon("kalle", "kula"))
				.Return(new AuthenticationQueryResult { Success = false });

			var result = _target.Logon(_model);

			result.Successful.Should().Be.False();
			_model.SelectedDataSourceContainer.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnFailureIfNoMatchingDatasource()
		{
			_dataSourceCont.Stub(x => x.AuthenticationTypeOption).Return(AuthenticationTypeOption.Application);
			_authenticationQuerier.Stub(x => x.TryLogon("kalle", "kula"))
				.Return(new AuthenticationQueryResult { PersonId = _personId, Success = true, Tennant = "WFM" });
			_dataSourceCont.Stub(x => x.DataSourceName).Return("Not WFM");
			_dataSourceCont.Stub(x => x.DataSource).Return(_dataSource);
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
				throw new System.NotImplementedException();
			}
		}
	}
}