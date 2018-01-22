function setupQualificationAutoComplete() {
    // for styling purpose the autocomplete dropdown will be appended to a container
    var containerSelector = "div.qualif-autocomplete-container";
    // removes existing element if any
    $(containerSelector).remove();
    $("<div class='qualif-autocomplete-container'></div>").appendTo("body");

    $("input:text[data-qualif-autocomplete=1][readonly!=readonly]")
        .autocomplete({
            appendTo: containerSelector,
            minLength: 1,
            source:
                function (request, response) {
                    var json = [];

                    $.get(buildUrl("/Qualification/GetQualificationsForAutoComplete"),
                        {
                            searchText: request.term
                        })
                        .done(function (data) {
                            json = data;
                        })
                        .fail(function (e) {
                            json = [];
                        })
                        .always(function (d) {
                            try {
                                response($.map(json, function (item) {
                                    return {
                                        label: item.qualificationName,
                                        // we will need these in the _renderItem method
                                        qualificationId: item.qualificationId,
                                        qualificationName: item.qualificationName
                                    };
                                }));
                            } catch (e) {
                                // You must always call the response callback even if you encounter an error. This ensures that the widget always has the correct state.
                                response([]);
                            }
                        });
                }
        })
        // iterate and attach a custom rendering logic
        .each(function () {
            // get the autocomplete widget instance from the 'data' property and define _renderItem
            $(this).data("ui-autocomplete")._renderItem = function (ul, item) {
                return $("<li>")
                    .append("<a>"
                    + "<table style='width:100%'>"
                    + "<tbody>"
                    + "<tr>"
                    + "<td style='width: 80%'>" + item.qualificationName + "</td>"
                    + "<td style='width: 20%'>" + item.qualificationId + "</td>"
                    + "</tr>"
                    + "</tbody>"
                    + "</table>"
                    + "</a>")
                  .appendTo(ul);
            };
        });
}