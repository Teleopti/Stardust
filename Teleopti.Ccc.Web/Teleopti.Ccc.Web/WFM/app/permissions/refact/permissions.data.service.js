(function () {
    'use strict';

    angular
        .module('wfm.permissions')
        .service('permissionsDataService', permissionsDataService);

    permissionsDataService.$inject = ['PermissionsServiceRefact'];

    function permissionsDataService(PermissionsServiceRefact) {
        this.setSelectedRole = setSelectedRole;
        this.getSelectedRole = getSelectedRole;
        this.selectFunction = selectFunction;
        this.selectOrganization = selectOrganization;
        this.prepareData = prepareData;
        this.selectAllFunction = selectAllFunction;
        this.selectDynamicOption = selectDynamicOption;
        this.prepareDynamicOption = prepareDynamicOption;
        var selectedRole;

        function selectFunction(selectedRole, functions, func) {
            if (func.IsSelected) {
                PermissionsServiceRefact.postFunctions.query({ Id: selectedRole.Id, Functions: functions }).$promise.then(function (result) {
                });
            } else {
                PermissionsServiceRefact.deleteFunction.delete({ Id: selectedRole.Id, FunctionId: func.FunctionId }).$promise.then(function (result) {
                });
            }
        }

        var allFunctions = [];

        function selectAllFunctionHelper(functions) {
            for (var i = 0; i < functions.length; i++) {
                allFunctions.push(functions[i].FunctionId);
                if (functions[i].ChildFunctions != null && functions[i].ChildFunctions.length > 0) {
                    selectAllFunctionHelper(functions[i].ChildFunctions)
                }
            }
        }

        function selectAllFunction(selectedRole, functions, selected) {
            selectAllFunctionHelper(functions);

            if (selected) {
                PermissionsServiceRefact.postFunctions.query({ Id: selectedRole.Id, Functions: allFunctions }).$promise.then(function (result) {
                    if (result != null) {
                        console.log('result post all functions', result);
                    }
                });
            } else {
                PermissionsServiceRefact.deleteAllFunction.delete({ Id: selectedRole.Id, FunctionId: functions[0].FunctionId, Functions: allFunctions }).$promise.then(function (result) {
                    if (result != null) {
                        console.log('result delete all functions', result);
                    }
                });
            }
        }

        function prepareDynamicOption(option) {
            return  {
                Id: selectedRole.Id,
                RangeOption: option.RangeOption
            }
        }

        function selectDynamicOption(option) {
            var preparedObject = prepareDynamicOption(option);

            PermissionsServiceRefact.assignOrganizationSelection.postData(preparedObject).$promise.then(function (result) {
                if (result != null) {
                    console.log(result);
                    return result;
                }
            });
        }

        function selectOrganization(orgData) {
            var data = prepareData(orgData);

            var selectedData = {
                Type: orgData.Type,
                Id: data.Id,
                DataId: orgData.Id
            }
            if (orgData.Type === 'BusinessUnit') {
                if (orgData.IsSelected) {
                    PermissionsServiceRefact.assignOrganizationSelection.postData(data).$promise.then(function (result) {
                        if (result != null) {
                            console.log(result);
                            return result;
                        }
                    });
                } else {
                    PermissionsServiceRefact.deleteAllData.delete(data).$promise.then(function (result) {
                        if (result != null) {
                            console.log('delete all bu data result', result);
                        }
                    });
                }
            }
            else if (orgData.IsSelected) {
                PermissionsServiceRefact.assignOrganizationSelection.postData(data).$promise.then(function (result) {
                    if (result != null) {
                        console.log('assign data result ', result);
                    }
                });
            }
            else if (!orgData.IsSelected) {
                PermissionsServiceRefact.deleteAvailableData.delete(selectedData).$promise.then(function (result) {
                    if (result != null) {
                        console.log('delete data result', result);
                    }
                });
            }
        }

        function arrayCreator(orgData) {
            var map = {};
            if (orgData.Type == "BusinessUnit") {
                if (map.BusinessUnits == null) {
                    map.BusinessUnits = [];
                }
                map.BusinessUnits = map.BusinessUnits.concat(orgData.Id);
                
                if (map.Sites == null) {
                    map.Sites = [];
                }
                map.Sites = map.Sites.concat(orgData.ChildNodes.map(function(site) { return site.Id; }));
                
                if (map.Teams == null) {
                    map.Teams = [];
                }
                orgData.ChildNodes.map(function(site) { return site.ChildNodes; })
                    .forEach(function(teams) {
                        map.Teams = map.Teams.concat(teams.map(function(team) { return team.Id; }));
                    });
            } else if (orgData.Type == "Site") {
                if (map.Sites == null) {
                    map.Sites = [];
                }
                map.Sites = map.Sites.concat(orgData.Id);
                
                if (map.Teams == null) {
                    map.Teams = [];
                }
                map.Teams = map.Teams.concat(orgData.ChildNodes.map(function(site) { return site.Id; }));
            } else if (orgData.Type == "Team") {
                if (map.Teams == null) {
                    map.Teams = [];
                }
                map.Teams = map.Teams.concat(orgData.Id);
            }

            return map;
        }

        function prepareData(orgData) {
            var preparedObject = arrayCreator(orgData);
            preparedObject.Id = selectedRole.Id;

            return preparedObject;
        }

        function setSelectedRole(role) {
            selectedRole = role;
        }

        function getSelectedRole() {
            return selectedRole;
        }

    }
})();
