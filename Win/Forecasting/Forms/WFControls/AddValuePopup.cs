using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Forecasting.Forms.WFControls
{
    public partial class AddValuePopup : RibbonForm
    {
        public AddValuePopup()
        {
            InitializeComponent();
            this.Load += new EventHandler(AddValuePopup_Load);

        }

        void AddValuePopup_Load(object sender, EventArgs e)
        {
            this.label1.Text  = InfoText;
            this.label2.Text = InfoType;
            
            //this.Tag = IsAbsolute;

        }


        [Browsable( false)]
        public string InfoText { get; set; }


        [Browsable(false)]
        public string InfoType { get; set; }


        public event EventHandler<CustomEventArgs<double>> ChangeWithThisValue;

        private void buttonAdvOk_Click(object sender, EventArgs e)
        {
            if(ChangeWithThisValue == null) return;

            //todo: move some typechecking in here, i cannot see how to because of different types of input expected from the user :/

            double result;
            if (double.TryParse(this.textBoxExt1.Text, out result))
            {
            ChangeWithThisValue.Invoke(this, new CustomEventArgs<double>(result));
            }
        }

        private void buttonAdvCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }



    }

  

}
