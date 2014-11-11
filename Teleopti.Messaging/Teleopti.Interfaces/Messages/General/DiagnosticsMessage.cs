﻿using System;

namespace Teleopti.Interfaces.Messages.General
{
	public class DiagnosticsMessage : MessageWithLogOnInfo
	{
		private readonly Guid _identity = Guid.NewGuid();

		public override Guid Identity
		{
			get { return _identity; }
		}
	}
}