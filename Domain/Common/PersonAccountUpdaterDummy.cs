using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class PersonAccountUpdaterDummy : IPersonAccountUpdater
	{

		public void UpdateOnTermination(DateOnly terminalDate, IPerson person)
		{
			// should do nothing
		}

		public void UpdateOnActivation(IPerson person)
		{
			// should do nothing
		}
	}
}