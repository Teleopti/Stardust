using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

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
			var personInfo = new PersonInfo(tenant, id);
			personInfo.SetIdentity(personInfoModel.Identity);
			personInfo.SetApplicationLogonCredentials(_checkPasswordStrength, personInfoModel.ApplicationLogonName, personInfoModel.Password);
			return personInfo;
		}
	}
}