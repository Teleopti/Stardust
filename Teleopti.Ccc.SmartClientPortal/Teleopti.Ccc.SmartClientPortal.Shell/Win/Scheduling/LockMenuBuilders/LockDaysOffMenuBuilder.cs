using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.LockMenuBuilders
{
	public class LockDaysOffMenuBuilder
	{
		public void Build(IEnumerable<IDayOffTemplate> dayOffList, EventHandler clickEvent, EventHandler otherClickEvent, MouseEventHandler mouseUpEvent, ToolStripMenuItem toolStripMenuItemLockDayOff, ToolStripMenuItem toolStripMenuItemLockFreeDaysRm)
		{
			IList<Description> descriptions = new List<Description>();
			ToolStripMenuItem toolStripMenuItemDayOffLockRibbon;
			ToolStripMenuItem toolStripMenuItemDayOffLockRm;

			IList<IDayOffTemplate> displayList = (from item in dayOffList
												  orderby item.Description.ShortName, item.Description.Name
												  select item).ToList();
			if (displayList.Count > 0)
			{
				toolStripMenuItemDayOffLockRibbon = new ToolStripMenuItem();
				toolStripMenuItemDayOffLockRm = new ToolStripMenuItem();
				toolStripMenuItemDayOffLockRibbon.Text = Resources.All;
				toolStripMenuItemDayOffLockRm.Text = Resources.All;
				toolStripMenuItemDayOffLockRibbon.Click += clickEvent;
				toolStripMenuItemDayOffLockRm.MouseUp += mouseUpEvent;
				toolStripMenuItemLockDayOff.DropDownItems.Add(toolStripMenuItemDayOffLockRibbon);
				toolStripMenuItemLockFreeDaysRm.DropDownItems.Add(toolStripMenuItemDayOffLockRm);
			}
			foreach (IDayOffTemplate dayOff in displayList)
			{
				if (((IDeleteTag)dayOff).IsDeleted)
					continue;
				if (descriptions.Count > 0)
				{
					if (descriptions.Contains(dayOff.Description))
						continue;
				}
				toolStripMenuItemDayOffLockRibbon = new ToolStripMenuItem();
				toolStripMenuItemDayOffLockRm = new ToolStripMenuItem();
				toolStripMenuItemDayOffLockRibbon.Text = dayOff.Description.ToString();
				toolStripMenuItemDayOffLockRm.Text = dayOff.Description.ToString();
				toolStripMenuItemDayOffLockRibbon.Tag = dayOff;
				toolStripMenuItemDayOffLockRm.Tag = dayOff;
				toolStripMenuItemDayOffLockRibbon.Click += otherClickEvent;
				toolStripMenuItemDayOffLockRm.Click += otherClickEvent;
				toolStripMenuItemLockDayOff.DropDownItems.Add(toolStripMenuItemDayOffLockRibbon);
				toolStripMenuItemLockFreeDaysRm.DropDownItems.Add(toolStripMenuItemDayOffLockRm);
				descriptions.Add(dayOff.Description);
			}
			var deleted = (from a in displayList
						  where ((IDeleteTag)a).IsDeleted
						  select a).ToList();
			if (deleted.Any())
			{
				var toolStripMenuItemDeletedDayOffLockRibbon = new ToolStripMenuItem();
				var toolStripMenuDeletedItemDayOffLockRm = new ToolStripMenuItem();
				toolStripMenuItemDeletedDayOffLockRibbon.Text = Resources.Deleted;
				toolStripMenuDeletedItemDayOffLockRm.Text = Resources.Deleted;
				toolStripMenuItemLockDayOff.DropDownItems.Add(toolStripMenuItemDeletedDayOffLockRibbon);
				toolStripMenuItemLockFreeDaysRm.DropDownItems.Add(toolStripMenuDeletedItemDayOffLockRm);

				foreach (IDayOffTemplate dayOff in deleted)
				{
					if (descriptions.Count > 0)
					{
						if (descriptions.Contains(dayOff.Description))
							continue;
					}

					toolStripMenuItemDayOffLockRibbon = new ToolStripMenuItem();
					toolStripMenuItemDayOffLockRm = new ToolStripMenuItem();
					toolStripMenuItemDayOffLockRibbon.Text = dayOff.Description.ToString();
					toolStripMenuItemDayOffLockRm.Text = dayOff.Description.ToString();
					toolStripMenuItemDayOffLockRibbon.Tag = dayOff;
					toolStripMenuItemDayOffLockRm.Tag = dayOff;
					toolStripMenuItemDayOffLockRibbon.Click += clickEvent;
					toolStripMenuItemDayOffLockRm.Click += otherClickEvent;
					toolStripMenuItemDeletedDayOffLockRibbon.DropDownItems.Add(toolStripMenuItemDayOffLockRibbon);
					toolStripMenuDeletedItemDayOffLockRm.DropDownItems.Add(toolStripMenuItemDayOffLockRm);
					descriptions.Add(dayOff.Description);
				}
			}
		}
	}
}