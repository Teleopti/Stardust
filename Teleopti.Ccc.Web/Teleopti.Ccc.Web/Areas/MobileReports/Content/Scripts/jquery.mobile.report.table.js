
(function ($, undefined) {
	$.widget("mobile.table", $.mobile.widget, {
		options: {
			disabled: true,
			isInit: false,
			chartSeries: 2
		},
		tableData: {

		},
		_eventHandler: function (event, payload) {
			var table = $(this).data('table'),
			    o = table.options;
			if (!event.isPropagationStopped()) {
				switch (payload.method) {
					case 'redraw':
						table._redraw();
						break;
					case 'data':
						table.tableData = payload.data;
						table._redraw();
						break;
				}
			}
		},
		_generateReportTable: function (reportInfo, tableData) {
			var self = this, o = self.options, tab = [];
			tab[tab.length] = '<table class="report-table"><thead><tr><th scope"col">';
			tab[tab.length] = reportInfo.PeriodLegend;
			tab[tab.length] = '</th><th scope="col">';
			tab[tab.length] = reportInfo.Y1Legend;
			if (o.chartSeries > 1) {
				tab[tab.length] = '</th><th scope="col">';
				tab[tab.length] = reportInfo.Y2Legend;
			}
			tab[tab.length] = '</th></tr></thead><tbody>';
			$.each(tableData, function () {
				tab[tab.length] = self._rowBuilder(this);
			});
			tab[tab.length] = "</tbody></table>";
			return tab.join('');
		},
		_rowBuilder: function (curRow) {
			var row = '<tr>';
			$.each(curRow, function () {
				row += '<td>' + this + '</td>';
			});
			row += '</tr>';
			return row;
		},
		_create: function () {

			var self = this,
			    o = $.extend(this.options, this.element.jqmData('options')),
			    caller = this.element;
			var thisPage = caller.closest('.ui-page');
			$.extend(self, {
				thisPage: thisPage,
				caller: caller
			});

			self.options.isInit = true;
			self.element.bind('table', self._eventHandler);
		},
		_redraw: function () {
			console.log('asdfasdf');
			var self = this,
			td = self.tableData;
			self.element.html(self._generateReportTable(td.ReportInfo, td.ReportData));
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
		},
		_prepareData: function (reportData) {
			var reportInfo = reportData.ReportInfo; //, reportData
			var row, self = this, o = self.options, data = [];
			$.each(reportData.ReportData, function (i, periodData) {
				row = [periodData['Period'], Number(periodData['DataColumn1']).toFixed(reportInfo.Y1DecimalsHint)];
				if (o.chartSeries > 1) {
					row.push(Number(periodData['DataColumn2']).toFixed(reportInfo.Y2DecimalsHint));
				}
				data.push(row);
			});
			reportData.ReportData = data;
			return reportData;
		},
		setData: function (data) {
			var self = this, o = self.options;
			o.chartType = data.ReportInfo.ChartTypeHint;
			o.chartSeries = data.ReportInfo.Y2Legend.length === 0 ? 1 : 2;
			self.element.trigger('table', { 'method': 'data', 'data': self._prepareData(data) });
		},
		clean: function () {
			var self = this;
			self.caller.removeData('table');
		},

		refresh: function () {
			var self = this, o = self.options;
			this._showHide();
			if (!o.disabled)
				setTimeout(function () { self.element.trigger('redraw'); }, 10);

		},
		setEnabled: function (value) {
			this.options.disabled = !value;
			this.refresh();
		}
	});
})(jQuery);