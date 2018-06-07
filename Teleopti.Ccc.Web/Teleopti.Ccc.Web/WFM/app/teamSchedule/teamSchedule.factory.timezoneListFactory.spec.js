(function () {
	'use strict';

	describe('#TimezoneListFactory#', function () {
		var target;
		beforeEach(module('wfm.teamSchedule'));
		beforeEach(function () {
			module(function ($provide) {
				$provide.service('TimezoneDataService', function () {
					return {
						getAll: function () {
							return {
								then: function (callback) {
									callback({
										Timezones: [
											{
												IanaId: "Asia/Hong_Kong",
												Name: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
											}, {
												IanaId: "Europe/Berlin",
												Name: "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna"
											}
										]
									});
								}
							};
						}
					};
				});
			})
		});
		beforeEach(inject(function (_TimezoneListFactory_) {
			target = _TimezoneListFactory_;
		}));

		it('should get all timezone list if no avaliable timezones input', function () {
			target.Create().then(function (timezoneList) {
				expect(timezoneList.Timezones.length).toEqual(2);

				expect(timezoneList.Timezones[0].IanaId).toEqual('Asia/Hong_Kong');
				expect(timezoneList.Timezones[0].Name).toEqual('(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi');
				expect(timezoneList.Timezones[1].IanaId).toEqual('Europe/Berlin');
				expect(timezoneList.Timezones[1].Name).toEqual('(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna');
			});
		});

		it('should get timezone list filter by avaliable timezones', function () {
			target.Create(['Asia/Hong_Kong']).then(function (timezoneList) {
				expect(timezoneList.Timezones.length).toEqual(1);
				expect(timezoneList.Timezones[0].IanaId).toEqual('Asia/Hong_Kong');
			});
		});

		it('should get short name by timezone id', function () {
			target.Create(['Asia/Hong_Kong']).then(function (timezoneList) {
				expect(timezoneList.GetShortName('Asia/Hong_Kong')).toEqual("UTC+08:00");
				expect(!!timezoneList.GetShortName('Europe/Berlin')).toBeFalsy();
			});
		})
	});
})();