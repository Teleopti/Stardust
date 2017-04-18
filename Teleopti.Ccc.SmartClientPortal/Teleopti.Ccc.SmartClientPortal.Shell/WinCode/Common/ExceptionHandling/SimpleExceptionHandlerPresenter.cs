namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ExceptionHandling
{
    public class SimpleExceptionHandlerPresenter
    {
        private readonly ISimpleExceptionHandlerView _view;
        private readonly SimpleExceptionHandlerModel _model;
        private readonly IClipboardOperationsForText _clipboardHandler;

        public SimpleExceptionHandlerPresenter(ISimpleExceptionHandlerView view, SimpleExceptionHandlerModel model, IClipboardOperationsForText clipboardHandler)
        {
            _view = view;
            _model = model;
            _clipboardHandler = clipboardHandler;
        }

        public void Initialize()
        {
            _view.SetTitle(_model.Title);
            _view.SetMessage(_model.Message);
        }

        public void CopyErrorDetails()
        {
            _clipboardHandler.Copy(_model.Exception.ToString());
        }
    }
}