'use-strict';
document.addEventListener('DOMContentLoaded', function (e) {
    $('#selectMaterials').val('');
    $(".input-date-view").val('');
    $(".input-date-view").datepicker({
        dateFormat: "mm/yy",
        showOn: "both",
        buttonImage: "../images/calendar.png",
        buttonImageOnly: true,
        showAnim: "slideDown",
        firstDay: 1,
        changeMonth: true,
        changeYear: true,
        dayNamesMin: ["CN", "T2", "T3", "T4", "T5", "T6", "T7"],
        monthNamesShort: ["Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"],
        onChangeMonthYear: function (year, month, inst) {
            var firstDay = new Date(year, month - 1, 1); // Ngày đầu tiên của tháng
            $(this).datepicker('setDate', firstDay);
            $(this).datepicker('hide');
            $(this).trigger('change');

        } 
    });

    $(".input-date-view").on('change', function (e) {
        e.preventDefault();
        if ($("#selectMaterials").val() != '') {
            $('#btnViewMaterial').removeClass('disabled');
            $(this).blur();
        } else {
            $("#selectMaterials").focus();
        }
    });

    $("#selectMaterials").on('change', function (e) {
        e.preventDefault();
        $('#containerDataManageMaterials .info-manage-materials').html('');
        $('#containerDataManageMaterials #tabLotMaterials').html('');
        if ($(".input-date-view").val() != '') {
            $('#btnViewMaterial').removeClass('disabled');
        } else {
            $(".input-date-view").focus();
        }
    });

    //Hiển thị sổ quản lý NVL
    $('#btnViewMaterial').on('click', function (e) {
        e.preventDefault();
        let valMaterial = $('#selectMaterials').val();
        let monthShow = $('.input-date-view').val();
        let formData = new FormData();
        formData.append('selectMaterials', valMaterial);
        formData.append('selectMonth', monthShow);
        $(this).addClass('disabled');
        $('#containerDataManageMaterials').addClass('d-none');
        $('#containerDataManageMaterials').after('<div class="spinner-grow custom-spinner"></div>');
        setTimeout(() => {
            fetch(`${window.baseUrl}flowmaterials/getContentMaterials`, {
                method: 'POST',
                body: formData
            })
                .then(async response => {
                    if (!response.ok) {
                        const errorResponse = await response.json();
                        throw new Error(`${response.status} - ${errorResponse.message}`);
                    }
                    return response.json();
                })
                .then(data => {
                    let infoMaterials = data.infoMaterials;
                    let results = data.infoLotMaterials;
                    let htmlInfo = '';
                    $('.spinner-grow').remove();
                    $('#containerDataManageMaterials').removeClass('d-none');
                    $('#monthSelected').text(monthShow);
                    infoMaterials.forEach(item => {
                        htmlInfo += `<p class="mb-0">Mã NVL: ${item.materialCode}</p>
                        <p class="ms-3 mb-0">Tên NVL: ${item.materialName}</p>`;
                    });
                    $('#containerDataManageMaterials .info-manage-materials').html(htmlInfo);

                    if (results.length > 0) {
                        $('#btnExported').removeClass('d-none');
                    }

                    if (results.length > 0) {
                        let htmlTabLink = '';
                        let htmlTabByLot = '';
                        results.forEach(item => {
                            htmlTabLink += `<div class="col-2 text-center mb-2">
                                <button class="btn btn-secondary btn-show-detail-lot" id="btnShow${item.lotNo}" data-lot="${item.lotNo}" type="button">${item.lotNo}</button>
                            </div>`;
                        });
                        htmlTabByLot += `</div>`;
                        $('#tabLotMaterials').html(htmlTabLink);
                    }
                })
                .catch(error => {
                    swal('Lỗi', error.message, 'error');
                    $('.swal-button--confirm').on('click', function (e) {
                        window.location.reload();
                    });
                });
        }, 2000);
    });
    $('#showDetailMaterial .btn-close-detail').on('click', function (e) {
        e.preventDefault();
        $('#showDetailMaterial').modal('hide');
        $('.btn-show-detail-lot').removeClass('active');
    });

    $('#showDetailMaterial').on('hidden.bs.modal', function (e) {
        $(this).find('.content-by-lot tbody').html('');
    });
    $('body').on('click', '#tabLotMaterials .btn-show-detail-lot', function (e) {
        e.preventDefault();
        $(this).toggleClass('active');
        $('#showDetailMaterial').modal('show');
        let dataLot = $(this).data('lot');
        $('#showDetailMaterial .btn-exported-book').attr("data-lotmaterial", dataLot);

        $('#tabInfoContents #importedContents tbody').html('');
        $('#tabInfoContents #importedContents tfoot .total-imported').html('');

        // Gửi request lấy thông tin NVL theo lô
        let valMaterial = $('#selectMaterials').val();
        let monthShow = $('.input-date-view').val();
        let formData = new FormData();
        formData.append('selectMaterial', valMaterial);
        formData.append('selectMonth', monthShow);
        formData.append('lotMaterial', dataLot);
        $('#tabInfoContents #importedContents').append('<div class="spinner-grow custom-spinner"></div>');
        fetch(`${window.baseUrl}flowmaterials/getContentByLot`, {
            method: 'POST',
            body: formData
        })
            .then(async response => {
                if (!response.ok) {
                    const errorResponse = await response.json();
                    throw new Error(`${response.status} - ${errorResponse.message}`);
                }
                return response.json();
            })
            .then(data => {
                let resultItemported = data.infoImported;
                let resultMaterialUsed = data.infoMaterialUsed;
                // Hiển thị nhập NVL
                setTimeout(() => {
                    if (resultItemported.length > 0) {
                        let htmlImportedMaterials = '';
                        let totalImported = 0;
                        resultItemported.forEach(item => {
                            totalImported += parseInt(item.qty, 10);
                            htmlImportedMaterials += `<tr>
                                        <td><div class='date-imported'>${item.dateImported}</div></td>
                                        <td><div class='request-no'>${item.requestNo}</div></td>
                                        <td><div class='lot-material'>${item.lotNo}</div></td>
                                        <td><div class='qty-imported'>${item.qty}</div></td>
                                        <td><div class='user-imported'>${item.userImported}</div></td>
                                    </tr>`;
                        });
                        $('#tabInfoContents #importedContents tbody').html(htmlImportedMaterials);
                        $('#tabInfoContents #importedContents tfoot .total-imported').html(totalImported);
                        $('#tabInfoContents #importedContents .table-content').removeClass('d-none');
                        $('#tabInfoContents #importedContents .custom-spinner').remove();
                    }
                    if (resultMaterialUsed.length > 0) {
                        let htmlMaterialUsed = '';
                        let totalUsed = 0;
                        resultMaterialUsed.forEach(item => {
                            totalUsed += parseInt(item.qtyUsed, 10);
                            let dateUsed = new Date(item.dateUsed);

                            // Tạo định dạng chuỗi ngày tháng mong muốn
                            dateUsed = dateUsed.toLocaleString("vi-VN", {
                                day: "2-digit",
                                month: "2-digit",
                            });

                            dateUsed = dateUsed.replace('-', '/');
                            htmlMaterialUsed += `<tr>
                                <td><div class='date-used'>${dateUsed}</div></td>
                                <td><div class='product-code'>${item.productCode}</div></td>
                                <td><div class='lot-production'>${item.lotProduction}</div></td>
                                <td><div class='request-no'>${item.requestNo}</div></td>
                                <td><div class='lot-material'>${item.lotMaterial}</div></td>
                                <td><div class='qty-picking'>${item.qtyPicking}</div></td>
                                <td><div class='qty-used'>${item.qtyUsed}</div></td>
                                <td style="width: 150px;"><div class='inventory-used'>${item.inventoryUsed}</div></td>
                                <td><div class='user-exported'>${item.userExported}</div></td>
                            </tr>`;
                        });
                        $('#tabInfoContents #usedMaterialContents tbody').html(htmlMaterialUsed);
                        $('#tabInfoContents #usedMaterialContents tfoot .total-used').html(totalUsed);
                    }
                }, 1000);
            })
            .catch(error => {
                swal('Lỗi', error.message, 'error');
                $('.swal-button--confirm').on('click', function (e) {
                    window.location.reload();
                });
            })
    });
  
    // Xử lý tải về sổ quản lý
    $('body').on('click', '.btn-exported-book', function (e) {
        e.preventDefault();
        let valMaterial = $('#selectMaterials').val();
        let formData = new FormData();
        $(this).addClass('disabled');

        let listMaterialImported = [];
        let listMaterialUsed = [];

        $('#importedContents table tbody tr').each(function (index, elem) {
            let objItems = {
                dateImported: $(elem).find('.date-imported').text().trim(),
                requestNo: $(elem).find('.request-no').text().trim(),
                lotNo: $(elem).find('.lot-material').text().trim(),
                qty: $(elem).find('.qty-imported').text().trim(),
                userImported: $(elem).find('.user-imported').text().trim(),
                note: '',
            };
            listMaterialImported.push(objItems);
        });

        $('#usedMaterialContents table tbody tr').each(function (index, elem) {
            let objItems = {
                dateUsed: $(elem).find('.date-used').text().trim(),
                productCode: $(elem).find('.product-code').text().trim(),
                lotProduction: $(elem).find('.lot-production').text().trim(),
                requestNo: $(elem).find('.request-no').text().trim(),
                lotMaterial: $(elem).find('.lot-material').text().trim(),
                qtyPicking: $(elem).find('.qty-picking').text().trim(),
                qtyUsed: $(elem).find('.qty-used').text().trim(),
                qtyDifference: "",
                inventoryUsed: $(elem).find('.inventory-used').text().trim(),
                userExported: $(elem).find('.user-exported').text().trim(),
                noteExported: '',
            };
            listMaterialUsed.push(objItems);
        });

        let lotMaterial = $(this).data('lotmaterial');

        formData.append('selectMaterials', valMaterial);
        formData.append('lotMaterial', lotMaterial);
        formData.append('infoMaterialImported', JSON.stringify(listMaterialImported));
        formData.append('infoMaterialUsed', JSON.stringify(listMaterialUsed));
        setTimeout(function () {
            fetch(`${window.baseUrl}FlowMaterials/ExportData`, {
                method: 'POST',
                body: formData
            })
                .then(async response => {
                    if (!response.ok) {
                        const errorResponse = await response.json();
                        throw new Error(`${response.status} - ${errorResponse.message}`);
                    }
                    return response.json();
                })
                .then(data => {
                    var result = window.atob(data.encodeFileName);
                    var excelName = data.fileDownloadName;
                    var buffer = new ArrayBuffer(result.length);
                    var bytes = new Uint8Array(buffer);
                    for (let i = 0; i < result.length; i++) {
                        bytes[i] = result.charCodeAt(i);
                    }
                    var blodArr = new Blob([bytes], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                    saveAs(blodArr, excelName);
                    swal({
                        title: 'Thông báo',
                        text: 'Tải về thành công!',
                        icon: 'success',
                        buttons: ["Tiếp tục xem", "Tải lại trang"],
                    }).then((isConfirmed) => {
                        if (isConfirmed) {
                            window.location.reload();
                        } else {
                            return;
                        }
                    });
                })
                .catch(error => {
                    swal('Lỗi', error.message, 'error');
                    $('.swal-button--confirm').on('click', function (e) {
                        window.location.reload();
                    });
                });
        },2000);

    });
});