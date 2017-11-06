(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('SizeStorageService', sizeStorageService);

	function sizeStorageService(){
		var key = 'teams_size_data';
		
		this.setSize = setSize;
		this.getSize = getSize;
		
		function setSize(tableHeight, tableBodyHeight, chartHeight){
			localStorage.setItem(key, JSON.stringify({tableHeight: tableHeight, tableBodyHeight:tableBodyHeight, chartHeight: chartHeight}));
		}
		
		function getSize(){
			var sizeData = localStorage.getItem(key);
			if (sizeData){
				return JSON.parse(sizeData);
			}
			
		}
	}
})();