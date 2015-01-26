using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Permissions.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage)]
	public class ApplicationFunctionsController : ApiController
	{
		private readonly IApplicationFunctionRepository _applicationFunctionRepository;
		private readonly ILicensedFunctionsProvider _licensedFunctionsProvider;
		private readonly ICurrentDataSource _currentDataSource;

		public ApplicationFunctionsController(IApplicationFunctionRepository applicationFunctionRepository, ILicensedFunctionsProvider licensedFunctionsProvider, ICurrentDataSource currentDataSource)
		{
			_applicationFunctionRepository = applicationFunctionRepository;
			_licensedFunctionsProvider = licensedFunctionsProvider;
			_currentDataSource = currentDataSource;
		}

		[UnitOfWork, HttpGet, Route("api/Permissions/ApplicationFunctions")]
		public virtual object GetAllFunctions()
		{
			var allFunctions = _applicationFunctionRepository.GetAllApplicationFunctionSortedByCode();
			var licensedFunctions = _licensedFunctionsProvider.LicensedFunctions(_currentDataSource.CurrentName()).ToArray();
			var isLicensed = new Func<IApplicationFunction, bool>(licensedFunctions.Contains);
			var parentFunctions = allFunctions.Where(f => f.Parent == null).ToArray();
			
			return new {Functions = createAllFunctionsHierarchy(parentFunctions,isLicensed)};
		}

		private ICollection<ApplicationFunctionViewModel> createAllFunctionsHierarchy(IApplicationFunction[] parentFunctions, Func<IApplicationFunction,bool> checkLicensed)
		{
			var result = new Collection<ApplicationFunctionViewModel>();
			foreach (var applicationFunction in parentFunctions)
			{
				var function = new ApplicationFunctionViewModel
				{
					FunctionCode = applicationFunction.FunctionCode,
					FunctionDescription = applicationFunction.FunctionDescription,
					LocalizedFunctionDescription = applicationFunction.LocalizedFunctionDescription,
					FunctionId = applicationFunction.Id.GetValueOrDefault(),
					IsDisabled = !checkLicensed(applicationFunction)
				};
				var childFunctions =
					createAllFunctionsHierarchy(applicationFunction.ChildCollection.OfType<IApplicationFunction>().ToArray(), checkLicensed);
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