using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
	public partial class ShovelingAnalyzerView : Form
	{
		public ShovelingAnalyzerView()
		{
			InitializeComponent();
		}

		public ShovelingAnalyzerView(DateOnly date, TimeSpan timeOfDay)
		{
			InitializeComponent();
		}
	}
}
