﻿using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Tenant.Model
{
	public class PersonInfoModels
	{
		public IEnumerable<PersonInfoModel> PersonInfos { get; set; }
	}

	public class PersonInfoModel
	{
		public string Tenant { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Identity { get; set; }
		public DateOnly? TerminalDate { get; set; }
		public Guid? PersonId { get; set; }
	}
}