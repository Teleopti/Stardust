using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
	public class PeoplePersister : IPeoplePersister
	{
		private readonly IPersistPersonInfo _personInfoPersister;
		private readonly IPersonInfoMapper _mapper;
		private readonly IPersonRepository _personRepository;
		private readonly ILoggedOnUser _currentLoggedOnUser;
		private readonly IUserValidator _userValidator;

		public PeoplePersister(IPersistPersonInfo personInfoPersister, IPersonInfoMapper mapper, IPersonRepository personRepository, ILoggedOnUser currentLoggedOnUser, IUserValidator userValidator)
		{
			_personInfoPersister = personInfoPersister;
			_mapper = mapper;
			_personRepository = personRepository;
			_currentLoggedOnUser = currentLoggedOnUser;
			_userValidator = userValidator;
		}

		public IList<RawUser> Persist(IEnumerable<RawUser> users)
		{
			var invalidUsers = new List<RawUser>();
			foreach (var user in users)
			{
				var isUserValid = _userValidator.Validate(user);
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
					_personRepository.Add(person);
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
						//unknown erro
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