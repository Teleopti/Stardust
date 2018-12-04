using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Wfm.Adherence.Monitor
{
	public class PhoneStateViewModel
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

		public IEnumerable<PhoneStateViewModel> Build()
		{
			return _stateGroups.LoadAll()
				.Select(x => new PhoneStateViewModel
				{
					Id = x.Id.GetValueOrDefault(),
					Name = x.Name
				})
				.ToArray();
		}
	}
}