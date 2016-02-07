using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Matrix
{
	public interface IApplicationFunctionResolver
	{
		HashSet<MatrixPermissionHolder> ResolveApplicationFunction(HashSet<MatrixPermissionHolder> list, IApplicationRole applicationRole, IUnitOfWorkFactory unitOfWorkFactory);
	}

	public class ApplicationFunctionResolver : IApplicationFunctionResolver
	{
		private readonly ApplicationFunctionsForRole _applicationFunctionsForRole;
		private readonly ISpecification<IApplicationFunction> _matrixFunctionSpecification =
			 new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix);
		private readonly IList<IApplicationFunction> _matrixFunctionsForThisInstance = new List<IApplicationFunction>();

		public ApplicationFunctionResolver(ApplicationFunctionsForRole applicationFunctionsForRole)
		{
			_applicationFunctionsForRole = applicationFunctionsForRole;
		}

		public HashSet<MatrixPermissionHolder> ResolveApplicationFunction(HashSet<MatrixPermissionHolder> list, IApplicationRole applicationRole, IUnitOfWorkFactory unitOfWorkFactory)
		{
			var result = new HashSet<MatrixPermissionHolder>();

			var availableFunctions = _applicationFunctionsForRole.AvailableFunctions(applicationRole, unitOfWorkFactory.Name);
			var matrixFunctionsForCurrentRole = availableFunctions.FilterBySpecification(_matrixFunctionSpecification).ToList();
			foreach (var applicationFunction in matrixFunctionsForCurrentRole)
			{
				_matrixFunctionsForThisInstance.Add(applicationFunction);
			}

			foreach (MatrixPermissionHolder holder in list)
			{
				foreach (IApplicationFunction function in _matrixFunctionsForThisInstance)
				{
					result.Add(new MatrixPermissionHolder(holder.Person, holder.Team, holder.IsMy, function));
				}
			}

			return result;
		}
	}
}