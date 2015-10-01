(function() {
    'use strict';

    describe('RolesDataService', function() {
        var $q,
            $filter,
            $httpBackend,
            $rootScope;

        var mockPermissionsService = {
            rolesPermissions: {
                query: function() {
                    var result = {
                        AvailableFunctions: [
                            {
                                Id: "1",
                                FunctionCode: "",
                                FunctionPath: "",
                                LocalizedFunctionDescription: ""
                            }
                        ],
                        AvailableBusinessUnits: [],
                        AvailableSites: [],
                        AvailableTeams: []
                    };
                    var queryDeferred = $q.defer();
                    queryDeferred.resolve(result);
                    return { $promise: queryDeferred.promise };
                }
            },
            assignOrganizationSelection: {
		        postData: function(data) {
		        	var queryDeferred = $q.defer();
		        	queryDeferred.resolve();
		        	return { $promise: queryDeferred.promise };
		        }
	        }
        }; 

        beforeEach(function () {
            module('wfm.permissions');
            module(function ($provide) {
                $provide.service('PermissionsService', function () { return mockPermissionsService; });
            });
        });

        beforeEach(inject(function (_$httpBackend_, _$q_, _$filter_, _$rootScope_) {
            $q = _$q_;
            $filter = _$filter_;          
            $httpBackend = _$httpBackend_;
            $rootScope = _$rootScope_;
        }));

        fit('should call the rest service with the good params', function() {
        	inject(function (RoleDataService) {
				var deferred = $q.defer();
        		var scope = $rootScope.$new();
                var roleId =  1;
		        var nodes = [{ type: 'Site', id: '22' }, { type: 'Team', id: '23' }];
		        var expectedObject = {
			        Id: roleId,
			        Sites: ["22"],
			        Teams: ['23']
				};
		        spyOn(mockPermissionsService.assignOrganizationSelection, 'postData').and.returnValue({ $promise: deferred.promise });

		        deferred.resolve();
		        RoleDataService.assignOrganizationSelection(roleId, nodes);
		        scope.$digest();

		        expect(mockPermissionsService.assignOrganizationSelection.postData).toHaveBeenCalledWith(expectedObject);
            });
        });
        it('should call the rest service with only a team when a team is selected', function (done) {
        	inject(function (RolesFunctionsService) {
        		
        		done();
        	});
        });
       
    });
})();