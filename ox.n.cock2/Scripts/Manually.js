
var indexNumber = 1;
$(document).ready(function () {

    $("#IsDiscountAllowed:checkbox").change(function () {
        if (this.checked) {
            $('#DivDiscountAllowed').show();
        }
        else {
            $('#DivDiscountAllowed').hide();
        }
    });

    $("#IsAddingSecondFlavor:checkbox").change(function () {
        if (this.checked) {
            $('#DivAddingSecondFlavor').show();
        }
        else {
            $('#DivAddingSecondFlavor').hide();
        }
    });

    $("#NoOfItems").bind('keyup mouseup', function () {
        updateProductTotal();
    });
    $("#Discount").bind('keyup mouseup', function () {
        updateProductTotal();
    });
    $('#IsDiscountAllowed:checkbox').change(function () {
        updateProductTotal();
    });

    $("#btnAddOrderItem").click(function () {

        var productCategory = $('#ProductCategoryId :selected');
        var productSubCategory = $('#ProductSubCategory :selected');
        var productSubCategorySecondFlavor = $('#ProductSubCategorySecondFlavor :selected');

        var productSubCategorySecondFlavor_val = '';
        var productSubCategorySecondFlavor_txt = '';

        if ($("#IsAddingSecondFlavor").is(':checked') && productSubCategorySecondFlavor.val() != null && productSubCategorySecondFlavor.val() != '')
        {
            productSubCategorySecondFlavor_val = productSubCategorySecondFlavor.val();
            productSubCategorySecondFlavor_txt = ', ' + productSubCategorySecondFlavor.text();
        }

        var productClassification = $('#ProductClassification :selected');
        var discount = 0;

        if (productCategory.val() == null || productCategory.val() == '') {
            $('#ProductCategory-warning').show();
            return;
        }
        $('#ProductCategory-warning').hide();

        if (productSubCategory.val() == null || productSubCategory.val() == '') {
            $('#ProductSubCategory-warning').show();
            return;
        }
        $('#ProductSubCategory-warning').hide();

        if (productClassification.val() == null || productClassification.val() == '') {
            $('#ProductClassification-warning').show();
            return;
        }
        $('#ProductClassification-warning').hide();

        if ($('#IsDiscountAllowed').is(":checked")) {
            discount = $("#Discount").val();
        }

        $('#noItemAdded').hide();
        $("#tblOrderList").append(
            '<tr class="rowOrderList" id="rowOrderList_' + indexNumber + '">' +
            '<td><input type="hidden" class="productCategory" value="' + productCategory.val() + '">' +
            '<div>' + productCategory.text() + '</div></td>' +

            '<td><input type="hidden" class="productSubCategory" value="' + productSubCategory.val() + '">' +
            '<input type="hidden" class="productSubCategorySecondFlavor" value="' + productSubCategorySecondFlavor_val + '">' +
            '<div>' + productSubCategory.text() + productSubCategorySecondFlavor_txt + '</div></td>' +

            '<td><input type="hidden" class="productClassification" value="' + productClassification.val() + '">' +
            '<div>' + productClassification.text() + '</div></div>' +

            '<td class="price">' + $('#Price').val() + '</td>' +
            '<td class="itemCount">' + $('#NoOfItems').val() + '</td>' +
            '<td class="discount">' + discount + '</td>' +
            '<td class="itemTotalPrice">' + $('#itemTotalPrice').val() + '</td>' +
            '<td><button class="btn btn-danger" onclick="deleteRow(' + indexNumber + ')">Delete row</button>' +
            '</td></tr>');

        getGrandTotal();
        resetAddOrderFields();
        indexNumber++;
    });
});

$("#ProductCategoryId").change(function () {
    var subCategoryDD = '<option value="">- - Please Select - -</option>';
    var productClassificationsDD = '<option value="">- - Please Select - -</option>';
    var selectedCategoryId = $(this).find(":selected").val();

    $('#ProductSubCategory').html(subCategoryDD);
    $('#ProductClassification').html(productClassificationsDD);

    if (selectedCategoryId != null && selectedCategoryId != '') {
        $.ajax({
            url: '/Home/GetProductCategoryDropdown',
            type: 'Get',
            data: { categoryId: selectedCategoryId },
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (result) {
                if (result.success == false) {
                    $('#alertModal').find('.modal-body').text("Product Category record Not found.");
                    $('#alertModal').modal('show');
                }
                else {
                    var subCategories = result.subCategories;
                    if (subCategories.length > 0) {
                        $.each(subCategories, function (index, item) {
                            subCategoryDD += '<option value="' + item.Value + '">' + item.Text + '</option>';
                        });
                        $('#ProductSubCategory').html(subCategoryDD);
                        $('#ProductSubCategorySecondFlavor').html(subCategoryDD);
                    }

                    var productClassifications = result.productClassifications;
                    if (productClassifications.length > 0) {
                        $.each(productClassifications, function (index, item) {
                            productClassificationsDD += '<option value="' + item.Value + '">' + item.Text + '</option>';
                        });
                        $('#ProductClassification').html(productClassificationsDD);
                    }
                }
            },
            error: function (xhr, status, err) {
                alert('Error: ' + err);
            }
        });
    }
});

$(".subCategory-classification").change(function () {
    let productSubCategoryValue = $('#ProductSubCategory :selected').val();
    $('#ProductSubCategorySecondFlavor option').show();
    if (productSubCategoryValue != '')
    {
        $('#ProductSubCategorySecondFlavor').find('option[value="' + productSubCategoryValue + '"').hide();
    }

    if (productSubCategoryValue != '' && $('#ProductClassification :selected').val() != '') {
        $.ajax({
            url: "/Home/GetProductDetail",
            type: 'Get',
            data: {
                subCategoryId: $('#ProductSubCategory').find(":selected").val(),
                classificationId: $('#ProductClassification').find(":selected").val(),
            },
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (result) {
                if (result.success == false) {
                    $('#alertModal').find('.modal-body').text("Product record Not found. Please add it first.");
                    $('#alertModal').modal('show');
                }
                else {
                    $('#Price').val(result.Price);
                    $('#Discount').val(result.DiscountAllowed);
                    updateProductTotal();
                }
            },
            error: function (xhr, status, err) {
                alert('Error: ' + err);
            }
        });
        return false;
    }
});

function placeOrder() {

    if ($('#tblOrderList > .rowOrderList').length > 0) {
        let orderItems = [];

        $('#tblOrderList > .rowOrderList').each(function () {
            let productSubCategoryIds = [];

            let productCategoryId = parseFloat($(this).find('.productCategory').first().val());
            let productSubCategoryId = parseFloat($(this).find('.productSubCategory').first().val());
            let productClassificationId = parseFloat($(this).find('.productClassification').first().val());
            let itemCount = parseFloat($(this).find('.itemCount').first().text());
            let discount = parseFloat($(this).find('.discount').first().text());

            let productSubCategoryIdSecondFlavor = parseFloat($(this).find('.productSubCategorySecondFlavor').first().val());

            productSubCategoryIds.push(productSubCategoryId);
            if (productSubCategoryIdSecondFlavor > 0)
                productSubCategoryIds.push(productSubCategoryIdSecondFlavor);

            orderItems.push({
                ProductCategoryId: productCategoryId,
                ProductSubCategoryIds: productSubCategoryIds,
                ProductClassificationId: productClassificationId,
                ItemCount: itemCount,
                Discount: discount
            });
        });

        let order = {
            CustomerName: $("#CustomerName").val(),
            OrderInstructions: $("#OrderInstructions").val(),
            OrderItems: orderItems
        };

        $.ajax({
            url: "/Home/PlaceOrderManually",
            type: 'Post',
            data: JSON.stringify(order),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            success: function (result) {
               
                if (result.success == true || result.success == 'true')
                {
                    $('#NetTotalItems').val(0);
                    $('#NetTotal').val(0);
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
                else
                {
                    $('#alertModal').find('.modal-body').text("Order not added. Please Try again.");
                    $('#alertModal').modal('show');
                }
            },
            error: function (xhr, status, err) {
                alert('Error: ' + err);
            }
        });
    }
}

function getGrandTotal() {
    let grandTotalItems = 0;
    let grandTotal = 0;

    if ($('#tblOrderList > .rowOrderList').length > 0) {
        $('#tblOrderList > .rowOrderList').each(function () {
            grandTotalItems += parseFloat($(this).find('.itemCount').first().text());
            grandTotal += parseFloat($(this).find('.itemTotalPrice').first().text());
        });
    }

    $('#NetTotalItems').text(grandTotalItems);
    $('#NetTotal').text(grandTotal);
}

function resetAddOrderFields() {
    $('#ProductCategoryId').val('');
    $('#ProductSubCategory').html('<option value="">- - Please Select - -</option>');
    $('#ProductClassification').html('<option value="">- - Please Select - -</option>');
    $('#Price').val('');
    $('#NoOfItems').val(1);
    $('#IsDiscountAllowed').prop('checked', false);
    $("#DivDiscountAllowed").hide();
    $('#Discount').val(0);
    $('#IsAddingSecondFlavor').prop('checked', false);
    $('#itemTotalPrice').val(0);
    $('#ProductSubCategorySecondFlavor').html('<option value="">- - Please Select - -</option>');
}

function updateProductTotal() {
    let noOfItems = $("#NoOfItems").val();
    var pricePerItem = Number($("#Price").val());
    var basicPrice = noOfItems * pricePerItem;

    if ($('#IsDiscountAllowed').is(":checked")) {
        basicPrice = basicPrice - $("#Discount").val()
    }
    $("#itemTotalPrice").val(basicPrice);
}

function deleteRow(indexToDelete) {
    $("#rowOrderList_" + indexToDelete).remove();
    if ($('#tblOrderList > .rowOrderList').length == 0) {
        $('#noItemAdded').show();
    }
    getGrandTotal();
}

function funExportToPDF() {
    $.ajax({
        url: "/Home/ExportToPDF",
        type: 'GET',
        contentType: 'application/json; charset=utf-8',
        data: {id: 11},
        success: function (result) {
            if (result === '0') {
                alert("Order Not found");
            }
            else if (result === '1') {
                alert("Order Uploaded, But not Found");
            }
            else if (result === '404') {
                alert("Internel Server Error");
            }
            else {
                const linkSource = `data:application/pdf;base64,${result}`;
                const downloadLink = document.createElement("a");
                const fileName = "Order.pdf";
                downloadLink.href = linkSource;
                downloadLink.download = fileName;
                downloadLink.click();
            }
        },
        error: function (xhr, status, err) {
            alert(err);
        }
    });
    return false;
}
