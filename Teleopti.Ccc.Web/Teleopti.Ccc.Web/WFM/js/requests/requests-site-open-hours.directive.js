'use strict';
(function () {
	angular.module('wfm.requests')
		.controller('requestsSiteOpenHoursCtrl', ['requestsDataService', 'requestsNotificationService', requestsSiteOpenHoursController])
		.directive('requestsSiteOpenHours', requestsSiteOpenHoursDirective);

	function requestsSiteOpenHoursController(requestsDataService, requestsNotificationService) {

		var vm = this;
		var sitesBefore, sitesAfter;
		vm.openHours = [];
		vm.sites = [];
		vm.save = save;
		vm.cancel = cancel;
		vm.selectedSite = null;
		init();
		function init() {
			var getSiteProgress = requestsDataService.getSitesPromise();
			getSiteProgress.success(function (sites) {
				sitesBefore = sites;
				vm.sites = [];
				angular.forEach(sites,
					function (site) {
						vm.sites.push(denormalizeSite(site));
					});
				if (vm.sites.length > 0)
					vm.selectedSite = vm.sites[0].Id;
			});
		}

		function save() {
			var sites = [];
			angular.forEach(vm.sites, function (site) {
				sites.push(normalizeSite(site));
			});
			sitesAfter = sites;
			if (isOpenHoursSame(sitesBefore, sitesAfter)) {
				requestsNotificationService.notifyNothingChanged();
			} else {
				var maintainOpenHoursProgress = requestsDataService.maintainOpenHoursPromise(sites);
				maintainOpenHoursProgress.success(function (persistedSites) {
					requestsNotificationService.notifySaveSiteOpenHoursSuccess(persistedSites);
				});
			}
			cleanUp();
		}

		function cancel() {
			cleanUp();
		}

		function cleanUp() {
			vm.sites = [];
			vm.showSite = false;
		}

		// these utilities have nothing to do with requests, so I keep everything contained in this directive
		function isOpenHoursSame(sitesBefore, sitesAfter) {
			if (!sitesBefore || !sitesAfter) return;
			var isSame = true;
			for (var i = 0; i < sitesBefore.length && isSame; i++) {
				for (var j = 0; j < sitesBefore[i].OpenHours.length && isSame; j++) {
					var openHourBefore = sitesBefore[i].OpenHours[j];
					var k = sitesBefore[i].OpenHours.length - 1;
					while (isSame && k >= 0) {
						var openHourAfter = sitesAfter[i].OpenHours[k];
						if (openHourBefore.WeekDay == openHourAfter.WeekDay) {
							if (openHourAfter.IsClosed != openHourBefore.IsClosed) isSame = false;
							if (openHourAfter.StartTime + ':00' != openHourBefore.StartTime) isSame = false;
							if (openHourAfter.EndTime + ':00' != openHourBefore.EndTime) isSame = false;
						}
						k--;
					}
				}
			}
			return isSame;
		}

		function normalizeSite(site) {
			var site = angular.copy(site);
			var weekTemplet = [];
			prepareWeekTemplet(weekTemplet);
			angular.forEach(site.OpenHours, function (openHour) {
				var timespan = formatTimespanObj({
					StartTime: openHour.StartTime,
					EndTime: openHour.EndTime
				});
				angular.forEach(openHour.WeekDaySelections, function (selection) {
					if (selection.Checked) {
						var keepLooking = true;
						for (var j = 0; j < weekTemplet.length && keepLooking; j++) {
							if (selection.WeekDay == weekTemplet[j].WeekDay) {
								weekTemplet[j].EndTime = timespan.EndTime;
								weekTemplet[j].StartTime = timespan.StartTime;
								weekTemplet[j].IsClosed = false;
								keepLooking = false;
							}
						}
					}
				});
			});
			site.OpenHours = weekTemplet;
			return site;
		}

		function prepareWeekTemplet(weekTemplet) {
			for (var i = 0; i < 7; i++) {
				var dayTemplet = {
					EndTime: "00:00",
					IsClosed: true,
					StartTime: "00:00",
					WeekDay: i
				};
				weekTemplet.push(dayTemplet);
			}
		}
		function denormalizeSite(site) {
			var siteCopy = angular.copy(site);
			var reformattedWorkingHours = [];
			siteCopy.OpenHours.forEach(function (a) {
				var startTime = parseTimespanString(a.StartTime),
					endTime = parseTimespanString(a.EndTime),
					timespan = {
						StartTime: startTime,
						EndTime: endTime
					};
				var workingHourRows = reformattedWorkingHours.filter(function (wh) {
					return sameTimespan(timespan, wh);
				});
				var workingHourRow;
				if (workingHourRows.length === 0) {
					workingHourRow = createEmptyWorkingPeriod(startTime, endTime);
					if (a.StartTime !== '00:00:00' || a.EndTime !== '00:00:00')
						reformattedWorkingHours.push(workingHourRow);
				} else {
					workingHourRow = workingHourRows[0];
				}
				angular.forEach(workingHourRow.WeekDaySelections,
								function (e) {
									if (e.WeekDay === a.WeekDay) {
										e.Checked = !a.IsClosed;
										e.IsClosed = a.IsClosed;
									}
								});
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