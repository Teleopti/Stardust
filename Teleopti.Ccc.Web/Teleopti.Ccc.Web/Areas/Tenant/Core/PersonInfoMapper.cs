using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.Tenant.Model;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class PersonInfoMapper : IPersonInfoMapper
	{
		public PersonInfo Map(PersonInfoModel personInfoModel)
		{
			var id = personInfoModel.PersonId.HasValue ?
							personInfoModel.PersonId.Value :
							Guid.Empty;
			var personInfo = new PersonInfo { Id = id, TerminalDate = personInfoModel.TerminalDate};
			personInfo.SetIdentity(personInfoModel.Identity);
			personInfo.SetApplicationLogonName(personInfoModel.UserName);
			personInfo.SetPassword(personInfoModel.Password);
			return personInfo;
		}
	}
}