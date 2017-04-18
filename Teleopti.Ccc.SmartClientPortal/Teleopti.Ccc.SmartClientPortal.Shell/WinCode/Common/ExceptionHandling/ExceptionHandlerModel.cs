using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ExceptionHandling
{
    public class ExceptionHandlerModel
    {
        private readonly Exception _exception;
        private readonly string _defaultEmail;
        private readonly IMapiMailMessage _mapiMessage;
        private readonly IWriteToFile _fileWriter;
	    private readonly ExceptionMessageBuilder _exceptionMessageBuilder;

		public ExceptionHandlerModel(Exception exception, string defaultEmail, IMapiMailMessage mapiMessage, IWriteToFile fileWriter, ExceptionMessageBuilder exceptionMessageBuilder)
        {
			_exceptionMessageBuilder = exceptionMessageBuilder;
			_exception = exception;
            _defaultEmail = defaultEmail;
            _mapiMessage = mapiMessage;
            _fileWriter = fileWriter;
        }

        public static string InformationText
        {
            get { return Resources.UnhandledExceptionMessage; }
        }

        public Exception Exception
        {
            get { return _exception; }
        }
        public static string ButtonCloseApplicationText
        {
            get { return Resources.Close; }
        }
        public static string LinkCopyToClipboardText
        {
            get { return Resources.CopyErrorMessageToClipboard; }
        }
        public static string ButtonSendEmailText
        {
            get { return Resources.SendEmailReport; }
        }
        public static string CheckBoxIncludeScreenshotText
        {
            get { return Resources.IncludeScreenShot; }
        }
        public static string FormHeaderText
        {
            get { return "Teleopti WFM"; }
        }
        public static Color FormColor
        {
            get { return ColorHelper.DialogBackColor(); }
        }
        public string DefaultEmail
        {
            get
            {
                return _defaultEmail;
            }
        }

        public static string EmailBody
        {
            get{return Resources.UnhandledExceptionEmailBody;}
        }

        public IMapiMailMessage MapiMessage
        {
            get { return _mapiMessage; }
        }

        public IWriteToFile FileWriter
        {
            get { return _fileWriter; }
        }
        
        public static string CannotWriteToFileMessage(string error)
        {
            return string.Concat(Resources.AFileContainingTheErrorMessageCouldNotBeCreated, " ", error);
        }

	    public string CompleteStackAndAssemblyText()
	    {
		    return _exceptionMessageBuilder.BuildCompleteExceptionMessage();
	    }
    }
}
