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
        },
        selectRole: function(role) {
            mockRoles.selectedRole =role;
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

    fit('should init controller with right data from service even if there is a network delay', inject(function ($controller) {
        var scope = $rootScope.$new();
        var allFunctions = [
           {
               FunctionId: "1",
               FunctionCode: "All",
               FunctionPath: "All",
               LocalizedFunctionDescription: "",
               ChildFunctions: [
               {
                   FunctionId: "3", ChildFunctions: []
               }]
           },
           {
               FunctionId: "4", FunctionCode: "GG", FunctionPath: "GG", LocalizedFunctionDescription: "",
               ChildFunctions: []
           }
        ];
        var allFunctionsAvailable = [
           {
               FunctionId: "1",
               FunctionCode: "All",
               FunctionPath: "All",
               LocalizedFunctionDescription: "",
               selected: true,
               ChildFunctions: [
               {
                   FunctionId: "3", ChildFunctions: [], selected: false
               }]
           },
           {
               FunctionId: "4", FunctionCode: "GG", FunctionPath: "GG", LocalizedFunctionDescription: "",
               selected: true,
               ChildFunctions: []
           }
        ];

        var getpromiss = function() {
            var deferred = $q.defer();
            setTimeout(function() {
                deferred.resolve(allFunctions);
            }, 1000);
            
            return deferred.promise;
        };
        var mockRoleFunctionServiceRefresh = {
            functionsDisplayed: [],
            refreshFunctions: function(id) {
                var deferred = $q.defer();
                mockRoleFunctionServiceRefresh.allFunctions = true;
                mockRoleFunctionServiceRefresh.functionsDisplayed = allFunctionsAvailable;
                deferred.resolve();
                return deferred.promise;
            }
        };

        $controller('RoleFunctionsController', { $scope: scope, $filter: $filter, RolesFunctionsService: mockRoleFunctionServiceRefresh, Roles: mockRoles, growl: mockGrowl });
        console.log(getpromiss().$promise);
        getpromiss().$promise.then(function(result) {
            mockRoleFunctionServiceRefresh.functionsDisplayed = result;
        });

        // when the roles selected the first time
        mockRoles.selectRole({ Id: '1' });
        // nodes should be selected correctly
        scope.$digest();

        expect(scope.functionsDisplayed[0].selected).toBe(true);

    }));
});  