
var datepicker = $.fn.datepicker.Constructor.prototype;
datepicker.fillMonthsGregorian = datepicker.fillMonths;

datepicker.fillMonthsJalaali = function () {
	var html = '';
	var i = 0;
	var monthsShort = $.proxy(moment.langData().jMonthsShort, moment.langData());
	while (i < 12) {
		html += '<span class="month">' + monthsShort(moment().jMonth(i)) + '</span>';
		i++;
	}
	this.picker.find('.datepicker-months td').append(html);
};

datepicker.fillMonths = function () {
	if (!Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
		this.fillMonthsGregorian();
	} else {
		this.fillMonthsJalaali();
	}
};

datepicker.fillGregorianCal = datepicker.fill;

datepicker.fillJalaaliCal = function () {

	if (!this.viewDate.isValid() || this.viewDate.year() <= 0) {
		return;
	};

	var jYear = this.viewDate.jYear();
	var jMonth = this.viewDate.jMonth();

	// due to the internal workings of moment-jalaali.js the start and end dates are converted.
	// the jalaali date is actually the gregorian date we wish to use.
	var jStartDate = this.startDate ? moment([this.startDate.jYear(), this.startDate.jMonth(), this.startDate.jDate()]) : null;
	var jEndDate = this.endDate ? moment([this.endDate.jYear(), this.endDate.jMonth(), this.endDate.jDate()]) : null;

	this.displayMonthText(jMonth, jYear);
	this.fillDayDisplay(jYear, jMonth, jStartDate, jEndDate);
	this.fillMonthPicker(jYear, jStartDate, jEndDate);
	this.fillYearPicker(jYear, jStartDate, jEndDate);
};

datepicker.displayMonthText = function (jMonth, jYear) {
	//render html and use &lrm; to ensure the year appears on the right.
	this.picker.find('.datepicker-days th:eq(1)')
		.html(moment.langData().jMonths(moment().jMonth(jMonth)) + " &lrm; " + jYear);

};

datepicker.fillDayDisplay = function (jYear, jMonth, jStartDate, jEndDate) {

	var currentMoment = this.get();

	var year = this.viewDate.year();
	var currentDate = currentMoment ? currentMoment.valueOf() : null;
	var month = this.viewDate.month();

	var prevMonth = this.getLastWorkingDayOfPrevMonth(jYear, jMonth);
	var nextMonthVal = moment(prevMonth).add('days', 42).valueOf();

	var html = [];
	var clsName;

	// Starting from the last 'start day of the week', move forward filling in the month
	while (prevMonth.valueOf() < nextMonthVal) {
		if (prevMonth.day() === this.weekStart) {
			html.push('<tr>');
		}
		clsName = '';

		if (prevMonth.jYear() < jYear || (prevMonth.jYear() == jYear && prevMonth.jMonth() < jMonth)) {
			clsName += ' old';
		} else if (prevMonth.jYear() > jYear || (prevMonth.jYear() == jYear && prevMonth.jMonth() > jMonth)) {
			clsName += ' new';
		}

		// used to calculate when clicking on the next gregorian month
		if (prevMonth.year() < year || (prevMonth.year() == year && prevMonth.month() < month)) {
			clsName += ' greOld';
		} else if (prevMonth.year() > year || (prevMonth.year() == year && prevMonth.month() > month)) {
			clsName += ' greNew';
		}

		if (prevMonth.valueOf() === currentDate) {
			clsName += ' active';
		}

		// due to the internal workings of moment-datepicker.js, the startdate and end date will be in jalaali, so we need to 
		// compare with the jalaali date, not the gregorian
		if ((jStartDate && prevMonth.valueOf() < jStartDate.valueOf()) || (jEndDate && prevMonth.valueOf() > jEndDate.valueOf())) {
			clsName += ' disabled';
		}
		html.push('<td class="day' + clsName + '" value="' + prevMonth.date() + '" >' + prevMonth.jDate() + '</td>'); // use date as value, only presentation shows jalaali date..

		if (prevMonth.day() === this.weekEnd) {
			html.push('</tr>');
		}
		prevMonth.add('days', 1);
	}

	this.picker.find('.datepicker-days tbody').empty().append(html.join(''));
};

datepicker.getLastWorkingDayOfPrevMonth = function (jYear, jMonth) {
	// using jalaali dates get the last day of the previous month and iterate backwards until we find the start of the week.
	var dateString = jYear + "/" + (jMonth > 9 ? jMonth : "0" + jMonth) + "/" + moment.jDaysInMonth(jYear, jMonth);
	var jPrevMonth = moment(dateString, "jYYYY/jMM/jDD");

	while (jPrevMonth.day() != this.weekStart) {
		jPrevMonth.add('day', -1);
	}

	return moment([jPrevMonth.year(), jPrevMonth.month(), jPrevMonth.date()]);;
};

datepicker.fillMonthPicker = function (jYear, jStartDate, jEndDate) {

	var currentMoment = this.get();

	var currentJMonth = currentMoment ? currentMoment.jMonth() : null;
	var currentJYear = currentMoment ? currentMoment.jYear() : null;

	var months = this.picker.find('.datepicker-months')
				.find('th:eq(1)')
					.text(jYear)
					.end()
				.find('span').removeClass('active').removeClass('disabled');

	if (currentJYear === jYear) {
		months.eq(currentJMonth).addClass('active');
	}

	if (((jStartDate) && jYear < jStartDate.jYear()) || ((jEndDate) && jYear > jEndDate.jYear())) {
		months.addClass('disabled');
	}
	if ((jStartDate) && jYear == jStartDate.jYear()) {
		months.slice(0, jStartDate.jMonth()).addClass('disabled');
	}
	if ((jEndDate) && jYear == jEndDate.jYear()) {
		months.slice(jEndDate.jMonth() + 1).addClass('disabled');
	}
};


datepicker.fillYearPicker = function (jYear, jStartDate, jEndDate) {

	var year = this.viewDate.year();
	var currentMoment = this.get();

	var currentJYear = currentMoment ? currentMoment.jYear() : null;

	var html = '';

	var jYearRounded = parseInt(jYear / 10, 10) * 10;
	var diffRoundedNumberOfYears = jYear - jYearRounded;
	year = year - diffRoundedNumberOfYears;
	jYear = jYearRounded;

	var yearCont = this.picker.find('.datepicker-years')
						.find('th:eq(1)')
							.text(jYear + '-' + (jYear + 9))
							.end()
						.find('td');
	jYear -= 1;
	year -= 1;
	for (var i = -1; i < 11; i++) {
		html += '<span class="year' + (i === -1 || i === 10 ? ' old' : '') + (currentJYear === jYear ? ' active' : '') +
         (((jStartDate) && jYear < jStartDate.jYear()) || ((jEndDate) && jYear > jEndDate.jYear()) ? ' disabled' : '') +
                '" value="' + year + '" >' + jYear + '</span>';
		jYear += 1;
		year += 1;
	}
	yearCont.html(html);
};


datepicker.fill = function () {

	if (!Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
		this.fillGregorianCal();
	} else {
		this.fillJalaaliCal();
	}
};

datepicker.clickGregorian = datepicker.click;

datepicker.click = function (e) {

	if (!Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
		this.clickGregorian(e);
	} else {
		this.clickJahaali(e);
	}
}

datepicker.clickJahaali = function (e) {

	e.stopPropagation();
	e.preventDefault();
	var target = $(e.target).closest('span, td, th');
	if (target.length === 1 && (target[0].nodeName.toLowerCase() === 'td' || target[0].nodeName.toLowerCase() === 'span')) {

		if (!target.is('.disabled')) {
			if (target.is('.day')) {
				// use value because the text is in jalaali.
				var day = parseInt(target.attr('value'), 10) || 1;
				var tempDate = this.viewDate.clone();

				// see if we have clicked on the previous or next gregorian month.
				if (target.is('.greOld')) {
					tempDate.startOf('month').add('days', -1);
				} else if (target.is('.greNew')) {
					tempDate.endOf('month').add('days', 1);
				}

				var month = tempDate.month();
				var year = tempDate.year();
				this.viewDate = moment([year, month, day]);
				this.set(this.viewDate);
			}
			else {
				if (!target.is('.disabled')) {
					if (target.is('.month')) {
						var newMonth = target.parent().find('span').index(target);
						this.viewDate.jMonth(newMonth);
					} else {
						// Use value because the text is in jalaali
						var year = parseInt(target.attr('value'), 10) || 1;
						this.viewDate.year(year);
					}

					if (this.viewMode !== this.minViewMode) {
						this.showMode(-1);
						this.set(this.viewDate, true);
					} else {
						this.set(this.viewDate);
					}
				}
			}
		}

		return;
	}

	this.clickGregorian(e);

};

