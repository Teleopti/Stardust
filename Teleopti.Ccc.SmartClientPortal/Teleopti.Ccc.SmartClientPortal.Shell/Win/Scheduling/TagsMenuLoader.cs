using System;
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
        private readonly ToolStripSplitButton _toolStripSetTagOnRibbonButton;
        private readonly ToolStripComboBox _toolStripAutoTagOnRibbonComboBox;
        private readonly IEnumerable<IScheduleTag> _tags;
        private readonly WeakReference<MouseEventHandler> _eventHandler;
        private readonly WeakReference<MouseEventHandler> _eventHandlerChangeTag;
        private readonly IScheduleTag _autoTag;

        public TagsMenuLoader(ToolStripMenuItem toolStripMenuItemRibbon, ToolStripMenuItem toolStripMenuItemGrid, IEnumerable<IScheduleTag> tags, 
            MouseEventHandler eventHandler, ToolStripSplitButton setTagOnRibbonButton, MouseEventHandler eventHandlerChangeTag, ToolStripComboBox toolStripAutoTagOnRibbonComboBox, IScheduleTag autoTag, ToolStripMenuItem toolStripMenuItemSetTagOnContextMenu)
        {
            if(toolStripMenuItemRibbon == null) throw new ArgumentNullException(nameof(toolStripMenuItemRibbon));
            if(toolStripMenuItemGrid == null) throw new ArgumentNullException(nameof(toolStripMenuItemGrid));
            if (toolStripMenuItemSetTagOnContextMenu == null) throw new ArgumentNullException(nameof(toolStripMenuItemSetTagOnContextMenu));
            if(setTagOnRibbonButton == null) throw new ArgumentNullException(nameof(setTagOnRibbonButton));
            if(tags == null) throw new ArgumentNullException(nameof(tags));
            if(eventHandler == null) throw new ArgumentNullException(nameof(eventHandler));
            if (eventHandlerChangeTag == null) throw new ArgumentNullException(nameof(eventHandlerChangeTag));
            if(toolStripAutoTagOnRibbonComboBox == null) throw new ArgumentNullException(nameof(toolStripAutoTagOnRibbonComboBox));
            if(autoTag == null) throw new ArgumentNullException(nameof(autoTag));

            _toolStripMenuItemRibbon = toolStripMenuItemRibbon;
            _toolStripMenuItemGrid = toolStripMenuItemGrid;
            _toolStripMenuItemSetTagOnContextMenu = toolStripMenuItemSetTagOnContextMenu;
            _toolStripSetTagOnRibbonButton = setTagOnRibbonButton;
            _toolStripAutoTagOnRibbonComboBox = toolStripAutoTagOnRibbonComboBox;
            _autoTag = autoTag;
            _tags = tags;
            _eventHandler = new WeakReference<MouseEventHandler>(eventHandler);
            _eventHandlerChangeTag = new WeakReference<MouseEventHandler>(eventHandlerChangeTag);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public void LoadTags()
        {
            foreach (var tag in _tags)
            {
                if (tag.IsDeleted) continue;

				var toolStripMenuItemRibbonTag = createToolStripMenuItemAndRegisterTagToIt(tag, _eventHandler);
				var toolStripMenuItemGridTag = createToolStripMenuItemAndRegisterTagToIt(tag, _eventHandler);
				var toolStripMenuItemOnContextMenu = createToolStripMenuItemAndRegisterTagToIt(tag, _eventHandlerChangeTag);
				var toolStripMenuItemSetTagOmRibbon = createToolStripMenuItemAndRegisterTagToIt(tag, _eventHandlerChangeTag);

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

		private static ToolStripMenuItem createToolStripMenuItemAndRegisterTagToIt(IScheduleTag tag, WeakReference<MouseEventHandler> mouseEventHandler)
	    {
			var toolStripMenuItem = new ToolStripMenuItem();
			registerTagToToolStripMenuItem(toolStripMenuItem, tag, mouseEventHandler);
			return toolStripMenuItem;
	    }

		private static void registerTagToToolStripMenuItem(ToolStripMenuItem toolStripMenuItem, IScheduleTag tag, WeakReference<MouseEventHandler> mouseEventHandler)
		{
			toolStripMenuItem.Text = tag.Description;
			toolStripMenuItem.Tag = tag;
			if(mouseEventHandler.TryGetTarget(out var target))
				toolStripMenuItem.MouseUp += target;
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

	            if (_eventHandler.TryGetTarget(out var target))
	            {
		            toolStripMenuItemRibbonTag.MouseUp += target;
		            toolStripMenuItemGridTag.MouseUp += target;
	            }

	            toolStripMenuItemRibbonDeletedTag.DropDownItems.Add(toolStripMenuItemRibbonTag);
                toolStripMenuItemGridDeletedTag.DropDownItems.Add(toolStripMenuItemGridTag);
            }    
        }
    }
}
