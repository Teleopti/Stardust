(function (angular) {
	'use strict';

	angular.module('wfm.people')
		.filter('filesize', function () {
			var units = ['B', 'KB', 'MB', 'GB'];
			return function (bytes, precision) {

				if (isNaN(parseFloat(bytes))) {
					return;
				}

				if (bytes < 1) {
					return '0 B';
				}

				if (isNaN(precision)) {
					precision = 1;
				}

				var unitIndex = Math.floor(Math.log(bytes) / Math.log(1024));

				if (unitIndex > units.length - 1) {
					unitIndex = units.length - 1;
				}

				var value = bytes / Math.pow(1024, unitIndex);

				return value.toFixed(precision) + ' ' + units[unitIndex];
			};
		});
})(angular);