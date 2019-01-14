(function (angular, moment) {
    'use strict';
    angular
        .module('wfm.gamification')
        .component('badgeCalculation', {
            templateUrl: 'app/gamification/html/g.component.badgeCalculation.tpl.html',
            controller: badgeCalculationController,
            controllerAs: '$ctrl'
        });

    badgeCalculationController.$inject = ['$translate', '$locale', 'GamificationDataService', 'CurrentUserInfo'];
    function badgeCalculationController($translate, locale, dataService, userInfo) {
        var $ctrl = this;
        var currentTimezone = userInfo.CurrentUserInfo().DefaultTimeZone;
        var dataPurgeDays = 30;

        $ctrl.$onInit = function () {
            fetchJobs();
            fetchPurgeDays();

            $ctrl.dateTimeFormat = locale.DATETIME_FORMATS.short;
            $ctrl.dateFormat = locale.DATETIME_FORMATS.shortDate;
            var startDate = moment.utc().subtract(2, 'days').toDate();
            var endDate = moment.utc().toDate();
            $ctrl.dateRange = { startDate: startDate, endDate: endDate };
            $ctrl.isValid = true;
        };

        $ctrl.validate = function () {
            if (moment($ctrl.dateRange.startDate).toDate() < moment.utc().subtract(dataPurgeDays, 'days').toDate()) {
                $ctrl.isValid = false;
                return $translate.instant('NoGamificationDataForThePeriod');
            }

            $ctrl.isValid = true;
        }

        $ctrl.calculate = function () {
            if ($ctrl.dateRange && $ctrl.dateRange.startDate && $ctrl.dateRange.endDate) {
                var start = moment.tz(moment($ctrl.dateRange.startDate).format('YYYY-MM-DD'), 'UTC').toDate();
                var end = moment.tz(moment($ctrl.dateRange.endDate).format('YYYY-MM-DD'), 'UTC').toDate();
                dataService.startCalculation(start, end).then(function () {
                    fetchJobs();
                });
            }
        }

        $ctrl.allowCalcalution = function () {
            $ctrl.intersected = false;;
            if ($ctrl.dateRange && $ctrl.dateRange.startDate && $ctrl.dateRange.endDate && $ctrl.isValid) {
                $ctrl.intersected = $ctrl.hasIntersection();
                return !$ctrl.intersected;
            }
        }

        function fetchPurgeDays() {
            dataService.fetchPurgeDays().then(function (days) {
                dataPurgeDays = days;
            })
        }

        function fetchJobs() {
            dataService.fetchCalculationJobs().then(function (jobs) {
                if (!$ctrl.jobs) {
                    $ctrl.jobs = jobs;
                    $ctrl.jobs.forEach(convertStartingTimeToCurrentTimezone);
                } else {
                    insertNewJobsFrom(jobs);
                }

                function convertStartingTimeToCurrentTimezone(job) {
                    job.startingTime = utcToTimezone(job.startingTime, currentTimezone);

                    function utcToTimezone(dateStr, timezone) {
                        if (!dateStr || !timezone) return dateStr;

                        var f = 'YYYY-MM-DDTHH:mm:ss';
                        var adjusted = moment.tz(dateStr, 'UTC').tz(timezone).format(f);
                        return adjusted;
                    }
                }
            });
        }

        function insertNewJobsFrom(jobs) {
            var n = jobs.length - $ctrl.jobs.length;
            for (var i = 0; i < n; i++) {
                $ctrl.jobs.unshift(jobs[i]);
            }
        }

        $ctrl.hasIntersection = function () {
            var result = false;
            if ($ctrl.jobs && $ctrl.jobs.length > 0) {
                var runningJobs = $ctrl.jobs.filter(function (item) {
                    return item.status == 'inprogress';
                });

                if (runningJobs.length > 0) {
                    runningJobs.forEach(function (j) {
                        var jobStart = moment(j.startDate,"MMM D, YYYY").toDate();
                        var jobEnd = moment(j.endDate,"MMM D, YYYY").toDate();
                        var start = moment($ctrl.dateRange.startDate,"MMM D, YYYY").toDate();
                        var end = moment($ctrl.dateRange.endDate,"MMM D, YYYY").toDate();

                        if ((start >= jobStart && start <= jobEnd) || (end >= jobStart && end <= jobEnd) || (start <= jobStart && end >= jobEnd)) {
                            result = true;
                        }
                    });
                }
            }

            return result;
        }
    }
})(angular, moment);