using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class ScheduleColorProvider : IScheduleColorProvider
	{
		public IEnumerable<Color> GetColors(IEnumerable<IScheduleColorSource> source)
		{
			var layerColors = from s in source
			                  where s.Projection != null
			                  let layers = s.Projection as IEnumerable<IVisualLayer>
			                  from l in layers
			                  select l.DisplayColor();

			var personAssignmentColors = from s in source
			                          where s.ScheduleDay != null
			                          let personAssignment = s.ScheduleDay.AssignmentHighZOrder()
			                          where personAssignment != null
			                          select personAssignment.MainShift.ShiftCategory.DisplayColor;

			var absenceColors = from s in source
			                    where s.ScheduleDay != null
			                    let personAbsences = s.ScheduleDay.PersonAbsenceCollection()
			                    where personAbsences != null
			                    from pa in personAbsences
			                    select pa.Layer.Payload.DisplayColor;

			var preferenceColors = from s in source
			                       where
			                       	s.PreferenceDay != null &&
			                       	s.PreferenceDay.Restriction != null
			                       let shiftCategory = s.PreferenceDay.Restriction.ShiftCategory
			                       let absence = s.PreferenceDay.Restriction.Absence
								   let dayOff = s.PreferenceDay.Restriction.DayOffTemplate
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