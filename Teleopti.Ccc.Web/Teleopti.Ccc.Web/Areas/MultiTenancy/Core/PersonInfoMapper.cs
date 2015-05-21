using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class PersonInfoMapper : IPersonInfoMapper
	{
		private readonly ICurrentTenantUser _currentTenantUser;
		private readonly ICheckPasswordStrength _checkPasswordStrength;

		public PersonInfoMapper(ICurrentTenantUser currentTenantUser, 
														ICheckPasswordStrength checkPasswordStrength)
		{
			_currentTenantUser = currentTenantUser;
			_checkPasswordStrength = checkPasswordStrength;
		}

		public PersonInfo Map(PersonInfoModel personInfoModel)
		{
			var id = personInfoModel.PersonId;
			var personInfo = new PersonInfo(_currentTenantUser.CurrentUser().Tenant, id);
			personInfo.SetIdentity(personInfoModel.Identity);
			personInfo.SetApplicationLogonCredentials(_checkPasswordStrength, personInfoModel.ApplicationLogonName, personInfoModel.Password);
			return personInfo;
		}
	}
}