using System;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class PersonInfo
	{
		private ApplicationLogonInfo _applicationLogonInfo;

		public PersonInfo() : this(new Tenant(string.Empty), Guid.NewGuid())
		{
		}

		public PersonInfo(Tenant tenant, Guid personId)
		{
			Tenant = tenant;
			Id = personId;
			RegenerateTenantPassword();
		}

		public virtual Guid Id { get; protected set; }
		public virtual string TenantPassword { get; protected set; }
		public virtual string Identity { get; protected set; }
		public virtual Tenant Tenant { get; protected set; }

		public virtual ApplicationLogonInfo ApplicationLogonInfo
		{
			get { return _applicationLogonInfo ?? (_applicationLogonInfo = new ApplicationLogonInfo());}
			protected set { _applicationLogonInfo = value; }
		}

		public virtual void SetApplicationLogonCredentials(ICheckPasswordStrength checkPasswordStrength, string logonName, string password)
		{
			if (logonName == null || password == null)
				return;
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
	}
}