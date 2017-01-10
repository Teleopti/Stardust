(function() {
	'use strict';
	angular
		.module('wfm.rta')
		.filter('agentFilter', agentFilter);

	function agentFilter() {
		var filter = this;

		function escapeRegExp(str) {
			return str.replace(/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g, "\\$&");
		}
		return function(data, input, includes) {
			var matchedItems = data.slice();
			var keywords = input.split(' ');
			keywords.forEach(function(keyword) {
				for (var i = 0; i < data.length; i++) {
					var item = data[i];
					var matched = false;
					for (var property in item) {
						if (includes !== undefined && includes.indexOf(property) === -1)
							continue;
						if (item[property] !== null && item[property] !== undefined)
							matched = matched || (item[property].toString().search(new RegExp(escapeRegExp(keyword), "i")) !== -1 ? true : false);
					}
					if (matched === false) {
						var index = matchedItems.indexOf(item);
						if (index > -1) {
							matchedItems.splice(index, 1);
						}
					}
				}
			});
			return matchedItems;
		};
		return filter;
	};
})();
