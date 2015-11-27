using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Permissions.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebPermissions, DefinedRaptorApplicationFunctionPaths.OpenPermissionPage)]
	public class ApplicationFunctionsController : ApiController
	{
		private readonly IApplicationFunctionsToggleFilter _applicationFunctionsToggleFilter;
		
		public ApplicationFunctionsController(IApplicationFunctionsToggleFilter applicationFunctionsToggleFilter)
		{
			_applicationFunctionsToggleFilter = applicationFunctionsToggleFilter;
		}

		[UnitOfWork, HttpGet, Route("api/Permissions/ApplicationFunctions")]
		public virtual ICollection<ApplicationFunctionViewModel> GetAllFunctions()
		{
			var functions = _applicationFunctionsToggleFilter.FilteredFunctions();
			return createAllFunctionsHierarchy(functions.Functions);
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
	}
}