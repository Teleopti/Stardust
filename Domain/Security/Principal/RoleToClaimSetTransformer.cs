using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public interface IRoleToClaimSetTransformer
    {
        ClaimSet Transform(IApplicationRole role, IUnitOfWork unitOfWork);
    }

	public interface IApplicationFunctionClaimStrategy
	{
		bool UseMeForClaim(Claim claim);
		Claim MakeClaim(IApplicationFunction applicationFunction);
		IApplicationFunction GetApplicationFunction(Claim claim);
	}

	public class ClaimWithId : IApplicationFunctionClaimStrategy
	{
		private readonly IApplicationFunctionRepository _repository;

		public ClaimWithId(IApplicationFunctionRepository repository) {
			_repository = repository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool UseMeForClaim(Claim claim) { return claim.Resource is AuthorizeApplicationFunction; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public Claim MakeClaim(IApplicationFunction applicationFunction)
		{
			return new Claim(
				string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace, "/", applicationFunction.FunctionPath),
				new AuthorizeApplicationFunction
					{
						ApplicationFunctionId = applicationFunction.Id.Value
					},
				Rights.PossessProperty
				);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IApplicationFunction GetApplicationFunction(Claim claim)
		{
			var resource = (AuthorizeApplicationFunction) claim.Resource;
			return _repository.Get(resource.ApplicationFunctionId);
		}
	}

	public class ClaimWithEntity : IApplicationFunctionClaimStrategy
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool UseMeForClaim(Claim claim) { return claim.Resource is IApplicationFunction; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public Claim MakeClaim(IApplicationFunction applicationFunction)
		{
			return new Claim(
				string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace, "/", applicationFunction.FunctionPath),
				applicationFunction,
				Rights.PossessProperty
				);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IApplicationFunction GetApplicationFunction(Claim claim)
		{
			return (IApplicationFunction)claim.Resource;
		}
	}

	public class RoleToClaimSetTransformer : IRoleToClaimSetTransformer
    {
        private readonly IFunctionsForRoleProvider _functionsForRoleProvider;
		private readonly IApplicationFunctionClaimStrategy _applicationFunctionClaimStrategy;

		public RoleToClaimSetTransformer(IFunctionsForRoleProvider functionsForRoleProvider, IApplicationFunctionClaimStrategy applicationFunctionClaimStrategy)
        {
        	_functionsForRoleProvider = functionsForRoleProvider;
        	_applicationFunctionClaimStrategy = applicationFunctionClaimStrategy;
        }

		public ClaimSet Transform(IApplicationRole role, IUnitOfWork unitOfWork)
        {
            var claims = new List<Claim>();

            var availableFunctions = _functionsForRoleProvider.AvailableFunctions(role, unitOfWork);

			claims.AddRange(availableFunctions.Select(_applicationFunctionClaimStrategy.MakeClaim));

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
        IEnumerable<IApplicationFunction> AvailableFunctions(IApplicationRole applicationRole, IUnitOfWork unitOfWork);
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

        public IEnumerable<IApplicationFunction> AvailableFunctions(IApplicationRole applicationRole, IUnitOfWork unitOfWork)
        {
            var licensedFunctions = _licensedFunctionsProvider.LicensedFunctions();
            var availableFunctions = new List<IApplicationFunction>();

            if (_roleIsPermittedToAllFunctionsSpecification.IsSatisfiedBy(applicationRole))
            {
                availableFunctions.AddRange(licensedFunctions);
                availableFunctions.AddRange(_externalFunctionsProvider.ExternalFunctions(unitOfWork));
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