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
			dates?.ToList().ForEach(d =>
			{
				var _d = _bankHolidayModelMapper.Map(d);
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
			});

		}

		public virtual BankHolidayCalendarViewModel Persist(BankHolidayCalendarForm input)
		{
			var calendar = PersistCalendar(input);
			PersistDates(calendar, input.Dates);
			return _bankHolidayModelMapper.MapModelChanged(calendar, input);
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