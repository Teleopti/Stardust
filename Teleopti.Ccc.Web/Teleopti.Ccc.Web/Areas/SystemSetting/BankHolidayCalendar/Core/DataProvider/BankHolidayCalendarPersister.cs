using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
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
				calendar = new Domain.SystemSettingWeb.BankHolidayCalendar();
				calendar.Name = name;
				_bankHolidayCalendarRepository.Add(calendar);
			}

			return calendar;
		}

		private List<IBankHolidayDate> PersistDates(IBankHolidayCalendar calendar, IEnumerable<BankHolidayDateForm> dates)
		{
			var _dates = _bankHolidayDateRepository.LoadAll().Where(d => d.Calendar.Id.Value == calendar.Id.Value).ToList();

			if (dates == null)
				return _dates;

			foreach (var d in dates)
			{
				var _d = CreateBankHolidayDate(calendar, d);
				if (_d == null)
					continue;
				_bankHolidayDateRepository.Add(_d);
				if (!_dates.Contains(_d))
					_dates.Add(_d);
			}

			return _dates;
		}

		private IBankHolidayDate CreateBankHolidayDate(IBankHolidayCalendar calendar, BankHolidayDateForm input)
		{
			IBankHolidayDate date;
			var _date = input.Date.Date;
			var des = input.Description;

			if (input.Id.HasValue)
			{
				date = _bankHolidayDateRepository.Load(input.Id.Value);
				date.Date = _date;
				date.Description = des;
				date.Calendar = calendar;
				if (input.IsDeleted)
					date.SetDeleted();
			}
			else if (_bankHolidayDateRepository.LoadAll().Where(d => d.Calendar.Id.Value == calendar.Id.Value && d.Date == _date && !d.IsDeleted).FirstOrDefault() == null)
			{
				date = new BankHolidayDate() { Date = _date, Description = des, Calendar = calendar };
			}
			else
			{
				date = null;
			}
			return date;
		}

		public virtual BankHolidayCalendarViewModel Persist(BankHolidayCalendarForm input)
		{
			var calendar = PersistCalendar(input);
			var dates = PersistDates(calendar, input.Years?.SelectMany(y => y.Dates));
			return _bankHolidayModelMapper.Map(calendar, dates);
		}

		public virtual bool Delete(Guid Id)
		{
			try
			{
				var calendar = _bankHolidayCalendarRepository.Load(Id);
				_bankHolidayCalendarRepository.Remove(calendar);
				_bankHolidayDateRepository.LoadAll().Where(d => d.Calendar.Id.Value == Id)?.ToList().ForEach(d => _bankHolidayDateRepository.Remove(d));
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