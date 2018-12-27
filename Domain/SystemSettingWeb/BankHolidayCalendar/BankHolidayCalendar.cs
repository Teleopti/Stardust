﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SystemSettingWeb
{
	public class BankHolidayCalendar : NonversionedAggregateRootWithBusinessUnit, IBankHolidayCalendar, IAggregateRootWithEvents
	{

		protected BankHolidayCalendar()
		{
			_dates = new List<IBankHolidayDate>();
		}

		public BankHolidayCalendar(string name)
			: this()
		{
			_name = name;
		}

		private bool _isDeleted;

		public virtual bool IsDeleted
		{
			get { return _isDeleted; }
			set { _isDeleted = value; }
		}

		public virtual void AddDate(IBankHolidayDate date)
		{
			InParameter.NotNull(nameof(date), date);
			if (!_dates.Contains(date))
			{
				_dates.Add(date);
			}
		}

		public virtual void DeleteDate(Guid Id)
		{
			InParameter.NotNull(nameof(Id), Id);
			IBankHolidayDate date;
			if ((date = _dates.ToList().Find(d => d.Id.Value == Id)) != null)
			{
				date.SetDeleted();
			}
		}

		public virtual void UpdateDate(IBankHolidayDate date)
		{
			InParameter.NotNull(nameof(date), date);
			IBankHolidayDate _date;
			if ((_date = _dates.ToList().Find(d => d.Id.Value == date.Id.Value)) != null)
			{
				_dates.Remove(_date);
				_date.Date = date.Date;
				_date.Description = date.Description;
				_dates.Add(_date);
			}
		}

		private string _name;

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		private readonly IList<IBankHolidayDate> _dates;

		public virtual ReadOnlyCollection<IBankHolidayDate> Dates => new ReadOnlyCollection<IBankHolidayDate>(_dates.Where(d=>!d.IsDeleted).OrderBy(d => d.Date).ToList());
	}
}
