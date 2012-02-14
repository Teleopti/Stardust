#region Import namespaces

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common.Collections;
using Teleopti.Ccc.WinCode.Common.ControlBinders;
using Teleopti.Ccc.WinCode.Common.ControlExtenders;
using Teleopti.Ccc.Win.Common;

#endregion

namespace Teleopti.Ccc.Win.Permissions
{
    public partial class PermissionNavigationScreenAdv : Form
    {

        #region Constructor

        public PermissionNavigationScreenAdv()
        {
            InitializeComponent();
        }

        #endregion

    }

}