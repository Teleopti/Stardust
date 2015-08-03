(function () {
	'use strict';

	angular.module('wfm.outbound').directive('selectableDataGrid', [dataGridSelectable]);

	function dataGridSelectable() {
	    return {
	    	scope: {
	    		startingOffset: '@?',
	    		itemsPerRow: '@',
				header: '=?',
	    		recordItems: '='				
	    	},
			require: ['selectableDataGrid'],
			controller: ['$scope', '$compile', selectableDataGridCtrl],
	    	templateUrl: 'html/outbound/data-grid-selectable.tpl.html',
			transclude: true,
	    	link: postlink

	    };

	    function postlink(scope, elem, attrs, ctrls, transcludeFn) {

	    	var selectableDataGrid = ctrls[0];
	        elem.addClass('selectable-data-grid');
	    	       
	        var partitions = selectableDataGrid.partitionRecordItems(scope.recordItems, parseInt(scope.itemsPerRow), scope.startingOffset ? parseInt(scope.startingOffset) : 0);
	        elem.append(selectableDataGrid.renderGrid(partitions, cellFn, scope.header));

	        scope.$watch(function() {
	            if (!(scope.startPos && scope.endPos)) return;
	            return {
	            	topLeftColIndex: Math.min(scope.startPos.colIndex, scope.endPos.colIndex),
	            	topLeftRowIndex: Math.min(scope.startPos.rowIndex, scope.endPos.rowIndex),	
	            	bottomRightColIndex: Math.max(scope.startPos.colIndex, scope.endPos.colIndex),
	            	bottomRightRowIndex: Math.max(scope.startPos.rowIndex, scope.endPos.rowIndex)
	        	};	            
	        }, function(newValue, oldValue) {
	        	if (newValue === oldValue) return;
	            if (newValue) {	              
	                scope.$broadcast('cells.selection.change', newValue);                    	               
	            }
	        }, true);           

	        function cellFn(record) {
	        	var isolatedScope = scope.$new(true);
	        	var returnElem;
	        	isolatedScope.$record = record;
	        	transcludeFn(isolatedScope, function (clone) {
	        		returnElem = clone;
	        	});

	        	return returnElem;
	        };					
	    }
	}

	function selectableDataGridCtrl($scope, $compile) {

		this.partitionRecordItems = partitionRecordItems;
	    this.renderGrid = renderGrid;

	    $scope.isDragging = false;
	    $scope.startPos = null;
	    $scope.endPos = null;
	  
	    $scope.mousedown = function (d, e) {	       
	    	if (!e.ctrlKey) $scope.$broadcast('cells.selection.reset');	     
	        $scope.startPos = { colIndex: d.colIndex, rowIndex: d.rowIndex };
	        $scope.endPos = { colIndex: d.colIndex, rowIndex: d.rowIndex };
	        $scope.isDragging = true;
	    }

	    $scope.mouseup = function () {
	    	$scope.isDragging = false;	    	
	    }

	    $scope.mouseenter = function (d, e) {	    	
	    	if (!$scope.isDragging) return;	   
	    	$scope.endPos = { colIndex: d.colIndex, rowIndex: d.rowIndex };
	    }

		function partitionRecordItems(recordItems, itemsPerRow, firstRowOffset) {
			var firstRowNumber = itemsPerRow - firstRowOffset;
			var partitions = [];
		    var curIndex = 0,
		        rowIndex = 0;


		    partitions.push(takeN(firstRowNumber, rowIndex, firstRowOffset) );

		    while (curIndex < recordItems.length) {
		        rowIndex += 1;
		        partitions.push(takeN(itemsPerRow, rowIndex) );
			}

			return partitions;

			function takeN(n, rowNum, pad) {
				var partition = [];
				var i;
			    var curOffset = 0;
			    if (pad) {
			        for (i = 0; i < pad; i ++) {
			            partition.push({ colIndex: curOffset, rowIndex: rowNum, data: null });
			            curOffset ++;
			        }
			    }
			    for (i = curIndex; i < curIndex + n; i++) {
			    	partition.push({ colIndex: curOffset, rowIndex: rowNum, data: i >= recordItems.length ? null : recordItems[i] });
			        curOffset ++;
			    }
			    curIndex = i;
			    return partition;
			}
		}

		function renderGrid(partitions, cellFn, header) {		  
			var table = angular.element('<table class="wfm-table"></table>');
			
			if (header) {
				var hrow = angular.element('<tr></tr>');
			    angular.forEach(header, function(h) {
			        hrow.append(angular.element('<th>' + h + '</th>'));
			    });
			    table.append(hrow);
			}
			

		    angular.forEach(partitions, function(rowData) {
		    	var row = angular.element('<tr></tr>');		        
		        angular.forEach(rowData, function (cellData) {
		        	var iscope = $scope.$new();
		        	iscope.$data = cellData;
		        	iscope.mousedown = $scope.mousedown;
		        	iscope.mouseup = $scope.mouseup;
		        	iscope.mouseenter = $scope.mouseenter;
							           
		            var cell = $compile(angular.element(
						'<td ' +
						'ng-mousedown="mousedown($data, $event)" ' +
						'ng-mouseup="mouseup($data, $event)"' +
						'ng-mouseenter="mouseenter($data, $event)"' +
						'ng-class="{\'selected\': $data.data && $data.data.isSelected}" >' +
						'</td>'))(iscope);

		            if (cellData.data !== null) {
		                iscope.$on('cells.selection.change', function(_scope, data) {		                  
		                    if (cellData.colIndex >= data.topLeftColIndex
		                        && cellData.colIndex <= data.bottomRightColIndex
		                        && cellData.rowIndex >= data.topLeftRowIndex
		                        && cellData.rowIndex <= data.bottomRightRowIndex)
		                        cellData.data.isSelected = true;
		                });

		                iscope.$on('cells.selection.reset', function() {
		                    cellData.data.isSelected = false;                          
		                });
		            }
		            cell.attr('col-index', cellData.colIndex);
		            cell.attr('row-index', cellData.rowIndex);		        

		            cell.append(cellData.data === null? '': cellFn(cellData.data));
		            row.append(cell);
		        });

		        table.append(row);
		    });
		    return table;
		}


	}



})();