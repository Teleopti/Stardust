using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Http;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Permissions.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPermissions)]
	public class ApplicationFunctionsController : ApiController
	{
		private readonly IApplicationFunctionsToggleFilter _applicationFunctionsToggleFilter;
		private readonly IInitializeLicenseServiceForTenant _initializeLicenseServiceForTenant;
		private readonly ICurrentDataSource _dataSource;

		public ApplicationFunctionsController(IApplicationFunctionsToggleFilter applicationFunctionsToggleFilter, IInitializeLicenseServiceForTenant initializeLicenseServiceForTenant, ICurrentDataSource dataSource)
		{
			_applicationFunctionsToggleFilter = applicationFunctionsToggleFilter;
			_initializeLicenseServiceForTenant = initializeLicenseServiceForTenant;
			_dataSource = dataSource;
		}

		[HttpGet, Route("api/Permissions/ApplicationFunctions")]
		public virtual ICollection<ApplicationFunctionViewModel> GetAllFunctions()
		{
			_initializeLicenseServiceForTenant.TryInitialize(_dataSource.Current());
			using (_dataSource.Current().Application.CreateAndOpenUnitOfWork())
			{
				var functions = _applicationFunctionsToggleFilter.FilteredFunctions();
				return createAllFunctionsHierarchy(functions.Functions);
			}
		}

		private ICollection<ApplicationFunctionViewModel> createAllFunctionsHierarchy(ICollection<SystemFunction> functions)
		{
			var result = new Collection<ApplicationFunctionViewModel>();
			foreach (var applicationFunction in functions)
			{
				if (applicationFunction.Hidden) continue;

				var function = new ApplicationFunctionViewModel
				{
					FunctionCode = applicationFunction.Function.FunctionCode,
					FunctionDescription = applicationFunction.Function.FunctionDescription,
					LocalizedFunctionDescription = applicationFunction.Function.LocalizedFunctionDescription,
					FunctionId = applicationFunction.Function.Id.GetValueOrDefault(),
					IsDisabled = !applicationFunction.IsLicensed,
				};
				var childFunctions =
					createAllFunctionsHierarchy(applicationFunction.ChildFunctions);
				childFunctions.ForEach(function.ChildFunctions.Add);
				result.Add(function);
			}
			return result;
		}
	}

	public class ApplicationFunctionViewModel
	{
		public ApplicationFunctionViewModel()
		{
			ChildFunctions = new Collection<ApplicationFunctionViewModel>();
		}

		public string FunctionDescription { get; set; }
		public string LocalizedFunctionDescription { get; set; }
		public string FunctionCode { get; set; }
		public Guid FunctionId { get; set; }
		public ICollection<ApplicationFunctionViewModel> ChildFunctions { get; set; }
		public bool IsDisabled { get; set; }
		public bool IsSelected => false;
	}
}