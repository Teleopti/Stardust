using System;
using System.Collections.Generic;
using System.Drawing;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.Win.Common.Controls
{
    /// <summary>
    /// a small form that gets a template and a namelist, it checks that the list is not in use and will save a copy of the template.
    /// </summary>
    /// <remarks>
    /// Created by: östenp
    /// Created date: 2008-03-27
    /// </remarks>
    public partial class SaveTemplateAs : Office2007Form
    {
        private SkillDayTemplate _skillTemplate;
        private WorkloadDayTemplate _WorkloadTemplate;

        public SaveTemplateAs()
        {
            InitializeComponent();
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="SaveTemplateAs"/> class.
        /// </summary>
        /// <param name="nameList">the namelist.</param>
        /// <param name="mySkillTemplate">the skilltemplate.</param>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2008-03-27
        /// </remarks>
    public SaveTemplateAs(IEnumerable<string> nameList,SkillDayTemplate mySkillTemplate)
        :this()
    {
        foreach (string s in nameList)
        {
            
            this.listBox1.Items.Add(s);
        }

        _skillTemplate = mySkillTemplate;
   }





    /// <summary>
    /// Initializes a new instance of the <see cref="SaveTemplateAs"/> class.
    /// </summary>
    /// <param name="nameList">The namelist.</param>
    /// <param name="myWorkTemplate">the worktemplate.</param>
    /// <remarks>
    /// Created by: östenp
    /// Created date: 2008-03-27
    /// </remarks>
        public SaveTemplateAs(IEnumerable<string> nameList, WorkloadDayTemplate  myWorkTemplate)
        : this()
    {
        foreach (string s in nameList)
        {

            this.listBox1.Items.Add(s);
        }

        _WorkloadTemplate = myWorkTemplate;

    }


        /// <summary>
        /// Handles the Click event of the buttonAdv1 control. and check that the entered name is not in use.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2008-03-27
        /// </remarks>
    private void buttonAdv1_Click(object sender, EventArgs e)
    {
        foreach (object item in listBox1.Items)
        {
            if ((string)item == textBoxExt1.Text)
            {
                listBox1.SelectedItem = item;
                this.label1.ForeColor = Color.Red;
                return;
            }
        }
        if (_WorkloadTemplate!= null )
        {
            //WorkloadDayTemplate wo = new WorkloadDayTemplate();
            //wo = _WorkloadTemplate.Clone();
            //wo.Name = this.textBoxExt1.Text;
            //wo.Save();
        }else if(_skillTemplate!=null)
        {
            //SkillDayTemplate st = new SkillDayTemplate();
            //st = _skillTemplate.Clone();
            //st.Name = this.textBoxExt1.Text;
            //st.Save()
        }//else
        //    throw new Exception("no template found");
    }
    
    
    
    
    
    }



}
