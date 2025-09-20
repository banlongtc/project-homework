window.baseUrl = window.location.hostname === 'localhost' ?
    'https://localhost:7275/' :
    'http://10.239.1.54/gwtest/';
window.einkUrl = 'https://10.239.4.40/api/esl';

let countQr = 0;
//$('.btn-read-condition').on('click', function (e) {
//    $('#editElement').modal('show');
//    $('#editElement').on('shown.bs.modal', function (e) {
//        $('.countQR').on('click', function (e) {
//            countQr++;
//            $.ajax({
//                url: `${window.baseUrl}home/gettestform`,
//                method: 'POST',
//                data: { checksheetVerId: 2 },
//                success: function (response) {
//                    let formConfigs = response.jsonFormFields;
//                    const form = $('#dynamicForm');
//                    form.empty();
//                    formConfigs.forEach(formConfig => {
//                        renderForm(form, formConfig, "", response.checksheetVersionId, "I-KTT-1");
//                        applyConditions(formConfig, "RG80GA35153Y", "", countQr);
//                    });
//                },
//                error: function (error) {
//                }
//            });
//            $('#btnSaveCondition').removeClass('disabled');
//            let arrDataPouchSavedExcel = JSON.parse(localStorage.getItem('INFO_OUTERDIAMETERS_SAVING')) || [];
//            if (arrDataPouchSavedExcel.length === 5) {
//                $('#btnSaveCondition').trigger('click');
//            }
//        });
//    });
//});

//$('.btn-supplementary-condition').on('click', function (e) {
//    $('#editElement').modal('show');
//    let modeForm = $(this).attr('data-modeevent');
//    $('#editElement').on('shown.bs.modal', function () {
//        const config = formConditionDemoJson[0];
//        renderForm(config, modeForm);
//        applyConditions(config, "RG80GA35153Y", "");
//        $("#btnSave").removeClass('disabled');
//    });
//});


//$('#btnSaveCondition').on('click', function () {
//    let arrDataPouchSavedExcel = JSON.parse(localStorage.getItem('INFO_OUTERDIAMETERS_SAVING')) || [];
//    let formDataMapping = [];
//    arrDataPouchSavedExcel.flat().forEach(newItem => {
//        let existingItem = formDataMapping.find(item => item.fieldName === newItem.fieldName);

//        if (existingItem) {
//            if (newItem.value !== "" && newItem.value !== null && newItem.value !== undefined) {
//                existingItem.value = newItem.value;
//            }
//        } else {
//            formDataMapping.push(newItem);
//        }
//    });
//    // Lưu dữ liệu lên database và chuyển thao tác khác
//    console.log(formDataMapping);
//});

function renderForm(form, formConfig, modeForm, positionWorking, dataBlinds) {
    let sections = JSON.parse(formConfig.sections, null, 4);
    sections.forEach(section => {
        const sectionDiv = $(`<div class="section" data-formid="${formConfig.formId}">`).addClass(section.sectionClass).attr('id', section.sectionId);
        section.rows.forEach(row => {
            const rowDiv = $('<div>').addClass(row.rowClass);
            row.cols.forEach(col => {
                const colDiv = $('<div>').addClass(col.colClass);
                col.elements.forEach(el => {
                    const wrapper = $('<div class="mb-3 render-item"></div>').attr('tabindex', el.tabIndex);
                    let label = $(`<label for="${el.elementId}" class="form-label">${el.label}</label>`);
                    let input = $(`<input type="${el.typeInput}" class="${el.customClass}" data-labeltext="${el.label}" data-fieldname="${el.fieldName}" id="${el.elementId}" value="${el.defaultValue ?? ""}" />`);
                    if (el.typeInput === 'checkbox') {
                        wrapper.addClass('form-check');
                        input.prop('checked', false);
                    }
                    if (el.typeElement === 'button') {
                        label = '';
                        input = $(`<button type="button" id="${el.elementId}" class="btn ${el.customClass}">${el.label}</button>`);
                    }

                    if (el.typeElement === 'textarea') {
                        input = $(`<textarea id="${el.elementId}" class="${el.customClass}"></textarea>`);
                    }

                    if (el.customClass.includes('d-none')) {
                        wrapper.addClass('d-none');
                    }

                    // Gắn event
                    (el.events || []).forEach(ev => {
                        const jsEvent = ev.type.toLowerCase().replace(/^on/, '');
                        if (ev.action === 'nextField') {
                            input.on(jsEvent, function (e) {
                                if (e.key === 'Enter' && !e.shiftKey) {
                                    let currentValue = $(this).val();
                                    let newValue = currentValue.replace(/Ư/g, 'W');
                                    $(this).val(newValue);
                                    $(this).removeClass('border-danger');
                                    $(this).parent().find('small').remove();

                                    $(`#${ev.targetElementId}`).focus();
                                }
                            });
                        } else if (ev.action === 'triggerSave' && modeForm == "") {
                            input.on(jsEvent, function (e) {
                                if (e.key === 'Enter') {
                                    let currentValue = $(this).val();
                                    let newValue = currentValue.replace(/Ư/g, 'W');
                                    $(this).val(newValue);
                                    $(this).removeClass('border-danger');
                                    $(this).parent().find('small').remove();
                                    setTimeout(() => {
                                        $(`#${ev.targetElementId}`).trigger('click');
                                    }, 800)
                                }
                            });
                        } else if (ev.action === 'showButton') {
                            input.on(jsEvent, function (e) {
                                // Dựa vào vị trí hiện tại gọi đến func xử lý logic riêng biệt
                                if (positionWorking.includes("KTT")) {
                                    // Gọi func xử lý đường kính
                                    handlerOuterDiameterKTT($(this), ev.targetElementId);
                                }
                            });
                        } else if (ev.action === 'scanQRContinue') {
                            input.on(jsEvent, function (e) {
                                // Dựa vào vị trí hiện tại gọi đến func xử lý logic riêng biệt
                                if (positionWorking.includes("KTT")) {
                                    // Gọi func xử lý đường kính
                                    handlerSavingOuterDiameterKTT(form, $(this), ev.targetElementId);
                                }
                            });
                        } else if (ev.action === 'showReadRuler') {
                            input.on(jsEvent, function (e) {
                                // Dựa vào vị trí hiện tại gọi đến func xử lý logic riêng biệt
                                if (positionWorking.includes("KTT")) {
                                    // Xử lý đọc thước vạch lỗi cong vênh
                                    handleReadRulerCode(form, $(this), ev.targetElementId);
                                }
                            });
                        } else if (ev.action === 'setValueUnit') {
                            input.on(jsEvent, function (e) {
                                // Dựa vào vị trí hiện tại gọi đến func xử lý logic riêng biệt
                                if (positionWorking.includes("KTT")) {
                                    // Xử lý nhập giá trị đo lỗi cong vênh
                                    handleUpdateValueErrorTwisted(form, $(this), ev.value);
                                }
                            });
                        } else if (ev.action === 'showChildError') {
                            input.on(jsEvent, async function (e) {
                                // Dựa vào vị trí hiện tại gọi đến func xử lý logic riêng biệt
                                if (positionWorking.includes("KTT")) {
                                    // Xử lý hiển thị lỗi con của lỗi Khác
                                    let errorStack = [];
                                    let arrErrors = [];
                                    let checkedErrors = new Set();
                                    await handleShowOtherErrorKTT(form, $(this), errorStack, arrErrors, checkedErrors);
                                }
                            });
                        }
                    });
                    wrapper.append(label).append(input);
                    colDiv.append(wrapper);
                });
                rowDiv.append(colDiv);
            });
            sectionDiv.append(rowDiv);
        });
        form.append(sectionDiv);
        if (dataBlinds != null) {
            form.find('.render-item input, .render-item select, .render-item textarea').each(function () {
                const fieldName = $(this).attr('data-fieldname');
                const value = dataBlinds[fieldName];

                if (typeof value !== 'undefined') { // Kiểm tra nếu giá trị tồn tại trong dataToFill
                    if ($(this).is(':checkbox')) {
                        $(this).prop('checked', value); // Xử lý checkbox
                    } else {
                        $(this).val(value); // Xử lý input, select, textarea
                    }
                }
            });
        }
       
    });
}

function applyConditions(formConfig, productCode, modeForm, countQr, errorData) {
    let sections = JSON.parse(formConfig.sections);
    sections.forEach(section => {
        section.rows.forEach(row => {
            row.cols.forEach(col => {
                col.elements.forEach(el => {
                    const current = $(`#${el.elementId}`);
                    (el.conditions || []).forEach(cond => {
                        let match = false;
                        if (cond.operator === 'contains') {
                            if (productCode != "") {
                                match = productCode.includes(cond.value);
                            } else {
                                match = false;
                            }
                            if (errorData != "") {
                                match = errorData.includes(cond.value);
                                $('#btnSaveCondition').attr('data-typeaction', 'Read Condition Add-On');
                            } else {
                                match = false;
                            }
                        } else if (cond.operator === 'notContains') {
                            if (productCode != "") {
                                match = !productCode.includes(cond.value);
                            }
                            if (errorData != "") {
                                match = !errorData.includes(cond.value);
                            }
                        } else if (cond.operator === "notEmpty") {
                            match = true;
                            current.closest('.render-item').append(`<small class="text-danger fst-italic">${cond.messageText}</small>`);

                        } else if (cond.operator === 'equals') {
                            if (countQr == cond.value) {
                                match = true;
                            }
                        }

                        if (cond.action === 'addRequiredField') {
                            current.addClass('border-danger');
                        }

                        if (modeForm === "supplementary-condition") {
                            match = true;
                            current.removeClass('border-danger');
                            current.closest('.render-item').find('small').remove();
                            $('#btnSaveCondition').removeClass('d-none');
                            current.blur();
                        }

                        if (errorData != '') {
                            current.removeClass('border-danger');
                            current.closest('.render-item').find('small').remove();
                        }

                        if (cond.action === 'show') {
                            current.closest('.render-item').toggle(match);
                        }

                        if (match && cond.focus && modeForm === '') {
                            current.focus();
                        }
                    });
                });
            });
        });
    });
}