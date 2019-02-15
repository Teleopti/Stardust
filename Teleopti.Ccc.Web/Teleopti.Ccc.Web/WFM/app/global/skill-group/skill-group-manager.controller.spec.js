describe('SkillGroupManager', function() {
	var $controller, $rootScope, scope, $httpBackend;

	var vm;
	var skills = [
		{
			Id: '5f15b334-22d1-4bc1-8e41-72359805d30f',
			Name: 'skill x',
			DoDisplayData: true,
			ShowAbandonRate: true,
			ShowReforecastedAgents: true
		},
		{
			Id: '502632DC-7A0C-434D-8A75-3153D5160787',
			Name: 'skill y',
			DoDisplayData: true,
			ShowAbandonRate: false,
			ShowReforecastedAgents: false
		}
	];

	var skillGroups = [
		{ Id: 'fa9b5393-ef48-40d1-b7cc-09e797589f81', Name: 'my skill area 1', Skills: skills },
		{ Id: '836cebb6-cee8-41a1-bb62-729f4b3a63f4', Name: 'my skill area 2', Skills: skills }
	];

	beforeEach(function() {
		module('wfm.skillGroup', 'wfm.notice', 'wfm.utilities');

		module(function($provide) {
			$provide.service('SkillGroupSvc', function() {
				return new FakeSkillGroupService();
			});
		});

		module(function($provide) {
			$provide.service('$state', function() {
				return {
					params: {
						selectedGroup: skillGroups[0]
					}
				};
			});
		});
	});

	beforeEach(inject(function(_$controller_, _$rootScope_, _$httpBackend_) {
		$controller = _$controller_;
		$rootScope = _$rootScope_;
		$rootScope._ = _;
		$httpBackend = _$httpBackend_;

		$httpBackend.whenGET('../ToggleHandler/AllToggles').respond(function() {
			return [200];
		});
	}));

	beforeEach(function() {
		createController();
		vm.skillGroups = skillGroups;
		vm.skills = skills;
	});

	it('should be able to create a skill-group', function() {
		vm.createSkillGroup(null);
		expect(vm.newGroup.Name).toEqual('');
		vm.newGroupName = 'newGroup';
		vm.saveNameEdit();
		expect(vm.skillGroups.length).toEqual(3);
		expect(vm.skillGroups[2].Name).toEqual('newGroup');
	});

	it('should be able to rename an existing skill-group', function() {
		vm.newGroupName = 'newName';
		vm.isNew = false;
		vm.selectedGroupIndex = 0;
		vm.saveNameEdit();
		expect(vm.skillGroups[0].Name).toEqual('newName');
	});

	it('should be able to remove a skill-group', function() {
		vm.selectedGroupIndex = 0;
		expect(vm.skillGroups[0]).not.toBe(null);
		vm.deleteSkillGroup(function() {
			expect(vm.skillGroups[0]).toBe(null);
		});
	});

	function FakeCurrentUserInfo() {
		this.CurrentUserInfo = function() {
			return { DefaultTimeZone: 'Europe/Stockholm' };
		};
	}

	function FakeSkillGroupService() {
		this.getSkills = function() {
			return {
				then: function() {
					return skills;
				}
			};
		};

		this.getSkillGroups = function() {
			return {
				then: function() {
					return skillGroups;
				}
			};
		};

		this.deleteSkillGroup = () => {
			return new Promise((resolve, reject) => {
				resolve('Fake delete succeeded!');
			});
		};
	}

	function createController() {
		scope = $rootScope.$new();

		vm = $controller('SkillGroupManagerController', {
			$scope: scope,
			$translate: null
		});
		scope.$digest();
		$httpBackend.flush();
	}
});
