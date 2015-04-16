using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Common.UI.SmartPartControls.SmartParts
{
    public partial class SmartPartBase : UserControl
    {
        private readonly IList<SmartPartParameter> _smartPartParameterCollection=new List<SmartPartParameter>();
        private readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();

        public SmartPartBase()
        {
            InitializeComponent();
            if (!DesignMode) InitializeBackgoundWorker();
        }

        private void SmartPartBase_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                toolStripStatusLabelIcon.Visible = true;
                StartBackgroundWorker();
            }
        }

        public string SmartPartHeaderTitle
        {
            get { return FormTitle.Text; }
            set { FormTitle.Text = value; }
        }

        public string SmartPartId { get; set; }

        public ReadOnlyCollection<SmartPartParameter> SmartPartParameters
        {
            get { return new ReadOnlyCollection<SmartPartParameter>(_smartPartParameterCollection); }
        }

        public void RefreshSmartPart(IList<SmartPartParameter> parameters)
        {
            _smartPartParameterCollection.Clear();
            AddSmartPartParameter(parameters);

            RefreshSmartPart();
        }

        public virtual void RefreshSmartPart()
        {
            StartBackgroundWorker();
        }

        public void SetFormHeaderImage(int smartPartBaseInageType)
        {
            switch (smartPartBaseInageType)
            {
                case 1: FormTitle.Image = Properties.Resources.win_apps;
                    break;

                case 2: FormTitle.Image = Properties.Resources.browser;
                    break;

                case 3: FormTitle.Image = Properties.Resources.WPF;
                    break;

                default:
                    break;
            }
        }

        public void AddSmartPartParameter(IList<SmartPartParameter> parameters)
        {
            if (parameters != null)
            {
                foreach (SmartPartParameter parameter in parameters)
                {
                    _smartPartParameterCollection.Add(parameter);
                }
            }
        }

        public void RegisterForMessageBrokerEvents(Type type)
        {
            if (SmartPartEnvironment.MessageBroker != null)
                SmartPartEnvironment.MessageBroker.RegisterEventSubscription(OnEventMessageHandler, type);
        }

        public void UnregisterForMessageBrokerEvents()
        {
            if (SmartPartEnvironment.MessageBroker != null)
                SmartPartEnvironment.MessageBroker.UnregisterSubscription(OnEventMessageHandler);
        }

        public virtual void OnBackgroundProcess(DoWorkEventArgs e)
        {
        }

        public virtual void AfterBackgroundProcessCompleted()
        {
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            // Raise SmartPartClosing custom event
            //stop background operation if smaprt part is closing
            //if(_backgroundWorker.IsBusy) _backgroundWorker.CancelAsync();

            Dispose(true);
        }

        private void backgroundWorker_DoWork(object sender,
            DoWorkEventArgs e)
        {
            if (_backgroundWorker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
               OnBackgroundProcess(e);
            }
        }

        private void backgroundWorker_RunWorkerCompleted(
            object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed) return;
            if (e.Error != null)
            {
                Exception ex = new Exception("Background thread exception", e.Error);
                throw ex;
            }

            if (!DesignMode)
            {
                toolStripStatusLabelIcon.Visible = false;
                AfterBackgroundProcessCompleted();
            }
        }

        private void OnEventMessageHandler(object sender, EventMessageArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, EventMessageArgs>(OnEventMessageHandler), sender, e);
            }
            else
            {
                if ((e.Message.DomainUpdateType == DomainUpdateType.Insert) ||
                    (e.Message.DomainUpdateType == DomainUpdateType.Update) ||
                    (e.Message.DomainUpdateType == DomainUpdateType.Delete))
                {
                    RefreshSmartPart();
                }
            }
        }

        private void InitializeBackgoundWorker()
        {
            _backgroundWorker.DoWork += backgroundWorker_DoWork;
            _backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
        }

        private void StartBackgroundWorker()
        {
            if (_backgroundWorker.IsBusy) return;
            _backgroundWorker.RunWorkerAsync();
        }
    }
}
