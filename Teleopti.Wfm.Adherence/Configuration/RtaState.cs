using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Configuration
{
	public class RtaState : AggregateEntity, IRtaState
	{
		private string _stateCode;
		private string _name;
		private Guid? _businessUnit;

		protected RtaState()
		{
		}

		protected internal RtaState(string stateCode)
			: this()
		{
			_name = stateCode;
			_stateCode = stateCode;
		}

		protected internal RtaState(string name, string stateCode)
			: this()
		{
			_name = name;
			_stateCode = stateCode;
		}

		protected virtual Guid? BusinessUnit
		{
			get { return _businessUnit ?? (_businessUnit = StateGroup.BusinessUnit); }
			set => _businessUnit = value;
		}

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public virtual string StateCode
		{
			get { return _stateCode; }
			set { _stateCode = value; }
		}

		public virtual IRtaStateGroup StateGroup
		{
			get { return Parent as IRtaStateGroup; }
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}

		public virtual IRtaState NoneEntityClone()
		{
			var clone = (IRtaState) MemberwiseClone();
			clone.SetId(null);
			return clone;
		}

		public virtual IRtaState EntityClone()
		{
			return (IRtaState) MemberwiseClone();
		}
	}
}