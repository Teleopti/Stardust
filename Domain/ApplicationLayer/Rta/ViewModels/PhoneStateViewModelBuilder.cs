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

		public IEnumerable<PhoneStateViewModel> Build(IEnumerable<Guid> ids)
		{
			return _stateGroups.LoadAll()
				.Where(s => ids == null || ids.Contains(s.Id.Value))
				.Select(x => new PhoneStateViewModel
				{
					Id = x.Id.GetValueOrDefault(),
					Name = x.Name
				})
				.ToArray();
		}
	}
}