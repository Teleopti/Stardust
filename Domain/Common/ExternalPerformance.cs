using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class ExternalPerformance: NonversionedAggregateRootWithBusinessUnit, IExternalPerformance
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

		public bool IsDeleted => _isDeleted;

		public void SetDeleted()
		{
			_isDeleted = true;
		}
	}
}
