﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
    public class TagsMenuLoader
    {
        private readonly ToolStripMenuItem _toolStripMenuItemRibbon;
        private readonly ToolStripMenuItem _toolStripMenuItemGrid;
        private readonly ToolStripMenuItem _toolStripMenuItemSetTagOnContextMenu;
        private readonly ToolStripSplitButton _toolStripSetTagOnRibbonButton;
        private readonly ToolStripComboBox _toolStripAutoTagOnRibbonComboBox;
        private readonly IList<IScheduleTag> _tags;
        private readonly MouseEventHandler _eventHandler;
        private readonly MouseEventHandler _eventHandlerChangeTag;
        private readonly IScheduleTag _autoTag;

        public TagsMenuLoader(ToolStripMenuItem toolStripMenuItemRibbon, ToolStripMenuItem toolStripMenuItemGrid, IList<IScheduleTag> tags, 
            MouseEventHandler eventHandler, ToolStripSplitButton setTagOnRibbonButton, MouseEventHandler eventHandlerChangeTag, ToolStripComboBox toolStripAutoTagOnRibbonComboBox, IScheduleTag autoTag, ToolStripMenuItem toolStripMenuItemSetTagOnContextMenu)
        {
            if(toolStripMenuItemRibbon == null) throw new ArgumentNullException("toolStripMenuItemRibbon");
            if(toolStripMenuItemGrid == null) throw new ArgumentNullException("toolStripMenuItemGrid");
            if (toolStripMenuItemSetTagOnContextMenu == null) throw new ArgumentNullException("toolStripMenuItemSetTagOnContextMenu");
            if(setTagOnRibbonButton == null) throw new ArgumentNullException("setTagOnRibbonButton");
            if(tags == null) throw new ArgumentNullException("tags");
            if(eventHandler == null) throw new ArgumentNullException("eventHandler");
            if (eventHandlerChangeTag == null) throw new ArgumentNullException("eventHandlerChangeTag");
            if(toolStripAutoTagOnRibbonComboBox == null) throw new ArgumentNullException("toolStripAutoTagOnRibbonComboBox");
            if(autoTag == null) throw new ArgumentNullException("autoTag");

            _toolStripMenuItemRibbon = toolStripMenuItemRibbon;
            _toolStripMenuItemGrid = toolStripMenuItemGrid;
            _toolStripMenuItemSetTagOnContextMenu = toolStripMenuItemSetTagOnContextMenu;
            _toolStripSetTagOnRibbonButton = setTagOnRibbonButton;
            _toolStripAutoTagOnRibbonComboBox = toolStripAutoTagOnRibbonComboBox;
            _autoTag = autoTag;
            _tags = tags;
            _eventHandler = eventHandler;
            _eventHandlerChangeTag = eventHandlerChangeTag;
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

		private static ToolStripMenuItem createToolStripMenuItemAndRegisterTagToIt(IScheduleTag tag, MouseEventHandler mouseEventHandler)
	    {
			var toolStripMenuItem = new ToolStripMenuItem();
			registerTagToToolStripMenuItem(toolStripMenuItem, tag, mouseEventHandler);
			return toolStripMenuItem;
	    }

		private static void registerTagToToolStripMenuItem(ToolStripMenuItem toolStripMenuItem, IScheduleTag tag, MouseEventHandler mouseEventHandler)
		{
			toolStripMenuItem.Text = tag.Description;
			toolStripMenuItem.Tag = tag;
			if(mouseEventHandler != null)
				toolStripMenuItem.MouseUp += mouseEventHandler;
		}

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void loadDeletedTags()
        {
            var deleted = from t in _tags
                          where t.IsDeleted
                          select t;

            if (deleted.Count() <= 0) return;

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

                toolStripMenuItemRibbonTag.MouseUp += _eventHandler;
                toolStripMenuItemGridTag.MouseUp += _eventHandler;

                toolStripMenuItemRibbonDeletedTag.DropDownItems.Add(toolStripMenuItemRibbonTag);
                toolStripMenuItemGridDeletedTag.DropDownItems.Add(toolStripMenuItemGridTag);
            }    
        }
    }
}
