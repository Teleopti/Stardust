using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.WinCode.FileImport
{
    [Serializable]
    public class FileImportException : Exception
    {
        public FileImportException()
        {
        }

        public FileImportException(string message)
            : base(message)
        {
        }

        public FileImportException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected FileImportException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}