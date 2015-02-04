using System;
using System.Windows.Forms;
using Autofac;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Backlog
{
	public partial class BacklogSelector : Form
	{
		private readonly IComponentContext _container;

		public BacklogSelector(IComponentContext container)
		{	
			InitializeComponent();
			_container = container;
			monthCalendar1.SelectionStart = new DateTime(2015, 06, 01);
			monthCalendar1.SelectionEnd = new DateTime(2015, 08, 23);
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			var period = new DateOnlyPeriod(new DateOnly(monthCalendar1.SelectionStart),
				new DateOnly(monthCalendar1.SelectionEnd));
			var blView = new BacklogView(_container, period);
			blView.Show();
			blView.Activate();
			Close();
		}
	}
}
