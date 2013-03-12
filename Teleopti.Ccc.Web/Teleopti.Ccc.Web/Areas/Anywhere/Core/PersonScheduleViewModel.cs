﻿using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class PersonScheduleViewModel
	{
		public string Name { get; set; }
		public string Site { get; set; }
		public string Team { get; set; }
		public IEnumerable<PersonScheduleViewModelLayer> Layers { get; set; }
	}

	public class PersonScheduleViewModelLayer
	{
		public string Color { get; set; }
		public DateTime? Start { get; set; }
		public int Minutes { get; set; }
	}
}