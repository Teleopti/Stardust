﻿using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class PersonAccountUpdaterDummy : IPersonAccountUpdater
	{
		public void Update(IPerson person)
		{
			throw new NotImplementedException();
		}
	}
}