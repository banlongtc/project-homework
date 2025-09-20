"use-strict";

document.addEventListener("DOMContentLoaded", function () {
    const tableDivLine = document.getElementById("tableLineProcessing");
    const userID = document.getElementById('userIdLogin').value;
    const userDisplayName = document.getElementById('userDisplayName').value;

    // Script cho user thao tác
    if (userID != '') {
        //Đọc mã vị trí làm việc
        if (!localStorage.getItem('modalCheckPosition')) {
            $('#showCheckPosition').modal('show');
        } else {
            $('.content-action-process').removeClass('d-none');
        }

        $('#showCheckPosition').on('shown.bs.modal', function (e) {
            $('#qrValuePosition').val('');
            $('#qrValuePosition').focus();
        });

        $('#showCheckPosition .btn-close').on('click', function (e) {
            $('#showCheckPosition').modal('hide');
            $('#btnReadPositionAgian').html('<button class="btn btn-success btn-md">Đọc mã kiểm tra vị trí</button>');
            $('#btnReadPositionAgian button').on('click', function (e) {
                $('#showCheckPosition').modal('show');
            });
        });

        $('#qrValuePosition').on('keypress', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                $('.btn-save-position').trigger('click');
            }
        });

        $('.btn-save-position').on('click', function (e) {
            e.preventDefault();
            let valPosition = $('#qrValuePosition').val();
            if (valPosition != "") {
                fetch(`${window.baseUrl}api/checkandsaveposition`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        positionWorking: valPosition,
                    })
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.status == 1) {
                            localStorage.setItem('modalCheckPosition', true);
                            if (data.positionWorking.includes("KTT")) {
                                window.location.href = $('.btn-pre-operation').attr('href');
                            }
                            if (data.positionWorking.includes("GCDM")) {
                                window.location.href = $('.btn-gcdm').attr('href');
                            }
                        } else {
                            alert("Không có mã vị trí đó trong hệ thống.")
                            window.location.reload();
                        }
                        $('#showCheckPosition').modal('hide');

                    })
                    .catch(error => {
                        console.log(error)
                    })
            }
        });

        const positionWorking = $('#positionWorkingCurrent').val();
        if (positionWorking.includes("KTT")) {
            $('.btn-pre-operation').removeClass('disabled');
            CheckWorkOrder("", positionWorking);
            ReadWorkOrder();
            PreOperation(positionWorking, userID, userDisplayName);
        } else if (positionWorking.includes("GCDM")) {
            $('.btn-gcdm').removeClass('disabled');
            let detailPositionWorking = positionWorking.split('-');
            $('#detailPosition').html(`<div class="detail-position-item">Gia công đầu mút Line ${detailPositionWorking[0]}, vị trí số ${detailPositionWorking[2]}, máy gia công số ${detailPositionWorking[3]}</div>`);
            $('#showFrequencyConditions').modal('show');
            CheckConditions('#showFrequencyConditions');
            //ReadQRTrayCreated();
            processGWProduction(positionWorking);
        }
    }

    //Chia line sản xuất
    DivLineForProduction(tableDivLine);
});

// Script cho chia line sản xuất
function DivLineForProduction(tableDivLine) {
    if (tableDivLine) {
        var q = new Date();
        var dateCurrent = new Date(q.getFullYear(), q.getMonth(), q.getDate());
        var timestampCurrent = dateCurrent.getTime();

        // Lưu dữ liệu line cho đóng gói từ lắp ráp
        $('.btn-save-assembly').on('click', function (e) {
            e.preventDefault();
            let arrData = [];
            let headers = [];
            let checkArray = true;
            $('.table-line-process thead th[data-col_title]').not(".d-none").each(function (index, item) {
                if ($(item).attr("data-col_title") !== undefined) {
                    headers[index] = $(item).attr("data-col_title");
                }
            });
            $(".table-line-process tbody tr.is-checked").each(function (i, elem) {
                let qtyUsed = parseInt($(elem).find("span.qty-used").text(), 10);
                let itemChecked = {};
                let totalQtyLine = 0;
                $(elem).find("input[type='number']").each(function (v, inpNum) {
                    if ($(inpNum).val() != "") {
                        totalQtyLine += parseInt($(inpNum).val(), 10);
                        checkArray = true;
                    } else {
                        checkArray = false;
                        $(inpNum).val("0");
                        $(inpNum).addClass("border-danger");
                        $(e.target).addClass("disabled");
                        $(".table-line-process .message .error-total").html("Số lượng chia đang trống. Vui lòng nhập.");
                    }
                    if (qtyUsed < totalQtyLine || qtyUsed > totalQtyLine) {
                        checkArray = false;
                        $(elem).find("input[type='number']").addClass("border-danger");
                        $(".table-line-process .message .error-total").html("Số lượng đang chia không khớp với số lượng dự định sản xuất.");
                    } else {
                        checkArray = true;
                        $(elem).find("input[type='number']").removeClass("border-danger");
                        $(".table-line-process .message .error-total").html("");
                    }
                });
                $('td', $(this)).not(".d-none").each(function (index, item) {
                    itemChecked[headers[index]] = $(item).find("input.input-line").val();
                    if (headers[index] == "note") {
                        itemChecked[headers[index]] = $(item).find("textarea.input-line").val();
                    }
                });
                if (checkArray) {
                    arrData.push(itemChecked);
                }
            });
            let processCode = $(".processcode").val();
            if (checkArray) {
                fetch(`${window.baseUrl}Assembly/SaveData`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json; charset=utf-8'
                    },
                    body: JSON.stringify({
                        jsonStrDivLine: JSON.stringify(arrData),
                        processCode: processCode
                    })
                })
                    .then(async response => {
                        if (!response.ok) {
                            const errorResponse = await response.json();
                            throw new Error(`${response.status} - ${errorResponse.message}`);
                        }
                        return response.json();
                    })
                    .then(data => {
                        alert(data.message);
                        location.reload();
                    })
                    .catch(error => {
                        alert(error);
                    })
            }
        });

        //Hiển thị chia line cho từng lot NVL
        $('#showDivLine').on('hidden.bs.modal', function () {
            $(this).find('.data-render').html('');
            $('#showDivLine .modal-body p.text-danger').remove();
        })
        $('.show-div-line').on('click', function (e) {
            e.preventDefault();

            let parentElem = $(e.target).parent().parent();
            let totalProductLine1 = parseInt(parentElem.find('input.line-1').val(), 10);
            let totalProductLine2 = parseInt(parentElem.find('input.line-2').val(), 10);
            let totalProductLine3 = parseInt(parentElem.find('input.line-3').val(), 10);
            let totalProductLine4 = parseInt(parentElem.find('input.line-4').val(), 10);

            let productCode = parentElem.attr('data-product_code');
            let lotProduct = parentElem.attr('data-lotno');

            $('.btn-save-line-lot').attr('data-workorder', parentElem.data('workorder'));
            $('#showDivLine #tableDivLine .data-render').html('');
            let processCode = parentElem.find("input.processcode").val();

            fetch(`${window.baseUrl}api/getreserveditem`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    workOrder: parentElem.data('workorder'),
                    processCode: processCode,
                })
            })
                .then(async response => {
                    if (!response.ok) {
                        const errorResponse = await response.json();
                        throw new Error(`${response.status} - ${errorResponse.message}`);
                    }
                    return response.json();
                })
                .then(data => {
                    $('#showDivLine .modal-body .title-select').remove();
                    $('#tableShowOld .data-render').html('');
                    let dataLot = data.dataLot;
                    let oldData = data.oldData;
                    let count = 0;
                    let htmlSelectMaterial = '';
                    if (processCode == "01060" || processCode == "01070" || processCode == "01075") {
                        $('#tableDivLine').addClass('d-none');
                        $('#oldContentsDivLine').addClass('d-none');
                        htmlSelectMaterial += `<div class="title-select mb-3">`;

                        if (dataLot.length > 0) {
                            let mergedData = Object.values(
                                dataLot.reduce((acc, item) => {
                                    let { productCode, lotNo, qty } = item;
                                    if (!acc[productCode]) {
                                        acc[productCode] = { productCode };
                                    }
                                    return acc;
                                }, {})
                            );
                            htmlSelectMaterial += `<span>Chọn mã NVL:</span>
                            <select class="form-select-xl" id="getMaterialCode">
                                <option value="">--Chọn mã NVL--</option>`;
                            mergedData.forEach(item => {
                                htmlSelectMaterial += `<option value="${item.productCode}">${item.productCode}</option>`;
                            });
                            htmlSelectMaterial += `</select>`;
                        } else {
                            htmlSelectMaterial += `<span>Chưa có NVL được Reserved trên MES</span>`;
                        }
                        htmlSelectMaterial += `</div>`;
                        $('#showDivLine #tableDivLine').parent().before(htmlSelectMaterial);
                        $('#getMaterialCode').on('change', function (e) {
                            e.preventDefault();
                            let htmlRender = '';
                            $('#tableShowOld .data-render').html('');
                            if ($(this).val() != '') {
                                $('#tableDivLine').removeClass('d-none');
                                $('#tableDivLine').parent().removeClass('d-none');
                                $('#oldContentsDivLine').removeClass('d-none');
                                dataLot.forEach(item => {
                                    if (item.productCode == $(this).val()) {
                                        let classDisabled = ["", "", "", ""];
                                        let totalProductLines = [totalProductLine1, totalProductLine2, totalProductLine3, totalProductLine4];

                                        for (let i = 0; i < totalProductLines.length; i++) {
                                            if (totalProductLines[i] <= 0) {
                                                classDisabled[i] = "disabled";
                                            }
                                        }
                                        htmlRender += `<tr class="list-item" 
                                        data-item="${productCode}" 
                                        data-lotitem="${lotProduct}" 
                                        data-product_code="${item.productCode}" 
                                        data-lot_product="${item.lotNo}" 
                                        data-qty-base="${item.qty}">
                                            <td class="align-middle text-center"><div class="item-product">${item.productCode}</div></td>
                                            <td class="align-middle text-center"><div>${item.lotNo}</div></td>
                                            <td class="align-middle text-center"><divclass="qty-base">${item.qty}</div></td>
                                            <td class="align-middle text-center" style="width: 130px;">
                                                <div class="input-group">
                                                    <input type="number" name="lineLot1" id="valueDivLine1_${count}" data-element-total="btn-total-line-1" data-line="1" class="text-center enter-value-line-1 ${classDisabled[0]}" />
                                                </div>
                                            </td>                                
                                            <td class="align-middle text-center" style="width: 130px;">
                                                <div class="input-group">
                                                    <input type="number" name="lineLot2" id="valueDivLine2_${count}" data-element-total="btn-total-line-2" data-line="2" class="text-center enter-value-line-2 ${classDisabled[1]}" />
                                                </div>
                                            </td>                                
                                            <td class="align-middle text-center" style="width: 130px;">
                                                <div class="input-group">
                                                    <input type="number" name="lineLot3" id="valueDivLine3_${count}" data-element-total="btn-total-line-3" data-line="3" class="text-center enter-value-line-3 ${classDisabled[2]}" />
                                                </div>
                                            </td>                                
                                            <td class="align-middle text-center" style="width: 130px;">
                                                <div class="input-group">
                                                    <input type="number" name="lineLot4" id="valueDivLine4_${count}" data-element-total="btn-total-line-4" data-line="4" class="text-center enter-value-line-4 ${classDisabled[3]}" />
                                                </div>
                                            </td>
                                            <td class="align-middle text-center d-none">
                                                <input type="checkbox" class="check-data-line"/>
                                            </td>
                                        </tr>`;
                                        count++;
                                    }
                                });
                                $('#showDivLine #tableDivLine .data-render').html(htmlRender);
                                $('#showDivLine #tableDivLine tr.list-item td').each(function (i, elem) {
                                    $(elem).on('change', 'input[type="number"]', function (e) {
                                        $(e.target).parent().parent().parent().find('input.check-data-line').trigger('change');
                                        let classBtn = $(e.target).data('element-total');
                                        $('.' + classBtn).trigger('click');
                                    });
                                });
                                if (oldData.length) {
                                    $('#tableShowOld').removeClass('d-none');
                                    let totalLine1 = 0;
                                    let totalLine2 = 0;
                                    let totalLine3 = 0;
                                    let totalLine4 = 0;
                                    oldData.forEach(item => {
                                        if ($(this).val() == item.productCode) {
                                            totalLine1 += item.line1;
                                            totalLine2 += item.line2;
                                            totalLine3 += item.line3;
                                            totalLine4 += item.line4;
                                            $('#tableShowOld .data-render').append(`<tr class="list-item" data-product_code="${item.productCode}" data-lot_product="${item.lotDivLine}">
                                                <td class="align-middle text-center"><div class="item-product">${item.productCode}</div></td>
                                                <td class="align-middle text-center"><div>${item.lotDivLine}</div></td>
                                                <td class="align-middle text-center">
                                                    <div class="total-line">
                                                        <span id="totalLine1">${item.line1}</span>
                                                    </div>
                                                </td>                                
                                                <td class="align-middle text-center">
                                                    <div class="total-line">
                                                        <span id="totalLine1">${item.line2}</span>             
                                                    </div>
                                                </td>                                
                                                <td class="align-middle text-center">
                                                    <div class="total-line">
                                                        <span id="totalLine1">${item.line3}</span>             
                                                    </div>
                                                </td>                                
                                                <td class="align-middle text-center">
                                                    <div cclass="total-line">
                                                        <span id="totalLine1">${item.line4}</span>             
                                                    </div>
                                                </td>
                                                <td class="align-middle text-center d-none">
                                                    <input type="checkbox" class="check-data-line"/>
                                                </td>
                                            </tr>`);

                                            let hasDivOrderLine1 = $(`.table-line-process #contentTable tr[data-workorder="${item.workOrder}"] .line-1`).val();
                                            let hasDivOrderLine2 = $(`.table-line-process #contentTable tr[data-workorder="${item.workOrder}"] .line-2`).val();
                                            let hasDivOrderLine3 = $(`.table-line-process #contentTable tr[data-workorder="${item.workOrder}"] .line-3`).val();
                                            let hasDivOrderLine4 = $(`.table-line-process #contentTable tr[data-workorder="${item.workOrder}"] .line-4`).val();
                                            if (totalLine1 == parseInt(hasDivOrderLine1, 10)) {
                                                $('.enter-value-line-1').addClass('disabled');
                                            }
                                            if (totalLine2 == parseInt(hasDivOrderLine2, 10)) {
                                                $('.enter-value-line-2').addClass('disabled');
                                            }
                                            if (totalLine3 == parseInt(hasDivOrderLine3, 10)) {
                                                $('.enter-value-line-3').addClass('disabled');
                                            }
                                            if (totalLine4 == parseInt(hasDivOrderLine4, 10)) {
                                                $('.enter-value-line-4').addClass('disabled');
                                            }
                                        }

                                    });
                                    $('#tableShowOld .data-render').append(`
                                    <tr>
                                        <td colspan="2" class="align-middle text-center">Tổng</td>
                                        <td class="align-middle text-center"><div class="total-line-1">${totalLine1}<button type="button" class="d-none btn-total-line-1"></button></div></td>
                                        <td class="align-middle text-center"><div class="total-line-2">${totalLine2}<button type="button" class="d-none btn-total-line-2"></button></div></td>
                                        <td class="align-middle text-center"><div class="total-line-3">${totalLine3}<button type="button" class="d-none btn-total-line-3"></button></div></td>
                                        <td class="align-middle text-center"><div class="total-line-4">${totalLine4}<button type="button" class="d-none btn-total-line-4"></button></div></td>
                                    </tr>`);
                                } else {
                                    $('#tableShowOld').addClass('d-none');
                                }
                            }
                        });
                    } else {
                        if (dataLot.length > 0) {
                            dataLot.forEach(item => {
                                let classDisabled = ["", "", "", ""];
                                let totalProductLines = [totalProductLine1, totalProductLine2, totalProductLine3, totalProductLine4];

                                for (let i = 0; i < totalProductLines.length; i++) {
                                    if (totalProductLines[i] <= 0) {
                                        classDisabled[i] = "disabled";
                                    }
                                }
                                $('#showDivLine #tableDivLine .data-render').append(`<tr class="list-item" 
                                data-item="${productCode}" 
                                data-lotitem="${lotProduct}" 
                                data-product_code="${item.productCode}" 
                                data-lot_product="${item.lotNo}" 
                                data-qty-base="${item.qty}">
                                    <td class="align-middle text-center"><div class="item-product">${item.productCode}</div></td>
                                    <td class="align-middle text-center"><div>${item.lotNo}</div></td>
                                    <td class="align-middle text-center"><div class="qty-base">${item.qty}</div></td>
                                    <td class="align-middle text-center" style="width: 130px;">
                                        <div class="input-group">
                                            <input type="number" name="lineLot1" id="valueDivLine1_${count}" data-line="1" data-element-total="btn-total-line-1" class="text-center enter-value-line-1 ${classDisabled[0]}" />
                                        </div>
                                    </td>                                
                                    <td class="align-middle text-center" style="width: 130px;">
                                        <div class="input-group">
                                            <input type="number" name="lineLot2" id="valueDivLine2_${count}" data-line="2" data-element-total="btn-total-line-2" class="text-center enter-value-line-2 ${classDisabled[1]}" />
                                        </div>
                                    </td>                                
                                    <td class="align-middle text-center" style="width: 130px;">
                                        <div class="input-group">
                                            <input type="number" name="lineLot3" id="valueDivLine3_${count}" data-line="3" data-element-total="btn-total-line-3" class="text-center enter-value-line-3 ${classDisabled[2]}" />
                                        </div>
                                    </td>                                
                                    <td class="align-middle text-center" style="width: 130px;">
                                        <div class="input-group">
                                            <input type="number" name="lineLot4" id="valueDivLine4_${count}" data-line="4" data-element-total="btn-total-line-4" class="text-center enter-value-line-4 ${classDisabled[3]}" />
                                        </div>
                                    </td>
                                    <td class="align-middle text-center d-none">
                                        <input type="checkbox" class="check-data-line"/>
                                    </td>
                                </tr>`);
                                count++;
                            });
                        } else {
                            $('#showDivLine #tableDivLine .data-render').html('<tr><td class="align-middle text-center" colspan="7">Không có dữ liệu</td></tr>')
                        }
                        if (oldData.length) {
                            $('#tableShowOld').removeClass('d-none');
                            let totalLine1 = 0;
                            let totalLine2 = 0;
                            let totalLine3 = 0;
                            let totalLine4 = 0;
                            oldData.forEach(item => {
                                totalLine1 += item.line1;
                                totalLine2 += item.line2;
                                totalLine3 += item.line3;
                                totalLine4 += item.line4;
                                let qtyBase = item.line1 + item.line2 + item.line3 + item.line4;
                                $('#tableShowOld .data-render').append(`<tr class="list-item" data-product_code="${item.productCode}" data-lot_product="${item.lotDivLine}">
                                    <td class="align-middle text-center"><div class="item-product">${item.productCode}</div></td>
                                    <td class="align-middle text-center"><div>${item.lotDivLine}</div></td>
                                    <td class="align-middle text-center">
                                        <div class="total-line">
                                            <span id="totalLine1">${item.line1}</span>
                                        </div>
                                    </td>                                
                                    <td class="align-middle text-center">
                                        <div class="total-line">
                                            <span id="totalLine1">${item.line2}</span>             
                                        </div>
                                    </td>                                
                                    <td class="align-middle text-center">
                                        <div class="total-line">
                                            <span id="totalLine1">${item.line3}</span>             
                                        </div>
                                    </td>                                
                                    <td class="align-middle text-center">
                                        <div cclass="total-line">
                                            <span id="totalLine1">${item.line4}</span>             
                                        </div>
                                    </td>
                                    <td class="align-middle text-center d-none">
                                        <input type="checkbox" class="check-data-line"/>
                                    </td>
                                </tr>`);

                                let hasDivOrderLine1 = $(`.table-line-process #contentTable tr[data-workorder="${item.workOrder}"] .line-1`).val();
                                let hasDivOrderLine2 = $(`.table-line-process #contentTable tr[data-workorder="${item.workOrder}"] .line-2`).val();
                                let hasDivOrderLine3 = $(`.table-line-process #contentTable tr[data-workorder="${item.workOrder}"] .line-3`).val();
                                let hasDivOrderLine4 = $(`.table-line-process #contentTable tr[data-workorder="${item.workOrder}"] .line-4`).val();
                                $(`#tableDivLine tr.list-item[data-product_code="${item.productCode}"][data-lot_product="${item.lotDivLine}"][data-qty-base="${qtyBase}"]`).addClass('d-none');
                                if (totalLine1 == parseInt(hasDivOrderLine1, 10)) {
                                    $('.enter-value-line-1').addClass('disabled');
                                }
                                if (totalLine2 == parseInt(hasDivOrderLine2, 10)) {
                                    $('.enter-value-line-2').addClass('disabled');
                                }
                                if (totalLine3 == parseInt(hasDivOrderLine3, 10)) {
                                    $('.enter-value-line-3').addClass('disabled');
                                }
                                if (totalLine4 == parseInt(hasDivOrderLine4, 10)) {
                                    $('.enter-value-line-4').addClass('disabled');                                }
                            });
                            $('#tableShowOld .data-render').append(`
                            <tr>
                                <td colspan="2" class="align-middle text-center">Tổng</td>
                                <td class="align-middle text-center"><div class="total-line-1">${totalLine1}<button type="button" class="d-none btn-total-line-1"></button></div></td>
                                <td class="align-middle text-center"><div class="total-line-2">${totalLine2}<button type="button" class="d-none btn-total-line-2"></button></div></td>
                                <td class="align-middle text-center"><div class="total-line-3">${totalLine3}<button type="button" class="d-none btn-total-line-3"></button></div></td>
                                <td class="align-middle text-center"><div class="total-line-4">${totalLine4}<button type="button" class="d-none btn-total-line-4"></button></div></td>
                            </tr>`);
                        } else {
                            $('#tableShowOld').addClass('d-none');
                        }
                    }

                    $('#showDivLine #tableDivLine tr.list-item td').each(function (i, elem) {
                        $(elem).on('change', 'input[type="number"]', function (e) {
                            $(e.target).parent().parent().parent().find('input.check-data-line').trigger('change');
                            let classBtn = $(e.target).data('element-total');
                            $('.' + classBtn).trigger('click');
                        });
                    });

                    let check = true;
                    $('body').on('change', '.check-data-line', function (e) {
                        e.preventDefault();
                        $('#showDivLine .modal-body p.text-danger').remove();
                        let totalRow = 0;
                        let $parent = $(e.target).parent().parent();
                        $parent.addClass('checked');
                        let valBase = parseInt($parent.data('qty-base'), 10);
                        $parent.find('td').has('input[type="number"]').each(function (i, elem) {
                            let valInput = parseInt($(elem).find('input[type="number"]').val() ?? "0", 10) ?? 0;
                            if (valInput > 0) {
                                valInput = valInput;
                            } else {
                                valInput = 0;
                            }
                            totalRow += valInput;
                        });
                        if (totalRow > valBase) {
                            check = false;
                        } else {
                            check = true;
                            $('.btn-save-line-lot').removeClass('disabled');
                        }
                        if (check == false) {
                            $parent.find('input[type="number"]').addClass('border-danger');
                            $('#showDivLine .modal-body').append('<p class="text-danger";">Số lượng chia cho NVL đang có lỗi. Vui lòng thử lại!!</p>');
                            $('.btn-save-line-lot').addClass('disabled');
                        } else {
                            $parent.find('input[type="number"]').removeClass('border-danger');
                            $('#showDivLine .modal-body p.text-danger').remove();
                        }
                    });

                    $('body').on('click', '.btn-total-line-1', function (e) {
                        $('#showDivLine .modal-body p.text-danger').remove();
                        let totalColumnLine = 0;
                        $('.enter-value-line-1').each(function (i, elem) {
                            let valInput = '';
                            if ($(elem).val() == '') {
                                valInput = '0';
                            } else {
                                valInput = $(elem).val();
                            }
                            totalColumnLine += parseInt(valInput, 10);
                        });
                        totalColumnLine += parseInt($(this).parent().text().trim(), 10);
                        if (totalProductLine1 < totalColumnLine) {
                            $('#showDivLine .modal-body').append('<p class="text-danger">Tổng chia line NVL đang lớn hơn chia line theo sản phẩm!</p>');
                        }
                    });

                    $('body').on('click', '.btn-total-line-2', function (e) {
                        let totalColumnLine = 0;
                        $('#showDivLine .modal-body p.text-danger').remove();
                        $('.enter-value-line-2').each(function (i, elem) {
                            let valInput = '';
                            if ($(elem).val() == '') {
                                valInput = '0';
                            } else {
                                valInput = $(elem).val();
                            }
                            totalColumnLine += parseInt(valInput, 10);
                        });
                        totalColumnLine += parseInt($(this).parent().text().trim(), 10);
                        if (totalProductLine2 < totalColumnLine) {
                            $('#showDivLine .modal-body').append('<p class="text-danger">Tổng chia line NVL đang lớn hơn chia line theo sản phẩm!</p>');
                        }
                    });

                    $('body').on('click', '.btn-total-line-3', function (e) {
                        let totalColumnLine = 0;
                        $('#showDivLine .modal-body p.text-danger').remove();
                        $('.enter-value-line-3').each(function (i, elem) {
                            let valInput = '';
                            if ($(elem).val() == '') {
                                valInput = '0';
                            } else {
                                valInput = $(elem).val();
                            }
                            totalColumnLine += parseInt(valInput, 10);
                        });
                        totalColumnLine += parseInt($(this).parent().text().trim(), 10);
                        if (totalProductLine3 < totalColumnLine) {
                            $('#showDivLine .modal-body').append('<p class="text-danger">Tổng chia line NVL đang lớn hơn chia line theo sản phẩm!</p>');
                        }
                    });

                    $('body').on('click', '.btn-total-line-4', function (e) {
                        let totalColumnLine = 0;
                        $('#showDivLine .modal-body p.text-danger').remove();
                        $('.enter-value-line-4').each(function (i, elem) {
                            let valInput = '';
                            if ($(elem).val() == '') {
                                valInput = '0';
                            } else {
                                valInput = $(elem).val();
                            }
                            totalColumnLine += parseInt(valInput, 10);
                        });
                        totalColumnLine += parseInt($(this).parent().text().trim(), 10);
                        if (totalProductLine4 < totalColumnLine) {
                            $('#showDivLine .modal-body').append('<p class="text-danger">Tổng chia line NVL đang lớn hơn chia line theo sản phẩm!</p>');
                        }
                    });
                })
                .catch(error => {
                    alert(error);
                })
        });
        $('.btn-save-line-lot').on('click', function (e) {
            e.preventDefault();
            let dataSave = [];
            let workorder = $(this).data('workorder');
            $('#showDivLine tbody tr.checked').each(function (i, elem) {
                let objItem = {};
                objItem.workOrder = workorder;
                objItem.productCode = $(elem).data('product_code');
                objItem.lotMaterial = $(elem).data('lot_product');
                objItem.line1 = $(elem).find('input.enter-value-line-1').val() != "" ? parseInt($(elem).find('input.enter-value-line-1').val(), 10) : 0;
                objItem.line2 = $(elem).find('input.enter-value-line-2').val() != "" ? parseInt($(elem).find('input.enter-value-line-2').val(), 10) : 0;
                objItem.line3 = $(elem).find('input.enter-value-line-3').val() != "" ? parseInt($(elem).find('input.enter-value-line-3').val(), 10) : 0;
                objItem.line4 = $(elem).find('input.enter-value-line-4').val() != "" ? parseInt($(elem).find('input.enter-value-line-4').val(), 10) : 0;
                dataSave.push(objItem);
            });
            if (dataSave.length > 0) {
                fetch(`${window.baseUrl}api/savedivforlot`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        dataSave: JSON.stringify(dataSave)
                    })
                })
                    .then(async response => {
                        if (!response.ok) {
                            const errorResponse = await response.json();
                            throw new Error(`${response.status} - ${errorResponse.message}`);
                        }
                        return response.json();
                    })
                    .then(data => {
                        alert(data.message);
                        window.location.reload();
                    })
                    .catch(error => {
                        alert(error);
                    })
            }
        });

        $('#showDivLine').on('change', '#tableDivLine tbody tr input[type="number"]', function (e) {
            e.preventDefault();
            $('#enterEink').modal('show');
            $('#showDivLine').addClass('d-none');
            let $this = $(this);
            let valLine = $this.attr('data-line');
            let materialCode = $this.parent().parent().parent().attr('data-product_code');
            let lotMaterial = $this.parent().parent().parent().attr('data-lot_product');
            let itemCode = $this.parent().parent().parent().attr('data-item');
            let lotItem = $this.parent().parent().parent().attr('data-lotitem');
            let qtyBase = parseInt($this.parent().parent().parent().attr('data-qty-base'), 10);
            let qtyDiv = $this.val();
            if (qtyDiv <= qtyBase) {
                $('#enterEink').on('shown.bs.modal', function (e) {
                    $('#einkDivLine').focus();
                    $('#einkDivLine').val('');
                    $('#einkDivLine').on('keypress', function (e) {
                        if (e.key === 'Enter') {
                            e.preventDefault();
                            let valEink = $(this).val();
                            fetch(`${window.baseUrl}processing/connectingeinkdivline`, {
                                method: 'POST',
                                headers: {
                                    'Content-Type': 'application/json'
                                },
                                body: JSON.stringify({
                                    line: valLine,
                                    productCode: itemCode,
                                    productLot: lotItem,
                                    materialCode: materialCode,
                                    lotMaterial: lotMaterial,
                                    qtyDiv: qtyDiv,
                                    einkMac: valEink,
                                })
                            })
                                .then(async response => {
                                    if (!response.ok) {
                                        const errorResponse = await response.json();
                                        throw new Error(`${response.status} - ${errorResponse.message}`);
                                    }
                                    return response.json();
                                })
                                .then(data => {
                                    swal('Thông báo', data.message, 'success');
                                    $('#enterEink').modal('hide');
                                    $('#showDivLine').removeClass('d-none');
                                })
                                .catch(error => {
                                    alert(error);
                                    $('#einkDivLine').focus();
                                    $('#einkDivLine').val('');
                                })
                        }
                    });
                });
            }
        });

        $('#enterEink .btn-close').on('click', function (e) {
            e.preventDefault();
            $('#enterEink').modal('hide');
            $('#showDivLine').removeClass('d-none');
        });
    }
    if ($('.table-calculator-item')) {
        // Chia line mới
        $('.qty-processing-wrapper input').val('');
        var widthTable = $(".table-calculator-item table").width();
        var heightTbody = $(".table-calculator-item table").height();

        // Style bảng thành freezer
        if (widthTable > 1300 || widthDocument < 1024) {
            $(".table-calculator-item table thead").addClass("freezer-column");
            $(".table-calculator-item table tbody").addClass("freezer-column");
            $(".table-calculator-item table tfoot").addClass("freezer-column");
        }
        if (heightTbody > 480) {
            $(".table-calculator-item table thead").addClass("freezer-row");
        }

        $(".table-calculator-item tbody.freezer-column tr").each(function (i, elem) {
            let width_0 = $(elem).find('td.item-freezer').eq(0).width() + 18;
            let width_1 = $(elem).find('td.item-freezer').eq(1).width() + 17;
            let width_2 = $(elem).find('td.item-freezer').eq(2).width() + 17;
            let width_3 = $(elem).find('td.item-freezer').eq(3).width() + 17;
            let width_4 = $(elem).find('td.item-freezer').eq(4).width() + 17;
            let width_5 = $(elem).find('td.item-freezer').eq(5).width() + 17;

            console.log(width_0);
            console.log(width_1);
            $(elem).find('td.item-freezer').eq(1).css('left', width_0);
            $(elem).find('td.item-freezer').eq(2).css('left', width_0 + width_1);
            $(elem).find('td.item-freezer').eq(3).css('left', width_0 + width_1 + width_2);
            $(elem).find('td.item-freezer').eq(4).css('left', width_0 + width_1 + width_2 + width_3);
            $(elem).find('td.item-freezer').eq(5).css('left', width_0 + width_1 + width_2 + width_3 + width_4);
            $(elem).find('td.item-freezer').eq(6).css('left', width_0 + width_1 + width_2 + width_3 + width_4 + width_5);
        });

        $(".table-calculator-item thead.freezer-column tr").each(function (i, elem) {
            let width_0 = $(elem).find('th.item-freezer').eq(0).width() + 18;
            let width_1 = $(elem).find('th.item-freezer').eq(1).width() + 17;
            let width_2 = $(elem).find('th.item-freezer').eq(2).width() + 17;
            let width_3 = $(elem).find('th.item-freezer').eq(3).width() + 17;
            let width_4 = $(elem).find('th.item-freezer').eq(4).width() + 17;
            let width_5 = $(elem).find('th.item-freezer').eq(5).width() + 17;

            $(elem).find('th.item-freezer').eq(1).css('left', width_0);
            $(elem).find('th.item-freezer').eq(2).css('left', width_0 + width_1);
            $(elem).find('th.item-freezer').eq(3).css('left', width_0 + width_1 + width_2);
            $(elem).find('th.item-freezer').eq(4).css('left', width_0 + width_1 + width_2 + width_3);
            $(elem).find('th.item-freezer').eq(5).css('left', width_0 + width_1 + width_2 + width_3 + width_4);
            $(elem).find('th.item-freezer').eq(6).css('left', width_0 + width_1 + width_2 + width_3 + width_4 + width_5);
        });

        let heightTrHead = $('#modalCalculatorTimeProd .table-calculator-item thead.freezer-row tr').eq(0).height();
        $('#modalCalculatorTimeProd .table-calculator-item thead.freezer-row tr').eq(1).find('th').css('top', (heightTrHead - 0.5));

        // check stt của bảng có chưa
        let arrDataSort = [];
        $('#modalCalculatorTimeProd table tbody tr').each(function (i, e) {
            const $row = $(this);
            const $indexInput = $row.find('input.indexWOProduction');
            const $enterValInput = $row.find('input.input-enter-val');

            const indexVal = $indexInput.val() || '';

            if (indexVal === '') {
                $enterValInput.addClass('disabled');
            } else {
                $enterValInput.removeClass('disabled');
            }

            $indexInput.off('change').on('change', function (ev) {
                ev.preventDefault();
                const newVal = parseInt($(this).val(), 10);
                const workOrder = $(this).parent().parent().find('.workorder').text().trim();

                if (!isNaN(newVal) && newVal > 0) {
                    $enterValInput.removeClass('disabled');
                    $('.btn-sort-wo').removeClass('disabled');

                    const objElemt = {
                        soTT: newVal,
                        workorder: workOrder,
                        htmlElem: $row.html().trim(),
                    };

                    const existingIndex = arrDataSort.findIndex(item => item.soTT === newVal && item.workorder === workOrder);
                    if (existingIndex !== -1) {
                        arrDataSort.splice(existingIndex, 1);
                    }
                    arrDataSort.push(objElemt);
                } else {
                    $enterValInput.addClass('disabled');
                }
            });
        });

        $('.btn-sort-wo').on('click', function (e) {
            e.preventDefault();

            $(this).append('<span class="spinner-grow spinner-grow-sm"></span>');
            setTimeout(() => {
                arrDataSort.sort((a, b) => {
                    let aSTT = a.soTT;
                    let bSTT = b.soTT;
                    return aSTT - bSTT;
                });
                let htmlTbody = '';
                arrDataSort.forEach(item => {
                    htmlTbody += `<tr tabindex="${(item.soTT - 1)}">${item.htmlElem}</tr>`;
                });
                $('#tableCalculators tbody').html(htmlTbody);
                arrDataSort.forEach(item => {
                    $(`#tableCalculators tbody tr[tabindex="${(item.soTT - 1)}"]`).each(function (i, e) {
                        $(this).find('input.indexWOProduction').val(item.soTT);
                    });
                });

                $(this).addClass('d-none');
                $(this).find('.spinner-grow').remove();
                $('.btn-save-calculator').removeClass('d-none');
                $('.qty-processing-wrapper').removeClass('d-none');
                $('.qty-processing-wrapper p.text-danger').html('1. Vui lòng nhập số lượng sản xuất theo giờ trước.');
                $('.table-calculator-item p.text-danger').html('2. Sau khi nhập số lượng sản xuất tính ra Cycle Time mới thực hiện tính thời gian sản xuất.');
                if ($('#qtyInHours').val() === '') {
                    $('#modalCalculatorTimeProd #tableCalculators tbody tr input.input-enter-val').each(function () {
                        $(this).addClass('disabled');
                    });
                }
            }, 2000);
        });
        // check character không có số
        $('body').on('change', '#modalCalculatorTimeProd #tableCalculators .character-input', function (e) {
            const regex = /^[A-Za-z]/;
            $(this).removeClass('border-danger');
            if (!regex.test($(this).val())) {
                showAlert('Thông báo', 'Vui lòng nhập các chữ cái A-Z', 'error', [false, 'Ok']);
                $(this).addClass('border-danger');
            }
        });
        $('body').on('change', '#modalCalculatorTimeProd #tableCalculators .qty-production-line', function (e) {
            e.preventDefault();
            $(this).removeClass('border-danger');

            let dataLine = $(this).attr('data-line');
            let qtyProcessLine = parseInt($(this).val(), 10);

            let cycleTime = parseFloat($('#cycleTime').val());

            let thisQtyWo = parseInt($(this).parent().parent().find('.qty-wo').text(), 10);
            let totalQtyProcessing = 0;
            $(this).parent().parent().find('td').each(function (i, col) {
                let valQtyProduct = $(col).find('input.qty-production-line').val() != '' ? $(col).find('input.qty-production-line').val() : "0";
                totalQtyProcessing += parseInt(valQtyProduct, 10);
            });

            if (qtyProcessLine > thisQtyWo || totalQtyProcessing > thisQtyWo) {
                $(this).addClass('border-danger');
                if ($(this).hasClass('border-danger')) {
                    $(".btn-save-line").addClass("disabled");
                }
                showAlert('Thông báo', 'Số lượng chia lớn hơn số lượng theo chỉ thị vui lòng kiểm tra lại.', 'error', [false, 'Ok']);
                return;
            } else {
                $(".btn-save-line").removeClass("disabled");
            }
            if (cycleTime <= 0 || isNaN(cycleTime)) {
                $('#qtyInHours').addClass('border-danger');
                return;
            }
            let cycleTimeMinutes = cycleTime / 60;
            let timeProductionMinutes = qtyProcessLine * cycleTimeMinutes;

            let timeProductionHours = timeProductionMinutes / 60;

            $(this).parent().parent().find(`input.time-production[data-line="${dataLine}"]`).val(timeProductionHours.toFixed(2));
            $(this).parent().addClass('is-changed');
        });
        $('body').on('change', '#modalCalculatorTimeProd #tableCalculators .date-start-lot', function (e) {
            $('.btn-save-calculator').removeClass('disabled');
        });
        $('#qtyInHours').on('change', function (e) {
            e.preventDefault();
            let valQtyHours = $(this).val();
            let cycleTime = 3600 / parseInt(valQtyHours, 10);
            $('#cycleTime').val(cycleTime);
            $(this).removeClass('border-danger');
            $('#modalCalculatorTimeProd #tableCalculators tbody tr input.input-enter-val').each(function () {
                $(this).removeClass('disabled');
            });
        });

        // Render data
        if ($('#jsonCalcTimeProd').val() !== '') {
            let dataAppend = JSON.parse($('#jsonCalcTimeProd').val()) || [];
            let dataTotals = JSON.parse($('#jsonTotalLine').val()) || [];
 
            $('#tableCalculators tbody tr').each(function (i, row) {
                let item = dataAppend[i];
                if (item) {
                    $(row).find('input.indexWOProduction').val(item.SoTT);
                    $(row).find('input.character-input').val(item.Character);
                }
                let dataProdLines = item.ProductionLines;
                dataProdLines.forEach(itemLine => {
                    $(row).find(`input.qty-production-line[data-line="${itemLine.DataLine}"]`).val(itemLine.Qty);
                    $(row).find(`input.time-production[data-line="${itemLine.DataLine}"]`).val(itemLine.Time);
                    $(row).find(`input.date-start-lot[data-line="${itemLine.DataLine}"]`).val(itemLine.StartDate);
                    $(row).find(`input.date-end-lot[data-line="${itemLine.DataLine}"]`).val(itemLine.EndDate);
                    $(row).find(`input.qty-in-day[data-line="${itemLine.DataLine}"]`).val(itemLine.QtyInDay);
                });
            });
            $('#tableCalculators tfoot tr').each(function (i, row) {
                Object.keys(dataTotals).forEach(key => {
                    $(row).find(`span.total-line[data-line="${key}"]`).html(dataTotals[key]);
                })
            });
        }

        // Đẩy lên server để xử lý qua gọi API
        $('.btn-save-calculator').on('click', function (e) {
            e.preventDefault();
            $(this).append('<span class="spinner-grow spinner-grow-sm"></span>');
            let arrDataSaved = [];
            let processCode = "";
            if ($('#cycleTime').val() != '') {
                $('#tableCalculators tbody tr').each(function (i, row) {
                    let rowData = {};
                    rowData.IndexNumber = $(row).find('input.indexWOProduction').val();
                    rowData.WorkOrder = $(row).find('.workorder').text().trim();
                    rowData.ProductCode = $(row).find('.product-code').text().trim();
                    rowData.LotNo = $(row).find('.lot-no').text().trim();
                    rowData.QtyWo = $(row).find('.qty-wo').text().trim();
                    rowData.Character = $(row).find('.character-input').val().toUpperCase();
                    rowData.CycleTime = $('#cycleTime').val();
                    processCode = $(row).find('.processcode').val();

                    let productionLines = [];
                    $(row).find('td.is-changed .qty-production-line').each(function () {
                        let dataLine = $(this).attr('data-line');
                        let timeProduction = $(row).find(`.time-production[data-line="${dataLine}"]`).val();
                        let dateStart = $(row).find(`.date-start-lot[data-line="${dataLine}"]`).val();

                        productionLines.push({
                            dataLine: dataLine,
                            qty: $(this).val(),
                            time: timeProduction,
                            startDate: dateStart,
                        });
                    })
                    rowData.ProductionLines = productionLines;
                    arrDataSaved.push(rowData);
                });
                fetch(`${window.baseUrl}processing/SaveCalcProductionTime`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json;'
                    },
                    body: JSON.stringify({
                        jsonStrDivLine: JSON.stringify(arrDataSaved),
                        processCode: processCode
                    })
                })
                    .then(async response => {
                        if (!response.ok) {
                            const errorResponse = await response.json();
                            throw new Error(`${response.status} - ${errorResponse.message}`);
                        }
                        return response.json();
                    })
                    .then(data => {
                        let dataAppend = data.dataRender;
                        let dataTotals = data.totalLines;
                        showAlert('Thông báo', data.message, 'success', [false, 'Ok'])
                            .then((isConfirmed) => {
                                if (isConfirmed) {
                                    $('#tableCalculators tbody tr').each(function (i, row) {
                                        let item = dataAppend[i];
                                        if (item) {
                                            $(row).find('input.indexWOProduction').val(item.indexNumber);
                                            $(row).find('input.character-input').val(item.character);
                                        }
                                        let dataProdLines = item.productionLines;
                                        dataProdLines.forEach(itemLine => {
                                            $(row).find(`input.qty-production-line[data-line="${itemLine.dataLine}"]`).val(itemLine.qty);
                                            $(row).find(`input.time-production[data-line="${itemLine.dataLine}"]`).val(itemLine.time);
                                            $(row).find(`input.date-start-lot[data-line="${itemLine.dataLine}"]`).val(itemLine.startDate);
                                            $(row).find(`input.date-end-lot[data-line="${itemLine.dataLine}"]`).val(itemLine.endDate);
                                            $(row).find(`input.qty-in-day[data-line="${itemLine.dataLine}"]`).val(itemLine.qtyInDay);
                                        });
                                    });
                                    $('#tableCalculators tfoot tr').each(function (i, row) {
                                        Object.keys(dataTotals).forEach(key => {
                                            $(row).find(`span.total-line[data-line="${key}"]`).html(dataTotals[key]);
                                        })
                                    });
                                    $(this).find('.spinner-grow').remove();
                                }
                            })
                    })
                    .catch(error => {
                        alert(error);
                    })
            }
        });

        // Check thời gian sản xuất không quá 30 phút
        $("#tableCalculators tbody tr").each(function (i, elem) {
            if ($(elem).find(".date-start-lot").val() !== '') {
                let dateTimeFull = new Date();
                let itemArr = [];
                let dateInput = new Date($(elem).find(".date-start-lot").val());
                let timestampInput = dateInput.getTime();
                if (timestampCurrent == timestampInput) {
                    dateTimeFull = dateInput;
                    let qtyInput = parseInt($(elem).attr('data-qty'), 10);
                    itemArr.push({
                        processCode: $(elem).find("input.processcode").val(),
                        workOrder: $(elem).attr("data-workorder"),
                        qtyUsed: qtyInput
                    });
                }
                checkTime30(dateTimeFull, itemArr);
            }
            $(elem).find('input:not(.indexWOProduction), textarea').on('change', function (e) {
                e.preventDefault();
                $(this).parent().parent().addClass('is-changed');
                $(".btn-save-line").removeClass("disabled");
            })
        });

        //Lưu công đoạn thường
        $(".btn-save-line").on("click", function (e) {
            e.preventDefault();
            let arrDataSaved = [];
            let processCode = "";
            $('#tableCalculators tbody tr.is-changed').each(function (i, row) {
                let rowData = {};
                rowData.IndexNumber = $(row).find('input.indexWOProduction').val();
                rowData.WorkOrder = $(row).find('.workorder').text().trim();
                rowData.ProductCode = $(row).find('.product-code').text().trim();
                rowData.LotNo = $(row).find('.lot-no').text().trim();
                rowData.QtyWo = $(row).find('.qty-wo').text().trim();
                rowData.Character = $(row).find('.character-input').val().toUpperCase();
                processCode = $(row).find('.processcode').val();

                let productionLines = [];
                $(row).find('td .qty-production-line').each(function () {
                    let dataLine = $(this).attr('data-line');
                    let timeProduction = $(row).find(`.time-production[data-line="${dataLine}"]`).val();
                    let dateStart = $(row).find(`.date-start-lot[data-line="${dataLine}"]`).val();
                    let dateEnd = $(row).find(`.date-end-lot[data-line="${dataLine}"]`).val();
                    let qtyInday = $(row).find(`.qty-in-day[data-line="${dataLine}"]`).val();
                    let qtyProduction = $(this).val();
                    if (qtyProduction && timeProduction && dateStart && dateEnd) {
                        productionLines.push({
                            dataLine: dataLine,
                            qty: $(this).val(),
                            time: timeProduction,
                            startDate: dateStart,
                            endDate: dateEnd,
                            qtyInDay: qtyInday
                        });
                    }
                })
                rowData.ProductionLines = productionLines;
                arrDataSaved.push(rowData);
            });
            if (arrDataSaved.length > 0) {
                fetch(`${window.baseUrl}processing/UpdateCalcTimes`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json; charset=utf-8'
                    },
                    body: JSON.stringify({
                        jsonStrDivLine: JSON.stringify(arrDataSaved),
                        processCode: processCode
                    })
                })
                    .then(async response => {
                        if (!response.ok) {
                            const errorResponse = await response.json();
                            throw new Error(`${response.status} - ${errorResponse.message}`);
                        }
                        return response.json();
                    })
                    .then(data => {
                        alert(data.message);
                        location.reload();
                    })
                    .catch(error => {
                        alert(error);
                    })
            }
        });
    }
}
function checkTime30(inputTime, itemData) {
    let newDate = new Date(inputTime);
    let interval = 10 * 60 * 1000; // 10 phut 1 lần
    function notify() {
        let currentDate = new Date();
        let timeDiff = newDate - currentDate;
        if (timeDiff <= 0) {
            clearInterval(notificationInterval);
        } else if (timeDiff <= 30 * 60 * 1000) { // Trước 30 phút
            fetch(`${window.baseUrl}Processing/CompareQtyWithInventory`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                },
                body: JSON.stringify({
                    strDataCheckQty: JSON.stringify(itemData)
                })
            })
                .then(async response => {
                    if (!response.ok) {
                        const errorResponse = await response.json();
                        throw new Error(`${response.status} - ${errorResponse.message}`);
                    }
                    return response.json();
                })
                .then(data => {
                    let WOOutStock = data.dataOutInventory;
                    if (data.status) {
                        clearInterval(notificationInterval);
                        for (let i = 0; i < itemData.length; i++) {
                            $(".table-line-process tbody tr[data-workorder='" + itemData[i].workOrder + "']").removeClass("bg-warning");
                        }
                    } else {
                        let strWoOutStock = "";
                        for (let j = 0; j < WOOutStock.length; j++) {
                            for (let i = 0; i < itemData.length; i++) {
                                if (WOOutStock[j].WORKORDER == itemData[i].workOrder) {
                                    strWoOutStock = itemData[i].workOrder;
                                    $(".table-line-process tbody tr[data-workorder='" + itemData[i].workOrder + "']").addClass("bg-warning");
                                }
                            }
                        }
                        alert(data.message + " tại workorder: " + strWoOutStock);
                    }
                })
                .catch(error => {
                    alert(error)
                })
        } else {
            clearInterval(notificationInterval);
        }
    }
    let notificationInterval = setInterval(notify, interval);
}
function ReadWorkOrder() {
    $('body').on('shown.bs.modal', '#readWorkOrderQR', function () {
        $('#qrWOValue').val('');
        $('#qrWOValue').focus();
        localStorage.removeItem('dataWorkOrderProd');
    });
    $('body').on('click', '#readWorkOrderQR .btn-close', function (e) {
        swal({
            title: 'Bạn chắc chắn muốn đóng thao tác?',
            text: 'Bạn không nên đóng thao tác khi đang thực hiện thao tác. Điều đó gây ra thiếu dữ liệu. Trân trọng cảm ơn!',
            icon: 'warning',
            buttons: ["Không", "Có"],
        }).then((isConfirmed) => {
            if (isConfirmed) {
                $('#readWorkOrderQR').modal('hide');
                $('.content-read-workorder').append('<button class="btn btn-success btn-read-workorder" id="btnReadWorkOrder">Đọc mã chỉ thị</button>');
            } else {
                $('#qrWOValue').focus();
                return;
            }
        });
    });
    $('body').on('click', '.content-read-workorder #btnReadWorkOrder', function (e) {
        $('#readWorkOrderQR').modal('show');
        $(this).remove();
    });
}
// Thao tác kiểm tra trước
function PreOperation(positionWorking, userID, userDisplayName) {
    // Đọc chỉ thị sản xuất
    $('body').on('keypress', '#qrWOValue', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#readWorkOrderQR #confirmWorkOrder').trigger('click');
        }
    });
    // Lấy thông tin workorder theo chỉ thị đã chia cho vị trí đang thức hiện
    $('body').on('click', '#readWorkOrderQR #confirmWorkOrder', function (e) {
        let valWorkOrder = $('#qrWOValue').val();
        //Tìm kiếm lên database để lấy dữ liệu line
        if (valWorkOrder != "") {
            CheckWorkOrder(valWorkOrder, positionWorking);
        }
    });

    // Hiển thị thao tác nhập điều kiện sản xuất tạo vị trí
    processInputConditionPreCheck(positionWorking);

    // Xử lý đọc mã nguyên vật liệu
    processReadMaterialQR(positionWorking);

    // Xử lý nhập đạt lỗi
    enterMaterialCheckResult(positionWorking);

    // Xử lý kết thúc lô sớm khi chưa đủ số lượng
    endEarlyProduction(positionWorking);
}
function CheckWorkOrder(workorder, positionWorking) {
    let jsonRequest = {};
    let urlApi = "";
    urlApi = window.baseUrl + 'api/getItemByWO';
    if (workorder == "") {
        jsonRequest.strDataCheck = positionWorking;
    } else {
        jsonRequest.jsonStr = workorder;
        jsonRequest.strDataCheck = positionWorking;
    }
    fetch(urlApi, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;',
        },
        body: JSON.stringify(jsonRequest)
    })
        .then(async response => {
            if (!response.ok) {
                const errorResponse = await response.json();
                throw new Error(`${errorResponse.message}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.status) {
                let dataRender = data.renderData;   
                let dataEntries = data.dataEntries;

                if (dataRender != null) {
                    // Lấy checksheet để áp dụng gọi form nhập
                    let csWorkstation = data.csWorkstation;
                    csWorkstation.forEach(item => {
                        if (item.isChecksheetCondition) {
                            $('.checksheet-condition').attr('data-checksheetid', item.checksheetId);
                            $('.checksheet-condition').text(item.checksheetCode);
                            $('.checksheet-condition').attr('data-checksheetversionid', item.lastUsedChecksheetVersionId);
                        } else {
                            $('.checksheet-operation').attr('data-checksheetid', item.checksheetId);
                            $('.checksheet-operation').text(item.checksheetCode);
                            $('.checksheet-operation').attr('data-checksheetversionid', item.lastUsedChecksheetVersionId);
                        }
                    });
                    if (data.csitemAssignments) {
                        if (data.csitemAssignments.isChecksheetCondition) {
                            $('.checksheet-condition').attr('data-checksheetid', data.csitemAssignments.checksheetId);
                            $('.checksheet-condition').text(data.csitemAssignments.checksheetCode);
                            $('.checksheet-condition').attr('data-checksheetversionid', data.csitemAssignments.lastUsedChecksheetVersionId);
                        } else {
                            $('.checksheet-operation').attr('data-checksheetid', data.csitemAssignments.checksheetId);
                            $('.checksheet-operation').text(data.csitemAssignments.checksheetCode);
                            $('.checksheet-operation').attr('data-checksheetversionid', data.csitemAssignments.lastUsedChecksheetVersionId);
                        }
                    }
                    if (dataRender.qtyInLine == 0) {
                        $('#readWorkOrderQR .modal-body .notice-error').html('Không được sản xuất trên line này! Vui lòng thử lại!');
                        $('#readWorkOrderQR').modal('show');
                        $('#qrWOValue').val('');
                        $('#qrWOValue').focus();
                        return;
                    }
                    $('#readWorkOrderQR .modal-body .notice-error').html('');
                    $('#readWorkOrderQR').modal('hide');
                    setParsedLocalStorageItem('dataWorkOrderProd', dataRender);
                    $('#lotProdContent').html(`
                                       <div class="item-column">
                                            <div class="item-data">
                                                <span>Chủng loại: </span>
                                                <span id="productCode">${dataRender.productCode}</span>
                                            </div>
                                        </div>
                                        <div class="item-column">
                                            <div class="item-data">
                                                <span>Lô sản xuất: </span>
                                                <span id="lotNoWO">${dataRender.lotNo}</span>
                                            </div>
                                        </div>
                                        <div class="item-column">
                                            <div class="item-data">
                                                <span>Line sản xuất: </span>
                                                <span id="lineCurrent">${dataRender.line}</span>
                                            </div>
                                        </div>
                                        <div class="item-column">
                                            <div class="item-data">
                                                <span>Số lượng trên line: </span>
                                                <span id="qtyInLine">${dataRender.qtyInLine}</span>
                                            </div>
                                        </div>
                                        <div class="item-column">
                                            <div class="item-data">
                                                <span>Năm: </span>
                                                <span id="yearProduction">${dataRender.year}</span>
                                            </div>
                                        </div>
                                        <input type="hidden" id="workOrderProd" value="${dataRender.workOrder}"/>
                                        ${data.csitemAssignments != null ? '<input type="hidden" id="itemAssignmentId" value="${data.csitemAssignments.itemAssignmentId}"/>' : ''}
                                    `);
                    if (data.currentAction === "Check Conditions") {
                        $('#groupBtnActions').html('<button class="btn btn-success btn-sm btn-read-condition" id="btnCheckCondition">Kiểm tra điều kiện</button>');
                    }
                    if (data.currentAction === "Read Conditions") {
                        if (getParsedLocalStorageItem('readConditionAddOn')) {
                            $('#groupBtnActions').html('<button class="btn btn-success btn-sm btn-read-condition" id="btnReadCondition">Đọc điều kiện bổ sung</button>');
                        } else {
                            $('#groupBtnActions').html('<button class="btn btn-success btn-sm btn-read-condition" id="btnReadCondition">Đọc điều kiện</button>');
                        }
                        
                    }
                    if (data.currentAction === "Read Materials") {
                        $('.confirm-condition-process').html('<p class="alert alert-success mt-2"><i class="bx bx-check"></i> Đã xác nhận điều kiện thiết bị.</p>');
                        $('#groupBtnActions').html(`<button class="btn btn-secondary btn-sm btn-read-condition me-2" id="btnCheckCondition" data-typeaction="New Conditions Add-On">Đọc điều kiện bổ sung</button>
                        <button class="btn btn-success btn-sm btn-read-material" id="btnReadMaterials">Đọc nguyên vật liệu</button>`); 
                    }
                    let htmlEnterResults = '';
                    if (dataEntries.length > 0) {
                        let processWoStatus = '';
                        let trayNo = '';
                        htmlEnterResults += `<div class="text-center">
                                <h5 class="fw-bolder">Thông tin các máng đã đọc</h5>
                            </div>
                            <div class="data-render-items">`;
                        dataEntries.forEach(item => {
                            htmlEnterResults += `<div class="tray-content">`;
                            let classSuccess = '';
                            let classDnone = '';
                            if ((item.qtyOfRead - item.qtyProduction) == 0 && item.qtyOK > 0) {
                                classSuccess = ' success';
                                classDnone = ' d-none';
                            }
                            if (item.processEntryStatus.includes("Connection Eink")) {
                                classDnone = '';
                            }
                            let dataQtyProduction = (item.qtyOfRead - item.qtyProduction);
                            if (classSuccess !== '') {
                                dataQtyProduction = item.qtyProduction;
                            }
                            trayNo = item.trayNo;
                            processWoStatus = item.processWOStatus;
                            let parseJsonSaved = item.jsonSaved != null ? JSON.parse(item.jsonSaved) : [];
                            htmlEnterResults += `
                                        <div class="card-custom-item${classSuccess}">
                                            <p class="title-entry text-center mb-2 fw-bolder">Thông tin máng ${item.trayNo}</p>
                                            <div class="card-custom-body mb-2">
                                                <div class="data-item">Số lượng đọc trên máng: <strong>${item.qtyOfRead}</strong></div>
                                                <div class="data-item">Số lượng thực hiện: <strong>${item.qtyProduction}</strong></div>
                                                <div class="data-item">Số lượng đạt: <strong>${item.qtyOK}</strong></div>
                                                <div class="data-item">Số lượng lỗi: <strong>${item.qtyNG}</strong></div>
                                                <div class="data-item">Số lượng còn lại cần thực hiện: <strong>${(item.qtyOfRead - item.qtyProduction)}</strong></div>
                                            </div>
                                            <div class="mt-2${classDnone} d-flex justify-content-center" id="actionTrays">
                                                <button class="btn btn-warning btn-sm btn-confirm-operations d-none" data-trayno="${item.trayNo}" data-entryid="${item.formEntryIndex}" id="btnConfirmOperation${item.entryIndex}">Tiếp tục thao tác</button>
                                                <button class="btn btn-success btn-show-again-enter-results btn-sm d-none" data-trayno="${item.trayNo}" data-qtyproduction="${dataQtyProduction}" id="btnEnterResultProd${item.entryIndex}">Nhập kết quả sản xuất</button>
                                                <input type="hidden" class="jsonDataSaved" value='${parseJsonSaved.formData !== undefined ? JSON.stringify(parseJsonSaved.formData) : ''}' />
                                                <button class="btn btn-success btn-sm btn-confirm-eink d-none" id="btnShowReadEink${item.trayNo}">Đọc thẻ Eink</button>
                                            </div>
                                        </div>
                                    </div>`;
                        });
                        htmlEnterResults += `</div>`;
                        let dataTrayNo = {
                            trayNo: trayNo,
                        }
                        setParsedLocalStorageItem('dataTrayCurrent', dataTrayNo);
                        $('#listContentEntered').html(htmlEnterResults);
                        if (processWoStatus === "In Processing") {
                            $('#listContentEntered').after('<div class="mt-3"><button class="btn btn-danger btn-end-early btn-sm" id="btnEndEarly">Kết thúc sớm</button></div>');
                        }
                    }
                    if (data.currentAction === "Working Wires") {
                        $('.confirm-condition-process').html('<p class="alert alert-success mt-2"><i class="bx bx-check"></i> Đã xác nhận điều kiện thiết bị.</p>');
                        $('#groupBtnActions').html(`<button class="btn btn-secondary btn-sm btn-read-condition me-2" id="btnCheckCondition" data-typeaction="New Conditions Add-On">Đọc điều kiện bổ sung</button>`);
                        if (dataEntries.length > 0) {
                            $('#actionTrays .btn-confirm-operations').removeClass('d-none');
                        }
                    }
                    if (data.currentAction === "Working Wires Continue") {
                        $('.confirm-condition-process').html('<p class="alert alert-success mt-2"><i class="bx bx-check"></i>Đã xác nhận điều kiện thiết bị.</p>');
                        $('#groupBtnActions').html(`<button class="btn btn-secondary btn-sm btn-read-condition me-2" id="btnCheckCondition" data-typeaction="New Conditions Add-On">Đọc điều kiện bổ sung</button>`);
                        $('#actionTrays .btn-confirm-operations').removeClass('d-none');
                    }
                    if (data.currentAction == "Enter production results") {
                        $('.confirm-condition-process').html('<p class="alert alert-success mt-2"><i class="bx bx-check"></i>Đã xác nhận điều kiện thiết bị.</p>');
                        if (dataEntries.length > 0) {
                            $('#actionTrays .btn-show-again-enter-results').removeClass('d-none');
                            $('#groupBtnActions').html(`<button class="btn btn-secondary btn-sm btn-read-condition mt-2" id="btnCheckCondition" data-typeaction="New Conditions Add-On">Đọc điều kiện bổ sung</button>`);
                        } else {
                            $('#groupBtnActions').html(`<button class="btn btn-secondary btn-sm btn-read-condition mt-2" id="btnCheckCondition" data-typeaction="New Conditions Add-On">Đọc điều kiện bổ sung</button>
                            <button class="btn btn-success btn-sm btn-show-again-enter-results mt-2" id="btnEnterResultProd">Nhập kết quả sản xuất</button>`);
                        }
                    }
                    if (data.currentAction == "Connection Eink") {
                        $('.confirm-condition-process').html('<p class="alert alert-success mt-2"><i class="bx bx-check"></i> Đã xác nhận điều kiện thiết bị.</p>');
                        $('#groupBtnActions').html(`<button class="btn btn-secondary btn-sm btn-read-condition me-2" id="btnCheckCondition" data-typeaction="New Conditions Add-On">Đọc điều kiện bổ sung</button>`);
                        $('#actionTrays .btn-confirm-eink').removeClass('d-none');
                    }
                    // Khi action check bất thường
                    if (data.currentAction === "Leader Check Abnormal") {
                        $('#leaderComfirmedAbnormal').modal('show');
                    }
                    if (data.currentAction === "New WorkOrder") {
                        $('#lotProdContent').html('');
                        $('#readWorkOrderQR').modal('show');
                        localStorage.removeItem('dataMaterialProd');
                        localStorage.removeItem('dataWorkOrderProd');
                    }
                } else {
                    swal('Lỗi', 'Workorder chưa được chia line!', 'error');
                }
                $('#readWorkOrderQR').modal('hide');
            } else {
                if (workorder === '') {
                    $('#readWorkOrderQR').modal('show');
                } else {
                    swal('Thông báo!', data.message, 'info')
                        .then((isConfirmed) => {
                            if (isConfirmed) {
                                $('#readWorkOrderQR').modal('show');
                            }
                        });
                }
                localStorage.removeItem('dataMaterialProd');
                localStorage.removeItem('PRODUCTION_CONTINUE');
            }
        })
        .catch(error => {
            swal('Lỗi', error.message, 'error').then((isConfirmed) => {
                if (isConfirmed) {
                    $('#readWorkOrderQR').modal('show');
                }
            });
            localStorage.removeItem('dataMaterialProd');
            localStorage.removeItem('PRODUCTION_CONTINUE');
        })
}

// Thao tác gia công đầu mút
function ReadQRTrayCreated() {
    $('#readQRTrayData').modal('show');
    $('body').on('shown.bs.modal', '#readQRTrayData', function () {
        $('#QRTraydata').val('');
        $('#QRTraydata').focus();
    });
    $('body').on('click', '#readQRTrayData .btn-close', function (e) {
        swal({
            title: 'Bạn chắc chắn muốn đóng thao tác?',
            text: 'Bạn không nên đóng thao tác khi đang thực hiện thao tác. Điều đó gây ra thiếu dữ liệu. Trân trọng cảm ơn!',
            icon: 'warning',
            buttons: ["Không", "Có"],
        }).then((isConfirmed) => {
            if (isConfirmed) {
                $('#readQRTrayData').modal('hide');
                $('.content-read-workorder').append('<button class="btn btn-success btn-read-workorder" id="btnReadWorkOrder">Đọc mã QR máng</button>');
            } else {
                $('#QRTraydata').focus();
                return;
            }
        });
    });
    $('body').on('click', '.content-read-workorder #btnReadWorkOrder', function (e) {
        $('#readQRTrayData').modal('show');
        $(this).remove();
    });
}
function processGWProduction(positionWorking) {
    let regexChars = /[!@#$%^&*(),.?":{}|<>]/g;
    $('body').on('keypress', '#QRTraydata', function (e) {
        if (e.key === 'Enter') {
            let valueQR = $(this).val();
            let arrValScaned = valueQR.split(regexChars);

            const isValidFormat = arrValScaned[0]?.length >= 3 &&
                arrValScaned[1]?.length >= 11 &&
                arrValScaned[2]?.length >= 6;
            if (!isValidFormat) {
                showAlert('Lỗi!', 'Định dạng máng không đúng. Vui lòng kiểm tra lại', 'error', [false, "Nhập lại"])
                    .then((isConfirmed) => {
                        if (isConfirmed) {
                            $('#QRTraydata').val('').focus();
                        }
                    });
                return;
            }
            let [trayNo, productCode, lotNo] = arrValScaned;
            fetch(`${window.baseUrl}api/getItemByWO`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json;'
                },
                body: JSON.stringify({
                    strDataCheck: positionWorking,
                    productCode: productCode,
                    lotNo: lotNo
                })
            })
                .then(async response => {
                    if (!response.ok) {
                        const errorResponse = await response.json();
                        throw new Error(`${errorResponse.message}`);
                    }
                    return response.json();
                })
                .then(data => {

                })
        }
    });
}

// ============================================== Xử lý thao tác kiểm tra trước ==============================================
function processInputConditionPreCheck(positionWorking) {
    const MODAL_SELECTOR = '#showConditionPreOperation';
    const MODAL_CONFIRM_FREQUENCY = '#showFrequencyConditions';
    const BTN_CONFIRM_SELECTOR = '.btn-confirm-preoperation';
    CheckConditions(MODAL_CONFIRM_FREQUENCY);
    // Hiển thị modal đọc điều kiện
    $('body').on('click', '#btnReadCondition', async function (e) {
        $(MODAL_SELECTOR).modal('show');
        $(MODAL_CONFIRM_FREQUENCY).modal('hide');

        let dataProductions = getParsedLocalStorageItem('dataWorkOrderProd');

        let frequencyId = $(MODAL_CONFIRM_FREQUENCY).find('input[type="radio"]:checked').attr('data-frequencyid');

        let requestType = $(this).attr('data-typeaction');

        if (frequencyId) {
            let response = await fetch(`${window.baseUrl}api/updatefrequency`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json;' },
                body: JSON.stringify({
                    frequencyId: frequencyId,
                    workOrderProd: dataProductions.workOrder,
                    positionWorking: positionWorking,
                    requestType: requestType
                })
            })

            if (!response.ok) {
                const errorResponse = await response.json();
                throw new Error(`${response.status} - ${errorResponse.message}`);
            }

            const data = await response.json();
            console.log(data.message);
            setParsedLocalStorageItem('dataWorkOrderProd', dataProductions);
            if (requestType !== undefined && requestType === "New Conditions Add-On") {
                setParsedLocalStorageItem('readConditionAddOn', true);
            }
        }
        $(this).remove();
    });

    // Xử lý khi modal được hiển thị
    $(MODAL_SELECTOR).on('shown.bs.modal', async function (e) {
        e.preventDefault();
        let checksheetId = $('#checksheetCodeCondition').attr('data-checksheetid');
        let checksheetVersionId = $('#checksheetCodeCondition').attr('data-checksheetversionid');
        let dataItem = getParsedLocalStorageItem('dataWorkOrderProd', {});

        fetch(`${window.baseUrl}api/GetFormConfig`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;'
            },
            body: JSON.stringify({
                checksheetVerId: checksheetVersionId,
                productCode: dataItem.productCode,
                productLot: dataItem.lotNo,
                workOrder: dataItem.workOrder,
                positionWorking: positionWorking,
                formType: 'form-info,form-enter-condition',
            })
        })
            .then(async response => {
                if (!response.ok) {
                    const errorResponse = await response.json();
                    throw new Error(`${response.status} - ${errorResponse.message}`);
                }
                return response.json();
            })
            .then(data => {
                let formConfigs = data.formFields;
                let dataBlinds = data.dataBlinds;
                let modeForm = "";
                if (getParsedLocalStorageItem('readConditionAddOn')) {
                    modeForm = "supplementary-condition";
                }
                let infoErrorTwisted = getParsedLocalStorageItem('errorTwisted', []);
                let errorLabel = "";
                if (infoErrorTwisted.length > 0) {
                    infoErrorTwisted.forEach(item => {
                        errorLabel = item.label;
                    })
                }
                const formConditions = $('#formConditions');
                formConditions.empty();
                formConfigs.forEach(formConfig => {
                    renderForm(formConditions, formConfig, modeForm, positionWorking, dataBlinds);
                    applyConditions(formConfig, dataItem.productCode, modeForm, "", errorLabel);
                });
            })
            .catch(error => {
                console.log(`Lỗi: ${error}`);
            })
    });

    // Xử lý khi click vào nút đóng modal
    $('body').on('click', `${MODAL_SELECTOR} .btn-close`, function (e) {
        e.preventDefault();
        swal({
            title: 'Bạn chắc chắn muốn đóng thao tác?',
            text: 'Bạn không nên đóng thao tác khi đang thực hiện thao tác. Điều đó gây ra thiếu dữ liệu. Trân trọng!',
            icon: 'warning',
            buttons: ["Không", "Có"],
        }).then((isConfirmed) => {
            if (isConfirmed) {
                if (getParsedLocalStorageItem('rulerCode')) {
                    alert("Chưa nhập thước vạch. Vui lòng nhập điều kiện đó để không bị mất dữ liệu");
                    return;
                } else {
                    window.location.reload();
                }
            }
        });
    });

    // Lưu lại thông tin điều kiện đã đọc trước đó
    $('body').on('click', BTN_CONFIRM_SELECTOR, async function (e) {
        e.preventDefault();

        let dataCondition = [];
        let formDataMapping = [];

        let requestType = $(this).attr('data-typeaction');
        $('#formConditions').find('.section').not('.d-none').find('.render-item').each(function (e) {
            if ($(this).css('display') !== 'none') {
                let input = $(this).find('input');
                input.each(function (e) {
                    let objFields = {};
                    if ($(this).val() != '') {
                        let inputId = $(this).attr('id');
                        let labelText = inputId
                            ? $(`label[for="${inputId}"]`).text().trim()
                            : $(this).closest('label').text().trim();
                        if (labelText.endsWith(":")) {
                            labelText = labelText.slice(0, -1); // Xóa ký tự cuối
                        }
                        objFields.label = labelText;
                        objFields.fieldName = $(this).attr('data-fieldname');
                        objFields.value = $(this).val();
                        dataCondition.push(objFields);
                    }
                });
            }
        });

        $(`#formConditions .render-item input`).each(function (i, elem) {
            let objFieldMapping = {};
            objFieldMapping.formId = $(elem).parent().parent().parent().parent().attr('data-formid');
            objFieldMapping.fieldName = $(elem).attr('data-fieldname');
            objFieldMapping.value = $(elem).val();
            formDataMapping.push(objFieldMapping);
        });

        let checksheetVersionId = $(`${MODAL_SELECTOR} .checksheet-condition`).attr('data-checksheetversionid');
        const mergedInfoProduction = {
            formData: formDataMapping,
        };

        const dataItem = getParsedLocalStorageItem('dataWorkOrderProd', {});
        let bodyItem = JSON.stringify({
            checksheetVersionId: checksheetVersionId,
            checkConditions: JSON.stringify(dataCondition),
            dataSaveMapping: JSON.stringify(mergedInfoProduction),
            positionWorking: positionWorking,
            workOrderProd: dataItem.workOrder,
            requestType: requestType,
            itemAssignmentId: $('#itemAssignmentId').val(),
        });
        try {
            const response = await fetch(`${window.baseUrl}api/CheckConditionPreOperation`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: bodyItem
            });

            if (!response.ok) {
                const errorResponse = await response.json();
                throw new Error(`${response.status} - ${errorResponse.message}`);
            }

            const data = await response.json();

            if (data.status) {
                swal({
                    title: 'Thành công',
                    text: data.message,
                    icon: 'success',
                    buttons: [false, "Ok"],
                }).then((isConfirmed) => {
                    if (isConfirmed) {
                        window.location.reload();
                        if (getParsedLocalStorageItem('readConditionAddOn')) {
                            localStorage.removeItem('readConditionAddOn');
                        }
                    }
                });
            } else {
                showAlert('Lỗi!', data.message, 'error', [false, "Ok"])
                    .then((isConfirmed) => {
                        if (isConfirmed) {
                            $(`${MODAL_SELECTOR} .render-item`).each(function () {
                                if ($(this).find('label').text().trim() == data.labelText) {
                                    $(this).find('input').addClass('border-danger');
                                }
                            });
                        }
                    });
            }
        } catch (error) {
            showAlert('Lỗi!', error.message, 'error', [false, "Ok"])
                .then((isConfirmed) => {
                    if (isConfirmed) {
                        window.location.reload();
                    }
                });
        }
    });
}
function processReadMaterialQR(positionWorking) {
    const PreCheckConfig = {
        MATERIAL_DATA_KEY: 'dataMaterialProd',
        SPECIAL_CHARS_REGEX: /[!@#$%^&*(),.?":{}|<>]/g,
        BASE_URL: window.baseUrl || ''
    };
    const SelectorsPreCheck = {
        READ_LOT_MATERIALS_MODAL: '#readLotMaterials',

        QR_MATERIAL_VALUE: '#qrMaterialValue',
        BTN_READ_MATERIALS: '#btnReadMaterials',
        BTN_COUNT_QR: '.btn-count-qr',

        QTY_INPUT_TRAY_KTT: '#readLotMaterials #qtyMaterialEntered',
        TRAY_NUMBER_KTT: '#readLotMaterials #trayNo',
        TIME_LIMIT_MATERIAL: '#readLotMaterials #timeLimit',
        LOT_MATERIAL: '#readLotMaterials #lotMaterial',

        OUTER_DIAMETER_MM_STD_KTT: '#stdOuterDiameterMM',
        OUTER_DIAMETER_INCH_STD_KTT: '#stdOuterDiameterInch',

        POUCH_NO_KTT_PREFIX: '#pouchNo_',
        OUTER_DIAMETER_POUCH_KTT_PREFIX: '#outerDiameterPouch_'
    };

    let countTray = 1;
    let countReadQr = 0;

    $('.checkAbnormal').prop('checked', false);

    // --- Xử lý Sự Kiện ---
    // 1. Xử lý sau khi modal đọc mã NVL được hiển thị
    $('body').on('shown.bs.modal', SelectorsPreCheck.READ_LOT_MATERIALS_MODAL, function (e) {
        $(SelectorsPreCheck.QR_MATERIAL_VALUE).val('').focus();
        $(SelectorsPreCheck.INFO_MATERIALS).html('');
        $('#btnConfirmWireHasBeenWorked').addClass('disabled');

        let dataMaterialProd = getParsedLocalStorageItem(PreCheckConfig.MATERIAL_DATA_KEY, []);
        let dataTrayCurrent = getParsedLocalStorageItem('dataTrayCurrent', []);
        if (dataMaterialProd.listPouchs != undefined && dataMaterialProd.listPouchs.length > 0) {
            countReadQr = dataMaterialProd.listPouchs.length;
        } else {
            countReadQr = 0;
        }

        if (dataMaterialProd.trayNo != undefined && dataMaterialProd.trayNo != '') {
            let trayNo = dataMaterialProd.trayNo.split('-');
            countTray += parseInt(trayNo[1], 10);
        } else {
            countTray = 1;
        }

        if (dataTrayCurrent != undefined && dataTrayCurrent.trayNo) {
            let trayNo = dataTrayCurrent.trayNo.split('-');
            countTray = parseInt(trayNo[1], 10) + 1;
        } else {
            countTray = 1;
        }

        if (dataTrayCurrent && dataMaterialProd.listPouchs != undefined && dataMaterialProd.listPouchs.length <= 0) {
            localStorage.removeItem(PreCheckConfig.MATERIAL_DATA_KEY);
        }

    });

    // 2. Xử lý đóng modal đọc mã NVL
    $('body').on('click', `${SelectorsPreCheck.READ_LOT_MATERIALS_MODAL} .btn-close`, function (e) {
        e.preventDefault();
        showAlert(
            'Bạn chắc chắn muốn đóng thao tác?',
            'Bạn không nên đóng thao tác khi đang thực hiện thao tác. Điều đó gây ra thiếu dữ liệu. Trân trọng!',
            'warning'
        ).then((isConfirmed) => {
            if (isConfirmed) {
                $(SelectorsPreCheck.READ_LOT_MATERIALS_MODAL).modal('hide');
                $(SelectorsPreCheck.BTN_READ_MATERIALS).removeClass('d-none');
                if (dataMaterialProd.listPouchs != undefined && dataMaterialProd.listPouchs.length > 0) {
                    countReadQr = dataMaterialProd.listPouchs.length;
                } else {
                    countReadQr = 0;
                }
            } else {
                $(SelectorsPreCheck.QR_MATERIAL_VALUE).focus();
            }
        });
    });

    // 3. Xử lý mở lại modal đọc mã NVL
    $('body').on('click', `.content-read-workorder ${SelectorsPreCheck.BTN_READ_MATERIALS}`, function () {
        $(SelectorsPreCheck.READ_LOT_MATERIALS_MODAL).modal('show');
        $(this).addClass('d-none');
    });

    // 4. Xử lý sự kiện nhấn Enter trên ô nhập mã NVL
    $('body').on('keypress', SelectorsPreCheck.QR_MATERIAL_VALUE, function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $(SelectorsPreCheck.BTN_COUNT_QR).trigger('click');
        }
    });

    // 5. Xử lý chuyển đổi QR mã NVL
    $('body').on('click', SelectorsPreCheck.BTN_COUNT_QR, async function () {
        let valueScaned = $(SelectorsPreCheck.QR_MATERIAL_VALUE).val().toUpperCase();
        if (valueScaned.length == 0) {
            return;
        }

        let checksheetId = $('#checksheetCodePreCheck').attr('data-checksheetid');
        let checksheetVersionId = $('#checksheetCodePreCheck').attr('data-checksheetversionid');

        // Kiểm tra xem đã đủ 5 pouch chưa trước khi đọc QR mới
        let currentMaterialData = getParsedLocalStorageItem(PreCheckConfig.MATERIAL_DATA_KEY, {});
        if (currentMaterialData.listPouchs != undefined && currentMaterialData.listPouchs.length >= 5) {
            await showAlert('Thông báo', 'Máng hiện tại đã đủ 5 pouch. Bạn vui lòng nhập máng mới.', 'info', [false, 'Nhập mới'])
                .then((isConfirmed) => {
                    if (isConfirmed) {
                        $(SelectorsPreCheck.QR_MATERIAL_VALUE).val('');
                        $('#btnConfirmWireHasBeenWorked').removeClass('disabled');
                    }
                });
            return;
        }

        $(SelectorsPreCheck.QR_MATERIAL_VALUE).val(valueScaned);
        countReadQr++;

        const arrValScaned = valueScaned.split(PreCheckConfig.SPECIAL_CHARS_REGEX);

        const isValidFormat = arrValScaned[0]?.length >= 11 &&
            arrValScaned[1]?.length >= 2 &&
            arrValScaned[2]?.length >= 6 &&
            arrValScaned[3]?.length >= 1 &&
            arrValScaned[4]?.length >= 6;
        if (!isValidFormat) {
            await showAlert('Lỗi!', 'Định dạng mã nguyên vật liệu không đúng. Vui lòng kiểm tra lại', 'error', [false, "Nhập lại"])
                .then((isConfirmed) => {
                    if (isConfirmed) {
                        $(SelectorsPreCheck.QR_MATERIAL_VALUE).removeClass('disabled').val('').focus();
                        countReadQr--;
                    }
                });
            return;
        }
        let [materialCode, qtyReadStr, lotMaterial, pouchNo, timeLimit] = arrValScaned;
        var qtyRead = parseInt(qtyReadStr, 10);
        var typeMaterial = materialCode.endsWith('Y') ? 'Dây dẫn TYC' : 'Dây dẫn thường';

        var [positionPrefix] = positionWorking.split('-');
        var currentTrayNo = `${positionPrefix}-${countTray}`;

        let materialObj = {
            materialCode: materialCode,
            qty: 0,
            qtyReadQR: qtyRead,
            lotMaterial: lotMaterial,
            pouchNo: pouchNo,
            listPouchs: [],
            timeLimit: timeLimit,
            typeMaterial: typeMaterial,
            trayNo: "",
        };
        $('#btnConfirmWireHasBeenWorked').addClass('disabled');

        var dataItem = getParsedLocalStorageItem('dataWorkOrderProd', {});
        let jsonRequest = {
            workOrder: dataItem.workOrder,
            timeLimit: timeLimit,
            lotMaterial: lotMaterial,
            materialCode: materialCode,
        };
        if (materialCode.endsWith('Y')) {
            jsonRequest = {
                workOrder: dataItem.workOrder,
                timeLimit: timeLimit,
                lotMaterial: lotMaterial,
                pouchNo: pouchNo,
                materialCode: materialCode,
            };
        }
        $(SelectorsPreCheck.QR_MATERIAL_VALUE).blur().addClass('disabled');
        try {
            const response = await fetch(`${PreCheckConfig.BASE_URL}api/getQtyInline`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json;' },
                body: JSON.stringify(jsonRequest)
            });

            if (!response.ok) {
                const errorResponse = await response.json();
                throw new Error(`${response.status} - ${errorResponse.message}`);
            }
            const apiData = await response.json();

            var existingMaterialData = getParsedLocalStorageItem(PreCheckConfig.MATERIAL_DATA_KEY);

            if (materialCode.endsWith('Y') && existingMaterialData != undefined && existingMaterialData.listPouchs != undefined &&
                existingMaterialData.listPouchs.length > 0 &&
                existingMaterialData.listPouchs.find(item => item.pouchNo === pouchNo) &&
                existingMaterialData.lotMaterial === lotMaterial) {

                await showAlert('Cảnh báo!', 'Đã có pouch trước đó, vui lòng thử lại!', 'warning', [false, "Nhập lại"])
                    .then((isConfirmed) => {
                        if (isConfirmed) {
                            $(SelectorsPreCheck.QR_MATERIAL_VALUE).removeClass('disabled').val('').focus();
                            countReadQr--;
                        }
                    });
                return;
            }

            if (apiData.renderData && apiData.renderData.length > 0) {
                let qtyCheckLine = 0;

                const lineMapping = { 'I': 'line1', 'II': 'line2', 'III': 'line3', 'IV': 'line4', 'V': 'line5' };
                qtyCheckLine = apiData.renderData[0][lineMapping[dataItem.line]] || 0;

                if (qtyCheckLine === 0) {
                    await showAlert('Lỗi!', 'Mã nguyên vật liệu này không được sản xuất trên line này!', 'error', [false, "Nhập lại"])
                        .then((isConfirmed) => {
                            if (isConfirmed) {
                                $(SelectorsPreCheck.QR_MATERIAL_VALUE).removeClass('disabled').val('').focus();
                            }
                        });
                    return;
                } else {
                    materialObj.pouchNo = pouchNo;
                    if (existingMaterialData) {
                        if (existingMaterialData.materialCode === materialCode && existingMaterialData.lotMaterial === lotMaterial) {
                            if (existingMaterialData.totalQtyHasRead >= qtyCheckLine) {
                                showAlert('Thông báo', 'Đã nhập đủ số lượng đã chia của NVL cho line ' + dataItem.line + '. Vui lòng nhập lô NVL mới.', 'warning', [false, "Nhập mới"])
                                    .then((isConfirmed) => {
                                        $(SelectorsPreCheck.QR_MATERIAL_VALUE).removeClass('disabled').val('').focus();
                                        countReadQr--;
                                        //localStorage.removeItem(PreCheckConfig.MATERIAL_DATA_KEY);
                                    });
                                return;
                            } else {
                                existingMaterialData.qtyReadQR = qtyRead;
                                existingMaterialData.pouchNo = pouchNo;
                                setParsedLocalStorageItem(PreCheckConfig.MATERIAL_DATA_KEY, existingMaterialData);
                            }
                        } else {
                            // Yêu cầu xác nhận khi đổi mã NVL
                            let confirmChangeMaterial = await showAlert('Thông báo', 'Bạn muốn thực hiện một mã khác. ' +
                                'Nếu chọn "Có" sẽ đọc mã NVL mới. ' +
                                'Nếu chọn "Không" thì tiếp tục nhập mã NVL trước đó. Trân trọng!', 'warning');
                            if (confirmChangeMaterial) {
                                $('#btnConfirmWireHasBeenWorked').addClass('disabled');
                                materialObj.qty = qtyRead;
                                setParsedLocalStorageItem(PreCheckConfig.MATERIAL_DATA_KEY, materialObj);
                            } else {
                                $(SelectorsPreCheck.QR_MATERIAL_VALUE).removeClass('disabled').val('').focus();
                                countReadQr--;
                                return;
                            }
                        }
                    } else {
                        materialObj.qty = qtyRead;
                        setParsedLocalStorageItem(PreCheckConfig.MATERIAL_DATA_KEY, materialObj);
                    }

                    fetch(`${window.baseUrl}api/GetFormConfig`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json;'
                        },
                        body: JSON.stringify({
                            checksheetVerId: checksheetVersionId,
                            productCode: dataItem.productCode,
                            productLot: dataItem.lotNo,
                            workOrder: dataItem.workOrder,
                            positionWorking: positionWorking,
                            formType: 'form-info,form-enter-info',
                        })
                    })
                        .then(async response => {
                            if (!response.ok) {
                                const errorResponse = await response.json();
                                throw new Error(`${response.status} - ${errorResponse.message}`);
                            }
                            return response.json();
                        })
                        .then(data => {
                            let formConfigs = data.formFields;
                            let dataBlinds = data.dataBlinds;
                            let modeForm = "";
                            const formConditions = $('#formReadMaterials');
                            formConditions.empty();
                            formConfigs.forEach(formConfig => {
                                renderForm(formConditions, formConfig, modeForm, positionWorking, dataBlinds);
                                applyConditions(formConfig, dataItem.productCode, modeForm, countReadQr, "");
                            });
                            $(`${SelectorsPreCheck.LOT_MATERIAL}`).val(lotMaterial);
                            $(`${SelectorsPreCheck.TIME_LIMIT_MATERIAL}`).val(timeLimit);
                            $(`${SelectorsPreCheck.TRAY_NUMBER_KTT}`).val(currentTrayNo);

                            // Lấy lại dữ liệu vật liệu sau khi đã cập nhật localStorage
                            existingMaterialData = getParsedLocalStorageItem(PreCheckConfig.MATERIAL_DATA_KEY);

                            const totalQtyReadForMaterial = (existingMaterialData != undefined && existingMaterialData.listPouchs != undefined && existingMaterialData.listPouchs.length > 0 ? existingMaterialData.listPouchs.reduce((sum, p) => sum + p.qtyPouch, 0) + qtyRead : qtyRead) ?? 0;
                            $(`${SelectorsPreCheck.QTY_INPUT_TRAY_KTT}`).val(totalQtyReadForMaterial);

                            // Cập nhật các trường hiển thị cho pouch hiện tại
                            $(`${SelectorsPreCheck.POUCH_NO_KTT_PREFIX}${countReadQr}`).val(pouchNo);

                            const screenWidth = window.innerWidth;
                            // Desktop bé
                            if (screenWidth == 1280) {
                                $('#readLotMaterials').removeClass('modal-push-up');
                            }
                            // Tablet samsung
                            if (screenWidth == 1317) {
                                $('#readLotMaterials').addClass('modal-push-up');

                                $(document).on('focus', `${SelectorsPreCheck.OUTER_DIAMETER_POUCH_KTT_PREFIX}${countReadQr}`, function () {
                                    $('#readLotMaterials').addClass('modal-push-up');
                                });
                                $(document).on('blur', `${SelectorsPreCheck.OUTER_DIAMETER_POUCH_KTT_PREFIX}${countReadQr}`, function () {
                                    $('#readLotMaterials').removeClass('modal-push-up');
                                });
                            }

                            // Kiểm tra số lượng đã đọc so với số lượng trên line
                            if (qtyCheckLine < totalQtyReadForMaterial) {
                                showAlert('Thông báo', 'Số lượng của mã NVL đã đủ trên line. Vui lòng thực hiện mã khác', 'warning', [false, 'Tiếp tục']);
                                $(SelectorsPreCheck.QR_MATERIAL_VALUE).removeClass('disabled').val('').focus();
                                return;
                            }
                        })
                        .catch(error => {
                            console.log(`Lỗi: ${error}`);
                        })
                }
            } else {
                await showAlert('Lỗi!', 'Nguyên vật liệu không đúng. Vui lòng kiểm tra lại!', 'error', [false, 'Nhập lại']);
                localStorage.removeItem(PreCheckConfig.MATERIAL_DATA_KEY);
                $(SelectorsPreCheck.QR_MATERIAL_VALUE).removeClass('disabled').val('').focus();
                countReadQr--;
            }

        } catch (error) {
            console.error(error);
            swal('Lỗi!', error.message || 'Có lỗi xảy ra khi lấy dữ liệu từ server.', 'error');
            localStorage.removeItem(PreCheckConfig.MATERIAL_DATA_KEY);
            $(SelectorsPreCheck.QR_MATERIAL_VALUE).val('').focus();
            countReadQr--;
        }
    });

    $('body').on('click', '#btnConfirmWireHasBeenWorked', function (e) {
        e.preventDefault();

        const currentMaterialData = getParsedLocalStorageItem('dataMaterialProd');
        if (!currentMaterialData || currentMaterialData.listPouchs.length === 0) {
            swal('Cảnh báo!', 'Chưa có pouch nào được nhập. Vui lòng nhập nguyên vật liệu trước.', 'warning');
            return;
        }

        if (currentMaterialData.listPouchs.length === 5) {
            saveAndProceed();
        }

        swal({
            title: 'Thông báo',
            text: 'Bạn đã nhập đủ số lượng Nguyên Vật Liệu chưa? Nhấn "Đã đủ" để lưu và tiếp tục, hoặc "Tiếp tục nhập" để nhập thêm.',
            icon: 'warning',
            buttons: ["Tiếp tục nhập", "Đã đủ"],
        }).then((isConfirmed) => {
            if (isConfirmed) {
                saveAndProceed();
            } else {
                $(SelectorsPreCheck.QR_MATERIAL_VALUE).removeClass('disabled').val('').focus(); // Cho phép nhập QR tiếp
                $(this).addClass('disabled');
            }
        });
    });

    // --- Hàm xử lý lưu và chuyển thao tác ---
    const saveAndProceed = async () => {
        let dataMaterialProd = getParsedLocalStorageItem('dataMaterialProd');
        if (!dataMaterialProd) {
            swal('Lỗi!', 'Không có dữ liệu nguyên vật liệu để lưu. Vui lòng nhập liệu trước.', 'error');
            return;
        }

        let qtyHasRead = 0;
        dataMaterialProd.listPouchs.forEach(item => {
            qtyHasRead += item.qtyPouch;
        });

        let checksheetVersionId = $('#checksheetCodePreCheck').attr('data-checksheetversionid');

        let arrDataPouchSavedExcel = getParsedLocalStorageItem('INFO_OUTERDIAMETERS_SAVING') || [];
        let formDataMapping = [];

        arrDataPouchSavedExcel.flat().forEach(newItem => {
            let existingItem = formDataMapping.find(item => item.fieldName === newItem.fieldName);
            if (existingItem) {
                if (newItem.value !== "" && newItem.value !== null && newItem.value !== undefined) {
                    existingItem.value = newItem.value;
                }
            } else {
                formDataMapping.push(newItem);
            }
        });
        let mergedInfoProduction = {
            formData: formDataMapping,
        };
        let bodyItem = JSON.stringify({
            checksheetVersionId: checksheetVersionId,
            jsonSaveDb: JSON.stringify(mergedInfoProduction),
            positionWorking: positionWorking,
            workOrderProd: $('#workOrderProd').val(),
            itemAssignmentId: $('#itemAssignmentId').val(),
            qtyOfReads: qtyHasRead,
            trayNo: dataMaterialProd.trayNo,
        });
        // Lưu dữ liệu lên database
        try {
            const response = await fetch(`${PreCheckConfig.BASE_URL}api/savetrayprecheck`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json;'
                },
                body: bodyItem
            });

            if (!response.ok) {
                const errorResponse = await response.json();
                throw new Error(`${response.status} - ${errorResponse.message}`);
            }
            const data = await response.json();

            if (data.status) {
                showAlert('Thành công!', 'Dữ liệu đã được lưu và chuyển thao tác.', 'success', [false, 'Ok']).then((isConfirmed) => {
                    if (isConfirmed) {
                        $(SelectorsPreCheck.READ_LOT_MATERIALS_MODAL).modal('hide');
                        $('#confirmWireHasBeenWorked').modal('show');
                        $('#confirmWireHasBeenWorked').attr('data-trayno', dataMaterialProd.trayNo);
                        $('#confirmWireHasBeenWorked').attr('data-entryid', data.formEntryId);
                        countReadQr = 0;
                        countTray++;
                        setParsedLocalStorageItem("INFO_OUTERDIAMETERS_SAVING", formDataMapping);
                    }
                });
            } else {
                swal('Lỗi!', data.message, 'error');
            }
        } catch (error) {
            console.error(error);
            swal('Lỗi!', error.message || error, 'error');
        }
    };

    // Hiển thị form nhập lý do cho người dùng nhập lý do bất thường. Lưu tất các các thông tin đã nhập trước đấy.
    $('body').on('change', '.checkAbnormal', function (e) {
        e.preventDefault();
        $('.btn-pause-abnormal').toggleClass('d-none');
        $('#btnConfirmWireHasBeenWorked').addClass('disabled');
    });

    $('.btn-pause-abnormal').on('click', async function (e) {
        e.preventDefault();
        showAlert('Thông báo', 'Bạn xác nhận có bất thường? Nếu không có vui lòng chọn "Không" để tiếp tục thực hiện. Xin cảm ơn.', 'warning', ['Không', 'Có'])
            .then(async (isConfirmed) => {
                if (isConfirmed) {
                    $('#readLotMaterials').modal('hide');
                    $('#enterSuccessErrorTray').modal('hide');
                    $('#confirmWireHasBeenWorked').modal('hide');

                    let checksheetId = $('#checksheetCodePreCheck').attr('data-checksheetid');
                    let checksheetVersionId = $('#checksheetCodePreCheck').attr('data-checksheetversionid');

                    // Lưu thông tin đã nhập khi có bất thường
                    let formDataMapping = [];

                    let arrDataPouchSavedExcel = getParsedLocalStorageItem('INFO_OUTERDIAMETERS_SAVING', []);
                    if (arrDataPouchSavedExcel.length > 0) {
                        arrDataPouchSavedExcel.flat().forEach(newItem => {
                            let existingItem = formDataMapping.find(item => item.fieldName === newItem.fieldName);
                            if (existingItem) {
                                if (newItem.value !== "" && newItem.value !== null && newItem.value !== undefined) {
                                    existingItem.value = newItem.value;
                                }
                            } else {
                                formDataMapping.push(newItem);
                            }
                        });
                    }

                    let mergedInfoProduction = {
                        formData: formDataMapping,
                    };

                    let errorPouchs = getParsedLocalStorageItem('errorPouchs') || [];

                    let otherErrors = getParsedLocalStorageItem('errorChildOthers', []);
                    let basicErrors = getParsedLocalStorageItem('errorDataBasic', []);
                    let specialErrors = getParsedLocalStorageItem('errorTwisted', []);
                    let mergedErrors = basicErrors.concat(otherErrors, specialErrors);

                    if (errorPouchs.length > 0) {
                        mergedErrors = errorPouchs;
                    }

                    let dataMaterials = getParsedLocalStorageItem(PreCheckConfig.MATERIAL_DATA_KEY, {});

                    let formEntryId = $('#confirmWireHasBeenWorked').attr('data-entryid');
                    let bodyItem = JSON.stringify({
                        checksheetVersionId: checksheetVersionId,
                        jsonSaveDb: formDataMapping.length > 0 ? JSON.stringify(mergedInfoProduction) : null,
                        positionWorking: positionWorking,
                        workOrderProd: $('#workOrderProd').val(),
                        itemAssignmentId: $('#itemAssignmentId').val(),
                        trayNo: dataMaterials.trayNo,
                        qtyOfReads: dataMaterials.qty,
                        qtyProcessing: parseInt($('#confirmWireHasBeenWorked #qtyProcessingRealTime').val(), 10),
                        qtyOK: parseInt($('#suitableQuantityKTT').val(), 10),
                        qtyNG: parseInt($('#nonConforminItemsKTT').val(), 10),
                        errorInfo: mergedErrors.length > 0 ? JSON.stringify(mergedErrors) : null,
                        formEntryId: formEntryId
                    });

                    try {
                        const response = await fetch(`${PreCheckConfig.BASE_URL}api/saveabnormal`, {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json;'
                            },
                            body: bodyItem
                        });

                        if (!response.ok) {
                            const errorResponse = await response.json();
                            throw new Error(`${response.status} - ${errorResponse.message}`);
                        }
                        const data = await response.json();

                        if (data.status) {
                            $('#alertPauseOperation').modal('show');
                            $('#modalConfirmReasonAbnormal').modal('hide');
                            $('#leaderComfirmedAbnormal').attr('data-checksheetverid', checksheetVersionId);
                            localStorage.removeItem('INFO_OUTERDIAMETERS_SAVING');
                            localStorage.removeItem(PreCheckConfig.MATERIAL_DATA_KEY);
                            localStorage.removeItem('leaderConfirm');
                            localStorage.removeItem('errorPouchs');
                        } else {
                            swal('Lỗi!', data.message, 'error');
                        }
                    } catch (error) {
                        console.error(error);
                        swal('Lỗi!', error.message || error, 'error');
                    }
                } else {
                    $('.btn-pause-abnormal').addClass('d-none');
                    $('#qrMaterialValue').focus();
                    $('.checkAbnormal').prop('checked', false);
                    return;
                }
            })
    });

    $('body').on('shown.bs.modal', '#leaderComfirmedAbnormal', function () {
        $('#leaderEmployeeNo').focus();
        $('#leaderComfirmedAbnormalLabel').html('Leader xác nhận bất thường');
        // Gọi form note
        fetch(`${window.baseUrl}api/getFormNote`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;'
            },
            body: JSON.stringify({
                positionWorking: positionWorking
            })
        })
            .then(async response => {
                if (!response.ok) {
                    const errorResponse = await response.json();
                    throw new Error(`${errorResponse.message}`);
                }
                return response.json();
            })
            .then(data => {
                let formConfigs = data.formField;
                const formConditions = $('#leaderConfirmReason');
                formConfigs.forEach(formConfig => {
                    renderForm(formConditions, formConfig, '', positionWorking, []);
                    applyConditions(formConfig, '', '', '', '');
                    $('#leaderComfirmedAbnormal').attr('data-checksheetverid', formConfig.checksheetVersionId);
                });
               
            })
            .catch(error => {
                console.log(error);
            })
    });
    // Sau đó sẽ phải đợi Leader đến xác nhận lý do và tạm dừng chuyển sang lô khác.
    $('#leaderConfirmReason').on('change', 'textarea#remarkOperations', function (e) {
        e.preventDefault();
        $('#btnPauseAndNewLot').toggleClass('disabled');
    });
    $('#btnPauseAndNewLot').on('click', function (e) {
        e.preventDefault();

        let checksheetVersionId = $('#leaderComfirmedAbnormal').attr('data-checksheetverid');
        let formDataMapping = [];

        $(`#leaderComfirmedAbnormal #leaderConfirmReason textarea`).each(function (i, elem) {
            let objFieldMapping = {};
            objFieldMapping.formId = $(elem).parent().parent().parent().parent().attr('data-formid');
            objFieldMapping.fieldName = $(elem).attr('data-fieldname');
            objFieldMapping.value = $(elem).val();
            formDataMapping.push(objFieldMapping);
        });

        let infoNoteOperations = {
            formData: formDataMapping,
        };

        fetch(`${window.baseUrl}api/leadercheck`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;',
            },
            body: JSON.stringify({
                positionWorking: positionWorking,
                workOrderProd: $('#workOrderProd').val(),
                leaderEmployeeNo: $('#leaderEmployeeNo').val(),
                leaderPassworkLv2: $('#passwordLeaderConfirmedLv2').val(),
                reasonLeaderConfirm: $('#remarkOperations').val(),
                checksheetVersionId: checksheetVersionId,
                jsonValueNote: JSON.stringify(infoNoteOperations),
                itemAssignmentId: $('#itemAssignmentId').val(),
            })
        })
            .then(async response => {
                if (!response.ok) {
                    const errorResponse = await response.json();
                    throw new Error(`${response.status} - ${errorResponse.message}`);
                }
                return response.json();
            })
            .then(data => {
                showAlert('Thông báo', data.message, 'info', [false, "Nhập chỉ thị mới"])
                    .then((isConfirmed) => {
                        if (isConfirmed) {
                            window.location.reload();
                        }
                    });
            })
            .catch(error => {
                showAlert('Lỗi', error.message, 'error', [false, "Nhập lại"])
                    .then((isConfirmed) => {
                        if (isConfirmed) {
                            $('.enter-password-leader input').each(function (e) {
                                $(this).val('');
                            });
                        }
                    });
            })
    });

    //================================ Xử lý xác nhận đã thao tác với dây dẫn ================================
    // 1. Tắt xác nhận thao tác
    $('body').on('click', '#confirmWireHasBeenWorked .btn-close', function (e) {
        e.preventDefault();
        swal({
            title: 'Bạn chắc chắn muốn đóng?',
            text: '',
            icon: 'warning',
            buttons: ["Không", "Có"],
        }).then((isConfirmed) => {
            if (isConfirmed) {
                window.location.reload();
            } else {
                return;
            }
        });
    });
    // 2. Mở lại xác nhận thao tác
    $('body').on('click', '.btn-confirm-operations', function (e) {
        $(this).addClass('d-none');
        $('#confirmWireHasBeenWorked').modal('show');
        $('#confirmWireHasBeenWorked').attr('data-trayno', $(this).attr('data-trayno'));
        let jsonSavedTray = $(this).parent().find('.jsonDataSaved').val();
        if ($('#confirmWireHasBeenWorked #jsonDataSaved').length == 0 && jsonSavedTray != '') {
            $('#confirmWireHasBeenWorked').append(`<input type="hidden" id="jsonDataSaved" value='${jsonSavedTray}' />`);
        }
    });
    // 3. Xử lý khi tick vào xác nhận đã thao tác
    $('body').on('shown.bs.modal', '#confirmWireHasBeenWorked', function (e) {
        e.preventDefault();

        // Reset trạng thái ban đầu của modal
        $('#confirmWireHasBeenWorked #qtyProcessingRealTime').val('');
        $('#confirmHasWorkedValue').prop('checked', false);
        $('#confirmHasWorkedValue').prop('checked', false);

        // Tắt checkbox và hiển thị thông báo chờ
        $('#countdownMessage').remove(); // Xóa thông báo cũ nếu có
        $('#confirmWireHasBeenWorked .enter-qty-processing').addClass('d-none');
        $('.confirm-operation').removeClass('d-none');

        //let dataWorkOrder = getParsedLocalStorageItem('dataWorkOrderProd', {});

        //let waitTime = 300;
        //if (dataWorkOrder.productCode.includes("A")) {
        //    waitTime = 1800;
        //}

        //$('#confirmWireHasBeenWorked .modal-body').prepend(`<p id="countdownMessage" class="text-danger">Bạn có ${waitTime / 60} phút để xác nhận chủng loại <strong>${dataWorkOrder.productCode}</strong>. Thời gian còn lại: <span id="countdownTimer"></span></p>`);

        //let timer = waitTime;
        //const countdownEl = $('#countdownTimer');

        //const countdownInterval = setInterval(function () {
        //    const minutes = Math.floor(timer / 60);
        //    const seconds = timer % 60;
        //    countdownEl.text(`${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`);

        //    if (timer <= 0) {
        //        clearInterval(countdownInterval);
        //        $('#countdownMessage').text(`Đã quá ${waitTime / 60} phút. Bây giờ bạn có thể xác nhận.`);
        //        setTimeout(function () {
        //            if (!$('#confirmHasWorkedValue').is(':checked')) {
        //                alert("Bạn đã không xác nhận sau khi hết thời gian quy định!");
        //            }
        //        }, 10000); // Ví dụ: kiểm tra sau 10 giây
        //    }
        //    timer--;
        //}, 1000);

        $('#confirmHasWorkedValue').on('change', function (e) {
            e.preventDefault();
            if ($(this).is(':checked')) {
                $('#confirmWireHasBeenWorked .enter-qty-processing').removeClass('d-none');
            } else {
                $('#confirmWireHasBeenWorked .enter-qty-processing').addClass('d-none');
            }
        });
        $('#confirmWireHasBeenWorked #qtyProcessingRealTime').on('change', function (e) {
            e.preventDefault();
            $('#confirmWireHasBeenWorked #confirmDataTray').removeClass('disabled');
        });
        let dataMaterialProd = getParsedLocalStorageItem(PreCheckConfig.MATERIAL_DATA_KEY, {});
        $('#confirmWireHasBeenWorked').attr('data-trayno', dataMaterialProd.trayNo);
    });
    // 4. Chuyển sang nhập đạt lỗi sau khi thao tác
    $('body').on('click', '#confirmWireHasBeenWorked #confirmDataTray', function (e) {
        e.preventDefault();
        let dataMaterialProd = getParsedLocalStorageItem(PreCheckConfig.MATERIAL_DATA_KEY, {});
        let qtyProcessing = parseInt($('#confirmWireHasBeenWorked #qtyProcessingRealTime').val(), 10);
        let remainingQty = dataMaterialProd.qty ?? (dataMaterialProd.qty - dataMaterialProd.qtyProduction);

        dataMaterialProd.qtyProduction = parseInt($('#confirmWireHasBeenWorked #qtyProcessingRealTime').val(), 10);
        dataMaterialProd.totalProduction = dataMaterialProd.totalProduction != undefined ? dataMaterialProd.qtyProduction + dataMaterialProd.totalProduction : dataMaterialProd.qtyProduction;

        if (qtyProcessing > remainingQty) {
            showAlert('Thông báo', 'Số lượng thao tác đang lớn hơn. Vui lòng kiểm tra lại.', 'error', [false, 'Nhập lại']);
            return;
        } else if (dataMaterialProd.totalProduction > dataMaterialProd.qty) {
            showAlert('Thông báo', 'Tổng số lượng đã thao tác đang lớn hơn. Vui lòng kiểm tra lại.', [false, 'Nhập lại']);
            return;
        } else {
            setParsedLocalStorageItem(PreCheckConfig.MATERIAL_DATA_KEY, dataMaterialProd);
            if (dataMaterialProd.totalProduction < dataMaterialProd.qty) {
                setParsedLocalStorageItem('PRODUCTION_CONTINUE', true);
            }
            // Chuyển trang thái của workorder đang làm sau khi xác nhận đã thao tác với dây dẫn
            fetch(`${window.baseUrl}api/UpdateStatusWOProcessing`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json;',
                },
                body: JSON.stringify({
                    workOrderProd: $('#workOrderProd').val(),
                    positionWorking: positionWorking,
                    qtyProcessing: qtyProcessing,
                    trayNo: $('#confirmWireHasBeenWorked').attr('data-trayno'),
                })
            })
                .then(async response => {
                    if (!response.ok) {
                        const errorResponse = await response.json();
                        throw new Error(`${response.status} - ${errorResponse.message}`);
                    }
                    return response.json();
                })
                .then(data => {
                    console.log(data.message);
                    $('#enterSuccessErrorTray').modal('show');
                    $('#confirmWireHasBeenWorked').modal('hide');
                    $('#enterSuccessErrorTray').attr('data-qtyproduction', $('#confirmWireHasBeenWorked #qtyProcessingRealTime').val());
                    let jsonSavedTray = $('#confirmWireHasBeenWorked #jsonDataSaved').val() || '';
                    if ($('#enterSuccessErrorTray #jsonDataSaved').length == 0 && jsonSavedTray !== '') {
                        $('#enterSuccessErrorTray').append(`<input type="hidden" id="jsonDataSaved" value='${jsonSavedTray || ''}' />`);
                    }
                })
                .catch(error => {
                    showAlert('Lỗi', error, 'error', [false, "Nhập lại"])
                        .then((isConfirmed) => {
                            if (isConfirmed) {
                                $('#confirmWireHasBeenWorked #qtyProcessingRealTime').val('').focus();
                            }
                        })
                })
        }
    });
    //================================ kết thúc thao tác với NVL ================================
}
function enterMaterialCheckResult(positionWorking) {
    // Tắt nhập lỗi
    $('body').on('click', '#enterSuccessErrorTray .btn-close', function (e) {
        e.preventDefault();
        showAlert('Bạn chắc chắn muốn đóng thao tác?',
            'Bạn không nên đóng thao tác khi đang thực hiện thao tác. Điều đó gây ra thiếu dữ liệu. Trân trọng!',
            'warning'
        ).then((isConfirmed) => {
            if (isConfirmed) {
                window.location.reload();
            } else {
                return;
            }
        });
    });

    // 1. Xử lý hiển thị modal nhập đạt lỗi
    $('body').on('shown.bs.modal', '#enterSuccessErrorTray', async function (e) {
        e.preventDefault();

        let checksheetVerId = $('#checksheetCodePreCheckFinal').attr('data-checksheetversionid');
        let dataItem = getParsedLocalStorageItem('dataWorkOrderProd', {});

        let dataMaterialProd = getParsedLocalStorageItem('dataMaterialProd', {});
        let trayNo = $(this).attr('data-trayno') || dataMaterialProd.trayNo;
        let qtyProduction = dataMaterialProd.qtyProduction;
        let bodyStr = JSON.stringify({
            checksheetVerId: checksheetVerId,
            productCode: dataItem.productCode,
            productLot: dataItem.lotNo,
            workOrder: dataItem.workOrder,
            positionWorking: positionWorking,
            formType: 'form-enter-info,form-enter-results',
        });
        if (!getParsedLocalStorageItem('PRODUCTION_CONTINUE')) {
            bodyStr = JSON.stringify({
                checksheetVerId: checksheetVerId,
                productCode: dataItem.productCode,
                productLot: dataItem.lotNo,
                workOrder: dataItem.workOrder,
                positionWorking: positionWorking,
                formType: 'form-enter-results',
            });
        }
        fetch(`${window.baseUrl}api/GetFormConfig`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;'
            },
            body: bodyStr,
        })
            .then(async response => {
                if (!response.ok) {
                    const errorResponse = await response.json();
                    throw new Error(`${response.status} - ${errorResponse.message}`);
                }
                return response.json();
            })
            .then(data => {
                let formConfigs = data.formFields;
                let dataBlinds = data.dataBlinds;
                let modeForm = "";
                const formEnterResults = $('#formEnterResults .error-container');
                let infoErrorTwisted = getParsedLocalStorageItem('errorTwisted', []);
                let errorLabel = infoErrorTwisted.length > 0 ? infoErrorTwisted[0].label : '';
              
                formEnterResults.empty();
                formConfigs.forEach(formConfig => {
                    renderForm(formEnterResults, formConfig, modeForm, positionWorking, dataBlinds);
                    applyConditions(formConfig, dataItem.productCode, modeForm, "", errorLabel);
                });
 
                $(`#enterSuccessErrorTray #lotMaterial`).val(dataMaterialProd.lotMaterial);
                $(`#enterSuccessErrorTray #timeLimit`).val(dataMaterialProd.timeLimit);
                $(`#enterSuccessErrorTray #trayNo`).val(dataMaterialProd.trayNo);

                $('#infoMaterials').addClass('d-none');
                $('#enterSuccessErrorTray #enterValueLabel').html('Nhập kết quả sản xuất cho máng: ' + trayNo);
                $('#enterSuccessErrorTray #qtyProcessing').html(qtyProduction);
                $('#enterSuccessErrorTray #suitableQuantityKTT').val(qtyProduction);
                $('#enterSuccessErrorTray #nonConforminItemsKTT').val(0);

                if (dataMaterialProd.totalProduction < dataMaterialProd.qty) {
                    $('#confirmationDataRemaining').css({ "pointer-events": "none", "opacity": '0.6' });
                    $('#saveDataForPreOperation').removeClass('disabled');
                } else {
                    $('#saveDataForPreOperation').addClass('disabled');
                }

                $('#confirmationDataRemaining').on('change', function (e) {
                    if ($('#saveDataForPreOperation').hasClass('disabled')) {
                        $('#saveDataForPreOperation').removeClass('disabled');
                    } else {
                        $('#saveDataForPreOperation').addClass('disabled');
                    }
                })

                setTimeout(() => {
                    infoErrorTwisted.forEach(item => {
                        let selector = `input[data-labeltext="${CSS.escape(item.label)}"]`;
                        let input = formEnterResults.find(selector);
                        let valueToSet = item.valueMeasured != null && item.valueMeasured !== ''
                            ? item.valueMeasured
                            : item.qtyError != null
                                ? item.qtyError
                                : 0;

                        input.val(valueToSet);
                    });
                }, 100);
                let otherErrors = getParsedLocalStorageItem('errorChildOthers', []);
                if (otherErrors.length > 0) {
                    if (otherErrors[0].qtyError > 0) {
                        if ($('#totalOtherError').length === 0) {
                            $('#othersKTT').parent().append(`<input type="number" id="totalOtherError" value="${otherErrors[0].qtyError}" class="ms-3 form-control" data-fieldname="othersKTT" />`);
                        }
                        let htmlErrors = displayOtherErrors(otherErrors);
                        $('#othersKTT').prop('checked', true);
                        $('#confirmSaved').after(`<div class="render-details mt-3 mb-3" id="treeErrorsView">
                        <div class="text-center fw-bolder fs-4">Chi tiết lỗi Khác</div>
                            ${htmlErrors}
                        </div>`);
                    }
                }

                let infoErrorBasics = getParsedLocalStorageItem('errorDataBasic', []);
                if (infoErrorBasics.length > 0) {
                    infoErrorBasics.forEach(item => {
                        $(`#enterSuccessErrorTray input.error-basic[data-labeltext="${item.label}"]`).val(item.qtyError);
                    })
                } 
             
                let totalErrors = 0;
                //Lỗi cơ bản
                $('body').on('change', 'input.error-basic', function (e) {
                    e.preventDefault();
                    let label = $(this).parent().find('label').text().includes(':') ? $(this).parent().find('label').text().slice(0, -1) : $(this).parent().find('label').text().trim();
                    let qtyError = parseInt($(this).val() ?? 0, 10);

                    let errorBasics = getParsedLocalStorageItem('errorDataBasic', []) || [];
                    if (errorBasics.length > 0) {
                        errorBasics = errorBasics.filter(item => item.label !== label);
                    }
                    if (qtyError > 0) {
                        errorBasics.push({ label, qtyError });
                    }
                    setParsedLocalStorageItem('errorDataBasic', errorBasics);
                    totalErrors += qtyError;
                    setParsedLocalStorageItem('totalError', true);
                    $('#nonConforminItemsKTT').val(totalErrors);
                    $('#suitableQuantityKTT').val(qtyProduction - totalErrors);
                });
                // Hiển thị lại tổng lỗi đã nhập
                if (getParsedLocalStorageItem('totalError')) {
                    let otherErrors = getParsedLocalStorageItem('errorChildOthers', []);
                    let basicErrors = getParsedLocalStorageItem('errorDataBasic', []);
                    let specialErrors = getParsedLocalStorageItem('errorTwisted', []);
                    let mergedErrors = basicErrors.concat(otherErrors, specialErrors);
                    mergedErrors.forEach(item => {
                        totalErrors += item.qtyError || 0;
                    });
                    $('#nonConforminItemsKTT').val(totalErrors);
                    $('#suitableQuantityKTT').val(qtyProduction - totalErrors);
                }
            })
            .catch(error => {
                alert(error);
            });

        // Đóng nhập lỗi con của lỗi khác
        $('#showChildErrors .btn-close').on('click', function (e) {
            e.preventDefault();
            $('#showChildErrors').modal('hide');
            $('#enterSuccessErrorTray').modal('show');
        });

    });
    // 2. Mở lại modal nhập kết quả khi load lại trang
    $('body').on('click', '.btn-show-again-enter-results', function (e) {
        e.preventDefault();
        $(this).addClass('d-none');
        let trayNo = $(this).attr('data-trayno');
        let qtyproduction = $(this).attr('data-qtyproduction');
        $('#enterSuccessErrorTray').attr('data-trayno', trayNo);
        $('#enterSuccessErrorTray').attr('data-qtyproduction', qtyproduction);
        $('#enterSuccessErrorTray').modal('show');
        let jsonSavedTray = $(this).parent().find('.jsonDataSaved').val();
        $('#enterSuccessErrorTray').attr('data-qtyproduction', qtyproduction);
        if ($('#enterSuccessErrorTray #jsonDataSaved').length == 0 && jsonSavedTray != '') {
            $('#enterSuccessErrorTray').append(`<input type="hidden" id="jsonDataSaved" value='${jsonSavedTray}' />`);
        }
    });
    // 3. Check số lượng và hiển thị lên eink
    $('#saveDataForPreOperation').on('click', function (e) {
        e.preventDefault();

        let jsonSaved = $('#enterSuccessErrorTray #jsonDataSaved').val();
        let otherErrors = getParsedLocalStorageItem('errorChildOthers', []);
        let basicErrors = getParsedLocalStorageItem('errorDataBasic', []);
        let specialErrors = getParsedLocalStorageItem('errorTwisted', []);

        let mergedErrors = basicErrors.concat(otherErrors, specialErrors);

        let dataMaterialProd = getParsedLocalStorageItem('dataMaterialProd', {});
        let dataWorkOrderProd = getParsedLocalStorageItem('dataWorkOrderProd', {});
        let checksheetVerId = $('#checksheetCodePreCheckFinal').attr('data-checksheetversionid');

        let trayNo = $('#enterSuccessErrorTray').attr('data-trayno') || dataMaterialProd.trayNo;

        let qtyProduction = parseInt($('#qtyProcessing').text().trim(), 10);
        let totalQtyEntered = parseInt($('#nonConforminItemsKTT').val(), 10) + parseInt($('#suitableQuantityKTT').val(), 10);

        if (qtyProduction === totalQtyEntered) {
            let formDataMapping = [];

            $('#enterSuccessErrorTray .render-item input').each(function () {
                let objectSaveExcel = {};
                let fieldName = $(this).attr('data-fieldname');
                objectSaveExcel.formId = $(this).parent().parent().parent().parent().attr('data-formid');
                objectSaveExcel.fieldName = fieldName;
                if ($(this).attr('type') == 'text') {
                    objectSaveExcel.value = $(this).val() || '';
                } else if ($(this).attr('type') == 'number') {
                    objectSaveExcel.value = '' + $(this).val() || '';
                } else if ($(this).attr('id') == 'confirmationDataRemaining') {
                    objectSaveExcel.value = $(this).is(':checked') ? 'OK' : '';
                } else if ($(this).attr('id') == 'othersKTT') {
                    objectSaveExcel.value = $(this).is(':checked') ? '' + $('#totalOtherError').val() : '';
                }
                formDataMapping.push(objectSaveExcel);
            });

            
            let arrDataPouchSavedExcel = [];
            if (jsonSaved != undefined) {
                arrDataPouchSavedExcel = JSON.parse(jsonSaved);
            } else if (getParsedLocalStorageItem('INFO_OUTERDIAMETERS_SAVING')) {
                arrDataPouchSavedExcel = getParsedLocalStorageItem('INFO_OUTERDIAMETERS_SAVING',[]);
            }
            if (arrDataPouchSavedExcel.length > 0) {
                arrDataPouchSavedExcel.flat().forEach(newItem => {
                    let existingItem = formDataMapping.find(item => item.fieldName === newItem.fieldName);
                    if (existingItem) {
                        if (newItem.value !== "" && newItem.value !== null && newItem.value !== undefined) {
                            existingItem.value = newItem.value;
                            if (existingItem.fieldName === 'timeWorking') {
                                existingItem.value = newItem.value;
                            } 
                            if (existingItem.fieldName === 'suitableQuantityKTT') {
                                existingItem.value = '' + (parseInt(newItem.value, 10) + totalQtyEntered);
                            }
                        }
                    } else {
                        formDataMapping.push(newItem);
                    }
                });
            }
            swal({
                title: 'Thông báo',
                text: 'Bạn có cần xác nhận lại trước khi thực hiện tiếp không?',
                icon: 'info',
                buttons: ["Có, Xác nhận lại", "Không, tiếp tục"]
            }).then((isConfirmed) => {
                if (isConfirmed) {
                    let infoJsonValue = {
                        formData: formDataMapping,
                    };
                    let infoJsonError = {
                        errorInfo: mergedErrors
                    }
                    let infoEink = {
                        soMang: trayNo,
                        chungLoai: dataWorkOrderProd.productCode,
                        loSanXuat: dataWorkOrderProd.lotNo,
                        loNVL: dataMaterialProd.lotMaterial,
                        HSD: dataMaterialProd.timeLimit,
                        soLuongDat: $('#suitableQuantityKTT').val(),
                        trangThai: dataMaterialProd.totalProduction === dataMaterialProd.qty ? 'Hoàn thành máng' : 'Nhập một phần',
                        nextAction: dataMaterialProd.totalProduction === dataMaterialProd.qty ? 'Read Materials' : 'Working Wires Continue',
                    }
                   
                    fetch(`${window.baseUrl}api/updateFinalEntry`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json;'
                        },
                        body: JSON.stringify({
                            jsonSaveDb: JSON.stringify(infoJsonValue),
                            errorInfo: JSON.stringify(infoJsonError),
                            einkInfo: JSON.stringify(infoEink),
                            positionWorking: positionWorking,
                            workOrderProd: $('#workOrderProd').val(),
                            itemAssignmentId: $('#itemAssignmentId').val(),
                            trayNo: trayNo,
                            qtyOK: $('#suitableQuantityKTT').val(),
                            qtyNG: $('#nonConforminItemsKTT').val(),
                            checksheetVersionId: checksheetVerId,
                            qtyProcessing: qtyProduction,
                            qtyOfReads: dataMaterialProd.qty,
                            qtyInline: dataWorkOrderProd.qtyInLine,
                        })
                    })
                        .then(async response => {
                            if (!response.ok) {
                                const errorResponse = await response.json();
                                throw new Error(`${response.status} - ${errorResponse.message}`);
                            }
                            return response.json();
                        })
                        .then(data => {
                            localStorage.removeItem('errorChildOthers');
                            localStorage.removeItem('errorDataBasic');
                            localStorage.removeItem('errorTwisted');
                            localStorage.removeItem('totalError');
                            if (dataMaterialProd.totalProduction === dataMaterialProd.qty) {
                                localStorage.removeItem('PRODUCTION_CONTINUE');
                            }
                            if (data.statusEink) {
                                setParsedLocalStorageItem('infoEink', infoEink);
                                showAlert('Thông báo', data.message, 'success', [false, "Kết nối Eink"])
                                    .then((isConfirmed) => {
                                        if (isConfirmed) {
                                            $('#readEinkValue').modal('show');
                                            $('#enterSuccessErrorTray').modal('hide');
                                        }
                                    });
                            } else {
                                showAlert('Thông báo', 'Đã cập nhật xong thông tin chủng loại sản xuất vào Eink trước đó. Vui lòng kiểm tra lại', 'success', [false, "Ok"])
                                    .then((isConfirmed) => {
                                        if (isConfirmed) {
                                            window.location.reload();
                                        }
                                    });
                            }
                            localStorage.removeItem('rulerCode');

                            if (!getParsedLocalStorageItem('PRODUCTION_CONTINUE')) {
                                dataMaterialProd.totalQtyHasRead = dataMaterialProd.qty;
                                dataMaterialProd.qty = 0;
                                dataMaterialProd.qtyReadQR = 0;
                                dataMaterialProd.pouchNo = "";
                                dataMaterialProd.listPouchs = [];
                                dataMaterialProd.timeLimit = "";
                                dataMaterialProd.typeMaterial = "";
                                dataMaterialProd.qtyProduction = 0;
                                dataMaterialProd.totalProduction = 0;

                                setParsedLocalStorageItem('dataMaterialProd', dataMaterialProd);
                            }
                            localStorage.removeItem('INFO_OUTERDIAMETERS_SAVING');

                        })
                        .catch(error => {
                            showAlert('Lỗi', error, 'error', [false, 'Xác nhận'])
                                .then((isConfirmed) => {
                                    return;
                                });
                        })
                } else {
                    return;
                }
            });
        } else {
            showAlert('Thông báo', 'Lỗi số lượng hàng phù hợp và không phù hợp không khớp với số lượng đã thao tác! Vui lòng nhập lại', 'warning', [false, "Nhập lại"])
                .then((isConfirmed) => {
                    if (isConfirmed) {
                        $('#nonConforminItemsKTT').addClass('border-danger').val('');
                        $('#suitableQuantityKTT').addClass('border-danger').val('');
                    }
                });
            return;
        }
      
    });
    $('body').on('shown.bs.modal', '#readEinkValue', function (e) {
        e.preventDefault();
        $('#einkTray').val('');
        $('#einkTray').focus();
    });
    $('body').on('click', '#readEinkValue .btn-close', function (e) {
        e.preventDefault();
        swal({
            title: 'Bạn chắc chắn không đọc thẻ E-ink?',
            text: 'Nếu bạn không đọc điều đó gây ra thiếu dữ liệu và sẽ không hiển thị lên thẻ E-Ink. Vì thế bạn nên đọc thẻ E-ink cần kết nối để hiển thị thông tin. Trân trọng cảm ơn!',
            icon: 'warning',
            buttons: ["Tiếp tục", "Đóng"],
            dangerMode: true,
        }).then((isConfirmed) => {
            if (isConfirmed) {
                window.location.reload();
            } else {
                return;
            }
        });
    });
    $('body').on('click', '.btn-confirm-eink', function (e) {
        e.preventDefault();
        $('#readEinkValue').modal('show');
        $(this).addClass('d-none');
    });
    //Kiểm tra eink
    $('body').on('keypress', '#einkTray', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            var barcodeEink = $(this).val().toUpperCase();
            if (barcodeEink.length >= 11) {
                let firstChar = barcodeEink.charAt(0);
                let remaining = barcodeEink.slice(1);
                let updatedRemaining = remaining.replace(/^000/, '');
                barcodeEink = firstChar + updatedRemaining;
            } else {
                barcodeEink = barcodeEink;
            }
            $(this).val(barcodeEink);
            if (barcodeEink != '') {
                fetch(`${window.baseUrl}api/checkeink`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        jsonStr: barcodeEink,
                    })
                })
                    .then(async response => {
                        if (!response.ok) {
                            const errorResponse = await response.json();
                            throw new Error(`${response.status} - ${errorResponse.message}`);
                        }
                        return response.json();
                    })
                    .then(data => {
                        if (data.status) {
                            $(this).addClass('success').removeClass('error');
                            showAlert('Thông báo', data.message, 'info', [false, "Tiếp tục"])
                                .then((isConfirmed) => {
                                    if (isConfirmed) {
                                        $('#confirmEink').trigger('click');
                                    }
                                });
                        }
                    })
                    .catch(error => {
                        $(this).addClass('error').removeClass('success');
                        showAlert('Thông báo', error.message, 'error', [false, "Tiếp tục"])
                            .then((isConfirmed) => {
                                if (isConfirmed) {
                                    $('#einkTray').val('');
                                    $('#einkTray').focus();
                                }
                            });
                    });
            }
        }
    });
    // Trigger lưu dữ liệu lên database eink
    $('body').on('click', '#confirmEink', function (e) {
        e.preventDefault();
        let getInfoShowEink = getParsedLocalStorageItem('infoEink', {});
        let macEink = $('#einkTray').val();
        fetch(`${window.baseUrl}api/triggerEink`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                einkInfo: JSON.stringify(getInfoShowEink),
                MAC: macEink,
                positionWorking: positionWorking,
                workOrderProd: $('#workOrderProd').val(),
            })
        })
            .then(async response => {
                if (!response.ok) {
                    const errorResponse = await response.json();
                    throw new Error(`${response.status} - ${errorResponse.message}`);
                }
                return response.json();
            })
            .then(data => {
                $(this).addClass('success').removeClass('error');
                showAlert('Thông báo', data.message, 'info', [false, "Tiếp tục"])
                    .then((isConfirmed) => {
                        if (isConfirmed) {
                            localStorage.removeItem('infoEink');
                            if (data.readNewWo) {
                                $('#readWorkOrderQR').modal('show');
                            } else {
                                window.location.reload();
                            }
                        }
                    });
            })
            .catch(error => {
                showAlert('Thông báo', error.message, 'error', [false, "Tiếp tục"])
                    .then((isConfirmed) => {
                        if (isConfirmed) {
                            window.location.reload();
                        }
                    });
            });
    });
}
// ============================================== Kết thúc xử lý thao tác kiểm tra trước ==============================================
// ============================================== Các function mở rộng ==============================================

function CheckConditions(MODAL_CONFIRM_FREQUENCY) {
    // Hiển thị xác nhận tần suất
    $('body').on('click', '.content-read-workorder #btnCheckCondition', function (e) {
        $(MODAL_CONFIRM_FREQUENCY).modal('show');

        if ($(this).attr('data-typeaction') !== undefined) {
            $(MODAL_CONFIRM_FREQUENCY).find('#btnReadCondition').attr('data-typeaction', $(this).attr('data-typeaction'));
        }
        $(this).addClass('d-none');


    });
    $(MODAL_CONFIRM_FREQUENCY).find('input[type="radio"]').each(function () {
        $(this).prop('checked', false);
    });
    // Hiển thị xác nhận tần suất
    $(MODAL_CONFIRM_FREQUENCY).on('shown.bs.modal', async function (e) {
        e.preventDefault();
        let dataProductions = getParsedLocalStorageItem('dataWorkOrderProd');
        $('input[type="radio"]').each(function () {
            dataProductions.frequencyIds.forEach(item => {
                if ($(this).attr('data-frequencyid') == item.frequencyId) {
                    $(this).prop('checked', true);
                    $(this).parent().addClass('d-none');
                } else {
                    $(this).prop('checked', false);
                }
                $('#btnReadCondition').removeClass('disabled').text('Đọc bổ sung');
            });
        });
        $('.frequency-item input[type="radio"]').each(function () {
            $(this).on('change', function () {
                $('#btnReadCondition').removeClass('disabled');
            });
        })
    });
    // Xử lý khi click vào nút đóng modal kiểm tra tần suất
    $('body').on('click', `${MODAL_CONFIRM_FREQUENCY} .btn-close`, function (e) {
        e.preventDefault();
        swal({
            title: 'Bạn chắc chắn muốn đóng thao tác?',
            text: 'Bạn không nên đóng thao tác khi đang thực hiện thao tác. Điều đó gây ra thiếu dữ liệu. Trân trọng!',
            icon: 'warning',
            buttons: ["Không", "Có"],
        }).then((isConfirmed) => {
            if (isConfirmed) {
                $(MODAL_CONFIRM_FREQUENCY).modal('hide');
                $('#btnCheckCondition').removeClass('d-none');
            }
        });
    });
}
function endEarlyProduction() {
    $('body').on('click', '#btnEndEarly', function (e) {
        e.preventDefault();
        showAlert('Thông báo', 'Bạn muốn kết thúc sớm? Vui lòng báo Leader xác nhận.', 'warning', ["Không", "Có"])
            .then((isConfirmed) => {
                if (isConfirmed) {
                    $('#leaderComfirmedAbnormal').modal('show');
                    $('#leaderComfirmedAbnormal #leaderComfirmedAbnormalLabel').html('Nhập lý do kết thúc sớm');
                } else {
                    window.location.reload();
                }
            });
    });
}
async function GetMenuChild(errorId, errorName) {
    try {
        const response = await fetch(`${window.baseUrl}api/getMenuChild`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                errorId: errorId,
                errorName: errorName
            })
        });

        if (!response.ok) {
            const errorResponse = await response.json();
            throw new Error(`${response.status} - ${errorResponse.message} `);
        }

        const data = await response.json();
        let menuChilds = data.menuChild;
        let htmlMenuChild = '';
        htmlMenuChild += `
            <h6 class="text-center">Nhập số lượng lỗi ${errorName}</h6>
            <div class="row">`;
        menuChilds.forEach(item => {
            if (item.checkMenuChild) {
                htmlMenuChild += `<div class="col-4 mt-2">
                                <div class="input-group input-other-error" data-parentname="${item.parentName}">
                                    <label for="checkOtherErrors${item.id}">${item.errorName}</label>
                                    <input type="checkbox" class="form-check-input checked-error-child-first" min="0" name="errorCheckbox" data-errorid="${item.id}" id="checkOtherErrors${item.id}" />
                                    <input type="number" class="form-control total-error-child-first d-none ms-2" min="0" id="totalOtherErrorChild${item.id}" />
                                </div>
                            </div>`;
            } else {
                htmlMenuChild += `<div class="col-4 mt-2">
                                <div class="input-group input-other-error" data-parentname="${item.parentName}">
                                    <label class="form-label" for="errorParent${item.id}">${item.errorName}</label>
                                    <input type="number" class="form-control w-100 error-other-first" min="0" data-errorid="${item.id}" id="errorItem${item.id}" />
                                </div>
                            </div>`;
            }

        });
        htmlMenuChild += '</div>';
        return { html: htmlMenuChild };
    } catch (error) {
        console.error('Lỗi khi gọi master lỗi:', error);
        throw error;
    }
}
function getParsedLocalStorageItem(key, defaultValue = null) {
    try {
        const item = localStorage.getItem(key);
        return item ? JSON.parse(item) : defaultValue;
    } catch (e) {
        console.error(`Error parsing localStorage item '${key}':`, e);
        return defaultValue;
    }
}
function setParsedLocalStorageItem(key, value) {
    try {
        localStorage.setItem(key, JSON.stringify(value));
    } catch (e) {
        console.error(`Error stringifying localStorage item '${key}':`, e);
    }
}
const showAlert = (title, text, icon, buttons = ["Không", "Có"]) => {
    return swal({ title, text, icon, buttons });
};
// -------------------------------------------- Xử lý lỗi theo dạng cây thư mục  --------------------------------------------
function findAllErrorNodesById(tree, errorId) {
    let results = [];
    for (let node of tree) {
        if (node.errorId === errorId) results.push(node);
        if (node.childErrors?.length) {
            results = results.concat(findAllErrorNodesById(node.childErrors, errorId));
        }
    }
    return results;
}
function findErrorNodeById(tree, errorId) {
    for (let node of tree) {
        if (node.errorId === errorId) return node;
        if (node.childErrors?.length) {
            const found = findErrorNodeById(node.childErrors, errorId);
            if (found) return found;
        }
    }
    return null;
}
function insertChild(tree, parentname, newChild) {
    for (let node of tree) {
        if (node.otherName === parentname || node.errorName === parentname) {
            node.childErrors = node.childErrors || [];
            node.childErrors.push(newChild);
            return true;
        }
        if (node.childErrors?.length && insertChild(node.childErrors, parentname, newChild)) return true;
    }
    return false;
}
function addQuantityToNodeById(tree, parentname, item) {
    for (let node of tree) {
        if (node.otherName === parentname || node.errorName === parentname) {
            node.savedOtherError = node.savedOtherError || [];
            node.savedOtherError.push(item);
            return true;
        }
        if (node.childErrors?.length) {
            if (addQuantityToNodeById(node.childErrors, parentname, item)) return true;
        }
    }
    return false;
}
function sumErrorsAndUpdate(tree) {
    let total = 0;

    for (let node of tree) {
        let selfQty = 0;

        if (node.savedOtherError?.length) {
            for (let item of node.savedOtherError) {
                selfQty += item.quantity || 0;
            }
        }

        if (node.childErrors?.length) {
            selfQty += sumErrorsAndUpdate(node.childErrors);
        }

        node.qtyError = selfQty;
        total += selfQty;
    }

    return total;
}
// -------------------------------------------- Kết thúc xử lý lỗi theo dạng cây thư mục  --------------------------------------------
function displayOtherErrors(errorArray, indent = 0) {
    let html = '';
    errorArray.forEach(error => {
       
        const prefix = "&nbsp;&nbsp;".repeat(indent * 4);
        if (error.otherName) {
            html += `<div>${prefix}<strong>- ${error.otherName}:</strong> (Số lượng: ${error.qtyError || 0})</div>`;
        } else if (error.errorName) {
            html += `<div>${prefix}<strong>- ${error.errorName}:</strong> (Số lượng: ${error.qtyError || 0})</div>`;
        }

        if (error.savedOtherError && error.savedOtherError.length > 0) {
            error.savedOtherError.forEach(subError => {
                html += `<div>${prefix}&nbsp;&nbsp;<strong>- ${subError.labelText}:</strong> ${subError.quantity || 0}</div>`;
            });
        }

        if (error.childErrors && error.childErrors.length > 0) {
            html += displayOtherErrors(error.childErrors, indent + 1);
        }
    });
    return html;
}
function handlerOuterDiameterKTT(elemTarget, targetButton) {
    // So sánh đường kính nhập vào đủ 3 số sau dấu phẩy
    const regex = /^\d+\.\d{3}$/;
    let valDiameter = elemTarget.val();
    if (!regex.test(valDiameter)) {
        showAlert('Thông báo', 'Đường kính đuôi dây dẫn phải nhập đủ 3 số sau dấu chấm', 'warning', [false, 'Nhập lại']);
        elemTarget.parent().addClass("outer-diameter error");
        elemTarget.focus();
        return false;
    }
    // So sánh đường kính nhập vào nằm trong giới hạn đường kính tiêu chuẩn
    // Nhập sai leader xác nhận và nhập lại đường kính đó
    // Nhập đúng sẽ hiển thị button được target trước đó
    let valDiameterStandard = $('#stdOuterDiameterMM').val() || "";
    valDiameterStandard = valDiameterStandard.split('~');
    let minStandard = valDiameterStandard[0];
    let maxStandard = valDiameterStandard[1];
    if (valDiameter >= minStandard && valDiameter <= maxStandard) {
        $(`#${targetButton}`).removeClass('d-none');
        $(`#${targetButton}`).attr('diameter-value', valDiameter);
        elemTarget.parent().removeClass('outer-diameter error').addClass("outer-diameter success");
        $(`#${targetButton}`).parent().removeClass('d-none');
    } else {
        let customNoticeAlertHTML = document.createElement("div");
        customNoticeAlertHTML.innerHTML =
        `<div>
            <p style="font-size: 18px; text-transform: capitalize;">Đường kính đuôi dây dẫn này đang lỗi.</p>
            <ul style="font-size: 14px; text-align: left;">
                <li>Nếu bạn đo sai hoặc ghi nhầm thì vui lòng ấn <strong>"Nhập lại"</strong> để thực hiện lại.</li>
                <li>Nếu có bất thường vui lòng ấn <strong>"Có bất thường"</strong> và báo Leader đến xác nhận, không tự ý xác nhận!</li>
            </ul>
        </div>`;
        swal({
            title: 'Lỗi',
            content: customNoticeAlertHTML,
            icon: "error",
            buttons: ["Nhập lại", "Có bất thường"],
        }).then((isConfirmed) => {
            if (isConfirmed) {
                const leaderConfirmError = {
                    valDiameter: valDiameter,
                    leaderCheck: true,
                };
                setParsedLocalStorageItem('leaderConfirm', leaderConfirmError);
                $(`#${targetButton}`).trigger('click');
            } else {
                elemTarget.val('');
                elemTarget.parent().removeClass('outer-diameter error');
                return;
            }
        })
        elemTarget.parent().removeClass('outer-diameter success').addClass("outer-diameter error");
        elemTarget.focus();
    }
}
function handlerSavingOuterDiameterKTT(form, thisElement, targetElement) {
    let diameterValue = parseFloat($(thisElement).attr('diameter-value'));
    let currentTrayNo = $('#trayNo').val();
    $(`#${targetElement}`).val('OK');

    let currentPouchDataInStorage = getParsedLocalStorageItem('dataMaterialProd', {});
    let qtyReadOfCurrentPouch = currentPouchDataInStorage.qtyReadQR;

    let pouchEntry = {
        pouchNo: currentPouchDataInStorage.pouchNo,
        valDiameter: diameterValue,
        qtyPouch: qtyReadOfCurrentPouch,
        confirmVal: $(`#${targetElement}`).val(),
        leaderCheck: false,
    };

    let leaderConfirmVal = getParsedLocalStorageItem('leaderConfirm');
    if (leaderConfirmVal) {
        pouchEntry.leaderCheck = leaderConfirmVal.leaderCheck;
        pouchEntry.valDiameter = diameterValue || leaderConfirmVal.valDiameter;
        pouchEntry.confirmVal = "NG";
        $(`#${targetElement}`).val('NG');
        $('#btnConfirmWireHasBeenWorked').addClass('disabled');
        setParsedLocalStorageItem('errorPouchs', pouchEntry);
    }
    if (currentPouchDataInStorage.listPouchs != undefined) {
        currentPouchDataInStorage.listPouchs.push(pouchEntry);
        currentPouchDataInStorage.qty = currentPouchDataInStorage.listPouchs.reduce((sum, p) => sum + p.qtyPouch, 0);
        currentPouchDataInStorage.pouchNo = currentPouchDataInStorage.pouchNo;
    }
    currentPouchDataInStorage.trayNo = currentTrayNo;
    setParsedLocalStorageItem('dataMaterialProd', currentPouchDataInStorage);
    
    // Lưu dữ liệu vào excel (cho từng pouch)
    const currentSaveDataPouchExcel = getParsedLocalStorageItem('INFO_OUTERDIAMETERS_SAVING') || [];
    const listData = [];
    // Lấy dữ liệu từ các input hiển thị trong form hiện tại cho pouch này
    form.find('.render-item').find('input').each(function (e) {
        listData.push({
            formId: $(this).parent().parent().parent().parent().attr('data-formid'),
            fieldName: $(this).attr('data-fieldname'),
            value: $(this).is(':checkbox') ? ($(this).is(':checked') ? pouchEntry.confirmVal : '') : $(this).val(),
        });
    });

    currentSaveDataPouchExcel.push(listData);
    setParsedLocalStorageItem('INFO_OUTERDIAMETERS_SAVING', currentSaveDataPouchExcel);

    localStorage.removeItem('dataTrayCurrent');
    if (leaderConfirmVal) {
        return;
    } else {
        // Reset UI cho lần quét/nhập tiếp theo
        setTimeout(function () {
            $('#qrMaterialValue').val('').removeClass('disabled').focus();
        }, 800);
        if (currentPouchDataInStorage.listPouchs.length < 5) {
            $('#btnConfirmWireHasBeenWorked').removeClass('disabled'); // Đảm bảo nút này được bật lại
        } else if (currentPouchDataInStorage.listPouchs.length === 5) {
            $('#btnConfirmWireHasBeenWorked').trigger('click'); // Tự động trigger khi đủ 5 pouch
        }
        form.empty();
    }
    
}
function handleReadRulerCode(form, thisElement, targetElement) {
    let label = thisElement.parent().find('label').text().includes(':') ? thisElement.parent().find('label').text().slice(0, -1) : thisElement.parent().find('label').text().trim();
    let qtyError = parseInt(thisElement.val() ?? 0, 10);

    let errorTwisted = getParsedLocalStorageItem('errorTwisted', []) || [];
    if (errorTwisted.length > 0) {
        errorTwisted = errorTwisted.filter(item => item.label !== label);
    }
    if (qtyError > 0) {
        errorTwisted.push({ label, qtyError });
    }
    setParsedLocalStorageItem('errorTwisted', errorTwisted);

    $('#saveDataForPreOperation').addClass('disabled');

    if (!localStorage.getItem('rulerCode')) {
        $(`#${targetElement}`).modal('show');
        $('#enterSuccessErrorTray').modal('hide');
        setParsedLocalStorageItem('rulerCode', true);
        setParsedLocalStorageItem('totalError', true);
    }
}
function handleUpdateValueErrorTwisted(form, $this, unit) {
    let infoErrorTwisted = getParsedLocalStorageItem('errorTwisted', []);
    let label = $this.parent().find('label').text().includes(':') ? $this.parent().find('label').text().slice(0, -1) : $this.parent().find('label').text().trim();
    let valueMeasured = $this.val() + unit;
    infoErrorTwisted.push({ label, valueMeasured });
    setParsedLocalStorageItem('errorTwisted', infoErrorTwisted);
    $this.val(valueMeasured);
    if ($('#saveDataForPreOperation').hasClass('disabled')) {
        $('#saveDataForPreOperation').addClass('disabled');
    } else {
        $('#saveDataForPreOperation').removeClass('disabled');
    }
    
}
async function handleShowOtherErrorKTT(form, $this, errorStack, arrErrors, checkedErrors) {
    let errorId = $this.data('errorid');
    let errorName = $this.parent().find('label').text().includes(':') ? $this.parent().find('label').text().slice(0, -1) : $this.parent().find('label').text().trim();
    let { html: htmlChildErrors } = await GetMenuChild(errorId, errorName);
    $('#listErrors').html(htmlChildErrors);
    $('#enterSuccessErrorTray').modal('hide');
    $('#showChildErrors').modal('show');
    arrErrors.push({
        otherName: errorName,
        childErrors: [],
    });

    const stored = getParsedLocalStorageItem('errorChildOthers', {}) || [];
    if (stored.length > 0) {
        const matchedNode = findErrorNodeById(stored, errorId);
        if (matchedNode?.savedOtherError?.length) {
            const lastItem = matchedNode.savedOtherError.at(-1);
            const input = $('body').find(`#listErrors input[type="number"][data-errorid="${lastItem.errorId}"]`);
            if (lastItem) input.val(lastItem.quantity || 0);
        }
    }


    $('body').on('change', '#showChildErrors .input-other-error input[type="checkbox"]', async function (e) {
        e.preventDefault();
        let errorId = $(this).data('errorid');
        let errorName = $(this).parent().find('label').text().includes(':') ? $(this).parent().find('label').text().slice(0, -1) : $(this).parent().find('label').text().trim();

        if ($(this).is(':checked')) {
            checkedErrors.add(errorId);

            errorStack.push($('#listErrors').html());
            let { html: htmlChildErrors } = await GetMenuChild(errorId, errorName);

            $('#listErrors').html(htmlChildErrors);
            $('#confirmCountErrors').html('Xác nhận lỗi');

            const newNode = {
                errorId,
                errorName,
                qtyError: 0,
                childErrors: [],
            };

            const parentname = $(this).closest('.input-other-error').attr('data-parentname');
            if (!findAllErrorNodesById(arrErrors, errorId).length) {
                insertChild(arrErrors, parentname, newNode);
            }

            const stored = getParsedLocalStorageItem('errorChildOthers', {}) || [];
            if (stored.length > 0) {
                const matchedNodes = findAllErrorNodesById(stored, errorId);
                matchedNodes.forEach(node => {
                    if (node?.savedOtherError?.length) {
                        node.savedOtherError.forEach(item => {
                            const input = $(`#listErrors input[type="number"][data-errorid="${item.errorId}"]`);
                            input.val(item.quantity || 0);
                        });
                    }
                });
            }
        } else {
            checkedErrors.delete(errorId);
        }
    });

    $('body').on('change', '#showChildErrors .input-other-error input[type="number"]', function (e) {
        let quantity = parseInt($(this).val(), 10) || 0;
        let parentname = $(this).parent().attr('data-parentname');
        let errorId = parseInt($(this).attr('data-errorid'), 10) || 0;
        let labelText = $(this).parent().find('label').text().includes(':') ? $(this).parent().find('label').text().slice(0, -1) : $(this).parent().find('label').text().trim();
        let item = {
            errorId,
            labelText,
            quantity
        };

        addQuantityToNodeById(arrErrors, parentname, item);
        sumErrorsAndUpdate(arrErrors);

        setParsedLocalStorageItem('errorChildOthers', arrErrors);
        $('#confirmCountErrors').removeClass('disabled').attr('data-parentname', parentname);

        setParsedLocalStorageItem('totalError', true);
    });

    $('#showChildErrors #confirmCountErrors').on('click', function (e) {
        e.preventDefault();
        showAlert('Thông báo', 'Nhập lỗi thành công, có nhập lỗi tiếp không?')
            .then((isConfirmed) => {
                if (!isConfirmed) {
                    $('#showChildErrors').modal('hide');
                    $('#enterSuccessErrorTray').modal('show');
                } else {
                    if (errorStack.length > 0) {
                        let previousErrors = errorStack.pop();
                        $('#listErrors').html(previousErrors);
                        const arrCheckedErrors = [...checkedErrors];
                        if (arrCheckedErrors.length > 0) {
                            arrCheckedErrors.forEach(item => {
                                const inputCheckBox = $('body').find(`#listErrors input[type="checkbox"][data-errorid="${item}"]`);
                                inputCheckBox.prop('checked', true);
                            })
                          
                        }
                        $(this).html('Quay lại');
                    } else {
                        return;
                    }
                }
            });
    });
}
// ============================================== Kết thúc các function mở rộng ==============================================