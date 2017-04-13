using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling.LockMenuBuilders
{
	public class LockAbsencesMenuBuilder
	{
		public void Build(IEnumerable<IAbsence> absences, EventHandler toolStripMenuItemLockAbsenceDaysClick,
		                  MouseEventHandler toolStripMenuItemLockAbsenceDaysMouseUp,
		                  ToolStripMenuItem toolStripMenuItemLockAbsence, ToolStripMenuItem toolStripMenuItemLockAbsencesRm,
		                  EventHandler toolStripMenuItemLockAbsencesClick,
		                  MouseEventHandler toolStripMenuItemAbsenceLockRmMouseUp)
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
				toolStripMenuItemAbsenceLockRibbon.Click += toolStripMenuItemLockAbsenceDaysClick;
				toolStripMenuItemAbsenceLockRm.MouseUp += toolStripMenuItemLockAbsenceDaysMouseUp;
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

				toolStripMenuItemAbsenceLockRibbon.Click += toolStripMenuItemLockAbsencesClick;
				toolStripMenuItemAbsenceLockRm.MouseUp += toolStripMenuItemAbsenceLockRmMouseUp;

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
					toolStripMenuItemAbsenceLockRibbon.Click += toolStripMenuItemLockAbsencesClick;
					toolStripMenuItemAbsenceLockRm.MouseUp += toolStripMenuItemAbsenceLockRmMouseUp;
					toolStripMenuItemDeletedAbsenceLockRibbon.DropDownItems.Add(toolStripMenuItemAbsenceLockRibbon);
					toolStripMenuItemDeletedAbsenceLockRm.DropDownItems.Add(toolStripMenuItemAbsenceLockRm);
				}
			}
		}
	}
}