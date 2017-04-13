using System.Collections.Generic;
using System.Windows;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Messaging
{
    /// <summary>
    /// Interaction logic for PushMessageForm.xaml
    /// </summary>
    public partial class PushMessageForm : Window
    {
    	readonly PushMessageControl _control;

        protected PushMessageForm()
        {
			InitializeComponent();
        }

    	public PushMessageForm(IEnumerable<IPerson> receivers, IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory) : this()
    	{
			_control = new PushMessageControl(unitOfWorkFactory,repositoryFactory);
			Content = _control;
			SetReceivers(receivers);
    	}

    	public void SetReceivers(IEnumerable<IPerson> receivers)
        {
            _control.SetReceivers(receivers);
        }
    }
}
