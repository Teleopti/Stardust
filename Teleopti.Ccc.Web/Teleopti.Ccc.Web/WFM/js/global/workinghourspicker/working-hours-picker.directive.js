(function() {
    'use strict';

    angular.module('wfm.workinghourspicker', ['wfm.timerangepicker']).directive('workingHoursPicker', [
        '$q', '$translate', '$filter',
		function ($q, $translate, $filter) {
		    return {
		        restrict: 'E',
		        scope: {
		            workingHours: '='
		        },
		        templateUrl: 'js/global/workinghourspicker/working-hours-picker.tpl.html',
		        link: postLink

		    };

		    function createEmptyWorkingPeriod(startTime, endTime) {
		    	var weekdaySelections = [];
		    	var startDow = (moment.localeData()._week) ? moment.localeData()._week.dow : 0;

		    	for (var i = 0; i < 7; i++) {
		    		var curDow = (startDow + i) % 7;
		    		weekdaySelections.push({ WeekDay: curDow, Checked: false });
		    	}

		    	return { StartTime: startTime, EndTime: endTime, WeekDaySelections: weekdaySelections };
		    }

		    function postLink(scope, elem, attrs) {

		        scope.enforceRadioBehavior = enforceRadioBehavior;
		        scope.addEmptyWorkingPeriod = addEmptyWorkingPeriod;
		        scope.removeWorkingPeriod = removeWorkingPeriod;
			    scope.getTimerangeDisplay = getTimerangeDisplay;

			    scope.disableNextDay = true;

		        var weekDays = createEmptyWorkingPeriod().WeekDaySelections;
		        var translations = [];
		        var i;
		        for (i = 0; i < weekDays.length; i++) {
		            translations.push($translate($filter('showWeekdays')(weekDays[i])));
		        }

		        $q.all(translations).then(function (ts) {
		            for (i = 0; i < weekDays.length; i++) {
		                weekDays[i].Text = ts[i];
		            }
		            scope.weekDays = weekDays;
		        });

		        function enforceRadioBehavior(refIndex, weekDay) {
		            clearConflictWorkingHourSelection(scope.workingHours, refIndex, weekDay);
		        }

		        function addEmptyWorkingPeriod(startTime, endTime) {			      
		            scope.workingHours.push(createEmptyWorkingPeriod(angular.copy(startTime), angular.copy(endTime)));
		        }

		        function removeWorkingPeriod(index) {
		            scope.workingHours.splice(index, 1);
		        }

		        function clearConflictWorkingHourSelection(workingHours, refIndex, weekDay) {
		            angular.forEach(workingHours, function (workingHour, i) {
		                if (i == refIndex) return;
		                angular.forEach(workingHour.WeekDaySelections, function (d) {
		                    if (weekDay == d.WeekDay) d.Checked = false;
		                });
		            });
		        };

		        function getTimerangeDisplay(startTime, endTime) {
			        var startTimeMoment = moment(startTime),
				        endTimeMoment = moment(endTime);
			        if (startTimeMoment.isSame(endTimeMoment, 'day')) {
				        return startTimeMoment.format('LT') + ' - ' + endTimeMoment.format('LT');
			        } else {
			        	return startTimeMoment.format('LT') + ' - ' + endTimeMoment.format('LT') + ' +1';
			        }
				}
		    }
		}
    ]);

})();