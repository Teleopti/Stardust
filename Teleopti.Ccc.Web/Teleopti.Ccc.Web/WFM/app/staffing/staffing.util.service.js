(function () {
    'use strict';

    angular
        .module('wfm.staffing')
        .factory('UtilService', utilService);

    utilService.inject = ['$translate', '$filter', '$q'];
    function utilService($translate, $filter, $q) {
        var service = {
            prepareStaffingData: prepareStaffingData,
            roundDataToOneDecimal: roundDataToOneDecimal,
            prepareSettings: prepareSettings,
            prepareSuggestedStaffingData: prepareSuggestedStaffingData
        };

        return service;

        ////////////////
        function prepareSettings(vmSettings) {
            var preparedSettings = {};
            preparedSettings.Id = vmSettings.Compensation;
            preparedSettings.MaxMinutesToAdd = convertOvertimeHoursToMinutes(vmSettings.MaxMinutesToAdd);
            preparedSettings.MinMinutesToAdd = convertOvertimeHoursToMinutes(vmSettings.MinMinutesToAdd);
            return preparedSettings;
        }

        function convertOvertimeHoursToMinutes(dateTime) {
            return dateTime;
            //return (dateTime.getMinutes() + dateTime.getHours() * 60);
        }

        function roundArrayContents(input, decimals) {
            var roundedInput = [];
            input.forEach(function (elm) {
                if (typeof elm != 'number') return
                roundedInput.push(parseFloat(elm.toFixed(decimals)));
            })
            return roundedInput;
        }

        function roundDataToOneDecimal(input) {
            input = roundArrayContents(input, 1)
            return input;
        }

        function prepareStaffingData(data) {
            var staffingData = {};

            staffingData.time = [];
            staffingData.scheduledStaffing = [];
            staffingData.forcastedStaffing = [];
            staffingData.absoluteDifference = [];

            staffingData.scheduledStaffing = roundDataToOneDecimal(data.DataSeries.ScheduledStaffing);
            staffingData.forcastedStaffing = roundDataToOneDecimal(data.DataSeries.ForecastedStaffing);
            staffingData.absoluteDifference = data.DataSeries.AbsoluteDifference;
            staffingData.forcastedStaffing.unshift($translate.instant('ForecastedStaff'));
            staffingData.scheduledStaffing.unshift($translate.instant('ScheduledStaff'));

            angular.forEach(data.DataSeries.Time,
                function (value, key) {
                    staffingData.time.push($filter('date')(value, 'shortTime'));
                },
                staffingData.time);
            staffingData.time.unshift('x');

            return staffingData;
        }

        function prepareSuggestedStaffingData(original, data) {
            var staffingData = original;

            staffingData.suggested = {};
            staffingData.suggested.forcastedStaffing = [];
            staffingData.suggested.scheduledStaffing = [];
            staffingData.suggested.absoluteDifference = [];

            staffingData.suggested.scheduledStaffing = roundDataToOneDecimal(data.DataSeries.ScheduledStaffing);
            staffingData.suggested.forcastedStaffing = roundDataToOneDecimal(data.DataSeries.ForecastedStaffing);
            staffingData.suggested.absoluteDifference = data.DataSeries.AbsoluteDifference;
            staffingData.suggested.forcastedStaffing.unshift($translate.instant('ForecastedStaff'));
            staffingData.suggested.scheduledStaffing.unshift($translate.instant('Suggested scheduled agents'));

            return staffingData;
        }
    }
})();