using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Insights
{
	public class InsightsReport : AggregateRoot_Events_ChangeInfo_Versioned, IInsightsReport
	{
		private string _name;
		private bool _isDeleted;
		private bool _isBuildIn;
		private IPerson _createdBy;
		private DateTime? _createdOn;

		public virtual bool IsDeleted => _isDeleted;
		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}

		public virtual IPerson CreatedBy
		{
			get { return _createdBy; }
			set { _createdBy = value; }
		}
		public virtual DateTime? CreatedOn
		{
			get { return _createdOn; }
			set { _createdOn = value; }
		}

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public virtual bool IsBuildIn
		{
			get { return _isBuildIn; }
			set { _isBuildIn = value; }
		}
	}
}
