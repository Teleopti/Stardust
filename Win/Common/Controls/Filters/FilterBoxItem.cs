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
    public partial class FilterBoxItem : UserControl
    {
        private string headerText;
        private object headerTag;
        private IList<TupleItem> alternatives;

        public FilterBoxItem()
        {
            InitializeComponent();
            Load += new EventHandler(FilterBoxItem_Load);


        }

        void FilterBoxItem_Load(object sender, EventArgs e)
        {
            this.label1.Text = HeaderText;
            foreach (var item in alternatives)
            {
                var controlitem = new FilterBoxListItem();
                controlitem.ListItemText = item.Text;
                controlitem.ValueItem = item.ValueMember;
                controlitem.SetCheckBox();
                
                flowLayoutPanel1.Controls.Add(controlitem );
            }
        }


        public string HeaderText
        {
            get { return headerText; }
            set { headerText = value; }
        }

        public object HeaderTag
        {
            get { return headerTag; }
            set { headerTag = value; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<TupleItem> Alternatives
        {
            get { return alternatives; }
            set { alternatives = value; }
        }

        internal FilterItem GetFilterItem()
        {
            var list = new List<TupleItem>();                   
            foreach (var item in this.flowLayoutPanel1.Controls.OfType<FilterBoxListItem>())
            {
                var ListItem = new TupleItem(item.CheckBoxValue , item.CheckBoxTag);
                list.Add(ListItem);
            }
            var filteritem = new FilterItem(headerTag, headerText, list);
            return filteritem;
        }
    }

}
