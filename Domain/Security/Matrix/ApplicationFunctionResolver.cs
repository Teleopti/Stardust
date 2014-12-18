using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
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
		private readonly IFunctionsForRoleProvider _functionsForRoleProvider;

		private readonly ISpecification<IApplicationFunction> _matrixFunctionSpecification =
			 new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix);

		private IList<IApplicationFunction> _matrixFunctions;


		public ApplicationFunctionResolver(IFunctionsForRoleProvider functionsForRoleProvider)
		{
			_functionsForRoleProvider = functionsForRoleProvider;
		}

		public HashSet<MatrixPermissionHolder> ResolveApplicationFunction(HashSet<MatrixPermissionHolder> list, IApplicationRole applicationRole, IUnitOfWorkFactory unitOfWorkFactory)
		{
			var result = new HashSet<MatrixPermissionHolder>();
			if (_matrixFunctions == null)
			{
				_matrixFunctions = new List<IApplicationFunction>();
				var availableFunctions = _functionsForRoleProvider.AvailableFunctions(applicationRole, unitOfWorkFactory);
				var tmpFunctions = availableFunctions.FilterBySpecification(_matrixFunctionSpecification).ToList();
				Guid tmp;
				foreach (var applicationFunction in tmpFunctions.Where(applicationFunction => Guid.TryParse(applicationFunction.ForeignId, out tmp)))
				{
					_matrixFunctions.Add(applicationFunction);
				}
			}

			foreach (MatrixPermissionHolder holder in list)
			{
				foreach (IApplicationFunction function in _matrixFunctions)
				{
					var newItem = new MatrixPermissionHolder(holder.Person, holder.Team, holder.IsMy, function);
					result.Add(newItem);
				}
			}

			return result;
		}
	}
}