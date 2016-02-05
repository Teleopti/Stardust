'use strict';

(function () {

	angular.module('wfm.seatMap').controller('seatmapObjectPropertyCtrl', seatmapObjectPropertyCtrl);

	function seatmapObjectPropertyCtrl() {
		var vm = this;

		vm.rolesStatus = function(role) {
			var appliedRoleCount = 0;
			role.Selected = false;

			vm.seats.forEach(function (seat) {
				var roleIndex = seat.RoleIdList.indexOf(role.Id);
				if (roleIndex > -1) appliedRoleCount++;
			});

			if (appliedRoleCount === vm.seats.length && vm.seats.length > 0) {
				role.Selected = true;
				return false;

			} else if (appliedRoleCount < vm.seats.length && appliedRoleCount > 0) {
				return true;
			}
		};

		vm.selectedStatusChanged = function (role) {
			vm.seats.forEach(function (seat) {
				var roleIndex = seat.RoleIdList.indexOf(role.Id);
				if (role.Selected && roleIndex === -1) {
					seat.RoleIdList.push(role.Id);
				}
				if (roleIndex > -1 && !role.Selected) {
					seat.RoleIdList.splice(roleIndex, 1);
				}
			});
		};
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

