using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.LanguageManager
{
    /// <summary>
    /// Select which item should be localized
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-12-05
    /// </remarks>
    public partial class SelectLocalizationItem : Form
    {
        private Assembly _assembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectLocalizationItem"/> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-05
        /// </remarks>
        public SelectLocalizationItem(Assembly assembly)
        {
            InitializeComponent();

            _assembly = assembly;
        }

        /// <summary>
        /// Handles the Load event of the SelectLocalizationItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-05
        /// </remarks>
        private void SelectLocalisationItem_Load(object sender, EventArgs e)
        {
            Type[] typesInAssembly = _assembly.GetTypes();
            var wantedTypes = (from t in typesInAssembly
                              where typeof(ILocalized).IsAssignableFrom(t) &&
                                    t.IsClass
                              orderby t.ToString()
                              select t).ToList();

            if (wantedTypes.Count == 0)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }
            listBoxItems.DataSource = wantedTypes;
            listBoxItems.SelectedIndex = 0;
        }

        /// <summary>
        /// Gets the selected localization item.
        /// </summary>
        /// <value>The selected localization item.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-05
        /// </remarks>
        public Type SelectedLocalizationItem
        {
            get { return listBoxItems.SelectedItem as Type; }
        }
    }
}
