﻿using System;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public class PersonApplicationLogonModel
	{
		public string ApplicationLogonName { get; set; }
		public Guid PersonId { get; set; }
	}
}
