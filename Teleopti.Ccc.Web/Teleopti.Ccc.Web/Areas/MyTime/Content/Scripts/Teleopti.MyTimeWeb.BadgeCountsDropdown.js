Teleopti.MyTimeWeb.BadgeCountsDropdown = (function ($) {

	var stopClosingOnClick = function () {
		$('#badge-period-navigator').click(function (ev) {
			ev.stopPropagation();
		});
	};

	var resetPeriodOnDropdownHidden = function (viewModel) {
		$('#BadgePanel').on('hidden.bs.dropdown', function (ev) {
			viewModel.resetPeriod();
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

				var vm = new Teleopti.MyTimeWeb.BadgeCountsDropdownViewModel(startMoment, periodType);
				ko.applyBindings(vm, el);
				vm.init();

				stopClosingOnClick();
				resetPeriodOnDropdownHidden(vm);
			});
		},
	};
})(jQuery);

