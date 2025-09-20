"use-strict";
// Đăng ký và hiển thị dữ liệu với thời gian thực
const connectionHub = new signalR.HubConnectionBuilder()
    .withUrl(`${window.baseUrl}materialTaskHub`)
    .configureLogging(signalR.LogLevel.Information) // Optional: Enable logging for debugging
    .build();
connectionHub.on('ReceiveInputValue', (inputClass, inputValue, parentValue) => {
    const inputElements = document.querySelectorAll('#'+ inputClass);
    if (inputElements) {
        inputElements.forEach((inputElement) => {
            inputElement.value = inputValue;
            inputElement.parentElement.parentElement.setAttribute('data-input', inputValue);
        })

        let parents = document.querySelectorAll('.is-checked');
        parents.forEach((item) => {
            if (item.getAttribute('data-workorder') == parentValue) {
                item.style.backgroundColor = '#ccc';
                
            }
        })
    } else {
        console.warn(`Element with ID ${inputClass} not found.`);
    }
});
// Function to start the connection
async function startConnection() {
    try {
        await connectionHub.start();
        console.log("Connected to SignalR hub");
    } catch (err) {
        console.error("Error while starting connection: ", err);
        // Retry connection after a delay
        setTimeout(startConnection, 5000); // Retry after 5 seconds
    }
}
// Handle connection closed event
connectionHub.onclose(async (error) => {
    // Attempt to reconnect after a delay
    await new Promise(resolve => setTimeout(resolve, 5000));
    startConnection();
});

// Start the connection
startConnection();

const checkTableAfterRender = document.getElementById('tableGetWOInMES');
document.addEventListener('DOMContentLoaded', function () {
    const tableData = document.querySelector('.table-data');

    $('#btnShowProcessForWO').on('click', function (e) {
        $('#selectToShowWorkOrder').toggleClass('d-none');
        $(this).toggleClass('up-chevron');
    });

    if (document.getElementById('btnShowProcessForWO')) {
        var selectLocations = document.getElementById('btnShowProcessForWO').parentElement;
        document.addEventListener('click', function (e) {
            if (!selectLocations.contains(e.target)) {
                if (!$('#selectToShowWorkOrder').hasClass('d-none')) {
                    $('#selectToShowWorkOrder').addClass('d-none');
                    $('#btnShowProcessForWO').removeClass('up-chevron');
                }
            }
        });
    }


    if ($('#selectToShowWorkOrder') != null) {
        $('#selectToShowWorkOrder .dropdown-item').each(function (i, elem) {
            $(elem).on('click', debounce((event) => {
                event.preventDefault();
                $('.table-see-inventoy').addClass('d-none');
                $(".table-inventory #inventoryContent").html("");
                if (checkTableAfterRender) {
                    if ($.fn.DataTable.isDataTable(checkTableAfterRender)) {
                        $(checkTableAfterRender).DataTable().destroy();
                    }
                }
                const beforeRender = document.querySelector(".before-render");
                if (tableData) {
                    tableData.classList.add('d-none');
                    beforeRender.classList.remove("d-none");
                }
                const valSelected = $(elem).data('value');
                const textSelected = $(elem).text();
                if (valSelected) {
                    // Hiển thị workorder 
                    const renderTable = document.getElementById('renderTable');
                    if (renderTable) {
                        renderTable.innerHTML = '';
                        processAjaxRenderWorkOrder(valSelected, textSelected);
                        $('#selectToShowWorkOrder').toggleClass('d-none');
                        $('.button-show-process .option-value').html(textSelected);
                        $('.button-show-process').toggleClass('up-chevron');
                    }

                } else {
                    window.location.reload();
                }
            }, 300));
        })
        SaveData();

        const flowInventory = document.querySelector('.btn-show-inventory');
        if (flowInventory) {
            flowInventory.addEventListener('click', debounce((event) => {
                let processCode = event.target.getAttribute('data-processCode');
                const beforeRender = document.querySelector(".before-render");
                beforeRender.classList.remove("d-none");
                tableData.classList.add('d-none');
                processingAjaxShowInventory(processCode);
            }, 300));
            const backTable = document.querySelector('.btn-back');
            backTable.addEventListener('click', debounce((event) => {
                event.target.parentElement.parentElement.classList.add('d-none');
                tableData.classList.remove('d-none');
            }, 300));
        }
    }
 
});

function debounce(func, wait) {
    let timeout;
    return function excutedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

function processAjaxRenderWorkOrder(processCode, processName) {

    fetch(`${window.baseUrl}Materials/GetWorkOrder`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            processCode: processCode
        }),
    })
        .then(async response => {
            if (!response.ok) {
                const errorResponse = await response.json();
                throw new Error(`${response.status} - ${errorResponse.message}`);
            }
            return response.json();
        })
        .then(data => {
            setTimeout(() => {
                $('.btn-show-inventory').attr('data-processCode', processCode);
                document.querySelector('.before-render').classList.add('d-none');
                document.querySelector('.title-table').innerHTML = `<h6 class="fs-4">Công đoạn ${processName}</h6>`;
                document.querySelector('.table-data').classList.remove('d-none');
                const renderTable = document.getElementById('renderTable');
                renderTable.innerHTML = renderHtml(data.workOrder);
                initializeDataTable(data.oldData);
            }, 1000);
        })
        .catch(error => {
            alert(error);
        });
}

function renderHtml(dataRender) {
    let html = '';
    dataRender.map((item, index) => {
        let character = "";
        let inputCharacter = '<input type="text" class="character-input item-' + index + '" id="characters_' + index + '" />';

        if (item.character != '') {
            inputCharacter = '<input type="text" class="character-input disabled" id="characters_' + index + '" value="' + item.character + '" />';
            character = "data-input=" + item.character;
        }

        let classContentAdd = '';
        if (character !== '') {
            classContentAdd = 'class="content-add"';
        }

        if (inputCharacter == '') {
            character = 'data-input=""';
        }

        html += `
        <tr ${classContentAdd} data-workorder="${item.workOrderNo}" data-qtybase="${item.orderedValue}">
            <td class="align-middle item-workorderno" rowspan="2" data-input="${item.workOrderNo}">${item.workOrderNo}</td>
            <td class="align-middle item-productcode" rowspan="2" data-input="${item.productCode}">${item.productCode}</td>
            <td class="align-middle item-lotno" rowspan="2" data-input="${item.lotNo}">${item.lotNo}</td>
            <td class="align-middle item-orderno" rowspan="2" data-input="${item.orderedValue}">${item.orderedValue}</td>
            <td class="align-middle td-character" rowspan="2" ${character}>
                <div class="input-group">
                    ${inputCharacter}
                </div>
            </td>
            <td class="align-middle td-date-time" rowspan="2">
                <div class="input-group">
                    <input type="text" title="Ngày dự định sản xuất" class="date-input date-intended" placeholder="dd/mm/yyyy" id="dateIntended_${index}_${item.processCode}" value="" />
                </div>
            </td>
            <td class="align-middle td-date-time time-select" rowspan="2">
                <div class="input-group">
                    <input type="text" title="Thời gian dự định sản xuất" class="time-input time-intended" placeholder="HH:mm" id="timeIntended_${index}_${item.processCode}" value="" />
                    <i class='bx bxs-time-five'></i>
                </div>
            </td>
            <td class="align-middle td-import-qty td-import-qty-0">
                <div class="input-group">
                    <input type="number" title="Số lượng nhập" class="import-value-0" id="importValue${index}_0" value="" />
                </div>
            </td>
            <td class="align-middle td-date-time">
                <div class="input-group">
                    <input type="text" title="Ngày nhập NVL" class="date-input row-date date-import-1" placeholder="dd/mm/yyyy" id="dateImport_${index}_0_${item.processCode}" value="" />
                </div>
            </td>
            <td class="align-middle td-date-time time-select">
                <div class="input-group">
                    <input type="text" title="Thời gian nhập NVL" class="time-input row-time time-import-1" placeholder="HH:mm" id="timeImport_${index}_0_${item.processCode}" value="" />
                    <i class='bx bxs-time-five'></i>
                </div>
            </td>
            <td class="align-middle d-none" data-input="${item.processCode}">
                <input type="text" class="processCode" value="${item.processCode}" />
            </td>
            <td class="align-middle d-none" rowspan="2">
                <input type="checkbox" class="check-item" />
            </td>
        </tr>
        <tr ${classContentAdd} data-workorder="${item.workOrderNo}" data-qtybase="${item.orderedValue}">
            <td class="align-middle item-workorderno d-none" data-input="${item.workOrderNo}">${item.workOrderNo}</td>
            <td class="align-middle item-productcode d-none"  data-input="${item.productCode}">${item.productCode}</td>
            <td class="align-middle item-lotno d-none" data-input="${item.lotNo}">${item.lotNo}</td>
            <td class="align-middle item-orderno d-none" data-input="${item.orderedValue}">${item.orderedValue}</td>
            <td class="align-middle td-character d-none" ${character}>
                <div class="input-group">
                    ${inputCharacter}
                </div>
            </td>
            <td class="align-middle td-date-time d-none">
                <div class="input-group">
                    <input type="text" title="Ngày dự định sản xuất" class="date-input date-intended" placeholder="dd/mm/yyyy" id="dateIntended_${index}_${item.processCode}" value="" />
                </div>
            </td>
            <td class="align-middle td-date-time time-select d-none">
                <div class="input-group">
                    <input type="text" title="Thời gian dự định sản xuất" class="time-input time-intended" placeholder="HH:mm" id="timeIntended_${index}_${item.processCode}" value="" />
                    <i class='bx bxs-time-five'></i>
                </div>
            </td>
            <td class="align-middle td-import-qty td-import-qty-1">
                <div class="input-group">
                    <input type="number" title="Số lượng nhập" class="import-value-1" id="importValue${index}_1" value="" />
                </div>
            </td>
            <td class="align-middle td-date-time">
                <div class="input-group">
                    <input type="text" title="Ngày nhập NVL" class="date-input row-date date-import-2" placeholder="dd/mm/yyyy" id="dateImport_${index}_1_${item.processCode}" value="" />
                </div>
            </td>
            <td class="align-middle td-date-time time-select">
                <div class="input-group">
                    <input type="text" title="Thời gian nhập NVL" class="time-input row-time time-import-2" placeholder="HH:mm" id="timeImport_${index}_1_${item.processCode}" value="" />
                    <i class='bx bxs-time-five'></i>
                </div>
            </td>
            <td class="align-middle d-none" data-input="${item.processCode}">
                <input type="text" class="processCode" value="${item.processCode}" />
            </td>
            <td class="align-middle d-none">
                <input type="checkbox" class="check-item" />
            </td>
        </tr>
        `;
    });
    return html;
}

function initializeDataTable(oldData) {
    var today = new Date();
    var dd = String(today.getDate()).padStart(2, '0');
    var mm = String(today.getMonth() + 1).padStart(2, '0');
    var yyyy = today.getFullYear();
    today = dd + '/' + mm + '/' + yyyy;

    if (checkTableAfterRender) {
        let isChanging = false;
        $(".date-input").datepicker({
            dateFormat: "dd/mm/yy",
            showOn: "both",
            buttonImage: "../images/calendar.png",
            buttonImageOnly: true,
            buttonText: "Chọn ngày",
            showAnim: "slideDown",
            firstDay: 1,
            dayNamesMin: ["CN", "T2", "T3", "T4", "T5", "T6", "T7"],
            monthNames: ["Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"],
            minDate: today,
        });
        $(".time-input").timepicker({
            timeFormat: "HH:mm",
            interval: 60,
            scrollbar: true,
            change: function () {
                if (!isChanging) {
                    isChanging = true;
                    $(this).trigger("change");
                    isChanging = false;
                }
            }
        });
        $(".bxs-time-five").click(function (event) {
            event.preventDefault();
            $(event.target).parent().find("input").trigger("change").focus();
            event.stopPropagation();
        });

        /*Xử lý dữ liệu*/
        let columnTable = $('#renderTable tr');
        columnTable.each(function (i, elem) {
            $(elem).find('td').not('.d-none').find("input").on("change", function (e) {
                e.preventDefault();  
                var value = $(e.target).val();
                var inputId = $(e.target).attr('id');
                if ($(e.target).hasClass('border-danger')) {
                    $(e.target).removeClass('border-danger');
                }
                let regexDigital = /^\d+$/;
                if ($(e.target).hasClass('character-input')) {
                    if (regexDigital.test(value)) {
                        swal({
                            title: "Lỗi",
                            text: 'Bắt buộc phải là ký tự A-Z',
                            icon: 'error',
                        })
                            .then((isConfirmed) => {
                                if (isConfirmed) {
                                    $(e.target).addClass('border-danger');
                                }
                            })
                    } else {
                        $(e.target).removeClass('border-danger');
                    }
                }
                $(e.target).parent().parent().attr("data-input", value);
                var parentValue = $(e.target).parent().parent().parent().data('workorder');

                // Validation value input
                if ($(e.target).parent().parent().parent().find('input.date-intended').val() == '') {
                    $(e.target).parent().parent().parent().find('input.date-intended').addClass('border-danger');
                }
                if ($(e.target).parent().parent().parent().find('input.time-intended').val() == '') {
                    $(e.target).parent().parent().parent().find('input.time-intended').addClass('border-danger');
                }
                if ($(e.target).parent().parent().parent().find('input.import-value-0').val() == '') {
                    $(e.target).parent().parent().parent().find('input.import-value-0').addClass('border-danger');
                }
                if ($(e.target).parent().parent().parent().find('input.date-import-1').val() == '') {
                    $(e.target).parent().parent().parent().find('input.date-import-1').addClass('border-danger');
                }
                if ($(e.target).parent().parent().parent().find('input.time-import-1').val() == '') {
                    $(e.target).parent().parent().parent().find('input.time-import-1').addClass('border-danger');
                }
              
                //Truyền dữ liệu cho taskHub
                connectionHub
                    .invoke("SendInputValue", inputId, value, parentValue)
                    .catch(err => console.error(err.toString()));

                let qtyOrdered = parseInt($(e.target).parent().parent().parent().find('td.item-orderno').attr('data-input'), 10);

                let valDateProd = $(e.target).parent().parent().parent().find('input.date-intended').val() != '' ? $(e.target).parent().parent().parent().find('input.date-intended').val().split('/') : [];
                let valTimeProd = $(e.target).parent().parent().parent().find('input.time-intended').val() != '' ? $(e.target).parent().parent().parent().find('input.time-intended').val().split(':') : [];
                if (valDateProd.length > 0 && valTimeProd.length > 0) {
                    let dateTimeProd = new Date(valDateProd[2], (valDateProd[1] - 1), valDateProd[0], valTimeProd[0], valTimeProd[1], 0);
                    let dateImport_1 = $(e.target).parent().parent().parent().find('input.date-import-1').val() != undefined && $(e.target).parent().parent().parent().find('input.date-import-1').val() != '' ? $(e.target).parent().parent().parent().find('input.date-import-1').val().split('/') : [];
                    let timeImport_1 = $(e.target).parent().parent().parent().find('input.time-import-1').val() != undefined && $(e.target).parent().parent().parent().find('input.time-import-1').val() != '' ? $(e.target).parent().parent().parent().find('input.time-import-1').val().split(':') : [];
                    let dateTimeImport_1 = new Date(dateImport_1[2], (dateImport_1[1] - 1), dateImport_1[0], timeImport_1[0], timeImport_1[1], 0);
                    dateTimeProd = dateTimeProd.getTime();
                    dateTimeImport_1 = dateTimeImport_1.getTime();

                    if (dateTimeImport_1 >= dateTimeProd && dateTimeImport_1 != null) {
                        swal({
                            title: "Lỗi",
                            text: 'Ngày nhập NVL lớn hơn ngày sản xuất',
                            icon: 'error',
                        })
                            .then((isConfirmed) => {
                                if (isConfirmed) {
                                    $('#saveDataLocation').addClass('disabled');
                                    $(e.target).parent().parent().parent().find('input.date-import-1').addClass('border-danger');
                                    $(e.target).parent().parent().parent().find('input.time-import-1').addClass('border-danger');
                                }
                            })
                    }
                }

                if ($(e.target).parent().parent().parent().find('input.processCode').val() == '01050') {
                    if ($(e.target).parent().parent().parent().find('input.character-input').val() == '') {
                        $(e.target).parent().parent().parent().find('input.character-input').addClass('border-danger');
                    }
                }

                $(e.target).parent().parent().parent().parent().each(function (i, trElem) {
                    $(trElem).find('tr[data-workorder="' + parentValue + '"]').addClass("is-checked");
                    if ($(trElem).find('tr[data-workorder="' + parentValue + '"] input').hasClass('border-danger')) {
                        $('#saveDataLocation').addClass('disabled');
                    } else {
                        $('#saveDataLocation').removeClass('disabled');
                    }

                    let qtyInputEnter0 = $(trElem).find('tr[data-workorder="' + parentValue + '"] input.import-value-0').val();
                    let qtyInputEnter1 = $(trElem).find('tr[data-workorder="' + parentValue + '"] input.import-value-1').val();
                    
                    if (qtyOrdered < parseInt(qtyInputEnter0)) {
                        swal({
                            title: "Lỗi",
                            text: 'Số lượng đang lớn hơn. Vui lòng kiểm tra lại!',
                            icon: 'error',
                        })
                            .then((isConfirmed) => {
                                if (isConfirmed) {
                                    $(trElem).find('tr[data-workorder="' + parentValue + '"] input.import-value-0').addClass('border-danger');
                                    $(trElem).find('tr[data-workorder="' + parentValue + '"] input.import-value-0').val('');
                                }
                            })
                    }
                    if (qtyInputEnter1 != '') {
                        let totalQtyEnter = parseInt(qtyInputEnter0) + parseInt(qtyInputEnter1);
                        if (qtyOrdered < totalQtyEnter) {
                            swal({
                                title: "Lỗi",
                                text: 'Số lượng đang lớn hơn. Vui lòng kiểm tra lại!',
                                icon: 'error',
                            })
                                .then((isConfirmed) => {
                                    if (isConfirmed) {
                                        $(trElem).find('tr[data-workorder="' + parentValue + '"] input.import-value-1').addClass('border-danger');
                                        $(trElem).find('tr[data-workorder="' + parentValue + '"] input.import-value-1').val('');
                                    }
                                });
                        }
                        if ($(trElem).find('tr[data-workorder="' + parentValue + '"] input.date-import-2').val() == '') {
                            $(trElem).find('tr[data-workorder="' + parentValue + '"] input.date-import-2').addClass('border-danger');
                        }
                        if ($(trElem).find('tr[data-workorder="' + parentValue + '"] input.time-import-2').val() == '') {
                            $(trElem).find('tr[data-workorder="' + parentValue + '"] input.time-import-2').addClass('border-danger');
                        }
                    }
                });

                if ($(e.target).attr('type') == 'number') {
                    $(e.target).parent().parent().parent().attr('data-qtyimport', $(e.target).val());
                }

            });
        });

        //Hiển thị dữ liệu đã được thêm
        if (oldData.length) {
            for (let i = 0; i < oldData.length; i++) {
                let dataOld = oldData[i];

                let dateProdStr = formatDateToDDMMYYYY(dataOld.dateProd);
                let timeProdStr = formatTimeToHHSS(dataOld.timeProd);

                let rowTb = $('#renderTable tr[data-workorder="' + dataOld.workOrder + '"]');
                rowTb.addClass('has-content');
                rowTb.removeClass('content-add');
                let arrRows = dataOld.rows;
                rowTb.each(function (i, elem) {
                    $(elem).find('input.date-intended').val(dateProdStr);
                    $(elem).find('input.time-intended').val(timeProdStr);
                });
                for (let j = 0; j < arrRows.length; j++) {
                    rowTb.each(function (i, elem) {
                        let dateImportStr = formatDateToDDMMYYYY(arrRows[j].dateImport);
                        let timeImportStr = formatTimeToHHSS(arrRows[j].timeImport);
                        $(elem).find('input[type="number"].import-value-' + j + '').val(arrRows[j].qtyImport);
                        $(elem).find('input.date-import-' + (j + 1) + '').val(dateImportStr);
                        $(elem).find('input.time-import-' + (j + 1) + '').val(timeImportStr);
                    });
                }
            }
        }
    }
}

function SaveData() {
    $("#saveDataLocation").on("click", function (e) {
        e.preventDefault();
        $(this).addClass("active");
        $('.message').html('');
        $(".spinner-border").removeClass('d-none');
        var rows = $('#tableGetWOInMES tbody tr.is-checked');
        var arrRows = [];
        let processCode = $('.processCode').val();
        let arrChecked = [];
        rows.each(function () {
            let $row = $(this);
            let workOrder = $row.data('workorder');
            let productCode = $row.find('td.item-productcode').data('input');
            let lotNo = $row.find('td.item-lotno').data('input');
            let qtyBase = $row.find('td.item-orderno').data('input');
            let character = $row.find('td.td-character').data('input') != undefined ? $row.find('td.td-character').data('input').toUpperCase() : '';
            let qtyImport = $row.find('td.td-import-qty').find('input').val();
            let dateProd = $row.find('td').find('input.date-intended').val();
            let timeProd = $row.find('td').find('input.time-intended').val();
            let dateImport_0 = $row.find('td').find('input.row-date').val();
            let timeImport_0 = $row.find('td').find('input.row-time').val();
            let existingEntry = arrRows.find(item => item.workOrder === workOrder);
            if (existingEntry) {
                existingEntry.rows.push({
                    workOrder: workOrder,
                    itemCode: productCode,
                    lotno: lotNo,
                    dateInput: dateImport_0,
                    timeInput: timeImport_0,
                    qtyImport: qtyImport ?? 0,
                });
            } else {
                arrRows.push({
                    workOrder: workOrder,
                    itemCode: productCode,
                    lotno: lotNo,
                    qty: qtyBase,
                    character: character,
                    dateIntended: dateProd,
                    timeIntended: timeProd,
                    processCode: processCode,
                    rows: [
                        {
                            workOrder: workOrder,
                            itemCode: productCode,
                            lotno: lotNo,
                            dateInput: dateImport_0,
                            timeInput: timeImport_0,
                            qtyImport: qtyImport ?? 0,
                        }
                    ]
                });
            }
            $('#tableGetWOInMES tbody tr[data-workorder="' + workOrder + '"]').each(function (i, elem) {
                $(elem).removeClass('is-checked');
                $(elem).addClass('content-add');
                $(elem).css('background-color');
            });

        });
        if (arrRows.length > 0) {
            SendDataToDB(arrRows);
            $(this).removeClass("active");
            $(this).addClass("disabled");
            //$(this).addClass("d-none");
        }
    });
}

function SendDataToDB(data) {
    fetch(`${window.baseUrl}Materials/SaveDataInDb`, {
        method: "POST",
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            jsonStr: JSON.stringify(data)
        }),
    })
        .then(async response => {
            if (!response.ok) {
                const errorResponse = await response.json();
                throw new Error(`${response.status} - ${errorResponse.message}`);
            }
            return response.json();
        })
        .then(data => {
            setTimeout(() => {
                alert(data.message);
                $(".spinner-border").addClass('d-none');
                $(".btn-redirect-ex").removeClass('disabled');
            }, 800);

        })
        .catch(error => {
            alert(error)
        })
}

function processingAjaxShowInventory(processCode) {
    fetch(`${window.baseUrl}Materials/SeeInventory`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            processCode: processCode
        }),
    })
        .then(async response => {
            if (!response.ok) {
                const errorResponse = await response.json();
                throw new Error(`${response.status} - ${errorResponse.message}`);
            }
            return response.json();
        })
        .then(data => {
            setTimeout(() => {
                $('.before-render').addClass('d-none');
                $('.table-data').addClass('d-none');
                $(".table-inventory tr th.item").remove();
                $(".table-inventory tr.work-order-item").remove();
                $('.table-see-inventoy').removeClass('d-none');
                let WOResults = data.dataAllWO;
                let RMResults = data.dataAllRM;
                renderInventoryHtml(WOResults, RMResults);
            }, 800);

        })
        .catch(error => {
            alert(error)
        })
}

function renderInventoryHtml(WOResults, RMResults) {
    let halfAmount = 0;
    let qtyUnused = 0;

    // Hiển thị workorder
    let sortArr = WOResults;
    for (let i = 0; i < sortArr.length; i++) {
        let classOnGoing = "";
        if (sortArr[i].statusname == "On Going Production") {
            classOnGoing = "wo-on-going";
        }
        if (sortArr[i].qtyUsed > 0) {
            $(".table-inventory #inventoryContent")
                .append(`
                <tr class="work-order-item ` + classOnGoing + `" data-work_order="` + sortArr[i].workOrder + `" data-qty_use="` + sortArr[i].qtyUsed + `">
                <th class="item">` + sortArr[i].workOrder + `</th>
                <th class="item">` + sortArr[i].productCode + `</th>
                <th class="item">` + sortArr[i].lotNo + `</th>
                <th class="item">` + sortArr[i].qtyUsed + `</th>
                </tr>`);
        }
    }

    // Hiển thị content RM 
    RMResults.map((item, index) => {
        let inventory = item.inventory;
        qtyUnused = parseInt(inventory, 10) - halfAmount;
        $('.table-inventory .rm-code-inventory').append(`<th colspan="2" class="item"><span>` + item.inputGoodsCode + `</span></th>`);
        $(".table-inventory .rm-name-inventory").append(`<th colspan="2" class="item"><span>` + item.inputGoodsName + `</span></th>`);
        $(".table-inventory .total-inventory").append(`<th colspan="2" class="item" data-rm_code="` + item.inputGoodsCode + `"><span>` + inventory + `</span></th>`);
        $(".table-inventory .half-amount").append(`<th colspan="2" class="item" data-rm_code="` + item.inputGoodsCode + `"><span>` + halfAmount + `</span></th>`);
        $(".table-inventory .qty-unused").append(`<th colspan="2" class="item" data-rm_code="` + item.inputGoodsCode + `"><span>` + qtyUnused + `</span></th>`);
        $(".table-inventory .title-head").append(`<th class="item"><span>SL chưa sử dụng</span></th>`);
        $(".table-inventory .title-head").append(`<th class="item"><span>Tồn còn lại</span></th>`);
        $("#inventoryContent .work-order-item").append(`<td class="rm-code" data-rm_code="` + item.inputGoodsCode + `"></td>`);
        $("#inventoryContent .work-order-item").append(`<td class="calc-inventory" data-rm_code="` + item.inputGoodsCode + `"></td>`);
    });

    // Hiển thị số lượng cần dùng của RM và Workorder
    let listOrderWithRM = [];
    sortArr.map(itemWO => {
        let listWOForQtyUnused = itemWO.listWorkOrderItems;
        listOrderWithRM.push(...listWOForQtyUnused);
    });
   
    listOrderWithRM.map(itemCheck => {
        itemCheck.map(item => {
            let inputgoodsCode = item.inputGoodsCode;
            $("#inventoryContent .work-order-item[data-work_order='" + item.workOrderItem + "'] td.rm-code[data-rm_code='" + inputgoodsCode + "']").text(item.qtyUnusedForWo).attr("data-qty_rm_used", item.qtyUnusedForWo);
            $("#inventoryContent .work-order-item[data-work_order='" + item.workOrderItem + "'] td.rm-code[data-rm_code='" + inputgoodsCode + "']").attr("data-qty_used", item.qtyUserForWo);
        });
    });

    // Tính toán số lượng còn lại
    $(".table-inventory .total-inventory").find("th.item").each(function (index, elem) {
        let totalInventory = $(elem).text();
        let valParent = $(elem).data("rm_code");
        let newHalfAmount = 0;
        let newQtyUnused = 0;

        $(".table-inventory tbody .work-order-item").each(function (i, el) {
            let dataQtyUse = $(el).find("td.rm-code[data-rm_code=" + valParent + "]").data("qty_rm_used");
            if (dataQtyUse != undefined) {
                let qtyCalc = parseInt(totalInventory, 10) - parseInt(dataQtyUse, 10);
                totalInventory = qtyCalc;
                newQtyUnused = qtyCalc;
                newHalfAmount += parseInt(dataQtyUse, 10);
                $(el).find("td.calc-inventory[data-rm_code=" + valParent + "]").html("<span>" + qtyCalc + "</span>").attr("data-unused_qty", qtyCalc);
                if (qtyCalc < 0) {
                    $(el).find("td.calc-inventory[data-rm_code=" + valParent + "]").addClass("text-danger background-error");
                }
                $(".table-inventory .qty-unused").find("th.item[data-rm_code=" + valParent + "]").each(function (index, elem) {
                    $(elem).html("<span>" + newQtyUnused + "</span>");
                    if (newQtyUnused < 0) {
                        $(elem).addClass("text-danger background-error");
                    }
                });
                $(".table-inventory .half-amount").find("th.item[data-rm_code=" + valParent + "]").each(function (index, elem) {
                    $(elem).html("<span>" + newHalfAmount + "</span>");
                });
            }
        });
    });
    // Thêm class vào các bảng để freezer cho các cột và dòng
    var heightTable = $(".table-inventory").height();
    var widthTable = $(".table-inventory").width();

    if (heightTable > 700) {
        $(".table-inventory thead").addClass("freezer-row");
        $(".table-inventory thead").addClass("freezer-column");
    } else {
        $(".table-inventory thead").removeClass("freezer-row");
        $(".table-inventory thead").removeClass("freezer-column");
    }

    if (widthTable > 1300) {
        $(".table-inventory tbody").addClass("freezer-column");
        $(".table-inventory thead").addClass("freezer-column");
    } else {
        $(".table-inventory tbody").removeClass("freezer-column");
        $(".table-inventory thead").removeClass("freezer-column");
    }

    $(".table-inventory tbody.freezer-column tr").each(function (i, elem) {
        var firstChildWidth = $(elem).find("th.item").first().width();
        var secondChild = $(elem).find("th.item").eq(1);
        var third = $(elem).find("th.item").eq(2);
        var fourth = $(elem).find("th.item").eq(3);
        secondChild.css("left", firstChildWidth + 18);
        third.css("left", secondChild.width() + firstChildWidth + (17 + 18));
        fourth.css("left", third.width() + secondChild.width() + firstChildWidth + (17 * 2 + 18));
    });

    $(".table-inventory thead.freezer-column tr.title-head").each(function (i, elem) {
        var firstChildWidth = $(elem).find("th:not(.item)").first().width();
        var secondChild = $(elem).find("th:not(.item)").eq(1);
        var third = $(elem).find("th:not(.item)").eq(2);
        var fourth = $(elem).find("th:not(.item)").eq(3);
        secondChild.css("left", firstChildWidth + 18);
        third.css("left", secondChild.width() + firstChildWidth + (17 + 18));
        fourth.css("left", third.width() + secondChild.width() + firstChildWidth + (17 * 2 + 18));
    });

    $(".table-inventory thead.freezer-row").each(function (i, elem) {
        var heightRow = $(elem).find("tr").first().height();
        $(elem).find("tr").eq(1).css("top", heightRow);
        $(elem).find("tr").eq(2).css("top", $(elem).find("tr").eq(1).height() + heightRow);
        $(elem).find("tr").eq(3).css("top", $(elem).find("tr").eq(2).height() + $(elem).find("tr").eq(1).height() + heightRow);
        $(elem).find("tr").eq(4).css("top", $(elem).find("tr").eq(3).height() + $(elem).find("tr").eq(1).height() + $(elem).find("tr").eq(2).height() + heightRow);
        $(elem).find("tr").eq(5).css("top", $(elem).find("tr").eq(4).height() + $(elem).find("tr").eq(3).height() + $(elem).find("tr").eq(1).height() + $(elem).find("tr").eq(2).height() + heightRow);
    });
}

function formatDateToDDMMYYYY(dateString) {
    const date = new Date(dateString);
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
}
function formatTimeToHHSS(timeString) {
    const [hours, minutes, seconds] = timeString.split(':');
    return `${hours}:${minutes}`;
}