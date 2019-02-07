using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class ExternalPerformance: AggregateRoot_Events_ChangeInfo_BusinessUnit, IExternalPerformance
	{
		private int _externalId;
		private string _name;
		private ExternalPerformanceDataType _dataType;
		private bool _isDeleted;

		public virtual int ExternalId
		{
			get => _externalId;
			set => _externalId = value;
		}

		public virtual string Name
		{
			get => _name;
			set => _name = value;
		}

		public virtual ExternalPerformanceDataType DataType
		{
			get => _dataType;
			set => _dataType = value;
		}

		public virtual bool IsDeleted
		{
			get => _isDeleted;
		}

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}
	}
}
