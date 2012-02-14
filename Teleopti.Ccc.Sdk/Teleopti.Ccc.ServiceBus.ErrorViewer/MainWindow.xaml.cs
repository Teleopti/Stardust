using System;
using System.Collections.ObjectModel;
using System.Windows;
using Rhino.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Messages;
using Teleopti.Ccc.ServiceBus.ErrorViewer.ViewModel;

namespace Teleopti.Ccc.ServiceBus.ErrorViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IIncomingMessageHandler _incomingMessageHandler;
        private readonly ObservableCollection<CustomErrorViewModel> _models = new ObservableCollection<CustomErrorViewModel>();
        private bool _isStarted;
        private static readonly object ModelsWriteLock = new object();

        public MainWindow() : this(Global.Container.Resolve<IIncomingMessageHandler>())
        {
        }

        public MainWindow(IIncomingMessageHandler incomingMessageHandler)
        {
            InitializeComponent();
            _incomingMessageHandler = incomingMessageHandler;
            _incomingMessageHandler.MessageArrived += HandleMessageArrived;
            DataContext = this;
        }

        private void HandleMessageArrived(CustomErrorMessage errorMessage)
        {
            if (Dispatcher.CheckAccess())
            {
                lock (ModelsWriteLock)
                {
                    _models.Add(new CustomErrorViewModel(errorMessage));
                }
            }
            else
            {
                Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action<CustomErrorMessage>(HandleMessageArrived), errorMessage);
            }
        }

        public ObservableCollection<CustomErrorViewModel> Models { get { return _models; } }

        private void buttonStartStop_Click(object sender, RoutedEventArgs e)
        {
                IServiceBus bus = Global.Container.Resolve<IServiceBus>();
                if (_isStarted)
                {
                    buttonStartStop.Content = "Start";
                    _isStarted = false;
                    bus.Unsubscribe<CustomErrorMessage>();
                }
                else
                {
                    buttonStartStop.Content = "Stop";
                    _isStarted = true;
                    bus.Subscribe<CustomErrorMessage>();
                }
        }

        private void listView1_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CustomErrorViewModel viewModel = listView1.SelectedItem as CustomErrorViewModel;
            if (viewModel!=null && !(viewModel.ObjectData is string))
            {
                IServiceBus bus = Global.Container.Resolve<IServiceBus>();
                bus.Send(new Endpoint{Uri = viewModel.Destination},viewModel.ObjectData);
            }
        }
    }
}
