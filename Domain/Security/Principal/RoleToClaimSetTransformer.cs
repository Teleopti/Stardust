using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public interface IRoleToClaimSetTransformer
    {
        ClaimSet Transform(IApplicationRole role, IUnitOfWorkFactory unitOfWorkFactory);
    }

	public class RoleToClaimSetTransformer : IRoleToClaimSetTransformer
    {
        private readonly IFunctionsForRoleProvider _functionsForRoleProvider;

		public RoleToClaimSetTransformer(IFunctionsForRoleProvider functionsForRoleProvider)
        {
        	_functionsForRoleProvider = functionsForRoleProvider;
        }

		public ClaimSet Transform(IApplicationRole role, IUnitOfWorkFactory unitOfWorkFactory)
        {
            var claims = new List<Claim>();

            var availableFunctions = _functionsForRoleProvider.AvailableFunctions(role, unitOfWorkFactory);

			claims.AddRange(
				availableFunctions
					.Select(applicationFunction =>
					        new Claim(
					        	string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace, "/", applicationFunction.FunctionPath),
								applicationFunction,
					        	Rights.PossessProperty
					        	))
				);

			if (role.AvailableData != null)
			{
				claims.Add(new Claim(string.Concat(
					TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace, "/AvailableData"),
				                     new AuthorizeExternalAvailableData(role.AvailableData), Rights.PossessProperty));

				IAuthorizeAvailableData authorizeAvailableData = null;
				switch (role.AvailableData.AvailableDataRange)
				{
					case AvailableDataRangeOption.Everyone:
						authorizeAvailableData = new AuthorizeEveryone();
						break;
					case AvailableDataRangeOption.MyBusinessUnit:
						authorizeAvailableData = new AuthorizeMyBusinessUnit();
						break;
					case AvailableDataRangeOption.MyOwn:
						authorizeAvailableData = new AuthorizeMyOwn();
						break;
					case AvailableDataRangeOption.MySite:
						authorizeAvailableData = new AuthorizeMySite();
						break;
					case AvailableDataRangeOption.MyTeam:
						authorizeAvailableData = new AuthorizeMyTeam();
						break;
				}
				if (authorizeAvailableData != null)
				{
					claims.Add(
						new Claim(
							string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
							              "/AvailableData"), authorizeAvailableData, Rights.PossessProperty));
				}
			}

        	return new DefaultClaimSet(ClaimSet.System,claims);
        }
    }

    internal class RoleIsPermittedToAllFunctionsSpecification : Specification<IApplicationRole>
    {
        public override bool IsSatisfiedBy(IApplicationRole obj)
        {
            return
                obj.ApplicationFunctionCollection.Any(a => a.FunctionPath == DefinedRaptorApplicationFunctionPaths.All);
        }
    }

    public interface IFunctionsForRoleProvider
    {
        IEnumerable<IApplicationFunction> AvailableFunctions(IApplicationRole applicationRole, IUnitOfWorkFactory unitOfWorkFactory);
    }

    public class FunctionsForRoleProvider : IFunctionsForRoleProvider
    {
        private readonly ILicensedFunctionsProvider _licensedFunctionsProvider;
        private readonly IExternalFunctionsProvider _externalFunctionsProvider;
        private readonly ISpecification<IApplicationRole> _roleIsPermittedToAllFunctionsSpecification =
            new RoleIsPermittedToAllFunctionsSpecification();

        public FunctionsForRoleProvider(ILicensedFunctionsProvider licensedFunctionsProvider, IExternalFunctionsProvider externalFunctionsProvider)
        {
            _licensedFunctionsProvider = licensedFunctionsProvider;
            _externalFunctionsProvider = externalFunctionsProvider;
        }

        public IEnumerable<IApplicationFunction> AvailableFunctions(IApplicationRole applicationRole, IUnitOfWorkFactory unitOfWorkFactory)
        {
            var licensedFunctions = _licensedFunctionsProvider.LicensedFunctions(unitOfWorkFactory.Name);
            var availableFunctions = new List<IApplicationFunction>();

            if (_roleIsPermittedToAllFunctionsSpecification.IsSatisfiedBy(applicationRole))
            {
                availableFunctions.AddRange(licensedFunctions);
                availableFunctions.AddRange(_externalFunctionsProvider.ExternalFunctions(unitOfWorkFactory.CurrentUnitOfWork()));
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