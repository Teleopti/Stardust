using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
    public class TagsMenuLoader
    {
        private readonly ToolStripMenuItem _toolStripMenuItemRibbon;
        private readonly ToolStripMenuItem _toolStripMenuItemGrid;
        private readonly ToolStripMenuItem _toolStripMenuItemSetTagOnContextMenu;
		private readonly UserLockHelper _userShiftCategoryLockHelper;
		private readonly ToolStripSplitButton _toolStripSetTagOnRibbonButton;
        private readonly ToolStripComboBox _toolStripAutoTagOnRibbonComboBox;
        private readonly IEnumerable<IScheduleTag> _tags;
        private readonly MouseEventHandler _eventHandlerChangeTag;
        private readonly IScheduleTag _autoTag;

        public TagsMenuLoader(ToolStripMenuItem toolStripMenuItemRibbon, ToolStripMenuItem toolStripMenuItemGrid, IEnumerable<IScheduleTag> tags, 
            ToolStripSplitButton setTagOnRibbonButton, MouseEventHandler eventHandlerChangeTag, ToolStripComboBox toolStripAutoTagOnRibbonComboBox, IScheduleTag autoTag, ToolStripMenuItem toolStripMenuItemSetTagOnContextMenu, UserLockHelper userShiftCategoryLockHelper)
        {
            _toolStripMenuItemRibbon = toolStripMenuItemRibbon;
            _toolStripMenuItemGrid = toolStripMenuItemGrid;
            _toolStripMenuItemSetTagOnContextMenu = toolStripMenuItemSetTagOnContextMenu;
			_userShiftCategoryLockHelper = userShiftCategoryLockHelper;
			_toolStripSetTagOnRibbonButton = setTagOnRibbonButton;
            _toolStripAutoTagOnRibbonComboBox = toolStripAutoTagOnRibbonComboBox;
            _autoTag = autoTag;
            _tags = tags;
            _eventHandlerChangeTag = eventHandlerChangeTag;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void LoadTags()
        {
            foreach (var tag in _tags)
            {
                if (tag.IsDeleted) continue;

				var toolStripMenuItemRibbonTag = createToolStripMenuItemAndRegisterTagToIt1(tag);
				var toolStripMenuItemGridTag = createToolStripMenuItemAndRegisterTagToIt1(tag);
				var toolStripMenuItemOnContextMenu = createToolStripMenuItemAndRegisterTagToIt2(tag, _eventHandlerChangeTag);
				var toolStripMenuItemSetTagOmRibbon = createToolStripMenuItemAndRegisterTagToIt2(tag, _eventHandlerChangeTag);

                _toolStripMenuItemRibbon.DropDownItems.Add(toolStripMenuItemRibbonTag);
                _toolStripMenuItemGrid.DropDownItems.Add(toolStripMenuItemGridTag);
				_toolStripMenuItemSetTagOnContextMenu.DropDownItems.Add(toolStripMenuItemOnContextMenu);
                _toolStripSetTagOnRibbonButton.DropDownItems.Add(toolStripMenuItemSetTagOmRibbon);
                _toolStripAutoTagOnRibbonComboBox.Items.Add(tag);
            }

			var keepOriginalTag = KeepOriginalScheduleTag.Instance;
			_toolStripAutoTagOnRibbonComboBox.Items.Insert(1, keepOriginalTag);
			
            if (_toolStripAutoTagOnRibbonComboBox.ComboBox != null)
            {
                _toolStripAutoTagOnRibbonComboBox.ComboBox.DisplayMember = "Description";
                _toolStripAutoTagOnRibbonComboBox.SelectedItem = _autoTag;
            }

            loadDeletedTags(); 
        }

		private ToolStripMenuItem createToolStripMenuItemAndRegisterTagToIt1(IScheduleTag tag)
	    {
			var toolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem.Text = tag.Description;
			toolStripMenuItem.Tag = tag;
			toolStripMenuItem.MouseUp += _userShiftCategoryLockHelper.ToolStripMenuItemLockTag;
			return toolStripMenuItem;
	    }
		private ToolStripMenuItem createToolStripMenuItemAndRegisterTagToIt2(IScheduleTag tag, MouseEventHandler handler)
		{
			var toolStripMenuItem = new ToolStripMenuItem();
			toolStripMenuItem.Text = tag.Description;
			toolStripMenuItem.Tag = tag;
			toolStripMenuItem.MouseUp += handler;
			return toolStripMenuItem;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void loadDeletedTags()
        {
            var deleted = _tags.Where (t => t.IsDeleted).ToArray();
            if (!deleted.Any()) return;

            var toolStripMenuItemRibbonDeletedTag = new ToolStripMenuItem();
            var toolStripMenuItemGridDeletedTag = new ToolStripMenuItem();

            toolStripMenuItemRibbonDeletedTag.Text = Resources.Deleted;
            toolStripMenuItemGridDeletedTag.Text = Resources.Deleted;
            _toolStripMenuItemRibbon.DropDownItems.Add(toolStripMenuItemRibbonDeletedTag);
            _toolStripMenuItemGrid.DropDownItems.Add(toolStripMenuItemGridDeletedTag);

            foreach (var tag in deleted)
            {
                var toolStripMenuItemRibbonTag = new ToolStripMenuItem();
                var toolStripMenuItemGridTag = new ToolStripMenuItem();

                toolStripMenuItemRibbonTag.Text = tag.Description;
                toolStripMenuItemGridTag.Text = tag.Description;

                toolStripMenuItemRibbonTag.Tag = tag;
                toolStripMenuItemGridTag.Tag = tag;

		            toolStripMenuItemRibbonTag.MouseUp += _userShiftCategoryLockHelper.ToolStripMenuItemLockTag;
		            toolStripMenuItemGridTag.MouseUp += _userShiftCategoryLockHelper.ToolStripMenuItemLockTag;

	            toolStripMenuItemRibbonDeletedTag.DropDownItems.Add(toolStripMenuItemRibbonTag);
                toolStripMenuItemGridDeletedTag.DropDownItems.Add(toolStripMenuItemGridTag);
            }    
        }
    }
}
