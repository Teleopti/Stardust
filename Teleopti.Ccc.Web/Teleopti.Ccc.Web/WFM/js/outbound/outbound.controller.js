var outbound = angular.module('wfm.outbound', []);

outbound.controller('OutboundListCtrl', [
	'$scope', '$state',
	function($scope, $state) {
		$scope.campaigns = [
			{ id: 1, name: "March Sales" },
			{ id: 2, name: "Apirl Sales" },
			{ id: 3, name: "Chocalate Sales"},
		];

		$scope.newName = "";
		$scope.selectedTarget = null;

		$scope.reset = function () {
			$scope.selectedTarget = null;
			console.log($scope.form);
			$scope.form.$setPristine();
		};

		var getNextId = function() {
			var curMaxId = -1;
			angular.forEach($scope.campaigns, function (v, k) { curMaxId = curMaxId > v.id ? curMaxId : v.id; });
			return curMaxId < 0 ? 1 : curMaxId + 1;
		};

		$scope.create = function() {			
			$scope.campaigns.unshift({
				id: getNextId(),
				name: $scope.newName
			});
			$scope.newName = "";
		};

		$scope.copy = function(obj) {
			var copiedObj = obj.clone();
			copiedObj.id = getNextId();			
			$scope.campaigns.push(copiedObj);
		};

		$scope.update = function(obj) {

		};

		$scope.show = function(obj) {
			$scope.selectedTarget = obj;
			console.log("Selected", obj);
			$state.go('outbound.edit', { id: $scope.selectedTarget.id });
			console.log("Going to id", $scope.selectedTarget.id);
		};

		$scope.delete = function(obj, idx) {
			if (confirm('Are you sure you want to delete this record?')) {
				if ($scope.campaigns.idx == obj) {
					$scope.campaigns.splice(idx, 1);
				} else {
					console.log("Unmatched element.");
				}
			}
		};

	}
]);

outbound.controller('OutboundEditCtrl', [
	'$scope', '$stateParams',
	function ($scope, $stateParams) {
		$scope.campaigns = [
			{ id: 1, name: "March Sales" },
			{ id: 2, name: "Apirl Sales" },
			{ id: 3, name: "Chocalate Sales" },
		];

		$scope.acToggle1 = true;

		$scope.skills = [
			{ label: "Phone", value: "Phone" },
			{ label: "Consultancy", value: "Consultancy" },
			{ label: "Writing", value: "Writing" }
		];

		$scope.availableWorkingHours = [
			{ label: "Closed", value: "Closed" }
		];

		angular.forEach($scope.campaigns, function(v, k) {
			if (v.id == $stateParams.id) {
				$scope.campaign = v;
			}
		});

		$scope.period = {
			startDate: moment().add(1, 'months').startOf('month').toDate(),
			endDate: moment().add(2, 'months').startOf('month').toDate()
		};

		$scope.params = {
			skill: $scope.skills[0]
		};

	}
]);