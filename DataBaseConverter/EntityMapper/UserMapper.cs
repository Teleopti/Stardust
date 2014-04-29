using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Tool for converting 6x user
    /// </summary>
    public class UserMapper : Mapper<IPerson, global::Domain.User>
    {
        private readonly IApplicationRole _agentApplicationRole;
        private readonly IApplicationRole _administratorApplicationRole;
        private readonly IList<IPerson> _existingPersons;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserMapper"/> class.
        /// </summary>
        /// <param name="mappedObjectPair">The mapped object pair.</param>
        /// <param name="timeZone">The time zone.</param>
        /// <param name="agentApplicationRole">The default role for an agent.</param>
        /// <param name="administratorApplicationRole">The default role for an administrator.</param>
        /// <param name="existingPersons">The existing persons.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/26/2007
        /// </remarks>
        public UserMapper(MappedObjectPair mappedObjectPair,
                          TimeZoneInfo timeZone,
                          IApplicationRole agentApplicationRole,
                          IApplicationRole administratorApplicationRole,
                          IList<IPerson> existingPersons)
            : base(mappedObjectPair, timeZone)
        {
            _existingPersons = existingPersons;
            _agentApplicationRole = agentApplicationRole;
            _administratorApplicationRole = administratorApplicationRole;
        }

        /// <summary>
        /// Maps the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 10/23/2007
        /// </remarks>
        public override IPerson Map(global::Domain.User oldEntity)
        {
            IPerson newPerson;

            if (oldEntity.Id == -1 || string.IsNullOrEmpty(oldEntity.LoginName))
                return null;

            newPerson =
                    _existingPersons.FirstOrDefault(
						p => ((p.AuthenticationInfo != null && p.AuthenticationInfo.Identity.ToUpperInvariant() == oldEntity.LoginDomain.ToUpperInvariant() + @"\" + oldEntity.LoginName.ToUpperInvariant())) 
							||
                             (p.ApplicationAuthenticationInfo != null && p.ApplicationAuthenticationInfo.ApplicationLogOnName.ToUpperInvariant() ==
                              oldEntity.LoginName.ToUpperInvariant() &&
                              string.IsNullOrEmpty(oldEntity.LoginDomain)));
            if (newPerson == null)
            {
                newPerson = MappedObjectPair.Agent.GetPaired(oldEntity.CccAgent);
            }
            if (newPerson == null)
            {
                newPerson = new Person();
                newPerson.Name = new Name(ConversionHelper.MapString(oldEntity.FirstName, 25),
                                          ConversionHelper.MapString(oldEntity.LastName, 25));
                newPerson.Email = ConversionHelper.MapString(oldEntity.Email, 50);
            }

            ((PermissionInformation)newPerson.PermissionInformation).SetDefaultTimeZone(TimeZone);

            ApplicationAuthenticationInfo appAuthInfo = new ApplicationAuthenticationInfo();
            appAuthInfo.ApplicationLogOnName = oldEntity.LoginName;
            //TODO: convert old password
            appAuthInfo.Password = oldEntity.LoginName;
            newPerson.ApplicationAuthenticationInfo = appAuthInfo;
            if (!String.IsNullOrEmpty(oldEntity.LoginDomain))
            {
                AuthenticationInfo winAuthInfo = new AuthenticationInfo();
				winAuthInfo.Identity = oldEntity.LoginDomain + @"\" + oldEntity.LoginName;
                newPerson.AuthenticationInfo = winAuthInfo;
            }

            if (oldEntity.IsAdmin)
                newPerson.PermissionInformation.AddApplicationRole(_administratorApplicationRole);
            else
                newPerson.PermissionInformation.AddApplicationRole(_agentApplicationRole);
            
            return newPerson;
        }
    }
}
