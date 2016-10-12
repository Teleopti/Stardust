using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeHistoricalAdherenceReadModelPersister:
		IHistoricalAdherenceReadModelPersister,
		IHistoricalAdherenceReadModelReader

	{
		private readonly IList<internalModel> _data = new List<internalModel>();

		public FakeHistoricalAdherenceReadModelPersister Has(HistoricalAdherenceReadModel model)
		{
			model.OutOfAdherences?
				.ForEach(x =>
				{
					AddOut(model.PersonId, model.Date, x.StartTime);
					if (x.EndTime.HasValue)
						AddIn(model.PersonId, model.Date, x.EndTime.Value);
				});
			return this;
		}

		public void AddIn(Guid personid, DateOnly date, DateTime timestamp)
		{
			_data.Add(new internalModel
			{
				PersonId = personid,
				Date = date,
				Timestamp = timestamp,
				Adherence = 0
			});
		}

		public void AddNeutral(Guid personid, DateOnly date, DateTime timestamp)
		{
			_data.Add(new internalModel
			{
				PersonId = personid,
				Date = date,
				Timestamp = timestamp,
				Adherence = 1
			});
		}

		public void AddOut(Guid personid, DateOnly date, DateTime timestamp)
		{
			_data.Add(new internalModel
			{
				PersonId = personid,
				Date = date,
				Timestamp = timestamp,
				Adherence = 2
			});
		}

		public HistoricalAdherenceReadModel Read(Guid personId, DateOnly date)
		{
			IEnumerable<internalModel> filteredData = _data
				.Where(x => x.PersonId == personId && x.Date == date)
				.OrderBy(x => x.Timestamp)
				.ToArray();
			var first = filteredData.FirstOrDefault();
			if (first != null && first.Adherence != 2)
			{
				var matchingOut = _data
					.OrderBy(x => x.Timestamp)
					.Reverse()
					.FirstOrDefault(x => x.PersonId == personId && x.Date < date && x.Adherence == 2);

				if (matchingOut != null)
					filteredData = new[] {matchingOut}.Concat(filteredData);
			}
			var last = filteredData.LastOrDefault();
			if (last != null && last.Adherence == 2)
			{
				var matchingIn = _data
					.OrderBy(x => x.Timestamp)
					.FirstOrDefault(x => x.PersonId == personId && x.Date > date && x.Adherence != 2);

				if (matchingIn != null)
					filteredData = filteredData.Append(matchingIn);
			}

			var aggregatedList = filteredData
				.Aggregate(new List<HistoricalOutOfAdherenceReadModel>(), (x, im) =>
				{
					if (im.Adherence == 2)
						x.Add(new HistoricalOutOfAdherenceReadModel { StartTime = im.Timestamp });
					else
					{
						var existing = x.FirstOrDefault(y => !y.EndTime.HasValue);
						if (existing != null)
							existing.EndTime = im.Timestamp;
					}

					return x;
				})
				.Where(x =>
				{
					if (!x.EndTime.HasValue) return true;
					var d = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
					return new DateTimePeriod(x.StartTime, x.EndTime.Value)
						.Intersect(new DateTimePeriod(d, d.AddDays(1)));
				})
				.ToArray();
			return new HistoricalAdherenceReadModel
			{
				PersonId = personId,
				Date = date,
				OutOfAdherences = aggregatedList
			};
		}

		private class internalModel
		{
			public Guid PersonId { get; set; }
			public DateOnly Date { get; set; }
			public DateTime Timestamp { get; set; }
			public int Adherence { get; set; }
		}
	}
}