using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class PersonInfoMapper : IPersonInfoMapper
	{
		private readonly ICurrentTenant _currentTenant;
		private readonly ICheckPasswordStrength _checkPasswordStrength;
		private readonly IHashFunction _currentHashFunction;

		public PersonInfoMapper(ICurrentTenant currentTenant, 
														ICheckPasswordStrength checkPasswordStrength, IHashFunction currentHashFunction)
		{
			_currentTenant = currentTenant;
			_checkPasswordStrength = checkPasswordStrength;
			_currentHashFunction = currentHashFunction;
		}

		public PersonInfo Map(PersonInfoModel personInfoModel)
		{
			var id = personInfoModel.PersonId;
			var personInfo = new PersonInfo(_currentTenant.Current(), id);
			personInfo.SetIdentity(personInfoModel.Identity);
			personInfo.SetApplicationLogonCredentials(_checkPasswordStrength, personInfoModel.ApplicationLogonName, personInfoModel.Password, _currentHashFunction);
			return personInfo;
		}
	}
}