using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Core;
using log4net;

namespace Teleopti.Messaging.DataAccessLayer
{
    public class FilterDeleter : ObjectDeleter
    {
		private static ILog Logger = LogManager.GetLogger(typeof(FilterDeleter));
        private const string FilterIdParameter = "@FilterId";
        private const string DeleteFilterById = "msg.sp_Filter_Delete";

        public FilterDeleter(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Deletes a filter from the database.
        /// </summary>
        /// <param name="filterId"></param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void UnregisterFilter(Guid filterId)
        {            
            try 
            {
                DeleteAddedRecord(DeleteFilterById, FilterIdParameter, filterId);
            }
            catch (Exception exception)
            {
                Logger.Error("Filter deleter error.", exception);
            }
        }
    }
}
