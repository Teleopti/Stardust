using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling
{
    public class TagsMenuLoader
    {
        private readonly ToolStripMenuItem _toolStripMenuItemRibbon;
        private readonly ToolStripMenuItem _toolStripMenuItemGrid;
        private readonly ToolStripMenuItem _toolStripMenuItemEditGrid;
        private readonly ToolStripSplitButton _toolStripSplitButton;
        private readonly ToolStripComboBox _toolStripComboBox;
        private readonly IList<IScheduleTag> _tags;
        private readonly MouseEventHandler _eventHandler;
        private readonly MouseEventHandler _eventHandlerChangeTag;
        private readonly IScheduleTag _autoTag;

        public TagsMenuLoader(ToolStripMenuItem toolStripMenuItemRibbon, ToolStripMenuItem toolStripMenuItemGrid, IList<IScheduleTag> tags, 
            MouseEventHandler eventHandler, ToolStripSplitButton splitButton, MouseEventHandler eventHandlerChangeTag, ToolStripComboBox toolStripComboBox, IScheduleTag autoTag, ToolStripMenuItem toolStripMenuItemEditGrid)
        {
            if(toolStripMenuItemRibbon == null) throw new ArgumentNullException("toolStripMenuItemRibbon");
            if(toolStripMenuItemGrid == null) throw new ArgumentNullException("toolStripMenuItemGrid");
            if (toolStripMenuItemEditGrid == null) throw new ArgumentNullException("toolStripMenuItemEditGrid");
            if(splitButton == null) throw new ArgumentNullException("splitButton");
            if(tags == null) throw new ArgumentNullException("tags");
            if(eventHandler == null) throw new ArgumentNullException("eventHandler");
            if (eventHandlerChangeTag == null) throw new ArgumentNullException("eventHandlerChangeTag");
            if(toolStripComboBox == null) throw new ArgumentNullException("toolStripComboBox");
            if(autoTag == null) throw new ArgumentNullException("autoTag");

            _toolStripMenuItemRibbon = toolStripMenuItemRibbon;
            _toolStripMenuItemGrid = toolStripMenuItemGrid;
            _toolStripMenuItemEditGrid = toolStripMenuItemEditGrid;
            _toolStripSplitButton = splitButton;
            _toolStripComboBox = toolStripComboBox;
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

                var toolStripMenuItemRibbonTag = new ToolStripMenuItem();
                var toolStripMenuItemGridTag = new ToolStripMenuItem();
                var toolStripMenuItemEditGridTag = new ToolStripMenuItem();
                var toolStripMenuItemEditRibbon = new ToolStripMenuItem();

                toolStripMenuItemRibbonTag.Text = tag.Description;
                toolStripMenuItemGridTag.Text = tag.Description;
                toolStripMenuItemEditGridTag.Text = tag.Description;
                toolStripMenuItemEditRibbon.Text = tag.Description;

                toolStripMenuItemRibbonTag.Tag = tag;
                toolStripMenuItemGridTag.Tag = tag;
                toolStripMenuItemEditGridTag.Tag = tag;
                toolStripMenuItemEditRibbon.Tag = tag;

                toolStripMenuItemRibbonTag.MouseUp += _eventHandler;
                toolStripMenuItemGridTag.MouseUp += _eventHandler;
                toolStripMenuItemEditGridTag.MouseUp += _eventHandlerChangeTag;
                toolStripMenuItemEditRibbon.MouseUp += _eventHandlerChangeTag;
                

                _toolStripMenuItemRibbon.DropDownItems.Add(toolStripMenuItemRibbonTag);
                _toolStripMenuItemGrid.DropDownItems.Add(toolStripMenuItemGridTag);
                _toolStripMenuItemEditGrid.DropDownItems.Add(toolStripMenuItemEditGridTag);
                _toolStripSplitButton.DropDownItems.Add(toolStripMenuItemEditRibbon);
                _toolStripComboBox.Items.Add(tag);
            }

            if (_toolStripComboBox.ComboBox != null && _toolStripComboBox.Items.Count > 0)
            {
                _toolStripComboBox.ComboBox.DisplayMember = "Description";
                _toolStripComboBox.SelectedItem = _autoTag;
            }

            LoadDeletedTags(); 
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void LoadDeletedTags()
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
