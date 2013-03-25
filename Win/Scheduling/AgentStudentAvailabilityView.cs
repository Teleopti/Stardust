using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Scheduling;

namespace Teleopti.Ccc.Win.Scheduling
{
	public partial class AgentStudentAvailabilityView : BaseRibbonForm, IAgentStudentAvailabilityView
	{
		public AgentStudentAvailabilityView()
		{
			InitializeComponent();
		}

		public void Update(TimeSpan? startTime, TimeSpan? endTime, bool endNextDay)
		{
			
		}
	}
}
