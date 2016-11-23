'use strict';
describe('DataController', function () {

	var $q,
		$rootScope;

	var mockRoleDataService = {
		assignOrganizationSelection: function (selectedRole, Type, Id) {
			var deferred = $q.defer();
			deferred.resolve();
			return deferred.promise;
		},
		deleteAvailableData: function (selectedRole, Type, Id) {
			var deferred = $q.defer();
			deferred.resolve();
			return deferred.promise;
		},
		refreshpermissions: function (id) {
			var deferred = $q.defer();
			deferred.resolve();
			return deferred.promise;
		},
		refreshOrganizationSelection: function () {
			var deferred = $q.defer();
			deferred.resolve();
			return deferred.promise;
		},
		deleteAllNodes: function (selectedRole, dataNodes) {
		}
	};
	var mockRoles = {
		selectedRole: {
			BuiltIn: false,
			Id: 1
		}
	}

	beforeEach(function () {
		module('wfm.permissions');
	});

	beforeEach(inject(function (_$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
	}));

	it('should not toggle node if role is BuiltIn', inject(function ($controller) {
		var scope = $rootScope.$new();
		var mockRoles = {
			selectedRole: {
				BuiltIn: true
			}
		}
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		var node = { $modelValue: { selected: false, Type: 'Team', ChildNodes: [] } };

		scope.toggleOrganizationSelection(node);

		scope.$digest();

		expect(node.$modelValue.selected).toBe(false);
	}));


	it('should simeselect all parents when it has both selected children and unselected children', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		scope.selectedRole = "1";
		var targetNode = prepareNodes(false, false, false);

		scope.toggleOrganizationSelection(targetNode);
		scope.$digest();

		expect(targetNode.$modelValue.selected).toBe(true);
		expect(targetNode.$parentNodeScope.$modelValue.selected).toBe(false);
		expect(targetNode.$parentNodeScope.$modelValue.semiSelected).toBe(true);
		expect(targetNode.$parentNodeScope.$parentNodeScope.$modelValue.selected).toBe(false);
		expect(targetNode.$parentNodeScope.$parentNodeScope.$modelValue.semiSelected).toBe(true);
	}));

	it('should select all parent when its children are all selected', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		scope.selectedRole = "1";
		var targetNode = prepareNodes(false, true, true);

		scope.toggleOrganizationSelection(targetNode);
		scope.$digest();

		expect(targetNode.$modelValue.selected).toBe(true);
		expect(targetNode.$parentNodeScope.$modelValue.selected).toBe(true);
		expect(targetNode.$parentNodeScope.$modelValue.semiSelected).toBe(false);
		expect(targetNode.$parentNodeScope.$parentNodeScope.$modelValue.selected).toBe(true);
		expect(targetNode.$parentNodeScope.$parentNodeScope.$modelValue.semiSelected).toBe(false);
	}));

	it('should unselect parent when its children are all unselected', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		scope.selectedRole = "1";
		var targetNode = prepareNodes(true, false, false);

		scope.toggleOrganizationSelection(targetNode);
		scope.$digest();

		expect(targetNode.$modelValue.selected).toBe(false);
		expect(targetNode.$parentNodeScope.$modelValue.selected).toBe(false);
		expect(targetNode.$parentNodeScope.$modelValue.semiSelected).toBe(false);
		expect(targetNode.$parentNodeScope.$parentNodeScope.$modelValue.selected).toBe(false);
		expect(targetNode.$parentNodeScope.$parentNodeScope.$modelValue.semiSelected).toBe(false);
	}));

	it('should same select all children', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		scope.selectedRole = "1";
		var targetNode = prepareNodes(false, false, false, true);

		scope.toggleOrganizationSelection(targetNode);
		scope.$digest();

		expect(targetNode.$modelValue.selected).toBe(true);
		checkChildren(targetNode);
		function checkChildren(node) {
			if (node.childNodes().length > 0) {
				node.childNodes().forEach(function (child) {
					expect(child.$modelValue.selected).toBe(true);
					checkChildren(child);
				});
			}
		}
	}));
	it('should same unselect all children', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		scope.selectedRole = "1";
		var targetNode = prepareNodes(true, true, true, true);
		targetNode.$modelValue.selected = true;
		scope.toggleOrganizationSelection(targetNode);
		scope.$digest();

		expect(targetNode.$modelValue.selected).toBe(false);
		checkChildren(targetNode);
		function checkChildren(node) {
			if (node.childNodes().length > 0) {
				node.childNodes().forEach(function (child) {
					expect(child.$modelValue.selected).toBe(false);
					checkChildren(child);
				});
			}
		}
	}));

	it('should save its parents when all is selected', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		scope.selectedRole = "1";
		var deferred = $q.defer();
		var expectedObject = [{ type: 'Team', id: '11' }, { type: 'Site', id: '1' }, { type: 'BU', id: '0' }];
		deferred.resolve();
		spyOn(mockRoleDataService, 'assignOrganizationSelection').and.returnValue(deferred.promise);
		var targetNode = prepareNodes(false, true, true);

		scope.toggleOrganizationSelection(targetNode);
		scope.$digest();

		expect(mockRoleDataService.assignOrganizationSelection).toHaveBeenCalledWith('1', expectedObject);
	}));

	it('should only save itself when its parent has unselected children', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		scope.selectedRole = "1";
		var deferred = $q.defer();
		var expectedObject = [{ type: 'Team', id: '11' }];
		deferred.resolve();
		spyOn(mockRoleDataService, 'assignOrganizationSelection').and.returnValue(deferred.promise);
		var targetNode = prepareNodes(false, true, false);

		scope.toggleOrganizationSelection(targetNode);
		scope.$digest();

		expect(mockRoleDataService.assignOrganizationSelection).toHaveBeenCalledWith('1', expectedObject);
	}));

	it('should save all children when it is selected', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		scope.selectedRole = "1";
		var deferred = $q.defer();
		var expectedObject = [{ type: 'BU', id: '0' }, { type: 'Site', id: '1' }, { type: 'Team', id: '11' }, { type: 'Team', id: '12' }, { type: 'Team', id: '13' }];
		deferred.resolve();
		spyOn(mockRoleDataService, 'assignOrganizationSelection').and.returnValue(deferred.promise);
		var targetNode = prepareNodes(false, false, true, true);

		scope.toggleOrganizationSelection(targetNode);
		scope.$digest();

		expect(mockRoleDataService.assignOrganizationSelection).toHaveBeenCalledWith('1', expectedObject);
	}));

	it('should delete all parents when unselect one out of all selected children', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		scope.selectedRole = "1";
		var deferred = $q.defer();
		var expectedObject = [{ type: 'Team', id: '11' }, { type: 'Site', id: '1' }, { type: 'BU', id: '0' }];
		deferred.resolve();
		spyOn(mockRoleDataService, 'deleteAllNodes').and.returnValue(deferred.promise);
		var targetNode = prepareNodes(true, true, true);

		scope.toggleOrganizationSelection(targetNode);
		scope.$digest();

		expect(mockRoleDataService.deleteAllNodes).toHaveBeenCalledWith('1', expectedObject);
	}));

	it('should only delete itself when its parent has selected children', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		scope.selectedRole = "1";
		var deferred = $q.defer();
		var expectedObject = [{ type: 'Team', id: '11' }];
		deferred.resolve();
		spyOn(mockRoleDataService, 'deleteAllNodes').and.returnValue(deferred.promise);
		var targetNode = prepareNodes(true, true, false);

		scope.toggleOrganizationSelection(targetNode);
		scope.$digest();

		expect(mockRoleDataService.deleteAllNodes).toHaveBeenCalledWith('1', expectedObject);
	}));

	

	it('should trigger the watch', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		var data = { BusinessUnit: [{ Sites: [{ Teams: [] }] }] };

		mockRoleDataService.organization = data;
		scope.$digest();

		expect(mockRoleDataService.organization).toBe(data);
	}));

	function prepareNodes(isTeam1Selected, isTeam2Selected, isTeam3Selected, shouldPrepareChildren) {
		function childNodes(nodes) {
			return function () {
				return nodes;
			};
		}
		var team1 = {
			$modelValue: { selected: isTeam1Selected, Id: '11', Type: 'Team', ChildNodes: [] },
			childNodes: childNodes([])
		};
		var team2 = {
			$modelValue: { selected: isTeam2Selected, Id: '12', Type: 'Team', ChildNodes: [] },
			childNodes: childNodes([])
		};
		var team3 = {
			$modelValue: { selected: isTeam3Selected, Id: '13', Type: 'Team', ChildNodes: [] },
			childNodes: childNodes([])
		};
		var site = {
			$modelValue: {
				selected: isTeam1Selected && isTeam2Selected && isTeam3Selected,
				semiSelected: !(isTeam1Selected && isTeam2Selected && isTeam3Selected),
				Id: '1',
				Type: 'Site',
				ChildNodes: [team1, team2, team3]
			},
			childNodes: childNodes([team1, team2, team3])
		}
		var bu = {
			$modelValue: {
				selected: site.$modelValue.selected,
				semiSelected: !site.$modelValue.selected,
				Id: '0',
				Type: 'BU',
				ChildNodes: [site]
			},
			childNodes: childNodes([site])
		}
		site.$parentNodeScope = bu;
		var targetNode;
		if (!shouldPrepareChildren) shouldPrepareChildren = false;
		if (shouldPrepareChildren) {
			targetNode = {
				$modelValue: bu.$modelValue,
				childNodes: childNodes([site])
			}
		} else {
			targetNode = {
				$modelValue: team1.$modelValue,
				$parentNodeScope: site,
				childNodes: childNodes([])
			}
		}
		return targetNode;
	}
});