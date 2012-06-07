using System.Collections.Generic;
using System.Windows;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WpfControls.Controls.Messaging
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
