using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win
{
    public partial class ControlTest : Form
    {
        public ControlTest()
        {
            InitializeComponent();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void button1_Click(object sender, EventArgs e)
        {
            StringBuilder s = new StringBuilder();
            foreach (var activity in this.twoListSelector1.GetSelected<IActivity>())
            {
                s.AppendLine(activity.Name);
            }
            MessageBox.Show(this, s.ToString(), "Choosen activities");

        }

        private void ControlTest_Load(object sender, EventArgs e)
        {
            using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                RepositoryFactory rep = new RepositoryFactory();
                IList<IActivity> availableActivities = rep.CreateActivityRepository(uow).LoadAllSortByName();
                IList<IActivity> selectedActivities = new List<IActivity>();
                selectedActivities.Add(new Activity("testing"));
                IActivity deleted = new Activity("gammal");
                ((IDeleteTag)deleted).SetDeleted();
                selectedActivities.Add(deleted);
                twoListSelector1.Initiate(availableActivities, selectedActivities, "Description.Name", "Hej, tjillevippen", "jhjhkjh");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (RightToLeft == System.Windows.Forms.RightToLeft.Yes)
            {
                RightToLeft = System.Windows.Forms.RightToLeft.No;
            }
            else
            {
                RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            }
            RightToLeftLayout = !RightToLeftLayout;       }
    }
}
