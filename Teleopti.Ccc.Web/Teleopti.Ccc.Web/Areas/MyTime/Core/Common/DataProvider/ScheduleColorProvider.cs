using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class ScheduleColorProvider : IScheduleColorProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public ScheduleColorProvider(ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}
		
		public IEnumerable<Color> GetColors(IScheduleColorSource source)
		{
			if (source == null)
				return new Color[] {};
			var scheduleDays = source.ScheduleDays ?? new IScheduleDay[] {};
			var projections = source.Projections ?? new IVisualLayerCollection[] { };
			var preferenceDays = source.PreferenceDays ?? new IPreferenceDay[] { };
			var allowedShiftCategories = source.WorkflowControlSet == null ? new IShiftCategory[] { } : source.WorkflowControlSet.AllowedPreferenceShiftCategories;
			var allowedAbsence = source.WorkflowControlSet == null ? new IAbsence[] { } : source.WorkflowControlSet.AllowedPreferenceAbsences;
			var allowedDayOffTemplates = source.WorkflowControlSet == null ? new IDayOffTemplate[] {} : source.WorkflowControlSet.AllowedPreferenceDayOffs;

			var layerColors = from p in projections
							  let layers = p as IEnumerable<IVisualLayer>
							  from l in layers
							  select l.Payload.ConfidentialDisplayColor_DONTUSE(_loggedOnUser.CurrentUser()); //probably not needed because always looking at its own schedule

			var assignments = scheduleDays.Select(x => x.PersonAssignment()).Where(x => x != null);
			
			var personAssignmentColors =
				(from assignment in assignments
				 where assignment.ShiftCategory != null
				 select assignment.ShiftCategory.DisplayColor)
					.ToList();

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

			var allowedShiftCategoryColors = from s in allowedShiftCategories select s.DisplayColor;
			var allowedAbsenceColors = from a in allowedAbsence select a.DisplayColor;
			var allowedDayOffTemplatesColors = from d in allowedDayOffTemplates select d.DisplayColor;

			return
				layerColors
					.Union(personAssignmentColors)
					.Union(absenceColors)
					.Union(preferenceColors)
					.Union(allowedShiftCategoryColors)
					.Union(allowedAbsenceColors)
					.Union(allowedDayOffTemplatesColors)
					.ToArray();
		}
	}
}