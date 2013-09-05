using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Teleopti.Ccc.Win.Common.Controls.Filters
{
    public partial class FilterBoxListItem : UserControl
    {
        private string listItemText;
        private object valueItem;

        public FilterBoxListItem()
        {
            InitializeComponent();
            Load += new EventHandler(FilterBoxListItem_Load);
        }

        void FilterBoxListItem_Load(object sender, EventArgs e)
        {
            this.checkBox1.Text = listItemText;
            this.checkBox1.Tag = valueItem;
        }

        public string ListItemText
        {
            get { return listItemText; }
            set { listItemText = value; }
        }

        public object ValueItem
        {
            get { return valueItem; }
            set { valueItem = value; }
        }


        public string CheckBoxValue
        {
            get { return checkBox1.Checked.ToString(); }
        }


        public object CheckBoxTag
        {
            get { return checkBox1.Tag; }
        }

        public void SetCheckBox()
        {
            this.checkBox1.Checked = true;
        }
    }
}
