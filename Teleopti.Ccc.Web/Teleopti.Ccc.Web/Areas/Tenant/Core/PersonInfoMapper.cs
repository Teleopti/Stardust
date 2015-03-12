using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.Tenant.Model;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class PersonInfoMapper : IPersonInfoMapper
	{
		private readonly IFindTenantByNameQuery _findTenantByNameQuery;

		public PersonInfoMapper(IFindTenantByNameQuery findTenantByNameQuery)
		{
			_findTenantByNameQuery = findTenantByNameQuery;
		}

		public PersonInfo Map(PersonInfoModel personInfoModel)
		{
			var id = personInfoModel.PersonId.HasValue ?
							personInfoModel.PersonId.Value :
							Guid.Empty;
			var tenant = _findTenantByNameQuery.Find(personInfoModel.Tenant);
			var personInfo = new PersonInfo(tenant) { Id = id, TerminalDate = personInfoModel.TerminalDate};
			personInfo.SetIdentity(personInfoModel.Identity);
			personInfo.SetApplicationLogonName(personInfoModel.UserName);
			personInfo.SetPassword(personInfoModel.Password);
			return personInfo;
		}
	}
}