﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
  public	class PeriodValueCalculationParameters
	{
	  private readonly WorkShiftLengthHintOption _lengthFactor;
	  private readonly bool _useMinimumPersons;
	  private readonly bool _useMaximumPersons;
	  private readonly MaxSeatsFeatureOptions _maxSeatsFeatureOption;
	  private readonly bool _hasMaxSeatSkill;
	  private readonly IDictionary<DateTime, IntervalLevelMaxSeatInfo> _maxSeatInfo;

	  public PeriodValueCalculationParameters(WorkShiftLengthHintOption lengthFactor, bool useMinimumPersons, bool useMaximumPersons, MaxSeatsFeatureOptions maxSeatsFeatureOption, bool hasMaxSeatSkill, IDictionary<DateTime, IntervalLevelMaxSeatInfo> maxSeatInfo)
	  {
		  _lengthFactor = lengthFactor;
		  _useMinimumPersons = useMinimumPersons;
		  _useMaximumPersons = useMaximumPersons;
		  _maxSeatsFeatureOption = maxSeatsFeatureOption;
		  _hasMaxSeatSkill = hasMaxSeatSkill;
		  _maxSeatInfo = maxSeatInfo;
	  }

	  public WorkShiftLengthHintOption LengthFactor
	  {
		  get { return _lengthFactor; }
	  }

	  public bool UseMinimumPersons
	  {
		  get { return _useMinimumPersons; }
	  }

	  public bool UseMaximumPersons
	  {
		  get { return _useMaximumPersons; }
	  }

	  public MaxSeatsFeatureOptions MaxSeatsFeatureOption
	  {
		  get { return _maxSeatsFeatureOption; }
	  }

	  public IDictionary<DateTime, IntervalLevelMaxSeatInfo> MaxSeatInfoPerInterval { get { return _maxSeatInfo; } }

	  public bool HasMaxSeatSkill
	  {
		  get { return _hasMaxSeatSkill;}
	  }

	}
}
