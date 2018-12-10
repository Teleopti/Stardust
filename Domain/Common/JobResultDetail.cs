using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
    public class JobResultDetail : AggregateEntity, IJobResultDetail
    {
        private readonly DetailLevel _detailLevel;
        private readonly string _message;
        private string _exceptionMessage;
        private string _exceptionStackTrace;
        private string _innerExceptionMessage;
        private string _innerExceptionStackTrace;
        private readonly DateTime _timestamp;

        public JobResultDetail(DetailLevel detailLevel, string message, DateTime timestamp, Exception exception) : this()
        {
            _detailLevel = detailLevel;
            _message = message;
            _timestamp = timestamp;
            if (HasException(exception))
            {
                ExtractException(exception);
            }
        }

        protected JobResultDetail()
        {
        }

        private static bool HasException(Exception exception)
        {
            return exception != null;
        }

        public virtual DateTime Timestamp
        {
            get { return _timestamp; }
        }

        public virtual string InnerExceptionStackTrace
        {
            get { return _innerExceptionStackTrace; }
        }

        public virtual string InnerExceptionMessage
        {
            get { return _innerExceptionMessage; }
        }

        public virtual string ExceptionStackTrace
        {
            get { return _exceptionStackTrace; }
        }

        public virtual string ExceptionMessage
        {
            get { return _exceptionMessage; }
        }

        public virtual string Message
        {
            get { return _message; }
        }

        public virtual DetailLevel DetailLevel
        {
            get { return _detailLevel; }
        }

        private void ExtractException(Exception exception)
        {
            _exceptionMessage = exception.Message;
            _exceptionStackTrace = exception.StackTrace;
            if (HasInnerException(exception))
            {
                _innerExceptionMessage = exception.InnerException.Message;
                _innerExceptionStackTrace = exception.InnerException.StackTrace;
            } 
        }

        private static bool HasInnerException(Exception exception)
        {
            return exception.InnerException!=null;
        }
    }
}