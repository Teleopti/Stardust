using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.LockMenuBuilders
{
	public class LockAbsencesMenuBuilder
	{
		public void Build(IEnumerable<IAbsence> absences, ToolStripMenuItem toolStripMenuItemLockAbsence,
			ToolStripMenuItem toolStripMenuItemLockAbsencesRm, UserLockHelper userShiftCategoryLockHelper)
		{
			var toolStripMenuItemAbsenceLockRibbon = new ToolStripMenuItem();
			var toolStripMenuItemDeletedAbsenceLockRibbon = new ToolStripMenuItem();
			var toolStripMenuItemAbsenceLockRm = new ToolStripMenuItem();
			var toolStripMenuItemDeletedAbsenceLockRm = new ToolStripMenuItem();
			var sortedAbsences = (from a in absences
				orderby a.Description.ShortName, a.Description.Name
				select a).ToList();

			if (sortedAbsences.Any())
			{
				toolStripMenuItemAbsenceLockRibbon.Text = Resources.All;
				toolStripMenuItemAbsenceLockRm.Text = Resources.All;
				toolStripMenuItemAbsenceLockRibbon.Click +=
					userShiftCategoryLockHelper.ToolStripMenuItemLockAbsenceDaysClick;
				toolStripMenuItemAbsenceLockRm.MouseUp +=
					userShiftCategoryLockHelper.ToolStripMenuItemLockAbsenceDaysMouseUp;
				toolStripMenuItemLockAbsence.DropDownItems.Add(toolStripMenuItemAbsenceLockRibbon);
				toolStripMenuItemLockAbsencesRm.DropDownItems.Add(toolStripMenuItemAbsenceLockRm);
			}

			foreach (IAbsence abs in sortedAbsences)
			{
				if (((IDeleteTag) abs).IsDeleted)
					continue;
				toolStripMenuItemAbsenceLockRibbon = new ToolStripMenuItem();
				toolStripMenuItemAbsenceLockRm = new ToolStripMenuItem();

				toolStripMenuItemAbsenceLockRibbon.Text = abs.Description.ToString();
				toolStripMenuItemAbsenceLockRm.Text = abs.Description.ToString();

				toolStripMenuItemAbsenceLockRibbon.Tag = abs;
				toolStripMenuItemAbsenceLockRm.Tag = abs;

				toolStripMenuItemAbsenceLockRibbon.Click +=
					userShiftCategoryLockHelper.ToolStripMenuItemLockAbsencesClick;
				toolStripMenuItemAbsenceLockRm.MouseUp +=
					userShiftCategoryLockHelper.ToolStripMenuItemAbsenceLockRmMouseUp;

				toolStripMenuItemLockAbsence.DropDownItems.Add(toolStripMenuItemAbsenceLockRibbon);
				toolStripMenuItemLockAbsencesRm.DropDownItems.Add(toolStripMenuItemAbsenceLockRm);
			}

			var deleted = (from a in sortedAbsences
				where ((IDeleteTag) a).IsDeleted
				select a).ToList();
			if (deleted.Any())
			{
				toolStripMenuItemDeletedAbsenceLockRm.Text = Resources.Deleted;
				toolStripMenuItemDeletedAbsenceLockRibbon.Text = Resources.Deleted;
				toolStripMenuItemLockAbsence.DropDownItems.Add(toolStripMenuItemDeletedAbsenceLockRibbon);
				toolStripMenuItemLockAbsencesRm.DropDownItems.Add(toolStripMenuItemDeletedAbsenceLockRm);

				foreach (IAbsence abs in deleted)
				{
					toolStripMenuItemAbsenceLockRibbon = new ToolStripMenuItem();
					toolStripMenuItemAbsenceLockRm = new ToolStripMenuItem();
					toolStripMenuItemAbsenceLockRibbon.Text = abs.Description.ToString();
					toolStripMenuItemAbsenceLockRm.Text = abs.Description.ToString();
					toolStripMenuItemAbsenceLockRibbon.Tag = abs;
					toolStripMenuItemAbsenceLockRm.Tag = abs;
					toolStripMenuItemAbsenceLockRibbon.Click +=
						userShiftCategoryLockHelper.ToolStripMenuItemLockAbsencesClick;
					toolStripMenuItemAbsenceLockRm.MouseUp +=
						userShiftCategoryLockHelper.ToolStripMenuItemAbsenceLockRmMouseUp;
					toolStripMenuItemDeletedAbsenceLockRibbon.DropDownItems.Add(toolStripMenuItemAbsenceLockRibbon);
					toolStripMenuItemDeletedAbsenceLockRm.DropDownItems.Add(toolStripMenuItemAbsenceLockRm);
				}
			}
		}
	}
}