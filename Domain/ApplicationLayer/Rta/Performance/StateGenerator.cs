using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance
{
	public interface IStateGenerator
	{
		IEnumerable<string> Generate(int count);
	}
	public class StateGenerator : IStateGenerator
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IRtaStateGroupRepository _rtaStateGroupRepository;

		public StateGenerator(ICurrentUnitOfWork unitOfWork, IRtaStateGroupRepository rtaStateGroupRepository)
		{
			_unitOfWork = unitOfWork;
			_rtaStateGroupRepository = rtaStateGroupRepository;
		}

		public IEnumerable<string> Generate(int count)
		{
			var stateCodes = new List<string>();
			for (var i = 0; i < count; i++)
			{
				var name = Convert.ToString(i);
				var stateGroup = new RtaStateGroup(name, false, true);
				stateGroup.AddState(name, name, Guid.Empty);
				_rtaStateGroupRepository.Add(stateGroup);
				stateCodes.Add(name);
			}

			_unitOfWork.Current().PersistAll();
			return stateCodes;
		}
	}
}