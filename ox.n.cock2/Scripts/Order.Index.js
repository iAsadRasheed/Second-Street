$(document).ready(function () {

    //$('.btn_PaidUnpaid').on('click', function () {
    //    let txt = $(this).text();

    //    if (txt == 'Paid') {
    //        $(this).text('Un-Paid');
    //    }
    //    else {
    //        $(this).text('Paid');
    //    }

    //    $("#PaidStatusConfirmationModal").modal('show');
    //});
});

function updateOrderStatus(id) {

    $.ajax({
        url: "/Order/ToggleOrderStatus",
        type: 'Post',
        data: { strOrderId: id },
        dataType: 'text',
        success: function (result) {
            if (result == 'True') {
                let btnPaidUnpaid = $('#tblOrders').find('#' + id + ' ' + '.btnPaidUnpaid').first();
                if (btnPaidUnpaid.text().trim().toLowerCase() == 'paid') {
                    btnPaidUnpaid.text('Un Paid');
                }
                else {
                    btnPaidUnpaid.text('Paid');
                }
            }
            else {
                alert('Some thing went wrong');
            }
        },
        error: function (xhr, status, err) {
            alert('Error: ' + err);
        }
    });
}

function updatePaidUnpaidBtnText()
{
    let btnPaidUnpaid = $('#tblOrders').find('#' + id + ' ' + '.btnPaidUnpaid').first();
    if (btnPaidUnpaid.text().trim().toLowerCase() == 'paid') {
        btnPaidUnpaid.text('Un Paid');
    }
    else {
        btnPaidUnpaid.text('Paid');
    }
}