using System;
using System.Text;
using Teleopti.Ccc.AgentPortal.Common;
using Teleopti.Ccc.AgentPortalCode.Common;

namespace Teleopti.Ccc.AgentPortal.AgentScheduleMessenger
{
    /// <summary>
    /// Display the Next Activiy Name and stating Time 
    /// </summary>
    /// <remarks>
    /// Created by: Sumedah
    /// Created date: 2008-07-26
    /// </remarks>
    public partial class NextActivityControl : BaseUserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NextActivityControl"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-07-26
        /// </remarks>
        public NextActivityControl( )
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Load event of the NextActivityControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-07-26
        /// </remarks>
        private void NextActivityControl_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            if (AgentScheduleStateHolder.IsInitialized)
            {
                AgentScheduleStateHolder.ActivityChanged += ScheduleStateHolder_ActivityChanged;
                AgentScheduleStateHolder.Instance().CheckNextActivity();
            }
        }

        /// <summary>
        /// Handles the ActivityChanged event of the AgentScheduleMessengerStateHolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ActivityChangedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-07-26
        /// </remarks>
        private void ScheduleStateHolder_ActivityChanged(object sender, ActivityChangedEventArgs e)
        {
            if (e.NextScheduleItem != null)
            {
                StringBuilder nextActivityTextBuilder = new StringBuilder();
                nextActivityTextBuilder.AppendLine(e.NextScheduleItem.Subject);
                nextActivityTextBuilder.AppendLine(e.NextScheduleItem.StartTime.ToShortTimeString());

                gradientLabelNextActivity.Text = nextActivityTextBuilder.ToString();
            }
            else
            {
                gradientLabelNextActivity.Text = string.Empty;
            }
            gradientLabelNextActivity.Invalidate();
        }

        private void UnhookEvents()
        {
            AgentScheduleStateHolder.ActivityChanged -= ScheduleStateHolder_ActivityChanged;
        }
    }
}
