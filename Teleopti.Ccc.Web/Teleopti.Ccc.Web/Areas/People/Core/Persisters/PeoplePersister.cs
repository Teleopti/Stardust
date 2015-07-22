using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Persisters
{
	public class PeoplePersister: IPeoplePersister
	{
		private readonly IApplicationRoleRepository _roleRepository;
		private readonly IPersistPersonInfo _personInfoPersister;
		private readonly IPersonInfoMapper _mapper;
		private readonly IPersonRepository _personRepository;
		private readonly ILoggedOnUser _currentLoggedOnUser;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private const int MaxNameLength = 25;
		private const int MaxApplicationUserIdLength = 50;

		public PeoplePersister(IApplicationRoleRepository roleRepository,IPersistPersonInfo personInfoPersister, IPersonInfoMapper mapper,IPersonRepository personRepository, ILoggedOnUser currentLoggedOnUser,ITenantUnitOfWork tenantUnitOfWork)
		{
			_roleRepository = roleRepository;
			_personInfoPersister = personInfoPersister;
			_mapper = mapper;
			_personRepository = personRepository;
			_currentLoggedOnUser = currentLoggedOnUser;
			_tenantUnitOfWork = tenantUnitOfWork;
		}

		public dynamic Persist(IEnumerable<RawUser> users)
		{
			var availableRoles = new Dictionary<string, IApplicationRole>();
			var allRoles = _roleRepository.LoadAllApplicationRolesSortedByName();
			allRoles.ForEach(r =>
			{
				if (!availableRoles.ContainsKey(r.DescriptionText.ToUpper()))
				{
					availableRoles.Add(r.DescriptionText.ToUpper(), r);
				}
			});
			//validate
			// DB persist
			var invalidUsers = new List<RawUser>();
			foreach (var user in users)
			{
				var isUserValid = true;
				var errorMsgBuilder = new StringBuilder();

				var person = new Person { Name = new Name(user.Firstname, user.Lastname) };
				person.PermissionInformation.SetDefaultTimeZone(
							_currentLoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());

				if (string.IsNullOrEmpty(user.ApplicationUserId) && string.IsNullOrEmpty(user.WindowsUser))
				{
					errorMsgBuilder.Append(Resources.NoLogonAccountErrorMsgSemicolon + " ");
					isUserValid = false;
				}
				else if (string.IsNullOrEmpty(user.ApplicationUserId))
				{
					errorMsgBuilder.Append(Resources.NoApplicationLogonAccountErrorMsgSemicolon + " ");
					isUserValid = false;
				}

				if (string.IsNullOrEmpty(user.Password))
				{
					errorMsgBuilder.Append(Resources.EmptyPasswordErrorMsgSemicolon + " ");
					isUserValid = false;
				}



				if (string.IsNullOrEmpty(user.Firstname) && string.IsNullOrEmpty(user.Lastname))
				{
					errorMsgBuilder.Append(Resources.BothFirstnameAndLastnameAreEmptyErrorMsgSemicolon + " ");
					isUserValid = false;
				}

				if (user.Firstname.Length > MaxNameLength)
				{
					errorMsgBuilder.Append(Resources.TooLongFirstnameErrorMsgSemicolon + " ");
					isUserValid = false;
				}

				if (user.Lastname.Length > MaxNameLength)
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
						roles.ForEach(r => person.PermissionInformation.AddApplicationRole(availableRoles[r.ToUpper()]));
					}

				}
				if (isUserValid)
				{
					_personRepository.Add(person);
					var tenantUserData = new PersonInfoModel()
					{
						ApplicationLogonName = user.ApplicationUserId,
						Identity = user.WindowsUser,
						Password = user.Password,
						PersonId = person.Id.GetValueOrDefault()
					};

					try
					{
						using (_tenantUnitOfWork.Start())
						{
							_personInfoPersister.Persist(_mapper.Map(tenantUserData));
						}
					}
					catch (PasswordStrengthException)
					{
						isUserValid = false;
						errorMsgBuilder.Append(Resources.PasswordPolicyErrorMsgSemicolon + " ");
						_personRepository.Remove(person);
					}
					catch (DuplicateIdentityException)
					{
						isUserValid = false;
						errorMsgBuilder.Append(Resources.DuplicatedWindowsLogonErrorMsgSemicolon + " ");
						_personRepository.Remove(person);
					}
					catch (DuplicateApplicationLogonNameException)
					{
						isUserValid = false;
						errorMsgBuilder.Append(Resources.DuplicatedApplicationLogonErrorMsgSemicolon + " ");
						_personRepository.Remove(person);
					}
				}

				if (isUserValid)
				{
					continue;
				}

				var errorMsg = errorMsgBuilder.ToString();
				errorMsg = errorMsg.Substring(0, errorMsg.Length - 2);
				user.ErrorMessage = errorMsg;
				invalidUsers.Add(user);
			}
			
			return new
			{
				InvalidUsers = invalidUsers,
				SuccessfulCount = users.Count() - invalidUsers.Count,
				InvalidCount = invalidUsers.Count
			};
		}
	}
}