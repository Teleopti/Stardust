(function() {

	'use strict';

	angular.module('wfm.outbound')
		.filter('showWeekdays', ['$translate', showWeekdays])
		.filter('showPhase', [showPhase])
		.filter('warningMessage', [warningMessage])
		.filter('showTimespan', showTimespan);

	function warningMessage() {
		return function(info) {
			var mapping= {
				OutboundOverstaffRule: 'Overstaffed',
				OutboundUnderSLARule: 'UnderSLA'
			}
			return mapping[info];
		}
	};

	function showPhase() {
		return function(statusCode) {
			var mapping = {
				1: 'Planned',
				2: 'Scheduled',
				4: 'Ongoning',
				8: 'Done'
			}
			return mapping[statusCode];
		}

	};

	function showWeekdays($translate) {	    
	    return function (input) {

	        var localeData = moment.localeData($translate.use());	     
	        var weekdays = localeData._weekdaysShort;

			if (input.WeekDay >= weekdays.length) {
				return "";
			}			
			return weekdays[input.WeekDay];
		};
	}

	function showTimespan() {
		return function (input) {
			if (input === null) return ' -- : -- ';

			var hours = Math.floor(input);
			var minutes = Math.ceil((input - hours) * 60);
			var minuteString = ('00' + minutes.toString()).slice(-2);
			return hours + ' : ' + minuteString;
		}
	}


})();