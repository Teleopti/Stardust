﻿using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.BankHolidayCalendar;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.Mapping;
using Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Models;

namespace Teleopti.Ccc.Web.Areas.SystemSetting.BankHolidayCalendar.Core.DataProvider
{

	public class BankHolidayCalendarPersister : IBankHolidayCalendarPersister
	{
		private readonly IBankHolidayCalendarRepository _bankHolidayCalendarRepository;
		private readonly IBankHolidayModelMapper _bankHolidayModelMapper;
		private readonly IBankHolidayDateRepository _bankHolidayDateRepository;
		private readonly IBankHolidayCalendarSiteRepository _bankHolidayCalendarSiteRepository;
		private readonly IBankHolidayCalendarSitePersister _bankHolidayCalendarSitePersister;
		private static readonly ILog logger = LogManager.GetLogger(typeof(BankHolidayCalendarPersister));

		public BankHolidayCalendarPersister(IBankHolidayCalendarRepository bankHolidayCalendarRepository, IBankHolidayModelMapper bankHolidayModelMapper, IBankHolidayDateRepository bankHolidayDateRepository, IBankHolidayCalendarSiteRepository bankHolidayCalendarSiteRepository,
			IBankHolidayCalendarSitePersister bankHolidayCalendarSitePersister)
		{
			_bankHolidayCalendarRepository = bankHolidayCalendarRepository;
			_bankHolidayModelMapper = bankHolidayModelMapper;
			_bankHolidayDateRepository = bankHolidayDateRepository;
			_bankHolidayCalendarSiteRepository = bankHolidayCalendarSiteRepository;
			_bankHolidayCalendarSitePersister = bankHolidayCalendarSitePersister;
		}

		private IBankHolidayCalendar persistCalendar(BankHolidayCalendarForm input)
		{
			IBankHolidayCalendar calendar;

			if (input.Id.HasValue)
			{
				calendar = _bankHolidayCalendarRepository.Load(input.Id.Value);
				if (!string.IsNullOrWhiteSpace(input.Name))
				{
					calendar.Name = input.Name;
				}
			}
			else
			{
				string name = string.IsNullOrWhiteSpace(input.Name) ? "Unknow Calendar" : input.Name;
				calendar = new Domain.SystemSetting.BankHolidayCalendar.BankHolidayCalendar();
				calendar.Name = name;
				_bankHolidayCalendarRepository.Add(calendar);
			}

			return calendar;
		}

		private List<IBankHolidayDate> persistDates(IBankHolidayCalendar calendar, IEnumerable<BankHolidayDateForm> dates)
		{
			var _dates = _bankHolidayDateRepository.LoadAll().Where(d => d.Calendar.Id.Value == calendar.Id.Value).ToList();

			if (dates == null)
				return _dates;

			foreach (var d in dates)
			{
				var _d = createBankHolidayDate(calendar, d);
				if (_d == null || _dates.Contains(_d))
					continue;

				_bankHolidayDateRepository.Add(_d);
				_dates.Add(_d);
			}

			return _dates;
		}

		private IBankHolidayDate createBankHolidayDate(IBankHolidayCalendar calendar, BankHolidayDateForm input)
		{
			IBankHolidayDate date;
			var inputDate = input.Date;
			var des = input.Description;

			if (input.Id.HasValue)
			{
				date = _bankHolidayDateRepository.Load(input.Id.Value);
				date.Date = inputDate;
				date.Description = des;
				date.Calendar = calendar;
				if (input.IsDeleted)
					date.SetDeleted();
			}
			else if (((date = _bankHolidayDateRepository.Find(inputDate, calendar)) != null) && date.IsDeleted)
			{
				date.Description = des;
				date.Active();
			}
			else if (date == null)
			{
				date = new BankHolidayDate { Date = inputDate, Description = des, Calendar = calendar };
			}
			else
			{
				date = null;
			}
			return date;
		}

		public virtual BankHolidayCalendarViewModel Persist(BankHolidayCalendarForm input)
		{
			var calendar = persistCalendar(input);
			var dates = persistDates(calendar, input.Years?.SelectMany(y => y.Dates));
			return _bankHolidayModelMapper.Map(calendar, dates);
		}

		public virtual bool Delete(Guid Id)
		{
			bool updateCalendarsForSitesResult = false;
			try
			{
				updateCalendarsForSitesResult = updateCalendarsForSites(Id);
				var calendar = _bankHolidayCalendarRepository.Load(Id);
				_bankHolidayCalendarRepository.Remove(calendar);
				_bankHolidayDateRepository.LoadAll().Where(d => d.Calendar.Id.Value == Id)?.ToList().ForEach(d => _bankHolidayDateRepository.Remove(d));

			}
			catch (Exception ex)
			{
				logger.Error("Delete Calendar failed.", ex);
				return false;
			}

			return true && updateCalendarsForSitesResult;
		}

		private bool updateCalendarsForSites(Guid Id)
		{
			var sites = _bankHolidayCalendarSiteRepository.FindSitesByCalendar(Id);
			var settings = _bankHolidayCalendarSiteRepository.LoadAll();
			var input = new List<SiteBankHolidayCalendarsViewModel>();
			sites?.ToList().ForEach(s =>
			{
				var model = new SiteBankHolidayCalendarsViewModel { Site = s };
				var calendars = settings.Where(c => c.Site.Id.Value == s).Select(c => c.Calendar.Id.Value);
				model.Calendars = calendars.Where(id => id != Id);
				input.Add(model);
			});
			return _bankHolidayCalendarSitePersister.UpdateCalendarsForSites(input);
		}

	}
}