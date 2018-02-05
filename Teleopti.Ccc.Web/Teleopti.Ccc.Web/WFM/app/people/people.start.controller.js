(function () {
	'use strict';

	angular.module('wfm.people')

		.controller('PeopleStart', ['$scope', PeopleStartController]);

	function PeopleStartController($scope) {

		$scope.getNumberOfCheckedRoles = function (roles) {
			var checkedRoles = 0;
			angular.forEach(roles, function (value, key) {
				if (value.checked) {
					checkedRoles++;
				}
			});

			return checkedRoles;
		};

		$scope.roles = [
			{ name: 'Agent' },
			{ name: 'Team leader' },
			{ name: 'London, site admin' },
			{ name: 'Partner Web' },
			{ name: 'Store staff' },
			{ name: 'Super Administrator Role' }
		];

		$scope.currentRoles = [];


		$scope.people = [
			{
				firstName: 'John',
				id: '1',
				lastName: 'Doe',
				phone: '123456123',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: true },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Jennie',
				id: '2',
				lastName: 'Flowers',
				role: 'Agent',
				phone: '123456123',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: true }
				]
			},
			{
				firstName: 'Bert',
				id: '3',
				lastName: 'Erickson',
				role: 'Agent',
				phone: '123456123',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Carolyn',
				id: '4',
				lastName: 'Garrett',
				role: 'Agent',
				phone: '123456123',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Christian',
				id: '5',
				lastName: 'Reeves',
				role: 'Agent',
				phone: '123456123',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Sergio',
				id: '6',
				lastName: 'Evans',
				role: 'Agent',
				phone: '123456123',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Edwin',
				id: '7',
				lastName: 'Morrison',
				role: 'Agent',
				phone: '123456123',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Tom',
				id: '8',
				lastName: 'Vaughn',
				role: 'Agent',
				phone: '123456123',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Ollie',
				id: '9',
				lastName: 'Norris',
				role: 'Agent',
				phone: '123456123',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Greg',
				id: '10',
				lastName: 'Harrington',
				role: 'Agent',
				phone: '123456123',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Clyde',
				id: '11',
				lastName: 'Wilkins',
				role: 'Agent',
				phone: '123456123',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
		];
		$scope.disabledOptions = true;

		$scope.checkExistingRoles = function (role) {
			var exists = false;
			angular.forEach($scope.currentRoles, function (value, key) {
				if (value.name === role.name) {
					exists = true;
				}
			});
			return exists;
		}


		$scope.checkRolesNotOnAll = function (role) {
			var exists = false;
			angular.forEach($scope.currentRoles, function (value, key) {
				if (value.name === role.name && value.usedBy === $scope.itemArr.length) {
					exists = true;
				}
			});
			return exists;
		}

		$scope.paginationOptions = { pageNumber: 1, totalPages: 7 };
		$scope.getPageData = function (pageIndex) {
			angular.log(pageIndex);
		};

		$scope.multi = false;
		$scope.itemArr = [];
		$scope.assertMulti = function (item) {

			$scope.currentRoles.length = 0;
			var indexOfItem = $scope.itemArr.indexOf(item);

			if (indexOfItem !== -1) {
				$scope.itemArr.splice(indexOfItem, 1);
			} else {
				$scope.itemArr.push(item);
			}

			$scope.temp = [];

			angular.forEach($scope.itemArr, function (person, key) {
				angular.forEach(person.roles, function (role, key) {
					if (role.checked) {
						if ($scope.checkIfRoleExistInCurrentRoles(role) === false) {
							role.usedBy = 1;
							$scope.temp = angular.copy(role);
							$scope.currentRoles.push($scope.temp);
						} else {
							$scope.incrementUsedBy(role);
						}
					}
				});
			});
		};

		$scope.isIndeterminate = function (role) {
			return role.usedBy !== $scope.itemArr.length;
		};

		$scope.isChecked = function (role) {
			return role.usedBy === $scope.itemArr.length;
		};

		$scope.incrementUsedBy = function (role) {
			angular.forEach($scope.currentRoles, function (currentRole, key) {
				if (role.name === currentRole.name) {
					currentRole.usedBy++;
				}
			});
		};

		$scope.test = function () {
			console.log("clicked indeterminate");
		}

		$scope.checkIfRoleExistInCurrentRoles = function (role) {
			var returnValue = false;
			angular.forEach($scope.currentRoles, function (currentRole, key) {
				if (role.name === currentRole.name) {
					returnValue = true;
				}
			});

			return returnValue;
		};

		$scope.modalShownGrant = false;
		$scope.modalShownRevoke = false;

		$scope.close = function () {
			$scope.modalShownGrant = false;
			$scope.modalShownRevoke = false;
		}

		$scope.save = function() {
			$scope.modalShownGrant = false;
			$scope.modalShownRevoke = false;
			$scope.resetMulti();


		}

		$scope.resetMulti = function () {
			$scope.itemArr = [];
			$scope.multi = false;
			for (var i = 0; i < $scope.people.length; i++) {
				$scope.people[i].marked = false;
			}
		}
	}
})();