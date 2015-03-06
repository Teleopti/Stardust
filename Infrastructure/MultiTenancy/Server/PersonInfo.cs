using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class PersonInfo
	{
		private string applicationLogonName;
		private string identity;
		private readonly Tenant tenant;

#pragma warning disable 169
		private DateOnly? terminalDate;
#pragma warning restore 169

		public PersonInfo()
		{
			tenant=new Tenant(string.Empty);
		}

		//TODO: tenant - remove
		public PersonInfo(string tenantName)
		{
			tenant=new Tenant(tenantName);
		}

		public PersonInfo(Tenant tenant)
		{
			this.tenant = tenant;
		}

		public virtual Guid Id { get; set; }
		public virtual string Password { get; set; }

		public virtual string Tenant
		{
			get { return tenant.Name; }
		}

		public virtual void SetApplicationLogonName(string logonName)
		{
			applicationLogonName = logonName;
		}

		public virtual void SetIdentity(string identityName)
		{
			identity = identityName;
		}
	}
}