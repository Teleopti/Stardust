'use strict';
describe('FunctionController', function() {

    var $q,
        $rootScope,
		$filter;

    var mockRoleFunctionService = {
        selectFunction:function(selectedRole, Type, Id) {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        },
        unselectFunction: function (selectedRole, Type, Id) {
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        },
        refreshFunctions: function(id) {
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
    };

    var mockGrowl = {};

    beforeEach(function () {
        module('wfm.permissions');
    });

    beforeEach(inject(function (_$q_, _$rootScope_, _$filter_) {
        $q = _$q_;
        $rootScope = _$rootScope_;
        $filter = _$filter_;
    }));

    it('should toggle the selected property of node from false to true', inject(function ($controller) {
        var scope = $rootScope.$new();
        $controller('RoleFunctionsController', { $scope: scope, $filter: $filter, RolesFunctionsService: mockRoleFunctionService, Roles: mockRoles, growl: mockGrowl });
        var node = {
        	$modelValue: { selected: false, Type: 'Team', ChildNodes: [] }, $nodeScope: { $parentNodeScope: null }
        };

        scope.toggleFunctionForRole(node);
        scope.$digest();

        expect(node.$modelValue.selected).toBe(true);
    }));
});  




});