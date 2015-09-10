'use strict';

angular
	.module('peopleService', ['ngResource'])
	.service('People', [
		'$resource', function($resource) {
			this.search = $resource('../api/Search/People/Keyword', {
				keyword: "@searchKey",
				pageSize: "@pageSize",
				currentPageIndex: "@currentPageIndex",
				sortedColumns: "@sortedColumn"
			}, {
				query: {
					method: 'GET',
					params: {},
					isArray: false
				}
			});

			this.importUsers = $resource('../api/People/ImportPeople', {}, {
				post: {
					method: 'POST',
					params: {},
					isArray: false
				}

			});

			this.loadAllSkills = $resource("../api/PeopleData/loadAllSkills", {}, {
				get: {
					method: "GET",
					params: {},
					isArray: true
				}
			});

			this.fetchPeople = $resource("../api/PeopleData/fetchPeople", {}, {
				post: {
					method: "POST",
					params: {},
					isArray: true
				}
			});

			this.updatePeopleWithSkills = $resource("../api/PeopleCommand/updateSkill", {}, {
				post: {
					method: "POST",
					params: {},
					isArray: false
				}
			});

			this.updatePeopleWithShiftBag = $resource("../api/PeopleCommand/updateShiftBag", {}, {
				post: {
					method: "POST",
					params: {},
					isArray: false
				}
			});

			this.loadAllShiftBags = $resource("../api/PeopleData/loadAllShiftBags", {}, {
				get: {
					method: "GET",
					params: {},
					isArray: true
				}
			});
		}
	]);