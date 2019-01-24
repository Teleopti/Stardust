(function () {
        'use strict';

        angular
            .module('wfm.rta')
            .controller('AdjustAdherenceController', AdjustAdherenceController);

        AdjustAdherenceController.$inject = ['CurrentUserInfo', 'rtaStateService', '$http', '$scope', '$state'];

        function AdjustAdherenceController(currentUserInfo, rtaStateService, $http, $scope, $state) {
            var vm = this;
            vm.showAdjustToNeutralForm = false;
            vm.adjustedPeriods = [];

            currentUserInfo.Load().then(function (data) {
                preselectDateAndTime(data);
                buildSelectedPeriod();

                $http.get('../api/Adherence/AdjustedPeriods')
                    .then(function (response) {
                        var data = response.data;
                        vm.adjustedPeriods = buildAdjustedPeriods(data);
                    });
            });

            function preselectDateAndTime(data) {
                vm.startDate = moment(new Date()).add(-1, 'days');
                vm.endDate = moment(new Date()).add(-1, 'days');

                vm.startTime = moment(new Date()).set({h: 8, m: 0});
                vm.endTime = moment(new Date()).set({h: 18, m: 0});

                vm.showMeridian = data.DateTimeFormat.ShowMeridian;
            }

            function buildSelectedPeriod() {
                var startTime = moment(vm.startDate).format('L') + ' ' + moment(vm.startTime).format('LT');
                var endTime = moment(vm.endDate).format('L') + ' ' + moment(vm.endTime).format('LT');
                vm.selectedPeriod = startTime + ' - ' + endTime;
            }

            function buildAdjustedPeriods(data) {
                return data.map(function (period) {
                    return {
                        StartTime: moment(period.StartTime).format('L LT'),
                        EndTime: moment(period.EndTime).format('L LT')
                    }
                })
            }

            $scope.$watch(function () { return vm.startDate; }, buildSelectedPeriod);
            $scope.$watch(function () { return vm.startTime; }, buildSelectedPeriod);
            $scope.$watch(function () { return vm.endDate; }, buildSelectedPeriod);
            $scope.$watch(function () { return vm.endTime; }, buildSelectedPeriod);

            vm.adjustToNeutral = function () {
                $http.post('../api/Adherence/AdjustPeriod', {
                    StartDateTime: formatDateTime(vm.startDate, vm.startTime),
                    EndDateTime: formatDateTime(vm.endDate, vm.endTime)
                })
            };

            function formatDateTime(date, time) {
                return moment(date).format('YYYY-MM-DD') + ' ' + moment(time).format('HH:mm');
            }

            vm.toggleAdjustToNeutralForm = function () {
                vm.showAdjustToNeutralForm = !vm.showAdjustToNeutralForm;
            };

            vm.goToAgents = rtaStateService.goToAgents;
            vm.goToOverview = rtaStateService.goToOverview;
            vm.goToHistoricalOverview = function () {
                $state.go('rta-historical-overview', {});
            };
        }
    }
)();
