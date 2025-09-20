// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
"use-strict";
document.addEventListener('DOMContentLoaded', function () {
    // Xử lý tải về checksheet
    if ($('#listWorkOrdersProduction').length > 0) {
        new DataTable($('#listWorkOrdersProduction'), {
            language: {
                info: 'Trang _PAGE_ của _PAGES_ trang',
                infoEmpty: 'Không có bản ghi nào',
                infoFiltered: '(Lọc từ _MAX_ bản ghi)',
                lengthMenu: 'Hiển thị _MENU_ trên một trang',
                zeroRecords: 'Xin lỗi không có kết quả',
                search: "Tìm kiếm: ",
                emptyTable: "Không có dữ liệu"
            },
            ordering: false,
        });

        $('#listWorkOrdersProduction').on('click', '.btn-show-download', function (e) {
            e.preventDefault();
            $('#showSelectCheckSheetForDownload').modal('show');
            var workorder = $(this).attr('data-workorder');
            var positionCode = $(this).attr('data-positioncode');
            fetch(`${window.baseUrl}admin/getChecksheetWithWO`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    workOrder: workorder,
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
                    let dataChecksheets = data.dataChecksheets;
                    let htmlOptions = '';
                    dataChecksheets.forEach(item => {
                        htmlOptions += `<option value="${item.checksheetVerId}" 
                        data-filepath="${item.filePath}" 
                        data-sheetname="${item.sheetName}">${item.fileName}_v${item.versionNumber}</option>`;
                    });
                    $('#selectChecksheet').append(htmlOptions);
                })
                .catch(error => {
                    console.log(error);
                });
            $('#showSelectCheckSheetForDownload #btnCreateFile').attr('data-workorder', workorder);
        });

        $('#showSelectCheckSheetForDownload .btn-close').on('click', function (e) {
            e.preventDefault();
            $('#showSelectCheckSheetForDownload').modal('hide');
        });

        $('body').on('change', '#selectChecksheet', function (e) {
            e.preventDefault();
            fetch(`${window.baseUrl}admin/getchecksheetposition`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json;'
                },
                body: JSON.stringify({
                    checksheetVerId: $(this).find(':selected').val()
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
                    let listPositions = data.listPositions || [];
                    let htmlOptions = '';
                    if (listPositions.length > 0) {
                        $('#selectPositionDownload').parent().removeClass('d-none');
                        listPositions.forEach(item => {
                            htmlOptions += `<option value="${item.positionCode}">${item.positionCode}</option>`;
                        });
                    }
                  
                    $('#selectPositionDownload').append(htmlOptions);
                    $('#selectPositionDownload').on('change', function (e) {
                        e.preventDefault();
                        $('#btnCreateFile').attr('data-positioncode', $(this).find(':selected').val());
                    });
                })
                .catch(error => {
                    console.log(error);
                });
            $('#btnCreateFile').removeClass('disabled');
        });

        $('#btnCreateFile').on('click', function (e) {
            e.preventDefault();
            $(this).parent().append('<span class="spinner-border"></span>');
            let checksheetVerId = $('#showSelectCheckSheetForDownload #selectChecksheet').find(':selected').val();
            let filePath = $('#showSelectCheckSheetForDownload #selectChecksheet').find(':selected').attr('data-filepath');
            let sheetName = $('#showSelectCheckSheetForDownload #selectChecksheet').find(':selected').attr('data-sheetname');
            let workOrder = $(this).attr('data-workorder');
            let positionCode = $(this).attr('data-positioncode') || "";
            if (checksheetVerId != '') {
                fetch(`${window.baseUrl}admin/DownloadChecksheet`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        checksheetVerId: checksheetVerId,
                        workOrder: workOrder,
                        filePath: filePath,
                        sheetName: sheetName,
                        positionCode: positionCode
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
                        var result = window.atob(data.encodeFileName);
                        var excelName = data.fileDownloadName;
                        var buffer = new ArrayBuffer(result.length);
                        var bytes = new Uint8Array(buffer);
                        for (let i = 0; i < result.length; i++) {
                            bytes[i] = result.charCodeAt(i);
                        }
                        var blodArr = new Blob([bytes], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                        saveAs(blodArr, excelName);
                        $(this).parent().find('.spinner-border').remove();
                        swal('Thông báo', 'Tải về thành công!', 'success', {
                            buttons: [false, "Ok"],
                        }).then((isConfirmed) => {
                            if (isConfirmed) {
                                window.location.reload();
                            } else {
                                return;
                            }
                        });
                    })
                    .catch(error => {
                        $(this).parent().find('.spinner-border').remove();
                        swal('Thông báo', error.message, 'error', {
                            buttons: [false, "Ok"],
                        }).then((isConfirmed) => {
                            if (isConfirmed) {
                                $('#selectPositionDownload').val('');
                            } else {
                                return;
                            }
                        });
                    })
            }
        });
    }

    // Xử lý upload checksheet
    if ($('.upload-checksheet').length > 0) {
        $('#selectLocationChild').val('');
        $('#selectLocationChild').on('change', function (e) {
            e.preventDefault();
            let locationChild = $(this).val();
            fetch(`${window.baseUrl}Checksheets/GetInfoByPosition`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    locationChildCode: locationChild
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
                    $('#renderDetailsLocationChild').removeClass('d-none');
                    let htmlSelect = '';
                    if (data.lines.length > 0) {
                        let lines = data.lines;
                        htmlSelect += `
                        <div class="form-check w-100">
                            <label class="form-check-label" for="selectAllLines">Áp dụng cho tất cả các line trong vị trí</label>
                            <input type="checkbox" id="selectAllLines" name="selectLines" class="form-check-input" checked/>
                        </div>  
                        <div class="select-items mt-3 d-none" id="componentSelectLines">
                            <label>Chọn line/máy áp dụng:</label>
                            <select class="form-select" name="selectSomeLines" id="selectLines" multiple>`;
                        lines.forEach(item => {
                            htmlSelect += `<option value="${item.idPosition}" selected="selected">Line/Máy ${item.idLine}</option>`;
                        });
                        htmlSelect += `</select>   
                        </div>`;
                    }   
                    $('#renderDetailsLocationChild').html(htmlSelect);
                })
                .catch(error => {
                    alert(error);
                    $('#selectLocationChild').val('');
                    $('#renderDetailsLocationChild').html('').addClass('d-none');
                })
        });

        $('#renderDetailsLocationChild').on('change', '#selectAllLines', function (e) {
            e.preventDefault();
            $('#componentSelectLines').addClass('d-none');
        });

        $('#filePathExcel').val('');
        $('#checksheetCode').val('');
        $('#selectLocation').val('');
        $('#isChangeForm').prop('checked', false);

        $('#filePathExcel').on('change', function (e) {
            e.preventDefault();
            $(this).parent().find('p.text-danger').remove();
            let files = $(this)[0].files;
            let fileNameArr = files[0].name.split(" ");
            let checksheetCode = fileNameArr[0].match(/^([A-Z]+\d+-GW\d{3}-\d+)/);
            $('#checksheetCode').val(checksheetCode[1]);
        });

        $('#selectLocation').on('change', function (e) {
            e.preventDefault();
            fetch(`${window.baseUrl}checksheets/getlocationchild`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    idLocation: $(this).val(),
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
                    if (data.dataRender.length > 0) {
                        $('#layoutSelectLocationChild').removeClass('d-none');
                        let htmlRender = '<option value="">--Vị trí thao tác--</option>';
                        data.dataRender.forEach(item => {
                            htmlRender += `<option value="${item.id}">${item.locationNameC}</option>`;
                        });
                        $('#selectLocationChild').html(htmlRender);
                    } else {
                        $('#layoutSelectLocationChild').addClass('d-none');
                    }
                })
                .catch(error => {
                    console.log(error);
                })
        })

        $('.btn-upload-checksheet').on('click', function (e) {
            e.preventDefault();
            $('#listFiles').html('');
            let checkContinue = true;
            if ($('#filePathExcel').val() == '') {
                $('#filePathExcel').addClass('border-danger');
                checkContinue = false;
                $('#filePathExcel').parent().append('<p class="text-danger fst-italic">Vui lòng chọn file checksheet!</p>');
            }
            if ($('#selectLocation').val() == '') {
                $('#selectLocation').addClass('border-danger');
                checkContinue = false;
                $('#selectLocation').parent().append('<p class="text-danger fst-italic">Vui lòng chọn công đoạn!</p>');
            }
            if ($('#checksheetCode').val() == '') {
                $('#checksheetCode').addClass('border-danger');
                checkContinue = false;
                $('#checksheetCode').parent().append('<p class="text-danger fst-italic">Vui lòng thêm mã checksheet!</p>');
            }
            if ($('#checksheetType').val() == '') {
                $('#checksheetType').addClass('border-danger');
                checkContinue = false;
                $('#checksheetType').parent().append('<p class="text-danger fst-italic">Vui lòng chọn loại checksheet!</p>');
            }
          
            if (checkContinue) {
                let locationChildId = $('#selectLocationChild').val();
                let locationId = $('#selectLocation').val();
                let lines = $('#selectLines').val();
                let checksheetCode = $('#checksheetCode').val();
                let isChange = $('#isChangeForm').is(':checked');

                let files = $('#filePathExcel')[0].files;
                let totalFiles = files.length;
                let uploadedFiles = 0;

                for (let i = 0; i < totalFiles; i++) {
                    var sizeInBytes = files[i].size;
                    var sizeInKB = (sizeInBytes / 1024).toFixed(2); 
                    let fileItem = document.createElement('div');
                    fileItem.className = 'file-name';
                    fileItem.innerHTML = `Chuẩn bị tải <strong>${files[i].name}</strong> / <strong>${sizeInKB}KB</strong>`;
                    $('#listFiles').append(fileItem);

                    const fileFormData = new FormData();
                    fileFormData.append('file', files[i]);
                    fileFormData.append('checksheetCode', checksheetCode);
                    fileFormData.append('locationId', locationId);
                    fileFormData.append('locationChildId', locationChildId);
                    fileFormData.append('lineChecksheet', JSON.stringify(lines));
                    fileFormData.append('isChange', isChange);
                    fileFormData.append('checksheetType', checksheetType);
                    new Promise(resolve => setTimeout(resolve, 1000));
                    uploadSingleFile(fileFormData, totalFiles, uploadedFiles, fileItem, files[i].name);
                    uploadedFiles++;
                }
               
            } else {
                document.getElementById('alertContent').classList.remove('d-none');
                document.getElementById('alertContent').innerHTML = 'Chưa đủ dữ liệu!';
            }
        });
    }

    // Hiển thị toàn bộ checksheets
    if ($('#allChecksheets').length > 0) {
        new DataTable($('#allChecksheets'), {
            language: {
                info: 'Trang _PAGE_ của _PAGES_ trang',
                infoEmpty: 'Không có bản ghi nào',
                infoFiltered: '(Lọc từ _MAX_ bản ghi)',
                lengthMenu: 'Hiển thị _MENU_ trên một trang',
                zeroRecords: 'Xin lỗi không có kết quả',
                search: "Tìm kiếm: ",
                emptyTable: "Không có dữ liệu"
            },
            ordering: false,
            columnDefs: [
                {
                    targets: 0,
                    width: "150px"
                },
                {
                    targets: 1,
                    width: "200px"
                },
            ]
        });
    }

    // Xử lý cập nhật lại checksheet
    if ($('.update-checksheet').length > 0) {
        // Hàm tiện ích để định dạng ngày
        const formatDate = (date) => {
            const dd = String(date.getDate()).padStart(2, '0');
            const mm = String(date.getMonth() + 1).padStart(2, '0');
            const yyyy = date.getFullYear();
            return `${dd}/${mm}/${yyyy}`;
        };

        const todayFormatted = formatDate(new Date());

        // Khởi tạo trạng thái ban đầu của các checkbox
        $('#setTimeEffective').prop('checked', false);
        $('#setChecksheetItems').prop('checked', false);

        // Cấu hình DatePicker mặc định
        const defaultDatePickerOptions = {
            dateFormat: "dd/mm/yy",
            showOn: "both",
            buttonImage: "../../../images/calendar.png",
            buttonImageOnly: true,
            buttonText: "Chọn ngày",
            showAnim: "slideDown",
            firstDay: 1,
            dayNamesMin: ["CN", "T2", "T3", "T4", "T5", "T6", "T7"],
            monthNames: ["Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"],
        };

        // Hàm kiểm tra thời gian hợp lệ và bật/tắt nút
        const isValidTimeAndEnableButton = (timeInput, buttonSelector) => {
            let rawTime = timeInput.val();
            const $button = $(buttonSelector);

            if (rawTime.length > 5 || (rawTime.length > 0 && rawTime.length < 4)) { // 5 characters for HH:MM
                alert("Thời gian không hợp lệ! Vui lòng nhập định dạng HH:MM.");
                $button.addClass('disabled');
                return false;
            }

            if (!rawTime.includes(':') && rawTime.length === 4) {
                rawTime = rawTime.slice(0, 2) + ":" + rawTime.slice(2);
                timeInput.val(rawTime);
            } else if (!rawTime.includes(':') && rawTime.length < 4 && rawTime.length > 0) {
                alert("Thời gian không hợp lệ! Vui lòng nhập định dạng HH:MM.");
                $button.addClass('disabled');
                return false;
            }


            const [hours, minutes] = rawTime.split(':').map(Number);

            if (isNaN(hours) || isNaN(minutes) || hours < 0 || hours > 23 || minutes < 0 || minutes > 59) {
                alert("Thời gian không hợp lệ! Vui lòng nhập giờ trong khoảng 00:00 đến 23:59.");
                $button.addClass('disabled');
                return false;
            }

            const now = new Date();
            const inputTime = new Date(now.getFullYear(), now.getMonth(), now.getDate(), hours, minutes, 0, 0);

            if (inputTime <= now) {
                alert("Thời gian phải sau thời điểm hiện tại!");
                $button.addClass('disabled');
                return false;
            }

            $button.removeClass('disabled');
            return true;
        };


        // Sự kiện thay đổi thời gian áp dụng checksheet
        $('#setTimeEffective').on('change', function (e) {
            e.preventDefault();
            $('#renderConfigChecksheet').html(`
                <div class="title-sub text-center fs-5 mb-4">Thay đổi thời gian áp dụng checksheet</div>
                <div class="box-custom">
                    <div class="row">
                        <div class="col-6">
                            <div class="input-group">
                                <label for="dateEfffectiveValue">Ngày có hiệu lực</label>
                            </div>
                        </div>
                        <div class="col-6">
                            <div class="input-group">
                                <input type="text" class="date-efffective-input w-100" name="DateEffective" id="dateEfffectiveValue" value="${todayFormatted}" />
                            </div>
                        </div>
                    </div>
                    <div class="row mt-3">
                        <div class="col-6">
                            <div class="input-group">
                                <label for="timeEfffectiveValue">Thời gian có hiệu lực</label>
                            </div>
                        </div>
                        <div class="col-6">
                            <div class="input-group">
                                <input type="text" class="date-efffective-input w-100" name="TimeEffective" id="timeEfffectiveValue" value="" placeholder="Vui lòng nhập giờ áp dụng. Ví dụ 08:05" />
                            </div>
                        </div>
                    </div>
                </div>
                <div class="checksheet-submit-button text-end mt-4">
                    <button class="btn btn-primary disabled" id="updateChecksheetTime">Cập nhật</button>
                </div>
            `);

            $("#dateEfffectiveValue").datepicker(defaultDatePickerOptions);

            $("#timeEfffectiveValue").on('change', function (e) {
                e.preventDefault();
                isValidTimeAndEnableButton($(this), '#updateChecksheetTime');
                $(this).blur();
            });

            // Cập nhật thời gian áp dụng cho checksheet
            $('#updateChecksheetTime').on('click', async function (e) {
                e.preventDefault();
                if ($(this).hasClass('disabled')) {
                    return; // Ngăn không cho gửi yêu cầu nếu nút bị disabled
                }

                const checksheetVerId = $('#checksheetVerId').val();
                const dateEffectiveValue = $('#dateEfffectiveValue').val();
                const timeEfffectiveValue = $('#timeEfffectiveValue').val();

                try {
                    const response = await fetch(`${window.baseUrl}checksheets/updateversionchecksheet`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                        },
                        body: JSON.stringify({
                            checksheetVerId: checksheetVerId,
                            dateTimeEffective: dateEffectiveValue + ' ' + timeEfffectiveValue,
                            note: "Cập nhật và phê duyệt sử dụng checksheet"
                        })
                    });

                    if (!response.ok) {
                        const errorResponse = await response.json();
                        throw new Error(`${response.status} - ${errorResponse.message}`);
                    }

                    const data = await response.json();
                    alert(data.message);
                    window.location.reload();
                } catch (error) {
                    alert(error.message);
                }
            });
        });

        // Sự kiện thay đổi checksheet theo chủng loại, lô
        $('#setChecksheetItems').on('change', function (e) {
            e.preventDefault();
            $('#renderConfigChecksheet').html(`
                <div class="box-custom">
                    <div class="date-time-component">
                        <div class="title-sub text-center fs-5 mb-3">Nhập ngày, giờ hiệu lực</div>
                        <div class="row">
                            <div class="col-6">
                                <div class="input-group">
                                    <label class="mb-2" for="dateEfffectiveValue">Ngày có hiệu lực</label>
                                    <div class="datepickter-component w-100">
                                        <input type="text" class="date-efffective-input w-100" name="DateEffective" id="dateEfffectiveValue" value="${todayFormatted}" />
                                    </div>
                                </div>
                            </div>
                            <div class="col-6">
                                <div class="input-group">
                                    <label class="mb-2" for="timeEfffectiveValue">Thời gian có hiệu lực</label>
                                    <div class="timepicker-component w-100">
                                        <input type="text" class="date-efffective-input w-100" name="TimeEffective" id="timeEfffectiveValue" value="" placeholder="Vui lòng nhập giờ áp dụng..." />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="list-product-items d-none">
                        <hr />
                        <div class="title-sub text-center fs-5 mb-3">Thêm chủng loại, lô sử dụng checksheet</div>
                        <div class="row">
                            <div class="col-6">
                                <div class="input-group">
                                    <label class="mb-2" for="productCode_0">Chủng loại</label>
                                    <input type="text" class="info-product-input product-code-input w-100" name="ProductCode" id="productCode_0" value="" placeholder="Vui lòng nhập chủng loại sản xuất...." />
                                </div>
                            </div>
                            <div class="col-6">
                                <div class="input-group">
                                    <label class="mb-2" for="productLot_0">Số lô theo chủng loại</label>
                                    <input type="text" class="info-product-input product-lot-input w-100" name="ProductLot" id="productLot_0" value="" placeholder="Vui lòng nhập lô sản xuất...." />
                                </div>
                            </div>
                        </div>
                        <div class="text-center mt-3">
                            <button class="btn btn-success btn-sm btn-add-item"><i class="bx bx-plus"></i>Thêm chủng loại</button>
                        </div>
                    </div>
                </div>
                <div class="checksheet-submit-button text-end mt-4">
                    <button class="btn btn-primary disabled" id="updateChecksheetItems">Cập nhật</button>
                </div>
            `);

            let count = 0;
            // Sử dụng delegation event cho nút thêm item
            $('#renderConfigChecksheet').on('click', '.btn-add-item', function (e) {
                e.preventDefault();
                count++;
                $('#renderConfigChecksheet .list-product-items .message-error').remove();
                const htmlClone = `
                    <div class="row mt-3">
                        <div class="col-6">
                            <div class="input-group">
                                <label class="mb-2" for="productCode_${count}">Chủng loại</label>
                                <input type="text" class="info-product-input product-code-input w-100" name="ProductCode" id="productCode_${count}" value="" placeholder="Vui lòng nhập chủng loại sản xuất...." />
                            </div>
                        </div>
                        <div class="col-6">
                            <div class="input-group">
                                <label class="mb-2" for="productLot_${count}">Số lô theo chủng loại</label>
                                <input type="text" class="info-product-input product-lot-input w-100" name="ProductLot" id="productLot_${count}" value="" placeholder="Vui lòng nhập lô sản xuất...." />
                            </div>
                        </div>
                    </div>`;
                $(this).parent().before(htmlClone);
            });

            $("#dateEfffectiveValue").datepicker(defaultDatePickerOptions);

            $("#timeEfffectiveValue").on('change', function (e) {
                e.preventDefault();
                isValidTimeAndEnableButton($(this), '#updateChecksheetItems'); // Validate time for this section
                $('.list-product-items').removeClass('d-none');
                $(this).blur();
            });

            // Cập nhật checksheet theo chủng loại, lô
            $('#updateChecksheetItems').on('click', async function (e) {
                e.preventDefault();
                if ($(this).hasClass('disabled')) {
                    return; // Ngăn không cho gửi yêu cầu nếu nút bị disabled
                }

                const checksheetVerId = $('#checksheetVerId').val();
                const dateEffectiveValue = $('#dateEfffectiveValue').val();
                const timeEfffectiveValue = $('#timeEfffectiveValue').val();

                $('#renderConfigChecksheet .list-product-items .message-error').remove();

                const arrDataProducts = [];
                $('#renderConfigChecksheet .list-product-items .row').each(function () {
                    const productCode = $(this).find('.product-code-input').val();
                    const productLot = $(this).find('.product-lot-input').val();
                    if (productCode !== '' && productLot !== '') {
                        arrDataProducts.push({
                            productCode: productCode,
                            productLot: productLot,
                        });
                    }
                });

                if (arrDataProducts.length > 0) {
                    try {
                        const response = await fetch(`${window.baseUrl}checksheets/updateversionchecksheet`, {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json',
                            },
                            body: JSON.stringify({
                                checksheetVerId: checksheetVerId,
                                dateTimeEffective: dateEffectiveValue + ' ' + timeEfffectiveValue,
                                note: "Cập nhật và phê duyệt sử dụng checksheet theo item sản xuất",
                                infoProducts: JSON.stringify(arrDataProducts)
                            })
                        });

                        if (!response.ok) {
                            const errorResponse = await response.json();
                            throw new Error(`${response.status} - ${errorResponse.message}`);
                        }

                        const data = await response.json();
                        alert(data.message);
                        window.location.reload();
                    } catch (error) {
                        alert(error.message);
                    }
                } else {
                    $('#renderConfigChecksheet .list-product-items').append('<p class="text-danger message-error fst-italic">Vui lòng nhập thông tin chủng loại, lô cần thiết!</p>');
                }
            });
        });
    }

    // Xử lý khác
    const navItems = document.querySelectorAll('.list-item a');
    const currentUrl = window.location.href;
    navItems.forEach(item => {
        if (item.href == currentUrl) {
            item.classList.add('active');
            const collapseMenu = item.parentElement.querySelector('.collapse-menu-dropdown');
            if (collapseMenu) {
                collapseMenu.classList.toggle('show');
            }
            if (item.classList.contains('active')) {
                item.parentElement.parentElement.parentElement.classList.add('show');
            }
        }
    })
    $('.list-group-item a.active').parent().parent().parent().parent().find('a.item-link').addClass('active');
    $('.collapse-action').on('click', function (e) {
        e.preventDefault();
        $(this).find('i').toggleClass('right-collapse');
        $('.show-navs').toggleClass('collapse-sidebar');
        $('.left-width').toggleClass('collapse-sidebar');
        $('.collapse-menu-dropdown').toggleClass('collapse-fixed');
    });

    // Hiển thị datatable cho quản lý chỉ thị sản xuất
    if ($('#managerProductionsContent').length > 0) {
        new DataTable($('#managerProductionsContent #tableAllProductions'), {
            language: {
                info: 'Trang _PAGE_ của _PAGES_ trang',
                infoEmpty: 'Không có bản ghi nào',
                infoFiltered: '(Lọc từ _MAX_ bản ghi)',
                lengthMenu: 'Hiển thị _MENU_ trên một trang',
                zeroRecords: 'Xin lỗi không có kết quả',
                search: "Tìm kiếm: ",
                emptyTable: "Không có dữ liệu"
            },
            ordering: false,
        });

        $('.btn-continue-production').on('click', function (e) {
            e.preventDefault();
            let workOrder = $(this).attr('data-workorder');
            fetch(`${window.baseUrl}admin/resetworkorder`, {
                'method': 'POST',
                headers: {
                    'Content-Type': 'application/json;'
                },
                body: JSON.stringify({
                    workOrder: workOrder
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
                    alert(error);
                    window.location.reload();
                })
        });
    }

    if ($('#listAllTrayCreated').length > 0) {
        new DataTable($('#listAllTrayCreated'), {
            language: {
                info: 'Trang _PAGE_ của _PAGES_ trang',
                infoEmpty: 'Không có bản ghi nào',
                infoFiltered: '(Lọc từ _MAX_ bản ghi)',
                lengthMenu: 'Hiển thị _MENU_ trên một trang',
                zeroRecords: 'Xin lỗi không có kết quả',
                search: "Tìm kiếm: ",
                emptyTable: "Không có dữ liệu"
            },
            ordering: false,
            columnDefs: [
                {
                    targets: 0,
                    width: "80px"
                },
                {
                    targets: 2,
                    width: "130px"
                },
                {
                    targets: 3,
                    width: "100px"
                },
                {
                    targets: 4,
                    width: "100px"
                }
            ]
        });

        $('#listAllTrayCreated .btn-edit-item').on('click', function (e) {
            e.preventDefault();
            let trayId = $(this).attr('data-trayid');
            let trayCode = $(this).attr('data-traycode');
            let positionCode = $(this).attr('data-positioncode');
            let workOrderEdit = $(this).attr('data-workorder');
            let entryindex = $(this).attr('data-entryindex');
            fetch(`${window.baseUrl}admin/getformresult`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    trayId: trayId,
                    trayCode: trayCode,
                    positionCode: positionCode,
                    workOrder: workOrderEdit,
                    entryIndex: entryindex
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
                    let formRender = data.elementRender;
                    let htmlRender = '';
                    htmlRender += `<input type="hidden" class="formId" data-formid="${formRender.idForm}" data-entryindex="${entryindex}" />`;
                    let groupedDataByElementId = formRender.formFields.map(section => {
                        let groupedFormMapping = section.formMapping.reduce((acc, element) => {
                            let elementId = element.elementId;
                            if (!acc[elementId]) {
                                acc[elementId] = [];
                            }
                            acc[elementId].push(element);
                            return acc;
                        }, {});

                        return {
                            sectionId: section.sectionId,
                            colInRow: section.colInRow,
                            groupedFormMapping: Object.keys(groupedFormMapping).map(key => ({
                                elementId: key,
                                elements: groupedFormMapping[key]
                            }))
                        };
                    });
                    groupedDataByElementId.forEach(field => {
                        htmlRender += `<div class="section update-item" id="${field.sectionId}">
                                        <div class="row">`;

                        let dataElems = field.groupedFormMapping;
                        dataElems.forEach(col => {
                            let elementId_1 = col.elementId;
                            htmlRender += `<div class="col-${12 / field.colInRow}">
                                <div id='${elementId_1}' class='element-content'>`;

                            col.elements.forEach(elem => {
                                let classNone = elem.isHidden ? ' d-none' : '';
                                if (elem.typeInput == 'checkbox') {
                                    htmlRender += `<div class="field-render-condition form-check${classNone}">
                                        <label class="mb-2" for="${elem.fieldName}">${elem.label}</label>
                                        <input type="${elem.typeInput}" name="${elem.fieldName}" id="${elem.fieldName}" data-value="${elem.value}" data-fieldname="${elem.fieldName}" class="form-check-input" />
                                    </div>`;
                                } else {
                                    htmlRender += `<div class="field-render-condition input-group${classNone}">
                                        <label class="mb-2" for="${elem.fieldName}">${elem.label}</label>
                                        <input type="${elem.typeInput}" name="${elem.fieldName}" id="${elem.fieldName}" value="${elem.value}" data-fieldname="${elem.fieldName}" class="form-control w-100" />
                                    </div>`;
                                }
                            });

                            htmlRender += '</div></div>';
                        });

                        htmlRender += `</div>
                            </div>`;
                    });
                    $('.error-container').html(htmlRender);
                    $('#trayNumber').html(trayCode);
                    $('#updateResultsProcess').modal('show');
                })
                .catch(error => {
                    alert(error.message);
                })
        });

        $('#updateResultsProcess .btn-close').on('click', function (e) {
            e.preventDefault();
            $('#updateResultsProcess').modal('hide');
        })
    }

    if ($('.list-user').length > 0) {
        new DataTable($('.list-user table'), {
            language: {
                info: 'Trang _PAGE_ của _PAGES_ trang',
                infoEmpty: 'Không có bản ghi nào',
                infoFiltered: '(Lọc từ _MAX_ bản ghi)',
                lengthMenu: 'Hiển thị _MENU_ trên một trang',
                zeroRecords: 'Xin lỗi không có kết quả',
                search: "Tìm kiếm: ",
                emptyTable: "Không có dữ liệu"
            },
            ordering: false,
            columnDefs: [
                {
                    targets: 9,
                    width: "80px"
                }
            ]
        });
    }
    if ($('.form-add-user')) {
        $('.option-select').on('click', function () {
            $(this).toggleClass('open');
        });
        $('#selectSection').on('change', function () {
            if ($(this).find('option:selected').text() == "ADM") {
                $('#selectProcess').parent().parent().addClass('d-none');
            } else {
                $('#selectProcess').parent().parent().removeClass('d-none');
            }
            if ($(this).find('option:selected') != "") {
                $('#inputGroupPrepend2').text($(this).find('option:selected').text());
            } else {
                $('#inputGroupPrepend2').text("GW");
            }
        });
        $('#EmployeeNo').on('change', function () {
            $('#PasswordHash').val($(this).val());
        });
        $('.show-password').on('click', function (e) {
            e.preventDefault();
            let passwordField = $('#PasswordHash');
            if (passwordField.attr('type') === "password") {
                passwordField.attr('type', 'text');
            } else {
                passwordField.attr('type', 'password');
            }
        });
        var forms = document.querySelectorAll('.form-add-user .needs-validation');
        forms.forEach(function (form) {
            form.reset();
            form.addEventListener('submit', function (e) {
                e.preventDefault();
                var newUser = {};
                var inputs = form.querySelectorAll('input, select');
                inputs.forEach(function (input) {
                    newUser[input.name] = input.value;
                });
                fetch(`${window.baseUrl}usermanagement/addnewuser`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json; charset=utf-8;'
                    },
                    body: JSON.stringify(newUser)
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
        })
    }
    if ($('.table-all-items').length > 0) {
        new DataTable($('#allPositions'), {
            language: {
                info: 'Trang _PAGE_ của _PAGES_ trang',
                infoEmpty: 'Không có bản ghi nào',
                infoFiltered: '(Lọc từ _MAX_ bản ghi)',
                lengthMenu: 'Hiển thị _MENU_ trên một trang',
                zeroRecords: 'Xin lỗi không có kết quả',
                search: "Tìm kiếm: ",
                emptyTable: "Không có dữ liệu"
            },
            ordering: false,
            columnDefs: [
                {
                    targets: 5,
                    width: '70px'
                }
            ]
        });
        new DataTable($('#allTools'), {
            language: {
                info: 'Trang _PAGE_ của _PAGES_ trang',
                infoEmpty: 'Không có bản ghi nào',
                infoFiltered: '(Lọc từ _MAX_ bản ghi)',
                lengthMenu: 'Hiển thị _MENU_ trên một trang',
                zeroRecords: 'Xin lỗi không có kết quả',
                search: "Tìm kiếm: ",
                emptyTable: "Không có dữ liệu"
            },
            ordering: false,
            columnDefs: [
                {
                    targets: 4,
                    width: '70px'
                }
            ]
        });
    }

    if ($('.list-templates').length > 0) {
        new DataTable($('.list-templates table'), {
            language: {
                info: 'Trang _PAGE_ của _PAGES_ trang',
                infoEmpty: 'Không có bản ghi nào',
                infoFiltered: '(Lọc từ _MAX_ bản ghi)',
                lengthMenu: 'Hiển thị _MENU_ trên một trang',
                zeroRecords: 'Xin lỗi không có kết quả',
                search: "Tìm kiếm: ",
                emptyTable: "Không có dữ liệu"
            },
            ordering: false,
        });
        $('.btn-delete-item').on('click', function (e) {
            e.preventDefault();
            let idTemplate = $(this).data('id_template');
            fetch(`${window.baseUrl}createform/delete`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json; charset=utf-8;'
                },
                body: JSON.stringify({
                    formId: idTemplate
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
        })
    }

    function uploadSingleFile(fileFormData, totalFiles, uploadedFiles, fileItem, fileName) {
        return new Promise((resolve, reject) => {
            let xhr = new XMLHttpRequest();
            xhr.open('POST', `${window.baseUrl}checksheets/uploadexcel`, true);
            xhr.upload.onprogress = function (event) {
                if (event.lengthComputable) {
                    const percentComplete = (event.loaded / event.total) * 100;
                    $('#progressBar').val(percentComplete);
                    fileItem.innerHTML = `Đang tải <strong>${fileName}</strong> - ${Math.round(percentComplete)}%.`;
                }
            };

            xhr.onload = function () {
                if (xhr.status == 200) {
                    fileItem.innerHTML = `Tải xong <strong>${fileName}</strong>`;
                    var response = JSON.parse(xhr.response);
                    swal({
                        title: 'Thông báo',
                        text: 'Tải lên thành công!',
                        icon: 'success',
                        buttons: [false, "Ok"],
                    }).then((isConfirmed) => {
                        if (isConfirmed) {
                            window.location.reload();
                        } else {
                            return;
                        }
                    });
                    resolve();
                } else {
                    var response = JSON.parse(xhr.response);
                    const errorMessage = response ? response.message : 'Unknown error';
                    fileItem.innerHTML = `<p class="text-danger">Lỗi khi tải <strong>${fileName}</strong></p>`;
                    reject(new Error(errorMessage));
                }
            };
            xhr.send(fileFormData);
        })
    }
});