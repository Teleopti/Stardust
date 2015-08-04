﻿(function () {
	'use strict';

    angular.module('wfm.outbound')
        .directive('selectableDataGrid', [selectableDataGrid])
        .directive('selectableDataGridCell', ['$parse', selectableDataGridCell]);

    function selectableDataGridCell($parse) {
        return {
            restrict: 'E',
            scope: {
                '$item': '=cellContent',
                '$colIndex': '@colIndex',
                '$rowIndex': '@rowIndex'
            },
            link: postlink
        };

        function postlink(scope, elem, attrs) {

            elem.addClass('selectable-data-grid-cell');

            var unselectable = false;

            attrs.$observe('unselectable', function (fnCode) {              
                if (fnCode) {
                    var unselectablePredicate = $parse(fnCode);
                    scope.$watch(function () {
                        return { item: scope.$item, predicate: unselectablePredicate(scope) };
                    }, function (value) {                      
                        if (!value.item) return;
                        unselectable = value.predicate;
                        if (unselectable) elem.addClass('ng-unselectable');
                        else elem.removeClass('ng-unselectable');
                    }, true);
                }
            });

            scope.$on('cells.selection.change', function (_scope, data) {          
                if (!angular.isDefined(scope.$colIndex) || !angular.isDefined(scope.$rowIndex) || !scope.$item) return;
                if (unselectable) return;

                if (scope.$colIndex >= data.topLeftColIndex
                    && scope.$colIndex <= data.bottomRightColIndex
                    && scope.$rowIndex >= data.topLeftRowIndex
                    && scope.$rowIndex <= data.bottomRightRowIndex)
                    scope.$item.isSelected = true;
            });

            scope.$on('cells.selection.reset', function () {
                if (!angular.isDefined(scope.$colIndex) || !angular.isDefined(scope.$rowIndex) || !scope.$item) return;
                scope.$item.isSelected = false;
            });            
        }
    }

	function selectableDataGrid() {
	    return {
            restrict: 'E',
	    	scope: {
	    		startingOffset: '@?',
	    		itemsPerRow: '@',
				gridHeaders: '=?',
	    		recordItems: '='				
	    	},
			require: ['selectableDataGrid'],
			controller: ['$scope', '$compile', selectableDataGridCtrl],	    
			transclude: true,
	    	link: postlink

	    };

	    function postlink(scope, elem, attrs, ctrls, transcludeFn) {

	    	var gridCtrl = ctrls[0];
	        elem.addClass('selectable-data-grid');
	    	       
	        var partitions = gridCtrl.partitionRecordItems(scope.recordItems, parseInt(scope.itemsPerRow), scope.startingOffset ? parseInt(scope.startingOffset) : 0);
	        elem.append(gridCtrl.renderGrid(partitions, cellFn, scope.gridHeaders));

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
	        	var iscope = scope.$new();
	        	var returnElem;
	        	iscope.$item = record;
	        	transcludeFn(iscope, function (clone) {
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

	    $scope.mouseup = function (d, e) {
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
		            iscope.cellContent = cellData.data;
		        	iscope.mousedown = $scope.mousedown;
		        	iscope.mouseup = $scope.mouseup;
		        	iscope.mouseenter = $scope.mouseenter;

		            var cellContainer = angular.element(
		                '<td ' +
		                'ng-mousedown="mousedown($data, $event)" ' +
		                'ng-mouseup="mouseup($data, $event)"' +
		                'ng-mouseenter="mouseenter($data, $event)"' +
		                'ng-class="{\'selected\': $data.data && $data.data.isSelected}" >' +
		                '</td>');

                    if (cellData.data !== null) {
                        var _cellContent = cellFn(cellData.data);
                        var wrapper = angular.element('<div></div>');
                        wrapper.append(_cellContent);
                        var cellContent = wrapper.find('selectable-data-grid-cell');
                        cellContent.attr('col-index', cellData.colIndex);
                        cellContent.attr('row-index', cellData.rowIndex);
                        cellContent.attr('cell-content', 'cellContent');
                        cellContainer.append(cellContent);
                    }
		      
		            var cell = $compile(cellContainer)(iscope); 		            
		            row.append(cell);
		        });

		        table.append(row);
		    });
		    return table;
		}
	}

})();