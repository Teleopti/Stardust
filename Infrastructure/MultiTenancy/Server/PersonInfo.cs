using System;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class PersonInfo
	{
		private readonly Tenant tenant;
		private ApplicationLogonInfo _applicationLogonInfo;

		public PersonInfo() : this(new Tenant(string.Empty), Guid.NewGuid())
		{
		}

		public PersonInfo(Tenant tenant, Guid personId)
		{
			this.tenant = tenant;
			Id = personId;
		}

		public virtual Guid Id { get; protected set; }
		//TODO: tenant move these to applicationlogoninfo
		public virtual string ApplicationLogonName { get; protected set; }
		//make private when oldschema is gone!
		public virtual string Password { get; protected set; }
		//
		public virtual string Identity { get; protected set; }
		public virtual DateOnly? TerminalDate { get; set; }

		public virtual ApplicationLogonInfo ApplicationLogonInfo
		{
			get
			{
				//Todo: tenant - remove this when app logon and password is moved to applicationlogoninfo
				return _applicationLogonInfo ?? (_applicationLogonInfo = new ApplicationLogonInfo(this));
			}
			protected set { _applicationLogonInfo = value; }
		}


		public virtual string Tenant
		{
			get { return tenant.Name; }
		}

		public virtual void SetApplicationLogonCredentials(ICheckPasswordStrength checkPasswordStrength, string logonName, string password)
		{
			if (logonName == null || password == null)
				return;
			setPassword(checkPasswordStrength, password);
			ApplicationLogonName = logonName;
		}

		public virtual void SetIdentity(string identityName)
		{
			Identity = identityName;
		}

		private void setPassword(ICheckPasswordStrength checkPasswordStrength, string newPassword)
		{
			checkPasswordStrength.Validate(newPassword);
			//todo: tenant get rid of domain dependency here
			Password = new OneWayEncryption().EncryptString(newPassword);
		}
	}
}