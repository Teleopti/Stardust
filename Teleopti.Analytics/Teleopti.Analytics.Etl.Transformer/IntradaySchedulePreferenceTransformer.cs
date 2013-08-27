using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
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

            foreach (var preferenceDay in rootList)
            {
	            if (!CheckIfPreferenceIsValid(preferenceDay.Restriction)) 
					continue;

	            var persons = stateHolder.PersonsWithIds(new List<Guid> {preferenceDay.Person.Id.GetValueOrDefault()});
	            if (!persons.Any()) 
					return;

	            var newDataRow = table.NewRow();
	            var schedulePart = stateHolder.GetSchedulePartOnPersonAndDate(persons[0], preferenceDay.RestrictionDate, scenario);
				newDataRow = SchedulePreferenceTransformerHelper.FillDataRow(newDataRow, preferenceDay.Restriction, schedulePart);
	            table.Rows.Add(newDataRow);
            }
        }

		public bool CheckIfPreferenceIsValid(IPreferenceRestriction preferenceRestriction)
		{
			return SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(preferenceRestriction);
        }

        
    }
}
