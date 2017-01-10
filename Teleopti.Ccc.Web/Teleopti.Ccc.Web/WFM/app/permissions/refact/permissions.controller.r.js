(function () {
  'use strict';

  angular
  .module('wfm.permissions')
  .controller('PermissionsCtrlRefact', PermissionsCtrl);

  PermissionsCtrl.$inject = ['$filter', 'PermissionsServiceRefact', 'permissionsDataService'];

  function PermissionsCtrl($filter, PermissionsServiceRefact, permissionsDataService) {
    var vm = this;
    vm.showCreateModal;
    vm.roleName;

    fetchData();
    vm.createRole = createRole;
    vm.editRole = editRole;
    vm.deleteRole = deleteRole;
    vm.copyRole = copyRole;
    vm.selectRole = selectRole;
    vm.checkMyRole = checkMyRole;
    vm.prepareTree = prepareTree;
    vm.toggleAllFunction = toggleAllFunction;
    vm.unSelectedFunctionsFilter = unSelectedFunctionsFilter;
    vm.selectedFunctionsFilter = selectedFunctionsFilter;
    vm.allFunctionsFilter = allFunctionsFilter;
    vm.isFunctionSelected = isFunctionSelected;
    vm.selectFunction = selectFunction;
    vm.selectedOrNot = false;
    vm.selectedRole = {};
    vm.selectedFunctions = {};
    vm.componentFunctions = [];
    vm.listHandler = listHandler;
    vm.selectDynamicOption = selectDynamicOption;
    vm.allDataFilter = allDataFilter;
    vm.selectedDataFilter = selectedDataFilter;
    vm.unselectedDataFilter = unselectedDataFilter;
    vm.filteredOrganizationSelection = {};
    vm.onFunctionClick = functionClick;
    vm.onNodeClick = nodeClick;
    vm.functionsDescriptionFilter = functionsDescriptionFilter;
    vm.orgDataDescriptionFilter = orgDataDescriptionFilter;

    var functionsFilter = $filter('functionsFilter');
    var dataFilter = $filter('dataFilter');
    var descriptionFilter = $filter('newDescriptionFilter');

    function isFunctionSelected(func) {
      return vm.selectedFunctions[func.FunctionId];
    }
    function selectFunction(func) {
      if (vm.selectedFunctions[func.FunctionId]) {
        delete vm.selectedFunctions[func.FunctionId];
      } else {
        vm.selectedFunctions[func.FunctionId] = true;
      }
    }

    function allDataFilter() {
      orgDataHandler(vm.organizationSelection);
    }

    function selectedDataFilter () {
      var data = dataFilter.selected(vm.organizationSelection);
      orgDataHandler(data);
    }

    function unselectedDataFilter() {
      var data = dataFilter.unselected(vm.organizationSelection);
      orgDataHandler(data);
    }

    function orgDataHandler(orgData) {
      vm.filteredOrganizationSelection = orgData;
    }

    function selectDynamicOption(option) {
      permissionsDataService.selectDynamicOption(option);
    }

    function listHandler(arr) {
      vm.componentFunctions = arr;
    }

    function orgDataDescriptionFilter(searchString) {
      var data = descriptionFilter.filterOrgData(vm.organizationSelection, searchString);
      orgDataHandler(data);
    }

    function functionsDescriptionFilter(searchString) {
      var filteredArray = descriptionFilter.filterFunctions(vm.applicationFunctions, searchString);
      listHandler(filteredArray);
    }

    function unSelectedFunctionsFilter() {
      var filteredArray = functionsFilter.unselected(vm.applicationFunctions, vm.selectedFunctions);
      listHandler(filteredArray);
    }

    function selectedFunctionsFilter() {
      var filteredArray = functionsFilter.selected(vm.applicationFunctions, vm.selectedFunctions);
      listHandler(filteredArray);
    }

    function allFunctionsFilter() {
      listHandler(vm.applicationFunctions);
    }

    function toggleAllFunction() {
      vm.selectedOrNot = !vm.selectedOrNot;
      toggleSelection(vm.applicationFunctions, vm.selectedOrNot);

      if (vm.selectedRole != null) {
        permissionsDataService.selectAllFunction(vm.selectedRole, vm.applicationFunctions, vm.selectedOrNot);
      }
    }

    function toggleSelection(functions, selectedOrNot) {
      if (functions != null) {
        for (var i = 0; i < functions.length; i++) {
          var func = functions[i];
          if (selectedOrNot) {
            vm.selectedFunctions[func.FunctionId] = true;
          } else {
            delete vm.selectedFunctions[func.FunctionId];
          }
          if (functions[i].ChildFunctions != null && functions[i].ChildFunctions.length > 0) {
            toggleSelection(functions[i].ChildFunctions, selectedOrNot);
          }
        }
      }
    }

    function toggleOrganizationSelecton(businessUnit, selectedOrNot) {
      businessUnit.IsSelected = selectedOrNot;

      for (var i = 0; i < businessUnit.ChildNodes.length; i++) {
        businessUnit.ChildNodes[i].IsSelected = selectedOrNot;
        for (var j = 0; j < businessUnit.ChildNodes[i].ChildNodes.length; j++) {
          businessUnit.ChildNodes[i].ChildNodes[j].IsSelected = selectedOrNot;
        }
      }
    }

    function refreshRoleSelection() {
      for (var i = 0; i < vm.roles.length; i++) {
        vm.roles[i].IsSelected = false;
      }
      vm.roles[0].IsSelected = true;
    }

    function createRole(roleName) {
      var roleData = { Description: roleName };
      PermissionsServiceRefact.roles.save(roleData).$promise.then(function (data) {
        vm.roles.unshift(data);
        vm.selectedRole = data;
        // permissionsDataService.setSelectedRole(vm.selectedRole);
        refreshRoleSelection();
        markSelectedRole(vm.selectedRole);
      });
      vm.showCreateModal = false;
      vm.roleName = '';
      toggleSelection(vm.applicationFunctions, false);

      if (vm.organizationSelection !== null && vm.organizationSelection.BusinessUnit !== null) {
        toggleOrganizationSelecton(vm.organizationSelection.BusinessUnit, false);
      }

    }

    function editRole(newRoleName, role) {
      PermissionsServiceRefact.manage.update({ Id: role.Id, newDescription: newRoleName }).$promise.then(function () {
        PermissionsServiceRefact.roles.query().$promise.then(function (data) {
          vm.roles = data;
          var selected = data.find(function (r) {
            return r.Id === role.Id;
          });
          markSelectedRole(selected);
        });
      });
    }

    function checkMyRole(role) {
      if (role.IsMyRole || role.BuiltIn) {
        return false;
      } else {
        return true;
      }
    }

    function deleteRole(role) {
      if (role.BuiltIn) {
        //Hide Delete button for built in roles
        return;
      }

      PermissionsServiceRefact.manage.deleteRole({ Id: role.Id });
      // permissionsDataService.setSelectedRole(null);
      var index = vm.roles.indexOf(role);
      vm.roles.splice(index, 1);

      if (role.Id === vm.selectedRole.Id) {
        toggleSelection(vm.applicationFunctions, false);
      }

      if (vm.organizationSelection !== null && vm.organizationSelection.BusinessUnit !== null) {
        toggleOrganizationSelecton(vm.organizationSelection.BusinessUnit, false);
      }
    }

    function copyRole(role) {
      PermissionsServiceRefact.copyRole.copy({ Id: role.Id }).$promise.then(function (data) {
        vm.roles.unshift(data);
        vm.selectedRole = data;
        // permissionsDataService.setSelectedRole(vm.selectedRole);
        refreshRoleSelection();
        markSelectedRole(vm.selectedRole);
      });
    }

    function selectRole(role) {
      markSelectedRole(role);

      PermissionsServiceRefact.manage.getRoleInformation({ Id: role.Id }).$promise.then(function (data) {
        vm.selectedRole = data;
        // permissionsDataService.setSelectedRole(vm.selectedRole);

        if (vm.selectedRole.AvailableTeams != null && vm.selectedRole.AvailableTeams.length > 0){
          matchOrganizationData();
        }
        if (vm.selectedRole.AvailableTeams != null && vm.selectedRole.AvailableTeams.length < 1){
          toggleOrganizationSelecton(vm.organizationSelection.BusinessUnit, false);
        }
        matchFunctionsForSelctedRole();
      });
    }

    function matchFunctionsForSelctedRole(){
      function loop(functions) {
        if (functions == null) {
          return;
        }
        functions.forEach(function(f) {
          vm.selectedFunctions[f.Id] = true;
          loop(f.ChildFunctions);
        });
      }
      vm.selectedFunctions = {};
      loop(vm.selectedRole.AvailableFunctions);
    }

    var matchBusinessUnit = function () {
      vm.organizationSelection.BusinessUnit.IsSelected = vm.selectedRole.AvailableBusinessUnits.some(function (bu) {
        return vm.organizationSelection.BusinessUnit.Id == bu.Id;
      });
    };

    var matchSites = function (sites) {
      sites.forEach(function (site) {
        site.IsSelected = vm.selectedRole.AvailableSites.some(function (s) {
          return site.Id == s.Id;
        });
      });
    };

    var matchTeams = function (sites) {
      sites.forEach(function(site) {
        site.ChildNodes.forEach(function(team) {
          team.IsSelected = vm.selectedRole.AvailableTeams.some(function(t) {
            return team.Id == t.Id;
          });
        });
      });
    };

    function matchOrganizationData() {
      var sites = vm.organizationSelection.BusinessUnit.ChildNodes;
      matchBusinessUnit();
      matchSites(sites);
      matchTeams(sites);
    }

    var previously = null;
    function markSelectedRole(role) {
      if (previously != null) {
        previously.IsSelected = false;
      }
      role.IsSelected = true;
      previously = role;
    }

    function prepareTree(appFunctions) {
      if (appFunctions != null && appFunctions.length > 1) {
        appFunctions[1].IsOpen = true;
        appFunctions[0].IsHidden = true;
      }
    }

    function fetchData() {
      PermissionsServiceRefact.roles.query().$promise.then(function (data) {
        vm.roles = data;
      });
      PermissionsServiceRefact.applicationFunctions.query().$promise.then(function (data) {
        vm.applicationFunctions = data;
        listHandler(vm.applicationFunctions);
        prepareTree(vm.applicationFunctions);
      });
      PermissionsServiceRefact.organizationSelection.get().$promise.then(function (data) {
        vm.organizationSelection = data;
        orgDataHandler(vm.organizationSelection);
      });
    }

    function nodeClick(node) {
      if (vm.selectedRole != null) {
      permissionsDataService.selectOrganization(node, vm.selectedRole);
      }
    }

    function functionClick(fn) {
      if (vm.selectedRole === null) {
        return;
      }
      var parents = permissionsDataService.findParentFunctions(vm.applicationFunctions, fn)
        .map(function(f) { return f.FunctionId; });
      var functions = [];
      if (isFunctionSelected(fn)) {
        functions = parents.concat(fn.FunctionId);
      }
      permissionsDataService.selectFunction(vm.selectedRole, functions, fn);
    }
  }
})();
