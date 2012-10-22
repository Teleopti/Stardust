using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Xsl;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.LanguageManager
{
    /// <summary>
    /// Form to manage translations
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-12-05
    /// </remarks>
    public partial class TranslationTool : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationTool"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-12-05
        /// </remarks>
        public TranslationTool()
        {
            InitializeComponent();

            //Ugly code to get the assembly loaded...
            DataHandlingStatusOption temp = DataHandlingStatusOption.DataEditing;
            if (temp == DataHandlingStatusOption.None) return;

            //AgentDayInformation adi = null;
            //if (adi != null) return;
        }

        private const string resourceFileKeyName = "ResXFile";
        private const string synchronizedLanguageKeyName = "SynchronizedLanguage";

        private void buttonBrowseFile_Click(object sender, EventArgs e)
        {
            openFileDialogResX.FileName = textBoxResXFile.Text;
            if (openFileDialogResX.ShowDialog() == DialogResult.OK)
            {
                textBoxResXFile.Text = openFileDialogResX.FileName;
            }
        }

        private void TranslationTool_Load(object sender, EventArgs e)
        {
            textBoxResXFile.Text = ConfigurationManager.AppSettings[resourceFileKeyName];
            comboBoxCultures.DisplayMember = "DisplayName";
            comboBoxCultures.ValueMember = "LCID";
            comboBoxCultures.DataSource = CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(c => c.DisplayName).ToList();

            textBoxResXFile.Text = ConfigurationManager.AppSettings[resourceFileKeyName];
            string synchronizedLanguage = ConfigurationManager.AppSettings[synchronizedLanguageKeyName];
            if (!string.IsNullOrEmpty(synchronizedLanguage))
            {
                comboBoxCultures.SelectedValue = int.Parse(synchronizedLanguage, CultureInfo.CurrentCulture);
                checkBoxNoSync.Checked = false;
            }
        }

        private void TranslationTool_FormClosed(object sender, FormClosedEventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            if (config.AppSettings.Settings.AllKeys.Contains(resourceFileKeyName))
            {
                config.AppSettings.Settings[resourceFileKeyName].Value = textBoxResXFile.Text;
            }
            else
            {
                config.AppSettings.Settings.Add(resourceFileKeyName, textBoxResXFile.Text);
            }
            if (config.AppSettings.Settings.AllKeys.Contains(synchronizedLanguageKeyName))
            {
                if (checkBoxNoSync.Checked)
                    config.AppSettings.Settings.Remove(synchronizedLanguageKeyName);
                else
                    config.AppSettings.Settings[synchronizedLanguageKeyName].Value = comboBoxCultures.SelectedValue.ToString();
            }
            else
            {
                if (!checkBoxNoSync.Checked)
                    config.AppSettings.Settings.Add(synchronizedLanguageKeyName, comboBoxCultures.SelectedValue.ToString());
            }
            config.Save();
        }

        private void checkBoxNoSync_CheckStateChanged(object sender, EventArgs e)
        {
            comboBoxCultures.Enabled = !checkBoxNoSync.Checked;
        }

        private void buttonCreateTranslation_Click(object sender, EventArgs e)
        {
            using (var selectAssembly = new SelectAssembly())
            {
                if (selectAssembly.ShowDialog() == DialogResult.Cancel) return;

                Assembly assembly = selectAssembly.SelectedAssembly;

                using (var selectLocalisationItem = new SelectLocalizationItem(assembly))
                {
                    if (selectLocalisationItem.ShowDialog() == DialogResult.Cancel) return;

                    Type selectedTypeForLocalisation = selectLocalisationItem.SelectedLocalizationItem;

                    var resourceHelper = new ResourceFileHelper(@textBoxResXFile.Text);
                    try
                    {
                        resourceHelper.CreateNewResources(selectedTypeForLocalisation);
                    }
                    catch (InvalidOperationException ex)
                    {
                        MessageBox.Show(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "An error occurred while trying to extract information from class: \n\n{0}",
                                ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, (this.RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0));

                        return;
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        MessageBox.Show(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                "Error while trying to write information to resx file: \n\n{0}",
                                ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, (this.RightToLeft == RightToLeft.Yes ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0));

                        return;
                    }

                    //Synchronize to other language resource
                    if (!checkBoxNoSync.Checked)
                        resourceHelper.SynchronizeToLanguage((int)comboBoxCultures.SelectedValue, false);

                    MessageBox.Show(
                        string.Format(CultureInfo.CurrentCulture,
                            "Added new translation for the type {0} into neutral language resx and to synchronized language resx.",
                            selectedTypeForLocalisation.Name),
                            "",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1,
                        ((RightToLeft == RightToLeft.Yes) ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0));
                }
                    
            }
            
        }

        private void buttonAddNewLanguage_Click(object sender, EventArgs e)
        {
            using(var selectLanguage = new SelectLanguage())
            {
                if (selectLanguage.ShowDialog() == DialogResult.Cancel) return;
                CultureInfo targetCulture = selectLanguage.SelectedCulture;

                var resourceHelper = new ResourceFileHelper(@textBoxResXFile.Text);
                resourceHelper.SynchronizeToLanguage(targetCulture.LCID, false);

                MessageBox.Show(
                    string.Format(CultureInfo.CurrentCulture,
                        "Created a new resx file for the new language: {0}. You must add the file to the project to include translation.",
                        targetCulture.DisplayName),
                    "",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    ((RightToLeft == RightToLeft.Yes) ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0));
            }
        }

        private void buttonExportForTranslation_Click(object sender, EventArgs e)
        {
            using (var selectLanguage = new SelectLanguage())
            {
                if (selectLanguage.ShowDialog() == DialogResult.Cancel) return;

            CultureInfo targetCulture = selectLanguage.SelectedCulture;
            saveFileDialogForTranslation.FileName = string.Format(CultureInfo.CurrentCulture,
                textBoxResXFile.Text.Insert(textBoxResXFile.Text.LastIndexOf(Path.DirectorySeparatorChar) + 1, "{0}_{1}_"),
                    "TranslateTo",
                    targetCulture.EnglishName).Replace(".resx",".xls");

            if (saveFileDialogForTranslation.ShowDialog() == DialogResult.Cancel) return;

            string exportFileName = @saveFileDialogForTranslation.FileName;
            if (exportFileName == textBoxResXFile.Text)
            {
                MessageBox.Show("The neutral language resource file cannot be replaced!",
                    "",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    ((RightToLeft==RightToLeft.Yes) ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0));
                return;
            }

            ResourceFileHelper resourceHelper = new ResourceFileHelper(@textBoxResXFile.Text);
            resourceHelper.CreateFileForLanguageTranslation(targetCulture.LCID, exportFileName);

            MessageBox.Show(
                string.Format(CultureInfo.CurrentCulture,
                    "Successfully exported file for translation to {0}, path: {1}.",
                        targetCulture.DisplayName,
                        exportFileName),
                        "",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                ((RightToLeft==RightToLeft.Yes) ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0));
            }
            
        }

        private void buttonImportFromTranslation_Click(object sender, EventArgs e)
        {
            using (var selectLanguage = new SelectLanguage())
            {
                if (selectLanguage.ShowDialog() == DialogResult.Cancel) return;

                CultureInfo targetCulture = selectLanguage.SelectedCulture;

                openFileDialogExcel.FileName = string.Format(
                    CultureInfo.CurrentCulture,
                    textBoxResXFile.Text.Insert(textBoxResXFile.Text.LastIndexOf(Path.DirectorySeparatorChar) + 1, "{0}_{1}_"),
                        "TranslateTo",
                        targetCulture.EnglishName).Replace(".resx", ".xls");

                if (openFileDialogExcel.ShowDialog() == DialogResult.Cancel) return;
                string importFileName = @openFileDialogExcel.FileName;

                XslCompiledTransform xslTransformer = new XslCompiledTransform();
                xslTransformer.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExcelToResx.xslt"));
                xslTransformer.Transform(importFileName, importFileName.Replace(".xls", ".resx"));

                ResourceFileHelper resourceHelper = new ResourceFileHelper(@textBoxResXFile.Text);
                resourceHelper.SynchronizeToLanguage(targetCulture.LCID, importFileName.Replace(".xls", ".resx"), true);

                MessageBox.Show(
                    string.Format(CultureInfo.CurrentCulture,
                        "Successfully imported file from {0} translation, path: {1}.",
                        targetCulture.DisplayName,
                        importFileName),
                        "",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    ((RightToLeft == RightToLeft.Yes) ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0));
            }
            
        }

        private void buttonExportSummary_Click(object sender, EventArgs e)
        {
            using (var selectLanguage = new SelectLanguage())
            {
                selectLanguage.MainLanguageVisible = true;
                if (selectLanguage.ShowDialog() == DialogResult.Cancel) return;

                int? lcid = null;
                string languageName;
                if (selectLanguage.MainLanguageSelected)
                {
                    languageName = "Neutral";
                }
                else
                {
                    lcid = selectLanguage.SelectedCulture.LCID;
                    languageName = selectLanguage.SelectedCulture.EnglishName;
                }

                saveFileDialogForTranslation.FileName = string.Format(CultureInfo.CurrentCulture,
                    textBoxResXFile.Text.Insert(textBoxResXFile.Text.LastIndexOf(Path.DirectorySeparatorChar) + 1, "{0}_{1}_"),
                        "Summary",
                        languageName).Replace(".resx", ".xls");

                if (saveFileDialogForTranslation.ShowDialog() == DialogResult.Cancel) return;

                string exportFileName = @saveFileDialogForTranslation.FileName;
                if (exportFileName == textBoxResXFile.Text)
                {
                    MessageBox.Show("The neutral language resource file cannot be replaced!",
                        "",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1,
                        ((RightToLeft == RightToLeft.Yes) ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0));
                    return;
                }

                ResourceFileHelper resourceHelper = new ResourceFileHelper(@textBoxResXFile.Text);
                resourceHelper.ExportLanguageSummary(lcid, exportFileName);

                MessageBox.Show(
                    string.Format(CultureInfo.CurrentCulture,
                        "Successfully exported summary for {0}, path: {1}.",
                            languageName,
                            exportFileName),
                            "",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1,
                    ((RightToLeft == RightToLeft.Yes) ? MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading : 0));
            }
            
        }
    }
}
