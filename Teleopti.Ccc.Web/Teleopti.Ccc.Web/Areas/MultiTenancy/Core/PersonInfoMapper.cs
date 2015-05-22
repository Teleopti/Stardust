using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class PersonInfoMapper : IPersonInfoMapper
	{
		private readonly ICurrentTenant _currentTenant;
		private readonly ICheckPasswordStrength _checkPasswordStrength;

		public PersonInfoMapper(ICurrentTenant currentTenant, 
														ICheckPasswordStrength checkPasswordStrength)
		{
			_currentTenant = currentTenant;
			_checkPasswordStrength = checkPasswordStrength;
		}

		public PersonInfo Map(PersonInfoModel personInfoModel)
		{
			var id = personInfoModel.PersonId;
			var personInfo = new PersonInfo(_currentTenant.Current(), id);
			personInfo.SetIdentity(personInfoModel.Identity);
			personInfo.SetApplicationLogonCredentials(_checkPasswordStrength, personInfoModel.ApplicationLogonName, personInfoModel.Password);
			return personInfo;
		}
	}
}