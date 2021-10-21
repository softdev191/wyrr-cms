$(function () {

    LoadData();
});

function LoadData() {
    var flagbVisible = true;

    $('#grid').dataTable({

        "bDestroy": true,
        "bProcessing": false,
        "sAjaxSource": "GetMasterUsers",
        "aoColumns": [
                { "mDataProp": "PublicKey" },
                { "mDataProp": "SecretKey" },
                


        {
            "mData": null,
            "bSearchable": false,
            "bSortable": false,
            "sClass": "text-center",
            "mRender": function (data, type, full) {

                var linkButtons = '';

                linkButtons += '<a class="btn btn-info btn-sm" onclick="return OpenPopup(this);"  href="../StellarMasterAccount/ViewStellarUser/' + full['ID'] + '">View</a> ';

                linkButtons += '<a class="btn btn-info btn-sm" onclick="return OpenPopup(this);"  href="../StellarMasterAccount/ManageStellarUser/' + full['ID'] + '">Edit</a> ';
               
                return linkButtons;
            }, "bVisible": flagbVisible
        }
        ],
        "fnRowCallback": function (nRow, aData, iDisplayIndex, iDisplayIndexFull) {
            if (aData.IsDeleted) {
                $(nRow).css('color', '#adadad');
            }
        }
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

function UpdateSuccessful(data) {
    //    if (data.indexOf("field-validation-error") > -1) return;

    if (data.success) {
        $('[data-popup="popup"]').fadeOut(0);
        LoadData();
        toastr.success('Saved Successfully');

    }
}


function ClosePopup() {
    $('[data-popup="popup"]').fadeOut(350);
    return false;
}

