﻿define(['buster', 'views/Dashboard/vm', 'shared/timezone-current'],
	function (buster, viewModel, timezoneCurrent) {
		timezoneCurrent.SetIanaTimeZone('Europe/Berlin');
		return function () {

			buster.testCase("Dashboard viewmodel", {

				"should create viewmodel": function () {
					var vm = new viewModel();
					assert(vm);
				}
			});
		};
	});