(function ($, undefined) {
	$.widget("mobile.graph", $.mobile.widget, {
		options: {
			disabled: true,
			ratio: 0.5,
			isInit: false,
			IsResizing: false,
			prepareData: null,
			adjustXLabels: null,
			chartSeries: 2,
			chartType: 'stackedline',
			chartTypes: {
				line: [
					null,
					function (id, gd) {
						var g = new RGraph.Line(id, gd.Y1, gd.Y2 || undefined);
						g.Set('chart.shadow', true);
						g.Set('chart.color', ['red', 'green']);
						g.Set('chart.chart.numxticks', 1);
						g.Set('chart.colors', ['green', 'red']);
						return g;
					} ],
				stackedline: [
					null,
					function (id, gd) {
						var g = new RGraph.Line(id, gd.Y1, gd.Y2 || undefined);
						g.Set('chart.filled', true);
						g.Set('chart.filled.accumulative', true);
						g.Set('chart.colors', ['green', 'red']);
						return g;
					} ],
				stackedbar: [
					function (data) {
						var a = [[], []];
						$.each(data.ReportData, function (periodIndex, periodData) {
							a[0].push(periodData['Period']);
							a[1].push([Number(periodData['DataColumn2']), Number(periodData['DataColumn1'])]);
						});
						return {
							Legends: [data.ReportInfo.Y2Legend, data.ReportInfo.Y1Legend],
							Y1: a[1],
							Y2: null,
							X: a[0]
						};
					},
					function (id, gd) {
						var g = new RGraph.Bar(id, gd.Y1);
						g.Set('chart.grouping', 'stacked');
						g.Set('chart.colors', ['red', 'green']);
						return g;
					} ]
			}, chartDrawGraph: function (id, gd) {
				var self = this, o = self.options;
				var graphFn = o.chartTypes[o.chartType][1];
				if (!$.isFunction(graphFn))
					console.log('ChartType: ' + o.chartType + ' not defined!');
				var graph = graphFn(id, gd);
				graph.context.clearRect(0, 0, o.width, o.height);

				var shortSeries = gd.Y1.length < 10;
				graph.Set('chart.hmargin', shortSeries ? 30 : 5);
				graph.Set('chart.labels', o.adjustXLabels(self, gd.X));
				graph.Set('chart.key', gd.Legends);
				graph.Set('chart.text.angle', shortSeries ? 0 : 45);
				graph.Set('chart.gutter.left', 60);
				graph.Set('chart.gutter.right', 5);
				graph.Set('chart.gutter.bottom', shortSeries ? 25 : 65);
				graph.Set('chart.key.position', 'gutter');
				graph.Set('chart.chart.numxticks', 0);
				graph.Set('chart.background.grid.vlines', false);
				graph.Draw();
			}
		},
		graphData: {
			Legends: ['No Data', ''],
			Y1: [0],
			Y2: [0],
			X: ['']
		},
		_eventHandler: function (event, payload) {
			var graph = $(this).data('graph'),
			    o = graph.options;
			if (!event.isPropagationStopped()) {
				switch (payload.method) {
					case 'redraw':
						graph._redraw();
						break;
					case 'data':
						graph.graphData = payload.data;
						break;
				}
			}
		}, // triggered from DOM
		_orientChange: function (e) {
			var self = $(e.currentTarget).data('graph');
			self._resizeArea();
			self.element.trigger('graph', { 'method': 'redraw' });
		},
		_create: function () {
			var self = this,
			    o = $.extend(this.options, this.element.jqmData('options')),
			    caller = this.element;
			if (!$.isFunction(o.prepareData)) {
				o.prepareData = self._prepareData;
			}
			if (!$.isFunction(o.adjustXLabels)) {
				o.adjustXLabels = self._adjustXLabels;
			}

			$(caller).html('<canvas id="report-graph-canvas">[No canvas support]</canvas>');

			var thisPage = caller.closest('.ui-page'),
			    canvas = $("#report-graph-canvas");
			$.extend(self, {
				thisPage: thisPage,
				caller: caller,
				canvas: canvas
			});

			self.options.isInit = true;

			$(window).bind('resize', function (e) {
				setTimeout(function () { caller.trigger('orientationchange'); }, 20);
			});

			caller.bind('orientationchange', self._orientChange);

			self.element.bind('graph', self._eventHandler);
		},
		_adjustXLabels: function (self, labels) {
			var o = self.options,
			    width = o.width - 20,
			    max = width / 20;
			var mod = 1, count = labels.length;
			for (var i = 0; i < 5; i++) {
				mod = Math.pow(2, i);
				if ((count / mod) < max) break;

			}
			console.log(JSON.stringify({ 'count': count, 'mod': mod, 'max': max }));
			var ret = [];
			$.each(labels, function (index) {
				ret[index] = '';
				if ((index % mod) == 0) ret[index] = labels[index];
			});
			console.log(JSON.stringify({ 'labels': ret }));
			return ret;
		},
		_prepareData: function (data) {
			var a = [[], [], []];
			$.each(data.ReportData, function (periodIndex, periodData) {
				a[0].push(periodData['Period']);
				a[1].push(Number(periodData['DataColumn1']));
				a[2].push(Number(periodData['DataColumn2']));

			});
			return {
				Legends: [data.ReportInfo.Y1Legend, data.ReportInfo.Y2Legend],
				Y1: a[1],
				Y2: a[2],
				X: a[0]
			};
		},
		_resizeArea: function () {
			var self = this,
			    o = this.options;
			if (o.disabled)
				return;
			var w = self.thisPage.width() - 20;
			o.width = Math.max(w, 600);
			o.height = o.width * o.ratio;
			self.canvas.attr({ 'width': o.width, 'height': o.height }).css({ 'width': w, 'height': (w * o.ratio) });
		},
		_redraw: function () {
			var self = this,
			    o = this.options,
			    gd = this.graphData,
			    id = self.canvas.attr('id');

			if (o.disabled)
				return;

			var drawGraphFn = o.chartDrawGraph;
			if (!$.isFunction(drawGraphFn))
				console.log('ChartDrawGraph: ' + o.chartDrawGraph + ' not defined!');

			drawGraphFn.call(self, id, gd);


		},
		setData: function (data) {
			var self = this, o = self.options;
			o.chartType = data.ReportInfo.ChartTypeHint;
			o.chartSeries = data.ReportInfo.Y2Legend.length === 0 ? 1 : 2;
			var prepFn = o.chartTypes[o.chartType][0] || self._prepareData;
			var chartData = prepFn.call(self, data);
			if (o.chartSeries < 2) {
				chartData.Legends = [chartData.Legends[0]];
				chartData.Y2 = null;
			}
			self.element.trigger('graph', { 'method': 'data', 'data': chartData });
		},
		clean: function () {
			var self = this;
			self.canvas.remove();
			self.caller.removeData('graph');
		},
		setEnabled: function (value) {
			this.options.disabled = !value;
			this.refresh();
		},
		refresh: function () {
			var self = this, o = self.options;
			this._showHide();
			if (!o.disabled)
				setTimeout(function () { self.element.trigger('orientationchange'); }, 10);
		},
		_init: function () {
			this._showHide();
		},
		_showHide: function () {
			var self = this, o = self.options;
			if (o.disabled) {
				self.element.hide();
			} else {
				self.element.show();
			}
		}
	});
})(jQuery);