using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory
{
	public class AuthenticationViewModelFactory : IAuthenticationViewModelFactory
	{
		private readonly IBusinessUnitProvider _businessUnitProvider;
		private readonly IDataSourcesProvider _dataSourceProvider;

		public AuthenticationViewModelFactory(IDataSourcesProvider dataSourceProvider,
		                                      IBusinessUnitProvider businessUnitProvider)
		{
			_dataSourceProvider = dataSourceProvider;
			_businessUnitProvider = businessUnitProvider;
		}

		public SignInViewModel CreateSignInViewModel()
		{
			return new SignInViewModel
			       	{
			       		ApplicationSignIn = createSignInApplicationViewModelWithDefault(null),
			       		WindowsSignIn = createSignInWindowsViewModelWithDefault(null)
			       	};
		}

		public SignInWindowsViewModel CreateSignInWindowsViewModel(SignInWindowsModel model)
		{
			return createSignInWindowsViewModelWithDefault(model);
		}

		private SignInWindowsViewModel createSignInWindowsViewModelWithDefault(SignInWindowsModel model)
		{
			model = model ?? new SignInWindowsModel();
			var windowsDataSourceViewModels = this.windowsDataSourceViewModels();
			if (string.IsNullOrEmpty(model.DataSourceName) && windowsDataSourceViewModels.AnyOrFalse())
			{
				model.DataSourceName = windowsDataSourceViewModels.First().Name;
			}
			
			return new SignInWindowsViewModel {SignIn = model, DataSources = windowsDataSourceViewModels};
		}

		public SignInApplicationViewModel CreateSignInApplicationViewModel(SignInApplicationModel model)
		{
			return createSignInApplicationViewModelWithDefault(model);
		}

		private SignInApplicationViewModel createSignInApplicationViewModelWithDefault(SignInApplicationModel model)
		{
			model = model ?? new SignInApplicationModel();
			var applicationDataSourceViewModels = this.applicationDataSourceViewModels();
			if (string.IsNullOrEmpty(model.DataSourceName) && applicationDataSourceViewModels.AnyOrFalse())
			{
				model.DataSourceName = applicationDataSourceViewModels.First().Name;
			}

			return new SignInApplicationViewModel {SignIn = model, DataSources = applicationDataSourceViewModels};
		}

		public SignInBusinessUnitViewModel CreateBusinessUnitViewModel(IDataSource dataSource, IPerson person)
		{
			var businessUnitViewModels = this.businessUnitViewModels(dataSource, person);
			
			var signInBusinessUnitModel = new SignInBusinessUnitModel
			                              	{
			                              		BusinessUnitId = businessUnitViewModels.AnyOrFalse() ? businessUnitViewModels.First().Id : Guid.Empty,
			                              		DataSourceName = dataSource.DataSourceName,
			                              		PersonId = person.Id.Value
			                              	};
			return new SignInBusinessUnitViewModel
			       	{
			       		BusinessUnits = businessUnitViewModels,
			       		SignIn = signInBusinessUnitModel
			       	};
		}

		private IEnumerable<DataSourceViewModel> applicationDataSourceViewModels()
		{
			return _dataSourceProvider.
				RetrieveDatasourcesForApplication()
				.SelectOrEmpty(
					x => new DataSourceViewModel { Name = x.DataSourceName }).ToList();
		}

		private IEnumerable<DataSourceViewModel> windowsDataSourceViewModels()
		{
			return _dataSourceProvider.
				RetrieveDatasourcesForWindows()
				.SelectOrEmpty(
					x => new DataSourceViewModel {Name = x.DataSourceName}).ToList();
		}

		private IEnumerable<BusinessUnitViewModel> businessUnitViewModels(IDataSource dataSource, IPerson person)
		{
			var businessUnits = _businessUnitProvider.RetrieveBusinessUnitsForPerson(dataSource, person);

			return businessUnits.SelectOrEmpty(
				bu => new BusinessUnitViewModel
				      	{
				      		Id = bu.Id.Value,
				      		Name = bu.Name
				      	}
				);
		}
	}
}