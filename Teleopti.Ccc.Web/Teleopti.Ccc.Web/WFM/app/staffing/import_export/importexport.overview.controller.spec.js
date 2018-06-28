'use strict';
describe('ImportDataController', function () {
	var $q, $httpBackend, $controller, scope, UtilService,  $window,  mockStaffingService;

	beforeEach(function () {
		module('wfm.staffing');
	});
	var deafultSkill = { DoDisplayData: true, IsMultisiteSkill: false, SkillType: 'SkillTypeChat', Id: '123' };
	var activeBpo1 = { Id: "123", Source: "bpo1" };
	var activeBpo2 = { Id: "123", Source: "bpo1" };
	var deafultSkillArea = [
		{ "Id": "123", "Name": "Skill1", "DoDisplayData": true, "SkillType": "SkillTypeInboundTelephony", "IsMultisiteSkill": false, "ShowAbandonRate": true, "ShowReforecastedAgents": true },
		{ "Id": "321", "Name": "Skill2", "DoDisplayData": true, "SkillType": "SkillTypeInboundTelephony", "IsMultisiteSkill": false, "ShowAbandonRate": true, "ShowReforecastedAgents": true }
	];

	
	beforeEach(
		inject(function (_$q_, _$rootScope_, _$httpBackend_, _$controller_, _UtilService_, _$window_) {
			$q = _$q_;
			$httpBackend = _$httpBackend_;
			$controller = _$controller_;
			UtilService = _UtilService_;
			$window = _$window_;
			scope = _$rootScope_.$new();


			mockStaffingService = {
				getActiveBpos: {
					query: function () {
						var queryDeferred = $q.defer();
						var result = [activeBpo1, activeBpo2];
						queryDeferred.resolve(result);
						return { $promise: queryDeferred.promise };
					}
				},
				getSkills: {
					query: function () {
						var queryDeferred = $q.defer();
						var result = [deafultSkill];
						queryDeferred.resolve(result);
						return { $promise: queryDeferred.promise };
					}
				},
				getSkillAreas: {
					get: function () {

						var queryDeferred = $q.defer();
						var result = { SkillAreas: deafultSkillArea };
						queryDeferred.resolve(result);
						return { $promise: queryDeferred.promise };
					}
				},
				getExportGapPeriodMessage: {
					get: function () {
						var queryDeferred = $q.defer();
						var result = { ExportBpoPeriodMessage: "" };
						queryDeferred.resolve(result);
						return { $promise: queryDeferred.promise };
					}
				},
				getExportStaffingPeriodMessage: {
					get: function () {
						var queryDeferred = $q.defer();
						var result = { ExportPeriodMessage: "" };
						queryDeferred.resolve(result);
						return { $promise: queryDeferred.promise };
					}
				}
				
			};

			

			$httpBackend.whenGET('../ToggleHandler/AllToggles').respond(function () {
				return [200];
			});

		})
	);

	it('should set the skill area', function () {
		var vm = $controller('ImportDataController', {
			$scope: scope
		});
		vm.selectedSkillArea = null;
		vm.selectedSkill = null;
		vm.selectedAreaChange("a");
		expect(vm.selectedSkillArea).toBe("a");
		expect(vm.selectedSkill).toBe(null);
	});

	it('should set the skill', function () {
		var vm = $controller('ImportDataController', {
			$scope: scope
		});
		vm.selectedSkillArea = null;
		vm.selectedSkill = null;
		vm.selectedSkillChange("a");
		expect(vm.selectedSkill).toBe("a");
		expect(vm.selectedSkillArea).toBe(null);
	});

	it('should get the list of bpos', function () {
		var vm = $controller('ImportDataController', {
			$scope: scope,
			staffingService: mockStaffingService
		});
		expect(vm.activeBpos.length).toBe(0);
		scope.$digest();
		expect(vm.activeBpos.length).toBe(2);
	});

	it('should get the list os skills', function () {
		var vm = $controller('ImportDataController', {
			$scope: scope,
			staffingService: mockStaffingService
		});
		expect(vm.activeBpos.length).toBe(0);
		scope.$digest();
		expect(vm.activeBpos.length).toBe(2);
	});

});
