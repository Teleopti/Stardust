using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class PersonInfoHelper : IPersonInfoHelper
	{
		private readonly ICheckPasswordStrength _checkPasswordStrength;
		private readonly IHashFunction _currentHashFunction;
		private readonly IFindTenantByName _findTenant;
		private readonly ICurrentDataSource _currentDataSource; 

		public PersonInfoHelper(ICheckPasswordStrength checkPasswordStrength, IHashFunction currentHashFunction, IFindTenantByName findTenant, ICurrentDataSource currentDataSource)
		{
			_checkPasswordStrength = checkPasswordStrength;
			_currentHashFunction = currentHashFunction;
			_findTenant = findTenant;
			_currentDataSource = currentDataSource;
		}

		public PersonInfo Create(IPersonInfoModel personInfoModel)
		{
			var id = personInfoModel.PersonId;
			var currentTenant = _findTenant.Find(_currentDataSource.CurrentName());
			var personInfo = new PersonInfo(currentTenant, id);
			personInfo.SetIdentity(personInfoModel.Identity);
			personInfo.SetApplicationLogonCredentials(_checkPasswordStrength, personInfoModel.ApplicationLogonName, personInfoModel.Password, _currentHashFunction);
			return personInfo;
		}

		public Tenant GetCurrentTenant()
		{
			return _findTenant.Find(_currentDataSource.CurrentName());
		}
	}
}