(function () {
	'use strict';

	angular.module('wfm.teamSchedule').factory('TimezoneListFactory', TimezoneListFactory);

	TimezoneListFactory.$inject = ['$q', 'TimezoneDataService'];
	function TimezoneListFactory($q, TimezoneDataService) {
		var timezones = [];

		var factory = {
			Create: create
		};

		function create(avaliableTimezones) {
			return $q(function (resolve, reject) {
				getTimezoneData().then(function () {
					resolve(new TimezoneList(avaliableTimezones));
				});
			});
		}

		function TimezoneList(avaliableTimezones) {
			this.Timezones = filterByAvaliableTimezones(avaliableTimezones);
		}

		TimezoneList.prototype.GetShortName = function (ianaId) {
			var timezones = this.Timezones;
			if (!timezones || !timezones.length) return '';
			var timezone = timezones.filter(function (tz) {
				return tz.IanaId === ianaId;
			})[0];
			if (!timezone) return '';

			var name = timezone.Name;
			var reg = /\((.+?)\)/;
			var result = reg.exec(name);
			return result ? result[1] : name;
		}

		function filterByAvaliableTimezones(avaliableTimezones) {
			if (!avaliableTimezones || !avaliableTimezones.length) return angular.copy(timezones);
			return timezones.filter(function (timezone) {
				return avaliableTimezones.indexOf(timezone.IanaId) >= 0;
			});
		}


		function getTimezoneData() {
			return $q(function (resolve, reject) {
				if (!!timezones.length) {
					resolve();
					return;
				}
				TimezoneDataService.getAll().then(function (data) {
					timezones = data.Timezones;
					resolve();
				});
			});

		}

		return factory;

	}
})();