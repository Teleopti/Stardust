Teleopti.MyTimeWeb.Schedule.Month = (function ($) {
	var self = this; 
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;
	var completelyLoaded;

	function _fetchMonthData() {
	    ajax.Ajax({
	        url: '../api/Schedule/FetchMonthData',
	        dataType: "json",
	        type: 'GET',
	        data: {
	            date: Teleopti.MyTimeWeb.Portal.ParseHash().dateHash
	        },
	        success: function (data) {
	            vm.readData(data);

	            vm.selectedDate.subscribe(function () {
		            if (vm != null) {
			            var date = vm.selectedDate();
			            date.startOf('month');
			            var probabilityPart = _getProbabilityUrlPart();
			            Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Month" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(date.format('YYYY-MM-DD')) + probabilityPart);
		            }
	            });
	            
	            completelyLoaded();
	        }
	    });
	}
    
	function _cleanBindings() {
        ko.cleanNode($('#page')[0]);
        if (vm != null) {
            vm.weekViewModels([]);
            vm = null;
        }
	}

	function _getProbabilityUrlPart() {
		var probabilityPart = '';
		var probability = Teleopti.MyTimeWeb.Portal.ParseHash().probability;
		if (probability) {
			probabilityPart = "/Probability/" + probability;
		}
		return probabilityPart;
	}
    
	return {
		Init: function () {
			if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
				Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Schedule/Month', Teleopti.MyTimeWeb.Schedule.Month.PartialInit, Teleopti.MyTimeWeb.Schedule.Month.PartialDispose);
			}
		},
		PartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
		    completelyLoaded = completelyLoadedCallback;
		    vm = new Teleopti.MyTimeWeb.Schedule.MonthViewModel(Teleopti.MyTimeWeb.Portal.NavigateTo);
		    ko.applyBindings(vm, $('#page')[0]);
		    _fetchMonthData();
		    readyForInteractionCallback();
		},
		PartialDispose: function () {
		    _cleanBindings();
		}
	};

})(jQuery);
