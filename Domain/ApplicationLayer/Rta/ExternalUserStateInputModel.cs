﻿using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class ExternalUserStateInputModel
	{
		public string AuthenticationKey { get; set; }
		public string PlatformTypeId { get; set; }
		public string SourceId { get; set; }
		public string UserCode { get; set; }
		public string StateCode { get; set; }
		public string StateDescription { get; set; }
		public bool IsLoggedOn { get; set; }
		public int SecondsInState { get; set; }
		public DateTime Timestamp { get; set; }
		public DateTime BatchId { get; set; }
		public bool IsSnapshot { get; set; }
	}
}