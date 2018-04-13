/// <reference path="~/Content/Scripts/knockout-2.2.1.js" />
/// <reference path="~/Content/moment/moment.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.BadgeCountsDropdown = (function ($) {

	var stopClosingOnClick = function () {
		$('#badge-period-navigator').click(function (ev) {
			ev.stopPropagation();
		});
	};

	var findStartDateOfWeek = function (dateMoment, weekStart) {
		var day = dateMoment.day();
		var offset = day - weekStart;
		if (offset < 0)
			offset += 7;
		return dateMoment.add('days', -offset);
	};

	var findStartDateOfPeriod = function (dateMoment, periodType, weekStart) {
		var start = null;
		switch (periodType) {
			case 'Weekly':
				start = findStartDateOfWeek(dateMoment.clone(), weekStart);
				break;
			case 'Monthly':
				start = dateMoment.clone().startOf('month');
				break;
		}
		return start;
	};

	return {
		Init: function () {
			Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function (userInfo) {
				var el = $('#badge-counts')[0];

				if (!el) return;

				var periodType = el.dataset.periodType;

				if (periodType === 'OnGoing') return;

				var startMoment = findStartDateOfPeriod(moment(), periodType, userInfo.WeekStart);

				var vm = new Teleopti.MyTimeWeb.BadgeCountsDropdownViewModel(startMoment, periodType, userInfo.DateFormatForMoment);
				ko.applyBindings(vm, el);
				vm.fetchBadgeCounts();

				stopClosingOnClick();
			});
		},
	};
})(jQuery);
