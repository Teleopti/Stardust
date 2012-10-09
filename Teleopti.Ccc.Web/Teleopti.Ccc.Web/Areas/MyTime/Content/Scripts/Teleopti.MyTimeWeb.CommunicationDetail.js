
Teleopti.MyTimeWeb.CommunicationDetail = (function ($) {

    function _showCommunication(position) {
        _hideEditSection();
        _showEditSection(position);
    }

    function _showEditSection(position) {
        var topPosition = $('#Communications-list').position().top - 1;
        if (!position)
            position = topPosition;
        if (position < topPosition)
            position = topPosition;
        console.log('position: ' + position);
        $('#Message-detail-section')
			.css({
			    'top': position
			})
			.fadeIn()
			;
    }

    function _hideEditSection() {
        $('#Message-detail-section')
			.hide()
			;
    }

    function _fadeEditSection(func) {
        $('#Message-detail-section')
            .fadeOut(400, func)
            ;
    }

    return {
        Init: function () {
        },
        HideEditSection: function () {
            _hideEditSection();
        },
        FadeEditSection: function (func) {
            _fadeEditSection(func);
        },
        ShowCommunication: function (position) {
            _showCommunication(position);
        }
    };

})(jQuery);

