using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ExceptionHandling;

namespace Teleopti.Ccc.WinCodeTest.ExceptionHandler
{
    [TestFixture]
    public class SimpleExceptionHandlerPresenterTest
    {
        private SimpleExceptionHandlerPresenter _target;
        private MockRepository _mocks;
        private ISimpleExceptionHandlerView _view;
        private readonly InvalidCastException _exception = new InvalidCastException("Really scary exception!");
        private SimpleExceptionHandlerModel _model;
        IClipboardOperationsForText _clipboardHandler;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<ISimpleExceptionHandlerView>();
            _clipboardHandler = _mocks.StrictMock<IClipboardOperationsForText>();
            _model = new SimpleExceptionHandlerModel(_exception, "WindowTitle", "This message should calm the panicked user.");

            _target = new SimpleExceptionHandlerPresenter(_view, _model, _clipboardHandler);
        }

        [Test]
        public void ShouldSetTextsForView()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _view.SetTitle(_model.Title));
                Expect.Call(() => _view.SetMessage(_model.Message));
            }

            using (_mocks.Playback())
            {
                _target.Initialize();
            }
        }

        [Test]
        public void ShouldCopyErrorDetails()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _clipboardHandler.Copy(_exception.ToString()));
            }

            using (_mocks.Playback())
            {
                _target.CopyErrorDetails();
            }
        }
    }
}
