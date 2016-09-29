using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
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

		public IEnumerable<PhoneStateViewModel> For(Guid[] ids)
		{
			return 
				from s in _stateGroups.LoadAll()
				from id in ids
				let sid = s.Id.Value
				where sid == id
				select new PhoneStateViewModel {Id = sid, Name = s.Name};
		}
	}
}