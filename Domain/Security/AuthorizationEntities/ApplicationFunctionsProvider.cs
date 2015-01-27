using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
	public class ApplicationFunctionsProvider : IApplicationFunctionsProvider
	{
		private readonly IApplicationFunctionRepository _applicationFunctionRepository;
		private readonly ILicensedFunctionsProvider _licensedFunctionsProvider;
		private readonly ICurrentDataSource _currentDataSource;

		public ApplicationFunctionsProvider(IApplicationFunctionRepository applicationFunctionRepository, ILicensedFunctionsProvider licensedFunctionsProvider, ICurrentDataSource currentDataSource)
		{
			_applicationFunctionRepository = applicationFunctionRepository;
			_licensedFunctionsProvider = licensedFunctionsProvider;
			_currentDataSource = currentDataSource;
		}

		public AllFunctions AllFunctions()
		{
			var licensedFunctions = _licensedFunctionsProvider.LicensedFunctions(_currentDataSource.CurrentName());
			var isLicensed =
				new Func<IApplicationFunction, bool>(
					f => f.ForeignSource != DefinedForeignSourceNames.SourceRaptor || licensedFunctions.Contains(f) ||
					     f.FunctionPath == DefinedRaptorApplicationFunctionPaths.All);
			var parentFunctions = _applicationFunctionRepository.GetAllApplicationFunctionSortedByCode().Where(f => f.Parent == null).ToArray();
			return new AllFunctions(parentFunctions.Select(f => new SystemFunction(f, isLicensed)).ToArray());
		}
	}
}