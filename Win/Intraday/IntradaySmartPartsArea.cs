using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Win.Common;
using Teleopti.Common.UI.SmartPartControls.SmartParts;

namespace Teleopti.Ccc.Win.Intraday
{
    public partial class IntradaySmartPartsArea : BaseUserControl
    {
        private const string Assembly = "Teleopti.Ccc.SmartParts";
        private const string ClassPrefix = "Teleopti.Ccc.SmartParts.Forecasting";
        private const string Detailed = ".DetailedSmartPart";
        private const string Validation = ".ValidationSmartPart";
        private const string Budget = ".BudgetsSmartPart";
        private const string Template = ".TemplateSmartPart";

        public IntradaySmartPartsArea()
        {
            InitializeComponent();
            InitializeSmartPartInvoker();
        }


        private void InitializeSmartPartInvoker()
        {
            string smartPartPath = ConfigurationManager.AppSettings["smartPartPath"];

            SmartPartWorker smartPartWorker = new SmartPartWorker(smartPartPath, gridWorkspace1 );
            SmartPartCommand smartPartCommand = new SmartPartCommand(smartPartWorker);
            SmartPartInvoker.SmartPartCommand = smartPartCommand;

            //todo  just for demo - some skill guid copied
            Guid g = new Guid("45818d7d-8ef0-4147-8008-9b3600fffad1");
            LoadSmartPart(g, 1, UserTexts.Resources.SkillValidationSmartPart, ClassPrefix + Validation, 0, 0);

        }
        private static void LoadSmartPart(Guid skill, int smartPartId, string SmartPartHeaderTitle,
                                        string smartPartName, int row, int col)
        {
            SmartPartInformation smartPartInfo = new SmartPartInformation();
            smartPartInfo.ContainingAssembly = Assembly;
            smartPartInfo.SmartPartName = smartPartName;
            smartPartInfo.SmartPartHeaderTitle = SmartPartHeaderTitle;
            smartPartInfo.GridColumn = col;
            smartPartInfo.GridRow = row;
            smartPartInfo.SmartPartId = smartPartId.ToString(CultureInfo.CurrentCulture);  // this need to be unique

            // Create SmartPart Parameters  [optional]
            IList<SmartPartParameter> parameters = new List<SmartPartParameter>();
            SmartPartParameter parameter = new SmartPartParameter("Skill", skill);

            parameters.Add(parameter);

            try
            {
                // Invoke SmartPart
                SmartPartInvoker.ShowSmartPart(smartPartInfo, parameters);
            }
            catch (FileLoadException)
            {
                //TODO:need to log exception
            }
            catch (FileNotFoundException)
            {
                //TODO:need to log exception
            }
        }

        ///todo
        /// V smartpartsgrid
        ///     V intitialize
        ///     V show something
        ///
        /// * smartpartsinitialaizing
        /// * smartpartssettings
        /// 




    }
}
