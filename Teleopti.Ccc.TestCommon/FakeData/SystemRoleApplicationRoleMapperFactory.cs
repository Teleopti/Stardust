using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class SystemRoleApplicationRoleMapperFactory
    {

        /// <summary>
        /// Creates the system role - application role mapper for test.
        /// </summary>
        /// <returns></returns>
        public static IList<SystemRoleApplicationRoleMapper> CreateSystemRoleApplicationRoleMapperListForTest(IList<SystemRole> systemRoles, IList<IApplicationRole> applicationRoles)
        {
            string activeDirectorySystemName = "ActiveDirectory";

            IList<SystemRoleApplicationRoleMapper> retList = new List<SystemRoleApplicationRoleMapper>();

            for (int index = 0; index< applicationRoles.Count;index++)
            {
                SystemRoleApplicationRoleMapper roleMapper = new SystemRoleApplicationRoleMapper();
                roleMapper.SystemName = activeDirectorySystemName;
                roleMapper.SystemRoleLongName = systemRoles[index].Name;
                roleMapper.ApplicationRole = applicationRoles[index];
                retList.Add(roleMapper);
            }

            retList = AddMapperThatContainsNonExistentSystemRole(retList);

            return retList;
        }

        /// <summary>
        /// Adds a SystemRoleApplicationRoleMapper to the input list that contains non existent system role.
        /// </summary>
        /// <param name="inputList">The input list.</param>
        /// <returns></returns>
        private static IList<SystemRoleApplicationRoleMapper> AddMapperThatContainsNonExistentSystemRole(IList<SystemRoleApplicationRoleMapper> inputList)
        {
            SystemRoleApplicationRoleMapper roleMapper = new SystemRoleApplicationRoleMapper();
            roleMapper.SystemName = inputList[0].SystemName;
            roleMapper.ApplicationRole = inputList[0].ApplicationRole;
            roleMapper.SystemRoleLongName = "NonExistentActiveDirectoryRole";
            inputList.Add(roleMapper);
            return inputList;
        }
    }
}
