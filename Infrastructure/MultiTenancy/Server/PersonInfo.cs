using System;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class PersonInfo
	{
		private readonly Tenant tenant;
		private ApplicationLogonInfo _applicationLogonInfo;

		public PersonInfo() : this(new Tenant(string.Empty))
		{
		}

		public PersonInfo(Tenant tenant)
		{
			this.tenant = tenant;
		}

		public virtual Guid Id { get; set; }
		//TODO: tenant move these to applicationlogoninfo
		public virtual string ApplicationLogonName { get; protected set; }
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

		public virtual void SetApplicationLogonCredentials(string logonName, string password)
		{
			//need to check password policy here!
			ApplicationLogonName = logonName;
			setPassword(password);
		}

		public virtual void SetIdentity(string identityName)
		{
			Identity = identityName;
		}

		private void setPassword(string newPassword)
		{
			//todo: tenant get rid of domain dependency here
			Password = new OneWayEncryption().EncryptString(newPassword);
		}
	}
}