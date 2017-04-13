using System;

namespace Teleopti.Ccc.WinCode.Common.ExceptionHandling
{
    public class SimpleExceptionHandlerModel
    {
        public SimpleExceptionHandlerModel(Exception exception, string dialogTitle, string message)
        {
            Exception = exception;
            Title = dialogTitle;
            Message = message;
        }
        
        public Exception Exception { get; private set; }
        
        public string Title { get; private set; }

        public string Message { get; private set; }
    }
}