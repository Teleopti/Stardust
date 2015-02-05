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
		private ILogonModel _model;
		private IApplicationData _appData;
		private const string userAgent = "something";

		const string nhibConf = @"<?xml version='1.0' encoding='utf-8'?>
		<datasource>
			<hibernate-configuration xmlns='urn:nhibernate-configuration-2.2'>
				<session-factory name='Teleopti WFM'>
					<property name='connection.connection_string'>
				
		        Data Source=.;Integrated Security=SSPI;Initial Catalog=main_clone_DemoSales_TeleoptiCCC7;Current Language=us_english
		      </property>
					<property name='command_timeout'>60</property>
				</session-factory>
			</hibernate-configuration>
			<matrix name='MatrixDatamartDemo'>
				<connectionString>
				<!--WISEMETA: default='[SQL_AUTH_STRING];Initial Catalog=[DB_ANALYTICS];Current Language=us_english'-->
				Data Source=.;Integrated Security=SSPI;Initial Catalog=main_clone_DemoSales_TeleoptiAnalytics;Current Language=us_english
		    </connectionString>
			</matrix>
			<authentication>
				<logonMode>mix</logonMode>
				<!--  win or mix -->
			</authentication>
		</datasource>";

		[SetUp]
		public void Setup()
		{
			_repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			_authenticationQuerier = MockRepository.GenerateMock<IAuthenticationQuerier>();
			_dataSource = MockRepository.GenerateMock<IDataSource>();
			_appData = MockRepository.GenerateStub<IApplicationData>();
			_dataSource.Expect(x => x.DataSourceName).Return("Teleopti WFM");
			_dataSourceCont = new DataSourceContainer(_dataSource, _repositoryFactory, null, AuthenticationTypeOption.Application);
			_model = new logonModel { DataSourceContainers = new List<IDataSourceContainer> { _dataSourceCont }, UserName = "kalle", Password = "kula" };
			_target = new MultiTenancyApplicationLogon(_repositoryFactory, _authenticationQuerier);
		}

		[Test]
		public void ShouldReturnSuccessOnSuccess()
		{
			var encrypted =
				@"4gDEvx5C5+Ib30tiCBsvr+UMhdQPRTi7IX9RaZjVWMzN/78k454m3WufD9PCZjYouQAmOT+VNdJYDORaq0LXMPqyB1KZ9jJ3H3PO9ndeX6YdGlZ54qBS6uj1vbUH2c8rO3K3g7Htl1HDEN/vbjyZz1XivaymnDR4g0EOOdgAP5o6AgllqaI9W3L6qMEzx0vhbqQFl2Nb7wDoNE0ZUn+/4VKUzRb+l/iGXXoymaJP6M3FHi8fxWrSZQQOFl2Q4PQ+tFM3/BqSibcmlWe3gD3laDFTfQORm+MLPPClL06DqGwpa8bsZtXhUjoQ2yOaEQfIEp+0Da3wjYoKDM6pmNjJLTWKPrHRl184faQNhIyyA971jMDCj2NlRu7ct8X5BsHKR5eOoh+MdSnxFZA/oOxECNDwavOtQFGYegtt2r5i9wKWVlhaGuotaS63djhVuvanwH1EsvxSEV60PkDAAYUJuWcAdJTg0YpxJGQWhqMV9JwfStq26TdKv+16KSoY8zdLv7pgnhowiFV2F4P+CxNcP2FznttwZWrDu3XCFcOycEBUWsEObB9jzt1JPuzlRNk5oiP4XHh4zcWKR+g2w/AY97EQMICsunxkDeKgfONAxuOJadH0E/imG6hPLzcEg9kZn/LPAAZC4hpPWze5F4s5C2iYIUUaTIuKS7uQUvRyryqwQl987750MGnSO+Sye0e4EYruKRrrcHnmMr8jwd1znqI85f18pWL9a60W7buwzRKdJIz4Rm+IcVE/f8t/hNZp9CD5zJVbueJBGn2dZR2FmHIrfuboRp7mW00MBPoJ5acCDKxEXnxEnQg4C4dOqe+HRZg/l8OZvgDlUBazq1X6/j/YK3NfuJslB2NhlyUoxbS6OYCzPmxOslEzoJzufZBVQVO/SAre+/RUgbHGVqGtx5L6i63JK5SBTffIYDSxsY0LBCi0cIgqrw2ey64OGQEuQfs5K4cL+W2JXRFxxsYlR72rlfhFPqj5SaBFLcWKcsGF4hJtYU+iYgv5995LtMYF
zjynBDpennBSNqkqCiW3EQRWBLLUsTvYDVTukgp553hrec5dBRnZbAJDPZ1C9vxaL41gULCDALIUiNUtN9oLPUGfTzC5QPXZaIFislDqPgIbGc4P4y+juPHoQ1eIe52i71XCAAEzX1iAYDdqrBnu4WMDjLxfvRviOTHjR8c28s+Pe0x7KGTV/FCYj/Tp7F/8r2XYwSrgKHnPNe34C3YvqxnhJs4DEVFHGIeVYlR9u1bH0oKmNowu1WeDdZcOrw7zJxzlOb+TfxtQWogWD+qtSk8wTc05SXeqoQY2W7wCS1VX6PI5MQPYsgB4MKaYMIV/UniN4N/z9q4ALif/1SJwbMqQGhnIrvd4WG2Hr35KaA0MjfYWK1rZp/SOGwBUfE59imt/0nqFueF7HiDAx82P9hXhB+/ebx6FsFmfuF+Yh/ZUdkn+x12EXZ2dCsTFzmufoQG/dtakH5ubNYuW+y5eD3O6CJaSwqg3jRKBWNo89223+lOsx8G0l+Z2Zt8cnTGU8GE2FBnt/qKMZyj+7zxuDF80KUxnoE+NIgM1rg8rju8=";

			var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			var personId = Guid.NewGuid();
			var person = new Person();
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_authenticationQuerier.Stub(x => x.TryLogon("kalle", "kula", userAgent))
				.Return(new AuthenticationQueryResult { PersonId = personId, Success = true, Tenant = "Teleopti WFM", DataSourceConfig = encrypted });
			_appData.Stub(x => x.CreateAndAddDataSource("")).Return(_dataSource).IgnoreArguments();
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
				.Return(new AuthenticationQueryResult { PersonId = personId, Success = true, Tenant = "Teleopti WFM", DataSource = nhibConf });
			_appData.Stub(x => x.CreateAndAddDataSource("")).Return(_dataSource).IgnoreArguments();
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