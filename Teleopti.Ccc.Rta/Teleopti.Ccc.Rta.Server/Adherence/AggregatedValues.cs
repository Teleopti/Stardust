﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Rta.Server.Adherence
{
	public class AggregatedValues
	{
		private readonly Dictionary<Guid, bool> _personAdherence = new Dictionary<Guid, bool>();
		public Guid Key { get; private set; }

		public AggregatedValues(Guid key)
		{
			Key = key;
		}

		public bool TryUpdateAdherence(Guid personId, double staffingEffect)
		{
			var inAdherence = staffingEffect.Equals(0);
			var changed = !_personAdherence.ContainsKey(personId) || 
			              _personAdherence[personId] != inAdherence;
			_personAdherence[personId] = inAdherence;
			return changed;
		}

		public int NumberOutOfAdherence()
		{
			return _personAdherence.Values.Count(x => x == false);
		}
	}
}