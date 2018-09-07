using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public class ExternalLogonReadModel
	{
		public Guid PersonId { get; set; }
		public int? DataSourceId { get; set; }
		public string UserCode { get; set; }

		public bool Deleted { get; set; }
		public bool Added { get; set; }
	}

	public interface IExternalLogonReader
	{
		IEnumerable<ExternalLogonReadModel> Read();
	}

	public interface IExternalLogonReadModelPersister
	{
		void Delete(Guid personId);
		void Add(ExternalLogonReadModel model);
		void Refresh();
	}
}