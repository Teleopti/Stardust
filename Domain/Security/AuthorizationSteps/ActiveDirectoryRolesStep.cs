using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.ActiveDirectory;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationSteps
{
    /// <summary>
    /// Loads the active directory groups as active directory roles
    /// </summary>
    public class ActiveDirectoryRolesStep : AuthorizationStep
    {
        private readonly string _userName;
        private readonly IActiveDirectoryUserRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDirectoryRolesStep"/> class.
        /// </summary>
        /// <param name="panelName">Name of the panel.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="description">The description.</param>
        public ActiveDirectoryRolesStep(
            string panelName,
            string userName,
            IActiveDirectoryUserRepository repository,
            string description)
            : base(panelName, description)
        {
            _userName = userName;
            _repository = repository;
        }

        /// <summary>
        /// Refreshes the own list. Template abstact method
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 2007-10-31
        /// </remarks>
        protected override IList<IAuthorizationEntity> RefreshOwnList()
        {
            IList<IAuthorizationEntity> resultList = new List<IAuthorizationEntity>();

            ActiveDirectoryUser user = _repository.FindUser(ActiveDirectoryUserMapper.SAMACCOUNTNAME, _userName);
            if (user == null)
            {
                WarningMessage = "xx Person with the logon name av '" + _userName  + "' is not found in Active Directory.";
                return resultList;
            }

            foreach (ActiveDirectoryGroup group in user.TokenGroups)
            {
                SystemRole role = new SystemRole();
                role.Name = group.CommonName;
                role.DescriptionText = group.DistinguishedName;
                resultList.Add(role);
            }
            return resultList;
        }
    }
}
