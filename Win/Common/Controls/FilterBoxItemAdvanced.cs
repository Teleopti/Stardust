using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Common;
using System.Security.Permissions;


namespace Teleopti.Ccc.Win.Common.Controls
{

    /// <summary>
    /// Item to be used in FilterBoxAdvanced
    /// </summary>
    public partial class FilterBoxItemAdvanced : BaseUserControl
    {
        private string preHeader;
        private IList<FilterAdvancedSetting> _columnList;
        public event EventHandler<EventArgs> RemoveMe;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public FilterBoxItemAdvanced(IList<FilterAdvancedSetting> columnList)
        {
            InitializeComponent();
            SetTexts();
            _columnList = new List<FilterAdvancedSetting>();
            Load += FilterBoxItemAdvancedLoad;
            timeSpanTextBox1.Visible = false;
            dateTimePickerAdvDate.Visible = false;
            office2007OutlookTimePicker1.Visible = false;
            comboBoxAdvCriteriaList.Visible = false;
            office2007OutlookTimePicker1.CreateAndBindList();
            timeSpanTextBox1.SetSize(85, textBoxExt1.Height);
            timeSpanTextBox1.Top = comboBoxAdvColumns.Top;
            _columnList = columnList;
        }

        /// <summary>
        /// Sets the combo box columns dropdown, column to filter on 
        /// </summary>
        /// <remarks>
        private void SetComboBoxColumns()
        {
            this.comboBoxAdvColumns.DisplayMember = "Text";
            this.comboBoxAdvColumns.ValueMember = "ValueObject";
            
            foreach (FilterAdvancedSetting item in _columnList)
            {
                this.comboBoxAdvColumns.Items.Add(item);
            }

            this.comboBoxAdvColumns.SelectedIndex = 0;
        }

        /// <summary>
        /// Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterBoxItemAdvancedLoad(object sender, EventArgs e)
        {
            this.labelPreHeader.Text = preHeader;
            if (_columnList.Count != 0)
                SetComboBoxColumns();
        }

        /// <summary>
        /// Header, Filter on, And Then On
        /// </summary>
        public string PreHeader
        {
            get { return preHeader; }
            set { preHeader = value; }
        }

        /// <summary>
        /// Return selected filters
        /// </summary>
        public FilterBoxAdvancedFilter SelectedFilterSetting()
        {
                FilterAdvancedSetting setting = ((FilterAdvancedSetting)this.comboBoxAdvColumns.SelectedItem);
                FilterAdvancedTupleItem filterOn = setting.FilterOn;
                FilterAdvancedTupleItem operand = (FilterAdvancedTupleItem)((Control)this.comboBoxAdvFilterOperand.SelectedItem).Tag;
                FilterAdvancedTupleItem criteria = null;
                int number;

                switch(setting.FilterCriteriaType)
                {
                    case FilterCriteriaType.Date: criteria = new FilterAdvancedTupleItem("Date", this.dateTimePickerAdvDate.Value); break; //criteria = this.dateTimePickerAdvDate.Value; break;
                    case FilterCriteriaType.HourMin : criteria = new FilterAdvancedTupleItem("HourMin",this.timeSpanTextBox1.Value); break; //criteria = this.timeSpanTextBox1.Value; break;
                    case FilterCriteriaType.List: criteria = new FilterAdvancedTupleItem("List", ((FilterAdvancedTupleItem)(((Control)this.comboBoxAdvCriteriaList.SelectedItem).Tag)).Value); break;//criteria = ((Control)this.comboBoxAdvCriteriaList.SelectedItem).Tag ; break;
                    case FilterCriteriaType.Text: criteria = new FilterAdvancedTupleItem("Text", this.textBoxExt1.Text); break; //criteria = this.textBoxExt1.Text; break;
                    case FilterCriteriaType.Time: criteria = new FilterAdvancedTupleItem("Time", this.office2007OutlookTimePicker1.TimeValue()); break; //criteria = this.office2007OutlookTimePicker1.TimeValue(); break;
                    case FilterCriteriaType.Number:
                        if (textBoxExt1.Text.Length == 0) textBoxExt1.Text = "0";
                        if (Int32.TryParse(textBoxExt1.Text, out number))
                            //criteria = number;
                            criteria = new FilterAdvancedTupleItem("Number", number);
                        else
                            throw new FormatException("The provided value could not be formatted to Int32");
                        break;
                }

                return new FilterBoxAdvancedFilter(filterOn, operand, criteria);
        }

        public FilterAdvancedSetting Setting
        {
            get { return ((FilterAdvancedSetting)this.comboBoxAdvColumns.SelectedItem); }
        }

        /// <summary>
        /// Remove a filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonRemoveClick(object sender, EventArgs e)
        {
        	var handler = RemoveMe;
            if (handler != null)
            {
            	handler.Invoke(this, null);
            }
        }

        public void HideFirstItemRemoveButton()
        {
            this.buttonRemove.Visible = false;
        }

        private void comboBoxAdvColumns_SelectedIndexChanged(object sender, EventArgs e)
        { 
            SetCriteriaType();
            FillFilterOperandCombo();
        }

        private void SetCriteriaType()
        {
            FilterCriteriaType criteriaType = ((FilterAdvancedSetting)this.comboBoxAdvColumns.SelectedItem).FilterCriteriaType;

            switch (criteriaType)
            {
                case FilterCriteriaType.HourMin : ShowCriteriaTypeHourMin(); break;
                case FilterCriteriaType.Date : ShowCriteriaTypeDate(); break;
                case FilterCriteriaType.Text : ShowCriteriaTypeText(); break;
                case FilterCriteriaType.Time: ShowCriteriaTypeTime(); break;
                case FilterCriteriaType.List: ShowCriteriaTypeList(); break;
                case FilterCriteriaType.Number: ShowCriteriaTypeText(); break;
            }
        }

        private void ShowCriteriaTypeHourMin()
        {
            textBoxExt1.Visible = false;
            timeSpanTextBox1.Visible = true;
            dateTimePickerAdvDate.Visible = false;
            office2007OutlookTimePicker1.Visible = false;
            comboBoxAdvCriteriaList.Visible = false;
        }

        private void ShowCriteriaTypeDate()
        {
            textBoxExt1.Visible = false;
            timeSpanTextBox1.Visible = false;
            dateTimePickerAdvDate.Visible = true;
            office2007OutlookTimePicker1.Visible = false;
            comboBoxAdvCriteriaList.Visible = false;
        }

        private void ShowCriteriaTypeTime()
        {
            textBoxExt1.Visible = false;
            timeSpanTextBox1.Visible = false;
            dateTimePickerAdvDate.Visible = false;
            office2007OutlookTimePicker1.Visible = true;
            comboBoxAdvCriteriaList.Visible = false;
        }

        private void ShowCriteriaTypeText()
        {
            textBoxExt1.Visible = true;
            timeSpanTextBox1.Visible = false;
            dateTimePickerAdvDate.Visible = false;
            office2007OutlookTimePicker1.Visible = false;
            comboBoxAdvCriteriaList.Visible = false;
        }

        private void ShowCriteriaTypeList()
        {
            textBoxExt1.Visible = false;
            timeSpanTextBox1.Visible = false;
            dateTimePickerAdvDate.Visible = false;
            office2007OutlookTimePicker1.Visible = false;
            comboBoxAdvCriteriaList.Visible = true;
            FillCriteriaCombo();
        }

        private void FillCriteriaCombo()
        {
            IList<FilterAdvancedTupleItem> criteriaList = ((FilterAdvancedSetting)this.comboBoxAdvColumns.SelectedItem).FilterCriteriaList;

            if (criteriaList != null)
            {
                this.comboBoxAdvCriteriaList.Items.Clear();

                this.comboBoxAdvCriteriaList.DisplayMember = "Text";
                this.comboBoxAdvCriteriaList.ValueMember = "Tag";
                foreach (FilterAdvancedTupleItem criteria in criteriaList)
                {
                    this.comboBoxAdvCriteriaList.Items.Add(new Control() { Text = criteria.Text, Tag = criteria });
                }

                this.comboBoxAdvCriteriaList.SelectedIndex = 0;
            }       
        }

        /// <summary>
        /// Sets the combo box with operands
        /// </summary>
        private void FillFilterOperandCombo()
        {
            IList<FilterAdvancedTupleItem> operands = (((FilterAdvancedSetting)this.comboBoxAdvColumns.SelectedItem).FilterOperands);

            this.comboBoxAdvFilterOperand.Items.Clear();
            this.comboBoxAdvFilterOperand.DisplayMember = "Text";
            this.comboBoxAdvFilterOperand.ValueMember = "Tag";

            foreach (FilterAdvancedTupleItem operand in operands)
            {
                this.comboBoxAdvFilterOperand.Items.Add(new Control() { Text = operand.Text, Tag = operand });
            }

            this.comboBoxAdvFilterOperand.SelectedIndex = 0;
        }

        /// <summary>
        /// Only numerics, but still possible to paste invalids...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxExt1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar)) e.Handled = true;
        }
        
    }
}
