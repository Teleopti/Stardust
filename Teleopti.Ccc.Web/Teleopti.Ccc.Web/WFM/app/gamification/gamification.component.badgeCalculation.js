(function (angular, moment) {
    'use strict';
    angular
        .module('wfm.gamification')
        .component('badgeCalculation', {
            templateUrl: 'app/gamification/html/g.component.badgeCalculation.tpl.html',
            controller: badgeCalculationController,
            controllerAs: '$ctrl'
        });

    badgeCalculationController.$inject = ['$locale', 'GamificationDataService', 'CurrentUserInfo'];
    function badgeCalculationController(locale, dataService, userInfo) {
        var $ctrl = this;
        var currentTimezone = userInfo.CurrentUserInfo().DefaultTimeZone;

        $ctrl.$onInit = function () {
            fetchJobs();

            $ctrl.dateTimeFormat = locale.DATETIME_FORMATS.medium;
            $ctrl.templateType = 'popup';
            var startDate = moment.utc().subtract(2, 'days').toDate();
            var endDate = moment.utc().toDate();
            $ctrl.dateRange = { startDate: startDate, endDate: endDate };
            $ctrl.dateRangeCustomValidators = [{
                key: 'NoGamificationDataForThePeriod',
                message: 'NoGamificationDataForThePeriod',
                validate: function (start, end) {
                    return moment(start).toDate() > moment.utc().subtract(30, 'days').toDate();
                }
            }];
        };

        $ctrl.calculate = function () {
            if ($ctrl.dateRange && $ctrl.dateRange.startDate && $ctrl.dateRange.endDate) {
                var start = moment($ctrl.dateRange.startDate).toDate();
                var end = moment($ctrl.dateRange.endDate).toDate();
                dataService.startCalculation(start, end).then(function () {
                    fetchJobs();
                });
            }
        }

        $ctrl.allowCalcalution = function () {
            if ($ctrl.dateRange && $ctrl.dateRange.startDate && $ctrl.dateRange.endDate) {
                $ctrl.intersected = hasIntersection();
                return !$ctrl.intersected;
            }
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

        function hasIntersection() {
            var result = false;
            if ($ctrl.jobs && $ctrl.jobs.length > 0) {
                var runningJobs = $ctrl.jobs.filter(function (item) {
                    return item.status == 'inprogress';
                });

                if (runningJobs.length > 0) {

                    runningJobs.forEach(function (j) {
                        if (moment(j.endDate).toDate() > moment($ctrl.dateRange.startDate).toDate() && moment($ctrl.dateRange.endDate).toDate() > moment(j.startDate).toDate()) {
                            result = true;
                        }
                    });
                }
            }

            return result;
        }
    }
})(angular, moment);