using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Logon
{
	public class ApplicationFunctionsForRole
	{
		private readonly ILicensedFunctionsProvider _licensedFunctionsProvider;
		private readonly IApplicationFunctionRepository _applicationFunctionRepository;

		private readonly ISpecification<IApplicationRole> _roleIsPermittedToAllFunctionsSpecification =
			new roleIsPermittedToAllFunctionsSpecification();

		private class roleIsPermittedToAllFunctionsSpecification : Specification<IApplicationRole>
		{
			public override bool IsSatisfiedBy(IApplicationRole obj)
			{
				return obj.ApplicationFunctionCollection.Any(a => a.FunctionPath == DefinedRaptorApplicationFunctionPaths.All);
			}
		}

		public ApplicationFunctionsForRole(
			ILicensedFunctionsProvider licensedFunctionsProvider, 
			IApplicationFunctionRepository applicationFunctionRepository
			)
		{
			_licensedFunctionsProvider = licensedFunctionsProvider;
			_applicationFunctionRepository = applicationFunctionRepository;
		}

		public IEnumerable<IApplicationFunction> AvailableFunctions(IApplicationRole applicationRole, string tenantName)
		{
			var licensedFunctions = _licensedFunctionsProvider.LicensedFunctions(tenantName);
			var availableFunctions = new List<IApplicationFunction>();

			if (_roleIsPermittedToAllFunctionsSpecification.IsSatisfiedBy(applicationRole))
			{
				availableFunctions.AddRange(licensedFunctions);
				availableFunctions.AddRange(_applicationFunctionRepository.ExternalApplicationFunctions());
			}
			else
			{
				foreach (var applicationFunction in applicationRole.ApplicationFunctionCollection)
				{
					if (((IDeleteTag)applicationFunction).IsDeleted) continue;
					if (applicationFunction.ForeignSource == DefinedForeignSourceNames.SourceRaptor)
					{
						var function = ApplicationFunction.FindByPath(licensedFunctions,
							applicationFunction.FunctionPath);
						if (function != null)
						{
							applicationFunction.IsPreliminary = function.IsPreliminary;
							applicationFunction.FunctionCode = function.FunctionCode;
							applicationFunction.FunctionDescription = function.FunctionDescription;
							applicationFunction.SortOrder = function.SortOrder;
						}
					}
					availableFunctions.Add(applicationFunction);
				}
			}
			return availableFunctions;
		}
	}
}