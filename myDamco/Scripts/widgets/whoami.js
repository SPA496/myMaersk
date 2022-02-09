$(function () {

    // Create a jquery plugin for doing setup of the widget news
    $.fn.widget_whoami = function () {
        return this.each(function () {
            var $this = $(this);
            var baseconf = $this.data("baseconf");
            var serviceUrl = $this.data("serviceurl");

            function Render(data) {
                var container = $(".templateProfileRow", $this).parent();
                var keys = ["FirstName", "LastName", "AddressLine1", "AddressLine2", "Country", "Phone", "Email", "Organization"];
                var keyLabels = ["First Name", "Last Name", "Address", "", "Country", "Phone", "E-mail", "Organization"];
                for (i = 0; i < keys.length; i++) {
                    var key = keys[i];
                    var label = keyLabels[i];
                    var value = data[key];
                    var row = $(".templateProfileRow", $this).clone();

                    row.attr("class", "profileRow")
                            .children(".label").html(label).attr("class", "label transparent")
                            .parent()
                            .children(".value").html(value);

                    if (label!="" || value!="") container.append(row.show());
                }
                $(".loadingspinner", $this).hide();
            }


            // Load and render profile from service
            var loadProfile = function () {
                $(".loadingspinner", $this).show();
                $.getJSON(serviceUrl, Render);
            }

            cache = $this.data("fromcache");
            if (cache != "") {
                Render(cache);
            }
            else
                loadProfile();
        });
    };

    // Setup widget for all instances by running the plugin
    $(".widget-whoami").widget_whoami();
});