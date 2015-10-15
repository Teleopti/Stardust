(function() {
    'use strict';

    describe('RolesFunctionsService', function() {
        var $q,
            $filter,
            $httpBackend;

        var mockPermissionsService = {
            applicationFunctions: {
                query: function() {
                    var queryDeferred = $q.defer();
                    queryDeferred.resolve([
                        {
                            FunctionDescription: "desc",
                            LocalizedFunctionDescription: "",
                            FunctionCode: "",
                            IsDisabled: false,
                            FunctionId: 1,
                            ChildFunctions: []
                        }
                    ]);
                    return { $promise: queryDeferred.promise };
                }
            },
            postFunction: {
                query: function() {
                    var queryDeferred = $q.defer();
                    queryDeferred.resolve({});
                    return { $promise: queryDeferred.promise };
                }
            },
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
            deleteAllFunction: {
				query: function (queryObject){}
            }
        }; 

        beforeEach(function () {
            module('wfm');
            module(function ($provide) {
                $provide.service('PermissionsService', function () { return mockPermissionsService; });
            });
        });

        beforeEach(inject(function (_$httpBackend_, _$q_, _$filter_) {
            $q = _$q_;
            $filter = _$filter_;          
            $httpBackend = _$httpBackend_;
            $httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'en');
            $httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, { Language: 'en', DateFormat: 'en' });
            $httpBackend.expectGET("html/forecasting/forecasting.html").respond(200);
            $httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'en');
            $httpBackend.expectGET("html/forecasting/forecasting-overview.html").respond(200);
        }));


        it('should select all nodes when toggle is true', function(done) {
            inject(function(RolesFunctionsService) {
                var role = { Id: 1 };
                var node = {
                    selected: false,
                    ChildFunctions: [{ selected: false }]
                };

                RolesFunctionsService.functionsDisplayed = [node];              
             RolesFunctionsService.selectAllFunctions(role);
             
                $httpBackend.flush();
                
                expect(node.selected).toBe(true);                
                expect(node.ChildFunctions[0].selected).toBe(true);               
                done();
            });
        });
        it('should select all nodes when "All" is selected in the server', function (done) {
            inject(function (RolesFunctionsService) {

                var nmbFunctionsTotal = {};
                var allSelected = true;
                var allFunctionsAvailable = [{
                    FunctionId: "1", FunctionCode: "All", FunctionPath: "All", LocalizedFunctionDescription: "",
                    ChildFunctions: [{ FunctionId: "2", ChildFunctions: [] },
                        { FunctionId: "3", ChildFunctions: [] }]
                },
                {
                    FunctionId: "4", FunctionCode: "GG", FunctionPath: "GG", LocalizedFunctionDescription: "",
                    selected: false,
                    ChildFunctions: []
                }
                ];
                var functionsAvailableForARole = [{
                    Id: "1", FunctionCode: "All", FunctionPath: "All", LocalizedFunctionDescription: "",
                    ChildFunctions: [{ Id: "2", ChildFunctions:[] }]
                  }];
                
                RolesFunctionsService.parseFunctions(allFunctionsAvailable,functionsAvailableForARole, nmbFunctionsTotal, allSelected);

                $httpBackend.flush();

                expect(allFunctionsAvailable[0].selected).toBe(true);
                expect(allFunctionsAvailable[0].ChildFunctions[0].selected).toBe(true);
                expect(allFunctionsAvailable[1].selected).toBe(true);
                done();
            });
        });

        it('should send a formatted request to PermissionsService to unselect all functions for a role in one call', function () {
        	inject(function (RolesFunctionsService) {

        		var allFunctionsAvailable = [{
        			FunctionId: "1", FunctionCode: "All", FunctionPath: "All", LocalizedFunctionDescription: "",
        			ChildFunctions: [{ FunctionId: "2", ChildFunctions: [] }]
        		},
                {
                	FunctionId: "4", FunctionCode: "GG", FunctionPath: "GG", LocalizedFunctionDescription: "",
                	selected: true,
                	ChildFunctions: []
                }
        		];
        		
        		RolesFunctionsService.functionsDisplayed = allFunctionsAvailable;

        		var expectedObject = {Id: 42, FunctionId: "1", Functions:["1","2","4"]};

		        spyOn(mockPermissionsService.deleteAllFunction, 'query');

        		RolesFunctionsService.unselectAllFunctions({ Id: 42 });

        		expect(mockPermissionsService.deleteAllFunction.query).toHaveBeenCalledWith(expectedObject);

        	});
        });
    });
})();