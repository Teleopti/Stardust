Teleopti.MyTimeWeb.BadgeCountsDropdownViewModel = function BadgeCountsDropdownViewModel(startMoment, periodType) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var getEndMoment = function (startMoment, periodType) {
		var m = startMoment.clone();
		switch (periodType) {
			case 'Weekly':
				m.add('days', 6);
				break;
			case 'Monthly':
				m.endOf('month');
				break;
		}
		return m;
	};

	var self = this;

	self.isFetchingData = ko.observable(false);
	self.badgeCounts = ko.observableArray();
	self.periodType = periodType;
	self.startMoment = ko.observable(startMoment);
	self.endMoment = ko.observable( getEndMoment(startMoment, periodType) );
	self.periodText = ko.observable();

	var updatePeriodText = function () {
		var t = Teleopti.MyTimeWeb.Common.FormatDatePeriod(self.startMoment(), self.endMoment(), false);
		self.periodText(t);
	};

	var lastIncrement;
	var incPeriod = function (n) {
		lastIncrement = n;
		var type = self.periodType === 'Weekly' ? 'week' : 'month';
		self.startMoment(self.startMoment().add(n, type));
		self.endMoment(self.startMoment().clone().endOf(type));
	};

	var revertPeriodChange = function () {
		incPeriod(-lastIncrement);
	};

	var getCurrentMoment = (function (m) {
		var current = m.clone();
		return function () {
			return current.clone();
		};
	})(startMoment);

	var resetPeriod = function () {
		var type = self.periodType === 'Weekly' ? 'week' : 'month';
		self.startMoment(getCurrentMoment());
		self.endMoment(self.startMoment().clone().endOf(type));
	};

	var fetch = function (from, to) {
		ajax.Ajax({
			url: 'Portal/GetBadges',
			dataType: 'json',
			cache: false,
			data: { from: from, to: to },
			success: function (data) {
				self.badgeCounts(data);
			},
			error: function (jqXHR, textStatus, errorThrown) {
				revertPeriodChange();
			},
			complete: function (jqXHR, textStatus) {
				self.isFetchingData(false);
				updatePeriodText();
			},
		});
	};

	var fetchBadgeCounts = function () {
		if (self.periodType === 'OnGoing') return;
		var from = Teleopti.MyTimeWeb.Common.FormatServiceDate(self.startMoment());
		var to = Teleopti.MyTimeWeb.Common.FormatServiceDate(self.endMoment());
		self.isFetchingData(true);
		fetch(from, to);
	};

	self.init = function () {
		fetchBadgeCounts();
	};

	self.clickPrev = function () {
		incPeriod(-1);
		fetchBadgeCounts();
	};

	self.clickNext = function () {
		incPeriod(1);
		fetchBadgeCounts();
	};

	self.resetPeriod = function () {
		resetPeriod();
		fetchBadgeCounts();
	};
};
