document.addEventListener('DOMContentLoaded', function (e) {
    if ($('#addForm').length > 0) {
        $('#templateName').val('');
        $('#positionFormEntryData').val('');
        $('#selectCheckSheet').val('');
        $('#formName').val('');
        $('#formCode').val('');
        $('#orderForm').val('');
        $('#formType').val('');
        $('#isRepeatable').prop('checked', false);
    }
    if ($('#formFieldMapping').length > 0) {
        $('#formFieldMapping')[0].reset();
    }
   
    $('#selectCheckSheet').on('change', function (e) {
        e.preventDefault();
      
        $('#templateName').val('');
        $('#templateName').attr('data-templateid', '');
        $('#positionFormEntryData').val('');

        if ($(this).val() != '') {
            fetch(`${window.baseUrl}createform/getpositionchecksheet`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    checksheetVerId: $(this).find('option:selected').attr('data-checksheetverid'),
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
                    if (data.postionNameWorking.includes(',')) {
                        $('#showPosition #positionFormEntryData').remove();
                        let arrPositionCode = data.postionNameWorking.split(',');
                        let htmlRender = '<select class="form-select w-100" id="positionFormEntryData">';
                        arrPositionCode.forEach(item => {
                            htmlRender += `<option value="${item}">${item}</option>`;
                        });
                        htmlRender += '</select>';
                        $('#showPosition').append(htmlRender);
                    } else {
                        $('#positionFormEntryData').val(data.postionNameWorking);
                    }
                   
                    $('#formName').focus();
                })
                .catch(error => {
                    alert(error);
                });
        }

    });

    $('#addCustomLayout').tooltip({
        title: 'Thêm section của form nhập',
        placement: 'bottom'
    })

    $('.btn-add-layout').on('click', function (e) {
        $('#addCustomLayout').tooltip('hide');
        $('.layout-default').toggleClass('d-none');
    });

    $('.layout-default .render-item').on('click', function (e) {
        let thisColNumber = $(this).data('colnumber');
        let sectionId = 'section' + generateRandomNumbers().join('');   
        let htmlRender = renderColWithNumber(thisColNumber, sectionId);
        $('#formContainer .box-layout').before(htmlRender);
        $('.layout-default').addClass('d-none');
        $('.btn-add-content').tooltip({
            title: 'Thêm content cho form nhập',
            placement: 'bottom'
        });
        $('.btn-edit-section').tooltip({
            title: 'Sửa section',
            placement: 'bottom'
        });
        $('.btn-delete-section').tooltip({
            title: 'Xóa section của form nhập',
            placement: 'bottom'
        });
    });

    $('#formContainer').on('click', '.edit-section .btn-edit-section', function (e) {
        e.preventDefault();
        $('#editSection').modal('show');
        let thisSection = $(this).parent().parent();
        thisSection.addClass('is-edit');
        $('#editSection').on('shown.bs.modal', function (e) {
            $('#sectionId').val('');
            $('#sectionClass').val('');
            $('#rowCellIndex').val('');
            $('#sectionId').focus();
        });
        $('#saveCustomSection').attr('data-sectionid', thisSection.attr('id'));
    });
    $('#saveCustomSection').on('click', function (e) {
        e.preventDefault();
        let thisSection = $(`#${$(this).attr('data-sectionid')}`);
        let newSectionId = $('#sectionId').val();
        let customClass = $('#sectionClass').val();
        let rowCellIndex = $('#rowCellIndex').val();
        thisSection.attr('id', newSectionId);
        thisSection.attr('sectionclass', customClass);
        thisSection.attr('rowcellindex', rowCellIndex);
        $('#editSection').modal('hide');
        thisSection.removeClass('is-edit');
    });
    $('#editSection .btn-close').on('click', function (e) {
        e.preventDefault();
        $('#editSection').modal('hide');
    })

    $('#formContainer').on('click', '.edit-section .btn-delete-section', function (e) {
        e.preventDefault();
        $(this).tooltip('hide');
        $(this).parent().parent().remove();
    });


    $('#formContainer').on('click', '.element-content .btn-add-content', function (e) {
        e.preventDefault();
        let elementId = $(this).parent().attr('id');
        $(this).tooltip('hide');
        let htmlAddContent = '';
        if ($(this).parent().find('.layout-default').length > 0) {
            $('.layout-default').remove();
        } else {
            htmlAddContent += `<div class="layout-default row content-layout">
                <div class="content-item col-6">
                    <button class="btn-add-item" data-elementid='${elementId}' data-content="text"><i class='bx bx-text'></i> <span>Text</span></button>
                </div>
                <div class="content-item col-6">
                     <button class="btn-add-item" data-elementid='${elementId}' data-content="input"><i class='bx bx-rename'></i> <span>Input</span></button>
                </div>
                <div class="content-item col-6">
                     <button class="btn-add-item" data-elementid='${elementId}' data-content="button"><i class='bx bx-code'></i> <span>Button</span></button>
                </div>
                <div class="content-item col-6">
                     <button class="btn-add-item" data-elementid='${elementId}' data-content="textarea"><i class='bx bx-rename'></i> <span>Textarea</span></button>
                </div>
                <div class="content-item col-6">
                      <button class="btn-add-item" data-elementid='${elementId}' data-content="dropdown"><i class='bx bx-list-ul'></i> <span>Dropdown</span></button>
                </div>
            </div>`;
        }
        $(this).after(htmlAddContent);
    });

    $('#formContainer').on('click', '.content-layout .btn-add-item', function (e) {
        e.preventDefault();
        let elementId = $(this).attr('data-elementid');
        let dataContent = $(this).attr('data-content');
        let htmlElemItem = renderElementContent(dataContent, '', '', '');
        $(`#${elementId} .btn-add-content`).before(htmlElemItem);

        $('.btn-edit-content').tooltip({
            title: 'Sửa content của form nhập',
            placement: 'bottom'
        });
        $('.btn-delete-content').tooltip({
            title: 'Xóa content của form nhập',
            placement: 'bottom'
        });

        $(this).parent().parent().remove();

        $(`#${elementId}`).sortable({
            items: ".element-item",
            update: function (event, ui) {
                var movedItemId = ui.item.attr("id");
                console.log("Moved item ID: " + movedItemId);
            }
        });
    });
    $('#formContainer').on('click', '.element-content .btn-edit-content', function (e) {
        e.preventDefault();
        $(this).parent().parent().addClass('is-edited');
        let typeElement = $(this).parent().parent().attr('data-elementtype');
        let elementItemId = $(this).parent().parent().find('input, textarea, button').attr('id');
        $('#saveCustomElement').attr('data-elementid', elementItemId);
        $(this).tooltip('hide');
        if (typeElement == 'button') {
            $('#editButtonElement').modal('show');
            $('#saveCustomButton').attr('data-elementid', elementItemId);
            let valEditElement = $(`#${elementItemId}`).parent().find('input[type="hidden"]').val() ?? '';
            let objectDataElement = JSON.parse(valEditElement);
            $('#buttonId').val(objectDataElement.elementId);
            $('#buttonClass').val(objectDataElement.customClass);
            $('#textButton').val(objectDataElement.label);
            $('#typeButton').val(objectDataElement.typeElement);
            $('#eventButton').val(JSON.stringify(objectDataElement.events, null, 0));
        }
        if (typeElement == 'input' || typeElement == 'textarea') {
            $('#editElement').modal('show');

            if (typeElement === 'textarea') {
                $('#typeInputRender').attr('disabled', true);
            } else {
                $('#typeInputRender').removeAttr('disabled');
            }

            if ($(`#${elementItemId}`).parent().hasClass('is-edited')) {
                let valEditElement = $(`#${elementItemId}`).parent().find('input[type="hidden"]').val() ?? '';
                if (valEditElement != '') {
                    let objectDataElement = JSON.parse(valEditElement);
                    if (objectDataElement.fieldName != '') {
                        $('#fieldNameMapping').addClass('disabled border border-secondary');
                    }
                    $('#fieldNameMapping').val(objectDataElement.fieldName)
                    $('#labelNameMapping').val(objectDataElement.label);
                    $('#customClass').val(objectDataElement.customClass);
                    $('#cellValueMapping').val(objectDataElement.startCell);
                    $('#typeInputRender').val(objectDataElement.typeInput);
                    $('#rowSpanMerged').val(objectDataElement.rowSpan);
                    $('#colSpanMerged').val(objectDataElement.colSpan);
                    $('#isMergedCell').prop('checked', objectDataElement.isMerged);
                    $('#dataSource').val(objectDataElement.dataSource);
                    if (objectDataElement.conditions.length > 0) {
                        $('.btn-add-condition').trigger('click');
                        $('#conditionJson').val(JSON.stringify(objectDataElement.conditions, null, 0));
                    }
                    if (objectDataElement.events.length > 0) {
                        $('.btn-add-event').trigger('click');
                        $('#eventJson').val(JSON.stringify(objectDataElement.events, null, 0));
                    }
                    if (objectDataElement.isMerged) {
                        $('#isMergedCell').trigger('change');
                    }
                    $('#checkCalcTotal').prop('checked', objectDataElement.isTotals);
                }
            }
        }
        else if (typeElement == 'text') {
            $('#editParagraph').modal('show'); 
        }
    });

    // Thêm điều kiện cho element nếu có
    $('#editElement').on('click', '.btn-add-condition', function (e) {
        e.preventDefault();
        $('#conditionContents').html(`<div class="condition-item">
            <textarea id="conditionJson" rows="4" class="w-100"></textarea>
            <button class="btn btn-danger btn-delete-item btn-sm"><i class="bx bx-trash"></i></button>
        </div>`);
        $('.btn-delete-item').on('click', function (e) {
            e.preventDefault();
            $('#conditionJson').val('');
        });
    });

    // Thêm event cho element
    $('#editElement').on('click', '.btn-add-event', function (e) {
        e.preventDefault();
        $('#eventContents').html(`<div class="event-item">
            <textarea id="eventJson" rows="4" class="w-100"></textarea>
            <button class="btn btn-danger btn-delete-item btn-sm mt-1"><i class="bx bx-trash"></i></button>
        </div>`);
        $('.btn-delete-item').on('click', function (e) {
            e.preventDefault();
            $('#eventJson').val('');
        });
    });

    $('#editElement').on('change', '#isMergedCell', function (e) {
        e.preventDefault();
        if ($(this).is(':checked')) {
            $('#editElement .show-merged-cell').removeClass('d-none'); 
        } else {
            $('#editElement .show-merged-cell').addClass('d-none');
        }
        
    });

    $('#formContainer').on('click', '.element-content .btn-delete-content', function (e) {
        e.preventDefault();
        $(this).tooltip('hide');
        $(this).parent().parent().remove();
    });

    $('#editElement').on('click', '.btn-close', function (e) {
        $('#editElement').modal('hide');
    });

    $('#editButtonElement').on('click', '.btn-close', function (e) {
        $('#editButtonElement').modal('hide');
    });

    $('#editElement').on('hidden.bs.modal', function (e) {
        e.preventDefault();
        let elementItemId = $('#saveCustomElement').attr('data-elementid');
        if ($(`#${elementItemId}valueEdited`).val()) {
            $('#editElement #formFieldMapping')[0].reset();
            $('#editElement .show-merged-cell').addClass('d-none');
            $('#eventContents').html('');
            $('#conditionContents').html('');
        } 
    });

    $('#editButtonElement').on('hidden.bs.modal', function (e) {
        e.preventDefault();
        let elementItemId = $('#saveCustomElement').attr('data-elementid');
        if ($(`#${elementItemId}valueEdited`).val()) {
            $('#editButtonElement #formButtonCustom')[0].reset();
        }
    });

    $('#fieldNameMapping').on('change', function (e) {
        e.preventDefault();
        $(this).removeClass('border-danger');
        $('#saveCustomElement').removeClass('disabled');
        $('.invalid-value').remove();
    });

    $('#cellValueMapping').on('change', function (e) {
        e.preventDefault();
        $(this).removeClass('border-danger');
        $('#saveCustomElement').removeClass('disabled');
        $('.invalid-value').remove();
    });

    // Xử lý element button
    $('#saveCustomButton').on('click', function (e) {
        let objFieldMapping = {};
        
        let buttonId = $(`#buttonId`).val();
        let buttonClass = $(`#buttonClass`).val();
        let textButton = $(`#textButton`).val();
        let typeButton = $(`#typeButton`).val();

        let elementId = $(this).attr('data-elementid');
        let typeElement = $(`#${elementId}`).parent().attr('data-elementtype');
        $(`#${elementId}valueEdited`).remove();

        let buttonEvent = JSON.parse($('#eventButton').val().replace(/'/g, '"'));

        objFieldMapping.elementId = buttonId;
        objFieldMapping.customClass = buttonClass;
        objFieldMapping.typeElement = typeElement;
        objFieldMapping.label = textButton;
        objFieldMapping.typeButton = typeButton;
        objFieldMapping.events = buttonEvent;
        objFieldMapping.tabIndex = $(`#${elementId}`).parent().index();

        $(`#${elementId}`).parent().addClass('is-edited');

        $(`#${elementId}`).text(textButton || 'Button');
        $(`#${elementId}`).attr('buttonclass', buttonClass);
        $(`#${elementId}`).after(`<input type="hidden" id="${elementId}valueEdited" value='${JSON.stringify(objFieldMapping)}' />`);

        $('#editButtonElement').modal('hide');

    });

    // Xử lý elements Input
    $('#saveCustomElement').on('click', function (e) {
        let objFieldMapping = {};
        let conditionJson = $('#conditionJson').val() != undefined ? JSON.parse($('#conditionJson').val().replace(/'/g, '"')) : [];
        let eventJson = $('#eventJson').val() != undefined ? JSON.parse($('#eventJson').val().replace(/'/g, '"')) : [];
        let elementId = $(this).attr('data-elementid');
        $(`#${elementId}valueEdited`).remove();

        let typeElement = $(`#${elementId}`).parent().attr('data-elementtype');
        let fieldNameMapping = $('#fieldNameMapping').val();
        let customClass = $('#customClass').val();
        let labelNameMapping = $('#labelNameMapping').val();
        let cellValueMapping = $('#cellValueMapping').val().toUpperCase() || "";
        let typeInputRender = $('#typeInputRender').find(':selected').val() || "";
        let rowSpanMerged = $('#rowSpanMerged').val() || 0;
        let colSpanMerged = $('#colSpanMerged').val() || 0;
        let dataSource = '';
        if ($('#dataSource').val() != '') {
            if ($('#dataSource').val().includes('.')) {
                dataSource = $('#dataSource').val();
            } else {
                dataSource = $('#dataSource').val() + '.' + fieldNameMapping;
            }
        } else {
            dataSource = '';
        }
        let isMergedCell = $('#isMergedCell').is(':checked');
        let checkCalcTotal = $('#checkCalcTotal').is(':checked');

        objFieldMapping.fieldName = fieldNameMapping;
        objFieldMapping.label = labelNameMapping;
        objFieldMapping.startCell = cellValueMapping;
        objFieldMapping.customClass = customClass;
        objFieldMapping.typeInput = typeInputRender;
        objFieldMapping.rowSpan = rowSpanMerged ? parseInt(rowSpanMerged, 10) : 0;
        objFieldMapping.colSpan = colSpanMerged ? parseInt(colSpanMerged, 10) : 0;
        objFieldMapping.isMerged = isMergedCell;
        objFieldMapping.typeElement = typeElement;
        objFieldMapping.elementId = fieldNameMapping;
        objFieldMapping.dataSource = dataSource;
        objFieldMapping.isTotals = checkCalcTotal;
        objFieldMapping.conditions = conditionJson;
        objFieldMapping.events = eventJson;
        objFieldMapping.tabIndex = $(`#${elementId}`).parent().index();

        let validateSaveData = true;

        if (fieldNameMapping == '') {
            $('#fieldNameMapping').parent().append('<div class="text-danger invalid-value">Cần phải nhập thông tin biến nội bộ cho Element này!</div>');
            $('#fieldNameMapping').addClass('border-danger');
            validateSaveData = false;
        } else if (cellValueMapping == '') {
            $('#cellValueMapping').append('<div class="text-danger invalid-value">Cần phải nhập ô thêm dữ liệu tương ứng trong file excel!</div>');
            $('#cellValueMapping').addClass('border-danger');
            validateSaveData = false;
        } else {
            validateSaveData = true;
        }
        if (validateSaveData) {
            $(`#${elementId}`).parent().addClass('is-edited');

            $(`#${elementId}`).parent().find('label').text(labelNameMapping || 'Label');
            $(`#${elementId}`).after(`<input type="hidden" id="${elementId}valueEdited" value='${JSON.stringify(objFieldMapping)}' />`);

            if ($(`#${elementId}`).parent().parent().parent().parent().parent().attr('rowcellindex') == undefined) {
                swal('Thông báo', 'Chưa có hàng thêm dữ liệu của section. Vui lòng thêm tại section để đủ dữ liệu!', 'warning');
            }
            $('#editElement').modal('hide');
        } else {
            $('#saveCustomElement').addClass('disabled');
        }
    });

    $("#formContainer").sortable({
        items: ".section",
        update: function (event, ui) {
            var movedItemId = ui.item.attr("id");
            console.log("Moved item ID: " + movedItemId);
        }
    });

    // Lưu field form
    let arrDataFieldMapping = [];
    $('.btn-save-template').on('click', function (e) {
        arrDataFieldMapping = [];
        e.preventDefault();
        let formName = $('#formName').val();
        // Loop section
        $('#formContainer .section').each(function (i, s) {
            let sectionId = $(this).attr('id');
            let sectionClass = $(this).attr('sectionclass');
            let sectionIndex = $(this).index();
            let rowCellIndex = $(this).attr('rowcellindex');
            let sectionInfo = {
                sectionId: sectionId,
                sectionClass: sectionClass,
                sectionIndex: sectionIndex,
                rowCellIndex: rowCellIndex,
                rows: [],
            };
            // Loop rows
            $(this).find('.row').each(function (i, r) {
                let rowClass = $(this).attr('class');
                let rowIndex = $(this).index();
                let rowInfo = {
                    rowClass,
                    rowIndex,
                    cols: [],
                };
                sectionInfo.rows.push(rowInfo);
                // Loop col
                $(this).find('.col-number').each(function (i, c) {
                    let colClass = $(this).attr('class');
                    let colIndex = $(this).index();
                    let colInfo = {
                        colClass,
                        colIndex,
                        elements: [],
                    };
                    rowInfo.cols.push(colInfo);
                    // Loop Element
                    $(this).find('.element-item').each(function (i, c) {
                        let configElements = $(this).find('input[type="hidden"]').val();
                        colInfo.elements.push(JSON.parse(configElements));
                    });
                });
            });
            arrDataFieldMapping.push(sectionInfo);
        });

        if (formName != '' && arrDataFieldMapping.length > 0) {
            let dataFormCreated = {
                checksheetVerId: $('#selectCheckSheet option:checked').val(),
                formPosition: $('#positionFormEntryData').val(),
                formName: formName,
                orderForm: $('#orderForm').val(),
                formMode: $('#formType').val(),
                formType: $('#isRepeatable').is(':checked'),
                formDataCreated: JSON.stringify(arrDataFieldMapping),
                formFields: arrDataFieldMapping,
                formId: $(this).attr('data-formid'),
            }
            
            fetch(`${window.baseUrl}createform/saveformtemplate`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json;'
                },
                body: JSON.stringify({
                    dataFormCreated: JSON.stringify(dataFormCreated)
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
                    alert(data.message);
                    window.location.reload();
                })
                .catch(error => {
                    alert(error);
                })
        } else {
            $('.title-layout .notification').html('<p class="text-danger">Có lỗi. Vui lòng thử lại!</p>');
        }
    });

    // render form
    if ($('#formDataReturn2').val() != undefined) {
        var formData = JSON.parse($('#formDataReturn2').val());
        $('#formType').val(formData.FormType).prop('selected', true);
        var formJsonData = JSON.parse(formData.JsonFormData);
        let htmlFormRender = '';
        let objFieldMapping = {};
        formJsonData.forEach(section => {
            htmlFormRender += `<div class="section" sectionclass="${section.sectionClass}" rowcellindex="${section.rowCellIndex}" tab-index="${section.sectionIndex}" id="${section.sectionId}">`;
            let rowsElement = section.rows;
            rowsElement.forEach(row => {
                htmlFormRender += `<div class="${row.rowClass}" tabindex="${row.rowIndex}">`;
                let colsElement = row.cols;
                colsElement.forEach(col => {
                    htmlFormRender += `<div class="${col.colClass}" tabindex="${col.colIndex}">`;
                    let elements = col.elements;

                    htmlFormRender += `<div id='element${generateRandomNumbers().join('')}' class='element-content'>`;
                    elements.forEach(elem => {
                        objFieldMapping.fieldName = elem.fieldName;
                        objFieldMapping.label = elem.label;
                        objFieldMapping.startCell = elem.startCell;
                        objFieldMapping.customClass = elem.customClass;
                        objFieldMapping.typeInput = elem.typeInput;
                        objFieldMapping.rowSpan = elem.rowSpan ? parseInt(elem.rowSpan, 10) : 0;
                        objFieldMapping.colSpan = elem.colSpan ? parseInt(elem.colSpan, 10) : 0;
                        objFieldMapping.isMerged = elem.isMerged;
                        objFieldMapping.typeElement = elem.typeElement;
                        objFieldMapping.elementId = elem.elementId;
                        objFieldMapping.dataSource = elem.dataSource;
                        objFieldMapping.isTotals = elem.isTotals;
                        objFieldMapping.conditions = elem.conditions;
                        objFieldMapping.events = elem.events;
                        objFieldMapping.tabIndex = elem.tabIndex;
                        htmlFormRender += renderElementContent(elem.typeElement, elem.label, elem.typeInput, JSON.stringify(objFieldMapping));
                    });
                    htmlFormRender += `<button class="btn-add-content"><i class='bx bx-plus'></i></button>
                        </div>
                    </div>`;
                });
                htmlFormRender += `</div>`;
            });
            htmlFormRender += `
            <div class="edit-section">
                    <button class="btn-edit-section"><i class="bx bx-edit"></i></button>
                    <button class="btn-delete-section"><i class="bx bx-trash"></i></button>
                </div></div>`;
        });
        $('#formContainer .box-layout').before(htmlFormRender);
    }

    //if ($('#formDataReturn').val() != undefined) {
    //    var formData = JSON.parse($('#formDataReturn').val());
    //    var htmlFormRender = '';
    //    let objFieldMapping = {};
    //    let groupedDataByElementId = formData.map(section => {
    //        let groupedFormMapping = section.formMapping.reduce((acc, element) => {
    //            let elementId = element.ElementId;
    //            if (!acc[elementId]) {
    //                acc[elementId] = [];
    //            }
    //            acc[elementId].push(element);
    //            return acc;
    //        }, {});

    //        return {
    //            sectionId: section.sectionId,
    //            colInRow: section.colInRow,
    //            // Chuyển đổi đối tượng đã nhóm thành một mảng để dễ dàng duyệt qua sau này
    //            groupedFormMapping: Object.keys(groupedFormMapping).map(key => ({
    //                ElementId: key,
    //                Elements: groupedFormMapping[key]
    //            }))
    //        };
    //    });

    //    groupedDataByElementId.forEach(section => {
    //        htmlFormRender += `<div class="section" id="${section.sectionId}">
    //        <div class="row">`;
    //        let dataElems = section.groupedFormMapping;
    //        dataElems.forEach(col => {
    //            let elementId = col.ElementId;
    //            htmlFormRender += ` <div class="col-${12 / section.colInRow}">
    //            <div id='${elementId}' class='element-content'>`;

    //            col.Elements.forEach(elem => {
    //                objFieldMapping.fieldName = elem.FieldName;
    //                objFieldMapping.label = elem.LabelText;
    //                objFieldMapping.startCell = elem.StartCell;
    //                objFieldMapping.rowIndex = elem.RowIndex ?? 0;
    //                objFieldMapping.colIndex = elem.ColIndex ?? 0;
    //                objFieldMapping.typeInput = elem.InputType;
    //                objFieldMapping.rowSpan = elem.RowSpan ?? 0;
    //                objFieldMapping.colSpan = elem.ColSpan ?? 0;
    //                objFieldMapping.isMerged = elem.IsMerged;
    //                objFieldMapping.isHidden = elem.IsHidden;
    //                objFieldMapping.sectionId = section.sectionId;
    //                objFieldMapping.colClass = elem.ColClass;
    //                objFieldMapping.typeElement = elem.ElementType;
    //                objFieldMapping.sectionIndex = elem.SectionIndex;
    //                objFieldMapping.elementId = elementId;
    //                objFieldMapping.dataSource = elem.DataSource;
    //                objFieldMapping.isTotals = elem.IsTotals;
    //                let htmlElem = renderElementContent(elem.ElementType, elem.LabelText, elem.InputType, JSON.stringify(objFieldMapping));
    //                htmlFormRender += `${htmlElem}
    //              `;
    //            });
    //            htmlFormRender += `
    //                <button class="btn-add-content"><i class='bx bx-plus'></i></button>
    //                </div>
    //            </div>`;
    //        });
    //        htmlFormRender += `
    //            </div>
    //            <div class="edit-section">
    //                <button class="btn-edit-section"><i class="bx bx-edit"></i></button>
    //                <button class="btn-delete-section"><i class="bx bx-trash"></i></button>
    //            </div>
    //        </div>`;
    //    });
    //    $('#formContainer .box-layout').before(htmlFormRender);
    //}
});

function renderColWithNumber(colNumber, sectionId) {
    let htmlLayout = '';

    if (colNumber == 1) {
        let elementId = 'element' + generateRandomNumbers().join('');
        htmlLayout = `<div class='section' id='${sectionId}'>
                <div class='row'>
                    <div class="col-12 col-xl-12 col-sm-12 col-md-12 col-lg-12 col-number">
                        <div id='${elementId}' class='element-content'>
                            <button class="btn-add-content"><i class='bx bx-plus'></i></button>
                        </div>
                    </div>
                </div>
                <div class="edit-section">
                    <button class="btn-edit-section"><i class="bx bx-edit"></i></button>
                    <button class="btn-delete-section"><i class="bx bx-trash"></i></button>
                </div>
            </div>`;
    }
    if (colNumber == 2) {
        let htmlCol = '';
        for (let i = 0; i < colNumber; i++) {
            let elementId = 'element' + generateRandomNumbers().join('');
            htmlCol += `<div class="col-${12 / colNumber} 
            col-xl-${12 / colNumber} 
            col-md-${12 / colNumber} 
            col-lg-${12 / colNumber} 
            col-sm-${12 / colNumber} 
            col-number">
                        <div id='${elementId}' class='element-content'>
                            <button class="btn-add-content"><i class='bx bx-plus'></i></button>
                        </div>
                    </div>`;
        }
        htmlLayout = `<div class='section' id='${sectionId}'>
                <div class='row'>
                    ${htmlCol}
                </div>
                <div class="edit-section">
                    <button class="btn-edit-section"><i class="bx bx-edit"></i></button>
                    <button class="btn-delete-section"><i class="bx bx-trash"></i></button>
                </div>
            </div>`;
    }
    if (colNumber == 3) {
        let htmlCol = '';
        for (let i = 0; i < colNumber; i++) {
            let elementId = 'element' + generateRandomNumbers().join('');
            htmlCol += `<div class="col-${12 / colNumber} 
            col-xl-${12 / colNumber} 
            col-md-${12 / colNumber} 
            col-lg-${12 / colNumber} 
            col-sm-${12 / colNumber} 
            col-number">
                        <div id='${elementId}' class='element-content'>
                            <button class="btn-add-content"><i class='bx bx-plus'></i></button>
                        </div>
                    </div>`;
        }
        htmlLayout = `<div class='section' id='${sectionId}'>
                <div class='row'>
                    ${htmlCol}
                </div>
                <div class="edit-section">
                    <button class="btn-edit-section"><i class="bx bx-edit"></i></button>
                    <button class="btn-delete-section"><i class="bx bx-trash"></i></button>
                </div>
            </div>`;
    }
    if (colNumber == 4) {
        let htmlCol = '';
        for (let i = 0; i < colNumber; i++) {
            let elementId = 'element' + generateRandomNumbers().join('');
            htmlCol += `<div class="col-${12 / colNumber} 
            col-xl-${12 / colNumber} 
            col-md-${12 / colNumber} 
            col-lg-${12 / colNumber} 
            col-sm-${12 / colNumber} 
            col-number">
                        <div id='${elementId}' class='element-content'>
                            <button class="btn-add-content"><i class='bx bx-plus'></i></button>
                        </div>
                    </div>`;
        }
        htmlLayout = `<div class='section' id='${sectionId}'>
                <div class='row'>
                    ${htmlCol}
                </div>
                <div class="edit-section">
                    <button class="btn-edit-section"><i class="bx bx-edit"></i></button>
                    <button class="btn-delete-section"><i class="bx bx-trash"></i></button>
                </div>
            </div>`;
    }
    if (colNumber == 6) {
        let htmlCol = '';
        for (let i = 0; i < colNumber; i++) {
            let elementId = 'element' + generateRandomNumbers().join('');
            htmlCol += `<div class="col-${12 / colNumber} 
            col-xl-${12 / colNumber} 
            col-md-${12 / colNumber} 
            col-lg-${12 / colNumber} 
            col-sm-${12 / colNumber} 
            col-number">
                        <div id='${elementId}' class='element-content'>
                            <button class="btn-add-content"><i class='bx bx-plus'></i></button>
                        </div>
                    </div>`;
        }
        htmlLayout = `<div class='section' id='${sectionId}'>
                <div class='row'>
                    ${htmlCol}
                </div>
                <div class="edit-section">
                    <button class="btn-edit-section"><i class="bx bx-edit"></i></button>
                    <button class="btn-delete-section"><i class="bx bx-trash"></i></button>
                </div>
            </div>`;
    }
    return htmlLayout;
}

function renderElementContent(dataElement, label, typeInput, objFieldMapping) {
    let htmlElem = '';
    if (dataElement == 'text') {
        let textId = 'text' + generateRandomNumbers().join('');
        htmlElem = `<div class='text-content input-custom mb-2 element-item' data-elementType='${dataElement}' id='${textId}'>
                <p>
                    Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.
                </p>
                 <div class="action-content">
                    <button class="btn-edit-content"><i class='bx bx-edit'></i></button>
                    <button class="btn-delete-content"><i class='bx bx-trash'></i></button>
                </div>
            </div>`;
    }
    if (dataElement == 'button') {
        let buttonId = 'button' + generateRandomNumbers().join('');
        htmlElem = `<div class='text-content input-custom mb-2 element-item' data-elementType='${dataElement}'>
                    <button type="button" class="btn btn-secondary" id='${buttonId}'>${label || "Label"}</button>
                    <input type="hidden" id="${buttonId}valueEdited" value='${objFieldMapping}' />
                 <div class="action-content">
                    <button class="btn-edit-content"><i class='bx bx-edit'></i></button>
                    <button class="btn-delete-content"><i class='bx bx-trash'></i></button>
                </div>
            </div>`;
    }
    if (dataElement == 'input') {
        let idInput = 'input' + generateRandomNumbers().join('');
        if (typeInput === 'checkbox') {
            htmlElem = `<div class='form-check element-item w-100' data-elementType='${dataElement}'>
                <label class="form-check-label" for='${idInput}'>${label || "Label"}</label>
                <input type="${typeInput}" name="${idInput}" id="${idInput}" class="input-render form-check-input" />
                <input type="hidden" id="${idInput}valueEdited" value='${objFieldMapping}' />
                <div class="action-content">
                    <button class="btn-edit-content"><i class='bx bx-edit'></i></button>
                    <button class="btn-delete-content"><i class='bx bx-trash'></i></button>
                </div>
            </div>`;
        } else {
            htmlElem = `<div class='input-custom element-item' data-elementType='${dataElement}'>
                <label for='${idInput}'>${label || "Label"}</label>
                <input type="${typeInput || "text"}" name="" id="${idInput}" class="input-render form-control" />
                <input type="hidden" id="${idInput}valueEdited" value='${objFieldMapping}' />
                <div class="action-content">
                    <button class="btn-edit-content"><i class='bx bx-edit'></i></button>
                    <button class="btn-delete-content"><i class='bx bx-trash'></i></button>
                </div>
            </div>`;
        }
      
    }
    if (dataElement == 'dropdown') {
        let iddropdown = 'dropdown' + generateRandomNumbers().join('');
        htmlElem = `<div class='input-custom element-item' data-elementType='${dataElement}'>
                <label for='${iddropdown}'>${label || "Label"}</label>
                <select class='form-select' id='${iddropdown}'></select>
                <div class="action-content">
                    <button class="btn-edit-content"><i class='bx bx-edit'></i></button>
                    <button class="btn-delete-content"><i class='bx bx-trash'></i></button>
                </div>
                <input type="hidden" id="${iddropdown}valueEdited" value='${objFieldMapping}' />
            </div>`;
    }
    if (dataElement == 'textarea') {
        let textareaId = 'textarea' + generateRandomNumbers().join('');
        htmlElem = `<div class='input-custom element-item' data-elementType='${dataElement}'>
                <label for='${textareaId}'>${label || "Label"}</label>
                <textarea id="${textareaId}" class="form-control" style="height: 100px"></textarea>
                <input type="hidden" id="${textareaId}valueEdited" value='${objFieldMapping}' />
                <div class="action-content">
                    <button class="btn-edit-content"><i class='bx bx-edit'></i></button>
                    <button class="btn-delete-content"><i class='bx bx-trash'></i></button>
                </div>
            </div>`;
    }
    return htmlElem;
}

function generateRandomNumbers() {
    let randomNumbers = [];
    while (randomNumbers.length < 6) {
        let randomNumber = Math.floor(Math.random() * 10);
        if (!randomNumbers.includes(randomNumber)) {
            randomNumbers.push(randomNumber);
        }
    }
    return randomNumbers;
}
