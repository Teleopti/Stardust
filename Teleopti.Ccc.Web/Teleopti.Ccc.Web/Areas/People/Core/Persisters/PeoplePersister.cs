using System;
using System.Collections.Generic;
using System.Text;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Persisters
{
	public class PeoplePersister : IPeoplePersister
	{
		private readonly IPersistPersonInfo _personInfoPersister;
		private readonly IPersonInfoMapper _mapper;
		private readonly IApplicationRoleRepository _roleRepository;
		private readonly IPersonRepository _personRepository;
		private readonly ILoggedOnUser _currentLoggedOnUser;
		private readonly IUserValidator _userValidator;
		private IDictionary<string, IApplicationRole> _allRoles;
		private bool initialized;

		public PeoplePersister(IPersistPersonInfo personInfoPersister, IPersonInfoMapper mapper, IApplicationRoleRepository roleRepository,
			IPersonRepository personRepository, ILoggedOnUser currentLoggedOnUser, IUserValidator userValidator)
		{
			_personInfoPersister = personInfoPersister;
			_mapper = mapper;
			_roleRepository = roleRepository;
			_personRepository = personRepository;
			_currentLoggedOnUser = currentLoggedOnUser;
			_userValidator = userValidator;

			initialized = false;
		}

		public IList<RawUser> Persist(IEnumerable<RawUser> users)
		{
			if (!initialized)
			{
				GetAllRoles();
				initialized = true;
			}

			var invalidUsers = new List<RawUser>();

			foreach (var user in users)
			{
				var errorMsgBuilder = new StringBuilder();
				var isUserValid = _userValidator.Validate(user, _allRoles, errorMsgBuilder);

				if (isUserValid)
				{
					IPerson person = null;
					try
					{
						person = PersistPerson(user);
					}
					catch (Exception e)
					{
						isUserValid = false;
						errorMsgBuilder.AppendFormat((Resources.InternalErrorXMsg + " "), e.Message);
					}

					if (isUserValid)
					{
						var tenantUserData = new PersonInfoModel
						{
							ApplicationLogonName = user.ApplicationUserId.IsNullOrEmpty() ? null : user.ApplicationUserId,
							Identity = user.WindowsUser.IsNullOrEmpty() ? null : user.WindowsUser,
							Password = user.Password.IsNullOrEmpty() ? null : user.Password,
							PersonId = person.Id.GetValueOrDefault()
						};

						try
						{
							PersistTenatData(tenantUserData);
						}
						catch (PasswordStrengthException)
						{
							isUserValid = false;
							errorMsgBuilder.Append(Resources.PasswordPolicyErrorMsgSemicolon + " ");
							RemovePerson(person);
						}
						catch (DuplicateIdentityException)
						{
							isUserValid = false;
							errorMsgBuilder.Append(Resources.DuplicatedWindowsLogonErrorMsgSemicolon + " ");
							RemovePerson(person);
						}
						catch (DuplicateApplicationLogonNameException)
						{
							isUserValid = false;
							errorMsgBuilder.Append(Resources.DuplicatedApplicationLogonErrorMsgSemicolon + " ");
							RemovePerson(person);
						}
						catch (Exception e)
						{
							isUserValid = false;
							errorMsgBuilder.AppendFormat((Resources.InternalErrorXMsg + " "), e.Message);
							RemovePerson(person);
						}
					}
				}

				if (isUserValid) continue;

				var errorMessage = errorMsgBuilder.ToString();
				user.ErrorMessage = errorMessage.Substring(0, errorMessage.Length - 2);
				invalidUsers.Add(user);
			}

			return invalidUsers;
		}

		private IPerson createPersonFromModel(RawUser rawPerson)
		{
			var person = new Person
			{
				Name = new Name(rawPerson.Firstname ?? " ", rawPerson.Lastname ?? " ")
			};

			var timeZone = _currentLoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			person.PermissionInformation.SetDefaultTimeZone(timeZone);

			if (!string.IsNullOrEmpty(rawPerson.Role))
			{
				var roleNames = StringHelper.SplitStringList(rawPerson.Role);
				foreach (var roleName in roleNames)
				{
					person.PermissionInformation.AddApplicationRole(_allRoles[roleName.ToUpper()]);
				}
			}

			return person;
		}

		[UnitOfWork]
		protected virtual void GetAllRoles()
		{
			_allRoles = new Dictionary<string, IApplicationRole>();
			var roles = _roleRepository.LoadAllApplicationRolesSortedByName();
			roles.ForEach(r =>
			{
				if (!_allRoles.ContainsKey(r.DescriptionText.ToUpper()))
				{
					_allRoles.Add(r.DescriptionText.ToUpper(), r);
				}
			});
		}

		[UnitOfWork]
		protected virtual IPerson PersistPerson(RawUser user)
		{
			var person = createPersonFromModel(user);
			_personRepository.Add(person);

			return person;
		}

		[UnitOfWork]
		protected virtual void RemovePerson(IPerson person)
		{
			_personRepository.Remove(person);
		}

		[TenantUnitOfWork]
		protected virtual void PersistTenatData(PersonInfoModel tenantUserData)
		{
			_personInfoPersister.Persist(_mapper.Map(tenantUserData));
		}
	}
}
