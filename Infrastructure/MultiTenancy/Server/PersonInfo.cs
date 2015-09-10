using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class PersonInfo
	{
		public PersonInfo() : this(new Tenant(string.Empty), Guid.NewGuid())
		{
		}

		public PersonInfo(Tenant tenant, Guid personId)
		{
			Tenant = tenant;
			Id = personId;
			RegenerateTenantPassword();
			ApplicationLogonInfo = new ApplicationLogonInfo();
		}

		public virtual Guid Id { get; protected set; }
		public virtual string TenantPassword { get; protected set; }
		public virtual string Identity { get; protected set; }
		public virtual Tenant Tenant { get; protected set; }
		public virtual ApplicationLogonInfo ApplicationLogonInfo { get; protected set; }

		public virtual void SetApplicationLogonCredentials(ICheckPasswordStrength checkPasswordStrength, string logonName, string password)
		{
			ApplicationLogonInfo.SetApplicationLogonCredentialsInternal(checkPasswordStrength, logonName, password);
		}

		public virtual void SetIdentity(string identityName)
		{
			Identity = identityName;
		}

		public virtual void RegenerateTenantPassword()
		{
			TenantPassword = Guid.NewGuid().ToString().Replace('-', 'x');
		}

		public virtual void ReuseTenantPassword(PersonInfo otherPersonInfo)
		{
			TenantPassword = otherPersonInfo.TenantPassword;
		}

		public virtual void ChangeTenant_OnlyUseInTest(Tenant newTenant)
		{
			Tenant = newTenant;
		}
	}
}