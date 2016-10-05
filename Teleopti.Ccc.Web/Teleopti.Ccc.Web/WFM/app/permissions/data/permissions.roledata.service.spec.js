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
                        AvailableSites: [{ Id: '2', Name: 'London' }],
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
            },
        	deleteAvailableData: {
		        query: function() {
		        	var queryDeferred = $q.defer();
		        	queryDeferred.resolve();
		        	return { $promise: queryDeferred.promise };
		        }
        	},
			deleteAllData: {
				query: function (data) {
					var queryDeferred = $q.defer();
					queryDeferred.resolve();
					return { $promise: queryDeferred.promise };
				}
			},

			organizationSelections: {
            query: function() {
            	var queryDeferred = $q.defer();
            	queryDeferred.resolve({
            		BusinessUnit: {
            			Id: '1', Name: 'WFM', Type: 'BusinessUnit', ChildNodes: [
					{ Id: '2', Name: 'London', Type: 'Site', ChildNodes: [] }]
            		}
            	});
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

	
        it('should call the rest service with the node only and not the parent', function () {
        	inject(function (RoleDataService) {
        		var deferred = $q.defer();
        		var scope = $rootScope.$new();
        		var roleId = 1;
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

		it('should select the parent if one child is selected at the service initialization', function () {
			inject(function (RoleDataService) {
				var scope = $rootScope.$new();
				var data = { //FIXME with a factory
					BusinessUnit: {
						Id: '1', Name: 'WFM', Type: 'BusinessUnit', ChildNodes: [
					{ Id: '2', Name: 'London', Type: 'Site', ChildNodes: [] }]
					}
				};

				RoleDataService.refreshOrganizationSelection();
				var dataForARole = {
					AvailableSites: { Id: '2', Name: 'London' }
				};

				scope.$digest();
				RoleDataService.refreshpermissions('2');

				scope.$digest();

				expect(RoleDataService.organization.BusinessUnit[0].selected).toBe(true);
				expect(RoleDataService.organization.BusinessUnit[0].ChildNodes[0].selected).toBe(true);
			});
		});
		it('should call the rest server with the parent and all children', function () {
			inject(function (RoleDataService) {
				var scope = $rootScope.$new();
				var selectedRole = '1';
				var nodes = [{ type: 'BusinessUnit', id: '2' }, { type: 'Site', id: '3' }];
				var expectedObject = {
						Id: '1',
						BusinessUnits: ['2'],
						Sites: ['3']
					};
				spyOn(mockPermissionsService.deleteAllData, 'query');
				
				RoleDataService.deleteAllNodes(selectedRole, nodes);

				expect(mockPermissionsService.deleteAllData.query).toHaveBeenCalledWith(expectedObject);
			});
		});

	    it('should call the rest server with the node and its children', function() {
	    	inject(function (RoleDataService) {
	    		var deferred = $q.defer();
				var scope = $rootScope.$new();
			    var node = [
				    { type: 'BusinessUnit', id: '1' }, { type: 'Site', id: '2' }, { type: 'Site', id: '3' }
			    ];

	    		var roleId = 1;
	    		var expectedObject = {
	    			Id: roleId,
	    			BusinessUnits: ['1'],
					Sites: ['2', '3']
	    		};
	    		

			    spyOn(mockPermissionsService.assignOrganizationSelection, 'postData').and.returnValue({ $promise: deferred.promise});
			    deferred.resolve();
			    RoleDataService.assignOrganizationSelection(roleId, node);
			    scope.$digest();

			    expect(mockPermissionsService.assignOrganizationSelection.postData).toHaveBeenCalledWith(expectedObject);
		    });
	    	

	    });

    });
})();