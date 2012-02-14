using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace Teleopti.Ccc.LanguageManager
{
    public partial class SelectAssembly : Form
    {
        public SelectAssembly()
        {
            InitializeComponent();
        }

        private void SelectAssembly_Load(object sender, EventArgs e)
        {
            listBoxAssemblies.DataSource = AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a.GetName().Name).ToList();
            listBoxAssemblies.SelectedItem = this.GetType().Assembly;
        }

        /// <summary>
        /// Gets the selected assembly.
        /// </summary>
        /// <value>The selected assembly.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-05
        /// </remarks>
        public Assembly SelectedAssembly
        {
            get { return listBoxAssemblies.SelectedItem as Assembly; }
        }
    }
}
