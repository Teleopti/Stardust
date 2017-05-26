(function() {
	"use strict";

	var Chart = window.Chart;

	angular
        .module("adminApp")
        .controller("hangfireMonitoringController", hangfireMonitoringController, ["tokenHeaderService"]);

	var timeout = 10000;
	var minutesToKeep = 60;
	var maxPoints = minutesToKeep * 60 / timeout * 1000;

	function hangfireMonitoringController($http, $interval, tokenHeaderService) {
		var vm = this;
		vm.getTypesOfSucceededEvents = getTypesOfSucceededEvents;
		vm.getTypesOfFailedEvents = getTypesOfFailedEvents;
		vm.requeueFailedEvents = requeueFailedEvents;
		vm.isFetching = false;
		var chartLabels = [];
		var datasets = [
			{
				label: "Enqueued",
				fill: false,
				lineTension: 0,
				backgroundColor: "blue",
				borderColor: "blue",
				pointBorderColor: "blue",
				pointBackgroundColor: "#fff",
				pointBorderWidth: 1,
				pointHoverRadius: 5,
				pointHoverBackgroundColor: "blue",
				pointHoverBorderColor: "rgba(220,220,220,1)",
				pointHoverBorderWidth: 2,
				pointRadius: 1,
				pointHitRadius: 10,
				data: [],
				spanGaps: false
			}, {
				label: "Succeeded",
				fill: false,
				lineTension: 0,
				backgroundColor: "green",
				borderColor: "green",
				pointBorderColor: "green",
				pointBackgroundColor: "#fff",
				pointBorderWidth: 1,
				pointHoverRadius: 5,
				pointHoverBackgroundColor: "green",
				pointHoverBorderColor: "rgba(220,220,220,1)",
				pointHoverBorderWidth: 2,
				pointRadius: 1,
				pointHitRadius: 10,
				data: [],
				spanGaps: false
			}
		];
		vm.eventCount = [];
		vm.oldestEvents = [];

		var ctx = document.getElementById("statsChart");
		var chart = new Chart(ctx,
		{
			type: "line",
			data: {
				labels: chartLabels,
				datasets: datasets
			},
			options: {
				tooltips: {
					mode: "x-axis"
				}
			}
		});

		setupPolling();
		getTypesOfSucceededEvents();
		getTypesOfFailedEvents();

		function setupPolling() {
			fetchData()
				.then(poll);
		}

		function fetchData() {
			return $http.get("./Hangfire/Statistics", tokenHeaderService.getHeaders())
				.then(function (data) {
					if (chartLabels.length >= maxPoints)
						chartLabels.shift();
					chartLabels.push(data.data.Time);
					if (datasets[0].data.length >= maxPoints)
						datasets[0].data.shift();
					datasets[0].data.push(data.data.TotalEventCount);
					if (datasets[1].data.length >= maxPoints)
						datasets[1].data.shift();
					datasets[1].data.push(data.data.SucceededEventCount);

					chart.update();

					vm.oldestEvents = data.data.OldestEvents.sort(byDuration);
				});
		}

		function getTypesOfSucceededEvents() {
			vm.isFetching = true;
			return $http.get("./Hangfire/TypesOfSucceededEvents", tokenHeaderService.getHeaders())
				.then(function (data) {
					vm.eventCount = data.data.sort(byCount);
					vm.isFetching = false;
				});
		}

		function getTypesOfFailedEvents() {
			vm.isFetching = true;
			return $http.get("./Hangfire/TypesOfFailedEvents", tokenHeaderService.getHeaders())
				.then(function (data) {
					vm.eventCountFailed = data.data.sort(byCount);
					vm.isFetching = false;
				});
		}

		function requeueFailedEvents(type) {
			
		}

		function poll() {
			$interval(fetchData, timeout);
		}

		function byCount(e1, e2) {
			return e2.Count - e1.Count;
		}

		function byDuration(e1, e2) {
			if (e2.Duration < e1.Duration)
				return -1;
			if (e2.Duration > e1.Duration)
				return 1;

			if (e1.Type < e2.Type)
				return -1;
			if (e1.Type > e2.Type)
				return 1;
			return 0;
		}
	}
})();