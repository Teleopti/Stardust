'use strict';
(function() {
	angular.module('wfm.requests')
		.controller('requestsSiteOpenHoursCtrl', ['requestsDataService', 'requestsNotificationService', requestsSiteOpenHoursController])
		.directive('requestsSiteOpenHours', requestsSiteOpenHoursDirective);

	function requestsSiteOpenHoursController(requestsDataService, requestsNotificationService) {

		var vm = this;
		vm.openHours = [];
		vm.sites = [];
		vm.save = save;
		vm.cancel = cancel;
		vm.selectedSite = null;
		init();
		function init() {
			var getSiteProgress = requestsDataService.getSitesPromise();
			getSiteProgress.success(function(sites) {
				vm.sites = [];
				angular.forEach(sites,
					function(site) {
						vm.sites.push(denormalizeSite(site));
					});
				if (vm.sites.length > 0)
					vm.selectedSite = vm.sites[0].Id;
			});
		}

		function save() {
			var sites = [];
			angular.forEach(vm.sites, function(site) {
				sites.push(normalizeSite(site));
			});
			var maintainOpenHoursProgress = requestsDataService.maintainOpenHoursPromise(sites);
			maintainOpenHoursProgress.success(function (persistedSites) {
				requestsNotificationService.notifySaveSiteOpenHoursSuccess(persistedSites);
				cleanUp();
			});
		}

		function cancel() {
			cleanUp();
		}

		function cleanUp() {
			vm.sites = [];
			vm.showSite = false;
		}

		// these utilities have nothing to do with requests, so I keep everything contained in this directive
		function normalizeSite(site) {
			var site = angular.copy(site);
			var formattedWorkingHours = [];
			site.OpenHours.forEach(function (d) {
				d.WeekDaySelections.forEach(function (e) {
					if (e.Checked) {
						var timespan = formatTimespanObj({
							StartTime: d.StartTime,
							EndTime: d.EndTime
						});
						formattedWorkingHours.push({
							WeekDay: e.WeekDay,
							StartTime: timespan.StartTime,
							EndTime: timespan.EndTime,
							IsClosed: e.IsClosed

						});
					}
				});
			});
			site.OpenHours = formattedWorkingHours;
			return site;
		}
		function denormalizeSite(site) {
			var siteCopy = angular.copy(site);
			var reformattedWorkingHours = [];
			siteCopy.OpenHours.forEach(function (a) {
				if (!a.IsClosed) {
					var startTime = parseTimespanString(a.StartTime),
						endTime = parseTimespanString(a.EndTime),
						timespan = {
							StartTime: startTime,
							EndTime: endTime
						};
					var workingHourRows = reformattedWorkingHours.filter(function(wh) {
						return sameTimespan(timespan, wh);
					});
					var workingHourRow;
					if (workingHourRows.length == 0) {
						workingHourRow = createEmptyWorkingPeriod(startTime, endTime);
						angular.forEach(workingHourRow.WeekDaySelections,
							function(e) {
								if (e.WeekDay == a.WeekDay) {
									e.Checked = true;
									e.IsClosed = a.IsClosed;
								}
							});
						reformattedWorkingHours.push(workingHourRow);
					} else {
						workingHourRow = workingHourRows[0];
						angular.forEach(workingHourRow.WeekDaySelections,
							function(e) {
								if (e.WeekDay == a.WeekDay) {
									e.Checked = true;
									e.IsClosed = a.IsClosed;
								}
							});
					}
				}
			});
			siteCopy.OpenHours = reformattedWorkingHours;
			return siteCopy;
		};
	}

	function formatTimespanObj(timespan) {
		var startTimeMoment = moment(timespan.StartTime),
			endTimeMoment = moment(timespan.EndTime);
		if (startTimeMoment.isSame(endTimeMoment, 'day')) {
			return {
				StartTime: startTimeMoment.format('HH:mm'),
				EndTime: endTimeMoment.format('HH:mm')
			};
		} else {
			return {
				StartTime: startTimeMoment.format('HH:mm'),
				EndTime: '1.' + endTimeMoment.format('HH:mm')
			};
		}
	}
	function createEmptyWorkingPeriod(startTime, endTime) {
		var weekdaySelections = [];
		var startDow = (moment.localeData()._week) ? moment.localeData()._week.dow : 0;
		for (var i = 0; i < 7; i++) {
			var curDow = (startDow + i) % 7;
			weekdaySelections.push({ WeekDay: curDow, Checked: false });
		}
		return { StartTime: startTime, EndTime: endTime, WeekDaySelections: weekdaySelections };
	}
	function sameTimespan(timespan1, timespan2) {
		var formattedTimespan1 = formatTimespanObj(timespan1),
			formattedTimespan2 = formatTimespanObj(timespan2);
		return formattedTimespan1.StartTime == formattedTimespan2.StartTime &&
			formattedTimespan1.EndTime == formattedTimespan2.EndTime;
	}
	function parseTimespanString(t) {
		if (!angular.isString(t)) return t;
		var parts = t.match(/^(\d[.])?(\d{1,2}):(\d{1,2})(:|$)/);
		if (parts) {
			var d = new Date();
			d.setHours(parts[2]);
			d.setMinutes(parts[3]);
			if (parts[1]) return moment(d).add(1, 'days').toDate();
			else return d;
		}
	}
	function requestsSiteOpenHoursDirective() {
		return {
			scope: {
				showSite: '='
			},
			restrict: 'E',
			controller: 'requestsSiteOpenHoursCtrl',
			controllerAs: 'requestsSiteOpenHours',
			bindToController: true,
			templateUrl: 'js/requests/html/requests-open-hours.html'
		}
	}
}());