$(document).ready(docReady);

function docReady() {
    $form = $("#formMain");
    $popup = $("#popup");
    $employeeNumbers = $("#employeeNumbers");
    $employeeNumbersToCheck = $("input[name=EmployeeNumbersToCheck]");

    // show the employee numbers popup
    $("#checkEmployeeEligibility").on("click", function () {
        var check = false;

        $("#employeeNumbersPopup").dialog({
            width: 650,
            position: { my: "center top", at: "center top+150", of: $("div.page") },
            modal: true,
            draggable: true,
            closeOnEscape: false,
            title: "Check Employee Eligibility",
            resizable: false,
            stack: true,
            dialogClass: "info-dialog qualif-dialog",
            buttons: {
                "Check": function () {
                    check = true;
                    $(this).dialog("close");
                },
                "Close": function () {
                    $(this).dialog("close");
                }
            },
            open: function () {
                $employeeNumbers.focus().val($employeeNumbersToCheck.val());
            },
            close: function () {
                $(this).dialog("destroy");

                if (check === true) {
                    BlockUI();

                    $employeeNumbersToCheck.val($employeeNumbers.val().replace(/(;|\r\n|\n|\r)/gm, ","));
                    /*
                    replace method removes all three types of line breaks by using this bit of a
                    regular expression: \r\n|\n|\r

                    This tells it to replace all instance of the \r\n then replace all \n than finally replace all \r.
                    It goes through and removes all types of line breaks from the designated text string.

                    The "gm" at the end of the regex statement signifies that the replacement should take place
                    over many lines (m) and that it should happen more than once (g).

                    The "g" in the javascript replace code stands for "greedy" which means the replacement should happen
                    more than once if possible. If for some reason you only wanted the first found occurrence in the
                    string to be replaced then you would not use the "g".

                    http://www.textfixer.com/tutorials/javascript-line-breaks.php
                    */
                    $form.submit();
                }
            }
        });
    });

    // remove employee
    $("#employees table.data").on("click", "a[data-delete=1]", function () {
        BlockUI();

        var tr = $(this).closest("tr.data-row");
        var employeeNumber = tr.find("input[name=employee_number]").val();

        var str = $employeeNumbersToCheck.val();
        $employeeNumbersToCheck.val(str.replace(employeeNumber, ""));

        $form.submit();
    });

    // show Employee Qualification dialog
    $("#employees table.data").on("click", "a[data-view-qualif=1]", function () {
        var tr = $(this).closest("tr.data-row");

        var bscEmployeeNumber = tr.find("input[name=bsc_employee_number]").val();
        var bscPositionNumber = $("#Position_BscPositionNumber").val();

        var $placeHolder = $popup.find("[data-role=placeholder]");
        $placeHolder.empty().append(ProgressElement());

        $popup.dialog({
            width: 700,
            position: { my: "center top", at: "center top+150", of: $("div.page") },
            modal: true,
            draggable: true,
            closeOnEscape: true,
            title: "Employee Qualifications",
            resizable: false,
            stack: true,
            dialogClass: "info-dialog qualif-dialog",
            buttons: {
                "Close": function () {
                    $(this).dialog("close");
                }
            },
            open: function () {
                tr.addClass("selected-row");

                $.get(buildUrl("/Home/GetEmployeeQualificationsForPosition"),
                    {
                        bscEmployeeNumber: bscEmployeeNumber,
                        bscPositionNumber: bscPositionNumber
                    }).done(function (html) {
                        $placeHolder.html(html);
                    }).fail(function () {
                        // close the dialog & show error message
                        $popup.dialog("close");
                        showDialog(Msg.ERROR_OCCURRED_ON_SERVER, "Error", { messageIcon: "warning" });
                    });
            },
            close: function () {
                // remove selected row css
                tr.removeClass("selected-row");

                $(this).dialog("destroy");
            }
        });
    });
}
