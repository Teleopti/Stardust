using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class RtaTracerReader : IRtaTracerReader
	{
		private readonly WithAnalyticsUnitOfWork _unitOfWork;

		public RtaTracerReader(WithAnalyticsUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IEnumerable<RtaTracerLog<T>> ReadOfType<T>()
		{
			throw new System.NotImplementedException();
		}
	}
}