
Teleopti.Start.Authentication.SignInViewModel = function (data) {
	var self = this;

	this.DataSources = ko.observableArray();
	this.ErrorMessage = ko.observable();
	this.HasErrorMessage = ko.computed(function () {
		var errorMessage = self.ErrorMessage();
		return errorMessage && errorMessage.length > 0;
	});

	this.Ajax = new Teleopti.Start.Authentication.JQueryAjaxViewModel();
	
	this.HasDataSources = ko.computed(function() {
		return self.DataSources().length > 0;
	});

	this.LoadDataSources = function () {
		$.ajax({
			url: data.baseUrl + "Start/AuthenticationApi/DataSources",
			dataType: "json",
			type: 'GET',
			cache: false,
			success: function (data, textStatus, jqXHR) {
				self.DataSources([]);

				var map = ko.utils.arrayMap(data, function (d) {
					return new Teleopti.Start.Authentication.DataSourceViewModel(d.Name, d.Type);
				});
				self.DataSources.push.apply(self.DataSources, map);
				var dataSources = self.DataSources();
				if (dataSources.length == 0) {
					self.ErrorMessage($('#Signin-error').data('nodatasourcetext'));
					return;
				}

				if (dataSources.length == 1) {
					self.SignIn(dataSources[0]);
				}
			},
			error: function (jqXHR, textStatus, errorThrown) {
				try {
					var json = JSON.parse(jqXHR.responseText);
					$('#Exception-message').text(json.Message);
					$('#Exception-div').show();
				}
				catch (e) {
					$('#body-inner').html('<h2>Error: ' + jqXHR.status + '</h2>');
					$('#dialog-modal-header').text(errorThrown);
					$('#dialog-modal-body').html(jqXHR.responseText);
					$('#dialog-modal').modal('show');
				}
				
			}
		});
	};

	this.SignIn = function (selectedDataSource) {
		var state = data.authenticationState;

		self.ErrorMessage('');

		state.TryToSignIn({
			data: {
				isWindows: selectedDataSource.IsWindows,
				type: selectedDataSource.Type,
				datasource: selectedDataSource.Name
			},
			errormessage: function (message) {
				self.ErrorMessage(message);
			},
			nobusinessunit: function () {
				self.ErrorMessage($('#Signin-error').data('nobusinessunitext'));
			}
		});

	};

};
