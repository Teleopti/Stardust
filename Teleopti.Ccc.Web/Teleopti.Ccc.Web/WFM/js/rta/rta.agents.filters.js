﻿
(function() {
	'use strict';
	var rtaAgentsFilters = angular.module('wfm.rta');

	rtaAgentsFilters.filter('agentFilter', [
			function() {
				return function(data, input) {
					var matchedItems = [];
					var keywords = input.split(' ');
					for (var i = 0; i < data.length; i++) {
						var item = data[i];
						for (var property in item) {
							var matched = true;
							keywords.forEach(function(keyword) {
								if (item[property] === null) {
									matched = false;
									return;
								}
								matched = matched && (item[property].toString().search(new RegExp(keyword, "i")) !== -1 ? true : false);
							});
							if (matched === true && matchedItems.indexOf(item) === -1)
								matchedItems.push(item);
						}
					}
					return matchedItems;
				};
			}
		])
		.filter('timeFormatGridCellFilter', function() {
			return function(value) {
				return moment.utc(value).format('YYYY-MM-DD HH:mm');
			}
		})
		.filter('timeDurationGridCellFilter', function() {
			return function(value) {
				var durationInSeconds = moment.duration(value, 'seconds');
				return (Math.floor(durationInSeconds.asHours()) + moment(durationInSeconds.asMilliseconds()).format(':mm:ss'));
			}
		});
})();
