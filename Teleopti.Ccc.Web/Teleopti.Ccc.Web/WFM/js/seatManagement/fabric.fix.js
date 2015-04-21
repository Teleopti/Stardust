(function () {

	/*  override of fabric.util.getScrollLeftTop */
	fabric.util.getScrollLeftTop = function (element, upperCanvasEl) {

		var firstFixedAncestor,
			origElement,
			left = 0,
			top = 0,
			docElement = fabric.document.documentElement,
			body = fabric.document.body || {
				scrollLeft: 0, scrollTop: 0
			};

		origElement = element;

		while (element && element.parentNode && !firstFixedAncestor) {

			element = element.parentNode;

			if (element.nodeType === 1 &&
				fabric.util.getElementStyle(element, 'position') === 'fixed') {
				firstFixedAncestor = element;
			}

			if (element.nodeType === 1 &&
				origElement !== upperCanvasEl &&
				fabric.util.getElementStyle(element, 'position') === 'absolute') {
				left = 0;
				top = 0;
			}
			else if (element === fabric.document) {
				// overrride - was not calculating scroll position correctly!
				//left = body.scrollLeft || docElement.scrollLeft || 0;
				//top = body.scrollTop || docElement.scrollTop || 0;

				left = docElement.scrollLeft || 0;
				top = docElement.scrollTop || 0;
			}
			else {
				left += element.scrollLeft || 0;
				top += element.scrollTop || 0;
			}
		}

		return { left: left, top: top };
	}

}());