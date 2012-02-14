﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Presentation;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Reporting
{
    public partial class ReportUserSelectorAuditingView : BaseUserControl, IReportUserSelectorAuditingView
    {
        private ReportUserSelectorAuditingPresenter _presenter;

        public ReportUserSelectorAuditingView()
        {
            InitializeComponent();
            if (DesignMode) return;
            
            SetTexts();
        }

        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode || !StateHolderReader.IsInitialized) return;
            base.OnLoad(e);
        }

        public override bool HasHelp
        {
            get
            {
                return false;
            }
        }

        public void UpdateUsersCombo(ReadOnlyCollection<ReportUserSelectorAuditingModel> userList)
        {
            comboBoxAdvUser.DisplayMember = "Text";
            comboBoxAdvUser.DataSource = userList;
        }

        public void SetSelectedUserById(Guid id)
        {
            foreach (ReportUserSelectorAuditingModel item in comboBoxAdvUser.Items)
            {
                if (item.Id.Equals(id))
                {
                    comboBoxAdvUser.SelectedIndex = comboBoxAdvUser.Items.IndexOf(item);
                    return;
                }
            }
        }

        public void SetSelectedUser(ReportUserSelectorAuditingModel reportUserSelectorAuditingModel)
        {
            comboBoxAdvUser.SelectedItem = reportUserSelectorAuditingModel;
        }

        public void Initialize()
        {
			  _presenter = new ReportUserSelectorAuditingPresenter(this, new ScheduleHistoryReport(UnitOfWorkFactory.Current, TeleoptiPrincipal.Current.Regional), UnitOfWorkFactory.Current);
            _presenter.Initialize();
        }

        public ReportUserSelectorAuditingModel SelectedUserModel
        {
            get { return (ReportUserSelectorAuditingModel)comboBoxAdvUser.SelectedItem; }
        }

        public IList<IPerson> SelectedUsers
        {
            get { return _presenter.SelectedUsers(); }
        }
    }
}
