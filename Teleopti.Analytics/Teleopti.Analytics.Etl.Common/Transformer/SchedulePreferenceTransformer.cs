using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;


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
					newDataRow = FillDataRow(newDataRow, preferenceRestriction, schedulePart);
					table.Rows.Add(newDataRow);
				}
			}
		}

		public bool CheckIfPreferenceIsValid(IPreferenceRestriction preferenceRestriction)
		{
			return SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(preferenceRestriction);
		}

		public static DataRow FillDataRow(DataRow dataRow, IPreferenceRestriction preferenceRestriction,
													 IScheduleDay schedulePart)
		{
			dataRow["person_restriction_code"] = preferenceRestriction.Id;
			var preferenceDay = (IPreferenceDay)preferenceRestriction.Parent;

			var restrictionDate = preferenceDay.RestrictionDate;
			dataRow["restriction_date"] = restrictionDate.Date;
			dataRow["person_code"] = schedulePart.Person.Id;
			dataRow["scenario_code"] = schedulePart.Scenario.Id;

			if (preferenceRestriction.ShiftCategory != null)
				dataRow["shift_category_code"] = preferenceRestriction.ShiftCategory.Id;

			if (preferenceRestriction.DayOffTemplate != null)
			{
				dataRow["day_off_code"] = preferenceRestriction.DayOffTemplate.Id;
				dataRow["day_off_name"] = preferenceRestriction.DayOffTemplate.Description.Name;
				dataRow["day_off_shortname"] = preferenceRestriction.DayOffTemplate.Description.ShortName;
			}

			dataRow["preference_type_id"] = SchedulePreferenceTransformerHelper.GetPreferenceTypeId(preferenceRestriction);

			// ReSharper disable PossibleInvalidOperationException
			if (preferenceRestriction.ActivityRestrictionCollection.Count > 0)
				dataRow["activity_code"] = preferenceRestriction.ActivityRestrictionCollection[0].Activity.Id;
			// ReSharper restore PossibleInvalidOperationException

			dataRow["must_have"] = preferenceRestriction.MustHave ? 1 : 0;

			if (preferenceRestriction.Absence != null)
				dataRow["absence_code"] = preferenceRestriction.Absence.Id;
			else
				dataRow["absence_code"] = DBNull.Value;

			var restrictionChecker = new RestrictionChecker();
			var permissionState = restrictionChecker.CheckPreference(schedulePart);
			if (permissionState == PermissionState.Satisfied)
			{
				dataRow["preference_fulfilled"] = 1;
				dataRow["preference_unfulfilled"] = 0;
			}

			else
			{
				dataRow["preference_fulfilled"] = 0;
				dataRow["preference_unfulfilled"] = 1;
			}

			dataRow["business_unit_code"] = schedulePart.Scenario.GetOrFillWithBusinessUnit_DONTUSE().Id;
			return dataRow;
		}
	}
}
