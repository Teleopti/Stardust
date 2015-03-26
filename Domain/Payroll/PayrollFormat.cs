using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Payroll
{
	public interface IPayrollFormat: IAggregateRoot
	{
		string Name { get; set; }
		Guid FormatId { get; set; }
	}
	public class PayrollFormat :  IPayrollFormat
	{
		private string _name;
		private Guid? _id;
		private Guid _formatId;

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public virtual Guid FormatId
		{
			get { return _formatId; }
			set { _formatId = value; }
		}

		public virtual bool Equals(IEntity other)
		{
			if (other == null)
				return false;
			if (this == other)
				return true;
			if (!other.Id.HasValue || !Id.HasValue)
				return false;

			return (Id.Value == other.Id.Value);

		}

		public virtual Guid? Id
		{
			get { return _id; }
		}

		public virtual void SetId(Guid? newId)
		{
			if (newId.HasValue)
			{
				_id = newId;
			}
			else
			{
				ClearId();
			}
		}

		public virtual void ClearId()
		{
			_id = null;
		}
	}
}