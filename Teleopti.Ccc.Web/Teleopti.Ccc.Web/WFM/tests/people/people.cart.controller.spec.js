'use strict';

describe("PeopleCartCtrl", function () {
	var $q,
		$rootScope,
		$httpBackend,
		controller;

	beforeEach(module('wfm'));

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_, _$controller_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
		controller = setUpController(_$controller_);
	}));

	var mockToggleService = {
		isFeatureEnabled: {
			query: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({
					IsEnabled: true
				});
				return { $promise: queryDeferred.promise };
			}
		},
	}

	var mockPeopleService = {
		loadAllSkills: {
			get: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve(
					[
						{ SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753", SkillName: "Channel Sales" },
						{ SkillId: "c5fffc8f-bcd6-47f7-9352-9f0800e39578", SkillName: "Direct Sales" },
						{ SkillId: "bc50fc19-c211-4e7a-8a1a-9f0801134e37", SkillName: "Email" },
						{ SkillId: "bc50fc19-c211-4e7a-8a1a-9f0801134e89", SkillName: "TestSkill" }
					]
				);
				return { $promise: queryDeferred.promise };
			}
		},
		loadAllShiftBags: {
			get: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve(
					[
						
					]
				);
				return { $promise: queryDeferred.promise };
			}
		},
		fetchPeople: {
			post: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve(
					[
						{
							"PersonId": "3833e4a7-dbf4-4130-9027-9b5e015b2580",
							"FirstName": "Tim",
							"LastName": "McMahon",
							"Team": "Paris/Team 1",
							"SkillIdList": ["f08d75b3-fdb4-484a-ae4c-9f0800e2f753", "c5fffc8f-bcd6-47f7-9352-9f0800e39578"],
							"ShiftBag": "Paris Full Time"
						},
						{
							"PersonId": "71d27b06-30c0-49fd-ae16-9b5e015b2580",
							"FirstName": "George",
							"LastName": "Lueker",
							"Team": "Paris/Team 1",
							"SkillIdList": ["c5fffc8f-bcd6-47f7-9352-9f0800e39578", "bc50fc19-c211-4e7a-8a1a-9f0801134e37"],
							"ShiftBag": "Paris Full Time"
						},
						{
							"PersonId": "1a714f36-ee87-4a06-88d6-9b5e015b2585",
							"FirstName": "Steve",
							"LastName": "Novack",
							"Team": "Paris/Team 1",
							"SkillIdList": ["c5fffc8f-bcd6-47f7-9352-9f0800e39578"],
							"ShiftBag": "Paris Full Time"
						}
					]
				);
				return { $promise: queryDeferred.promise };
			}
		}
	};

	it("should show skill selected status according to people skill list", inject(function () {
		var scope = $rootScope.$new();
		scope.$digest(); // this is needed to resolve the promise
		var availableSkills = controller.availableSkills;

		expect(availableSkills.length).toEqual(4);
		expect(availableSkills[0].Selected).toEqual(false);
		expect(availableSkills[0].Status).toEqual('partial');
		expect(availableSkills[1].Selected).toEqual(true);
		expect(availableSkills[1].Status).toEqual('all');
		expect(availableSkills[2].Selected).toEqual(false);
		expect(availableSkills[2].Status).toEqual('partial');
		expect(availableSkills[3].Selected).toEqual(false);
		expect(availableSkills[3].Status).toEqual('none');
	}));

	it("should construct person skills according to available skills", inject(function() {
		var scope = $rootScope.$new();
		scope.$digest(); // this is needed to resolve the promise
		var availablePeople = controller.availablePeople;

		expect(availablePeople.length).toEqual(3);
		expect(availablePeople[0].Skills()).toEqual("Channel Sales, Direct Sales");
		expect(availablePeople[1].Skills()).toEqual("Direct Sales, Email");
		expect(availablePeople[2].Skills()).toEqual("Direct Sales");
	}));

	it("should remove skill from person skills when skill is deselected", inject(function() {
		var scope = $rootScope.$new();
		scope.$digest(); // this is needed to resolve the promise
		var skill = {
			SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			SkillName: "Channel Sales",
			Selected: false
		};
		controller.skillSelectedStatusChanged(skill);

		var availablePeople = controller.availablePeople;
		expect(availablePeople[0].Skills()).toEqual("Direct Sales");
		expect(availablePeople[1].Skills()).toEqual("Direct Sales, Email");
		expect(availablePeople[2].Skills()).toEqual("Direct Sales");
	}));

	it("should add skill to person skills for who does not have the skill when skill is selected", inject(function() {
		var scope = $rootScope.$new();
		scope.$digest(); // this is needed to resolve the promise
		var skill = {
			SkillId: "f08d75b3-fdb4-484a-ae4c-9f0800e2f753",
			SkillName: "Channel Sales",
			Selected: true
		};
		controller.skillSelectedStatusChanged(skill);

		var availablePeople = controller.availablePeople;
		expect(availablePeople[0].Skills()).toEqual("Channel Sales, Direct Sales");
		expect(availablePeople[1].Skills()).toEqual("Channel Sales, Direct Sales, Email");
		expect(availablePeople[2].Skills()).toEqual("Channel Sales, Direct Sales");
	}));

	it("should remove person correctly", inject(function () {
		var scope = $rootScope.$new();
		scope.$digest(); // this is needed to resolve the promise
		var person = {
			"PersonId": "3833e4a7-dbf4-4130-9027-9b5e015b2580",
		};
		controller.removePerson(person);

		var availablePeople = controller.availablePeople;
		expect(availablePeople.length).toEqual(2);
		expect(availablePeople[0].PersonId).toEqual("71d27b06-30c0-49fd-ae16-9b5e015b2580");
		expect(availablePeople[1].PersonId).toEqual("1a714f36-ee87-4a06-88d6-9b5e015b2585");
	}));

	function setUpController($controller) {
		var stateParams = { selectedPeopleIds: [], commandTag: "AdjustSkill" }
		return $controller("PeopleCartCtrl", { $stateParams: stateParams, Toggle: mockToggleService, People: mockPeopleService });
	};
});
