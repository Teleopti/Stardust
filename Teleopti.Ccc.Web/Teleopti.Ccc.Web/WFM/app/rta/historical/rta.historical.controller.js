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
                var shiftInfo = buildShiftInfo(data.Schedules);

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

                if (toggles.RTA_SolidProofWhenManagingAgentAdherence_39351) {
                    var states = [];
                    states = states.concat(offChangesFor(startOfShift, totalSeconds, "2016-10-10 07:00", "", "In adherence", { activity: "Before shift start", time: '' }));
                    states = states.concat(phoneChangesFor(startOfShift, totalSeconds, 10, "2016-10-10 08:01", "Phone", "In adherence", { activity: "Phone", time: "08:00 - 09:00" }));
                    states = states.concat(phoneChangesFor(startOfShift, totalSeconds, 5, "2016-10-10 09:01", "Break", "Neutral", { activity: "Break", time: "09:00 - 09.30" }));
                    states = states.concat(phoneChangesFor(startOfShift, totalSeconds, 5, "2016-10-10 09:31", "Phone", "In adherence", { activity: "Phone", time: "09:30 - 11:00" }));
                    states = states.concat(offChangesFor(startOfShift, totalSeconds, "2016-10-10 10:01", "Phone", "Out of adherence", { activity: "Phone", time: "09:30 - 11:00" }));
                    states = states.concat(phoneChangesFor(startOfShift, totalSeconds, 8, "2016-10-10 10:16", "Phone", "In adherence", { activity: "Phone", time: "09:30 - 11:00" }));
                    states = states.concat(phoneChangesFor(startOfShift, totalSeconds, 2, "2016-10-10 11:04", "Lunch", "Neutral", { activity: "Lunch", time: "11:00 - 12:00" }));
                    states = states.concat(phoneChangesFor(startOfShift, totalSeconds, 1, "2016-10-10 11:55", "Lunch", "Neutral", { activity: "Lunch", time: "11:00 - 12:00" }));
                    states = states.concat(phoneChangesFor(startOfShift, totalSeconds, 20, "2016-10-10 12:01", "Phone", "In adherence", { activity: "Phone", time: "12:00 - 14:00" }));
                    states = states.concat(phoneChangesFor(startOfShift, totalSeconds, 9, "2016-10-10 14:01", "Break", "Neutral", { activity: "Break", time: "14:00 - 15:00" }));
                    states = states.concat(offChangesFor(startOfShift, totalSeconds, "2016-10-10 14:55", "Break", "In adherence", { activity: "Break", time: "14:00 - 15:00" }));
                    states = states.concat(phoneChangesFor(startOfShift, totalSeconds, 18, "2016-10-10 15:16", "Phone", "In adherence", { activity: "Phone", time: "15:00 - 17:00" }));
                    states = states.concat(offChangesFor(startOfShift, totalSeconds, "2016-10-10 17:05", "After shift end", "In adherence", { activity: "After shift end", time: "" }));

                    states = states.map(function(state, i) {
                        state.id = i
                        var nextState = states[i + 1]
                        if (nextState != null) {
                            state.width = calculateWidth(state.time, nextState.time, totalSeconds)
                        } else {
                            state.width = calculateWidth(state.time, shiftInfo.end, totalSeconds)
                        }
                        return state;
                    })

                    vm.rulechanges = states
                    vm.rulechanges2 = states.reduce(function(arr, state) {
                        var key = state.key
                        var existing = arr.find(function(item) {
                            console.log(item.key, key)
                            return item.key.activity === key.activity && item.key.time === key.time
                        })
                        if (existing == null) {
                            arr.push({
                                key: key,
                                header: key,
                                items: [state]
                            })
                        } else {
                            existing.items.push(state)
                        }

                        return arr
                    }, [])
                }
            });

        function offChangesFor(startOfShift, totalSeconds, time, activity, adherence, key) {
            return {
                offset: calculateWidth(startOfShift, time, totalSeconds),
                state: "Logged off",
                time: moment(time),
                activity: activity,
                activityColor: (function() {
                    if (activity == "Phone") {
                        return "lightgreen"
                    } else if (activity == "Lunch") {
                        return "yellow"
                    } else if (activity == "Break") {
                        return "red"
                    }
                }()),
                rule: activity !== "" ? activity + '-Logged off rule' : "Logged off rule",
                ruleColor: "darksalmon",
                adherence: adherence,
                adherenceColor: (function() {
                    if (adherence === "In adherence") {
                        return "green";
                    } else if (adherence === "Neutral") {
                        return "yellow";
                    } else if (adherence === "Out of adherence") {
                        return "red"
                    }
                }()),
                key: key
            }
        }

        function phoneChangesFor(startOfShift, totalSeconds, num, time, activity, adherence, key) {
            var arr = [];
            for (var i = 0; i < num; i++) {
                var state = i % 2 == 0 ? 'Call' : 'Ready'
                var m = moment(time, 'YYYY-MM-DD HH:mm')
                m.add(6 * i, 'm')
                arr.push({
                    offset: calculateWidth(startOfShift, m.format('YYYY-MM-DD HH:mm:ss'), totalSeconds),
                    state: state,
                    time: m,
                    activity: activity,
                    activityColor: (function() {
                        if (activity == "Phone") {
                            return "lightgreen"
                        } else if (activity == "Lunch") {
                            return "yellow"
                        } else if (activity == "Break") {
                            return "red"
                        }
                    }()),
                    rule: activity + '-' + state + ' rule',
                    ruleColor: adherence === "In adherence" ? "darkgreen" : "burlywood",
                    adherence: adherence,
                    adherenceColor: (function() {
                        if (adherence === "In adherence") {
                            return "green";
                        } else if (adherence === "Neutral") {
                            return "yellow";
                        } else if (adherence === "Out of adherence") {
                            return "red"
                        }
                    }()),
                    key: key
                });
            }

            return arr;
        }

        function buildShiftInfo(schedule) {
            if (schedule.length === 0)
                return {
                    start: moment().hour(8).minute(0).second(0),
                    stop: moment().hour(17).minute(0).second(0),
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