using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class ApplicationRoleFactory
    {
        public static ApplicationRole CreateRole(string name, string description)
        {
            ApplicationRole role = new ApplicationRole();
            role.DescriptionText = description;
            role.Name = name;
            return role;
        }

        public static IList<IApplicationRole> CreateApplicationRolesAndFunctionsStructure()
        {
            IList<IApplicationRole> resultList = new List<IApplicationRole>();
            IList<IApplicationFunction> functionList = ApplicationFunctionFactory.CreateApplicationFunctionList("APP", "CODE", 4);
            for (int counter = 0; counter < 5; counter++)
            {
                string shortName = "Role" + counter.ToString(CultureInfo.InvariantCulture);
                string name = "RoleDescription" + counter.ToString(CultureInfo.InvariantCulture);
                IApplicationRole role = CreateRole(shortName, name);
                for (int functionCounter = 0; functionCounter <= counter; functionCounter++)
                {
                    role.AddApplicationFunction(functionList[functionCounter]);
                }
                resultList.Add(role);
            }
            return resultList;
        }

        public static IList<IApplicationRole> CreateShippedRoles(
            out IApplicationRole administratorRole, 
            out IApplicationRole agentRole,
            out IApplicationRole unitRole,
            out IApplicationRole siteRole,
            out IApplicationRole teamRole)
        {
            IList<IApplicationRole> resultList = CreateShippedRoles();

            administratorRole = resultList[0];
            agentRole = resultList[1];
            unitRole = resultList[2];
            siteRole = resultList[3];
            teamRole = resultList[4];
            return resultList;
        }

        public static IList<IApplicationRole> CreateShippedRoles()
        {
            IList<IApplicationRole> resultList = new List<IApplicationRole>();
            resultList.Add(CreateRole(ShippedApplicationRoleNames.AdministratorRole, ShippedApplicationRoleNames.AdministratorRole));
            resultList.Add(CreateRole(ShippedApplicationRoleNames.AgentRole, ShippedApplicationRoleNames.AgentRole));
            resultList.Add(CreateRole(ShippedApplicationRoleNames.BusinessUnitAdministratorRole, ShippedApplicationRoleNames.BusinessUnitAdministratorRole));
            resultList.Add(CreateRole(ShippedApplicationRoleNames.SiteManagerRole, ShippedApplicationRoleNames.SiteManagerRole));
            resultList.Add(CreateRole(ShippedApplicationRoleNames.TeamLeaderRole, ShippedApplicationRoleNames.TeamLeaderRole));
            return resultList;
        }
    }
}
