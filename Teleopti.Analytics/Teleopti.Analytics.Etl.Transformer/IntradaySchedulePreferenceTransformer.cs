using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer
{
	public class IntradaySchedulePreferenceTransformer : IIntradaySchedulePreferenceTransformer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Transform(IEnumerable<IPreferenceDay> rootList, DataTable table, ICommonStateHolder stateHolder, IScenario scenario)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);
			var preferenceDayAndPersonList = new HashSet<PreferenceDayAndPerson>();
			foreach (var preferenceDay in rootList)
			{
				if (!CheckIfPreferenceIsValid(preferenceDay.Restriction))
					continue;

				var persons = stateHolder.PersonsWithIds(new List<Guid> { preferenceDay.Person.Id.GetValueOrDefault() });
				if (!persons.Any())
					return;

				var schedulePart = stateHolder.GetSchedulePartOnPersonAndDate(persons[0], preferenceDay.RestrictionDate, scenario);
				var preferenceDayAndPerson = new PreferenceDayAndPerson
				{
					PersonId = schedulePart.Person.Id.GetValueOrDefault(),
					PreferenceDate = preferenceDay.RestrictionDate
				};
				if (preferenceDayAndPersonList.Contains(preferenceDayAndPerson))
					continue;
				var newDataRow = table.NewRow();

				newDataRow = SchedulePreferenceTransformerHelper.FillDataRow(newDataRow, preferenceDay.Restriction, schedulePart);
				table.Rows.Add(newDataRow);
				preferenceDayAndPersonList.Add(preferenceDayAndPerson);
			}
		}

		public bool CheckIfPreferenceIsValid(IPreferenceRestriction preferenceRestriction)
		{
			return SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(preferenceRestriction);
		}

		internal class PreferenceDayAndPerson
		{
			public DateOnly PreferenceDate { get; set; }
			public Guid PersonId { get; set; }
			public override bool Equals(Object obj)
			{
				if (!(obj is PreferenceDayAndPerson)) return false;

				var p = (PreferenceDayAndPerson)obj;
				return GetHashCode().Equals(p.GetHashCode());
			}
			public override int GetHashCode()
			{
				var idHash = PersonId.GetHashCode();
				var dateHash = PreferenceDate.GetHashCode();

				return idHash ^ dateHash;
			}
		}
	}
}
