//using System;
//using System.Collections.Generic;
//using NUnit.Framework;
//using Rhino.Mocks;
//using SharpTestsEx;
//using Teleopti.Ccc.Domain.Common;
//using Teleopti.Ccc.Domain.Repositories;
//using Teleopti.Ccc.Domain.Security;
//using Teleopti.Ccc.Domain.Security.Authentication;
//using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
//using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
//using Teleopti.Ccc.TestCommon.FakeRepositories;
//using Teleopti.Interfaces.Domain;
//using Teleopti.Interfaces.Infrastructure;

//namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Client
//{
//	[TestFixture]
//	[TenantClientTest]
//	public class MultiTenancyApplicationLogonTest
//	{
//		public PostHttpRequestFake HttpRequestFake;
//		public IMultiTenancyApplicationLogon Target;
//		public FakePersonRepository PersonRepository;

//		private const string userAgent = "something";

//		[Test]
//		public void ShouldReturnSuccessOnSuccess()
//		{
//			var dataSource = MockRepository.GenerateMock<IDataSource>();
//			dataSource.Expect(x => x.DataSourceName).Return("[not set]");
//			var repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
//			var applicationData = new ApplicationDataFake();
//			applicationData.SetDataSource(dataSource);
//			var model = new LogonModel { UserName = "kalle", Password = "kula" };
//			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
//			var uow = MockRepository.GenerateMock<IUnitOfWork>();
//			var personId = Guid.NewGuid();
//			var person = new Person();
//			person.SetId(personId);
//			PersonRepository.Has(person);
//			var dataSourceCfg = new DataSourceConfig
//			{
//				AnalyticsConnectionString = Encryption.EncryptStringToBase64("constrringtoencrypt", EncryptionConstants.Image1, EncryptionConstants.Image2),
//				ApplicationNHibernateConfig = new Dictionary<string, string>()
//			};

//			var queryResult = new AuthenticationQueryResult
//			{
//				PersonId = personId,
//				Success = true,
//				Tenant = "[not set]",
//				DataSourceConfiguration = dataSourceCfg
//			};
//			HttpRequestFake.SetReturnValue(queryResult);
			
//			//var personRepository = MockRepository.GenerateMock<IPersonRepository>();
//			dataSource.Stub(x => x.Application).Return(uowFactory);
//			uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
//			//repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRepository);
//			//PersonRepository.Stub(x => x.LoadPersonAndPermissions(personId)).Return(person);
//			//PersonRepository.Stub(x => x.Get(personId)).Return(person);
//			var result = Target.Logon(model, applicationData,userAgent);

//			result.Successful.Should().Be.True();
//			model.SelectedDataSourceContainer.User.Should().Be.EqualTo(person);

//		}

//		//[Test]
//		//public void ShouldReturnSuccessOnSuccessFromFile()
//		//{
//		//	var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
//		//	var uow = MockRepository.GenerateMock<IUnitOfWork>();
//		//	var personId = Guid.NewGuid();
//		//	var person = new Person();
//		//	var dataSourceCfg = new DataSourceConfig
//		//	{
//		//		AnalyticsConnectionString = "",
//		//		ApplicationNHibernateConfig = new Dictionary<string, string>()
//		//	};
//		//	var personRepository = MockRepository.GenerateMock<IPersonRepository>();
//		//	_authenticationQuerier.Stub(x => x.TryApplicationLogon("kalle", "kula", userAgent))
//		//		.Return(new AuthenticationQueryResult { PersonId = personId, Success = true, Tenant = "Teleopti WFM", DataSourceConfiguration = dataSourceCfg });
//		//	_dataSourcesFactory.Stub(x => x.Create(dataSourceCfg.ApplicationNHibernateConfig, dataSourceCfg.AnalyticsConnectionString)).Return(_dataSource);
//		//	_dataSource.Stub(x => x.Application).Return(uowFactory);
//		//	uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
//		//	_repositoryFactory.Stub(x => x.CreatePersonRepository(uow)).Return(personRepository);
//		//	personRepository.Stub(x => x.LoadOne(personId)).Return(person);
//		//	var result = _target.Logon(_model, userAgent);

//		//	result.Successful.Should().Be.True();
//		//	_model.SelectedDataSourceContainer.User.Should().Be.EqualTo(person);

//		//}

//		[Test]
//		public void ShouldReturnFailureOnNoSuccess()
//		{
//			var applicationData = new ApplicationDataFake();
//			var queryResult = new AuthenticationQueryResult {Success = false};
//			HttpRequestFake.SetReturnValue(queryResult);
//			var model = new LogonModel { UserName = "kalle", Password = "kula" };

//			var result = Target.Logon(model, applicationData, userAgent);

//			result.Successful.Should().Be.False();
//			model.SelectedDataSourceContainer.Should().Be.Null();
//		}
//	}
//}