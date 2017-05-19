using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public class HistoricalChangeReadModel
	{
		public Guid PersonId { get; set; }
		public DateOnly? BelongsToDate { get; set; }
		public DateTime Timestamp { get; set; }

		public string StateName { get; set; }
		public Guid? StateGroupId { get; set; }

		public string ActivityName { get; set; }
		public int? ActivityColor { get; set; }

		public string RuleName { get; set; }
		public int? RuleColor { get; set; }

		public HistoricalChangeInternalAdherence? Adherence { get; set; }
	}

	public interface IHistoricalChangeReadModelPersister
	{
		void Persist(HistoricalChangeReadModel model);
		void Remove(DateTime until);
	}

	public interface IHistoricalChangeReadModelReader
	{
		IEnumerable<HistoricalChangeReadModel> Read(Guid personId, DateTime startTime, DateTime endTime);
	}

	public enum HistoricalChangeInternalAdherence
	{
		In,
		Out,
		Neutral
	}
}