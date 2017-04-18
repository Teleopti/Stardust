using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.ExceptionHandler
{
    [TestFixture]
    public class ExceptionHandlerTest
    {
        private MockRepository _mocks;
        private IExceptionHandlerView _view;
        InvalidCastException _exception = new InvalidCastException("Väldigt läskigt fel", new Exception("Väldigt läskigt Inner-Exception"));
        private IWriteToFile _fileWriter;
        private IMapiMailMessage _mapi;
        private ExceptionHandlerModel _model;
        private ExceptionHandlerPresenter _target;
	    private ExceptionMessageBuilder _exceptionMessageBuilder;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IExceptionHandlerView>();
            _mapi = _mocks.StrictMock<IMapiMailMessage>();
            _fileWriter = _mocks.StrictMock<IWriteToFile>(); 
			_exceptionMessageBuilder = new ExceptionMessageBuilder(_exception, null);
			_model = new ExceptionHandlerModel(_exception, "hej", _mapi, _fileWriter, _exceptionMessageBuilder);
            _target = new ExceptionHandlerPresenter(_view, _model);
        }

        [Test]
        public void CanInitializePresenter()
        {
            using (_mocks.Record())
            {
                _view.InformationText = Resources.UnhandledExceptionMessage;
                _view.ButtonSendEmailText(Resources.SendEmailReport);
                _view.ButtonCloseApplicationText(Resources.Close);
                _view.LinkCopyToClipboardText(Resources.CopyErrorMessageToClipboard);
                _view.CheckBoxIncludeScreenshotText(Resources.IncludeScreenShot);
                _view.SetColors(ColorHelper.DialogBackColor());
                _view.FormHeaderText("Teleopti WFM");
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
            }
        }

        [Test, Apartment(ApartmentState.STA)]//, Ignore("Clipboard should be injected")]
        public void CanCopyToClipboard()
        {
            var completeStackTrace = _model.CompleteStackAndAssemblyText();
			completeStackTrace.Should().Contain("Timestamp: ");
			var expectedWithTimeStampRemoved = removeTimeStamp(completeStackTrace);

            Clipboard.Clear();
            _target.CopyToClipboard();
	        var clipboardStackTrace = Clipboard.GetDataObject().GetData(DataFormats.Text).ToString();
			var expectedClipboardStackTraceWithTimeStampRemoved = removeTimeStamp(clipboardStackTrace);

			StringAssert.AreEqualIgnoringCase(expectedWithTimeStampRemoved, expectedClipboardStackTraceWithTimeStampRemoved);
        }

	    private string removeTimeStamp(string text)
	    {
		    var lines = Regex.Split(text, "\r\n|\r\n").Skip(1);
		    return string.Join(Environment.NewLine, lines.ToArray());
	    }

        [Test]
        public void CanPopEmail()
        {
            string email = "mammamu@teleopti.com";
            StringSetting stringSetting = new StringSetting();
            stringSetting.StringValue = email;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(_model.Exception.Source);

            using (_mocks.Record())
            {
                Expect.Call(()=>_target.WriteToFile("path")).IgnoreArguments();
                _mapi.Body = ExceptionHandlerModel.EmailBody;
                _mapi.Subject = _model.Exception.Message;
                Expect.Call(_mapi.Files).Return(new ArrayList());
                Expect.Call(()=>_mapi.CreateMessage(string.Empty, new ArrayList())).IgnoreArguments();
                Expect.Call(_view.IncludeScreenshot()).Return(true);
                Expect.Call(_view.ScreenRectangle()).Return(new Rectangle(0, 20, 40, 40)).Repeat.AtLeastOnce();
                Expect.Call(() => _view.ScreenshotFromImage(new Bitmap(10, 60))).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target.PopEmail();
            }
        }
        [Test]
        public void CanExitApplication()
        {
            using (_mocks.Record())
            {
                _view.Close();
                _view.SetDialogResult(DialogResult.OK);
            }

            using (_mocks.Playback())
            {
                _target.Close();
            }
        }
        [Test]
        public void CanGetIncludeScreenshot()
        {
            using (_mocks.Record())
            {
                Expect.Call(_view.ScreenRectangle()).Return(new Rectangle(0, 20, 40, 40)).Repeat.AtLeastOnce();
                Expect.Call(() => _view.ScreenshotFromImage(new Bitmap(10, 60))).IgnoreArguments();
            }

            using (_mocks.Playback())
            {
                _target.CreateScreenshot(string.Empty);
            }
        }
        [Test]
        public void CanGetIncludeScreenshotCheckState()
        {
            Assert.IsFalse(_view.IncludeScreenshot());
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void CanGetCannotSaveFileText()
        {
            Assert.IsTrue(ExceptionHandlerModel.CannotWriteToFileMessage("Error").Length> 10);
        }

        [Test]
        public void ShouldGetProductVersionFromCurrentAssembly()
        {
            var sqlException = SqlExceptionConstructor.CreateSqlException("Timeout", 123);
			var exceptionMessageBuilder = new ExceptionMessageBuilder(sqlException, null);
			var model = new ExceptionHandlerModel(sqlException, "", _mapi, _fileWriter, exceptionMessageBuilder);
            var expectedString = model.CompleteStackAndAssemblyText();

			expectedString.Should().Contain("Product Version: " + Application.ProductVersion);
        }

		[Test]
	    public void ShouldGetTimeStamp()
		{
			var exceptionText = _model.CompleteStackAndAssemblyText();
			exceptionText.Should().Contain("Timestamp: ");
		}

		[Test]
		public void ShouldGetAllExceptionsIfSqlExceptionException()
		{
			var sqlException = SqlExceptionConstructor.CreateSqlException("Timeout", 123);
			var exceptionMessageBuilder = new ExceptionMessageBuilder(sqlException, null);
			var model = new ExceptionHandlerModel(sqlException, "", _mapi, _fileWriter, exceptionMessageBuilder);
			var expectedString = model.CompleteStackAndAssemblyText();
			expectedString.Should().Contain("System.Data.SqlClient.SqlError: Timeout\r\nSystem.Data.SqlClient.SqlError: Timeout");
		}

		[Test]
		public void CompleteStackAndAssemblyText_Always_ShouldIncludeEnabledToggleFeatures()
		{
			var features = new Dictionary<Toggles, bool>
			               {
				               {Toggles.TestToggle, true}
			               };
			var allToggleFeatures = new ActiveTogglesStub(features);
			var exceptionMessageBuilder = new ExceptionMessageBuilder(_exception, allToggleFeatures);

			var model = new ExceptionHandlerModel(SqlExceptionConstructor.CreateSqlException("Any Exception will do", 123), "", _mapi, _fileWriter, exceptionMessageBuilder);
			var expectedString = model.CompleteStackAndAssemblyText();

			expectedString.Should().Contain(string.Format("{0} = {1}", Toggles.TestToggle, features[Toggles.TestToggle]));
		}

		[Test]
		public void CompleteStackAndAssemblyText_Always_ShouldIncludeDisabledToggleFeatures()
		{
			var features = new Dictionary<Toggles, bool>
			               {
				               {Toggles.TestToggle, false}
			               };
			var allToggleFeatures = new ActiveTogglesStub(features);
			var exceptionMessageBuilder = new ExceptionMessageBuilder(_exception, allToggleFeatures);

			var model = new ExceptionHandlerModel(SqlExceptionConstructor.CreateSqlException("Any Exception will do", 123), "", _mapi, _fileWriter, exceptionMessageBuilder);
			var expectedString = model.CompleteStackAndAssemblyText();

			expectedString.Should().Contain(string.Format("{0} = {1}", Toggles.TestToggle, features[Toggles.TestToggle]));
		}

	    [Test]
	    public void CompleteStackAndAssemblyText_WhenUnableToReadDataFromToggleService_ShouldShowExceptionAndInfoAboutUnknownToggles()
	    {
		    const string exceptionInfo = "System.Data.SqlClient.SqlError";
			var allToggleFeatures = new ActiveTogglesThatThrowsWhenTryingToReadFeatures();
			
		    var sqlException = SqlExceptionConstructor.CreateSqlException(exceptionInfo, 123);
			var exceptionMessageBuilder = new ExceptionMessageBuilder(sqlException, allToggleFeatures);
			var model = new ExceptionHandlerModel(sqlException, "", _mapi, _fileWriter, exceptionMessageBuilder);
			var expectedString = model.CompleteStackAndAssemblyText();

			expectedString.Should().Contain(exceptionInfo);
		    expectedString.Should().Contain(ExceptionMessageBuilder.ToggleFeaturesUnknown);
	    }

	    [Test]
	    public void ShouldReturnInnerExceptionChain()
	    {
			var allToggleFeatures = new ActiveTogglesThatThrowsWhenTryingToReadFeatures();
			var exception = new InvalidCastException("outer", new Exception("inner1", new Exception("inner2")));
		    string expectedString = string.Empty;
		    try
		    {
			    throw exception;
		    }
		    catch (Exception)
		    {
			    var exceptionMessageBuilder = new ExceptionMessageBuilder(exception, allToggleFeatures);
			    var model = new ExceptionHandlerModel(exception, "", _mapi, _fileWriter, exceptionMessageBuilder);
			    expectedString = model.CompleteStackAndAssemblyText();
		    }
		    finally
		    {
				expectedString.Should().Contain("outer");
				expectedString.Should().Contain("inner1");
				expectedString.Should().Contain("inner2");
		    }

	    }

		public class ActiveTogglesStub : ITogglesActive
		{
			private readonly IDictionary<Toggles, bool> _features;

			public ActiveTogglesStub(IDictionary<Toggles, bool> features)
			{
				_features = features;
			}

			public IDictionary<Toggles, bool> AllActiveToggles()
			{
				return _features;
			}
		}

		public class ActiveTogglesThatThrowsWhenTryingToReadFeatures : ITogglesActive
		{
			public IDictionary<Toggles, bool> AllActiveToggles()
			{
				throw new NotImplementedException();
			}
		}
    }
    
}
