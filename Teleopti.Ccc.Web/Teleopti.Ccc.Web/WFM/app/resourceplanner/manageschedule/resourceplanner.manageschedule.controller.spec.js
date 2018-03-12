'use strict';
describe('ResourceplannerManageScheduleCtrl', function () {
	var $q,
		$rootScope,
		$httpBackend,
		$scope,
		controller,
		$controller,
		manageScheduleSrvc;

	beforeEach(function () {
		module('wfm.resourceplanner');
	});

	beforeEach(inject(function (_$q_, _$rootScope_, _$controller_, _$httpBackend_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$controller = _$controller_;
		$httpBackend = _$httpBackend_;
		manageScheduleSrvc = setUpManageScheduleServ();
	}));


	it('should the controller not be null', function () {
		controller = setUpController($controller);
		expect(controller).not.toBe(null);
	});

	it('should disappear the progress bar when error happens', function () {
		controller = setUpController($controller);

		var fromScenario = {
			DefaultScenario: true,
			Id: "1",
			Name: "Default"
		};
		var toScenario = {
			DefaultScenario: false,
			Id: "2",
			Name: "Low"
		};
		
		var period = {
				startDate: new Date(2018, 3, 12),
				endDate: new Date(2018, 4, 12)
			};
		var teamSelection = ["e5f968d7-6f6d-407c-81d5-9b5e015ab495"];

		controller.isImportSchedule = true;
		controller.runManaging(fromScenario, toScenario, period, teamSelection);
		$scope.$digest();
		expect(controller.showProgress).toEqual(false);

	});


	function setUpController($controller) {
		$scope = $rootScope.$new();
		return $controller('ResourceplannerManageScheduleCtrl', { ManageScheduleSrvc: manageScheduleSrvc, $scope: $scope });
	}

	function setUpManageScheduleServ() {
		return {
			scenarios: {
				query: function () {
					var queryDeferred = $q.defer();
					queryDeferred.resolve([
						{
							Id: "1",
							Name: "Default",
							DefaultScenario: true
						}]);
					return { $promise: queryDeferred.promise };
				}
			},
			organization: {
				query: function () {
					var queryDeferred = $q.defer();
					queryDeferred.resolve([
						{
							ChildNodes: [{
								ChildNodes: [
									{
										Choosable: false,
										Id: "e5f968d7-6f6d-407c-81d5-9b5e015ab495",
										Name: "Students",
										Type: "Team"

									}],
								Choosable: false,
								Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
								Name: "London",
								Type: "Site"
							}],
							Choosable: true,
							Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
							Name: "TeleoptiCCCDemo",
							Type: "BusinessUnit"
						}]);
					return { $promise: queryDeferred.promise };
				}
			},
			runImporting: {
				post: function () {
					var deferred = $q.defer();
					deferred.reject();
					return { $promise: deferred.promise };
				}
			}
		}
	}

});
