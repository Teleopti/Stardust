using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Main;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{
    public partial class CopyToSkillView : BaseDialogForm, ICopyToSkillView
    {
        public CopyToSkillPresenter Presenter { get; set; }

        public CopyToSkillView(CopyToSkillModel model)
        {
            InitializeComponent();

            if (DesignMode) return;
						var currentUnitOfWork = new FromFactory(() => UnitOfWorkFactory.Current);

            Presenter = new CopyToSkillPresenter(this, model,
                                                 new CopyToSkillCommand(this, model,
                                                                        WorkloadRepository.DONT_USE_CTOR(currentUnitOfWork),
                                                                        UnitOfWorkFactory.Current),
                                                 SkillRepository.DONT_USE_CTOR(currentUnitOfWork),
                                                 UnitOfWorkFactory.Current);
            SetTexts();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode)return;

            Presenter.Initialize();

            if (comboBoxAdvSkills.Items.Count > 0)
                comboBoxAdvSkills.SelectedIndex = 0;
        }

        public void SetCopyFromText(string sourceWorkloadName)
        {
            labelSourceWorkload.Text = sourceWorkloadName;
        }

        public void ToggleIncludeTemplates(bool includeTemplates)
        {
            checkBoxAdvIncludeTemplates.Checked = includeTemplates;
        }

        public void ToggleIncludeQueues(bool includeQueues)
        {
            checkBoxAdvIncludeQueues.Checked = includeQueues;
        }

        public void AddSkillToList(string name, ISkill skill)
        {
            comboBoxAdvSkills.Items.Add(new TupleItem(name, skill));
        }

        public void NoMatchingSkillsAvailable()
        {
            ShowErrorMessage(UserTexts.Resources.NoSkillMatchingPropertiesForCurrentSkillFound,UserTexts.Resources.CopyToThreeDots);
        }

        public void TriggerEntitiesNeedRefresh(IEnumerable<IRootChangeInfo> changes)
        {
            EntityEventAggregator.TriggerEntitiesNeedRefresh(this,changes);
        }

        private void buttonAdvOk_Click(object sender, EventArgs e)
        {
            Presenter.Copy();
        }

        private void checkBoxAdvIncludeTemplates_CheckedChanged(object sender, EventArgs e)
        {
            Presenter.ToggleIncludeTemplates(checkBoxAdvIncludeTemplates.Checked);
        }

        private void checkBoxAdvIncludeQueues_CheckStateChanged(object sender, EventArgs e)
        {
            Presenter.ToggleIncludeQueues(checkBoxAdvIncludeQueues.Checked);
        }

        private void comboBoxAdvSkills_SelectedIndexChanged(object sender, EventArgs e)
        {
            Presenter.SetTargetSkill(((ISkill) ((TupleItem) comboBoxAdvSkills.SelectedItem).ValueMember));
        }

		private void CopyToSkillView_Load(object sender, EventArgs e)
		{

		}

		private void tableLayoutPanel1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{

		}

		private void buttonAdvCancel_Click(object sender, EventArgs e)
		{

		}

		private void tableLayoutPanel2_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{

		}
    }
}
