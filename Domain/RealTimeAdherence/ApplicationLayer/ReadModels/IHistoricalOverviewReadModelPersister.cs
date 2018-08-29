using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels
{
	public interface IHistoricalOverviewReadModelPersister
	{
		void Upsert(HistoricalOverviewReadModel model);
	}

	public interface IHistoricalOverviewReadModelReader
	{
		IEnumerable<HistoricalOverviewReadModel> Read(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds);
	}
	
	public class HistoricalOverviewReadModel
	{
		public DateTime Date;
		public Guid PersonId;
		
		public string SiteName { get; set; }
		public string TeamName { get; set; }
		public Guid? TeamId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int? Adherence { get; set; }
	}
}