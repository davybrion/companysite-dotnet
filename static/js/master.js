(function(window,document,$,undefined){
	
	$.localScroll({duration: 200});

	$nav = $('#primary-nav');
	
	$(window).bind('ready scroll', function () {				
		handleScroll();
	});

	var handleScroll = function () {	
		if ($(window).width() > 940 && $(window).scrollLeft() === 0) {
			var docHeight = docHeight || $(document).height(),
				footerLocation = footerLocation || docHeight - 335,
				scrollTop = $(window).scrollTop() + 15 + $nav.height() + 245;
			
			if($(window).scrollTop() > 220) {
				$nav.addClass('fixed');
			} else {
				$nav.removeClass('fixed');
			}
			
			if(scrollTop > footerLocation) {
				$nav.css({
					position: 'absolute',
					top: footerLocation - 600
				})
			} else {
				$nav.removeAttr('style');
			}
		} else {
			$nav.removeClass('fixed');
		}
	}
})(window, document, jQuery);	
