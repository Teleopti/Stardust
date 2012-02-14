using System.Collections.Generic;
using System.Windows.Controls;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WinCode.Common.Messaging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Controls.Messaging
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
