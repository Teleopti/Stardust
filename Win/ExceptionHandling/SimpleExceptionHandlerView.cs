using System;
using System.Data.SqlClient;
using log4net;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Common.ExceptionHandling;

namespace Teleopti.Ccc.Win.ExceptionHandling
{
    public partial class SimpleExceptionHandlerView : BaseRibbonForm, ISimpleExceptionHandlerView
    {
        private readonly SimpleExceptionHandlerPresenter _presenter;
        private readonly ILog _log = LogManager.GetLogger(typeof (SimpleExceptionHandlerView));

        public SimpleExceptionHandlerView(Exception exception, string dialogTitle, string message)
            : this()
        {
            IClipboardOperationsForText clipboardHandler = new ClipboardOperationsForText();
            var model = new SimpleExceptionHandlerModel(exception, dialogTitle, message);
            _presenter = new SimpleExceptionHandlerPresenter(this, model, clipboardHandler);
            _presenter.Initialize();
            _log.Error(message, exception);

			if(exception is DataSourceException)
				SqlConnection.ClearAllPools();
        }

        public SimpleExceptionHandlerView()
        {
            InitializeComponent();
            if (!DesignMode)
                SetTexts();
        }

        public void SetTitle(string title)
        {
            Text = title;
        }

        public void SetMessage(string message)
        {
            labelInformationText.Text = message;
        }

        private void buttonCopyErrorDetails_Click(object sender, EventArgs e)
        {
            _presenter.CopyErrorDetails();
        }
    }
}
