describe('PeopleStartCtrl', function() {
	'use strict';
	var $q,
		$rootScope,
		$httpBackend,
		$translate;
	var stateParams = { selectedPeopleIds: [], commandTag: 'AdjustSkill', currentKeyword: '', paginationOptions: {} };
	var sentMessage;
	var mockNoticeService = {
		info: function (message, timeout, flag) {
			sentMessage = message;
		}
	};

	beforeEach(function(){
		module('wfm.peopleold');
		module('externalModules');
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_, _$controller_, _$translate_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$translate = _$translate_;
	}));

	var mockToggleService = {
		Wfm_PeopleWeb_PrepareForRelease_47766: true
	}

	var mockPeopleService = {
		search: {
			query: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({
					People: [
						{
							FirstName: "Ashley",
							LastName: "Andeen",
							EmploymentNumber: "12345",
							LeavingDate: "2015-04-09",
							OptionalColumnValues: [
								{
									"Key": "CellPhone",
									"Value": "123456"
								}
							],
							Team: "Paris/Team 1"
						}
					],
					OptionalColumns: ["CellPhone"]
				});
				return { $promise: queryDeferred.promise };
			}
		}
	};

	it("should show agent by search function", inject(function($controller) {
		var scope = $rootScope.$new();

		$controller("PeopleStartCtrl", { $scope: scope, $stateParams: stateParams, Toggle: mockToggleService, PeopleService: mockPeopleService, NoticeService: mockNoticeService });

		scope.searchOptions.keyword = "ashley";
		scope.searchKeyword();
		scope.$digest(); // this is needed to resolve the promise

		expect(scope.searchResult.length).toEqual(1);
		expect(scope.searchResult[0].FirstName).toEqual("Ashley");
		expect(scope.optionalColumns.length).toEqual(1);
		expect(scope.optionalColumns[0]).toEqual("CellPhone");
		expect(scope.searchResult[0].OptionalColumnValues[0].Key).toEqual("CellPhone");
		expect(scope.searchResult[0].OptionalColumnValues[0].Value).toEqual("123456");
	}));

	it("should show my team as default keyword", inject(function($controller) {
		var scope = $rootScope.$new();
		$controller("PeopleStartCtrl", { $scope: scope, $stateParams: stateParams, Toggle: mockToggleService, PeopleService: mockPeopleService, NoticeService: mockNoticeService });

		scope.searchKeyword();
		scope.$digest(); // this is needed to resolve the promise

		expect(scope.searchOptions.keyword).toEqual("\"Paris\" \"Team 1\"");
	}));

	it("should show release notification only in people.start state", inject(function($controller) {
		var scope = $rootScope.$new();
		var state = {
			current: {}
		};
		state.current.name = "peopleold.start";
		$controller("PeopleStartCtrl", { $state: state, $scope: scope, $stateParams: stateParams, Toggle: mockToggleService, People: mockPeopleService, NoticeService: mockNoticeService });

		var message = $translate.instant('WFMReleaseNotificationWithoutOldModuleLink')
		.replace('{0}', $translate.instant('People'))
		.replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx' target='_blank' target='_blank'>")
		.replace('{2}', '</a>');
		expect(sentMessage).toEqual(message);

		sentMessage = "";
		state.current.name = "seatPlan";
		$controller("PeopleStartCtrl", { $state: state, $scope: scope, $stateParams: stateParams, Toggle: mockToggleService, People: mockPeopleService, NoticeService: mockNoticeService });
		expect(sentMessage).toEqual("");
	}));
});
