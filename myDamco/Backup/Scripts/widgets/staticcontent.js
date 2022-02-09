$(function () {

    // Create a jquery plugin for doing setup of the widget news
    $.fn.widget_rssstaticcontent = function () {
        return this.each(function () {
            var $this = $(this);

            var baseconf = $this.data("baseconf");
            var instanceconf = $this.data("instanceconf");
            var content = $(".widget-content", $this);


            content.html(baseconf.content);

            $(".widget-head a.refresh", $this).hide();

        });
    };

    // Setup News widgets for all damconews instances
    $(".widget-staticcontent").widget_rssstaticcontent();
});