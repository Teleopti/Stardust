using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;

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
			var stateCodes = Enumerable.Range(0, count).Select(i =>
			{
				var name = Convert.ToString(i);
				var stateGroup = new RtaStateGroup(name, false, true);
				stateGroup.AddState(name, name);
				_rtaStateGroupRepository.Add(stateGroup);
				return name;
			}).ToArray();

			_unitOfWork.Current().PersistAll();
			return stateCodes;
		}
	}
}