using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.Win.Common.Controls.Filters
{
    public partial class FilterBox : BaseRibbonForm
    {
        public event EventHandler<FilterBoxEventArgs> FilterClicked;
        private IList<FilterItem> _items;
        public FilterBox()
        {
            InitializeComponent();
            _items = new List<FilterItem>();
            SetTexts();
            SetColor();
            Load += new EventHandler(FilterBox_Load);
        }

        private void SetColor()
        {
            this.splitContainerAdv1.Panel1.BackgroundColor = ColorHelper.ControlGradientPanelBrush();
            this.splitContainerAdv1.Panel2.BackgroundColor = ColorHelper.ControlGradientPanelBrush();
        }

        void FilterBox_Load(object sender, EventArgs e)
        {
            foreach (var item in _items)
            {
                var box = new FilterBoxItem
                              {
                                  HeaderText = item.Text,
                                  HeaderTag = item.ValueItem,
                                  Alternatives = item.Alternatives
                              };
                // this.splitContainerAdv1.Panel1.Controls.Add(box);
                this.flowLayoutPanel1.Controls.Add(box);
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal IList<FilterItem> Items
        {
        set { _items = value; }
        }

        private void buttonAdvOk_Click(object sender, EventArgs e)
        {
            InvokeFilterClicked();
            this.Close();
        }

        private void buttonAdvCancel_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void buttonAdvApply_Click(object sender, EventArgs e)
        {
            InvokeFilterClicked();
        }

        private void InvokeFilterClicked()
        {
        	var handler = FilterClicked;
            if (handler != null)
            {
                handler.Invoke(this, CollectFilterBoxArgs());
            }
        }

        private FilterBoxEventArgs CollectFilterBoxArgs()
        {
            var filterE = new FilterBoxEventArgs();
            foreach (var item in flowLayoutPanel1.Controls.OfType<FilterBoxItem >())
            {
                filterE.FilterItems.Add(item.GetFilterItem());             
            }
            return filterE;
        }
    }



    public class FilterBoxEventArgs : EventArgs
    {
        private IList< FilterItem>  _filterItem;

        public FilterBoxEventArgs()
        {
            _filterItem =  new List<FilterItem>();
        }

        internal IList< FilterItem> FilterItems
        {
            get { return _filterItem; }
        }
    }
}
