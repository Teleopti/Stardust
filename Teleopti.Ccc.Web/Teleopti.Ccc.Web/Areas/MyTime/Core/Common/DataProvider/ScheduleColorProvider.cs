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

			var shiftCategoryColors = from s in source
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

			return
				layerColors
					.Union(shiftCategoryColors)
					.Union(absenceColors)
					.ToArray();
		}
	}
}