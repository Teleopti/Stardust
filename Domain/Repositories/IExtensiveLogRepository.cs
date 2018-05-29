using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Repositories
{
	public class ExtensiveLog
	{
		public Guid Id { get; set; }
		public Guid? ObjectId { get; set; }
		public Guid? Person { get; set; }
		public Guid? BusinessUnit { get; set; }
		public DateTime UpdatedOn { get; set; }
		public string RawData { get; set; }
		/// <summary>
		/// It could be the name of the class to indicate what is the type of the log 
		/// </summary>
		public string EntityType { get; set; }

		public string IpAddress { get; set; }
		public string HostName { get; set; }
	}

	public interface IExtensiveLogRepository
	{
		void Add(object obj, Guid objId, string entityType);
		IList<ExtensiveLog> LoadAll();
	}
}