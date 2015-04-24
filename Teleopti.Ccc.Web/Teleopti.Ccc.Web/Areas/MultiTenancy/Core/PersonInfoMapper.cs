using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
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
			var id = personInfoModel.PersonId;
			var tenant = _findTenantByNameQuery.Find(personInfoModel.Tenant);
			var dateOnly = personInfoModel.TerminalDate.HasValue ? new DateOnly(personInfoModel.TerminalDate.Value) : (DateOnly?)null;
			var personInfo = new PersonInfo(tenant, id) { TerminalDate = dateOnly};
			personInfo.SetIdentity(personInfoModel.Identity);
			personInfo.SetApplicationLogonCredentials(_checkPasswordStrength, personInfoModel.ApplicationLogonName, personInfoModel.Password);
			return personInfo;
		}
	}
}