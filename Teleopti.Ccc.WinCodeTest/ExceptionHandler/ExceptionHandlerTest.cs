using System;
using System.Collections;
using System.Data.SqlClient;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.ExceptionHandling;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
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

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IExceptionHandlerView>();
            _mapi = _mocks.StrictMock<IMapiMailMessage>();
            _fileWriter = _mocks.StrictMock<IWriteToFile>(); 
            _model = new ExceptionHandlerModel(_exception, "hej", _mapi, _fileWriter);
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
                _view.FormHeaderText("Teleopti CCC");
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
            }
        }

        [Test, RequiresSTA]//, Ignore("Clipboard should be injected")]
        public void CanCopyToClipboard()
        {
            string expectedString = _model.CompleteStackAndAssemblyText();
            Clipboard.Clear();
            _target.CopyToClipboard();
            StringAssert.AreEqualIgnoringCase(expectedString, Clipboard.GetDataObject().GetData(DataFormats.Text).ToString());
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
        public void ShouldGetAllExceptionsIfSqlExceptionException()
        {
            var createSqlException = SqlExceptionCreator.CreateSqlException("Timeout", 123);
            var model = new ExceptionHandlerModel(createSqlException, "", _mapi, _fileWriter);
            var expectedString = model.CompleteStackAndAssemblyText();
            expectedString.Should().StartWith("System.Data.SqlClient.SqlError: Timeout\nSystem.Data.SqlClient.SqlError: Timeout2");
        }
    }

    /// <summary>
    /// Class for creating SQL-Exceptions
    /// </summary>
    public static class SqlExceptionCreator
    {
        public static SqlException CreateSqlException(string errorMessage, int errorNumber)
        {
            var collection = GetErrorCollection();
            var error = GetError(errorNumber, errorMessage);
            var error2 = GetError(errorNumber+1, errorMessage+"2");

            var addMethod = collection.GetType().GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance);
            addMethod.Invoke(collection, new object[] { error });
            addMethod.Invoke(collection, new object[] { error2 });

            var types = new[] { typeof(string), typeof(SqlErrorCollection) };
            var parameters = new object[] { errorMessage, collection };

            var constructor = typeof(SqlException).
                GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);

            var exception = (SqlException)constructor.Invoke(parameters);

            return exception;
        }

        private static SqlError GetError(int errorCode, string message)
        {
            var parameters = new object[] {
            errorCode, (byte)0, (byte)10, "server", message, "procedure", 0 };
            var types = new[] {
            typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string),
            typeof(string), typeof(int) };

            var constructor = typeof(SqlError).
                GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);
            var error = (SqlError)constructor.Invoke(parameters);
            return error;
        }

        private static SqlErrorCollection GetErrorCollection()
        {
            var constructor = typeof(SqlErrorCollection).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
            var collection = (SqlErrorCollection) constructor.Invoke(new object[] {});
            return collection;
        }
    }
}
