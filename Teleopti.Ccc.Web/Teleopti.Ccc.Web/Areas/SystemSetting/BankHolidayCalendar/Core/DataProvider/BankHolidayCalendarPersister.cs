using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSettingWeb;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.Mapping;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider
{

	public class BankHolidayCalendarPersister : IBankHolidayCalendarPersister
	{
		private readonly IBankHolidayCalendarRepository _bankHolidayCalendarRepository;
		private readonly IBankHolidayModelMapper _bankHolidayModelMapper;
		private readonly IBankHolidayDateRepository _bankHolidayDateRepository;
		private static readonly ILog logger = LogManager.GetLogger(typeof(BankHolidayCalendarPersister));

		public BankHolidayCalendarPersister(IBankHolidayCalendarRepository bankHolidayCalendarRepository, IBankHolidayModelMapper bankHolidayModelMapper, IBankHolidayDateRepository bankHolidayDateRepository
			)
		{
			_bankHolidayCalendarRepository = bankHolidayCalendarRepository;
			_bankHolidayModelMapper = bankHolidayModelMapper;
			_bankHolidayDateRepository = bankHolidayDateRepository;
		}

		private IBankHolidayCalendar PersistCalendar(BankHolidayCalendarForm input)
		{
			IBankHolidayCalendar calendar;

			if (input.Id.HasValue)
			{
				calendar = _bankHolidayCalendarRepository.Load(input.Id.Value);
				if (!string.IsNullOrWhiteSpace(input.Name))
				{
					calendar.Name = input.Name;
					_bankHolidayCalendarRepository.Add(calendar);
				}
			}
			else
			{
				string name = string.IsNullOrWhiteSpace(input.Name) ? "Unknow Calendar" : input.Name;
				calendar = new Domain.SystemSettingWeb.BankHolidayCalendar(name);
				_bankHolidayCalendarRepository.Add(calendar);
			}

			return calendar;
		}

		private void PersistDates(IBankHolidayCalendar calendar, IEnumerable<BankHolidayDateForm> dates)
		{
			if (dates == null)
				return;
			foreach (var d in dates)
			{
				var _d = CreateBankHolidayDate(calendar, d);
				if (_d == null)
					continue;
				switch (d.Action)
				{
					case BankHolidayDateAction.CREATE:
						calendar.AddDate(_d);
						break;
					case BankHolidayDateAction.DELETE:
						calendar.DeleteDate(_d.Id.Value);
						break;
					case BankHolidayDateAction.UPDATE:
						calendar.UpdateDate(_d);
						break;
				}
				_bankHolidayDateRepository.Add(_d);
			}
		}

		private IBankHolidayDate CreateBankHolidayDate(IBankHolidayCalendar calendar, BankHolidayDateForm input)
		{
			IBankHolidayDate date = null;
			var _date = input.Date;
			var des = input.Description;

			if (input.Id.HasValue)
			{
				date = _bankHolidayDateRepository.Load(input.Id.Value);
				date.Date = _date;
				date.Description = des;
				if (input.IsDeleted)
					date.SetDeleted();
			}
			else
			{
				if (calendar.Dates.ToList().Find(d => d.Date == _date) == null)
					date = new BankHolidayDate() { Date = _date, Description = des };
			}
			return date;
		}

		public virtual BankHolidayCalendarViewModel Persist(BankHolidayCalendarForm input)
		{
			var calendar = PersistCalendar(input);
			PersistDates(calendar, input.Years?.SelectMany(y => y.Dates));
			return _bankHolidayModelMapper.Map(calendar);
		}

		public virtual bool Delete(Guid Id)
		{
			try
			{
				_bankHolidayCalendarRepository.Delete(Id);
			}
			catch (Exception ex)
			{
				logger.Error("Delete Calendar failed.", ex);
				return false;
			}

			return true;
		}

	}
}