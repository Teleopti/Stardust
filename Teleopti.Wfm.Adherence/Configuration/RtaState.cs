using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.Configuration
{
	public class RtaState : Entity, IRtaState
	{
		private string _stateCode;
		private string _name;
		private Guid? _businessUnit;
		private IEntity _parent;

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

		public virtual Guid? BusinessUnit
		{
			get { return _businessUnit; }
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

		public virtual IRtaStateGroup Parent
		{
			get { return _parent as IRtaStateGroup; }
			set { _parent = value; }
		}
	}
}