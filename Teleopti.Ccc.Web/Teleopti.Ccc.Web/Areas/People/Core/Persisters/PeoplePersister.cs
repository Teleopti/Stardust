using System;
using System.Collections.Generic;
using System.Text;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.Web.Areas.People.Controllers;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
		private readonly IUnitOfWorkFactory _uowFactory;

		public PeoplePersister(IPersistPersonInfo personInfoPersister, IPersonInfoMapper mapper, IApplicationRoleRepository roleRepository,
			IPersonRepository personRepository, ILoggedOnUser currentLoggedOnUser, IUserValidator userValidator,
			IUnitOfWorkFactory uowFactory)
		{
			_personInfoPersister = personInfoPersister;
			_mapper = mapper;
			_roleRepository = roleRepository;
			_personRepository = personRepository;
			_currentLoggedOnUser = currentLoggedOnUser;
			_userValidator = userValidator;
			_uowFactory = uowFactory;
		}

		public IList<RawUser> Persist(IEnumerable<RawUser> users)
		{
			var invalidUsers = new List<RawUser>();

			var availableRoles = new Dictionary<string, IApplicationRole>();
			using (var uow = _uowFactory.CreateAndOpenUnitOfWork())
			{
				var allRoles = _roleRepository.LoadAllApplicationRolesSortedByName();
				allRoles.ForEach(r =>
				{
					if (!availableRoles.ContainsKey(r.DescriptionText.ToUpper()))
					{
						availableRoles.Add(r.DescriptionText.ToUpper(), r);
					}
				});
			}

			foreach (var user in users)
			{
				var isUserValid = _userValidator.Validate(user, availableRoles);
				var errorMsgBuilder = new StringBuilder(_userValidator.ErrorMessage);

				if (isUserValid)
				{
					var person = new Person
					{
						Name =
							new Name(user.Firstname.IsNullOrEmpty() ? " " : user.Firstname,
								user.Lastname.IsNullOrEmpty() ? " " : user.Lastname)
					};
					person.PermissionInformation.SetDefaultTimeZone(
									_currentLoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone());
					_userValidator.ValidRoles.ForEach(r => person.PermissionInformation.AddApplicationRole(r));

					try
					{
						using (var uow = _uowFactory.CreateAndOpenUnitOfWork())
						{
							_personRepository.Add(person);
						}
					}
					catch (Exception ex)
					{
						isUserValid = false;
						// TODO: What error should be handled?
						errorMsgBuilder.AppendFormat("Failed to create new person with ApplicationUserId {0} for unknown reason.", user.ApplicationUserId);
					}

					if (isUserValid)
					{
						var tenantUserData = new PersonInfoModel()
						{
							ApplicationLogonName = user.ApplicationUserId.IsNullOrEmpty() ? null : user.ApplicationUserId,
							Identity = user.WindowsUser.IsNullOrEmpty() ? null : user.WindowsUser,
							Password = user.Password.IsNullOrEmpty() ? null : user.Password,
							PersonId = person.Id.GetValueOrDefault()
						};

						try
						{
							persistTenatData(tenantUserData);
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
						catch (Exception e)
						{
							isUserValid = false;
							errorMsgBuilder.AppendFormat("Failed to create new person with ApplicationUserId {0} for unknown reason.", user.ApplicationUserId);
							_personRepository.Remove(person);
						}
					}
				}

				if (isUserValid) continue;

				var errorMsg = errorMsgBuilder.ToString();
				errorMsg = errorMsg.Substring(0, errorMsg.Length - 2);
				user.ErrorMessage = errorMsg;
				invalidUsers.Add(user);
			}

			return invalidUsers;
		}

		[TenantUnitOfWork]
		protected virtual void persistTenatData(PersonInfoModel tenantUserData)
		{
			_personInfoPersister.Persist(_mapper.Map(tenantUserData));
		}
	}
}