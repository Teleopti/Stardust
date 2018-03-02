(function () {
    'use strict';
    angular
        .module('wfm.gamification')
        .component('badgeCalculation', {
            templateUrl: 'app/gamification/html/g.component.badgeCalculation.tpl.html',
            controller: badgeCalculationController,
            controllerAs: '$ctrl',
            bindings: {
                Binding: '=',
            },
        });

    badgeCalculationController.$inject = ['GamificationDataService', 'CurrentUserInfo'];
    function badgeCalculationController(dataService, userInfo) {
        var $ctrl = this;
        var currentTimezone = userInfo.CurrentUserInfo().DefaultTimeZone;

        $ctrl.$onInit = function () {
            fetchJobs();

            $ctrl.templateType = 'popup';
            var startDate = moment.utc().subtract(2, 'days').toDate();
            var endDate = moment.utc().toDate();
            $ctrl.dateRange = { startDate: startDate, endDate: endDate };
            $ctrl.dateRangeCustomValidators = [{
                key: 'startDateValidation',
                message: 'StartDateCannotEarlierThanLastPurgeDate',
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
            return $ctrl.dateRange && $ctrl.dateRange.startDate && $ctrl.dateRange.endDate;
        }

        $ctrl.$onChanges = function (changesObj) { };
        $ctrl.$onDestroy = function () { };

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
    }
})();