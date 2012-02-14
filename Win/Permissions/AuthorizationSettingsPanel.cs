using System.ComponentModel;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Permissions
{
    public partial class AuthorizationSettingsPanel : UserControl, IGuestDataHandling<AuthorizationSettings>
    {
        private AuthorizationSettings _dataSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationSettingsPanel"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        public AuthorizationSettingsPanel()
        {
            InitializeComponent();
        }

        #region IGuestDataHandling Members

        /// <summary>
        /// Gets or sets the datasource.
        /// </summary>
        /// <value>The data source.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        [Browsable(false)]
        public AuthorizationSettings DataSource
        {
            get
            {
                if (_dataSource != null)
                {
                    ExchangeData(ExchangeDataOption.ClientToServer);
                }
                return _dataSource;
            }
            set
            {
                if (value != null)
                {
                    _dataSource = value;
                    ExchangeData(ExchangeDataOption.ServerToClient);
                }
            }
        }

        /// <summary>
        /// Validates the user edited data in control.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        public bool ValidateData(ExchangeDataOption direction)
        {
            return true;
        }

        /// <summary>
        /// Exchances the data between the controls and the datasource object.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        public void ExchangeData(ExchangeDataOption direction)
        {
            if (direction == ExchangeDataOption.ServerToClient)
            {
                xchkDatabaseDefinedRoles.Checked = _dataSource.UseDatabaseDefinedRoles;
                xchkActiveDirectoryDefinedRoles.Checked = _dataSource.UseActiveDirectoryDefinedRoles;
            }
            else
            {
                _dataSource.UseDatabaseDefinedRoles = xchkDatabaseDefinedRoles.Checked;
                _dataSource.UseActiveDirectoryDefinedRoles = xchkActiveDirectoryDefinedRoles.Checked;
            }
        }

        #endregion

    }
}
