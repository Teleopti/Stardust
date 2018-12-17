(function () {
	'use strict';

	angular.module('wfm.rta').controller('RtaHistoricalController77045', RtaHistoricalController);

	RtaHistoricalController.$inject = ['$http', '$state', '$stateParams', 'rtaService', '$translate', 'RtaTimeline', '$scope'];

	function RtaHistoricalController($http, $state, $stateParams, rtaService, $translate, rtaTimeline, $scope) {
		var vm = this;

		vm.highlighted = {};
		vm.diamonds = [];
		vm.cards = [];
		$stateParams.open = $stateParams.open === "true";
		vm.openRecordedOutOfAdherences = $stateParams.open;
		vm.openApprovedPeriods = $stateParams.open;
		vm.openApproveForm = $stateParams.open;
		vm.hasModifyAdherencePermission = false;
		vm.invalidTime = false;

		var calculate;
		var timelineStart;
		var timelineEnd;

		loadData();

		$http.get('../api/Adherence/Permissions', {
			params: {
				personId: $stateParams.personId,
				date: $stateParams.date
			}
		}).then(function (response) {
			vm.hasModifyAdherencePermission = response.data.ModifyAdherence;
		});

		function loadData() {
			$http.get('../api/HistoricalAdherence/ForPerson', {
				params: {
					personId: $stateParams.personId,
					date: $stateParams.date
				}
			}).then(function (response) {
				var data = response.data;

				data.Schedules = data.Schedules || [];
				data.OutOfAdherences = data.OutOfAdherences || [];
				data.RecordedOutOfAdherences = data.RecordedOutOfAdherences || [];
				data.ApprovedPeriods = data.ApprovedPeriods || [];
				data.Changes = data.Changes || [];
				data.Timeline = data.Timeline || {};
				data.Navigation = data.Navigation || {};

				calculate = rtaTimeline.makeCalculator(data.Timeline.StartTime, data.Timeline.EndTime);

				vm.personId = data.PersonId;
				vm.agentName = data.AgentName;
				vm.date = moment($stateParams.date).format('L');
				vm.adherencePercentage = data.AdherencePercentage;
				vm.showAdherencePercentage = data.AdherencePercentage !== null;

				vm.currentTimeOffset = calculate.Offset(data.Now);

				vm.agentsFullSchedule = buildAgentsFullSchedule(data.Schedules);

				vm.outOfAdherences = buildOutOfAdherence(data.Timeline, data.OutOfAdherences);
				vm.recordedOutOfAdherences = buildRecordedOutOfAdherences(data.Timeline, data.RecordedOutOfAdherences);
				vm.approvedPeriods = buildApprovedPeriods(data.Timeline, data.ApprovedPeriods);

				vm.fullTimeline = buildTimeline(data);
				timelineStart = data.Timeline.StartTime;
				timelineEnd = data.Timeline.EndTime;

				vm.cards = mapChanges(data.Changes, data.Schedules);
				vm.diamonds = buildDiamonds(data.Changes);
				vm.lateForWork = buildLateForWork(data.Changes);

				if ($stateParams.date > data.Navigation.First) {
					var previousDay = moment($stateParams.date).subtract(1, 'day');
					vm.previousHref = $state.href($state.current.name, {personId: vm.personId, date: previousDay.format('YYYYMMDD')});
				}

				if ($stateParams.date < data.Navigation.Last) {
					var nextDay = moment($stateParams.date).add(1, 'day');
					vm.nextHref = $state.href($state.current.name, {personId: vm.personId, date: nextDay.format('YYYYMMDD')});
				}
			});

		}

		function buildTimeline(data) {
			var timeline = [];
			var time = moment(data.Timeline.StartTime).startOf('hour');
			var endTime = moment(data.Timeline.EndTime);
			while (true) {
				time.add(1, 'hour');
				if (time.isSameOrAfter(endTime))
					break;
				timeline.push({
					Offset: calculate.Offset(time),
					Time: time.clone()
				});
			}
			return timeline;
		}

		function buildOutOfAdherence(timeline, intervals) {
			return intervals.map(function (i) {
				return buildInterval(timeline, i)
			});
		}

		function buildRecordedOutOfAdherences(timeline, intervals) {
			return intervals
				.map(function (interval) {
					var o = buildInterval(timeline, interval);

					o.click = function () {
						vm.recordedOutOfAdherences.forEach(function (r) {
							r.highlight = false;
						});
						o.highlight = true;

						vm.openRecordedOutOfAdherences = true;
						vm.openApprovedPeriods = true;
						vm.openApproveForm = true;

						vm.approveStartTime = putTimeBetween(moment(interval.StartTime), moment(timelineStart), moment(timelineEnd)).toDate();
						vm.approveEndTime = putTimeBetween(moment(interval.EndTime), moment(timelineStart), moment(timelineEnd)).toDate();

						startTime = moment(vm.approveStartTime).format("LTS");
						endTime = moment(vm.approveEndTime).format("LTS");

						if (moment(vm.approveStartTime).isBefore(timelineStart))
							vm.approveStartTime = moment(timelineStart).toDate();
					};

					return o
				});
		}

		var startTime;
		Object.defineProperty(vm, 'approveStartTimeString', {
			get: function () {
				return startTime;
			},
			set: function (value) {
				startTime = value;
				var m = moment('1970-01-01T' + moment(startTime, "LTS").format('HH:mm:ss'));
				vm.approveStartTime = m.isValid() ? m.toDate() : undefined;
			}
		});

		var endTime;
		Object.defineProperty(vm, 'approveEndTimeString', {
			get: function () {
				return endTime;
			},
			set: function (value) {
				endTime = value;
				var m = moment('1970-01-01T' + moment(endTime, "LTS").format('HH:mm:ss'));
				vm.approveEndTime = m.isValid() ? m.toDate() : undefined;
			}
		});

		$scope.$watch(function () {
				return vm.approveStartTime
			},
			function (newValue, oldValue) {
				if (timelineStart && timeChanged(newValue, oldValue))
					vm.approveStartTime = adjustDate(newValue, timelineStart, timelineEnd);

				validateTimeInput();
				updateApprovePositioning();
			}
		);

		$scope.$watch(function () {
				return vm.approveEndTime
			},
			function (newValue, oldValue) {
				if (timelineStart && timeChanged(newValue, oldValue)) {
					newValue = adjustDate(newValue, timelineStart, timelineEnd);
					newValue = adjustDate(newValue, vm.approveStartTime, timelineEnd);
					vm.approveEndTime = newValue;
				}

				validateTimeInput();
				updateApprovePositioning();
			}
		);

		function validateTimeInput() {
			vm.invalidTime = !(vm.approveStartTime && 
				vm.approveEndTime &&
				moment(vm.approveStartTime).isBefore(vm.approveEndTime));

			if (!vm.approveStartTime || !vm.approveEndTime) 
				vm.invalidTimeMessage = "IllegalTimeInput";
			else if (moment(vm.approveStartTime).isAfter(vm.approveEndTime)) 
				vm.invalidTimeMessage = "EndTimeMustBeGreaterOrEqualToStartTime";
			else 
				vm.invalidTimeMessage = undefined;
		}

		function updateApprovePositioning() {
			if (vm.approveStartTime && vm.approveEndTime) {
				vm.approveWidth = calculate.Width(vm.approveStartTime, vm.approveEndTime);
				vm.approveOffset = calculate.Offset(vm.approveStartTime);
				return;
			}
			vm.approveWidth = undefined;
			vm.approveOffset = undefined;
		}

		function timeChanged(time, oldTime) {
			var newTimestamp = time ? time.getTime() : null;
			var oldTimestamp = oldTime ? oldTime.getTime() : null;
			return newTimestamp != oldTimestamp;
		}

		function adjustDate(time, start) {
			if (!time)
				return undefined;
			if (!start)
				return time;

			start = moment(start);

			var timeToAdd = moment(time);
			var attempt = start.clone()
				.set('hour', timeToAdd.hour())
				.set('minute', timeToAdd.minute())
				.set('second', timeToAdd.second());

			var result = attempt.isSameOrAfter(start) ?
				attempt.toDate() :
				attempt.add(1, "days").toDate();

			if (timeChanged(result, time))
				return result;
			return time;
		}

		function putTimeBetween(time, start, end) {
			if (time.isBefore(start))
				return start;
			if (time.isAfter(end))
				return end;
			return time;
		}

		vm.cancelApprove = function () {
			vm.openApproveForm = false;
			vm.approveStartTime = undefined;
			vm.approveEndTime = undefined;
		};

		vm.submitApprove = function () {
			$http.post('../api/HistoricalAdherence/ApprovePeriod',
				{
					PersonId: $stateParams.personId,
					StartDateTime: moment(vm.approveStartTime).format("YYYY-MM-DD HH:mm:ss"),
					EndDateTime: moment(vm.approveEndTime).format("YYYY-MM-DD HH:mm:ss")
				}
			).then(loadData);
		};

		function buildApprovedPeriods(timeline, intervals) {
			return intervals
				.map(function (interval) {
					var o = buildInterval(timeline, interval);
					o.click = function () {
						vm.approvedPeriods.forEach(function (a) {
							a.highlight = false;
						});
						o.highlight = true;
						vm.openApprovedPeriods = true;
					};
					o.remove = function () {
						$http.post('../api/HistoricalAdherence/RemoveApprovedPeriod',
							{
								PersonId: $stateParams.personId,
								StartDateTime: moment(interval.StartTime).format("YYYY-MM-DD HH:mm:ss"),
								EndDateTime: moment(interval.EndTime).format("YYYY-MM-DD HH:mm:ss")
							}
						).then(loadData);
					};
					return o;
				});
		}

		function buildInterval(timeline, interval) {
			var startTime = interval.StartTime ? moment(interval.StartTime) : moment(timeline.StartTime);
			var startTimeDisplay = undefined;
			if (interval.StartTime) {
				startTimeDisplay = startTime.isBefore(timeline.StartTime) ?
					startTime.format('LLL') :
					startTime.format('LTS');
			}
			var endTime = interval.EndTime ? moment(interval.EndTime) : moment(timeline.EndTime);
			var endTimeDisplay = undefined;
			if (interval.EndTime) {
				endTimeDisplay = endTime.format('LTS');
			}

			return {
				Width: calculate.Width(startTime, endTime),
				Offset: calculate.Offset(startTime),
				StartTime: startTimeDisplay,
				EndTime: endTimeDisplay
			};
		}

		function buildAgentsFullSchedule(schedules) {
			return schedules.map(function (layer) {
				return {
					Width: calculate.Width(layer.StartTime, layer.EndTime),
					Offset: calculate.Offset(layer.StartTime),
					StartTime: moment(layer.StartTime),
					EndTime: moment(layer.EndTime),
					Color: layer.Color
				};
			});
		}

		function buildDiamonds(changes) {
			return changes.map(function (change) {
				return {
					Time: change.Time,
					Offset: calculate.Offset(change.Time),
					RuleColor: !change.RuleColor ? "rgba(0,0,0,0.54)" : change.RuleColor,
					Color: change.RuleColor,
					click: change.click,
					get highlight() {
						return change.highlight;
					}
				};
			});
		}

		function buildLateForWork(changes) {
			var change = changes.find(function (d) {
				return d.LateForWorkMinutes;
			});
			if (change) {
				return {
					minutes: change.LateForWorkMinutes,
					offset: calculate.Offset(change.Time),
					click: change.click
				};
			}
		}

		function mapChanges(changes, schedules) {

			var makeCard = function (allChanges, header, color, startTime, endTime) {

				var changes = allChanges;
				if (startTime)
					changes = allChanges.filter(function (change) {
						return change.Time >= startTime && change.Time < endTime;
					});

				var card = {
					Header: header,
					Color: color,
					isOpen: $stateParams.open,
					StartTime: startTime,
					EndTime: endTime
				};

				card.Items = mapChanges(allChanges, changes, card);

				return card;
			};

			var mapChanges = function (allChanges, changes, card) {
				return changes.map(function (change) {

					change.click = function () {
						allChanges.forEach(function (c) {
							c.highlight = false;
						});
						change.highlight = true;
						card.isOpen = true;
					};

					return {
						Time: change.Time,
						Duration: change.Duration,
						Activity: change.Activity,
						ActivityColor: change.ActivityColor,
						Rule: change.Rule,
						RuleColor: change.RuleColor,
						State: change.State,
						Adherence: change.Adherence,
						AdherenceColor: change.AdherenceColor,
						lateForWorkMinutes: change.LateForWorkMinutes,

						click: change.click,
						get highlight() {
							return change.highlight;
						}
					};
				});
			};

			if (schedules.length === 0)
				return [makeCard(changes, $translate.instant('NoShift'), 'black')];

			var beforeShift = [makeCard(changes, $translate.instant('BeforeShiftStart'), 'black', '0000-01-01T00:00:00', schedules[0].StartTime)];
			var afterShift = [makeCard(changes, $translate.instant('AfterShiftEnd'), 'black', schedules[schedules.length - 1].EndTime, '9999-01-01T00:00:00')];
			return beforeShift
				.concat(schedules.map(function (l) {
					var activityStart = moment(l.StartTime);
					var activityEnd = moment(l.EndTime);
					var header = l.Name + ' ' + activityStart.format('LT') + ' - ' + activityEnd.format('LT');
					return makeCard(changes, header, l.Color, l.StartTime, l.EndTime);
				}))
				.concat(afterShift)
				.filter(function (card) {
					return card.Items.length > 0;
				});
		}
	}
})();
