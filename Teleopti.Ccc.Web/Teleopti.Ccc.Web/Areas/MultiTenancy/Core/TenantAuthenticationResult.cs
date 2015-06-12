﻿using System;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public class TenantAuthenticationResult
	{
		public bool Success { get; set; }
		public string FailReason { get; set; }
		public Guid PersonId { get; set; }
		public string Tenant { get; set; }
		public DataSourceConfiguration DataSourceConfiguration { get; set; }
		public bool PasswordExpired { get; set; }
		public string TenantPassword { get; set; }
	}
}