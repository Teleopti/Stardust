(function() {
    'use strict';
    angular.module('wfm.rta').controller('RtaHistoricalController', RtaHistoricalController);
    RtaHistoricalController.$inject = ['$stateParams', 'rtaService', 'Toggle'];

    function RtaHistoricalController($stateParams, rtaService, toggles) {
        var vm = this;

        var id = $stateParams.personId;

        vm.ooaTooltipTime = function(time) {
            if (time == null)
                return '';

            return time.format('HH:mm:ss');
        };

        rtaService.getAgentHistoricalData(id)
            .then(function(data) {
                if (data.Changes == null) {
                    data.Changes = [];
                }

                var shiftInfo = buildShiftInfo(data.Schedules, data.Now);

                vm.personId = data.PersonId;
                vm.agentName = data.AgentName;
                vm.date = moment(data.Now);
                var totalSeconds = shiftInfo.totalSeconds + 7200;
                var startOfShift = shiftInfo.start.clone().add(-1, 'hour');

                vm.currentTimeOffset = calculateWidth(startOfShift, data.Now, totalSeconds);

                vm.agentsFullSchedule = data.Schedules.map(function(layer) {
                    return {
                        Width: calculateWidth(layer.StartTime, layer.EndTime, totalSeconds),
                        Offset: calculateWidth(startOfShift, layer.StartTime, totalSeconds),
                        StartTime: moment(layer.StartTime),
                        EndTime: moment(layer.EndTime),
                        Color: layer.Color
                    };
                });

                vm.outOfAdherences = data.OutOfAdherences.map(function(ooa) {
                    var startTime = moment(ooa.StartTime);
                    var endTime = ooa.EndTime != null ? moment(ooa.EndTime) : null
                    return {
                        Width: calculateWidth(ooa.StartTime, ooa.EndTime != null ? ooa.EndTime : data.Now, totalSeconds),
                        Offset: calculateWidth(startOfShift, ooa.StartTime, totalSeconds),
                        StartTime: startTime,
                        EndTime: endTime
                    };
                });

                vm.fullTimeline = buildTimeline(shiftInfo);

                vm.changes = data.Changes.map(function(change) {
                    change.offset = calculateWidth(startOfShift, change.Time, totalSeconds);
                    return change;
                });
                vm.sortedChanges = mapChanges(data.Changes, data.Schedules);
            });

        function mapChanges(changes, schedules) {
            return changes.reduce(function(arr, change) {
                var changeTime = moment(change.Time);
                var earliestStartTime = earliest(schedules);
                var latestEndTime = latest(schedules);

                var key;
                if (changeTime.isBefore(earliestStartTime)) {
                    key = 'Before shift start';
                } else if (changeTime.isAfter(latestEndTime)) {
                    key = 'After shift end';
                } else {
                    var activityWhenChangeOccurred = schedules.find(function(layer) {
                        return layer.StartTime <= change.Time && layer.EndTime >= change.Time;
                    });
                    var activityStart = moment(activityWhenChangeOccurred.StartTime);
                    var activityEnd = moment(activityWhenChangeOccurred.EndTime);
                    // "phone 08:00 - 09:00"
                    key = activityWhenChangeOccurred.Name + ' ' + activityStart.format('HH:mm') + ' - ' + activityEnd.format('HH:mm');
                }
                var existing = arr.find(function(item) {
                    return item.key === key;
                });
                if (existing == null) {
                    arr.push({
                        key: key,
                        header: key,
                        items: [change]
                    });
                } else {
                    existing.items.push(change);
                }

                return arr;
            }, []);
        }

        function buildShiftInfo(schedule, serverNow) {
            if (schedule.length === 0)
                return {
                    start: moment(serverNow).hour(8).minute(0).second(0),
                    stop: moment(serverNow).hour(17).minute(0).second(0),
                    totalSeconds: (17 - 8) * 3600
                };

            var earliestStartTime = earliest(schedule);
            var latestEndTime = latest(schedule);
            var start = earliestStartTime.clone();
            start.startOf('hour');
            var end = latestEndTime.clone();
            end.endOf('hour');

            return {
                start: start,
                end: end,
                totalSeconds: latestEndTime.diff(earliestStartTime, 'seconds')
            };
        }

        vm.scrollTo = function(id) {
            var row = document.querySelector('tr[data-id="' + id + '"]')
            var diamond = document.querySelector('.diamond[data-id="' + id + '"]')
            document.getElementById('rulechanges').scrollTop = row.offsetTop - 100

            var highlighted = document.querySelectorAll('.highlight')
            for (var i = 0; i < highlighted.length; i++) {
                highlighted[i].classList.remove('highlight')
            }
            row.classList.add("highlight")
            diamond.classList.add("highlight")
        }

        function calculateWidth(startTime, endTime, totalSeconds) {
            var diff = moment(endTime).diff(moment(startTime), 'seconds');
            return (diff / totalSeconds) * 100 + '%';
        }

        function buildTimeline(shiftInfo) {
            var timeline = [];
            var currentMoment = shiftInfo.start.clone().add(-1, 'hours');

            var totalHours = shiftInfo.totalSeconds / 3600 + 2;
            var hourPercent = 100 / totalHours;
            for (var i = 1; i < totalHours; i++) {
                currentMoment.add(1, 'hour');
                var percent = hourPercent * i;
                timeline.push({
                    Offset: percent + '%',
                    Time: currentMoment.clone()
                });
            }

            return timeline;
        }

        function earliest(arr) {
            return arr
                .map(function(el) {
                    return moment(el.StartTime);
                })
                .sort(sorter)[0];
        }

        function latest(arr) {
            return arr
                .map(function(el) {
                    return moment(el.EndTime);
                })
                .sort(reverseSorter)[0];
        }

        function sorter(first, second) {
            return first.diff(second, 'seconds');
        }

        function reverseSorter(first, second) {
            return sorter(second, first);
        }
    }
})();