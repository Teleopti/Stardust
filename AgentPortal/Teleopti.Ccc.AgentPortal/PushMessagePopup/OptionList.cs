using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Teleopti.Ccc.AgentPortal.PushMessagePopup
{
    public partial class OptionList : UserControl
    {
        private string _selected;
        public event EventHandler RadioButtonIsChecked;
        public OptionList()
        {
            InitializeComponent();
        }

        public OptionList(IEnumerable<string> optionList)
        {
            panel1.Controls.Clear();
            foreach (string item in optionList)
            {
                CreateRadioButton(item);
            }
        }

        //Fix later
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
        public ICollection<string> OptionItems
        {
            set
            {
                panel1.Controls.Clear();
                _selected = "";
                if (value.Count == 1)
                {
                    IEnumerator<string> enumerator = value.GetEnumerator();
                    enumerator.MoveNext();
                    _selected = enumerator.Current;
                }

                foreach (string item in value)
                {
                    CreateRadioButton(item);
                }
            }
        }

        public string Selected
        {
            get
            {
                foreach (object s in panel1.Controls)
                {
                    if (s.GetType() == typeof (RadioButton))
                    {
                        var t = (RadioButton) s;
                        if (t.Checked)
                            return t.Text;
                    }
                }
                return _selected ;
            }
        }

        private void CreateRadioButton(string item)
        {
            var button = new RadioButton {Checked = false, Text = item, AutoSize = true};
            button.CheckedChanged += button_CheckedChanged;
            panel1.Controls.Add(button);
        }
        
        private void button_CheckedChanged(object sender, EventArgs e)
        {
            if (RadioButtonIsChecked != null)
            {
                RadioButtonIsChecked.Invoke(this, null);
            }
        }
    }
}