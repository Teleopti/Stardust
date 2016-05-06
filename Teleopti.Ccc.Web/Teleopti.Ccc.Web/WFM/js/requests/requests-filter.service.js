﻿(function() {
	"use strict";

	angular.module('wfm.requests').service('RequestsFilter', [
			function() {
				var vm = this;
				vm.Filters = [];

				vm.RemoveFilter = function (filterName) {
					for (var i = 0; i < vm.Filters.length; i++) {
						var filter = vm.Filters[i];
						if (filter.hasOwnProperty(filterName)) {
							vm.Filters.splice(i, 1);
						}
					}
				}

				vm.SetFilter = function(name, filter) {
					var nameInLowerCase = name.trim().toLowerCase();
					var expectedFilterNames = ["status", "absence", "subject", "message"];

					if (expectedFilterNames.indexOf(nameInLowerCase) > -1) {
						var filterName = nameInLowerCase.charAt(0).toUpperCase()
							+ nameInLowerCase.slice(1);
						vm.RemoveFilter(filterName);
						if (filter == undefined || filter.trim().length === 0) return;

						var filterObj = {};
						filterObj[filterName] = filter.trim();
						vm.Filters.push(filterObj);
					}
				}

				vm.ResetFilter = function() {
					vm.Filters = [];
				}
			}
		]
	);
})();