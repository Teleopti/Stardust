using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Security.Principal;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
{
    public class TeleoptiPrincipalAuthorizationPolicy : IAuthorizationPolicy
    {
        private readonly string _id;
        private readonly ClaimSet _tokenClaims;
        private readonly PersonContainer _personContainer;
        private readonly ClaimSet _issuer;

        public TeleoptiPrincipalAuthorizationPolicy(ClaimSet tokenClaims, PersonContainer personContainer) : this()
        {
            if (tokenClaims == null)
            {
                throw new ArgumentNullException(nameof(tokenClaims));
            }
            _issuer = tokenClaims.Issuer;
            _tokenClaims = tokenClaims;
            _personContainer = personContainer;
        }

        public TeleoptiPrincipalAuthorizationPolicy()
        {
            _id = Guid.NewGuid().ToString();
        }

        public ClaimSet Issuer => _issuer;

	    public string Id => _id;

	    public PersonContainer PersonContainer => _personContainer;

	    public IIdentity Identity { get; set; }

        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            // Add the token claim set to the evaluation context.
            AddClaimSet(evaluationContext);

            SetIdentityIfNeeded(evaluationContext);

            SetPrincipalIfNeeded(evaluationContext);

            // Return true if the policy evaluation is finished.)
            return true;
        }

        private void SetPrincipalIfNeeded(EvaluationContext evaluationContext)
        {
            object obj;
            if (!evaluationContext.Properties.TryGetValue("Principal",out obj))
            {
                if (!evaluationContext.Properties.TryGetValue("Identities", out obj))
                    throw new FaultException("No Identity found");

                IList<IIdentity> identities = obj as IList<IIdentity>;
                if (identities == null || identities.Count <= 0)
                    throw new FaultException("No Identity found");

                evaluationContext.Properties.Add("Principal",new TeleoptiPrincipal(identities[0], _personContainer?.Person));
            }
        }

        private void SetIdentityIfNeeded(EvaluationContext evaluationContext)
        {
            object obj;
            if (!evaluationContext.Properties.TryGetValue("Identities", out obj))
            {
                if (Identity==null)
                {
                    Identity = new GenericIdentity("", "Anonymous");
                }

                evaluationContext.Properties.Add("Identities",new List<IIdentity>{Identity});
            }
        }

        private void AddClaimSet(EvaluationContext evaluationContext)
        {
            evaluationContext.AddClaimSet(this, _tokenClaims);
        }
    }
}