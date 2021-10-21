$(function () {
    LoadData();
});


function LoadData() {
    var flagbVisible = true;

    $('#grid').dataTable({

        "bDestroy": true,
        "bProcessing": true,
        "sAjaxSource": "GetUserTransactionSettings",
        "aoColumns": [
                { "mDataProp": "UserName" },
                { "mDataProp": "TransactionPerDay" },
                { "mDataProp": "TransactionAmountPerDay" },



        {
            "mData": null,
            "bSearchable": false,
            "bSortable": false,
            "sClass": "text-center",
            "mRender": function (data, type, full) {

                var linkButtons = '';

                linkButtons += '<a class="btn btn-info btn-sm" onclick="return OpenPopup(this);"  href="../UserTransactionSetting/ViewSetting/' + full['Id'] + '">View</a> ';

                linkButtons += '<a class="btn btn-info btn-sm" onclick="return OpenPopup(this);"  href="../UserTransactionSetting/ManageTransaction/' + full['Id'] + '">Edit</a> ';


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


