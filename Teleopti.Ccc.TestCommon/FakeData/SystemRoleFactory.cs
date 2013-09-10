using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class SystemRoleFactory
    {

        /// Creates system roles list.
        /// </summary>
        /// <returns></returns>
        public static IList<SystemRole> CreateRolesForSystemRoleApplicationRoleMapperTest()
        {
            List<SystemRole> roles = new List<SystemRole>();

            roles.Add(CreateRole("AdminActiveDirectoryRole", "AdminActiveDirectoryRole"));
            roles.Add(CreateRole("TFSUserActiveDirectoryROle", "TFSUserActiveDirectoryROle"));
            return roles;
        }

        /// <summary>
        /// Adds another system role to system role list that is not mapped in the SystemRole - ApplicationRole mapper.
        /// </summary>
        /// <param name="inputList">The input list.</param>
        /// <returns></returns>
        public static IList<SystemRole> AddNotMappedSystemRoleToSystemRoleList(IList<SystemRole> inputList)
        {
            inputList.Add(CreateRole("NotMappedActiveDirectoryRole", "NotMappedActiveDirectoryRole"));
            return inputList;
        }


        /// <summary>
        /// Creates a system role.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public static SystemRole CreateRole(string name, string description)
        {
            SystemRole role = new SystemRole();
            role.DescriptionText = description;
            role.Name = name;
            return role;
        }
    }
}
