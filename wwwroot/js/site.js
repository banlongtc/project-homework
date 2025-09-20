// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
"use-strict";

document.addEventListener("DOMContentLoaded", function () {
    let formLogin = document.querySelector(".form-login");

    // Đăng nhập hệ thống
    if (formLogin != null) {
        LoginSystem();
    }
    if ($('.user-actions').length > 0) {
        const tabId = crypto?.randomUUID?.() || Math.random().toString(36);
        localStorage.setItem('tab_' + tabId, 'active');
        let hasLoggedOut = false;
        const logoutTime = 120 * 60 * 1000;
        let logoutTimer;

        function logout() {
            if (hasLoggedOut) return;
            hasLoggedOut = true;

            // Đăng xuất đồng bộ tab khác
            localStorage.removeItem('tab_' + tabId);
            localStorage.setItem('forceLogout', Date.now().toString());

            fetch(`${window.baseUrl}account/logout`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json; charset=utf-8;'
                }
            })
                .then(response => response.json())
                .then(data => {
                    setTimeout(() => {
                        // Dọn dẹp local/sessionStorage
                        const itemsToClear = [
                            'modalCheckPosition', 'dataWorkOrderProd', 'forceLogout', 'checkTabsAt'
                        ];
                        itemsToClear.forEach(key => localStorage.removeItem(key));
                        window.location.href = data.redirectUrl;
                    }, 1000);
                })
                .catch(error => {
                    alert(error);
                });
        }

        function resetTimer() {
            if (hasLoggedOut) return;
            clearTimeout(logoutTimer);
            logoutTimer = setTimeout(logout, logoutTime);
        }

        // Xử lý khi tab bị đóng hoặc chuyển sang trạng thái không hoạt động
        function cleanupTab() {
            localStorage.removeItem('tab_' + tabId);
            localStorage.setItem('checkTabsAt', Date.now().toString());
        }

        window.addEventListener('beforeunload', cleanupTab);
        window.addEventListener('pagehide', cleanupTab);

        // Đồng bộ logout giữa các tab
        window.addEventListener('storage', function (event) {
            if (event.key === 'checkTabsAt') {
                setTimeout(() => {
                    const stillOpen = Object.keys(localStorage).some(k => k.startsWith('tab_'));
                    if (!stillOpen) {
                        localStorage.setItem('forceLogout', Date.now().toString());
                    }
                }, 300);
            }

            if (event.key === 'forceLogout') {
                logout();
            }
        });

        document.addEventListener('DOMContentLoaded', resetTimer);
        ['mousemove', 'keyup', 'touchstart'].forEach(evt =>
            document.addEventListener(evt, resetTimer)
        );

        // Bắt sự kiện click nút logout thủ công
        $('.btn-logout').on('click', function (e) {
            e.preventDefault();
            logout();
        });

        // Ping định kỳ để cập nhật lastActiveAt
        setInterval(() => {
            fetch(`${window.baseUrl}account/ping`, {
                method: 'POST',
                credentials: 'include'
            });
        }, 20000);
    }
  
    // Xử lý nhập nguyên vật liệu
    const modalImport = document.getElementById('staticImportWire');
    const modalImportOthers = document.getElementById('staticImportOthers');

    if (modalImport != null) {
        modalImport.addEventListener('shown.bs.modal', function () {
            localStorage.removeItem("product_prev_import");
            localStorage.removeItem('materials_holding');
            $('.qty-receving').html('');
            $('#barcodeInput').focus();
            startInterval();
        });
        $('#staticImportWire .btn-close').on('click', function (e) {
            e.preventDefault();
            $('#staticImportWire').modal('hide');
            if (localStorage.getItem("product_prev_import")) {
                $('.btn-pause-import').trigger('click');
            }
        });
        //Nhập NVL khác
        $('.content-other-material').html('');
        modalImportOthers.addEventListener('shown.bs.modal', function () {
            localStorage.removeItem("product_prev_import");
            let arrProcess = [];
            $('#selectToLocationMaterials .dropdown-item').each(function (i, elem) {
                let value = $(elem).data('value');
                arrProcess.push(value);
            });
            if (arrProcess.length > 0) {
                setTimeout(() => {
                    fetch(`${window.baseUrl}importmaterials/getMaterialOthers`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json; charset="utf-8";'
                        },
                        body: JSON.stringify({
                            processCode: JSON.stringify(arrProcess)
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
                            setTimeout(() => {
                                let oldData = data.oldItems;
                                let htmlTable = '';
                                arrProcess.forEach(processItem => {
                                    htmlTable += `
                                    <div class="info-material-import" data-process=${processItem}>
                                        <div class="title-table"><p class="text-center">Bảng nguyên vật liệu công đoạn ${processItem}</p></div>
                                        <table class="table table-material-others">
                                            <thead>
                                                <tr>
                                                    <th class="align-middle">STT</th>
                                                    <th class="align-middle">Mã NVL</th>
                                                    <th class="align-middle">Lot NVL</th>
                                                    <th class="align-middle">Số lượng nhập</th>
                                                    <th class="align-middle">Chọn nhập</th>
                                                </tr>
                                            </thead>
                                            <tbody id="renderOtherMaterials">`;
                                    if (data.items.length > 0) {
                                        let i = 1;
                                        data.items.forEach(item => {
                                            if (processItem == item.processCode) {
                                                let itemList = item.listItems;

                                                itemList.forEach(itemChild => {
                                                    let classDisabled = '';
                                                    if (oldData.length > 0) {
                                                        oldData.forEach(itemOld => {
                                                            if (itemOld.itemCode == itemChild.materialCode &&
                                                                itemChild.lotMaterial == itemOld.lotNo && itemChild.idItem == itemOld.idRecev) {
                                                                classDisabled = 'class="d-none"';
                                                            }
                                                        });
                                                    }

                                                    htmlTable += `                                            
                                                    <tr ${classDisabled}>
                                                        <td class="align-middle">${i}</td >
                                                        <td class="align-middle"><div class="render-item product-code" data-idreceiving="${itemChild.idItem}">${itemChild.materialCode}</div></td>
                                                        <td class="align-middle"><div class="render-item lot-no">${itemChild.lotMaterial}</div></td>
                                                        <td class="align-middle"><div class="render-item qty-import">${itemChild.qtyReceiving}</div></td>
                                                        <td class="align-middle">
                                                            <div class="check-material">
                                                                <input type="checkbox" class="check-item" />
                                                            </div>
                                                        </td>
                                                    </tr>`;
                                                    i++;
                                                });
                                            }
                                        });
                                    } else {
                                        htmlTable += `<tr class="align-middle"><td colspan="5">Không có nguyên vật liệu trên Receiving Plan List</td></tr>`;
                                    }
                                    htmlTable +=
                                        `</tbody>
                                        </table>
                                        <div class="request-number-wrapper input-group">
                                            <label>Mã request: </label>
                                            <input type="text" class="request-value-other"/>
                                        </div>
                                    </div>`;
                                })

                                $('#staticImportOthers .content-other-material').html(htmlTable);

                                if ($('.content-other-material .table').height() > 360) {
                                    $('.content-other-material .table thead th').addClass('freezer-row');
                                }

                                let dataImport = [];
                                $('#renderOtherMaterials .check-item').on('click', function (e) {
                                    var $item = $(this).parent().parent().parent(); // Giả định mỗi dòng là 1 div.item
                                    var productCode = $item.find("div.product-code").text();
                                    var idRecev = $item.find("div.product-code").attr('data-idreceiving');
                                    var objIndex = dataImport.findIndex(x => x.idRecev === idRecev);

                                    if ($(this).is(':checked')) {
                                        // Nếu được check thì thêm vào nếu chưa có
                                        if (objIndex === -1) {
                                            var obj = {
                                                productCode: productCode,
                                                idRecev: idRecev,
                                                qty: $item.find("div.qty-import").text(),
                                                lotNo: $item.find("div.lot-no").text(),
                                                pouchNo: '',
                                                timeLimit: '',
                                                typeMaterial: 'Nguyên liệu khác',
                                                pauseStatus: '',
                                                requestNo: '',
                                            };
                                            dataImport.push(obj);
                                        }
                                    } else {
                                        // Nếu bỏ check thì xóa object khỏi danh sách
                                        if (objIndex !== -1) {
                                            dataImport.splice(objIndex, 1);
                                        }
                                    }
                                    $item.toggleClass('checked');
                                    localStorage.setItem('product_prev_import', JSON.stringify(dataImport));
                                });

                                $('.request-value-other').on('change', function (e) {
                                    e.preventDefault();
                                    $('.btn-save-others').removeClass('disabled');
                                    let dataSave = localStorage.getItem('product_prev_import') != undefined ? JSON.parse(localStorage.getItem('product_prev_import')) : [];
                                    $('#renderOtherMaterials tr.checked').each(function (i, elem) {
                                        let productCode = $(elem).find('div.product-code').text();
                                        let lotno = $(elem).find('div.lot-no').text();
                                        dataSave.forEach(item => {
                                            if (item.productCode == productCode && item.lotNo == lotno) {
                                                item.requestNo = $(e.target).val();
                                                item.pauseStatus = "Imported";
                                            }
                                        });
                                    });
                                    localStorage.removeItem('product_prev_import');
                                    localStorage.setItem('product_prev_import', JSON.stringify(dataSave));
                                });

                                $('.btn-save-others').on('click', function (e) {
                                    e.preventDefault();
                                    let dataSave = localStorage.getItem('product_prev_import') != undefined ? JSON.parse(localStorage.getItem('product_prev_import')) : [];
                                    saveOtherMaterialsImport(dataSave, "imported");
                                    localStorage.removeItem('product_prev_import', "");  
                                });

                            }, 500);

                        })
                        .catch(error => {
                            alert(error);
                        })
                }, 500);
            }
        });
        modalImportOthers.addEventListener('hide.bs.modal', function () {
            $('.content-qr-render input').each(function (i, elem) {
                $(elem).val('');
            });
            window.location.reload();
        });

        // Nhập NVL dây dẫn
        $('#barcodeInput').on('change', function (e) {
            $('.btn-convert-content').trigger('click');
        });
        $('.btn-convert-content').on('click', function (e) {
            e.preventDefault();
            var valueScan = $('#barcodeInput').val().toUpperCase();
            const specialChars = /[!@#$%^&*(),.?":{}|<>]/g; // Regex để tìm ký tự đặc biệt
            var arrVal = valueScan.split(specialChars);
            var dataImport = [];
            var checkProductCode = true;
            $('#barcodeInput').val(valueScan);
            var oldData = localStorage.getItem('product_prev_import') != undefined ? JSON.parse(localStorage.getItem('product_prev_import')) : [];
            var holdData = localStorage.getItem('materials_holding') != undefined ? JSON.parse(localStorage.getItem('materials_holding')) : [];
          
            if (valueScan.length > 0) {
                
                if (arrVal[0].length >= 11
                    && (arrVal[1] !== undefined && arrVal[1].length >= 2)
                    && (arrVal[2] !== undefined && arrVal[2].length >= 6)
                    && (arrVal[3] !== undefined && arrVal[3].length >= 0)
                    && (arrVal[4] !== undefined && arrVal[4].length >= 0)
                ) {
                    var nameCode = arrVal[0];
                    var pouchLot = arrVal[2];
                    var qtyRead = parseInt(arrVal[1], 10);
                    var pouchNo = arrVal[3] == '' ? '00' : arrVal[3];
                    var timeLimit = '';
                    if (arrVal[4] == '') {
                        $('.content-qr-render').before('<div class="input-timelimit mt-4 input-group justify-content-center" style="width:100%;"><label class="form-label" for=timeLimit>Nhập thời gian hết hạn:</label><input type="text" id="timeLimit" class="form-control" style="width: 200px;" /></div>');
                    } else {
                        timeLimit = arrVal[4];
                    }
                    if ($('#timeLimit').length > 0) {
                        $('#timeLimit').on('change', function (e) {
                            timeLimit = $(this).val();
                        });
                    }
                    let oldValueScanned = '';
                    if (holdData.length > 0) {
                        for (let i = 0; i < holdData.length; i++) {
                            if (nameCode == holdData[i].productCode && pouchLot == holdData[i].lotNo) {
                                qtyRead = parseInt(holdData[i].qty, 10);
                            }
                        }  
                        holdData = removeItem(holdData, nameCode, pouchLot, qtyRead);
                        localStorage.setItem('materials_holding', JSON.stringify(holdData));
                    }
                    if (oldData.length > 0) {
                        for (let i = 0; i < oldData.length; i++) {
                            if (nameCode == oldData[i].productCode && pouchLot == oldData[i].lotNo) {
                                qtyRead += parseInt(oldData[i].qty, 10);
                            } else {
                                checkProductCode = false;
                                oldValueScanned = oldData[i].productCode + '%' + oldData[i].qty + '%' + oldData[i].lotNo + '%' + oldData[i].pouchNo + '%' + oldData[i].timeLimit
                            }
                        }
                    }

                    if (checkProductCode) {
                        let typeMaterial = "Dây dẫn thường";
                        if (nameCode.endsWith('Y')) {
                            typeMaterial = 'Dây dẫn TYC';
                        }
                        var obj = {
                            productCode: nameCode,
                            qty: qtyRead,
                            lotNo: pouchLot,
                            pouchNo: pouchNo,
                            timeLimit: timeLimit,
                            typeMaterial: typeMaterial,
                            pauseStatus: '',
                            requestNo: '',
                            idRecev: 0,
                        }
                        dataImport.push(obj);
                        $('#barcodeInput').blur();
                        checkQtyMes(obj);
                        setTimeout(() => {
                            $('#barcodeInput').val('');
                            $('#barcodeInput').focus();
                        }, 1000);

                    } else {
                        alert("Đang thực hiện một mã khác vui lòng tạm dừng trước khi nhập mã mới");
                        $('#barcodeInput').val(oldValueScanned);
                        return;
                    }
                }
            } else {
                return;
            }
        });

        $('.content-other-material input').each(function (i, elem) {
            $(elem).on('change', function () {
                if ($(elem).hasClass('border-danger')) {
                    $(elem).removeClass('border-danger');
                    $('#staticImportOthers .content-other-material .error').remove();
                }
            });
        });

        $('.btn-save-import').on('click', function (e) {
            e.preventDefault();
            $('#enterRequestNo').modal('show');
            $('#staticImportWire').modal('hide');
        });
        $('.btn-reload-content').on('click', function () {
            $('#barcodeInput').val('');
            $('#barcodeInput').focus();
        });

        $('.btn-pause-import').on('click', function (e) {
            e.preventDefault();
            let productCodeUpdate = $('.content-qr-render .name-code').data('value');
            let dataSave = localStorage.getItem('product_prev_import') != undefined && localStorage.getItem('product_prev_import') != '' ? JSON.parse(localStorage.getItem('product_prev_import')) : {};
            if (dataSave.productCode == productCodeUpdate) {
                findProduct.pauseStatus = 'Holding';
            }

            if (dataSave.length > 0) {
                $(".overlay").removeClass('d-none');
                $('.spinner-border').removeClass('d-none');
                saveMaterialImport(dataSave, "paused");
                alert('Tạm dừng nhập');
            }
        });

        $('.btn-save-requestno').on('click', function (e) {
            e.preventDefault();
            let requestNo = $('#requestNo').val();
            let dataSave = localStorage.getItem('product_prev_import') != undefined ? JSON.parse(localStorage.getItem('product_prev_import')) : {};
            let productCodeUpdate = $('.content-qr-render .name-code').data('value');
            if (dataSave.productCode == productCodeUpdate) {
                dataSave.pauseStatus = 'Imported';
                dataSave.requestNo = requestNo;
            }
            if (productCodeUpdate == "" || productCodeUpdate == undefined) {
                $('#renderOtherMaterials tr.checked').each(function (index, elem) {
                    if (dataSave.productCode === $(elem).find('div.product-code').text()) {
                        dataSave.pauseStatus = 'Imported';
                        dataSave.requestNo = requestNo;
                    }
                });
            }

            saveMaterialImport(dataSave, "imported");
            localStorage.setItem('product_prev_import', ""); 
        });
    }


    //Datatable Theo dõi hết hạn nguyên liệu
    new DataTable('#tableTimeLimitMaterials', {
        language: {
            info: 'Trang _PAGE_ của _PAGES_ trang',
            infoEmpty: 'Không có bản ghi nào',
            infoFiltered: '(Lọc từ _MAX_ bản ghi)',
            lengthMenu: 'Hiển thị _MENU_ trên một trang',
            zeroRecords: 'Xin lỗi không có kết quả',
            emptyTable: "Không có dữ liệu",
            search: "Tìm kiếm: "
        },
        ordering: false,
        searching: false,
    });
    $('.search-history-layout input').on('change', function (e) {
        e.preventDefault();
        if ($(this).val() != "") {
            $('.search-history-layout button').removeClass('disabled');
        } else {
            $('.search-history-layout button').addClass('disabled');
        }
       
    });

    // Thay đổi mật khẩu
    $('.input-password input').on('change', function (e) {
        e.preventDefault();
        $(this).parent().find('button.btn-show-password').toggleClass('d-none');
    });
    $('#modalChangePassword .btn-show-password').on('click', function (e) {
        e.preventDefault();
        let typeInput = $(this).parent().parent().find('input').attr('type');
        if (typeInput == 'password') {
            $(this).html('<i class="bx bxs-hide"></i>');
            $(this).parent().parent().find('input').attr('type', 'text');
        } else {
            $(this).html('<i class="bx bxs-show"></i>');
            $(this).parent().parent().find('input').attr('type', 'password');
        }
    });
    $('#newPassword').on('change', function (e) {
        e.preventDefault();
        let valInput = $(this).val();
        let oldPassword = $('#oldPassword').val();
        let validatePassword = ValidatePasswordWithSpecialCharacters(valInput);
        if (valInput == oldPassword) {
            $(this).addClass('border-danger');
            $(this).parent().parent().find('div.message-error').html(`<p style="margin-bottom: 0;">Không được giống mật khẩu cũ</p>`);
        } else if (validatePassword != "") {
            $(this).addClass('border-danger');
            $(this).parent().parent().find('div.message-error').html(`<p style="margin-bottom: 0;">${validatePassword}</p>`);
        } else {
            $(this).removeClass('border-danger');
            $(this).parent().parent().find('div.message-error').html('');
        }
    });
    $('#confirmPassword').on('change', function (e) {
        e.preventDefault();
        let confirmValue = $(this).val();
        let newPassword = $('#newPassword').val();
        if (confirmValue === newPassword) {
            $('.btn-update-password').removeClass('disabled');
            $(this).removeClass('border-danger');
            $(this).parent().parent().find('div.message-error').html('');
        } else {
            $(this).addClass('border-danger');
            $(this).parent().parent().find('div.message-error').html(`<p style="margin-bottom: 0;">Mật khẩu không khớp</p>`);
        }

    });
    $('.btn-update-password').on('click', function (e) {
        e.preventDefault();
        let passwordUser = window.btoa($('#confirmPassword').val());
        let userId = $(this).data('userid');
        fetch(`${window.baseUrl}account/changepassword`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json; charset="utf-8";'
            },
            body: JSON.stringify({
                passwordUser: passwordUser,
                idUser: userId,
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
            })
            .catch(error => {
                alert(error);
            })
    })

    // Tìm kiếm thông tin NVL đã được viết phiếu
    $('#dateImportExported').datepicker({
        dateFormat: "dd/mm/yy",
        showOn: "both",
        buttonImage: "../images/calendar.png",
        buttonImageOnly: true,
        buttonText: "Chọn ngày",
        showAnim: "slideDown",
        firstDay: 1,
        dayNamesMin: ["CN", "T2", "T3", "T4", "T5", "T6", "T7"],
        monthNames: ["Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"],
    });
    $('.input-date-search').datepicker({
        dateFormat: "dd/mm/yy",
        showOn: "both",
        buttonImage: "../images/calendar.png",
        buttonImageOnly: true,
        buttonText: "Chọn ngày",
        showAnim: "slideDown",
        firstDay: 1,
        dayNamesMin: ["CN", "T2", "T3", "T4", "T5", "T6", "T7"],
        monthNames: ["Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"],
    });

    $('.search-history-layout input').val('');
    const tableHistory = document.getElementById('tableHistory');
    var today = new Date();
    var dd = String(today.getDate()).padStart(2, '0');
    var mm = String(today.getMonth() + 1).padStart(2, '0');
    var yyyy = today.getFullYear();
    today = dd + '/' + mm + '/' + yyyy;
    if (tableHistory) {
        searchDataWHExported(tableHistory, '', today, '', '');
    }
    $('#searchBtn').on('click', function (e) {
        e.preventDefault();
        $('.history-block .spinner-grow').removeClass('d-none');
        $(tableHistory).DataTable().destroy();
        $('.history-block .layout-content').addClass('d-none');
        $('#historyContent').html('');
        let textMaterial = $('#materialHistoryExported').val();
        let dateImport = $('#dateImportExported').val();
        let whPosition = $('#whLocation').val();
        let userExport = $('#userExport').val();
        if (textMaterial !== '' || dateImport !== '' || whPosition !== '' || userExport !== '') {
            searchDataWHExported(tableHistory ,textMaterial, dateImport, whPosition, userExport);
        } else {
            $('.history-block .spinner-grow').addClass('d-none');
            alert("Cần nhập thông tin cần tìm kiếm");
        }
    });    

});
// Tìm kiếm lịch sử phiếu xuất kho
function searchDataWHExported(tableHistory, textMaterial, dateImport, whPosition, userExport) {
    $('.history-block .spinner-grow').removeClass('d-none');
    fetch(`${window.baseUrl}materials/searchhistory`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json; charset="utf-8";'
        },
        body: JSON.stringify({
            itemCode: textMaterial ?? "",
            dateImport: dateImport ?? "",
            whPosition: whPosition ?? "",
            userId: userExport ?? ""
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
            setTimeout(() => {
                $('.history-block .layout-content').removeClass('d-none');
                let htmlResult = '';
                data.resultSearch.forEach(item => {
                    let newDateImport = new Date(item.dateImport);
                    let newStrDateImport = newDateImport.getFullYear() + '-' + String(newDateImport.getMonth() + 1).padStart(2, '0') + '-' + String(newDateImport.getDate()).padStart(2, '0');
                    let dateImport = new Date(newStrDateImport + ' ' + item.timeImport);
                    dateImport = 'Thứ ' + (dateImport.getDay() + 1) + ', ' + String(dateImport.getDate()).padStart(2, '0') + '/' + String(dateImport.getMonth() + 1).padStart(2, '0') + '/' + dateImport.getFullYear() + ' ' + String(dateImport.getHours()).padStart(2, '0') + ':' + String(dateImport.getMinutes()).padStart(2, '0') + ':' + String(dateImport.getSeconds()).padStart(2, '0');
                    htmlResult += `
                        <tr>
                            <td class="align-middle"><div class="text-center date-ex-item">${dateImport}</div></td>
                            <td class="align-middle"><div class="text-center item-code">${item.itemCode}</div></td>
                            <td class="align-middle"><div class="text-center qty-ex">${item.qtyExport}</div></td>
                            <td class="align-middle"><div class="text-center wh-location">${item.whPosition}</div></td>
                            <td class="align-middle"><div class="text-center unit-ex">${item.userExport}</div></td>
                        </tr>`;
                });

                $('.history-block .spinner-grow').addClass('d-none');

                $('#historyContent').html(htmlResult);
                if (tableHistory) {
                    new DataTable(tableHistory, {
                        language: {
                            info: 'Trang _PAGE_ của _PAGES_ trang',
                            infoEmpty: 'Không có bản ghi nào',
                            infoFiltered: '(Lọc từ _MAX_ bản ghi)',
                            lengthMenu: 'Hiển thị _MENU_ trên một trang',
                            zeroRecords: 'Xin lỗi không có kết quả',
                            emptyTable: "Không có dữ liệu"
                        },
                        searching: false,
                        ordering: false,
                        columnDefs: [
                            {
                                target: 0,
                                width: "200px"
                            },
                            {
                                target: 1,
                                width: "300px"
                            },
                            {
                                target: 2,
                                width: "200px"
                            },
                        ],
                        initComplete: function (settings, json) {
                            mergeRows();
                        },
                        drawCallback: function (settings) {
                            mergeRows();
                        },
                    });
                    function mergeRows() {
                        var rows = $(".list-item-ex table tbody tr");
                        var last = null;

                        rows.each(function (index, row) {
                            var cell = $(row).find('td:first');
                            if (last && cell.text() === last.text()) {
                                var rowspan = last.attr('rowspan') || 1;
                                rowspan = Number(rowspan) + 1;
                                last.attr('rowspan', rowspan);
                                cell.remove();
                            } else {
                                last = cell;
                            }
                        });
                    }
                }
            }, 2000);
        })
        .catch(error => {
            alert(error);
        })
}
// Lưu dữ liệu nhập NVL
function saveMaterialImport(dataCanSave, status) {
    fetch(`${window.baseUrl}ImportMaterials/SaveMaterialImport`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json; charset="utf-8";'
        },
        body: JSON.stringify({
            dataSave: JSON.stringify(dataCanSave),
            status: status
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
            $(".overlay").addClass('d-none');
            $('.spinner-border').addClass('d-none');
            if (data.message !== "paused") {
                alert("Nhập NVL thành công");
                $('.btn-save-others').addClass('disabled');
                $('#enterRequestNo').modal('hide');
                $('#staticImportWire').modal('show');
            }
            
            $('#barcodeInput').val('');
            $('#barcodeInput').focus();
            $('.content-qr-render').html('');
        })
        .catch(error => {
            alert(error);
        })
}
// Lưu dữ liệu NVL khác
function saveOtherMaterialsImport(dataCanSave, status) {
    fetch(`${window.baseUrl}ImportMaterials/SaveOtherMaterialsImport`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json; charset="utf-8";'
        },
        body: JSON.stringify({
            dataSave: JSON.stringify(dataCanSave),
            status: status
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
            $(".overlay").addClass('d-none');
            $('.spinner-border').addClass('d-none');
            if (data.message !== "paused") {
                alert("Nhập NVL thành công");
                window.location.reload();
            }

            $('#barcodeInput').val('');
            $('#barcodeInput').focus();
            $('.content-qr-render').html('');
        })
        .catch(error => {
            alert(error);
        })
}
// Kiểm tra số lượng tồn kho trên MES
function checkQtyMes(dataImport) {
    fetch(`${window.baseUrl}ImportMaterials/CheckQtyOnMes`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json; charset="utf-8";'
        },
        body: JSON.stringify({
            strDataCheck: JSON.stringify(dataImport)
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

            if ($('.btn-check-qty')) {
                $('.btn-check-qty').addClass('disabled');
            }

            $('.content-qr-render').html(renderItemImport(dataImport));

            $('.qty-receving').html(`<p>Số lượng cần nhập: ${data.qty}</p>`);

            $('.btn-pause-import').removeClass('disabled');

            $(`.list-item-plan tbody tr[data-material_code='${dataImport.productCode}'][data-material_lot='${dataImport.lotNo}'][data-material_qty='${dataImport.qty}']`).css('backgroundColor', '#08ff00');
            let idRecev = parseInt($(`.list-item-plan tbody tr[data-material_code='${dataImport.productCode}'][data-material_lot='${dataImport.lotNo}'][data-material_qty='${dataImport.qty}']`).attr('data-idreceiving'), 10);
            dataImport.idRecev = idRecev;
            localStorage.setItem('product_prev_import', JSON.stringify(dataImport));

            $('.btn-save-import').removeClass('disabled');
        })
        .catch(error => {
            alert(error);
            if (localStorage.getItem('product_prev_import') != '') {
                $('.btn-save-import').removeClass('disabled');
            }
            $('.btn-save-import').addClass('disabled');
        })
}
// Hiển thị thông tin NVL đã đọc
function renderItemImport(dataRender) {
    let html = ``;
    html += `
            <div class="name-code content-item" data-slug="name_code" data-value="`+ dataRender.productCode + `">
                <span class="title">Mã nguyên vật liệu:</span>
                <span class="value">`+ dataRender.productCode + `</span>
            </div>
            <div class="qty content-item" data-slug="qty" data-value="`+ dataRender.qty + `">
                <span class="title">Số lượng:</span>
                <span class="value">`+ dataRender.qty + `</span>
            </div>
            <div class="lot-number content-item" data-slug="lot_num" data-value="`+ dataRender.lotNo + `">
                <span class="title">Số Lot:</span>
                <span class="value">`+ dataRender.lotNo + `</span>
            </div>
            <div class="pouch-number content-item" data-slug="pouch_num" data-value="`+ dataRender.pouchNo + `">
                <span class="title">Số pouch:</span>
                <span class="value">`+ dataRender.pouchNo + `</span>
            </div>
            <div class="time-limit content-item" data-slug="time_limit" data-value="`+ dataRender.timeLimit + `">
                <span class="title">Hạn tiệt trùng:</span>
                <span class="value">`+ dataRender.timeLimit + `</span>
            </div>`;
    return html;
}
// Đăng nhập
function LoginSystem() {

    localStorage.removeItem('dataTrayCurrent');

    let btnLogin = document.getElementById("btnLogin");
    let btnShowpassword = document.getElementById("btnShowPassword");
    let inputPassword = document.getElementById("Password");
    let inputUsername = document.getElementById("Username");
    let formLogin = document.getElementById("loginModal");
    // Hiển thị password
    inputPassword.addEventListener("input", (event) => {
        event.preventDefault();
        if (inputPassword.value != "") {
            btnShowpassword.classList.remove("d-none");
            event.target.classList.remove("border-danger");
            document.querySelector(".message-login-failed").innerHTML = "";
            inputPassword.parentElement.parentElement.querySelector(".message-error").innerHTML = "";
            btnLogin.classList.remove("disabled");
        } else {
            btnShowpassword.classList.add("d-none");
            btnLogin.classList.add("disabled");
        }
    });
    inputUsername.addEventListener("input", (event) => {
        event.preventDefault();
        event.target.classList.remove("border-danger");
        document.querySelector(".message-login-failed").innerHTML = "";
        btnLogin.classList.remove("disabled");
        event.target.parentElement.querySelector(".message-error").innerHTML = "";
    });

    // Button show password
    btnShowpassword.addEventListener("click", (event) => {
        event.preventDefault();
        if (inputPassword.type == 'password') {
            inputPassword.type = 'text';
            btnShowpassword.innerHTML = '<i class="bx bxs-hide"></i>';
        } else {
            inputPassword.type = 'password';
            btnShowpassword.innerHTML = '<i class="bx bxs-show"></i>';
        }
    });

    // Đăng nhập
    formLogin.addEventListener("keyup", (event) => {
        if (event.key === "Enter") {
            btnLogin.click();
        }
    });
    btnLogin.addEventListener("click", (event) => {
        event.preventDefault();
        btnLogin.classList.add('active');
        let boolUsername = false;
        let boolPassword = false;
        let username = inputUsername.value;
        let passwordUser = inputPassword.value;
        let validateUsername = ValidateUsername(username);
        let validatePassword = ValidatePassword(passwordUser);
        let parentElementUser = inputUsername.parentElement;
        let parentElementPassword = inputPassword.parentElement.parentElement;
        if (validateUsername != "") {
            inputUsername.classList.add("border-danger");
            parentElementUser.querySelector(".message-error").innerHTML = validateUsername;
        } else {
            boolUsername = true;
        }
        if (validatePassword != "") {
            inputPassword.classList.add("border-danger");
            parentElementPassword.querySelector(".message-error").innerHTML = validatePassword;
        } else {
            boolPassword = true;
        }
        document.querySelector(".spinner-grow").classList.remove("d-none");
        if (boolUsername && boolPassword) {
            passwordUser = window.btoa(passwordUser);
            let data = {
                username: username,
                password: passwordUser
            };
            // Send data to the server (example URL)
            fetch(`${window.baseUrl}account/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(data) // Send the data as JSON
            })
                .then(async response => {
                    if (!response.ok) {
                        const errorResponse = await response.json();
                        throw new Error(`${response.status} - ${errorResponse.message}`);
                    }
                    return response.json();
                })
                .then(data => {
                    // Handle success
                    setTimeout(() => {
                        if (data.status != '1') {
                            inputUsername.classList.add("border-danger");
                            inputPassword.classList.add("border-danger");
                            document.querySelector(".message-login-failed").innerHTML = data.message;
                            btnLogin.classList.add("disabled");
                        } else {
                            if (data.firstLogin == true || data.changePassword == true) {
                                alert('Bạn cần phải thay đổi mật khẩu khi đăng nhập lần đầu tiên');
                                $(formLogin).modal('hide');
                                $('.btn-update-password').attr('data-userId', data.userId);
                                $('#modalChangePassword').modal('show');
                            } else {
                                window.location.href = data.redirectUrl;
                            }  
                            inputUsername.classList.remove("border-danger");
                            inputPassword.classList.remove("border-danger");
                        }
                        btnLogin.classList.remove('active');
                        document.querySelector(".spinner-grow").classList.add("d-none");
                    }, 1000);
                })
                .catch(error => {
                    // Handle error
                    alert(error);
                });
        } else {
            btnLogin.classList.remove('active');
            document.querySelector(".spinner-grow").classList.add("d-none");
        }
    });

    $('.btn-update-password').on('click', function (e) {
        $('#modalChangePassword').modal('hide');
        $(formLogin).modal('show');
        inputPassword.value = "";
    });
}
// ----------------- Kiểm tra NVL đang tạm dừng -----------------
let intervalId;
function checkHoldingMaterials() {
    fetch(`${window.baseUrl}api/CheckHolding`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json; charset=utf-8;'
        },
        body: JSON.stringify()
    })
        .then(async response => {
            if (!response.ok) {
                const errorResponse = await response.json();
                throw new Error(`${response.status} - ${errorResponse.message}`);
            }
            return response.json();
        })
        .then(data => {
            if (data.itemImported.length > 0) {
                var arrImported = JSON.parse(data.itemImported);
                arrImported.forEach(item => {
                    $(`.list-item-plan tbody tr[data-material_code='${item.ItemCode}'][data-material_lot='${item.LotNo}'][data-idreceiving='${item.idRecev}']`).addClass('imported').removeClass('holding');
                })
            }
            if (data.hasDataReturn.length > 0) {
                var arrHolding = JSON.parse(data.hasDataReturn);
                if (arrHolding.length > 0) {
                    let html = '';
                    html += `
                    <div style="width: 50%; margin: 0 auto;">
                    <span>Đang tạm dừng: </span>
                        <ul class="list-group">
                    `;
                    for (let i = 0; i < arrHolding.length; i++) {
                        $(`.list-item-plan tbody tr[data-material_code='${arrHolding[i].productCode}'][data-material_lot='${arrHolding[i].lotNo}'][data-idreceiving='${arrHolding[i].idRecev}']`).addClass('holding').removeClass('imported');
                        html += `<li class="list-group-item" style="font-size: 12px; border: none;">Mã: ${arrHolding[i].productCode}, số lô: ${arrHolding[i].lotNo}, số lượng: ${arrHolding[i].qty}</li>`;
                    }
                    html += `</ul>
                    </div>`;
                    $('#staticImportWire .modal-body .hold-count').html(html);
                }
                localStorage.setItem('materials_holding', data.hasDataReturn);
                if (localStorage.getItem('materials_holding') != '') {
                    swal('Thông báo!', 'Có nguyên vật liệu tạm dừng chưa nhập về. Vui lòng kiểm tra và nhập lại!', 'warning')
                    $('#barcodeInput').focus();
                }
                clearInterval(intervalId);
            } else {
                $('#staticImportWire .modal-body .hold-count').html('');
                clearInterval(intervalId);
                $('#barcodeInput').focus();
            }
        })
        .catch(error => {
            alert(error);
            clearInterval(intervalId);
        })
}
function startInterval() {
    intervalId = setInterval(checkHoldingMaterials, 800);
}
// Hiển thị thông tin đã tạm dừng
function renderHolding(arr) {
    let html = `
    <ul class="list-item-holding">`;
    for (let i = 0; i < arr.length; i++) {
        html += `
        <li>
            <div class="d-flex item-content">
                <span class="label">Mã nguyên vật liệu:</span>
                <span class="value">${arr[i].productCode}</span>
            </div>
             <div class="d-flex item-content">
                <span class="label">Số lô:</span>
                <span class="value">${arr[i].lotNo}</span>
            </div>
            <div class="d-flex item-content">
                <span class="label">Số lượng:</span>
                <span class="value">${arr[i].qty}</span>
            </div>
        </li>`;
    }
    html += `</ul>`;
    return html;
}
function removeItem(materials, productCode, lotNo) {

    return materials.filter(item => {

        return !(item.productCode === productCode && item.lotNo === lotNo);

    });

}
// ----------------- Kết thúc -----------------
function ValidateUsername(value) {
    let message;
    let regex = /[!@#$%^&*()\-+={}[\]:;"'<>,.?\/|\\]/;
    if (value == "") {
        message = "Tên đăng nhập không được để trống";
    } else if (value.length < 4 || value.length > 8) {
        message = "Tên đăng nhập từ 4 - 8 ký tự";
    } else if (regex.test(value)) {
        message = "Tên đăng nhập có các ký tự đặc biệt";
    } else {
        message = "";
    }
    return message;
}
function ValidatePassword(value) {
    let message;
   
    if (value == "") {
        message = "Mật khẩu không được để trống";
    } else if (value.length < 4) {
        message = "Mật khẩu từ 4 ký tự trở lên";
    } else {
        message = "";
    }
    return message;
}
function ValidatePasswordWithSpecialCharacters(value) {
    let message;
    let regex = /[!@#$%^&*()\-+={}[\]:;"'<>,.?\/|\\]/;
    let regexUpper = /[A-Z]/;
    let regexLower = /[a-z]/;
    let regexDigital = /[0-9]/;
    if (value == "") {
        message = "Mật khẩu không được để trống";
    } else if (value.length < 8) {
        message = "Mật khẩu từ 8 ký tự trở lên";
    } else if (!regex.test(value)) {
        message = "Mật khẩu phải có ký tự đặc biệt";
    } else if (!(regexUpper.test(value) && regexLower.test(value))) {
        message = "Mật khẩu phải có cả chữ hoa, chữ thường";
    } else if (!regexDigital.test(value)) {
        message = "Mật khẩu phải có số";
    } else {
        message = "";
    }
    return message;
}