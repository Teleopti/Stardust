'use strict';


(function () {

	angular.module('wfm.seatMap').controller('seatmapObjectPropertyCtrl', seatmapObjectPropertyCtrl);

	seatmapObjectPropertyCtrl.$inject = ['$scope'];

	function seatmapObjectPropertyCtrl($scope) {
		var vm = this;

		vm.updateRolesStatus = function () {
			vm.roles.forEach(function (role) {
				var hasCount = 0;
				role.Status = "none";
				role.Selected = false;

				vm.seats.forEach(function (seat) {
					var roleIndex = seat.Roles.indexOf(role.Id);
					if (roleIndex > -1) hasCount++;
				});
				if (hasCount === vm.seats.length) {
					role.Status = "all";
					role.Selected = true;
				} else if (hasCount < vm.seats.length && hasCount > 0) {
					role.Status = "partial";
				}
			});
		};

		vm.getRoleById = function (id) {
			for (var i = 0; i < vm.roles.length; i++) {
				if (vm.roles[i].Id == id)
					return vm.roles[i];
			}
			return {};
		};

		vm.selectedStatusChanged = function (role) {
			vm.seats.forEach(function (seat) {
				var roleIndex = seat.Roles.indexOf(role.Id);
				if (role.Selected && roleIndex === -1) {
					seat.Roles.push(role.Id);
				}
				if (roleIndex > -1 && !role.Selected) {
					seat.Roles.splice(roleIndex, 1);
				}
				
				//console.log("seat", seat.Name, vm.getCorrespondingRoleNames(seat));
			});
		};

		vm.getCorrespondingRoleNames = function (seat) {
			var seatRoleNames = [];
			seat.Roles.forEach(function (seatRoleId) {
				seatRoleNames.push(vm.getRoleById(seatRoleId).DescriptionText);
			});
			return seatRoleNames;
		};

		$scope.$watch(function () {
			var seatIds = [];
			for (var i = 0; i < vm.seats.length; i++) seatIds.push(vm.seats[i].id);
			return seatIds;
		}, function () {
			vm.updateRolesStatus();
		}, true);

		vm.init = function () {
			vm.updateRolesStatus();
		};

		vm.init();
	};

	angular.module('wfm.seatMap').directive('seatmapObjectProperty', seatmapObjectProperty);

	function seatmapObjectProperty() {
		return {
			controller: 'seatmapObjectPropertyCtrl',
			controllerAs: 'vm',
			bindToController: true,
			scope: {
				roles: '=',
				seats: '='
			},
			templateUrl: 'js/seatManagement/html/objectproperty.html'
		};
	};
})();


/*!
 * Change based on angular-ui-indeterminate
 * https://github.com/angular-ui/ui-indeterminate
 * Version: 1.0.0 - 2015-07-01T06:28:52.320Z
 * License: MIT
 */


(function () {
	'use strict';
	/**
	 * Provides an easy way to toggle a checkboxes indeterminate property
	 *
	 * @example <input type="checkbox" seat-role-indeterminate="isUnkown">
	 */
	angular.module('wfm.seatMap').directive('seatRoleIndeterminate', ['$timeout', function ($timeout) {
		return {
			compile: function (tElm, tAttrs) {
				if (!tAttrs.type || tAttrs.type.toLowerCase() !== 'checkbox') {
					return angular.noop;
				}

				return function ($scope, elm, attrs) {
					$scope.$watch(attrs.seatRoleIndeterminate, function (newVal) {
						elm[0].indeterminate = !!newVal;
						if (newVal) {
							elm.one('click', function () {
								if (elm.attr('checked') !== 'checked') {
									$timeout(function () {
										$scope.$emit('click', elm[0]);
										elm[0].click();
									});
								}
							});
						}
					});
				};
			}
		};
	}
	]);

}());

