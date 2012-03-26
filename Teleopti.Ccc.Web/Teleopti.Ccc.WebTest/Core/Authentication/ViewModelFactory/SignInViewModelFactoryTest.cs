using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Authentication.ViewModelFactory
{
	[TestFixture]
	public class SignInViewModelFactoryTest
	{
		private MockRepository _mocks;
		private IAuthenticationViewModelFactory _target;
		private IDataSourcesProvider _dataSourceProvider;
		private IBusinessUnitProvider _businessUnitProvider;
		private List<IDataSource> _applDataSources;
		private List<IDataSource> _winDataSources;
		private IPerson _person;
		private IDataSource _datasoure;
		private List<IBusinessUnit> _businessUnits;
		private IBusinessUnit _bu1;

		[SetUp]
		public void Setup()
		{
			_person = new Person();
			_person.SetId(Guid.NewGuid());
			_datasoure = new testDataSource("ds");
			_mocks = new MockRepository();
			_dataSourceProvider = _mocks.DynamicMock<IDataSourcesProvider>();
			_businessUnitProvider = _mocks.DynamicMock<IBusinessUnitProvider>();

			_bu1 = _mocks.DynamicMock<IBusinessUnit>();

			_target = new AuthenticationViewModelFactory(_dataSourceProvider, _businessUnitProvider);

			_applDataSources = new List<IDataSource>
			                   	{
			                   		new testDataSource("ds1"),
			                   		new testDataSource("ds2")
			                   	};
			_winDataSources = new List<IDataSource>
			                  	{
			                  		new testDataSource("wds1")
			                  	};

			_businessUnits = new List<IBusinessUnit>
			                 	{
			                 		_bu1
			                 	};
		}

		[Test]
		public void ShouldCreateBusinessViewModel()
		{
			const string bu1Name = "bu1";
			var bu1Id = Guid.NewGuid();
			using (_mocks.Record())
			{
				Expect.Call(_bu1.Name).Return(bu1Name);
				Expect.Call(_bu1.Id).Return(bu1Id);

				Expect.Call(_businessUnitProvider.RetrieveBusinessUnitsForPerson(_datasoure, _person)).Return(_businessUnits);
			}
			using (_mocks.Playback())
			{
				var result = _target.CreateBusinessUnitViewModel(_datasoure, _person, AuthenticationTypeOption.Windows);

				result.HasBusinessUnits.Should().Be.True();
				result.BusinessUnits.Select(x => x.Name).Should().Have.SameValuesAs(_businessUnits.Select(x => x.Name));
				result.SignIn.AuthenticationType.Should().Be.EqualTo(AuthenticationTypeOption.Windows);
			}
		}

		[Test]
		public void ShouldSetBusinessUnitModelBusinessUnitIdToFirstBusinessUnitId()
		{
			const string bu1Name = "bu1";
			var bu1Id = Guid.NewGuid();
			using (_mocks.Record())
			{
				Expect.Call(_bu1.Name).Return(bu1Name);
				Expect.Call(_bu1.Id).Return(bu1Id);

				Expect.Call(_businessUnitProvider.RetrieveBusinessUnitsForPerson(_datasoure, _person)).Return(_businessUnits);
			}
			using (_mocks.Playback())
			{
				var result = _target.CreateBusinessUnitViewModel(_datasoure, _person, AuthenticationTypeOption.Application);
				result.SignIn.BusinessUnitId.Should().Not.Be.EqualTo(Guid.Empty);
				result.SignIn.BusinessUnitId.Should().Be.EqualTo(_businessUnits.First().Id);
				result.SignIn.AuthenticationType.Should().Be.EqualTo(AuthenticationTypeOption.Application);
			}
		}

		[Test]
		public void ShouldCreateSignInViewModel()
		{
			using (_mocks.Record())
			{
				Expect.Call(_dataSourceProvider.RetrieveDatasourcesForApplication()).Return(_applDataSources);
				Expect.Call(_dataSourceProvider.RetrieveDatasourcesForWindows()).Return(_winDataSources);
			}

			using (_mocks.Playback())
			{
				var result = _target.CreateSignInViewModel();

				result.HasApplicationSignIn.Should().Be.True();
				result.ApplicationSignIn.HasDataSource.Should().Be.True();
				result.ApplicationSignIn.DataSources.Select(x => x.Name).Should().Have.SameValuesAs(
					_applDataSources.Select(x => x.DataSourceName));

				result.HasWindowsSignIn.Should().Be.True();
				result.WindowsSignIn.HasDataSource.Should().Be.True();
				result.WindowsSignIn.DataSources.Select(x => x.Name).Should().Have.SameValuesAs(
					_winDataSources.Select(x => x.DataSourceName));
			}
		}


		[Test]
		public void ShouldCreateSignInApplicationViewModel()
		{
			var postModel = new SignInApplicationModel();
			using (_mocks.Record())
			{
				Expect.Call(_dataSourceProvider.RetrieveDatasourcesForApplication()).Return(_applDataSources);
			}

			using (_mocks.Playback())
			{
				var result = _target.CreateSignInApplicationViewModel(postModel);

				result.HasDataSource.Should().Be.True();
				result.DataSources.Select(x => x.Name).Should().Have.SameValuesAs(_applDataSources.Select(x => x.DataSourceName));
				result.SignIn.Should().Be.SameInstanceAs(postModel);
			}
		}
		[Test]
		public void ShouldSetApplicationModelDatasourceToFirstDataSourceNameIfEmpty()
		{
			var postModel = new SignInApplicationModel();
			using (_mocks.Record())
			{
				Expect.Call(_dataSourceProvider.RetrieveDatasourcesForApplication()).Return(_applDataSources);
			}

			using (_mocks.Playback())
			{
				var result = _target.CreateSignInApplicationViewModel(postModel);
				result.SignIn.DataSourceName.Should().Be.EqualTo(_applDataSources.First().DataSourceName);
			}
		}
		[Test]
		public void ShouldLeaveApplicationModelDatasourceNameIfNonEmpty()
		{
			var postModel = new SignInApplicationModel {DataSourceName = "Allready set"};
			using (_mocks.Record())
			{
				Expect.Call(_dataSourceProvider.RetrieveDatasourcesForApplication()).Return(_applDataSources);
			}

			using (_mocks.Playback())
			{
				var result = _target.CreateSignInApplicationViewModel(postModel);
				result.SignIn.DataSourceName.Should().Be.EqualTo(postModel.DataSourceName);
			}
		}
		[Test]
		public void ShouldSetWindowsModelDatasourceToFirstDataSourceNameIfEmpty()
		{
			var postModel = new SignInWindowsModel();
			using (_mocks.Record())
			{
				Expect.Call(_dataSourceProvider.RetrieveDatasourcesForWindows()).Return(_applDataSources);
			}

			using (_mocks.Playback())
			{
				var result = _target.CreateSignInWindowsViewModel(postModel);
				result.SignIn.DataSourceName.Should().Be.EqualTo(_applDataSources.First().DataSourceName);
			}
		}
		[Test]
		public void ShouldLeaveWindowsModelDatasourceNameIfNonEmpty()
		{
			var postModel = new SignInWindowsModel { DataSourceName = "Allready set" };
			using (_mocks.Record())
			{
				Expect.Call(_dataSourceProvider.RetrieveDatasourcesForWindows()).Return(_applDataSources);
			}

			using (_mocks.Playback())
			{
				var result = _target.CreateSignInWindowsViewModel(postModel);
				result.SignIn.DataSourceName.Should().Be.EqualTo(postModel.DataSourceName);
			}
		}

		

		


		[Test]
		public void ShouldCreateWindowsSignInViewModel()
		{
			var postModel = new SignInWindowsModel();

			using (_mocks.Record())
			{
				Expect.Call(_dataSourceProvider.RetrieveDatasourcesForWindows()).Return(_winDataSources);
			}

			using (_mocks.Playback())
			{
				var result = _target.CreateSignInWindowsViewModel(postModel);

				result.HasDataSource.Should().Be.True();
				result.DataSources.Select(x => x.Name).Should().Have.SameValuesAs(_winDataSources.Select(x => x.DataSourceName));
				result.SignIn.Should().Be.SameInstanceAs(postModel);
			}
		}

		private class testDataSource : IDataSource
		{
			private readonly string _name;

			public testDataSource(string name)
			{
				_name = name;
			}

			public IUnitOfWorkFactory Statistic
			{
				get { throw new NotImplementedException(); }
			}


			public IUnitOfWorkFactory Application
			{
				get { throw new NotImplementedException(); }
			}


			public IAuthenticationSettings AuthenticationSettings
			{
				get { throw new NotImplementedException(); }
			}


			public string DataSourceName
			{
				get { return _name; }
			}


			public void ResetStatistic()
			{
				throw new NotImplementedException();
			}

            public string Server { get; set; }

            public string InitialCatalog { get; set; }
			public string OriginalFileName { get; set; }
			public AuthenticationTypeOption AuthenticationTypeOption { get; set; }

			public void Dispose()
			{
			}
		}
	}
}