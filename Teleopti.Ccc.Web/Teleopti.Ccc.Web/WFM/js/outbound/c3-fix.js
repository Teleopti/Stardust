(function() {

	var CLASS = {
		target: 'c3-target',
		chart: 'c3-chart',
		chartLine: 'c3-chart-line',
		chartLines: 'c3-chart-lines',
		chartBar: 'c3-chart-bar',
		chartBars: 'c3-chart-bars',
		chartText: 'c3-chart-text',
		chartTexts: 'c3-chart-texts',
		chartArc: 'c3-chart-arc',
		chartArcs: 'c3-chart-arcs',
		chartArcsTitle: 'c3-chart-arcs-title',
		chartArcsBackground: 'c3-chart-arcs-background',
		chartArcsGaugeUnit: 'c3-chart-arcs-gauge-unit',
		chartArcsGaugeMax: 'c3-chart-arcs-gauge-max',
		chartArcsGaugeMin: 'c3-chart-arcs-gauge-min',
		selectedCircle: 'c3-selected-circle',
		selectedCircles: 'c3-selected-circles',
		eventRect: 'c3-event-rect',
		eventRects: 'c3-event-rects',
		eventRectsSingle: 'c3-event-rects-single',
		eventRectsMultiple: 'c3-event-rects-multiple',
		zoomRect: 'c3-zoom-rect',
		brush: 'c3-brush',
		focused: 'c3-focused',
		defocused: 'c3-defocused',
		region: 'c3-region',
		regions: 'c3-regions',
		tooltipContainer: 'c3-tooltip-container',
		tooltip: 'c3-tooltip',
		tooltipName: 'c3-tooltip-name',
		shape: 'c3-shape',
		shapes: 'c3-shapes',
		line: 'c3-line',
		lines: 'c3-lines',
		bar: 'c3-bar',
		bars: 'c3-bars',
		circle: 'c3-circle',
		circles: 'c3-circles',
		arc: 'c3-arc',
		arcs: 'c3-arcs',
		area: 'c3-area',
		areas: 'c3-areas',
		empty: 'c3-empty',
		text: 'c3-text',
		texts: 'c3-texts',
		gaugeValue: 'c3-gauge-value',
		grid: 'c3-grid',
		gridLines: 'c3-grid-lines',
		xgrid: 'c3-xgrid',
		xgrids: 'c3-xgrids',
		xgridLine: 'c3-xgrid-line',
		xgridLines: 'c3-xgrid-lines',
		xgridFocus: 'c3-xgrid-focus',
		ygrid: 'c3-ygrid',
		ygrids: 'c3-ygrids',
		ygridLine: 'c3-ygrid-line',
		ygridLines: 'c3-ygrid-lines',
		axis: 'c3-axis',
		axisX: 'c3-axis-x',
		axisXLabel: 'c3-axis-x-label',
		axisY: 'c3-axis-y',
		axisYLabel: 'c3-axis-y-label',
		axisY2: 'c3-axis-y2',
		axisY2Label: 'c3-axis-y2-label',
		legendBackground: 'c3-legend-background',
		legendItem: 'c3-legend-item',
		legendItemEvent: 'c3-legend-item-event',
		legendItemTile: 'c3-legend-item-tile',
		legendItemHidden: 'c3-legend-item-hidden',
		legendItemFocused: 'c3-legend-item-focused',
		dragarea: 'c3-dragarea',
		EXPANDED: '_expanded_',
		SELECTED: '_selected_',
		INCLUDED: '_included_'
	};
	
	c3.applyFix = function() {
		c3.chart.internal.fn.selectPath = function (target, d) {
			var $$ = this;
			$$.config.data_onselected.call($$, d, target.node());
		};
	}
	c3.restoreFix = function() {
		c3.chart.internal.fn.selectPath = function (target, d) {
			var $$ = this;
			$$.config.data_onselected.call($$, d, target.node());
			target.transition().duration(100).style("fill",
			function () {
				return $$.d3.rgb($$.color(d)).brighter(0.75);
			});
		};
	}

	c3.chart.internal.fn.drag = function (mouse) {
		var $$ = this, config = $$.config, main = $$.main, d3 = $$.d3;
		var sx, sy, mx, my, minX, maxX, minY, maxY;

		if ($$.hasArcType()) { return; }
		if (!config.data_selection_enabled) { return; } // do nothing if not selectable
		if (config.zoom_enabled && !$$.zoom.altDomain) { return; } // skip if zoomable because of conflict drag dehavior
		if (!config.data_selection_multiple) { return; } // skip when single selection because drag is used for multiple selection

		sx = $$.dragStart[0];
		sy = $$.dragStart[1];
		mx = mouse[0];
		my = mouse[1];
		minX = Math.min(sx, mx);
		maxX = Math.max(sx, mx);
		minY = (config.data_selection_grouped) ? $$.margin.top : Math.min(sy, my);
		maxY = (config.data_selection_grouped) ? $$.height : Math.max(sy, my);

		main.select('.' + CLASS.dragarea)
            .attr('x', minX)
            .attr('y', minY)
            .attr('width', maxX - minX)
            .attr('height', maxY - minY);

		// TODO: binary search when multiple xs
		main.selectAll('.' + CLASS.shapes).selectAll('.' + CLASS.shape)
            .filter(function (d) { return config.data_selection_isselectable(d); })
            .each(function (d, i) {
            	var shape = d3.select(this),
                    isSelected = shape.classed(CLASS.SELECTED),
                    isIncluded = shape.classed(CLASS.INCLUDED),
                    _x, _y, _w, _h, toggle, isWithin = false, box;
            	if (shape.classed(CLASS.circle)) {
            		_x = shape.attr("cx") * 1;
            		_y = shape.attr("cy") * 1;
            		toggle = $$.togglePoint;
            		isWithin = minX < _x && _x < maxX && minY <= _y && _y <= maxY;            	
            	}
            	else if (shape.classed(CLASS.bar)) {
            		box = c3.chart.internal.fn.getPathBox(this);
            		_x = box.x;
            		_y = box.y;
            		_w = box.width;
            		_h = box.height;
            		toggle = $$.togglePath;
            		isWithin = !(maxX < _x || _x + _w < minX) && !(maxY < _y || _y + _h < minY);

            	} else {
            		// line/area selection not supported yet								
            		return;
            	}
            	if (isWithin ^ isIncluded) {
            		shape.classed(CLASS.INCLUDED, !isIncluded);
            		// TODO: included/unincluded callback here
            		shape.classed(CLASS.SELECTED, !isSelected);
            		toggle.call($$, !isSelected, shape, d, i);
            	}
            });
	};


})();