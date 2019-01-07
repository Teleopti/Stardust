'use strict';
describe('ResourceplannerManageScheduleCtrl', function () {
	var $q,
		$rootScope,
		$httpBackend,
		$scope,
		controller,
		$controller,
		manageScheduleSrvc,
		$translate;

	beforeEach(function () {
		module('wfm.resourceplanner');
	});

	beforeEach(inject(function (_$q_, _$rootScope_, _$controller_, _$httpBackend_, _$translate_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$controller = _$controller_;
		$httpBackend = _$httpBackend_;
		$translate = _$translate_;
		manageScheduleSrvc = setUpManageScheduleServ();
		$httpBackend.expectGET('../ToggleHandler/AllToggles').respond(200, 'mock');
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
	
	function setupHappyPath(){
		var fromScenario = {
			DefaultScenario: false,
			Id: "1",
			Name: "Default"
		};
		var toScenario = {
			DefaultScenario: true,
			Id: "2",
			Name: "Low"
		};

		var period = {
			startDate: moment().add(1,'days'),
			endDate: moment().add(8,'days')
		};
		var teamSelection = ["e5f968d7-6f6d-407c-81d5-9b5e015ab495"];

		return {fromScenario: fromScenario, toScenario: toScenario, period: period, teamSelection:teamSelection };
	}

	it('should show confirm', function () {
		controller = setUpController($controller);

		var input = setupHappyPath();

		controller.isImportSchedule = true;
		controller.validateAndShowModal(input.fromScenario, input.toScenario, input.period, input.teamSelection);
		$scope.$digest();
		
		expect(controller.showConfirmModal).toEqual(true);

	});

	it('should not show confirm when date is today', function () {
		controller = setUpController($controller);

		var input = setupHappyPath();
		input.period.startDate = moment();

		controller.isImportSchedule = true;
		controller.validateAndShowModal(input.fromScenario, input.toScenario, input.period, input.teamSelection);
		$scope.$digest();

		expect(controller.showConfirmModal).toEqual(false);
	});

	it('should validate start date', function () {
		controller = setUpController($controller);

		var input = setupHappyPath();
		input.period.startDate = null;

		controller.isImportSchedule = true;
		let result = controller.validateManagingParameters(input.fromScenario, input.toScenario, input.period, input.teamSelection);
		
		expect(result.messages[0]).toEqual($translate.instant('SelectStartDate'));

	});

	it('should validate end date', function () {
		controller = setUpController($controller);

		var input = setupHappyPath();
		input.period.endDate = null;

		controller.isImportSchedule = true;
		let result = controller.validateManagingParameters(input.fromScenario, input.toScenario, input.period, input.teamSelection);
		
		expect(result.messages[0]).toEqual($translate.instant('SelectEndDate'));

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
