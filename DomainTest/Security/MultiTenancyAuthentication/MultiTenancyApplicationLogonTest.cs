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

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			_authenticationQuerier = MockRepository.GenerateMock<IAuthenticationQuerier>();
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_person = PersonFactory.CreatePerson("kalle");
			_target = new MultiTenancyApplicationLogon(_repositoryFactory, _authenticationQuerier);
		}

		[Test]
		public void ShouldReturnSuccessOnSuccess()
		{
			var dataSourceCont = MockRepository.GenerateMock<IDataSourceContainer>();
			var dataSource = MockRepository.GenerateMock<IDataSource>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var model = new logonModel {DataSourceContainers = new List<IDataSourceContainer> {dataSourceCont}, UserName = "kalle", Password = "kula"};
			var personId = Guid.NewGuid();

			dataSourceCont.Stub(x => x.AuthenticationTypeOption).Return(AuthenticationTypeOption.Application);
			_authenticationQuerier.Stub(x => x.TryLogon("kalle", "kula"))
				.Return(new AuthenticationQueryResult {PersonId = personId, Success = true, Tennant = "WFM"});
			dataSourceCont.Stub(x => x.DataSourceName).Return("WFM");
			dataSourceCont.Stub(x => x.DataSource).Return(dataSource);
			dataSource.Stub(x => x.Application).Return(unitOfWorkFactory);
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			_repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(_personRepository);
			_personRepository.Stub(x => x.LoadOne(personId)).Return(_person);
			dataSourceCont.Expect(x => x.SetUser(_person));
			var result = _target.Logon(model);

			result.Successful.Should().Be.True();
			model.SelectedDataSourceContainer.Should().Be.EqualTo(dataSourceCont);
			model.UserName.Should().Be.EqualTo("kalle");

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