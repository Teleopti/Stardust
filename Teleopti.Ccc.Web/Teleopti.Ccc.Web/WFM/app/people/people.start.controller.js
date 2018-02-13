var __assign = (this && this.__assign) || Object.assign || function(t) {
    for (var s, i = 1, n = arguments.length; i < n; i++) {
        s = arguments[i];
        for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
            t[p] = s[p];
    }
    return t;
};
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = y[op[0] & 2 ? "return" : op[0] ? "throw" : "next"]) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [0, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var _this = this;
var fetchDefaultOptions = {
    method: 'GET',
    headers: {
        Accept: 'application/json',
        'Content-Type': 'application/json',
        Cache: 'no-cache'
    },
    credentials: 'include'
};
// Fetch wrapper for json
var fetchJSON = function (input, init) { return __awaiter(_this, void 0, void 0, function () {
    return __generator(this, function (_a) {
        return [2 /*return*/, fetch(input, init).then(function (response) { return response.json(); })];
    });
}); };
// Array.map
var personToId = function (_a) {
    var PersonId = _a.PersonId;
    return PersonId;
};
// Array.sort
var sortRolesByName = function (role1, role2) { return role1.Name >= role2.Name; };
var sortPeopleByName = function (person1, person2) { return person1.FirstName >= person2.FirstName; };
// API calls
var searchPeople = function (keyword) {
    if (keyword === void 0) { keyword = 'a'; }
    return __awaiter(_this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            return [2 /*return*/, fetchJSON("../api/Search/People/Keyword?currentPageIndex=1&keyword=" + keyword + "&pageSize=10&sortColumns=LastName:true", fetchDefaultOptions).then(function (_a) {
                    var People = _a.People;
                    return People;
                })];
        });
    });
};
var getRoles = function () { return __awaiter(_this, void 0, void 0, function () {
    return __generator(this, function (_a) {
        return [2 /*return*/, fetchJSON('../api/PeopleData/fetchRoles', fetchDefaultOptions)];
    });
}); };
var getPersons = function (_a) {
    var _b = _a.Date, Date = _b === void 0 ? '2017-02-08' : _b, PersonIdList = _a.PersonIdList;
    return __awaiter(_this, void 0, void 0, function () {
        return __generator(this, function (_c) {
            return [2 /*return*/, fetchJSON("../api/PeopleData/fetchPersons", __assign({}, fetchDefaultOptions, { method: 'POST', body: JSON.stringify({ Date: Date, PersonIdList: PersonIdList }) }))];
        });
    });
};
var getPeople = function () { return __awaiter(_this, void 0, void 0, function () {
    var peopleResult, PersonIdList, persons;
    return __generator(this, function (_a) {
        switch (_a.label) {
            case 0: return [4 /*yield*/, searchPeople()];
            case 1:
                peopleResult = _a.sent();
                PersonIdList = peopleResult.map(personToId);
                return [4 /*yield*/, getPersons({ PersonIdList: PersonIdList })];
            case 2:
                persons = _a.sent();
                return [2 /*return*/, persons
                        .map(function (person) { return (__assign({}, person, { selected: false })); })
                        .sort(sortPeopleByName)];
        }
    });
}); };
(function () {
    'use strict';
    angular.module('wfm.people').controller('PeopleStart', ['$scope', PeopleStartController]);
    function PeopleStartController($scope) {
        $scope.roles = [];
        $scope.people = [];
        $scope.selectedPeople = [];
        $scope.selectedPeopleIds = [];
        $scope.selectedPeopleRoleIds = [];
        $scope.selectedRoleIds = [];
        getRoles().then(function (roles) {
            $scope.roles = roles;
            $scope.$digest();
        });
        getPeople().then(function (people) {
            $scope.people = people;
            $scope.$digest();
        });
        $scope.toggleSelectedPerson = function (person) {
            if ($scope.selectedPeopleIds.includes(person.Id))
                $scope.selectedPeopleIds = $scope.selectedPeopleIds.filter(function (id) { return id !== person.Id; });
            else
                $scope.selectedPeopleIds = $scope.selectedPeopleIds.concat([person.Id]);
            console.log($scope.selectedPeopleIds);
        };
        $scope.isSelectedPerson = function (person) { return $scope.selectedPeopleIds.includes(person.Id); };
        $scope.toggleSelectedRole = function (roleId) {
            if ($scope.selectedRoleIds.includes(roleId))
                $scope.selectedRoleIds = $scope.selectedRoleIds.filter(function (id) { return id !== roleId; });
            else
                $scope.selectedRoleIds = $scope.selectedRoleIds.concat([roleId]);
        };
        $scope.isSelectedRole = function (roleId) { return $scope.selectedRoleIds.includes(roleId); };
        $scope.getPerson = function (id) {
            return $scope.people.find(function (person) { return person.Id === id; });
        };
        $scope.getRole = function (id) {
            return $scope.roles.find(function (role) { return role.Id === id; });
        };
        $scope.$watch('selectedPeopleIds', function () {
            $scope.selectedPeople = $scope.people.filter(function (person) { return $scope.selectedPeopleIds.includes(person.Id); });
            var roleIds = {};
            $scope.selectedPeople.forEach(function (person) {
                person.Roles.forEach(function (role) { return (roleIds[role.Id] = null); });
            });
            $scope.selectedPeopleRoleIds = Object.keys(roleIds);
        });
        $scope.logRoles = function (roles) {
            console.log(roles);
        };
        $scope.getNumberOfCheckedRoles = function (roles) {
            return roles.filter(function (role) { return role.checked === true; }).length;
        };
        $scope.joinRoleNames = function (_a) {
            var Roles = _a.Roles;
            return Roles.map(function (r) { return r.Name; }).join(', ');
        };
        $scope.getRoleMatchesOnSelected = function (roleId) {
            var count = 0;
            $scope.selectedPeopleIds.forEach(function (id) {
                return $scope.getPerson(id).Roles.forEach(function (r) {
                    if (roleId === r.Id)
                        count++;
                });
            });
            return count;
        };
        $scope.isRoleOnAllSelected = function (roleId) {
            return $scope.getRoleMatchesOnSelected(roleId) === $scope.selectedPeopleIds.length;
        };
        $scope.paginationOptions = { pageNumber: 1, totalPages: 7 };
        $scope.getPageData = function (pageIndex) {
            console.log('PageIndex:', pageIndex);
        };
        $scope.selectedPersons = [];
        $scope.assertMulti = function (item) {
            $scope.currentRoles.length = 0;
            var indexOfItem = $scope.selectedPersons.indexOf(item);
            if (indexOfItem !== -1) {
                $scope.selectedPersons.splice(indexOfItem, 1);
            }
            else {
                $scope.selectedPersons.push(item);
            }
            angular.forEach($scope.selectedPersons, function (person, key) {
                angular.forEach(person.Roles, function (role, key) {
                    if (role.checked === false)
                        return;
                    if ($scope.checkIfRoleExistInCurrentRoles(role) === false) {
                        role.usedBy = 1;
                        $scope.currentRoles.push(angular.copy(role));
                    }
                    else {
                        $scope.incrementUsedBy(role);
                    }
                });
            });
        };
        $scope.isUsedBySome = function (roleId) {
            var count = $scope.getRoleMatchesOnSelected(roleId);
            return count > 0;
        };
        $scope.incrementUsedBy = function (role) {
            angular.forEach($scope.currentRoles, function (currentRole, key) {
                if (role.Name === currentRole.Name) {
                    currentRole.usedBy++;
                }
            });
        };
        $scope.checkIfRoleExistInCurrentRoles = function (role) {
            var returnValue = false;
            angular.forEach($scope.currentRoles, function (currentRole, key) {
                if (role.Name === currentRole.Name) {
                    returnValue = true;
                }
            });
            return returnValue;
        };
        $scope.viewState = {
            showGrant: false,
            showRevoke: false,
            showEditRoles: false,
            showGrantChips: false,
            showRevokeChips: false,
            reset: function () {
                $scope.viewState.showGrant = false;
                $scope.viewState.showRevoke = false;
                $scope.viewState.showEditRoles = false;
                $scope.viewState.showGrantChips = false;
                $scope.viewState.showRevokeChips = false;
            }
        };
        $scope.close = function () {
            $scope.viewState.reset();
            // $scope.clearSelectedRoles();
        };
        $scope.clearSelectedPeople = function () { return ($scope.selectedPeopleIds.length = 0); };
        $scope.clearSelectedRoles = function () { return ($scope.selectedRoleIds.length = 0); };
        $scope.getInitialState = function (role) {
            if ($scope.isRoleOnAllSelected(role.Id)) {
                return 'checked';
            }
            else if ($scope.isUsedBySome(role.Id)) {
                return 'intermediate';
            }
            else {
                return 'neutral';
            }
        };
        $scope.getRoleState = function (role) {
            if (typeof role.state === 'undefined') {
                role.state = $scope.getInitialState(role);
            }
            return role.state;
        };
        $scope.roleIsChanged = function (role) {
            var currentState = $scope.getRoleState(role);
            var initialState = $scope.getInitialState(role);
            var initialChecked = initialState === 'checked';
            var currentChecked = currentState === 'checked';
            console.log('roleIsChanged on', role.Name, currentState, initialState);
            return initialChecked !== currentChecked;
        };
        $scope.toggleRoleState = function (role) {
            console.log("Toggle " + role.Name + "(" + role.Id + ") " + role.state);
            switch ($scope.getRoleState(role)) {
                case 'neutral':
                    if ($scope.isRoleOnAllSelected(role.Id)) {
                        role.state = 'checked';
                        role.checked = true;
                    }
                    else if ($scope.isUsedBySome(role.Id)) {
                        role.state = 'intermediate';
                        role.checked = false;
                    }
                    else {
                        role.state = 'checked';
                        role.checked = true;
                    }
                    //role.state = 'checked';
                    //role.checked = true;
                    break;
                case 'checked':
                    role.state = 'neutral';
                    role.checked = false;
                    break;
                case 'intermediate':
                    role.state = 'checked';
                    role.checked = true;
                    break;
                default:
                    role.state = 'neutral';
                    role.checked = false;
            }
        };
        $scope.save = function (shouldGrant) {
            // angular.forEach($scope.selectedPersons, function(person, key) {
            // 	angular.forEach(person.Roles, function(role, key) {
            // 		angular.forEach($scope.newRoles, function(newRole, key) {
            // 			if (role.name === newRole.name) {
            // 				console.log(shouldGrant);
            // 				role.checked = shouldGrant;
            // 			}
            // 		});
            // 	});
            // });
            // $scope.newRoles.length = 0;
            // $scope.clearSelectedPeople();
            // $scope.clearSelectedRoles();
            $scope.viewState.reset();
        };
    }
})();
