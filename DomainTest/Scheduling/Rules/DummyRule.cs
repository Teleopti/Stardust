﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	public class DummyRule : INewBusinessRule
	{
		public DummyRule(bool mandatory)
		{
			IsMandatory = mandatory;
			FriendlyName = string.Empty;
		}

		public string ErrorMessage => string.Empty;

		public bool IsMandatory { get; }

		public bool HaltModify { get; set; }
		public bool Configurable => true;

		public bool ForDelete
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public bool Checked { get; private set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
		{
			Checked = true;
			return new List<IBusinessRuleResponse>
			{
				new BusinessRuleResponse(typeof(DummyRule), "Check Result of DummyRule", true,
					IsMandatory, new DateTimePeriod(), new Person(), new DateOnlyPeriod(), FriendlyName)
			};
		}

		public string FriendlyName { get; }
		public string Description => "Description of DummyRule";
	}

	public class AnotherDummyRule : INewBusinessRule
	{
		public AnotherDummyRule(bool mandatory)
		{
			IsMandatory = mandatory;
			FriendlyName = string.Empty;
		}

		public bool IsMandatory { get; }

		public bool HaltModify { get; set; }
		public bool Configurable => true;

		public bool ForDelete
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public bool Checked { get; private set; }

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
		{
			Checked = true;
			return new List<IBusinessRuleResponse>
			{
				new BusinessRuleResponse(typeof(AnotherDummyRule), "Check Result of AnotherDummyRule", true,
					IsMandatory, new DateTimePeriod(), new Person(), new DateOnlyPeriod(), FriendlyName)
			};
		}

		public string FriendlyName { get; }
		public string Description => "Description of AnotherDummyRule";
	}
}