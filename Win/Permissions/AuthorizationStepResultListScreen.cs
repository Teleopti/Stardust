using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Permissions
{
    public partial class AuthorizationStepResultListScreen : BaseRibbonForm
    {
        private IAuthorizationStep _step;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationStepResultListScreen"/> class.
        /// </summary>
        /// <param name="step">The current step.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/29/2007
        /// </remarks>
        public AuthorizationStepResultListScreen(IAuthorizationStep step)
            : this()
        {
            _step = step;
            DataBindUserRightsGrid();
            ExchangeData(ExchangeDataOption.ServerToClient);
        }

        /// <summary>
        /// Gets the close button.
        /// </summary>
        /// <value>The close button.</value>
        public Button CloseButton
        {
            get { return xbtnClose; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationStepResultListScreen"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/29/2007
        /// </remarks>
        private AuthorizationStepResultListScreen()
        {
            InitializeComponent();
            if (!this.DesignMode) SetTexts();
        }

        /// <summary>
        /// Exchanges the data.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/29/2007
        /// </remarks>
        private void ExchangeData(ExchangeDataOption direction)
        {
            if (direction == ExchangeDataOption.ServerToClient)
            {
                xlblCurrentStep.Text = _step.PanelName;
                xlblCurrentStepDescription.Text = _step.PanelDescription;
                FillUserRightsGrid();
            }
            else
            {
                //
            }
        }

        /// <summary>
        /// Databinds the personItem rights grid.
        /// </summary>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/29/2007
        /// </remarks>
        private void DataBindUserRightsGrid()
        {
            //TODO: the majority of the databind is made on the designer. Move it here from there.
            xgridResultList.AutoGenerateColumns = false;
        }

        /// <summary>
        /// Fill the personItem rights grid according to the provided authorization step.
        /// </summary>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/29/2007
        /// </remarks>
        private void FillUserRightsGrid()
        {
            IList<IAuthorizationEntity> providedList = _step.ProvidedList<IAuthorizationEntity>();
            xgridResultList.DataSource = providedList;
        }


        /// <summary>
        /// Handles the Click event of the xbtnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/29/2007
        /// </remarks>
        private void xbtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Validates the control data.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/29/2007
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private bool ValidateControlData()
        {
            return true;
        }

    }
}