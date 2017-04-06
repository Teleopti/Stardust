(function (angular) {
	'use strict';

	angular.module('wfm.people').service('PeopleService', [
		'$resource', '$http', function($resource, $http) {
			var newImportJobUrl = '../api/People/NewImportAgentJob';
			var uploadAgentUrl = '../api/People/UploadAgent';

			this.newImportJob = function (file, fields) {
				return submitFileAndFields(newImportJobUrl, file, fields);
			};

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

			this.uploadUserFromFile = function(file) {
				var config = overload({
					url: '../api/People/UploadPeople',
					method: 'POST',
					responseType: 'arraybuffer',
					file: file,
					headers: {
						'Accept': 'application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/json'
					}
				});
				return $http(config);

			};

			this.uploadAgentFromFile = function (file, fields) {
				return submitFileAndFields(uploadAgentUrl, file, fields);
			};

			function submitFileAndFields(url, file, fields) {
				var config = {
					url: url,
					method: 'POST',
					responseType: 'arraybuffer',
					file: file,
					headers: {
						'Accept': 'application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/json'
					}
				};
				if (fields) {
					config.fields = normalizeFormData(fields);
				}
				config = overload(config);
				return $http(config);
			}

			function normalizeFormData(fields) {
				return {
					StartDate: fields.startDate,
					RoleIds: angular.isArray(fields.roles) && fields.roles.length > 0 ? fields.roles.join(',') : undefined,
					TeamId: fields.team,
					SkillIds: angular.isArray(fields.skills) && fields.skills.length > 0 ? fields.skills.join(',') : undefined,
					ExternalLogonId: fields.externalLogon,
					ContractId: fields.contract,
					ContractScheduleId: fields.contractSchedule,
					ShiftBagId: fields.shiftBag,
					PartTimePercentageId: fields.partTimePercentage,
					SchedulePeriodType: fields.schedulePeriodType,
					SchedulePeriodLength: fields.schedulePeriodLength
				};
			}

			this.downloadFileTemplate = function() {
				var config = overload({
					url: '../api/People/UserTemplate',
					method: 'POST',
					responseType: 'arraybuffer',
					headers: {
						'Accept': 'application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
					}
				});
				return $http(config);

			};

			this.downloadFileTemplateAgent = function () {
				var config = overload({
					url: '../api/People/AgentTemplate',
					method: 'POST',
					responseType: 'arraybuffer',
					headers: {
						'Accept': 'application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
					}
				});
				return $http(config);
			};

			function overload(config) {
				function addFieldToFormData(formData, val, key) {
					if (val !== undefined) {
						if (angular.isDate(val)) {
							val = val.toISOString();
						}
						if (angular.isString(val)) {
							formData.append(key, val);
						} else if (config.sendFieldsAs === 'form') {
							if (angular.isObject(val)) {
								for (var k in val) {
									if (val.hasOwnProperty(k)) {
										addFieldToFormData(formData, val[k], key + '[' + k + ']');
									}
								}
							} else {
								formData.append(key, val);
							}
						} else {
							val = angular.isString(val) ? val : JSON.stringify(val);
							if (config.sendFieldsAs === 'json-blob') {
								formData.append(key, new Blob([val], { type: 'application/json' }));
							} else {
								formData.append(key, val);
							}
						}
					}
				}

				config.headers = config.headers || {};
				config.headers['Content-Type'] = undefined;
				config.transformRequest = config.transformRequest ?
				(angular.isArray(config.transformRequest) ?
					config.transformRequest : [config.transformRequest]) : [];
				config.transformRequest.push(function(data) {
					var formData = new FormData();
					var allFields = {};
					var key;
					for (key in config.fields) {
						if (config.fields.hasOwnProperty(key)) {
							allFields[key] = config.fields[key];
						}
					}
					if (data) allFields.data = data;
					for (key in allFields) {
						if (allFields.hasOwnProperty(key)) {
							var val = allFields[key];
							if (config.formDataAppender) {
								config.formDataAppender(formData, key, val);
							} else {
								addFieldToFormData(formData, val, key);
							}
						}
					}

					if (config.file != null) {
						var fileFormName = config.fileFormDataName || 'file';

						if (angular.isArray(config.file)) {
							var isFileFormNameString = angular.isString(fileFormName);
							for (var i = 0; i < config.file.length; i++) {
								formData.append(isFileFormNameString ? fileFormName : fileFormName[i], config.file[i],
								(config.fileName && config.fileName[i]) || config.file[i].name);
							}
						} else {
							formData.append(fileFormName, config.file, config.fileName || config.file.name);
						}
					}
					return formData;
				});

				return config;
			}
		}
	]);
})(angular);
