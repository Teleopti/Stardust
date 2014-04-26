using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Reflection;
using System.Text;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
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
            get { return "Teleopti CCC"; }
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
            string asseblyString = string.Empty;
	        string exceptionInfoText;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                asseblyString += assembly.ToString();
            }

            //Note: SQL exceptions are different, we need to loop the Error collection
            //to get all the information.
            var sqlException = _exception as SqlException;

	        if (sqlException == null)
	        {
				exceptionInfoText = string.Concat(_exception.ToString(), asseblyString);   
	        }
	        else
	        {
		        exceptionInfoText = extractSqlException(asseblyString, sqlException);
	        }


			return extractFeatureInfo(exceptionInfoText, _allToggles);
        }

		private static string extractFeatureInfo(string exceptionText, ITogglesActive allToggles)
	    {
			var text = new StringBuilder(exceptionText);
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
			return text.ToString();

	    }

        private static string extractSqlException(string asseblyString, SqlException sqlException)
        {
            var errString = string.Empty;
            
            foreach (SqlError err in sqlException.Errors)
            {
                errString += err + "\n";
            }
            
            return string.Concat(errString, asseblyString);
        }
    }
}
