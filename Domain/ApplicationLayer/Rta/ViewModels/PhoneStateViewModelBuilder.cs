using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class PhoneStateViewModel
	{
		public IEnumerable<PhoneStates> PhoneStates { get; set; }
	}

	public class PhoneStates
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}

	public class PhoneStateViewModelBuilder
	{
		private readonly IRtaStateGroupRepository _stateGroups;

		public PhoneStateViewModelBuilder(IRtaStateGroupRepository stateGroups)
		{
			_stateGroups = stateGroups;
		}

		public PhoneStateViewModel For(Guid[] ids)
		{
			var lookup = _stateGroups.LoadAll().ToLookup(s => s.Id.Value);
			return new PhoneStateViewModel
			{
				PhoneStates = (from id in ids
					select new PhoneStates {Id = id, Name = lookup[id].First().Name}).ToArray()
			};
		}
	}
}