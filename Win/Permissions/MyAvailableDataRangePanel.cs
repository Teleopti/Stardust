using System.ComponentModel;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Permissions
{
    public partial class MyAvailableDataRangePanel : UserControl, IGuestDataHandling<IAvailableData>
    {
        private IAvailableData _dataSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyAvailableDataRangePanel"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        public MyAvailableDataRangePanel()
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
        public IAvailableData DataSource
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
                switch(_dataSource.AvailableDataRange)
                {
                    case AvailableDataRangeOption.Everyone:
                        optionEveryone.Checked = true;
                        break;
                    case AvailableDataRangeOption.MyBusinessUnit:
                        optionMyBusinessUnit.Checked = true;
                        break;
                    case AvailableDataRangeOption.MySite:
                        optionMySite.Checked = true;
                        break;
                    case AvailableDataRangeOption.MyTeam:
                        optionMyTeam.Checked = true;
                        break;
                    case AvailableDataRangeOption.MyOwn:
                        optionMyself.Checked = true;
                        break;
                    default:
                        optionNone.Checked = true;
                        break;

                }
            }
            else
            {
                if (optionEveryone.Checked)
                    _dataSource.AvailableDataRange = AvailableDataRangeOption.Everyone;
                if (optionMyBusinessUnit.Checked)
                    _dataSource.AvailableDataRange = AvailableDataRangeOption.MyBusinessUnit;
                if (optionMySite.Checked)
                    _dataSource.AvailableDataRange = AvailableDataRangeOption.MySite;
                if (optionMyTeam.Checked)
                    _dataSource.AvailableDataRange = AvailableDataRangeOption.MyTeam;
                if (optionMyself.Checked)
                    _dataSource.AvailableDataRange = AvailableDataRangeOption.MyOwn;
                if (optionNone.Checked)
                    _dataSource.AvailableDataRange = AvailableDataRangeOption.None;
            }
        }

        #endregion

    }
}
