using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy
{
	public class PersonInfo
	{
#pragma warning disable 169
		private DateOnly? terminalDate;
		private string applicationLogonName;
		private string identity;
#pragma warning restore 169
#pragma warning disable 649
		private Tenant tenant;
#pragma warning restore 649

		public PersonInfo()
		{
			tenant=new Tenant(string.Empty);
		}

		public PersonInfo(string tenantName)
		{
			tenant=new Tenant(tenantName);
		}


		public virtual Guid Id { get; set; }
		public virtual string Password { get; set; }

		public virtual string Tenant
		{
			get { return tenant.Name; }
		}
	}
}