using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Forecasting.QuickForecastPages;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.QuickForecast
{
	public partial class SelectWorkload : BaseUserControl, IPropertyPageNoRoot<QuickForecastModel>
    {
		private readonly ICollection<ISkill> _skills;
        private readonly ICollection<string> _errorMessages = new List<string>();
 
		public SelectWorkload()
		{
			InitializeComponent();
		}

        public SelectWorkload(ICollection<ISkill> skills):this()
        {
	        _skills = skills;
            SetTexts();
            setColors();
        }

        private void setColors()
        {
            BackColor = ColorHelper.WizardBackgroundColor();
            treeViewSkills.BackColor = ColorHelper.WizardPanelBackgroundColor();
        }

		public void Populate(QuickForecastModel stateObj)
        {
			reloadSkillsTreeView(stateObj);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool Depopulate(QuickForecastModel stateObj)
        {
            if (!validated()) return false;

			stateObj.SelectedWorkloads.Clear();
			foreach (var checkedNode in treeViewSkills.CheckedNodes.Cast<TreeNodeAdv>().Where(checkedNode => checkedNode.Tag != null))
			{
				stateObj.SelectedWorkloads.Add((Guid)checkedNode.Tag);
			}

            return true;
        }

        public void SetEditMode()
        {
        }

        public string PageName
        {
            get { return Resources.SelectWorkload; }
        }

        public ICollection<string> ErrorMessages
        {
            get { return _errorMessages; }
        }

        private void reloadSkillsTreeView(QuickForecastModel stateObj)
        {
			treeViewSkills.Nodes.Clear();
	        var root = new TreeNodeAdv(Resources.Skills);
	        treeViewSkills.Nodes.Add(root);
	        foreach (var skill in _skills)
	        {
		        var skillNode = new TreeNodeAdv(skill.Name);
		        root.Nodes.Add(skillNode);
		        foreach (var workLoad in skill.WorkloadCollection)
		        {
			       skillNode.Nodes.Add(new TreeNodeAdv(workLoad.Name){Tag = workLoad.Id,
				   Checked = stateObj.SelectedWorkloads.Contains(workLoad.Id.GetValueOrDefault())});
		        }
	        }
			root.Expand();
        }

        private bool validated()
        {
            if (treeViewSkills.CheckedNodes.Count != 0)
                return true;

            MessageBoxAdv.Show(Resources.YouHaveToSelectAtLeastOneWorkload, Resources.Message,
                               MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return false;
        }

       
    }
}
