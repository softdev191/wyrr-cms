
//-------------------------------------------------------------------------------------------------------------------------------------------------
//----- Handle Global Ajax Requests --------------------------
//-------------------------------------------------------------------------------------------------------------------------------------------------


var AjaxGlobalHandler = {
    Initiate: function (options) {
        $.ajaxSetup({ cache: false });

        // Ajax events fire in following order
        $(document).ajaxStart(function () {
            $.blockUI({
                message: options.AjaxWait.AjaxWaitMessage,
                css: options.AjaxWait.AjaxWaitMessageCss
            });
        }).ajaxSend(function (e, xhr, opts) {
        }).ajaxError(function (e, xhr, opts) {
            if (options.SessionOut.StatusCode == xhr.status) {
                $('#logout_popup').modal('show');
                startTimer();
                return;
            }
            //toastr.error(options.AjaxErrorMessage);            
        }).ajaxSuccess(function (e, xhr, opts) {
        }).ajaxComplete(function (e, xhr, opts) {
        }).ajaxStop(function () {
            $.unblockUI();
        });
    }
};

var pathArray = window.location.pathname.split('/');
var secondLevelLocation = pathArray[1];
//alert(secondLevelLocation);

var waitimageUrl =rootURL;  
//alert(rootURL);

var sessionoutRedirect = 'Login/Login';

$(document).ready(function () {
    var options = {
        AjaxWait: {
            AjaxWaitMessage: "<img src='"+ waitimageUrl + "' />",
            AjaxWaitMessageCss: { width: "100px", left: "45%" }
        },
        AjaxErrorMessage: "Error occured while processing you request !!!",
        SessionOut: {
            StatusCode: 590,
            RedirectUrl: sessionoutRedirect
        }
    };

    AjaxGlobalHandler.Initiate(options);    
});


// Below code to display session expired message
var c = 0; max_count = 10; logout = true;

function startTimer() {
    setTimeout(function () {
        logout = true;
        c = 0;
        max_count = 10;
        $('#timer').html(max_count);        
        startCount();

    }, 3000);
}

function resetTimer() {
    logout = false;
    $('#logout_popup').modal('hide');
    startTimer();
}

function timedCount() {
    c = c + 1;
    remaining_time = max_count - c;
    if (remaining_time == 0 && logout) {
        $('#logout_popup').modal('hide');
        location.href = $('#lnkLogin').attr("href");

    } else {
        $('#timer').html(remaining_time);
        t = setTimeout(function () { timedCount() }, 1000);
    }
}

function startCount() {
    timedCount();
}