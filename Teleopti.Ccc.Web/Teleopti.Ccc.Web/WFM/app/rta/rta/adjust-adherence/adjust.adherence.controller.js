(function () {
    'use strict';

    angular
        .module('wfm.rta')
        .controller('AdjustAdherenceController', AdjustAdherenceController);

    AdjustAdherenceController.$inject = ['CurrentUserInfo', '$scope'];

    function AdjustAdherenceController(currentUserInfo, $scope) {
        var vm = this;
        var startDate, startTime, endDate, endTime;
        vm.showAdjustToNeutralForm = false;

        currentUserInfo.Load().then(function (data) {
            vm.startDate = moment(new Date()).add(-1, 'days');
            vm.endDate = moment(new Date()).add(-1, 'days');

            vm.startTime = moment(new Date()).set({h: 8, m: 0});
            vm.endTime = moment(new Date()).set({h: 18, m: 0});

            vm.showMeridian = data.DateTimeFormat.ShowMeridian;

            buildSelectedPeriod();
        });

        $scope.$watch(function () {
            return vm.startDate;
        }, function (newValue, oldValue) {
            startDate = moment(newValue);
            buildSelectedPeriod();
        });

        $scope.$watch(function () {
            return vm.startTime;
        }, function (newValue, oldValue) {
            startTime = moment(newValue);
            buildSelectedPeriod();
        });

        $scope.$watch(function () {
            return vm.endDate;
        }, function (newValue, oldValue) {
            endDate = moment(newValue);
            buildSelectedPeriod();
        });

        $scope.$watch(function () {
            return vm.endTime;
        }, function (newValue, oldValue) {
            endTime = moment(newValue);
            buildSelectedPeriod();
        });

        function buildSelectedPeriod() {
            if (startDate && startTime && endDate && endTime)
                vm.selectedPeriod = startDate.format('L') + ' ' + startTime.format('LT') + ' - ' + endDate.format('L') + ' ' + endTime.format('LT');
        }

        vm.toggleAdjustToNeutralForm = function () {
            vm.showAdjustToNeutralForm = !vm.showAdjustToNeutralForm;
        }
    }
})();
