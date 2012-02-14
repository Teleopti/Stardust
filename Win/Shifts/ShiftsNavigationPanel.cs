﻿using System.Windows.Forms;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.ExceptionHandling;

namespace Teleopti.Ccc.Win.Shifts
{
    public partial class ShiftsNavigationPanel : Common.BaseUserControl
    {
        private readonly ShiftsModule.IShiftsExplorerFactory _shiftsExplorerFactory;

        public ShiftsNavigationPanel(ShiftsModule.IShiftsExplorerFactory shiftsExplorerFactory)
        {
            _shiftsExplorerFactory = shiftsExplorerFactory;
            InitializeComponent();
            SetTexts();
        }

        private void linkLabelShifts_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            open();
        }

        private void open()
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                _shiftsExplorerFactory.Create();
            }
            catch (DataSourceException dataSourceException)
            {
                using (var view = new SimpleExceptionHandlerView(dataSourceException,
                                                                    Resources.Shifts,
                                                                    Resources.ServerUnavailable))
                {
                    view.ShowDialog();
                }
            }

            Cursor.Current = Cursors.Default;
        }

        public void OpenShifts()
        {
            open();
            //linkLabelShifts_LinkClicked(this, new LinkLabelLinkClickedEventArgs(linkLabelShifts.Links[0]));
        }
    }
}