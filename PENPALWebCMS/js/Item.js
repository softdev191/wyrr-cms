$(function () {
    LoadView();
});

function LoadView() {
    $.get('GetSearchCriteria', function (data) {
        if (data) {
            $('#partialOrderView').html(data);

        }
        else {
            toastr.error('Error occurred');
        }

    });
}

function LoadSearchResult(data) {
    $('#categorySearchResult').show();

    $('#searchProduct').dataTable({
        "bDestroy": true,
        "bProcessing": true,
        "aaData": data.aaData,

        "aoColumns": [

                        {
                            "mDataProp": "ItemName"
                        },
                        {
                            "mDataProp": "Brand"
                        },
                    {
                        "mDataProp": "Description"
                    }





        ]
    });
}

//Get Category Wise Data
function GetData(value) {
   
    $.ajax({
        url: $('#formCategory').attr('action'),
        type: $('#formCategory').attr('method'),
        data: $('#formCategory').serialize(),
        success: function (data) {
            LoadSearchResult(data);
        }
    });
}

