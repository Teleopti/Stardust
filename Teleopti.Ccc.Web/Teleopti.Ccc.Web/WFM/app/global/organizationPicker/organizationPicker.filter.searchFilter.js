(function (angular) {
	'use strict';

	angular.module('wfm.organizationPicker')
		.filter('searchFilter', ['$sce', searchFilter]);

	var entityMap = {
		'&': '&amp;',
		'<': '&lt;',
		'>': '&gt;',
		'"': '&quot;',
		"'": '&#39;',
		'/': '&#x2F;',
		'`': '&#x60;',
		'=': '&#x3D;'
	};

	function escape(text) {
		if (!angular.isString(text))
			return ''
		return text.replace(/[&<>"'`=\/]/g, function (s) { return entityMap[s] })
	}

	function searchFilter($sce) {
		return function (input, query) {
			if (query === '')
				return escape(input)

			var r = new RegExp('(' + query + ')', 'ig')

			if (input.search(r) !== -1) {
				return $sce.trustAsHtml(escape(input.replace(r, '{{||{{$&}}||}}')).replace(/({{\|\|{{)/g, '<span class="match">').replace(/(}}\|\|}})/g, '</span>'))
			}

			return escape(input)
		}
	}
})(angular);