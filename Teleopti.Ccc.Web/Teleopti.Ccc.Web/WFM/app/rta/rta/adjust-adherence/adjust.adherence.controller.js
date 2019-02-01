(function () {
        'use strict';

        angular
            .module('wfm.rta')
            .controller('AdjustAdherenceController', AdjustAdherenceController);

        AdjustAdherenceController.$inject = ['CurrentUserInfo', 'rtaStateService', '$http', '$scope', '$state', '$timeout'];

        function AdjustAdherenceController(currentUserInfo, rtaStateService, $http, $scope, $state, $timeout) {
            var vm = this;
            vm.showAdjustToNeutralForm = false;
            vm.adjustedPeriods = [];

            currentUserInfo.Load().then(function (data) {
                preselectDateAndTime(data);
                buildSelectedPeriod();
                loadData();
            });

            function preselectDateAndTime(data) {
                vm.startDate = moment(new Date()).add(-1, 'days');
                vm.endDate = moment(new Date()).add(-1, 'days');

                vm.startTime = moment(new Date()).set({h: 8, m: 0});
                vm.endTime = moment(new Date()).set({h: 18, m: 0});

                vm.showMeridian = data.DateTimeFormat.ShowMeridian;
            }

            function buildSelectedPeriod() {
                var startTime = moment(vm.startTime).isValid() ? moment(vm.startTime).format('LT') : '?';
                var endTime = moment(vm.endTime).isValid() ? moment(vm.endTime).format('LT') : '?';
                var start = moment(vm.startDate).format('L') + ' ' + startTime;
                var endTime = moment(vm.endDate).format('L') + ' ' + endTime;
                vm.selectedPeriod = start + ' - ' + endTime;
            }

            function loadData() {
                $http.get('../api/Adherence/AdjustedPeriods')
                    .then(function (response) {
                        var data = response.data;
                        vm.adjustedPeriods = buildAdjustedPeriods(data);
                    });
            }

            function buildAdjustedPeriods(data) {
                return data.map(function (period) {
                    return {
                        StartTime: moment(period.StartTime).format('L LT'),
                        EndTime: moment(period.EndTime).format('L LT')
                    }
                })
            }

            vm.adjustToNeutral = function () {
                if(!(moment(vm.startTime).isValid() && moment(vm.endTime).isValid()))
                    return;
                $http.post('../api/Adherence/AdjustPeriod', {
                    StartDateTime: formatDateTime(vm.startDate, vm.startTime),
                    EndDateTime: formatDateTime(vm.endDate, vm.endTime)
                }).then(loadData);
            };

            function formatDateTime(date, time) {
                return moment(date).format('YYYY-MM-DD') + ' ' + moment(time).format('HH:mm');
            }

            $scope.$watch(function () {
                return vm.startDate;
            }, function () {
                if (shouldAutoFixDate())
                    vm.endDate = vm.startDate;
                if (shouldAutoFixTime())
                    vm.endTime = vm.startTime;
                buildSelectedPeriod();
            });

            $scope.$watch(function () {
                return vm.startTime;
            }, function () {
                if (shouldAutoFixTime()) {
                    $timeout(function() {
                        if(!moment(vm.startTime).isValid())
                            return;
                        vm.endTime = vm.startTime;
                    }, 1000)
                }
                buildSelectedPeriod();
            });

            $scope.$watch(function () {
                return vm.endDate;
            }, function () {
                if (shouldAutoFixDate())
                    vm.startDate = vm.endDate;
                if (shouldAutoFixTime())
                    vm.startTime = vm.endTime;
                buildSelectedPeriod();
            });

            $scope.$watch(function () {
                return vm.endTime;
            }, function () {
                if (shouldAutoFixTime()) {
                    $timeout(function(){
                        if(!moment(vm.endTime).isValid())
                            return;
                        vm.startTime = vm.endTime;
                    }, 1000);
                }
                buildSelectedPeriod();
            });

            function shouldAutoFixTime() {
                var isSameDay = moment(vm.startDate).isSame(moment(vm.endDate), 'day');
                var isStartTimeAfterEndTime = moment(vm.startTime).isAfter(moment(vm.endTime));

                return (isSameDay && isStartTimeAfterEndTime);
            }

            function shouldAutoFixDate() {
                return moment(vm.startDate).isAfter(moment(vm.endDate), 'day');
            }
            
            vm.toggleAdjustToNeutralForm = function () {vm.showAdjustToNeutralForm = !vm.showAdjustToNeutralForm;};

            vm.goToAgents = rtaStateService.goToAgents;
            vm.goToOverview = rtaStateService.goToOverview;
            vm.goToHistoricalOverview = function () {
                $state.go('rta-historical-overview', {});
            };
        }
    }
)();
