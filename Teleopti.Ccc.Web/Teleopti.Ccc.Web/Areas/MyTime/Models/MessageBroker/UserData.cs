﻿using System;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.MessageBroker
{
	public class UserData
	{
		public Guid BusinessUnitId { get; set; }
		public string DataSourceName { get; set; }
		public string Url { get; set; }
	}
}