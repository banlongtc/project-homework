"use-strict";
connectionHub.on("ReceiveDataMC", function (data) {
    if ($('#divMCContent').length > 0) {
        var dataReceived = JSON.parse(data);
        dataReceived.forEach(item => {
            var shifts = item.shifts;
            shifts.forEach(shift => {
                shift.infoShifts.forEach(info => {
                    info.machineShift.forEach(machine => {
                        $(`#tableMC${shift.shiftLabel} tbody tr[data-workorder="${info.workOrder}"]`).each(function (index, elem) {
                            let percentUsed = parseInt((machine.qtyHasProcessed / machine.qtyDiv * 100)) + '%';
                            $(elem).find(`td[data-machine="${machine.machineShift}"]`).find('.show-percent').text(percentUsed);
                            $(elem).find(`td[data-machine="${machine.machineShift}"]`).find('.progress-bar').css('width', percentUsed);
                            $(elem).find(`td[data-machine="${machine.machineShift}"]`).find('.percent-used').text(percentUsed);
                            $(elem).find(`td[data-machine="${machine.machineShift}"]`).find('.qty-used').text(machine.qtyHasProcessed + '/' + machine.qtyDiv);
                        });
                    })
                   
                });
            });
        });
    }
});
document.addEventListener('DOMContentLoaded', function () {
    if ($('#divMCContent').length > 0) {
        $('.navbar').addClass('d-none');
    }

    $('#showDivMC').on('hidden.bs.modal', function (e) {
        $('#tableDivMC .data-render .list-item').remove();
        $('#tableDivMC').addClass('d-none');
    })

    $('#contentTablePrintLabels tr input[type="number"]').each(function (i, elem) {
        $(elem).val('');
    });

    $('.show-div-mc').on('click', function (e) {
        let parentElem = $(e.target).parent().parent().parent();
        let processCode = parentElem.find("input.processcode").val();
        fetch(`${window.baseUrl}printlabels/getreserveditem`, {
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
                let workOrder = parentElem.data('workorder');
                let htmlSelectMaterial = '';
                htmlSelectMaterial += `<option value="">--Chọn mã NVL--</option>`;
                data.dataLot.forEach(item => {
                    htmlSelectMaterial += `<option value="${item.productCode}">${item.productCode}</option>`;
                });
                $('#selectMaterial').html(htmlSelectMaterial);

                if (data.oldDivMaterials.length > 0) {
                    let htmlOldData = '';
                    let colTotals = Array(12).fill(0);
                    data.oldDivMaterials.forEach(item => {
                        let qty = Array(12).fill(0);
                        const shiftMap = {
                            "Shift1": 0,
                            "Shift2": 4,
                            "Shift3": 8,
                        };

                        let shiftOffset = shiftMap[item.shiftLabel] || 0;
                        let machineIndex = parseInt(item.machineShift, 10) - 1;

                        if (shiftOffset >= 0 && machineIndex >= 0) {
                            qty[shiftOffset + machineIndex] = item.qtyDiv;
                            colTotals[shiftOffset + machineIndex] += item.qtyDiv;
                        }
                        htmlOldData +=
                            `<tr class="list-item" data-workorder="${item.workOrder}" data-product_code="${item.materialCode}" data-lot_product="${item.lotMaterial}">
                                    <td class='align-middle'><div style="width: 200px;" class="item-render">${item.materialCode}</div></td>
                                    <td class='align-middle'><div style="width: 100px;" class="item-render">${item.lotMaterial}</div></td>
                                    ${qty.map((q, idx) => {
                                let shiftLabel = idx < 4 ? 'Shift1' : idx < 8 ? 'Shift2' : 'Shift3';
                                let machineNumber = (idx % 4) + 1;
                                        return `<td data-shift='${shiftLabel}' class='td-render-item align-middle'><div style="width: 100px;" class="qty-item" data-machine="${machineNumber}" data-qtydiv="${q}">${q}</div></td>`;
                            }).join('')}
                              </tr>`;
                    });
                    let htmlTotals = `<tfoot>
                        <tr>
                            <td colspan="2">Tổng</td>
                            ${colTotals.map(total => `<td class="align-middle"><div style="width: 100px;">${total}</div></td>`).join('')}
                        </tr>
                    </tfoot>`;
                    $('#tableHasDivMaterial .table .data-render').html(htmlOldData); 
                    $('#tableHasDivMaterial .table').append(htmlTotals); 
                }


                $('body').on('change', '#selectMaterial', function (e) {
                    $('#tableDivMC .data-render').html('');
                    $('#tableHasDivMaterial').removeClass('d-none');
                    if ($(this).val() != '') {
                        $('#tableDivMC').removeClass('d-none');
                        let htmlRender = '';
                        data.dataLot.forEach(item => {
                            if (item.productCode == $(this).val()) {
                                htmlRender +=
                                    `<tr class="list-item" data-workorder="${workOrder}" data-product_code="${item.productCode}" data-lot_product="${item.lotNo}" data-qtybase="${item.qty}">
                                    <td class='align-middle'><div <div style="width: 200px;" class="item-render">${item.productCode}</div></td>
                                    <td class='align-middle'><div <div style="width: 100px;" class="item-render">${item.lotNo}</div></td>
                                    <td class='align-middle'><div style="width: 100px;" class="item-render">${item.qty}</div></td>
                                    <td data-shift='Shift1' class='align-middle'><div style="width: 100px;" class="qty-item"><input type="number" data-machine='1' class="form-control disabled" min=0 id="qtyMC1Shift1" /></div></td>
                                    <td data-shift='Shift1' class='align-middle'><div style="width: 100px;" class="qty-item"><input type="number" data-machine='2' class="form-control disabled" min=0 id="qtyMC2Shift1" /></div></td>
                                    <td data-shift='Shift1' class='align-middle'><div style="width: 100px;" class="qty-item"><input type="number" data-machine='3' class="form-control disabled" min=0 id="qtyMC3Shift1" /></div></td>
                                    <td data-shift='Shift1' class='align-middle'><div style="width: 100px;" class="qty-item"><input type="number" data-machine='4' class="form-control disabled" min=0 id="qtyMC4Shift1" /></div></td>
                                    <td data-shift='Shift2' class='align-middle'><div style="width: 100px;" class="qty-item"><input type="number" data-machine='1' class="form-control disabled" min=0 id="qtyMC1Shift2" /></div></td>
                                    <td data-shift='Shift2' class='align-middle'><div style="width: 100px;" class="qty-item"><input type="number" data-machine='2' class="form-control disabled" min=0 id="qtyMC2Shift2" /></div></td>
                                    <td data-shift='Shift2' class='align-middle'><div style="width: 100px;" class="qty-item"><input type="number" data-machine='3' class="form-control disabled" min=0 id="qtyMC3Shift2" /></div></td>
                                    <td data-shift='Shift2' class='align-middle'><div style="width: 100px;" class="qty-item"><input type="number" data-machine='4' class="form-control disabled" min=0 id="qtyMC4Shift2" /></div></td>
                                    <td data-shift='Shift3' class='align-middle'><div style="width: 100px;" class="qty-item"><input type="number" data-machine='1' class="form-control disabled" min=0 id="qtyMC1Shift3" /></div></td>
                                    <td data-shift='Shift3' class='align-middle'><div style="width: 100px;" class="qty-item"><input type="number" data-machine='2' class="form-control disabled" min=0 id="qtyMC2Shift3" /></div></td>
                                    <td data-shift='Shift3' class='align-middle'><div style="width: 100px;" class="qty-item"><input type="number" data-machine='3' class="form-control disabled" min=0 id="qtyMC3Shift3" /></div></td>
                                    <td data-shift='Shift3' class='align-middle'><div style="width: 100px;" class="qty-item"><input type="number" data-machine='4' class="form-control disabled" min=0 id="qtyMC4Shift3" /></div></td>
                                </tr>`;
                            }
                        });
                        $('#tableDivMC .data-render').html(htmlRender);
                    }
                    if (data.oldDataDivByWorkOrder.length > 0) {
                        let hasDivWorkOrder = data.oldDataDivByWorkOrder;
                        hasDivWorkOrder.forEach(item => {
                            $(`#tableDivMC tbody .list-item[data-workorder="${workOrder}"] td[data-shift="${item.shiftLabel}"] input[data-machine="${item.machineShift}"]`).removeClass('disabled');
                        });
                    } else {
                        $('#showDivMC .modal-body').append('<p class="text-danger">Chưa chia số lượng ở workorder. Vui lòng thực hiện lại</p>');
                    }
                });
            })
            .catch(error => {
                alert(error);
            });
    });
    // Xử lý nhập lại máy khi có thay đổi
    $('body').on('change', '#contentTablePrintLabels tr.has-content input[type="number"]', function (e) {
        $('#enterChangeValue').modal('show');
        let thisInputChange = $(this);
        let valChange = thisInputChange.val();
        $('.table-print-label .error-total').html('');
        $('#enterChangeValue .btn-close').on('click', function (e) {
            swal({
                title: "Bạn muốn thoát?",
                icon: "info",
                buttons: ["Không", "Có"],
            }).then((isConfirmed) => {
                if (isConfirmed) {
                    $('#enterChangeValue').modal('hide');
                    window.location.reload();
                    thisInputChange.val('');
                } else {
                    return;
                }
            });
        });
        $('#confirmNote').on('click', function (e) {
            e.preventDefault();
            let valNote = $('#textNote').val().trim();
            
            thisInputChange.removeClass('border-danger');
            $('#enterChangeValue').modal('hide');
            thisInputChange.parent().parent().each(function (i, elem) {
                $(elem).removeClass('has-content');
                $(elem).find('textarea').val(valNote);
                $(elem).find('input[type="number"]').val('');
            });
            thisInputChange.val(valChange);
            $('.btn-save-printlabel').removeClass('disabled');
        });
    });
    //Xử lý chia NVL cho máy
    $('body').on('change', '#showDivMC input[type="number"]', function (e) {
        let parentTrElem = $(this).parent().parent().parent();
        let totalQtyDiv = 0;
        let qtyMaterial = parseInt(parentTrElem.data('qtybase'), 10);
        parentTrElem.addClass('is-div');
        parentTrElem.find('input[type="number"]').not('.disabled').each(function (i, elem) {
            let valueInput = parseInt($(elem).val(), 10);
            totalQtyDiv += valueInput;
        });
        if (qtyMaterial == totalQtyDiv) {
            $('#showDivMC .modal-body .error-message').remove();
            $(this).removeClass('border-danger').addClass('border-success');
            $('#btnSaveDivMaterialForMC').removeClass('disabled');
        } else {
            $(this).addClass('border-danger');
            $('#showDivMC .modal-body').append('<p class="error-message text-danger">Số lượng chia không khớp với số lượng pick cho NVL. Vui lòng thử lại!</p>');
        }
    });
    $('body').on('click', '#showDivMC #btnSaveDivMaterialForMC', function (e) {
        e.preventDefault();
        let dataMaterialsDiv = [];
        $('#showDivMC tbody tr.is-div').each(function (i, elem) {
            let objSave = {};
            objSave.workorder = "" + $(elem).data('workorder');
            objSave.materialCode = $(elem).data('product_code');
            objSave.lotMaterial = $(elem).data('lot_product');
            objSave.shiftLabel = $(elem).find('input[type="number"]').not('.disabled').parent().parent().data('shift');
            objSave.machineShift = "" + $(elem).find('input[type="number"]').not('.disabled').data('machine');
            objSave.qtyDiv = $(elem).find('input[type="number"]').not('.disabled').val();
            dataMaterialsDiv.push(objSave);
        });
        let jsonStr = JSON.stringify(dataMaterialsDiv);
        fetch(`${window.baseUrl}printlabels/savedivmaterial`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json; charset=utf-8;'
            },
            body: JSON.stringify({
                jsonStr: jsonStr
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
            })
            .catch(error => {
                alert(error.message);
            })
    });


    if ($('#contentTablePrintLabels tr').length > 0) {
        loadContent();
        $(".btn-save-printlabel").on("click", function (e) {
            e.preventDefault();
            let mergedDataArray = [];
            let check = [];

            var table = $('.table-print-label');
            var rows = table.find('tbody tr.is-checked');
            $(".table-print-label .message .error-input").html("");
            rows.each(function () {
                let $row = $(this);
                var workOrder = $row.data('workorder');
                var productCode = $row.data('product_code');
                var lotNo = $row.data('lot_no');
                var qtyUsed = $row.data("qty_prod");
                var characterWo = $row.data("character") || $row.find('td').not(".hidden").find('input.character-wo').val();
                var typeLabel = $row.data("typelabel");
                var dateProd = $row.find('input.date-prod').val();
                var note = $row.find('textarea').val();

                let shifts = [];
                for (let m = 1; m <= 3; m++) {
                    let shift = {
                        shift: `Shift${m}`,
                        machines: []
                    };
                    for (let s = 1; s <= 4; s++) {
                        let machineValue = $row.find(`td[data-flag="shift${m}"] .machine-${s}`).val();
                        shift.machines.push({
                            machine: s,
                            value: machineValue
                        });
                    }
                    shifts.push(shift);
                }

                let existingEntry = mergedDataArray.find(item => item.workOrder === workOrder);

                if (existingEntry) {
                    existingEntry.rows.push({
                        typeLabel: typeLabel,
                        dateProd: dateProd,
                        shifts: shifts,
                        note: note
                    });
                } else {
                    mergedDataArray.push({
                        workOrder: workOrder || "",
                        productCode: productCode,
                        lotNo: lotNo,
                        qtyUsed: qtyUsed,
                        characterWo: characterWo.toUpperCase(),
                        rows: [
                            {
                                typeLabel: typeLabel,
                                dateProd: dateProd,
                                shifts: shifts,
                                note: note
                            }
                        ]
                    });
                }
            });
            let processCode = $(".processcode").val();
            fetch(`${window.baseUrl}PrintLabels/SaveData`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                },
                body: JSON.stringify({
                    strDataPrintLabels: JSON.stringify(mergedDataArray),
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
                    window.location.reload();
                })
                .catch(error => {
                    alert(error);
                })
        });
    }
});

function loadContent() {
    fetch(`${window.baseUrl}PrintLabels/RenderContent`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json; charset=utf-8'
        },
    })
        .then(async response => {
            if (!response.ok) {
                const errorResponse = await response.json();
                throw new Error(`${response.status} - ${errorResponse.message}`);
            }
            return response.json();
        })
        .then(data => {
            let oldData = data.oldData;

            var today = new Date();
            var dd = String(today.getDate()).padStart(2, '0');
            var mm = String(today.getMonth() + 1).padStart(2, '0');
            var yyyy = today.getFullYear();
            today = dd + '/' + mm + '/' + yyyy;
            $(".date-prod").datepicker({
                dateFormat: "dd/mm/yy",
                showOn: "both",
                buttonImage: "./images/calendar.png",
                buttonImageOnly: true,
                buttonText: "Chọn ngày",
                showAnim: "slideDown",
                firstDay: 1,
                dayNamesMin: ["CN", "T2", "T3", "T4", "T5", "T6", "T7"],
                monthNames: ["Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"],
                minDate: today,
            });
            $('.date-prod').on('change', function (e) {
                if ($(this).hasClass('border-danger')) {
                    $(this).removeClass('border-danger');
                    $(".table-print-label .message .error-input").html("");
                }
            });
            // Check lượng NVL
            var q = new Date();
            var dateCurrent = new Date(q.getFullYear(), q.getMonth(), q.getDate());
            var timestampCurrent = dateCurrent.getTime();
            if ($(".time-intended").length) {
                let dateTimeFull = new Date();
                let itemArr = [];
                $(".time-intended").each(function (i, elem) {
                    let valueDate = $(elem).parent().data("date_intended");
                    let dateInput = new Date(valueDate + " 00:00:00");
                    let timestampInput = dateInput.getTime();
                    if (timestampCurrent == timestampInput) {
                        let timeIntended = $(elem).val();
                        dateTimeFull = new Date(valueDate + " " + timeIntended);
                        let qtyInput = parseInt($(elem).parent().parent().find("input.qty-used").val(), 10);
                        itemArr.push({
                            processCode: $(elem).parent().parent().find("input.processcode").val(),
                            workOrder: $(elem).parent().parent().find("input.work-order").val(),
                            qtyUsed: qtyInput
                        });
                    }
                });  
                checkTime30(dateTimeFull, itemArr);
            }

            $('input[type="number"]').on('change', function (e) {
                e.preventDefault();
                let $thisParent = $(this).parent().parent();
                $thisParent.addClass('is-checked');
                let workOrderParent = $thisParent.data('workorder');
                let typeLabel = $thisParent.data('typelabel');
                let qtyWorkOrder = parseInt($thisParent.data('qty_prod'), 10);
                let totalQtyDiv = 0;
                let $matchedRows = $(`#contentTablePrintLabels tr.is-checked[data-workorder='${workOrderParent}'][data-typelabel='${typeLabel}']`);
                $matchedRows.each(function () {
                    // Lấy giá trị của input
                    $(this).find('input[type="number"]').each(function (i, e) {
                        let inputVal = $(this).val();
                        let currentValue = inputVal ? parseInt(inputVal, 10) : 0;
                        totalQtyDiv += currentValue;
                    })
                });
                let valDateProd = $thisParent.find('input.date-prod').val().trim();
                //Validation khi nhập số lượng
                if (qtyWorkOrder < totalQtyDiv) {
                    if (valDateProd === '') {
                        $(".btn-save-printlabel").addClass("disabled");
                        $thisParent.find('input.date-prod').addClass('border-danger');
                        $(".table-print-label .message .error-input").html('<p class="alert alert-danger">Chưa có ngày sản xuất. Vui lòng nhập.</p>');
                    } else {
                        $(".btn-save-printlabel").addClass("disabled");
                        $(e.target).addClass('border-danger');
                        $(".table-print-label .message .error-total").html('<p class="alert alert-danger">Tổng số lượng chia đang lớn hơn số lượng sản xuất. Vui lòng kiểm tra lại.</p>')
                    }
                } else if (qtyWorkOrder > totalQtyDiv) {
                    if (valDateProd === '') {
                        $(".btn-save-printlabel").addClass("disabled");
                        $thisParent.find('input.date-prod').addClass('border-danger');
                        $(".table-print-label .message .error-input").html('<p class="alert alert-danger">Chưa có ngày sản xuất. Vui lòng nhập.</p>');
                        $(".table-print-label .message .error-total").html('');
                    } else {
                        $(".btn-save-printlabel").addClass("disabled");
                        $(e.target).addClass('border-danger');
                        $thisParent.find('input.date-prod').removeClass('border-danger');
                        $(".table-print-label .message .error-input").html('');
                        $(".table-print-label .message .error-total").html('<p class="alert alert-danger">Tổng số lượng chia đang nhỏ hơn số lượng sản xuất. Vui lòng kiểm tra lại.</p>')
                    }
                } else {
                    if (valDateProd === '') {
                        $(".btn-save-printlabel").addClass("disabled");
                        $thisParent.find('input.date-prod').addClass('border-danger');
                        $(".table-print-label .message .error-input").html('<p class="alert alert-danger">Chưa có ngày sản xuất. Vui lòng nhập.</p>');
                    } else {
                        $thisParent.find('input.date-prod').removeClass('border-danger');
                        $(".table-print-label .message .error-total").html('');
                        $(".btn-save-printlabel").removeClass("disabled");
                        $('input[type="number"]').each(function () {
                            $(this).removeClass('border-danger');
                        })
                        $(".table-print-label .message .error-input").html("");
                    }

                }
            });

            let heightTable = $('.table-print-label table').height();
            if (heightTable > 400) {
                $('.table-print-label table thead').addClass('table-freezer-row');
                $('.table-print-label table thead').each(function (i, elem) {
                    $(elem).find('tr').eq(0).css('top', 0);
                    $(elem).find('tr').eq(1).css('top', $(elem).find('tr').eq(0).height());
                    $(elem).find('tr').eq(1).css('z-index', 0);
                });
            }

            $(".table-print-label tbody tr").each(function (i, elem) {
                $(elem).find('td.item-freezer').eq(1).css('left', 82);
                $(elem).find('td.item-freezer').eq(2).css('left', 199);
                $(elem).find('td.item-freezer').eq(3).css('left', 280);
                $(elem).find('td.item-freezer').eq(4).css('left', 357);
                $(elem).find('td.item-freezer').eq(5).css('left', 434);
                $(elem).find('td.item-freezer').eq(6).css('left', 511);
                $(elem).find('td.item-freezer').eq(7).css('left', 648);
            });

            $(".table-print-label thead tr").each(function (i, elem) {
                $(elem).find('th.item-freezer').eq(1).css('left', 82);
                $(elem).find('th.item-freezer').eq(2).css('left', 199);
                $(elem).find('th.item-freezer').eq(3).css('left', 280);
                $(elem).find('th.item-freezer').eq(4).css('left', 357);
                $(elem).find('th.item-freezer').eq(5).css('left', 434);
                $(elem).find('th.item-freezer').eq(6).css('left', 511);
                $(elem).find('th.item-freezer').eq(7).css('left', 648);
            });


            $.each(oldData, function (index, workOrdeData) {
                let rows = workOrdeData.rows;
                $.each(rows, function (rowIndex, row) {
                    $('tr[data-workorder="' + workOrdeData.workOrder + '"][data-typelabel="' + row.typeLabel + '"]').each(function (index) {
                        let $this = $(this);
                        $.each(row.dataItems, function (itemIndex, item) {   
                            if (index === itemIndex) {
                                $this.find('.date-prod').val(item.dateProd);
                                $this.find('.character-wo').val(workOrdeData.character);
                                $this.addClass("has-content");
                                $.each(item.machines, function (machineIndex, machine) {
                                    let shiftFlag = machine.shiftLabel.toLowerCase();
                                    $this.find(`td[data-flag="${shiftFlag}"]`).find(`input.machine-${machine.machineShift}`).val(machine.qtyDiv);
                                    $this.find('textarea').val(machine.remarks);
                                })
                            }
                        })
                    });
                });
            });
        })
        .catch(error => {
            alert(error);
        })
}