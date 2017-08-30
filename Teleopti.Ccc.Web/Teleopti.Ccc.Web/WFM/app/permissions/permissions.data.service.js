(function() {
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
		this.findChildFunctions = findChildFunctions;
		this.findParentFunctions = findParentFunctions;
		this.findFunction = findFunction;
		this.findOrgData = findOrgData;
		this.setIdForSiteWithAllTeamsSelected = setIdForSiteWithAllTeamsSelected;
		this.getIdForSiteWithAllTeamsSelected = getIdForSiteWithAllTeamsSelected;

		var selectedRole;
		var idForSiteWithAllTeamsSelected = null;

		function findChildFunctions(fn) {
			function inner(functions) {
				if (functions == null) {
					return [];
				}

				return functions.reduce(function(children, func) {
					return children.concat(func).concat(inner(func.ChildFunctions));
				}, []);
			}

			return inner(fn.ChildFunctions);
		}

		function findParentFunctions(allFunctions, func) {
			function inner(parents, toCheck, func) {
				if (toCheck == null) {
					return null;
				}
				var match = toCheck.find(function(f) {
					return f.FunctionId === func.FunctionId;
				});
				if (match != null) {
					return parents;
				}

				for (var i = 0; i < toCheck.length; i++) {
					var f = toCheck[i];
					var result = inner(parents.concat(f), f.ChildFunctions, func);
					if (result != null) {
						return result;
					}
				}

				return null;
			}

			var result = inner([], allFunctions, func);
			if (result == null) {
				return [];
			}

			return result;
		}

		function findFunction(functions, functionId) {
			var children = [].concat(functions);
			while (children.length > 0) {
				var current = children.shift();
				if (current.FunctionId === functionId) {
					return current;
				}

				if (current.ChildFunctions != null) {
					children = children.concat(current.ChildFunctions);
				}
			}
		}

		function matchFunction(functions, func) {
			return functions.some(function(id) {
				return id == func.FunctionId;
			});
		}

		function selectFunction(selectedRole, functions, func) {
			if (matchFunction(functions, func)) {
				PermissionsServiceRefact.postFunctions.query({
					Id: selectedRole.Id,
					Functions: functions
				}).$promise.then(function(result) {});
			} else {
				PermissionsServiceRefact.deleteFunction.delete({
					Id: selectedRole.Id,
					FunctionId: func.FunctionId
				}).$promise.then(function(result) {});
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
			allFunctions = [];
			selectAllFunctionHelper(functions);

			if (selected) {
				return PermissionsServiceRefact.postFunctions.query({
					Id: selectedRole.Id,
					Functions: allFunctions
				}).$promise;
			} else {
				return PermissionsServiceRefact.deleteAllFunction.delete({
					Id: selectedRole.Id,
					FunctionId: functions[0].FunctionId,
					Functions: allFunctions
				}).$promise;
			}
		}

		function prepareDynamicOption(option, selectedRole) {
			return {
				Id: selectedRole.Id,
				RangeOption: option.RangeOption
			}
		}

		function selectDynamicOption(option, selectedRole) {
			var preparedObject = prepareDynamicOption(option, selectedRole);

			PermissionsServiceRefact.assignOrganizationSelection.postData(preparedObject).$promise.then(function(result) {
				if (result != null) {
					return result;
				}
			});
		}

		function findOrgData(orgData, orgDataId) {
			var children = [].concat(orgData);
			while (children.length > 0) {
				var current = children.shift();
				if (current.Id === orgDataId) {
					return current
				}

				if (current.ChildNodes != null) {
					children = children.concat(current.ChildNodes);
				}
			}
		}

		function selectOrganization(orgData, selectedRole, isSelected) {
			var data = prepareData(orgData, selectedRole);
			var selectedData = {
				Type: orgData.Type,
				Id: data.Id,
				DataId: orgData.Id
			}
			if (orgData.Type === 'BusinessUnit') {
				if (isSelected) {
					PermissionsServiceRefact.assignOrganizationSelection.postData(data).$promise.then(function(result) {
						if (result != null) {
							return result;
						}
					});
				} else {
					PermissionsServiceRefact.deleteAllData.delete(data).$promise.then(function(result) {});
				}
			} else if (orgData.Type === 'Site') {
				if (isSelected) {
					PermissionsServiceRefact.assignOrganizationSelection.postData(data).$promise.then(function(result) {});
				} else {
					PermissionsServiceRefact.deleteAllData.delete(data).$promise.then(function(result){});
					}
				}
				else if (orgData.Type === 'Team') {
					if (isSelected) {
						PermissionsServiceRefact.assignOrganizationSelection.postData(data).$promise.then(function(result) {});
					} else {
						PermissionsServiceRefact.deleteAvailableData.delete(selectedData).$promise.then(function(result) {});
					}
				}
		}

		function arrayCreator(orgData) {
			var map = {};
			if (orgData.Type == "BusinessUnit") {
				// if (map.BusinessUnits == null) {
				// 	map.BusinessUnits = [];
				// }
				// map.BusinessUnits = map.BusinessUnits.concat(orgData.Id);
				map.BusinessUnits = [];

				if (map.Sites == null) {
					map.Sites = [];
				}
				map.Sites = map.Sites.concat(orgData.ChildNodes.map(function(site) {
					return site.Id;
				}));

				if (map.Teams == null) {
					map.Teams = [];
				}
				orgData.ChildNodes.map(function(site) {
						return site.ChildNodes;
					})
					.forEach(function(teams) {
						map.Teams = map.Teams.concat(teams.map(function(team) {
							return team.Id;
						}));
					});
			} else if (orgData.Type == "Site") {
				if (map.Sites == null) {
					map.Sites = [];
				}
				map.Sites = map.Sites.concat(orgData.Id);

				if (map.Teams == null) {
					map.Teams = [];
				}
				map.Teams = map.Teams.concat(orgData.ChildNodes.map(function(site) {
					return site.Id;
				}));
			} else if (orgData.Type == "Team") {
				if (map.Teams == null) {
					map.Teams = [];
				}
				if (getIdForSiteWithAllTeamsSelected() != null) {
					map.Sites = [];
					map.Sites.push(idForSiteWithAllTeamsSelected);
				}
				map.Teams = map.Teams.concat(orgData.Id);
			}

			return map;
		}

		function setIdForSiteWithAllTeamsSelected(id) {
			idForSiteWithAllTeamsSelected = id;
		}

		function getIdForSiteWithAllTeamsSelected() {
			return idForSiteWithAllTeamsSelected;
		}

		function prepareData(orgData, selectedRole) {
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
