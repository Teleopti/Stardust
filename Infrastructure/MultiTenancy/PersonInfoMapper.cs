using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Security;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
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

		public PersonInfo Map(IPersonInfoModel personInfoModel)
		{
			var id = personInfoModel.PersonId;
			var personInfo = new PersonInfo(_currentTenant.Current(), id);
			personInfo.SetIdentity(personInfoModel.Identity);
			personInfo.SetApplicationLogonCredentials(_checkPasswordStrength, personInfoModel.ApplicationLogonName, personInfoModel.Password, _currentHashFunction);
			return personInfo;
		}
	}
}