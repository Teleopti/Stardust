using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class SiteAdherence
	{
		private readonly Dictionary<Guid, bool> _personAdherence = new Dictionary<Guid, bool>(); 

		public bool TryUpdateAdherence(Guid personId, double staffingEffect)
		{
			var adherence = staffingEffect.Equals(0);
			var changed = !_personAdherence.ContainsKey(personId) || 
			              _personAdherence[personId] != adherence;
			_personAdherence[personId] = adherence;
			return changed;
		}

		public int NumberOutOfAdherence()
		{
			return _personAdherence.Values.Count(x => x == false);
		}
	}
}