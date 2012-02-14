using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Matrix
{
    public interface IApplicationFunctionResolver
    {
        HashSet<MatrixPermissionHolder> ResolveApplicationFunction(HashSet<MatrixPermissionHolder> list, IApplicationRole applicationRole, IUnitOfWork unitOfWork);
    }

    public class ApplicationFunctionResolver : IApplicationFunctionResolver
    {
        private readonly IFunctionsForRoleProvider _functionsForRoleProvider;

        private readonly ISpecification<IApplicationFunction> _matrixFunctionSpecification =
            new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix);

        public ApplicationFunctionResolver(IFunctionsForRoleProvider functionsForRoleProvider)
        {
            _functionsForRoleProvider = functionsForRoleProvider;
        }

        public HashSet<MatrixPermissionHolder> ResolveApplicationFunction(HashSet<MatrixPermissionHolder> list, IApplicationRole applicationRole, IUnitOfWork unitOfWork)
        {
            var result = new HashSet<MatrixPermissionHolder>();
            var availableFunctions = _functionsForRoleProvider.AvailableFunctions(applicationRole, unitOfWork);
            var matrixFunctions = availableFunctions.FilterBySpecification(_matrixFunctionSpecification);

            foreach (MatrixPermissionHolder holder in list)
            {
                foreach (IApplicationFunction function in matrixFunctions)
                {
                    var newItem = new MatrixPermissionHolder(holder.Person, holder.Team, holder.IsMy, function);
                    result.Add(newItem);
                }
            }

            return result;
        }
    }
}