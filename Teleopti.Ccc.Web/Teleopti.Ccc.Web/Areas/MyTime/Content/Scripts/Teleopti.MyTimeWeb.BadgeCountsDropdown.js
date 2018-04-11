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

	return {
		Init: function () {
			Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function (userInfo) {
				var el = $('#badge-counts')[0];

				if (!el) return;

				var periodType = el.dataset.periodType;

				if (periodType === 'OnGoing') return;

				var startMoment = moment().locale(Teleopti.MyTimeWeb.Common.DateFormatLocale);

				if (periodType === 'Weekly')
					startMoment.startOf('week');
				else
					startMoment.startOf('month');

				var vm = new Teleopti.MyTimeWeb.BadgeCountsDropdownViewModel(startMoment, periodType, userInfo.DateFormatForMoment);
				ko.applyBindings(vm, el);
				vm.fetchBadgeCounts();

				stopClosingOnClick();
			});
		},
	};
})(jQuery);
