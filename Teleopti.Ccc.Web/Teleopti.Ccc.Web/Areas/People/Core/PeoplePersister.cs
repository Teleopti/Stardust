using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core
{
	public class PeoplePersister: IPeoplePersister
	{
		private IApplicationRoleRepository _roleRepository;
		private readonly ITenantDataManager _tenantDataManager;
		private readonly IPersonRepository _personRepository;
		private const int MaxNameLength = 25;
		private const int MaxApplicationUserIdLength = 50;

		public PeoplePersister(IApplicationRoleRepository roleRepository, ITenantDataManager tenantDataManager, IPersonRepository personRepository)
		{
			_roleRepository = roleRepository;
			_tenantDataManager = tenantDataManager;
			_personRepository = personRepository;
		}

		public dynamic Persist(IEnumerable<RawUser> users)
		{
			var availableRoles = new Dictionary<string, IApplicationRole>();
			_roleRepository.LoadAllApplicationRolesSortedByName().ForEach(r => availableRoles.Add(r.DescriptionText.ToUpper(), r));
			//validate
			// DB persist
			var invalidUsers = new List<RawUser>();
			foreach (var user in users)
			{
				var isUserValid = true;
				var errorMsgBuilder = new StringBuilder();
				if (string.IsNullOrEmpty(user.Password))
				{
					errorMsgBuilder.Append(Resources.EmptyPasswordErrorMsgSemicolon + " ");
					isUserValid = false;
				}

				if (string.IsNullOrEmpty(user.ApplicationUserId) && string.IsNullOrEmpty(user.WindowsUser))
				{
					errorMsgBuilder.Append(Resources.NoLogonAccountErrorMsgSemicolon +" ");
					isUserValid = false;
				}

				if (string.IsNullOrEmpty(user.FirstName) && string.IsNullOrEmpty(user.LastName))
				{
					errorMsgBuilder.Append(Resources.BothFirstnameAndLastnameAreEmptyErrorMsgSemicolon + " ");
					isUserValid = false;
				}

				if (user.FirstName.Length > MaxNameLength)
				{
					errorMsgBuilder.Append(Resources.TooLongFirstnameErrorMsgSemicolon + " ");
					isUserValid = false;
				}

				if (user.LastName.Length > MaxNameLength)
				{
					errorMsgBuilder.Append(Resources.TooLongLastnameErrorMsgSemicolon + " ");
					isUserValid = false;
				}

				if (user.ApplicationUserId.Length > MaxApplicationUserIdLength)
				{
					errorMsgBuilder.Append(Resources.TooLongApplicationUserIdErrorMsgSemicolon + " ");
					isUserValid = false;
				}

				if (!string.IsNullOrEmpty(user.Role))
				{
					var roles = user.Role.Split(',').Select(x => x.Trim()).ToList();
					var invalidRolesBuilder = new StringBuilder();
					var hasInvalidRole = false;

					foreach (var role in roles.Where(role => !availableRoles.ContainsKey(role.ToUpper())))
					{
						hasInvalidRole = true;
						var roleNotExist = role + ", ";
						invalidRolesBuilder.Append(roleNotExist);
					}

					if (hasInvalidRole)
					{
						var errorRolesMsg = invalidRolesBuilder.ToString();
						errorRolesMsg = errorRolesMsg.Substring(0, errorRolesMsg.Length - 2);
						errorRolesMsg = string.Format(Resources.RoleXNotExistErrorMsgSemicolon + " ", errorRolesMsg);
						errorMsgBuilder.Append(errorRolesMsg);
						isUserValid = false;
					}
					else if (isUserValid)
					{
						var person = new Person { Name = new Name(user.FirstName, user.LastName) };
						roles.ForEach(r => person.PermissionInformation.AddApplicationRole(availableRoles[r.ToUpper()]));
						_personRepository.Add(person);

						var tenantUserData = new TenantAuthenticationData()
						{
							ApplicationLogonName = user.ApplicationUserId,
							Identity = user.WindowsUser,
							Password = user.Password,
							PersonId = person.Id.GetValueOrDefault()
						};
						var result = _tenantDataManager.SaveTenantData(tenantUserData);

						if (!result.Success)
						{
							errorMsgBuilder.Append(result.FailReason + "; ");
							isUserValid = false;
						}
					}
				}

				if (isUserValid) continue;

				var errorMsg = errorMsgBuilder.ToString();
				errorMsg = errorMsg.Substring(0, errorMsg.Length - 2);
				user.ErrorMessage = errorMsg;
				invalidUsers.Add(user);
			}
			
			return new
			{
				InvalidUsers = invalidUsers
			};
		}
	}
}