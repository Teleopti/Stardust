using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class HistoricalAdherenceReadModelPersister : IHistoricalAdherenceReadModelPersister {

		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly IJsonSerializer _serializer;

		public HistoricalAdherenceReadModelPersister(ICurrentUnitOfWork unitOfWork, IJsonSerializer serializer)
		{
			_unitOfWork = unitOfWork;
			_serializer = serializer;
		}

		public void AddIn(Guid personid, DateOnly date, DateTime timestamp)
		{
		}

		public void AddNeutral(Guid personid, DateOnly date, DateTime timestamp)
		{
			throw new NotImplementedException();
		}

		public void AddOut(Guid personid, DateOnly date, DateTime timestamp)
		{
			throw new NotImplementedException();
		}
	}

}