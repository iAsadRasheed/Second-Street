var indexNumber = 1;
var grossTotalPrice = 0;

$(document).ready(function () {
    $(".btn-menu_item").click(function () {
        var strProductCategoryId = $(this).attr('id');
        var productCategoryId = Number(strProductCategoryId);

        if (productCategoryId != null) {
            $.ajax({
                url: '/Home/GetProductById',
                type: 'Get',
                data: { Id: productCategoryId },
                dataType: 'html',
                contentType: 'application/json; charset=utf-8',
                success: function (result) {

                    $('#model-Body').html(result);
                    $('#productsByCategoryModal').modal('show');
                },
                error: function (xhr, status, err) {
                    alert('Error: ' + err);
                }
            });
        }
    });

    $('#tblOrderList').on('change keyup', 'input[name="noOfItems"]', function () {
        updateProductTotal($(this).closest('tr.rowOrderList'));
    });

    $('#tblOrderList').on('change keyup', 'input[name="discountAllowed"]', function () {
        updateProductTotal($(this).closest('tr.rowOrderList'));
    });

    $('#tblOrderList').on('click', 'input[name="isDiscountAllowed"]', function () {

        var currentTableRow = $(this).closest('tr.rowOrderList');
        var discountInputElement = currentTableRow.find('input[name="discountAllowed"]').first();

        if (this.checked) {
            discountInputElement.css('visibility', 'visible');
            let defaultDiscount = currentTableRow.find('input[name="defaultDiscount"]').val();
            currentTableRow.find('input[name="discountAllowed"]').val(defaultDiscount);
        }
        else { discountInputElement.css('visibility', 'hidden'); }

        updateProductTotal(currentTableRow);
    });

    $('#isGrossDiscountAllowed').click(function () {

        if (this.checked) {
            $('#GrossDiscountAllowed').val(0).css('visibility', 'visible');
            $('#NetTotal').text(grossTotalPrice - $('#GrossDiscountAllowed').val());
        }
        else {
            $('#GrossDiscountAllowed').css('visibility', 'hidden');
            $('#NetTotal').text(grossTotalPrice - 0);
        }
    });

    $("#deliveryType .btn").click(function () {
        $("#deliveryType .btn").removeClass('btn-success');
        $(this).addClass('btn-success');
    });

    $('#GrossDiscountAllowed').bind('change keyup', function () {
        $('#NetTotal').text(grossTotalPrice - $(this).val());
    });
});

function getDetail(id) {
    $.ajax({
        url: '/Home/GetProductDetailsById',
        type: 'Get',
        data: { Id: id },
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: function (result) {

            $('#productsByCategoryModal').modal('hide');
            let productDetail = result.productDetail;
            let numberOfItems = $("#numberOfItems").val();

            if (numberOfItems == null || numberOfItems == '')
                numberOfItems = 1;

            let classificationName = productDetail.classificationName != null ? productDetail.classificationName : '';

            $("#tblOrderList").append('<tr class="rowOrderList" id="rowOrderList_' + indexNumber + '">' +
                '<td><input type="hidden" class="productId" value="' + productDetail.productId + '">' +
                '<input name="defaultDiscount" type="number" style="display:none" value="' + productDetail.DiscountAllowed + '"/>' +
                '<div>' + productDetail.categoryName + '</div></td>' +

                '<td>' + productDetail.subCategoryName + '</td>' +
                '<td>' + classificationName + '</td>' +
                '<td class="price">' + productDetail.Price + '</td>' +
                '<td><input required name="noOfItems" type="number" value="' + numberOfItems + '" min="0" onkeyup="if(!value>0) value=0;" class="form-control" style="width:75px;padding:4px"></td>' +
                '<td><input name="isDiscountAllowed" type="checkbox" />' +
                '<input name="discountAllowed" type="number" class="form-control" style="margin-left:4px; width:75px; visibility:hidden" value="' + productDetail.DiscountAllowed + '"/>' +
                '<td class="itemTotalPrice">' + (productDetail.Price * numberOfItems) + '</td>' +
                '<td><button class="btn btn-danger" onclick="deleteRow(' + indexNumber + ')">Delete row</button>' +
                '</td></tr>');

            $("#numberOfItems").val(1);
            $('#noItemAdded').hide();
            getGrossTotal();
            indexNumber++;
        },
        error: function (xhr, status, err) {
            alert('Error: ' + err);
        }
    });
}

function deleteRow(indexToDelete) {
    $("#rowOrderList_" + indexToDelete).remove();
    if ($('#tblOrderList > .rowOrderList').length == 0) {
        $('#noItemAdded').show();
    }
    getGrossTotal();
}

function getGrossTotal() {
    let grossTotalItems = 0;

    grossTotalPrice = 0;

    if ($('#tblOrderList > .rowOrderList').length > 0) {
        $('#tblOrderList > .rowOrderList').each(function () {
            grossTotalItems += parseFloat($(this).find('[name="noOfItems"]').first().val());
            grossTotalPrice += parseFloat($(this).find('.itemTotalPrice').first().text());
        });
    }

    $('#GrossTotalItems').text(grossTotalItems);
    $('#GrossTotal').text(grossTotalPrice);
    $('#NetTotal').text(grossTotalPrice - $('#GrossDiscountAllowed').val());
}

function updateProductTotal(currentTableRow) {
    var inputNoOfItem = currentTableRow.find('input[name="noOfItems"]').first();
    var price = currentTableRow.find('.price').first().text();
    var totalPrice = Number(inputNoOfItem.val()) * Number(price);

    if (currentTableRow.find('input[name="isDiscountAllowed"]').first().is(':checked')) {
        totalPrice = totalPrice - Number(currentTableRow.find('input[name="discountAllowed"]').first().val());
    }
    currentTableRow.find('.itemTotalPrice').first().text(totalPrice);
    getGrossTotal();
}


function placeOrder() {
    if ($('#tblOrderList > .rowOrderList').length > 0) {
        let orderItems = [];

        $('#tblOrderList > .rowOrderList').each(function () {

            let productId = parseFloat($(this).find('.productId').first().val());
            let noOfItems = parseFloat($(this).find('[name="noOfItems"]').first().val());
            let discount = parseFloat($(this).find('[name="discountAllowed"]').first().val());

            orderItems.push({
                ProductId: productId,
                ItemCount: noOfItems,
                Discount: discount
            });
        });

        let order = {
            CustomerName: $("#CustomerName").val(),
            OrderInstructions: $("#OrderInstructions").val(),
            Products: orderItems,
            Discount: $("#GrossDiscountAllowed").val(),
        };

        $.ajax({
            url: "/Home/PlaceNewOrder",
            type: 'Post',
            data: JSON.stringify(order),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (result) {

                if (result.success == true || result.success == 'true') {

                    grossTotalPrice = 0;

                    $('#GrossTotalItems').val(0);
                    $('#GrossTotal').text(grossTotalPrice);
                    $('#NetTotal').text(0);
                    $('#noItemAdded').show();
                    $("#tblOrderList").html('');

                    let byteCharacters = atob(result.base64String);
                    let byteNumbers = new Array(byteCharacters.length);
                    for (let i = 0; i < byteCharacters.length; i++) {
                        byteNumbers[i] = byteCharacters.charCodeAt(i);
                    }
                    let byteArray = new Uint8Array(byteNumbers);
                    let file = new Blob([byteArray], { type: 'application/pdf;base64' });
                    let fileURL = URL.createObjectURL(file);
                    window.open(fileURL);
                }
                else {
                    alert('Some thing went wrong. ');

                    $('#PaidStatusConfirmationModal').find('.modal-body').text("Order not added. Please Try again.");
                    $('#PaidStatusConfirmationModal').modal('show');
                }
            },
            error: function (xhr, status, err) {
                alert('Error: ' + err);
            }
        });
    }
}