
(function() {﻿
	'use strict';﻿﻿
	angular﻿.module('wfm.rta')﻿.controller('RtaHistoricalController', RtaHistoricalController);﻿﻿
	RtaHistoricalController.$inject = [];﻿﻿
	function RtaHistoricalController() {﻿
		var vm = this;
		vm.agentName = 'Herman Dahlén';

		vm.agentsFullSchedule = [{
				Color: 'lightgreen',
				Offset: '10%',
				Width: '10%'
			}, {
        Color: 'red',
				Offset: '20%',
				Width: '5%'
      },
      {
        Color: 'lightgreen',
        Offset: '25%',
        Width: '15%'
      },
      {
				Color: 'yellow',
				Offset: '40%',
				Width: '10%'
			}, {
				Color: 'lightgreen',
				Offset: '50%',
				Width: '20%'
			}, {
				Color: 'red',
				Offset: '70%',
				Width: '10%'
			}, {
				Color: 'lightgreen',
				Offset: '80%',
				Width: '10%'
			}];

			vm.outOfAdherences = [{
				Offset: '3%',
				Width: '4%',
				StartTime: '08:00:00',
				EndTime: '08:15:00'
			}, {
				Offset: '10%',
				Width: '5%',
				StartTime: '08:00:00',
				EndTime: '08:15:00'
			}, {
				Offset: '27%',
				Width: '.04%',
				StartTime: '11:58:00',
				EndTime: '12:00:00'
			}, {
				Offset: '28%',
				Width: '5%',
				StartTime: '12:00:00',
				EndTime: '12:05:00'
			}, {
				Offset: '82%',
				Width: '5%',
				StartTime: '16:00:00',
				EndTime: '16:05:00'
			}];

		vm.fullTimeline = [{
			Offset: 'calc(10% - 16px)',
			Time: '09:00'
		}, {
			Offset: 'calc(20% - 16px)',
			Time: '10:00'
		}, {
			Offset: 'calc(30% - 16px)',
			Time: '11:00'
		}, {
			Offset: 'calc(40% - 16px)',
			Time: '12:00'
		}, {
			Offset: 'calc(50% - 16px)',
			Time: '13:00'
		}, {
			Offset: 'calc(60% - 16px)',
			Time: '14:00'
		}, {
			Offset: 'calc(70% - 16px)',
			Time: '15:00'
		}, {
			Offset: 'calc(80% - 16px)',
			Time: '16:00'
		}, {
			Offset: 'calc(90% - 16px)',
			Time: '17:00'
		}];

		﻿﻿
	}﻿
})();
