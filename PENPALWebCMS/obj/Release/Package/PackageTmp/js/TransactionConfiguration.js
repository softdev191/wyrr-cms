$(function () {
    LoadData();
});


function LoadData() {
    var flagbVisible = true;

    $('#grid').dataTable({

        "bDestroy": true,
        "bProcessing": false,
        "sAjaxSource": "GetTransactionSettings",
        "aoColumns": [
                //{ "mDataProp": "TransactionPerDay" },

                { "mDataProp": "ServiceTaxPerTransaction" },
                 { "mDataProp": "MinimumAmountForTransaction" },
                 { "mDataProp": "PayPalMargin" },
                 { "mDataProp": "MinimumAmountForPayPalTransaction" },
                 { "mDataProp": "CoinbaseMargin" },
                 { "mDataProp": "EnableAddPayPal" },
                 { "mDataProp": "EnableWithdrawPayPal" },
                 { "mDataProp": "EnableAddCoinbase" },
                 { "mDataProp": "CoinbaseSendLimit" },

        {
            "mData": null,
            "bSearchable": false,
            "bSortable": false,
            "sClass": "text-center",
            "mRender": function (data, type, full) {

                var linkButtons = '';

                linkButtons += '<a class="btn btn-info btn-sm" onclick="return OpenPopup(this);"  href="../TransactionConfiguration/ViewSetting/' + full['Id'] + '">View</a> ';

                linkButtons += '<a class="btn btn-info btn-sm" onclick="return OpenPopup(this);"  href="../TransactionConfiguration/ManageTransaction/' + full['Id'] + '">Edit</a> ';


                return linkButtons;
            }, "bVisible": flagbVisible
        }
        ],

    });
}

function OpenPopup(obj) {

    var url = obj.getAttribute("href");

    $.get(url, function (data) {
        if (data) {
            $('#partialContent').html(data);
            $('[data-popup="popup"]').fadeIn(350);
        }
        else {
            toastr.error('Error occurred');
        }

    });
    return false;
}


function ClosePopup() {
    $('[data-popup="popup"]').fadeOut(350);
    return false;
}

function UpdateSuccessful(data) {
    //    if (data.indexOf("field-validation-error") > -1) return;

    if (data.success) {
        $('[data-popup="popup"]').fadeOut(0);
        LoadData();
        toastr.success('Saved Successfully');

    }
}


function ValidateData() {
    var inputTransactionPerDayVal = $('#TransactionPerDay').val();
    var inputTransactionPerMonthVal = $('#TransactionPerMonth').val();
    if (inputTransactionPerDayVal != "") {
        if (inputTransactionPerMonthVal != "") {

            if (inputTransactionPerDayVal > inputTransactionPerMonthVal) {
                toastr.error('Transaction per day should be less than Transaction per month');
                return false;
            }

        }
    }
}

function Client_Change()
{
    debugger;

    var LumenChargeRangeID = $('#drpRange option:selected').val();

    var serachCriteria = JSON.stringify({ rangeId: LumenChargeRangeID });

    $.ajax({
        type: "POST",
        url: "../TransactionConfiguration/GetChargesFromRange",
        data: serachCriteria,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,
        success: function (response) {

            if (response.success) {

                $('#txtCharges').val(response.fee);
            }
            else {
                toastr.error('No order records to download');
            }

        },
        failure: function (response) {
            alert(response.d);
        }
    });


}


function setCompleteStatus(checkboxElem) {

    if (checkboxElem.checked) {

        $("#txtCharges").prop("readonly", true);
        $("#drpRange").prop("disabled", true);

        $("#txtServiceTax").prop("readonly", false);
    }
    else {
        $("#txtServiceTax").prop("readonly", true);
        $("#txtCharges").prop("readonly", false);
        $("#drpRange").prop("disabled", false);

    }


}

//function LoadView() {
//    $.get('GetSearchCriteria', function (data) {
//        if (data) {
//            $('#partialOrderView').html(data);
//            $(function () {

//            });
//        }
//        else {
//            toastr.error('Error occurred');
//        }

//    });
//}

//$( function() {
//  var spinner = $( "#spinner" ).spinner();

//  $( "#disable" ).on( "click", function() {
//    if ( spinner.spinner( "option", "disabled" ) ) {
//      spinner.spinner( "enable" );
//    } else {
//      spinner.spinner( "disable" );
//    }
//  });
//  $( "#destroy" ).on( "click", function() {
//    if ( spinner.spinner( "instance" ) ) {
//      spinner.spinner( "destroy" );
//    } else {
//      spinner.spinner();
//    }
//  });
//  $( "#getvalue" ).on( "click", function() {
//    alert( spinner.spinner( "value" ) );
//  });
//  $( "#setvalue" ).on( "click", function() {
//    spinner.spinner( "value", 5 );
//  });

//  $( "button" ).button();
//} );
