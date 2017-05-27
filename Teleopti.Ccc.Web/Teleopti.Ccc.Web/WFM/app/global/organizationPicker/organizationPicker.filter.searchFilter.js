(function (angular) {
	'use strict';

	angular.module('wfm.organizationPicker')
		.filter('searchFilter', searchFilter);

	function searchFilter() {
		return function (input, query) {
			var r = new RegExp(query, 'i')
			return input.replace(r, '<span class="match">$&</span>')
		}
	}
})(angular);