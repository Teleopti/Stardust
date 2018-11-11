using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.LockMenuBuilders
{
	public class LockShiftCategoriesMenuBuilder
	{
		public void Build(IEnumerable<IShiftCategory> shiftCategories,
		                  ToolStripMenuItem toolStripMenuItemLockShiftCategory,
		                  ToolStripMenuItem toolStripMenuItemLockShiftCategoriesRm,
						UserLockHelper userShiftCategoryLockHelper)
		{
			var toolStripMenuItemShiftCategoryLockRibbon = new ToolStripMenuItem();
			var toolStripMenuItemShiftCategoryLockRm = new ToolStripMenuItem();
			var toolStripMenuItemDeletedShiftCategoryLockRibbon = new ToolStripMenuItem();
			var toolStripMenuItemDeletedShiftCategoryLockRm = new ToolStripMenuItem();
			var sortedCategories = (from c in shiftCategories
			                       orderby c.Description.ShortName, c.Description.Name
			                       select c).ToList();
			if (sortedCategories.Any())
			{
				toolStripMenuItemShiftCategoryLockRibbon.Text = Resources.All;
				toolStripMenuItemShiftCategoryLockRm.Text = Resources.All;
				toolStripMenuItemShiftCategoryLockRibbon.Click += userShiftCategoryLockHelper.ToolStripMenuItemLockShiftCategoryDaysClick;
				toolStripMenuItemShiftCategoryLockRm.MouseUp += userShiftCategoryLockHelper.ToolStripMenuItemLockShiftCategoryDaysMouseUp;
				toolStripMenuItemLockShiftCategory.DropDownItems.Add(toolStripMenuItemShiftCategoryLockRibbon);
				toolStripMenuItemLockShiftCategoriesRm.DropDownItems.Add(toolStripMenuItemShiftCategoryLockRm);
			}
			foreach (IShiftCategory shiftCategory in sortedCategories)
			{
				if (((IDeleteTag) shiftCategory).IsDeleted)
					continue;
				toolStripMenuItemShiftCategoryLockRibbon = new ToolStripMenuItem();
				toolStripMenuItemShiftCategoryLockRm = new ToolStripMenuItem();
				toolStripMenuItemShiftCategoryLockRibbon.Text = shiftCategory.Description.ToString();
				toolStripMenuItemShiftCategoryLockRm.Text = shiftCategory.Description.ToString();
				toolStripMenuItemShiftCategoryLockRibbon.Tag = shiftCategory;
				toolStripMenuItemShiftCategoryLockRm.Tag = shiftCategory;
				toolStripMenuItemShiftCategoryLockRibbon.Click += userShiftCategoryLockHelper.ToolStripMenuItemLockShiftCategoriesClick;
				toolStripMenuItemShiftCategoryLockRm.MouseUp += userShiftCategoryLockHelper.ToolStripMenuItemLockShiftCategoriesMouseUp;
				toolStripMenuItemLockShiftCategory.DropDownItems.Add(toolStripMenuItemShiftCategoryLockRibbon);
				toolStripMenuItemLockShiftCategoriesRm.DropDownItems.Add(toolStripMenuItemShiftCategoryLockRm);
			}
			var deleted = (from a in sortedCategories
			              where ((IDeleteTag) a).IsDeleted
			              select a).ToList();
			if (deleted.Any())
			{
				toolStripMenuItemDeletedShiftCategoryLockRibbon.Text = Resources.Deleted;
				toolStripMenuItemDeletedShiftCategoryLockRm.Text = Resources.Deleted;
				toolStripMenuItemLockShiftCategory.DropDownItems.Add(toolStripMenuItemDeletedShiftCategoryLockRibbon);
				toolStripMenuItemLockShiftCategoriesRm.DropDownItems.Add(toolStripMenuItemDeletedShiftCategoryLockRm);

				foreach (IShiftCategory category in deleted)
				{
					toolStripMenuItemShiftCategoryLockRibbon = new ToolStripMenuItem();
					toolStripMenuItemShiftCategoryLockRm = new ToolStripMenuItem();
					toolStripMenuItemShiftCategoryLockRibbon.Text = category.Description.ToString();
					toolStripMenuItemShiftCategoryLockRm.Text = category.Description.ToString();
					toolStripMenuItemShiftCategoryLockRibbon.Tag = category;
					toolStripMenuItemShiftCategoryLockRm.Tag = category;
					toolStripMenuItemShiftCategoryLockRibbon.Click += userShiftCategoryLockHelper.ToolStripMenuItemLockShiftCategoriesClick;
					toolStripMenuItemShiftCategoryLockRm.MouseUp += userShiftCategoryLockHelper.ToolStripMenuItemLockShiftCategoriesMouseUp;
					toolStripMenuItemDeletedShiftCategoryLockRibbon.DropDownItems.Add(toolStripMenuItemShiftCategoryLockRibbon);
					toolStripMenuItemDeletedShiftCategoryLockRm.DropDownItems.Add(toolStripMenuItemShiftCategoryLockRm);

				}
			}
		}
	}
}