
Teleopti.Start.Authentication.SignInView = function (args) {

	var events = new ko.subscribable();
	
	this.Display = function (data) {
		var viewModel = new Teleopti.Start.Authentication.SignInViewModel({
			baseUrl: args.baseUrl,
			events: events,
			authenticationState: args.authenticationState
		});
		data.render(args.html);
		ko.applyBindings(viewModel, data.element[0]);
		viewModel.LoadDataSources();

		_initPlaceHolderText();

	};


	function _initPlaceHolderText() {
		if (!$.support.placeholder) {
			var active = document.activeElement;
			$(':text, :password').focus(function () {
				if ($(this).attr('placeholder') != '' && $(this).val() == $(this).attr('placeholder')) {
					$(this).val('').removeClass('hasPlaceholder');
				}
			}).blur(function () {
				if ($(this).attr('placeholder') != '' && ($(this).val() == '' || $(this).val() == $(this).attr('placeholder'))) {
					$(this).val($(this).attr('placeholder')).addClass('hasPlaceholder');
				}
			});
			$(':text, :password').blur();
			$(active).focus();
			$('form').submit(function () {
				$(this).find('.hasPlaceholder').each(function () { $(this).val(''); });
			});
		}
	}

};
