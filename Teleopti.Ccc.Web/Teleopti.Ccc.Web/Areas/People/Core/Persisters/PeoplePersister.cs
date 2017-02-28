using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.Web.Areas.People.Core.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.People.Core.Persisters
{
	public class PeoplePersister : IPeoplePersister
	{
		private readonly IApplicationRoleRepository _roleRepository;
		private readonly IPersonRepository _personRepository;
		private readonly ILoggedOnUser _currentLoggedOnUser;
		private readonly IUserValidator _userValidator;
		private IDictionary<string, IApplicationRole> _allRoles;
		private readonly ITenantUserPersister _tenantUserPersister;
		private bool initialized;

		public PeoplePersister(IApplicationRoleRepository roleRepository,
			IPersonRepository personRepository, ILoggedOnUser currentLoggedOnUser, IUserValidator userValidator, ITenantUserPersister tenantUserPersister)
		{
			_roleRepository = roleRepository;
			_personRepository = personRepository;
			_currentLoggedOnUser = currentLoggedOnUser;
			_userValidator = userValidator;
			_tenantUserPersister = tenantUserPersister;

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

						var errorMessages = _tenantUserPersister.Persist(tenantUserData);

						if (errorMessages.Any())
						{
							isUserValid = false;

							RemovePerson(person);

							errorMessages.ForEach(m =>
							{
								errorMsgBuilder.Append(m + " ");
							});
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
			var person = new Person();
			person.SetName(new Name(rawPerson.Firstname ?? " ", rawPerson.Lastname ?? " "));

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
	}
}
