using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;


namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
{
    public class CustomAuthorizationManager : ServiceAuthorizationManager
    {
        protected override System.Collections.ObjectModel.ReadOnlyCollection<IAuthorizationPolicy> GetAuthorizationPolicies(OperationContext operationContext)
        {
            var policies = new List<IAuthorizationPolicy>(base.GetAuthorizationPolicies(operationContext));

            var headerPosition = operationContext.IncomingMessageHeaders.FindHeader(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderName,
                                                                                    TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace);
            if (headerPosition != -1)
            {
                var headerReader = operationContext.IncomingMessageHeaders.GetReaderAtHeader(headerPosition);
                var headerData = TeleoptiAuthenticationHeader.ReadHeader(headerReader);
                if (headerData != null)
                {
                    Guid businessUnitId = headerData.BusinessUnit;
                    IBusinessUnit businessUnit = null;
                    IDataSource datasource = null;
                    string userName = headerData.UserName;
                    var teleoptiPolicies = new List<IAuthorizationPolicy>();

                    if (!headerData.UseWindowsIdentity && !string.IsNullOrEmpty(headerData.UserName))
                    {
                        var customToken = new CustomUserNameSecurityToken(headerData.UserName, headerData.Password,
                                                                          headerData.DataSource,businessUnitId);
                        var customUserNameSecurityTokenAuthenticator = new TeleoptiSecurityTokenAuthenticator();
                        teleoptiPolicies.AddRange(customUserNameSecurityTokenAuthenticator.ValidateToken(customToken));
                        businessUnit = customUserNameSecurityTokenAuthenticator.BusinessUnit;
                        datasource = customUserNameSecurityTokenAuthenticator.DataSource;
                    }

                    WindowsIdentity windowsIdentity = null;
                    if (headerData.UseWindowsIdentity)
                    {
                        windowsIdentity = GetWindowsIdentity(operationContext);

                    		if (windowsIdentity != null)
                            {
                                using (var token = new CustomWindowsSecurityToken(windowsIdentity, headerData.DataSource, businessUnitId))
                                {
                                    var windowsSecurityTokenAuthenticator = new WindowsTokenAuthenticator();
                                    teleoptiPolicies.AddRange(windowsSecurityTokenAuthenticator.ValidateToken(token));
                                    businessUnit = windowsSecurityTokenAuthenticator.BusinessUnit;
                                    datasource = windowsSecurityTokenAuthenticator.DataSource;
                                    userName = windowsIdentity.Name;
                                }
                            }
                    }
                    var identity = new TeleoptiIdentity(userName, datasource, businessUnit.Id, businessUnit.Name, windowsIdentity, null);
                    foreach (TeleoptiPrincipalAuthorizationPolicy authorizationPolicy in teleoptiPolicies)
                    {
                        authorizationPolicy.Identity = identity;
                    }
                    policies.InsertRange(0,teleoptiPolicies);
                }
            }
            var unconditinalPolicy = policies.FirstOrDefault(p => p.GetType().Name.Contains("Unconditional"));
            if (unconditinalPolicy!=null)
            {
                policies.Remove(unconditinalPolicy);
            }
            if (policies.Count == 1)
            {
                policies.Insert(0, new TeleoptiPrincipalAuthorizationPolicy(new DefaultClaimSet(ClaimSet.System,new Claim(ClaimTypes.Anonymous,string.Empty,Rights.PossessProperty)), null));
            }
            return policies.AsReadOnly();
        }

        private static WindowsIdentity GetWindowsIdentity(OperationContext operationContext)
        {
            var transportToken = operationContext.IncomingMessageProperties.Security.TransportToken.SecurityToken;
            var windowsToken = transportToken as WindowsSecurityToken;
            if (windowsToken == null) return null;

            WindowsIdentity windowsIdentity = windowsToken.WindowsIdentity;
            
            return windowsIdentity;
        }
    }
}