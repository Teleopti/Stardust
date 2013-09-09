using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Globalization;

namespace Teleopti.Ccc.LanguageManager
{
    /// <summary>
    /// Form for Lanugage/Culture selection
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-12-05
    /// </remarks>
    public partial class SelectLanguage : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectLanguage"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-05
        /// </remarks>
        public SelectLanguage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Load event of the SelectLanguage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-05
        /// </remarks>
        private void SelectLanguage_Load(object sender, EventArgs e)
        {
            listBoxLanguages.DataSource = CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(c => c.DisplayName).ToList();
            listBoxLanguages.SelectedIndex = 0;
        }

        /// <summary>
        /// Gets the selected culture.
        /// </summary>
        /// <value>The selected culture.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-05
        /// </remarks>
        public CultureInfo SelectedCulture
        {
            get { return listBoxLanguages.SelectedItem as CultureInfo; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [main language visible].
        /// </summary>
        /// <value><c>true</c> if [main language visible]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-07
        /// </remarks>
        [Browsable(false)]
        public bool MainLanguageVisible
        {
            get { return checkBoxMainLanguage.Visible; }
            set { checkBoxMainLanguage.Visible = value; }
        }

        private void checkBoxMainLanguage_CheckedChanged(object sender, EventArgs e)
        {
            listBoxLanguages.Enabled = !checkBoxMainLanguage.Visible;
        }

        /// <summary>
        /// Gets a value indicating whether [main language selected].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [main language selected]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-07
        /// </remarks>
        [Browsable(false)]
        public bool MainLanguageSelected
        {
            get { return checkBoxMainLanguage.Checked; }
        }
    }
}
