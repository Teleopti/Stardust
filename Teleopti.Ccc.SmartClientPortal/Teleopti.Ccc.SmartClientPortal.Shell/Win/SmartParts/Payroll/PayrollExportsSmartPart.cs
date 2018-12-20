using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.Coders;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.PayrollFormatter;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.PayrollExportPages.PayrollExportSmartPart;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts.Payroll
{
    public partial class PayrollExportsSmartPart : ExtendedSmartPartBase, WinCode.Common.IObservable<IPayrollResult>
    {
        private ICollection<IPayrollResult> _payrollResults;
        private PayrollExportSmartPartViewModel _model;
        private GridSizeType _normalGridsize;

        public PayrollExportsSmartPart()
        {
            _normalGridsize = SmartPartEnvironment.SmartPartWorkspace.GridSize;
            SmartPartEnvironment.SmartPartWorkspace.GridSize = GridSizeType.OneByOne;
            InitializeComponent();
            _model = new PayrollExportSmartPartViewModel(this);
            var exportView = new PayrollExportDataGridView();
            exportView.DataContext = _model;
            LoadExtender(exportView);
        }

        private void loadData()
        {
            using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
				var currUow = new ThisUnitOfWork(uow);
                var guid = (Guid)SmartPartParameters[0].Value;
                var payrollExport = new PayrollExportRepository(currUow).Get(guid);
                _payrollResults = new PayrollResultRepository(currUow).GetPayrollResultsByPayrollExport(payrollExport);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            RegisterForMessageBrokerEvents(typeof(IPayrollResult));

            if (SmartPartEnvironment.MessageBroker != null)
            {
                SmartPartEnvironment.MessageBroker.RegisterEventSubscription(OnProgressChanged, typeof(IJobResultProgress));
            }
        }

	    private void OnProgressChanged(object sender, EventMessageArgs e)
	    {
			var domainObject = e?.Message?.DomainObject;
		    if (domainObject == null) 
				return;
		    
			var exportProgress = new JobResultProgressDecoder().Decode(domainObject);

		    if (exportProgress != null)
			    _model.UpdateProgress(exportProgress);
	    }

	    private void UnregisterEvents()
        {
            if (SmartPartEnvironment.MessageBroker != null)
            {
                SmartPartEnvironment.MessageBroker.UnregisterSubscription(OnProgressChanged);
            }
            SmartPartEnvironment.SmartPartWorkspace.GridSize = _normalGridsize;   
        }

        public override void OnBackgroundProcess(System.ComponentModel.DoWorkEventArgs e)
        {
            base.OnBackgroundProcess(e);

            try
            {
                base.OnBackgroundProcess(e);
                loadData();
            }
            catch (DataException)
            {
                //TODO Log Exception
            }
        }

        public override void AfterBackgroundProcessCompleted()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(AfterBackgroundProcessCompleted));
                return;
            }

            base.AfterBackgroundProcessCompleted();

            _model.LoadPayrollResults(_payrollResults);
        }

        #region IObservable<IPayrollResult> Members

        public void Notify(IPayrollResult item)
        {
            IXPathNavigable navigable;
            using(IUnitOfWork unitOfWork = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
            {
                unitOfWork.Reassociate(item);
                navigable = new XmlDocument {InnerXml = item.XmlResult.XPathNavigable.CreateNavigator().OuterXml};
            }
            DocumentFormat format = DocumentFormat.LoadFromXml(navigable);
            var formatter = PayrollFormatterFactory.CreatePayrollFormatter(format);

            saveFile(navigable, format, formatter);
        }

        private void saveFile(IXPathNavigable navigable, DocumentFormat format, PayrollFormatterBase formatter)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = string.Format(CultureInfo.InvariantCulture, "{0} file|*.{0}|All files (*.*)|*.*",
                                                      formatter.FileSuffix);
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (saveFileDialog.FileName != null)
                    {
                        Stream formattedData = formatter.Format(navigable, format);
                        formattedData.Position = 0;

                        try
                        {
                            using (StreamReader reader = new StreamReader(formattedData, format.Encoding))
                            {
                                using (
                                    StreamWriter writer = new StreamWriter(saveFileDialog.FileName, false,
                                                                           format.Encoding))
                                {
                                    writer.Write(reader.ReadToEnd());
                                }
                            }
                        }
                        catch (IOException)
                        {
                            MessageBoxAdv.Show(new WeakOwner(this), UserTexts.Resources.TheFileIsLockedByAnotherProgram,
                                            UserTexts.Resources.OpenTeleoptiCCC, MessageBoxButtons.OK,
                                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1,
                                            (((ITeleoptiPrincipalForLegacy) TeleoptiPrincipalForLegacy.CurrentPrincipal).UnsafePerson.PermissionInformation.
                                                RightToLeftDisplay)
                                                ? MessageBoxOptions.RtlReading |
                                                  MessageBoxOptions.RightAlign
                                                : 0);

                            saveFile(navigable, format, formatter);
                        }
                    }
                }
            }
        }

        #endregion
    }

    public class WeakOwner : IWin32Window
    {
        private readonly WeakReference _window;

        public WeakOwner(IWin32Window window)
        {
            _window = new WeakReference(window);
        }

        public IntPtr Handle => _window.IsAlive ? ((IWin32Window)_window.Target).Handle : IntPtr.Zero;
	}
}
