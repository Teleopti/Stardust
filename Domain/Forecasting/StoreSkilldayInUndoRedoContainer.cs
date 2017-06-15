using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public static class StoreSkilldayInUndoRedoContainer
	{
		public static void FillWith(this IUndoRedoContainer undoRedoContainer, IEnumerable<ISkillDay> skillDays)
		{
			foreach (var skillDay in skillDays)
			{
				skillDay.SkillStaffPeriodCollection.ForEach(x => undoRedoContainer.SaveState(x.Payload));
			}
		}
	}
}