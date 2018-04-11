﻿/// <reference path="~/Content/Scripts/knockout-2.2.1.js" />
/// <reference path="~/Content/moment/moment.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.BadgeCountsDropdownViewModel = function BadgeCountsDropdownViewModel(startMoment, periodType, dateFormat) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();

	var self = this;

	self.badgeCounts = ko.observableArray();
	self.periodType = periodType;
	self.startMoment = ko.observable(startMoment);

	if (self.periodType === 'Weekly')
		self.endMoment = ko.observable(self.startMoment().clone().endOf('week'));
	else if (self.periodType === 'Monthly')
		self.endMoment = ko.observable(self.startMoment().clone().endOf('month'));

	self.periodText = ko.computed(function () {
		return self.startMoment().format(dateFormat) +
			' – ' +
			self.endMoment().format(dateFormat);
	});

	var incPeriod = function (n) {
		var type = self.periodType === 'Weekly' ? 'week' : 'month';
		self.startMoment(self.startMoment().add(n, type));
		self.endMoment(self.startMoment().clone().endOf(type));
	};

	self.fetch = function (from, to) {
		ajax.Ajax({
			url: 'Portal/GetBadges',
			dataType: 'json',
			cache: false,
			data: { from: from, to: to },
			success: function (data) {
				self.badgeCounts(data);
			}
		});
	};

	self.fetchBadgeCounts = function () {
		if (self.periodType === 'OnGoing') return;
		var from = self.startMoment().toDate().toJSON();
		var to = self.endMoment().toDate().toJSON();
		self.fetch(from, to);
	};

	self.clickPrev = function () {
		incPeriod(-1);
		self.fetchBadgeCounts();
	};

	self.clickNext = function () {
		incPeriod(1);
		self.fetchBadgeCounts();
	};
};