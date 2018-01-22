/// <reference path="sitetimeout.js" />
$(document).on("ready.sitemaster", masterDocReady);



var sessionCounterHandle, autoLogoutHandle, tabFlashHandle, autoLogoutAlertIsOpen, timeoutHandle;
var continueSession = false;
var secondsRemaining = 0;
var $autoLogoutAlert;
//var oldTitle;
//var newTitle = "!!!!!!!!!!EHOS TimeOut";

var timeoutSecRemain = 0;

// frequency of the auto logout popup
// this must be less than session time out AND timeout in the <forms/> element
// var AUTO_LOGOUT_PROMPT_INTERVAL_SECONDS = 60;
// count down AFTER the auto log out popup is displayed. The user has 30 seconds to click "Stay Logged In"
// var AUTO_LOGOUT_PROMPT_CLOSE_SECONDS = 30;

function masterDocReady() {
    
    // apply mask and attach datepickers
    $("input.masked-input-date")
        .not("[readonly='readonly']")
        .mask("99/99/9999", { placeholder: "_" })
        .datepicker({
            buttonImage: "../static/images/calendar.png",
            buttonImageOnly: true,
            showOn: "both",
            showButtonPanel: true,
            changeMonth: true,
            changeYear: true
        });

    // apply mask to time input and restrict character input to numbers
    $("input.masked-input-time")
        .not("[readonly=readonly]")
        .mask("99:99").keypress(function (e) {
            if (IsSpecialKey(e.keyCode)) {
                return;
            }

            var validChars = "0123456789"
            var c = String.fromCharCode(e.keyCode);
            if (c && validChars.indexOf(c) === -1) {
                e.preventDefault();
            }
        });

    //oldTitle = document.title;

    $autoLogoutAlert = $("#autoLogoutAlert");

    var autoLogoutEnabled = $("#txtAutoLogoutEnabled").val() == "1";
    //alert($autoLogoutAlert + " ---- " + autoLogoutEnabled);

    //// start the auto logout timer
    if (autoLogoutEnabled) {
      
        beginAutoLogoutPromptCounter();

    }

    //timeoutSecRemain = $("#txtLoginTimeoutMinutes").val() * 60;
    //if user continue to login after being informed he/she will be log out soon, start counter
    //if (timeoutSecRemain != 0) {
    //    beginLoginTimeoutCounter();
    //}

    //// restart autologout timeout counter, if a keypress or mouse click is detected
    $(document).on("keydown.autologout keypress.autologout click.autologout", function () {
        if (autoLogoutEnabled) {
            beginAutoLogoutPromptCounter();
            console.info("auto log out timer reset");
        }
    });

    //// reset session timeout counter
    $("#btnStayLoggedIn").click(function () {
        continueSession = true;
        $autoLogoutAlert.dialog("close");
    });

}

//function beginLoginTimeoutCounter() {
//    //timeoutSecRemain = $("#txtLoginTimeOutMinutes").val() * 60;
//    if (timeoutSecRemain != 0) {
//        clearInterval(timeoutHandle);
//        timeoutHandle = setInterval(loginTimeoutCounter, 1000);
//    }
   
//}

function beginAutoLogoutPromptCounter() {
    secondsRemaining = $("#txtAutoLogoutIntervalSeconds").val();//AUTO_LOGOUT_PROMPT_INTERVAL_SECONDS;
    if (secondsRemaining !== 0) {
        clearInterval(sessionCounterHandle);
        sessionCounterHandle = setInterval(updateSessionTimeoutCounter, 1000);
    }
}


//function loginTimeoutCounter() {
//    if (timeoutSecRemain >= 0) {
//        timeoutSecRemain--;
//    }
//    else {
//        clearInterval(timeoutHandle);
//        // logout the user by navigating to the 'Log Out' url
//        window.location = getLogoutUrl(1);
//    }
//}

function updateSessionTimeoutCounter() {
    
    if (secondsRemaining >= 0) {
        var min = parseInt(secondsRemaining / 60);
        var sec = secondsRemaining - (min * 60);
        secondsRemaining--;
    }
    else {
        clearInterval(sessionCounterHandle);

        // the dialog is already being displayed
        if (autoLogoutAlertIsOpen) {
            return;
        }

        continueSession = false;

        var autoLogoutTimeoutSeconds = $("#txtAutoLogoutTimeoutSeconds").val()

        $("#autoLogoutSeconds").html(autoLogoutTimeoutSeconds).data("timeout", autoLogoutTimeoutSeconds);
       
        // show auto logout alert
        $autoLogoutAlert.dialog({
            //width: 400,
            width: 250,
            //height: 120,
            height: 150,
            modal: true,
            draggable: true,
            //closeOnEscape: true,
            closeOnEscape: false,
            title: "Warning",
            resizable: false,
            stack: true,
            dialogClass: "session-timeout warning-dialog",
            open: function (event, ui) {
                autoLogoutAlertIsOpen = true;

                // hide the [x] icon
                //$("div.session-timeout").find("a.ui-dialog-titlebar-close").css("display", "none");
                $(".ui-dialog-titlebar-close").hide();
                //focus on the button
                $("#btnStayLoggedIn").focus();
            },
            close: function (event, ui) {
                $(this).dialog("destroy");

                if (continueSession === true) {
                    // clear timer handle
                    clearTimeout(autoLogoutHandle);

                    //document.title = oldTitle;

                    // restart countdown for prompt
                    beginAutoLogoutPromptCounter();
                }
                autoLogoutAlertIsOpen = false;
            }
        });
        

        // auto log out if 'Continue' in the dialog box is not clicked
        // within 30 seconds
        autoLogoutHandle = setInterval(function () {
            var secondsRemaining = parseInt($("#autoLogoutSeconds").data("timeout"));
            if (secondsRemaining > 0) {
                $("#autoLogoutSeconds").html(secondsRemaining--).data("timeout", secondsRemaining);
                //if (secondsRemaining % 2 == 0) {
                //    document.title = oldTitle;                  
                //}
                //else if (secondsRemaining % 2 == 1) {
                //    document.title = newTitle;                    
                //}
            }
            else {
                clearInterval(autoLogoutHandle);

                // logout the user by navigating to the 'Log Out' url
                window.location = getLogoutUrl(1);
                
                              
            }
        }, 1000);
        
    }
}

function getLogoutUrl(logoutReason) {
    return buildUrl("Account/LogOut");//$("#header_loginView_hypLogout").attr("href") + "?logoutReason=" + logoutReason;
}
