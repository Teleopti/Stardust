using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels
{
	public interface IHistoricalOverviewReadModelPersister
	{
		void Upsert(HistoricalOverviewReadModel model);
	}

	public interface IHistoricalOverviewReadModelReader
	{
		IEnumerable<HistoricalOverviewReadModel> Read(IEnumerable<Guid> personIds);
	}
	
	public class HistoricalOverviewReadModel
	{
		public DateOnly Date;
		public Guid PersonId;
		public int? Adherence { get; set; }
		public bool WasLateForWork { get; set; }
		public int MinutesLateForWork { get; set; }
		public int? SecondsInAdherence { get; set; }
		public int? SecondsOutOfAdherence { get; set; }
	}
}