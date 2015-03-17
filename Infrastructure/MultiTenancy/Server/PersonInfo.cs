﻿using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server
{
	public class PersonInfo
	{
		private readonly Tenant tenant;

		public PersonInfo()
		{
			tenant=new Tenant(string.Empty);
		}

		public PersonInfo(Tenant tenant)
		{
			this.tenant = tenant;
		}

		public virtual Guid Id { get; set; }
		public virtual string Password { get; protected set; }
		public virtual string Identity { get; protected set; }
		public virtual string ApplicationLogonName { get; protected set; }
		public virtual DateOnly? TerminalDate { get; set; }

		public virtual string Tenant
		{
			get { return tenant.Name; }
		}

		public virtual void SetApplicationLogonName(string logonName)
		{
			ApplicationLogonName = logonName;
		}

		public virtual void SetIdentity(string identityName)
		{
			Identity = identityName;
		}

		public virtual void SetPassword(string password)
		{
			Password = password;
		}
	}
}