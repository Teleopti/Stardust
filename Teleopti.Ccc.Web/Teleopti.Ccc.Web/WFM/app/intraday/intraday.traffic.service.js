(function() {
  "use strict";
  angular.module("wfm.intraday").service("intradayTrafficService", [
    "$filter",
    "intradayService",
    "$translate",
    "$log",
    function($filter, intradayService, $translate, $log) {
      var service = {
        trafficChart: {}
      };

      var trafficData = {
        forecastedCallsObj: {
          series: [],
          max: {}
        },
        actualCallsObj: {
          series: [],
          max: {}
        },
        forecastedAverageHandleTimeObj: {
          series: [],
          max: {}
        },
        actualAverageHandleTimeObj: {
          series: [],
          max: {}
        },
        summary: {},
        hasMonitorData: false,
        waitingForData: false,
        timeSeries: [],
        currentInterval: []
      };

      var hiddenArray = [];
      var intervalStart;
	  var max;

	  var roundSeries = function (data, decimals) {
		  return data.map(function(x) {
			  if (x) {
				  return +x.toFixed(decimals);
			  }
			  return x;
		  });
	  }
	
      service.setTrafficData = function(result, isToday) {
		clearData();
		trafficData.forecastedCallsObj.series = result.DataSeries.ForecastedCalls;
		trafficData.actualCallsObj.series = result.DataSeries.CalculatedCalls;
		trafficData.forecastedAverageHandleTimeObj.series = roundSeries(result.DataSeries.ForecastedAverageHandleTime, 1);
		trafficData.actualAverageHandleTimeObj.series = roundSeries(result.DataSeries.AverageHandleTime, 1);

        trafficData.latestActualInterval =
          $filter("date")(result.LatestActualIntervalStart, "shortTime") +
          " - " +
          $filter("date")(result.LatestActualIntervalEnd, "shortTime");
        intervalStart = $filter("date")(
          result.LatestActualIntervalStart,
          "shortTime"
        );

        trafficData.forecastedCallsObj.max = Math.max.apply(
          Math,
          trafficData.forecastedCallsObj.series
        );
        trafficData.actualCallsObj.max = Math.max.apply(
          Math,
          trafficData.actualCallsObj.series
        );
        trafficData.forecastedAverageHandleTimeObj.max = Math.max.apply(
          Math,
          trafficData.forecastedAverageHandleTimeObj.series
        );
        trafficData.actualAverageHandleTimeObj.max = Math.max.apply(
          Math,
          trafficData.actualAverageHandleTimeObj.series
        );

        trafficData.forecastedCallsObj.series.splice(0, 0, "Forecasted_calls");
        trafficData.actualCallsObj.series.splice(0, 0, "Calls");
        trafficData.forecastedAverageHandleTimeObj.series.splice(
          0,
          0,
          "Forecasted_AHT"
        );
        trafficData.actualAverageHandleTimeObj.series.splice(0, 0, "AHT");

        trafficData.summary = {
          summaryForecastedCalls: $filter("number")(
            result.Summary.ForecastedCalls,
            1
          ),
          summaryForecastedAverageHandleTime: $filter("number")(
            result.Summary.ForecastedAverageHandleTime,
            1
          ),
          summaryCalculatedCalls: $filter("number")(
            result.Summary.CalculatedCalls,
            1
          ),
          summaryAverageHandleTime: $filter("number")(
            result.Summary.AverageHandleTime,
            1
          ),
          forecastActualCallsDifference: $filter("number")(
            result.Summary.ForecastedActualCallsDiff,
            1
          ),
          forecastActualAverageHandleTimeDifference: $filter("number")(
            result.Summary.ForecastedActualHandleTimeDiff,
            1
          )
        };

        angular.forEach(
          result.DataSeries.Time,
          function(value, key) {
            this.push($filter("date")(value, "shortTime"));
          },
          trafficData.timeSeries
        );

        if (trafficData.timeSeries[0] != "x") {
          trafficData.timeSeries.splice(0, 0, "x");
        }

        trafficData.hasMonitorData = result.IncomingTrafficHasData;

        trafficData.currentInterval = [];
        if (isToday) {
          getCurrent();
        }

        service.initTrafficChart();

        return trafficData;
      };

      service.getData = function() {
        return trafficData;
      };

      var clearData = function() {
        trafficData.timeSeries = [];
        trafficData.forecastedCallsObj.series = [];
        trafficData.actualCallsObj.series = [];
        trafficData.forecastedAverageHandleTimeObj.series = [];
        trafficData.actualAverageHandleTimeObj.series = [];
      };

      var getCurrent = function() {
        if (
          trafficData.forecastedCallsObj.max > trafficData.actualCallsObj.max
        ) {
          max = trafficData.forecastedCallsObj.max;
        } else {
          max = trafficData.actualCallsObj.max;
        }

        for (var i = 0; i < trafficData.timeSeries.length; i++) {
          if (trafficData.timeSeries[i] === intervalStart) {
            trafficData.currentInterval[i] = max;
          } else {
            trafficData.currentInterval[i] = null;
          }
          trafficData.currentInterval[0] = "Current";
        }
      };

      var request;
      function cancelPendingRequest() {
        if (request) {
          request.$cancelRequest("cancel");
        }
      }

      service.pollSkillData = function(selectedItem) {
        trafficData.waitingForData = true;
        cancelPendingRequest();

        request = intradayService.getSkillMonitorStatistics.query({
          id: selectedItem.Id
        });

        request.$promise.then(
          function(result) {
            trafficData.waitingForData = false;
            service.setTrafficData(result, true);
          },
          function(error) {
            trafficData.hasMonitorData = false;
          }
        );
      };

      service.pollSkillDataByDayOffset = function(
        selectedItem,
        toggles,
        dayOffset
      ) {
        trafficData.waitingForData = true;
        cancelPendingRequest();

        request = intradayService.getSkillMonitorStatisticsByDayOffset.query({
          id: selectedItem.Id,
          dayOffset: dayOffset
        });

        request.$promise.then(
          function(result) {
            trafficData.waitingForData = false;
            service.setTrafficData(result, dayOffset === 0);
          },
          function(error) {
            trafficData.hasMonitorData = false;
          }
        );
      };

      //Deprecated nothing should call this
      service.pollSkillAreaData = function(selectedItem) {
        trafficData.waitingForData = true;
        cancelPendingRequest();

        request = intradayService.getSkillAreaMonitorStatistics.query({
          id: selectedItem.Id
        });
        request.$promise.then(
          function(result) {
            trafficData.waitingForData = false;
            service.setTrafficData(result, true);
          },
          function(error) {
            trafficData.hasMonitorData = false;
          }
        );
      };

      service.pollSkillAreaDataByDayOffset = function(
        selectedItem,
        toggles,
        dayOffset
      ) {
        trafficData.waitingForData = true;
        cancelPendingRequest();

        request = intradayService.getSkillAreaMonitorStatisticsByDayOffset.query(
          {
            id: selectedItem.Id,
            dayOffset: dayOffset
          }
        );
        request.$promise.then(
          function(result) {
            trafficData.waitingForData = false;
            service.setTrafficData(result, dayOffset === 0);
          },
          function(error) {
            trafficData.hasMonitorData = false;
          }
        );
      };

      service.initTrafficChart = function() {
        service.trafficChart = c3.generate({
          bindto: "#trafficChart",
          data: {
            x: "x",
            columns: [
              trafficData.timeSeries,
              trafficData.forecastedCallsObj.series,
              trafficData.actualCallsObj.series,
              trafficData.forecastedAverageHandleTimeObj.series,
              trafficData.actualAverageHandleTimeObj.series,
              trafficData.currentInterval
            ],
            hide: hiddenArray,
            types: {
              Current: "bar"
            },
            colors: {
              Forecasted_calls: "#99D6FF",
              Calls: "#0099FF",
              Forecasted_AHT: "#FFC285",
              AHT: "#FB8C00"
            },
            names: {
              Forecasted_calls: $translate.instant('ForecastedVolume') + " ←",
              Calls: $translate.instant('ActualVolume') + " ←",
              Forecasted_AHT:
                $translate.instant('ForecastedAverageHandlingTime') + " →",
              AHT: $translate.instant('ActualAverageHandlingTime') + " →"
            },
            axes: {
              Forecasted_AHT: "y2",
              AHT: "y2"
            }
          },
          axis: {
            x: {
              label: {
                text: $translate.instant('SkillTypeTime'),
                position: "outer-center"
              },
              type: "category",
              tick: {
                culling: {
                  max: 24
                },
                fit: true,
                centered: true,
                multiline: false
              }
            },
            y: {
              label: {
                text: $translate.instant('Volume'),
                position: "outer-middle"
              },
              tick: {
                format: d3.format(".1f")
              }
            },
            y2: {
              label: {
                text: $translate.instant('AverageHandlingTime'),
                position: "outer-middle"
              },
              show: true,
              tick: {
                format: d3.format(".1f")
              }
            }
          },
          legend: {
            item: {
              onclick: function(id) {
                if (hiddenArray.indexOf(id) > -1) {
                  hiddenArray.splice(hiddenArray.indexOf(id), 1);
                } else {
                  hiddenArray.push(id);
                }
                service.initTrafficChart();
              }
            }
          },
          transition: {
            duration: 500
          }
        });
      };

      service.initTrafficChart();

      return service;
    }
  ]);
})();
