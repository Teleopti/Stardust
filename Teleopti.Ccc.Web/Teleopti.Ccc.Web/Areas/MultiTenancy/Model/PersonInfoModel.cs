﻿using System;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Model
{
	public class PersonInfoModel
	{
		public string ApplicationLogonName { get; set; }
		public string Password { get; set; }
		public string Identity { get; set; }
		public Guid PersonId { get; set; }
	}
}