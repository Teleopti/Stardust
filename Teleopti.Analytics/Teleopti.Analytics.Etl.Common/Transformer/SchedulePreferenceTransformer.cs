using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class SchedulePreferenceTransformer : ISchedulePreferenceTransformer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Transform(IEnumerable<IScheduleDay> rootList, DataTable table)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("table", table);

			foreach (var schedulePart in rootList)
			{
				var restrictionBases = schedulePart.RestrictionCollection();
				foreach (var restrictionBase in restrictionBases)
				{
					var preferenceRestriction = restrictionBase as IPreferenceRestriction;
					if (!CheckIfPreferenceIsValid(preferenceRestriction))
						continue;

					var newDataRow = table.NewRow();
					newDataRow = SchedulePreferenceTransformerHelper.FillDataRow(newDataRow, preferenceRestriction, schedulePart);
					table.Rows.Add(newDataRow);
				}
			}
		}

		public bool CheckIfPreferenceIsValid(IPreferenceRestriction preferenceRestriction)
		{
			return SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(preferenceRestriction);
		}
	}
}
