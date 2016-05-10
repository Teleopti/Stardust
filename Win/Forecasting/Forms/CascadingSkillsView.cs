using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Ccc.Win.Main;
using Teleopti.Ccc.WinCode.Forecasting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    public partial class CascadingSkillsView : BaseDialogForm
    {
        
        public CascadingSkillsView(CopyToSkillModel model)
        {
            InitializeComponent();
				
            if (DesignMode) return;
						var currentUnitOfWork = new FromFactory(() => UnitOfWorkFactory.Current);

            //Presenter = new CopyToSkillPresenter(this, model,
            //                                     new CopyToSkillCommand(this, model,
            //                                                            new WorkloadRepository(currentUnitOfWork),
            //                                                            UnitOfWorkFactory.Current),
            //                                     new SkillRepository(currentUnitOfWork),
            //                                     UnitOfWorkFactory.Current);
            SetTexts();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode)return;   
        }

        public void SetCopyFromText(string sourceWorkloadName)
        {
            labelSourceWorkload.Text = sourceWorkloadName;
        }
    }
}
