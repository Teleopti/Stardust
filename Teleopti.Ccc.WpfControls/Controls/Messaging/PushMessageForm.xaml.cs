using System.Collections.Generic;
using System.Windows;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Controls.Messaging
{
    /// <summary>
    /// Interaction logic for PushMessageForm.xaml
    /// </summary>
    public partial class PushMessageForm : Window
    {
        PushMessageControl _control = new PushMessageControl();

        
        public PushMessageForm():this(new List<IPerson>())
        {
        }

        public PushMessageForm(IEnumerable<IPerson> receivers)
        {
            InitializeComponent();
            Content = _control;
            SetReceivers(receivers);
            
        }

        public void SetReceivers(IEnumerable<IPerson> receivers)
        {
            _control.SetReceivers(receivers);
        }
    }
}
