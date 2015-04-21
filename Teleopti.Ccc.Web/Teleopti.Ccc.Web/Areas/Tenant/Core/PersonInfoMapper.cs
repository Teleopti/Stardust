﻿using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.Tenant.Model;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class PersonInfoMapper : IPersonInfoMapper
	{
		private readonly IFindTenantByNameQuery _findTenantByNameQuery;
		private readonly ICheckPasswordStrength _checkPasswordStrength;

		public PersonInfoMapper(IFindTenantByNameQuery findTenantByNameQuery, ICheckPasswordStrength checkPasswordStrength)
		{
			_findTenantByNameQuery = findTenantByNameQuery;
			_checkPasswordStrength = checkPasswordStrength;
		}

		public PersonInfo Map(PersonInfoModel personInfoModel)
		{
			var id = personInfoModel.PersonId ?? Guid.Empty;
			var tenant = _findTenantByNameQuery.Find(personInfoModel.Tenant);
			var dateOnly = personInfoModel.TerminalDate.HasValue ? new DateOnly(personInfoModel.TerminalDate.Value) : (DateOnly?)null;
			var personInfo = new PersonInfo(tenant) { Id = id, TerminalDate = dateOnly};
			personInfo.SetIdentity(personInfoModel.Identity);
			personInfo.SetApplicationLogonCredentials(_checkPasswordStrength, personInfoModel.ApplicationLogonName, personInfoModel.Password);
			return personInfo;
		}
	}
}