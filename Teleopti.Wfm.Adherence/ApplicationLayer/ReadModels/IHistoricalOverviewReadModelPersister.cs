using System;
using System.Collections.Generic;


namespace Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels
{
	public interface IHistoricalOverviewReadModelPersister
	{
		void Upsert(HistoricalOverviewReadModel model);
		void Remove(DateTime removeUntil);
	}

	public interface IHistoricalOverviewReadModelReader
	{
		IEnumerable<HistoricalOverviewReadModel> Read(IEnumerable<Guid> personIds);
	}
	
	public class HistoricalOverviewReadModel
	{
		public DateOnly Date;
		public Guid PersonId;
		public bool WasLateForWork { get; set; }
		public int MinutesLateForWork { get; set; }
		public int? SecondsInAdherence { get; set; }
		public int? SecondsOutOfAdherence { get; set; }
	}
}