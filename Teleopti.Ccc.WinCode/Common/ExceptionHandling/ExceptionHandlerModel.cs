using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Reflection;
using System.Text;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common.ExceptionHandling
{
    public class ExceptionHandlerModel
    {
        private Exception _exception;
        private string _defaultEmail;
        private IMapiMailMessage _mapiMessage;
        private IWriteToFile _fileWriter;
	    private ITogglesActive _allToggles;
	    public const string ToggleFeaturesUnknown = "ToggleFeatures unknown";

		public ExceptionHandlerModel(Exception exception, string defaultEmail, IMapiMailMessage mapiMessage, IWriteToFile fileWriter, ITogglesActive allToggles)
        {
			_allToggles = allToggles;
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
	        var text = new StringBuilder();

			//Note: SQL exceptions are different, we need to loop the Error collection
			//to get all the information.
			var sqlException = _exception as SqlException;
	        if (sqlException == null)
	        {
		        text.Append(_exception);
	        }
	        else
	        {
		        appendSqlException(text, sqlException);
	        }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                text.AppendLine(assembly.ToString());
            }

	        appendVersionInfo(text);
			appendFeatureInfo(text, _allToggles);

	        return text.ToString();
        }

	    private static void appendVersionInfo(StringBuilder text)
	    {
			var customAttribute =
				Attribute.GetCustomAttribute(typeof(ExceptionHandlerModel).Assembly,
					typeof(AssemblyInformationalVersionAttribute), false) as AssemblyInformationalVersionAttribute;
		    if (customAttribute != null)
		    {
			    text.AppendLine();
			    text.Append("Product Version: ");
			    text.Append(customAttribute.InformationalVersion);
			    text.AppendLine();
			    text.AppendLine();
		    }
	    }

	    private static void appendFeatureInfo(StringBuilder text, ITogglesActive allToggles)
	    {
			text.AppendLine();
			text.AppendLine();
			try
		    {

				foreach (var entry in allToggles.AllActiveToggles())
			    {
				    text.AppendLine(string.Format("{0} = {1} ", entry.Key, entry.Value));
			    }
		    }
		    catch (Exception)
		    {
				text.AppendLine(ToggleFeaturesUnknown);
		    }
	    }

        private static void appendSqlException(StringBuilder text, SqlException sqlException)
        {
	        foreach (SqlError error in sqlException.Errors)
	        {
		        text.AppendLine(error.ToString());
	        }
        }
    }
}
