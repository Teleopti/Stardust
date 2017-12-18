using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IExternalPerformanceData : IAggregateRoot
	{
		IExternalPerformance ExternalPerformance { get; set; }
		DateTime DateFrom { get; set; }
		Guid PersonId { get; set; }
		string OriginalPersonId { get; set; }
		int Score { get; set; }
	}

	public class ExternalPerformanceData : SimpleAggregateRoot, IExternalPerformanceData
	{
		private IExternalPerformance _externalPerformance;

		private Guid _personId;
		private DateTime _dateFrom;
		private string _originalPersonId;
		private int _score;

		public virtual IExternalPerformance ExternalPerformance
		{
			get { return _externalPerformance; }
			set
			{
				_externalPerformance = value;
			}
		}
		public virtual DateTime DateFrom
		{
			get
			{
				return _dateFrom;
			}
			set
			{
				_dateFrom = value;
			}
		}
		public virtual Guid PersonId
		{
			get
			{
				return _personId;
			}
			set
			{

				_personId = value;
			}
		}
		public virtual string OriginalPersonId { get { return _originalPersonId; } set { _originalPersonId = value; } }
		public virtual int Score { get { return _score; } set { _score = value; } }
	}
}
