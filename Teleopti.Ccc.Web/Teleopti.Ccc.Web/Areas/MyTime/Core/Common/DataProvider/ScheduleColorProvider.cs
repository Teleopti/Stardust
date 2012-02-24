using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class ScheduleColorProvider : IScheduleColorProvider
	{
		public IEnumerable<Color> GetColors(IScheduleColorSource source)
		{
			if (source == null)
				return new Color[] {};

			var scheduleDays = source.ScheduleDays ?? new IScheduleDay[] {};
			var projections = source.Projections ?? new IVisualLayerCollection[] { };
			var preferenceDays = source.PreferenceDays ?? new IPreferenceDay[] { };

			var layerColors = from p in projections
							  let layers = p as IEnumerable<IVisualLayer>
							  from l in layers
							  select l.DisplayColor();

			var personAssignmentColors = from d in scheduleDays
										 let personAssignment = d.AssignmentHighZOrder()
										 where personAssignment != null
										 select personAssignment.MainShift.ShiftCategory.DisplayColor;

			var absenceColors = from d in scheduleDays
			                    let personAbsences = d.PersonAbsenceCollection()
			                    where personAbsences != null
			                    from pa in personAbsences
			                    select pa.Layer.Payload.DisplayColor;

			var preferenceColors = from d in preferenceDays
			                       where d.Restriction != null
			                       let shiftCategory = d.Restriction.ShiftCategory
			                       let absence = d.Restriction.Absence
								   let dayOff = d.Restriction.DayOffTemplate
			                       let colors = new[]
			                                    	{
			                                    		shiftCategory == null ? Color.Transparent : shiftCategory.DisplayColor,
			                                    		absence == null ? Color.Transparent : absence.DisplayColor,
														dayOff == null ? Color.Transparent : dayOff.DisplayColor
			                                    	}.Where(c => c != Color.Transparent)
			                       from c in colors
			                       select c;

			return
				layerColors
					.Union(personAssignmentColors)
					.Union(absenceColors)
					.Union(preferenceColors)
					.ToArray();
		}
	}
}