(function () {
	'use strict';

	angular.module('wfm.rta').controller('RtaHistoricalController47230', RtaHistoricalController);

	RtaHistoricalController.$inject = ['$http', '$state', '$stateParams', 'rtaService', '$translate', 'RtaTimeline', '$scope'];

	function RtaHistoricalController($http, $state, $stateParams, rtaService, $translate, rtaTimeline, $scope) {
		var vm = this;

		vm.highlighted = {};
		vm.diamonds = [];
		vm.cards = [];
		vm.previousHref;
		vm.nextHref;
		$stateParams.open = $stateParams.open === "true";
		vm.openRecordedOutOfAdherences = $stateParams.open;
		vm.openApprovedPeriods = $stateParams.open;
		vm.openApproveForm = $stateParams.open;

		vm.ooaTooltipTime = function (time) {
			if (time == null)
				return '';
			return time.format('HH:mm:ss');
		};

		var calculate;
		var timelineStart;

		loadData();

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

				vm.cards = mapChanges(data.Changes, data.Schedules);

				vm.diamonds = buildDiamonds(data);

				if ($stateParams.date > data.Navigation.First) {
					var previousDay = moment($stateParams.date).subtract(1, 'day');
					vm.previousHref = $state.href($state.current.name, {personId: vm.personId, date: previousDay.format('YYYYMMDD')});
					vm.previousTooltip = previousDay.format('L');
				}

				if ($stateParams.date < data.Navigation.Last) {
					var nextDay = moment($stateParams.date).add(1, 'day');
					vm.nextHref = $state.href($state.current.name, {personId: vm.personId, date: nextDay.format('YYYYMMDD')});
					vm.nextTooltip = nextDay.format('L');
				}
			});

		}

		function buildTimeline(data) {
			var timeline = [];
			var time = moment(data.Timeline.StartTime);
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

						vm.approveStartTime = moment(interval.StartTime).toDate();
						vm.approveEndTime = moment(interval.EndTime).toDate();

						if (moment(vm.approveStartTime).isBefore(timelineStart))
							vm.approveStartTime = moment(timelineStart).toDate();
					};

					return o
				});
		}

		$scope.$watch(function () {
				return vm.approveStartTime
			},
			function (newValue, oldValue) {
				if (timelineStart)
					vm.approveStartTime = putTimeAfter(newValue, oldValue, timelineStart);
			}
		);

		$scope.$watch(function () {
				return vm.approveEndTime
			},
			function (newValue, oldValue) {
				vm.approveEndTime = putTimeAfter(newValue, oldValue, vm.approveStartTime);
			}
		);

		function putTimeAfter(time, oldTime, putAfter) {
			var oldTimestamp = oldTime ? oldTime.getTime() : null;
			var newTimestamp = time ? time.getTime() : null;
			if (oldTimestamp != newTimestamp) {
				time = intoTimeline(time);
				if (time.isBefore(putAfter))
					time = time.add(1, "days");
				return time.toDate();
			}
			return time;
		}

		function intoTimeline(time) {
			var time = moment(time);
			var daysToTimeline = moment(timelineStart).diff(time, "days");
			return time.add(daysToTimeline, "days");
		}

		vm.cancelApprove = function () {
			vm.openApproveForm = false;
		};

		vm.submitApprove = function () {
			$http.post('../api/HistoricalAdherence/ApprovePeriod',
				{
					personId: $stateParams.personId,
					startTime: moment(vm.approveStartTime).format("YYYY-MM-DD HH:mm:ss"),
					endTime: moment(vm.approveEndTime).format("YYYY-MM-DD HH:mm:ss")
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
					return o;
				});
		}

		function buildInterval(timeline, interval) {
			var startTime = moment(interval.StartTime);
			var endTime = moment(interval.EndTime);

			return {
				Width: calculate.Width(startTime, endTime),
				Offset: calculate.Offset(startTime),
				StartTime: startTime.isBefore(timeline.StartTime) ?
					startTime.format('LLL') :
					startTime.format('LTS'),
				EndTime: endTime.format('LTS')
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

		function buildDiamonds(data) {
			return data.Changes.map(function (change) {
				change.Offset = calculate.Offset(change.Time);
				change.RuleColor = !change.RuleColor ? "rgba(0,0,0,0.54)" : change.RuleColor;
				change.Color = change.RuleColor;
				change.click = function () {
					vm.diamonds.forEach(function (d) {
						d.highlight = false;
					});
					change.highlight = true;
					vm.cards.find(function (card) {
						return card.Items.includes(change);
					}).isOpen = true;
				};
				return change;
			});
		}

		function mapChanges(changes, schedules) {

			var makeCard = function (header, color, startTime, endTime) {
				return {
					Header: header,
					Items: [],
					Color: color,
					isOpen: $stateParams.open,
					StartTime: startTime,
					EndTime: endTime
				}
			};

			if (schedules.length === 0) {
				var card = makeCard($translate.instant('NoShift'), 'black');
				card.Items = changes;
				return [card];
			}

			var beforeShift = [makeCard($translate.instant('BeforeShiftStart'), 'black', '0000-01-01T00:00:00', schedules[0].StartTime)];
			var afterShift = [makeCard($translate.instant('AfterShiftEnd'), 'black', schedules[schedules.length - 1].EndTime, '9999-01-01T00:00:00')];
			return beforeShift
				.concat(schedules.map(function (l) {
					var activityStart = moment(l.StartTime);
					var activityEnd = moment(l.EndTime);
					var header = l.Name + ' ' + activityStart.format('HH:mm') + ' - ' + activityEnd.format('HH:mm');
					return makeCard(header, l.Color, l.StartTime, l.EndTime);
				}))
				.concat(afterShift)
				.map(function (card) {
					card.Items = changes.filter(function (change) {
						return change.Time >= card.StartTime && change.Time < card.EndTime;
					});
					return card
				})
				.filter(function (card) {
					return card.Items.length > 0;
				});

		}

	}
})();
