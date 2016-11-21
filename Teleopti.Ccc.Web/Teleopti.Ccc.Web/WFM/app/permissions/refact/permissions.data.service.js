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

        var selectedRole;
        var data = {};

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
                            console.log('assign bu data result ', result);
                        }
                    });
                } else {
                    PermissionsServiceRefact.deleteAllData.delete(data).$promise.then(function () {
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
                PermissionsServiceRefact.deleteAvailableData.delete(selectedData).$promise.then(function () {
                    if (result != null) {
                        console.log('delete data result', result);
                    }
                });
            }
        }

        function arrayCreator(orgData) {
            var attributeName = orgData.Type + 's';
            if (!data[attributeName]) {
                data[attributeName] = [];
            }
            data[attributeName].push(orgData.Id);

            if (orgData.ChildNodes != null && orgData.ChildNodes.length > 0) {
                for (var i = 0; i < orgData.ChildNodes.length; i++) {
                    arrayCreator(orgData.ChildNodes[i]);
                }
            }
            return data;
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
