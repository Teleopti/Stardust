'use strict';
describe('DataController', function() {

	var $q,
		$rootScope;

	var mockRoleDataService = {
		assignOrganizationSelection:function(selectedRole, Type, Id) {
			var deferred = $q.defer();
			deferred.resolve();
			return deferred.promise;
		},
		deleteAvailableData: function (selectedRole, Type, Id) {
			var deferred = $q.defer();
			deferred.resolve();
			return deferred.promise;
		},
		refreshpermissions: function(id) {
			var deferred = $q.defer();
			deferred.resolve();
			return deferred.promise;
		},
		refreshOrganizationSelection: function () {
			var deferred = $q.defer();
			deferred.resolve();
			return deferred.promise;
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
	

	it('should toggle the selected property of node from false to true', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		var node = {
		    $modelValue: { selected: false, Type: 'Team' ,ChildNodes:[]}, $parentNodeScope: null
		};
		
		scope.toggleOrganizationSelection(node);
		scope.$digest();

		expect(node.$modelValue.selected).toBe(true);
	}));

	it('should toggle the selected property of node from true to false', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		var node = {$modelValue:{ selected: true,Type: 'Team', ChildNodes: [] }};

		scope.toggleOrganizationSelection(node);
		scope.$digest();

		expect(node.$modelValue.selected).toBe(false);
	}));
	it('should not toggle node if role is BuiltIn', inject(function ($controller) {
		var scope = $rootScope.$new();
		var mockRoles = {
			selectedRole: {
				BuiltIn: true
			}
		}
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		var node = { $modelValue: {selected: false, Type: 'Team',ChildNodes: [] } };

		scope.toggleOrganizationSelection(node);
		
		scope.$digest();

		expect(node.$modelValue.selected).toBe(false);
	}));

	it('should toggle parent to selected if child is selected', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		var node = { $modelValue: {selected: false,Type: 'Team', ChildNodes: [] }, $parentNodeScope: { $modelValue: { selected: false } } };

		scope.toggleOrganizationSelection(node);
		scope.$digest();

		expect(node.$modelValue.selected).toBe(true);
		expect(node.$parentNodeScope.$modelValue.selected).toBe(true);

	}));
	it('should not send the parent to the service when a child is selected', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		var node = {
			$modelValue: { selected: false, Type: 'Team', Id: '22', ChildNodes: [] },
			$parentNodeScope: { $modelValue: { selected: false, Id: '11' } }
		};
		scope.selectedRole = "1";
		var deferred = $q.defer();
		var expectedObject = [{ type: 'Team', id: '22' }] ; 
		deferred.resolve();
		spyOn(mockRoleDataService, 'assignOrganizationSelection').and.returnValue(deferred.promise);

		scope.toggleOrganizationSelection(node);
		scope.$digest();

		expect(mockRoleDataService.assignOrganizationSelection).toHaveBeenCalledWith('1', expectedObject);

		expect(node.$modelValue.selected).toBe(true);
		expect(node.$parentNodeScope.$modelValue.selected).toBe(true);

	}));
	it('should toggle child and child-of-child to not selected if parent is deselected', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		var node = { $modelValue: { selected: true,Type: 'Team', ChildNodes: [{ selected: true, ChildNodes: [{ selected: true, ChildNodes: [] }] }] } };
		scope.toggleOrganizationSelection(node);
		scope.$digest();

		expect(node.$modelValue.ChildNodes[0].selected).toBe(false);
		expect(node.$modelValue.ChildNodes[0].ChildNodes[0].selected).toBe(false);

	}));
	it('should toggle child and child-of-child to selected if parent is selected', inject(function ($controller) {
	    var scope = $rootScope.$new();
	    $controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
	    var node = { $modelValue: { selected: false, Type: 'BusinessUnit', ChildNodes: [{ selected: false, Type: 'Site', ChildNodes: [{ selected: false, Type: 'Team', ChildNodes: [] }] }] } };
	    scope.toggleOrganizationSelection(node);
	    scope.$digest();

	    expect(node.$modelValue.ChildNodes[0].selected).toBe(true);
	    expect(node.$modelValue.ChildNodes[0].ChildNodes[0].selected).toBe(true);

	}));
	it('should deselect the parent if no childs are selected', inject(function ($controller) {
	    var scope = $rootScope.$new();
	    $controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
	    var node = { $modelValue: { selected: true,Type: 'Team', ChildNodes: [] }, $parentNodeScope: { $modelValue: { selected: true, Type:'Site', ChildNodes: [] } } };
	    scope.toggleOrganizationSelection(node);
	    scope.$digest();

	    expect(node.$parentNodeScope.$modelValue.selected).toBe(false);
	    

	}));
	it('should not deselect the parent if more than one child is selected', inject(function ($controller) {
	    var scope = $rootScope.$new();
	    $controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
	    var node = {
	        $modelValue: {
	             selected: true, ChildNodes: []
	        },
	        $parentNodeScope: {
	            $modelValue: {
	                selected: true, Type: 'BusinessUnit',
                    Id:2,
	                ChildNodes: [
                         { selected: true, Type: 'Site'} ,
                         { selected: false, Type: 'Site' } 
	                ]
	            }
	        }
	    };

	    scope.toggleOrganizationSelection(node);
	    scope.$digest();

	    expect(node.$parentNodeScope.$modelValue.selected).toBe(true);
	}));


	it('should trigger the watch', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		var data = { BusinessUnit: [{ Sites: [{ Teams: [] }] }] };

		mockRoleDataService.organization = data;
		scope.$digest();

		expect(mockRoleDataService.organization).toBe(data);
	}));

	it('should send node and its children', inject(function ($controller) {
		var scope = $rootScope.$new();
		scope.selectedRole = '5';
		$controller('RoleDataController', {$scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		var node = {
			$modelValue: {
				Id: '3',
				Type: 'BusinessUnit',
				selected: false,
				ChildNodes: [{ selected: false, Type: 'Site', Id: '4', ChildNodes: [] },
					{ selected: false, Type: 'Site', Id: '5', ChildNodes: [] }]
			}
		};
		var expectedObj = [
			{ type: 'Site', id: '4' },
			{ type: 'Site', id: '5' },
			{ type: 'BusinessUnit', id: '3' }
		];

		spyOn(mockRoleDataService,"assignOrganizationSelection");

		scope.toggleOrganizationSelection(node);
		scope.$digest();

		expect(mockRoleDataService.assignOrganizationSelection).toHaveBeenCalledWith('5', expectedObj);
	}));
});