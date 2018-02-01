(function () {
	'use strict';

angular.module('wfm.people')
	.controller('PeopleStart', [ '$scope', PeopleStartController ]);

	function PeopleStartController($scope) {

		$scope.people = [
			{
				firstName: 'John',
				id: '1',
				lastName: 'Doe',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Jennie',
				id: '2',
				lastName: 'Flowers',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Bert',
				id: '3',
				lastName: 'Erickson',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Carolyn',
				id: '4',
				lastName: 'Garrett',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Christian',
				id: '5',
				lastName: 'Reeves',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Sergio',
				id: '6',
				lastName: 'Evans',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Edwin',
				id: '7',
				lastName: 'Morrison',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Tom',
				id: '8',
				lastName: 'Vaughn',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Ollie',
				id: '9',
				lastName: 'Norris',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Greg',
				id: '10',
				lastName: 'Harrington',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Clyde',
				id: '11',
				lastName: 'Wilkins',
				role: 'Agent',
				phone: '123456123'
			},
		];

		$scope.disabledOptions = true;

		$scope.existingRoles = [
			{name: 'Agent', checked: true},
			{name: 'Team leader', checked: true}
		];

		$scope.roles = [
			{name: 'Agent', checked: true},
			{name: 'Team leader', checked: true},
			{name: 'London, site admin', checked: false},
			{name: 'Partner Web', checked: false},
			{name: 'Store staff', checked: false},
			{name: 'Super Administrator Role', checked: false}
		];

		$scope.checkExistingRoles = function(role){
			var exists = false;
			angular.forEach($scope.existingRoles, function(value, key){
				if(value.name === role.name){
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
			var indexOfItem = $scope.itemArr.indexOf(item);

			if(indexOfItem !== -1) {
				$scope.itemArr.splice(indexOfItem, 1); 
			}else{
				$scope.itemArr.push(item);
			}
		}

		$scope.modalShownGrant = false;
		$scope.modalShownRevoke = false;

		$scope.close = function(){
			$scope.modalShownGrant = false;
			$scope.modalShownRevoke = false;
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