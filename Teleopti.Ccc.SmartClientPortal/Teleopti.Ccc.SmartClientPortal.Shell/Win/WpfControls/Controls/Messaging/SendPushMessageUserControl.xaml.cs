using System.Collections.Generic;
using System.Windows.Controls;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Messaging;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Messaging
{

    /// <summary>
    /// UserControl for just sending a message
    /// Use SendPushMessageControl for sending/followup
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2009-06-04
    /// </remarks>
    public partial class SendPushMessageUserControl : UserControl
    {
        private SendPushMessageViewModel _model;

        /// <summary>
        /// Gets the receivers.
        /// </summary>
        /// <value>The receivers.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-06-04
        /// </remarks>
        public IList<IPerson> Receivers 
        {
            get { return _model.Receivers; }
        }

        public SendPushMessageUserControl():this(new List<IPerson>())
        {
            
        }

        public SendPushMessageUserControl(IPerson receiver):this(new List<IPerson>(){receiver})
        {
            
        }

        public SendPushMessageUserControl(IList<IPerson> receivers)
        {
            InitializeComponent();
            _model = new SendPushMessageViewModel();
            receivers.ForEach(r => _model.Receivers.Add(r));
            DataContext = _model;
        }
    }
}
