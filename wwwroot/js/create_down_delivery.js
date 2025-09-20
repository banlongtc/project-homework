'use-strict';
document.addEventListener('DOMContentLoaded', function () {
    const layoutDelivery = document.querySelector('.delivery-container');
    
    if (layoutDelivery != null) {
        layoutDelivery.innerHTML = "";
        renderDeliveryItem();
        
        if ($.fn.DataTable.isDataTable(layoutDelivery)) {
            $(layoutDelivery).DataTable().destroy();
        }

        $(".btn-close-modal").on("click", function (e) {
            $("#ticketDetails").hide();
            $("#detailContent .hidden-input").html("");
            $(".overlay").remove();
        });
        // Tạo phiếu xuất
        const btnCreating = document.getElementById('buttonCreated');
        if (btnCreating != null) {
            btnCreating.addEventListener('click', debounce((event) => {
                var arrValue = [];
                let checkRg90 = false;
                if ($("#detailContent .tab-content .tab-pane").not(".d-none")) {
                    var thisElem = $("#detailContent .tab-content .tab-pane").not(".d-none");
                    thisElem.each(function (index, elemP) {
                        var titleTicket = $(elemP).data("title_ticket");
                        var objectData = {
                            title: titleTicket,
                            value: [],
                            productCode: []
                        };

                        $(elemP).find("tbody.ticket-content tr").each(function (index, elmItem) {
                            var objItem = {};
                            $(elmItem).has("td").each(function (index, itemDetail) {
                                $(itemDetail).find("input").each(function (i, itemInput) {
                                    if ($(itemInput).hasClass("item-code")) {
                                        objItem.itemCode = $(itemInput).val();
                                    }
                                    if ($(itemInput).hasClass("qty-ex")) {
                                        objItem.qty = $(itemInput).val();
                                    }
                                    if ($(itemInput).hasClass("unit-code")) {
                                        objItem.unitCode = $(itemInput).val();
                                    }
                                    if ($(itemInput).hasClass("position-wh")) {
                                        objItem.positionWH = $(itemInput).val();
                                    }
                                    if ($(itemInput).hasClass("lot_no")) {
                                        objItem.lotNO = $(itemInput).val();
                                    }
                                });
                                $(itemDetail).find("textarea").each(function (i, itemtextarea) {
                                    objItem.remarks = $(itemtextarea).val();
                                });
                            });
                            if (objItem.itemCode.includes('RG90')) {
                                checkRg90 = true;
                            }
                            objectData.value.push(objItem);
                        });

                        $("#detailContent .hidden-input").find("input.productCode").each(function (i, elemHidden) {
                            var workOrder = $(elemHidden).data("work_order");
                            var productCode = $(elemHidden).val();
                            var objProductCode = {
                                productCode: typeof productCode == "string" && productCode.indexOf(',') == true ? productCode.split(",") : productCode,
                                workOrder: typeof workOrder == "string" && workOrder.indexOf(',') == true ? workOrder.split(",") : workOrder,
                            };
                            objectData.productCode.push(objProductCode);
                        });
                        arrValue.push(objectData);
                    });
                }
                var dateImport = $("#detailContent .hidden-input .dateImport").val();
                var timeImport = $("#detailContent .hidden-input .timeImport").val();
                var dateArr = dateImport.split("/");
                var timeArr = timeImport.split(":");
                var dateNow = new Date();
                var timestamp = dateNow.getTime();
                var dateTimeEx = "";
                if (timeArr[0] < 12) {
                    dateTimeEx = dateArr[2] + "" + dateArr[1] + "" + dateArr[0] + "_" + timeArr[0] + "" + timeArr[1] + "AM_" + timestamp;
                } else {
                    dateTimeEx = dateArr[2] + "" + dateArr[1] + "" + dateArr[0] + "_" + timeArr[0] + "" + timeArr[1] + "PM_" + timestamp;
                }
                $("#ticketDetails .loading").removeClass("d-none");
                creatingDeliveryInExcel(arrValue, dateTimeEx, dateImport, timeImport, checkRg90);
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

function renderDeliveryItem() {
    fetch(`${window.baseUrl}Materials/RenderDeliveryItem`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
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
            if (data.rg90Items.length == 0) {
                $('.list-rg90-items h5').addClass('d-none');
            }
            setTimeout(() => {
                let flattenedItems = data.deliveryItems
                    .flatMap(d =>
                        d.dataItems.map(di => ({
                            dateTime: new Date(`${d.dateImport.split('/').reverse().join('-')}T${d.timeImport}:00`),
                            qtyUnused: di.qtyUnused,
                            inputGoodsCode: di.inputGoodsCode	
                        })));
                let flattenedRG90Items = data.rg90Items
                    .flatMap(d =>
                        d.dataItems.map(di => ({
                            dateTime: new Date(`${d.dateImport.split('/').reverse().join('-')}T${d.timeImport}:00`),
                            qtyUnused: di.qtyUnused,
                            inputGoodsCode: di.inputGoodsCode
                        })));

                let groupedItems = flattenedItems.reduce((acc, item) => {
                    let key = `${item.inputGoodsCode}|${item.dateTime.toISOString()}`;
                    if (!acc[key]) {
                        acc[key] = 0;
                    }
                    acc[key] += item.qtyUnused;
                    return acc;
                }, {});

                let groupedRG90Items = flattenedRG90Items.reduce((acc, item) => {
                    let key = `${item.inputGoodsCode}|${item.dateTime.toISOString()}`;
                    if (!acc[key]) {
                        acc[key] = 0;
                    }
                    acc[key] += item.qtyUnused;
                    return acc;
                }, {});

                let sortedGroupedItems = Object.entries(groupedItems)
                    .map(([key, totalQty]) => {
                        let [inputGoodsCode, dateTimeStr] = key.split('|');
                        return {
                            inputGoodsCode,
                            dateTime: new Date(dateTimeStr),
                            totalQty
                        };
                    }).sort((a, b) => a.dateTime - b.dateTime);

                let sortedGroupedRG90Items = Object.entries(groupedRG90Items)
                    .map(([key, totalQty]) => {
                        let [inputGoodsCode, dateTimeStr] = key.split('|');
                        return {
                            inputGoodsCode,
                            dateTime: new Date(dateTimeStr),
                            totalQty
                        };
                    }).sort((a, b) => a.dateTime - b.dateTime);

                let accumulatedResults = {};
                sortedGroupedItems.forEach(item => {
                    if (!accumulatedResults[item.inputGoodsCode]) {
                        accumulatedResults[item.inputGoodsCode] = 0;
                    }
                    accumulatedResults[item.inputGoodsCode] += item.totalQty;
                    item.cumulativeQty = accumulatedResults[item.inputGoodsCode];
                });

                let accumulatedRG90Results = {};
                sortedGroupedRG90Items.forEach(item => {
                    if (!accumulatedResults[item.inputGoodsCode]) {
                        accumulatedRG90Results[item.inputGoodsCode] = 0;
                    }
                    accumulatedRG90Results[item.inputGoodsCode] += item.totalQty;
                    item.cumulativeQty = accumulatedRG90Results[item.inputGoodsCode];
                });

                $('#listOtherMaterials').append(renderCardLayout(data.deliveryItems));
                $('.list-rg90-items').removeClass('d-none');
                $('#listRG90Materials').append(renderCardLayoutRG90(data.rg90Items));
                for (let i = 0; i < data.deliveryItems.length; i++) {
                    let item = data.deliveryItems[i].dataItems;
                    $('.delivery-container .card-item-' + i + ' .spinner-border').removeClass('d-none');
                    $('.delivery-container .card-item-' + i + '').append('<div class="overlay"></div>');
                    renderContentItems(item.sort((a, b) => a.processCode - b.processCode), i, sortedGroupedItems);
                }
                for (let i = 0; i < data.rg90Items.length; i++) {
                    let item = data.rg90Items[i].dataItems;
                    $('.delivery-container .card-item-' + i + ' .spinner-border').removeClass('d-none');
                    $('.delivery-container .card-item-' + i + '').append('<div class="overlay"></div>');
                    renderRG90ContentItems(item.sort((a, b) => a.processCode - b.processCode), i, sortedGroupedRG90Items);
                }
                // Hiển thị detail.
                $(".btn-show-details").on("click", function (e) {
                    e.preventDefault();
                    var arrTicketDetail = [];
                    var checkItems = false;
                    var classParent = $(this).data("class_parent");

                    $("." + classParent + " tbody tr.is-changed").each(function (i, elem) {
                        var itemChanged = {
                            value: []
                        };
                        var itemDetails = {};
                        $(elem).find("td.qty-ticket-1").each(function (index, item) {
                            itemDetails = {
                                wordOrder: "",
                                productCode: "",
                                processProd: "",
                                itemCode: "",
                                unitCode: "",
                                qtyTicket: "",
                                titleTicket: "",
                            };

                            $(elem).find("td").each(function (index, ele) {
                                if ($(ele).hasClass("process-prod")) {
                                    itemDetails.wordOrder = $(ele).data("input");
                                    itemDetails.productCode = $(ele).data("product_code");
                                    itemDetails.processProd = $(ele).data("process_prod");
                                    checkItems = true;
                                } else {
                                    checkItems = false;
                                }
                                if ($(ele).hasClass("item-code")) {
                                    itemDetails.itemCode = $(ele).data("item_code");
                                    itemDetails.unitCode = $(ele).data("unit_code");
                                    checkItems = true;
                                } else {
                                    checkItems = false;
                                }
                            });
                            if ($(item).data("qty_ticket") != undefined) {
                                itemDetails.qtyTicket = $(item).find("input").val();
                                checkItems = true;
                                itemDetails.titleTicket = $(item).data("title");
                            } else {
                                checkItems = false;
                            }
                            if (checkItems) {
                                itemChanged.value.push(itemDetails);
                            }

                        });
                        $(elem).find("td.qty-ticket-2").each(function (index, item) {
                            itemDetails = {
                                wordOrder: "",
                                productCode: "",
                                processProd: "",
                                itemCode: "",
                                unitCode: "",
                                qtyTicket: "",
                                titleTicket: "",
                            };
                            $(elem).find("td").each(function (index, ele) {
                                if ($(ele).hasClass("process-prod")) {
                                    itemDetails.wordOrder = $(ele).data("input");
                                    itemDetails.productCode = $(ele).data("product_code");
                                    itemDetails.processProd = $(ele).data("process_prod");
                                    checkItems = true;
                                } else {
                                    checkItems = false;
                                }
                                if ($(ele).hasClass("item-code")) {
                                    itemDetails.itemCode = $(ele).data("item_code");
                                    itemDetails.unitCode = $(ele).data("unit_code");
                                    checkItems = true;
                                } else {
                                    checkItems = false;
                                }
                            });
                            if ($(item).data("qty_ticket") != undefined) {
                                itemDetails.qtyTicket = $(item).find("input").val();
                                checkItems = true;
                                itemDetails.titleTicket = $(item).data("title");
                            } else {
                                checkItems = false;
                            }
                            if (checkItems) {
                                itemChanged.value.push(itemDetails);
                            }
                        });
                        $(elem).find("td.qty-ticket-3").each(function (index, item) {
                            itemDetails = {
                                wordOrder: "",
                                productCode: "",
                                processProd: "",
                                itemCode: "",
                                unitCode: "",
                                qtyTicket: "",
                                titleTicket: "",
                            };
                            $(elem).find("td").each(function (index, ele) {
                                if ($(ele).hasClass("process-prod")) {
                                    itemDetails.wordOrder = $(ele).data("input");
                                    itemDetails.productCode = $(ele).data("product_code");
                                    itemDetails.processProd = $(ele).data("process_prod");
                                    checkItems = true;
                                } else {
                                    checkItems = false;
                                }
                                if ($(ele).hasClass("item-code")) {
                                    itemDetails.itemCode = $(ele).data("item_code");
                                    itemDetails.unitCode = $(ele).data("unit_code");
                                    checkItems = true;
                                } else {
                                    checkItems = false;
                                }
                            });

                            if ($(item).data("qty_ticket") != undefined) {
                                itemDetails.qtyTicket = $(item).find("input").val();
                                checkItems = true;
                                itemDetails.titleTicket = $(item).data("title");
                            } else {
                                checkItems = false;
                            }

                        });
                        arrTicketDetail.push(itemChanged);
                        $("#ticketDetails").show();
                    });

                    if (arrTicketDetail.length > 0) {
                        $("#ticket-1 .ticket-content").html("");
                        $("#ticket-2 .ticket-content").html("");
                        $("#ticket-3 .ticket-content").html("");
                        var productCode = [];
                        let workOrder = [];
                        var idSection = $("." + classParent + "").parent().attr('id');
                        var dateImport = $(`#${idSection} .${classParent} .dateimport`).text();
                        var timeImport = $(`#${idSection} .${classParent} .timeimport`).text();
                        var textDateTime = timeImport + "h, ngày " + dateImport;
                        $(".time-value").html(textDateTime);
                        $("#ticketDetails").append("<div class='overlay'></div>");
                        for (let i = 0; i < arrTicketDetail.length; i++) {
                            for (let j = 0; j < arrTicketDetail[i].value.length; j++) {
                                let itemCode = "";
                                let qty = "";
                                let unit = arrTicketDetail[i].value[j].unitCode;
                                let position = "";
                                workOrder.push(arrTicketDetail[i].value[j].wordOrder);
                                productCode.push(arrTicketDetail[i].value[j].productCode);

                                if (arrTicketDetail[i].value[j].titleTicket == "Phiếu 1") {
                                    itemCode = arrTicketDetail[i].value[j].itemCode;
                                    qty = arrTicketDetail[i].value[j].qtyTicket;
                                    position = arrTicketDetail[i].value[j].processProd;
                                    $("#ticket-1").removeClass("d-none");
                                    $("#detailContent #ticket-tab-1").removeClass("d-none");
                                    $("#ticket-1 .ticket-content").append(`
                                            <tr>
                                                <td><input type="text" class="item-code" value="`+ itemCode + `" /></td>
                                                <td><input type="text" class="lot_no" /></td>
                                                <td><input type="text" class="unit-code" value="`+ unit + `" /></td>
                                                <td><input type="text" class="qty-ex" value="`+ qty + `" /></td>
                                                <td><input type="text" class="position-wh" value="`+ position + `" /></td>
                                                <td><textarea class="remarks" rows="1" title="Ghi chú"></textarea></td>
                                            </tr>`);
                                }
                                if (arrTicketDetail[i].value[j].titleTicket == "Phiếu 2") {
                                    itemCode = arrTicketDetail[i].value[j].itemCode;
                                    qty = arrTicketDetail[i].value[j].qtyTicket;
                                    position = arrTicketDetail[i].value[j].processProd;
                                    $("#ticket-2").removeClass("d-none");
                                    $("#detailContent #ticket-tab-2").removeClass("d-none");
                                    $("#ticket-2 .ticket-content").append(`
                                            <tr> 
                                                <td><input type="text" class="item-code" value="`+ itemCode + `" /></td>
                                                <td><input type="text" class="lot_no" /></td>
                                                <td><input type="text" class="unit-code" value="`+ unit + `" /></td>
                                                <td><input type="text" class="qty-ex" value="`+ qty + `" /></td>
                                                <td><input type="text" class="position-wh" value="`+ position + `" /></td>
                                                <td><textarea class="remarks" rows="1" title="Ghi chú"></textarea></td>
                                            </tr>`);
                                }
                                if (arrTicketDetail[i].value[j].titleTicket == "Phiếu 3") {
                                    itemCode = arrTicketDetail[i].value[j].itemCode;
                                    qty = arrTicketDetail[i].value[j].qtyTicket;
                                    position = arrTicketDetail[i].value[j].processProd;
                                    $("#ticket-3").removeClass("d-none");
                                    $("#detailContent #ticket-tab-3").removeClass("d-none");
                                    $("#ticket-3 .ticket-content").append(`
                                            <tr>
                                                <td><input type="text" class="item-code" value="`+ itemCode + `" /></td>
                                                <td><input type="text" class="lot_no" /></td>
                                                <td><input type="text" class="unit-code" value="`+ unit + `" /></td>
                                                <td><input type="text" class="qty-ex" value="`+ qty + `" /></td>
                                                <td><input type="text" class="position-wh" value="`+ position + `" /></td>
                                                <td><textarea class="remarks" rows="1" title="Ghi chú"></textarea></td>
                                            </tr>`);
                                }
                            }
                        }
                        let uniqueWorkOrder = workOrder.map(item => {
                            let strItem = String(item);
                            // Tách chuỗi thành mảng, loại bỏ trùng lặp bằng Set và sau đó nối lại thành chuỗi
                            if (strItem.includes(",")) {
                                return [...new Set(strItem.split(','))].join(',');
                            } else {
                                return strItem;
                            }
                        });
                        let uniqueProducts = productCode.map(item => {
                            // Tách chuỗi thành mảng, loại bỏ trùng lặp bằng Set và sau đó nối lại thành chuỗi
                            let strItem = String(item);
                            if (strItem.includes(",")) {
                                return [...new Set(strItem.split(','))].join(',');
                            } else {
                                return strItem;
                            }
                        });
                        let uniqueWorkOrder2 = [...new Set(uniqueWorkOrder)];
                        let uniqueProducts2 = [...new Set(uniqueProducts)];

                        $("#detailContent .hidden-input").append(`
                                <input type="hidden" class="productCode" data-work_order="`+ uniqueWorkOrder2.join(",") + `" value="` + uniqueProducts2.join(",") + `" />
                                <input type="hidden" class="dateImport" value="`+ dateImport + `" />
                                <input type="hidden" class="timeImport" value="`+ timeImport + `" />
                                `);
                    }
                });
            }, 1000);
        })
        .catch(error => {
            alert(error);
        })
}

function renderCardLayout(arrDelivery) {
    let dateCurrent = new Date();
    let newStrDate = dateCurrent.getDate().toString().padStart(2, '0') + "/" + (dateCurrent.getMonth() + 1).toString().padStart(2, '0') + "/" + dateCurrent.getFullYear().toString();
    let strHtml = ``;
    for (let i = 0; i < arrDelivery.length; i++) {
        let data = arrDelivery[i];
        strHtml +=
        `<div class="card-item-${i} border-dotted card-style">
            <div class="header-card">
                <div class="d-flex align-items-center mb-3">
                    <div class="datetime-content">
                        Ngày lập phiếu:
                        <span class="ms-2 fw-bold datetimecurrent">${newStrDate}</span>
                    </div>
                </div>
                <div class="d-flex align-items-center mb-3">
                    <div class="datetime-content">
                        Ngày dự định xuất nguyên vật liệu:
                        <span class="ms-2 fw-bold dateimport">${data.dateImport}</span>
                    </div>
                </div>   
                <div class="d-flex align-items-center mb-3">
                    <div class="datetime-content">
                        Giờ dự định xuất nguyên vật liệu:
                        <span class="ms-2 fw-bold timeimport">${data.timeImport}</span>
                    </div>
                </div>
            </div>
            <div class="content-table mt-3">
                <div class="message text-danger"></div>
                <table id="listOtherItemsTable_${i}" class="table text-center d-none">
                    <thead>
                        <tr>
                            <th scope="col" data-title="Kho nhập" class="align-middle"></th>
                            <th scope="col" data-title="Mã NVL" class="align-middle"></th>
                            <th scope="col" data-title="Số lượng sử dụng" class="align-middle"></th>
                            <th scope="col" data-title="Số lượng tồn thực tế" class="align-middle"></th>
                            <th scope="col" data-title="Số lượng dự định nhập" class="align-middle"></th>
                            <th scope="col" data-title="Số lượng tổng tồn" class="align-middle"></th>
                            <th scope="col" data-title="Số lượng cần nhập" class="align-middle"></th>
                            <th scope="col" data-title="Số lượng nhập thực tế" class="align-middle"></th>
                            <th scope="col" data-title="Phiếu 1" class="align-middle"></th>
                            <th scope="col" data-title="Phiếu 2" class="align-middle"></th>
                            <th scope="col" data-title="Phiếu 3" class="align-middle"></th>
                            <th scope="col" data-title="" class="align-middle"></th>
                        </tr>
                    </thead>
                    <tbody id="listitemtable_${i}">
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan="10"></td>
                            <td class="button-export">
                                <button class="btn btn-show-details disabled btn-success btn-sm" data-class_parent="card-item-${i}" type="button">Chi tiết</button>
                            </td>
                        </tr>
                    </tfoot>
                </table>
            </div>
            <div class="spinner-border text-primary d-none"></div>
        </div>`;
    }
    return strHtml;
}

function renderCardLayoutRG90(arrDelivery) {
    let dateCurrent = new Date();
    let newStrDate = dateCurrent.getDate().toString().padStart(2, '0') + "/" + (dateCurrent.getMonth() + 1).toString().padStart(2, '0') + "/" + dateCurrent.getFullYear().toString();
    let strHtml = ``;
    for (let i = 0; i < arrDelivery.length; i++) {
        let data = arrDelivery[i];
        strHtml +=
            `<div class="card-item-${i} border-dotted card-style">
            <div class="header-card">
                <div class="d-flex align-items-center mb-3">
                    <div class="datetime-content">
                        Ngày lập phiếu:
                        <span class="ms-2 fw-bold datetimecurrent">${newStrDate}</span>
                    </div>
                </div>
                <div class="d-flex align-items-center mb-3">
                    <div class="datetime-content">
                        Ngày dự định xuất nguyên vật liệu:
                        <span class="ms-2 fw-bold dateimport">${data.dateImport}</span>
                    </div>
                </div>   
                <div class="d-flex align-items-center mb-3">
                    <div class="datetime-content">
                        Giờ dự định xuất nguyên vật liệu:
                        <span class="ms-2 fw-bold timeimport">${data.timeImport}</span>
                    </div>
                </div>
            </div>
            <div class="content-table mt-3">
                <div class="message text-danger"></div>
                <table id="rg90Items_${i}" class="table-rg90item table text-center d-none">
                    <thead>
                        <tr>
                            <th scope="col" data-title="Kho nhập" class="align-middle"></th>
                            <th scope="col" data-title="Mã NVL" class="align-middle"></th>
                            <th scope="col" data-title="Số lượng sử dụng" class="align-middle"></th>
                            <th scope="col" data-title="Số lượng tồn thực tế" class="align-middle"></th>
                            <th scope="col" data-title="Số lượng dự định nhập" class="align-middle"></th>
                            <th scope="col" data-title="Số lượng tổng tồn" class="align-middle"></th>
                            <th scope="col" data-title="Số lượng cần nhập" class="align-middle"></th>
                            <th scope="col" data-title="Số lượng nhập thực tế" class="align-middle"></th>
                            <th scope="col" data-title="Phiếu 1" class="align-middle"></th>
                            <th scope="col" data-title="Phiếu 2" class="align-middle"></th>
                            <th scope="col" data-title="Phiếu 3" class="align-middle"></th>
                            <th scope="col" data-title="" class="align-middle"></th>
                        </tr>
                    </thead>
                    <tbody id="listRg90itemtable_${i}">
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan="10"></td>
                            <td class="button-export">
                                <button class="btn btn-show-details disabled btn-success btn-sm" data-class_parent="card-item-${i}" type="button">Chi tiết</button>
                            </td>
                        </tr>
                    </tfoot>
                </table>
            </div>
            <div class="spinner-border text-primary d-none"></div>
        </div>`;
    }
    return strHtml;
}

function renderContentItems(data, i, sortedGroupedItems) {
    setTimeout(() => {
        $('.delivery-container .card-item-' + i + ' .spinner-border').addClass('d-none');
        $('.delivery-container .card-item-' + i + ' .overlay').remove();
        $('.delivery-container .card-item-' + i + ' table').removeClass('d-none');
        $(".delivery-container #listitemtable_" + i + "").html(renderTbodyTable(data, i, sortedGroupedItems));
        new DataTable("#listOtherItemsTable_" + i + "", {
            language: {
                info: '',
                infoEmpty: '',
                infoFiltered: '',
                lengthMenu: 'Hiển thị _MENU_ trên một trang',
                zeroRecords: '',
            },
            searching: false,
            paging: false,
            ordering: false,
            columnDefs: [
                {
                    target: 0,
                    width: "120px"
                },
                {
                    target: 1,
                    width: "120px"
                },
                {
                    target: 2,
                    width: "120px"
                },
                {
                    target: 3,
                    width: "90px"
                },
                {
                    target: 4,
                    width: "100px"
                },
                {
                    target: 5,
                    width: "100px"
                },
                {
                    target: 6,
                    width: "120px"
                },
                {
                    target: 7,
                    width: "120px"
                },
                {
                    target: 8,
                    width: "120px"
                },
                {
                    target: 9,
                    width: "120px"
                },
                {
                    target: 10,
                    width: "120px"
                },
                {
                    target: 11,
                    className: "d-none"
                }
            ],
            drawCallback: (settings) => {
                $("#listOtherMaterials .qty-import").each(function (i, elem) {
                    $(elem).on("change", function (e) {
                        var valInput = $(e.target).val();
                        $(this).parent().attr("data-can_import", valInput);
                        var classParent = $(this).data("parent_elem");
                        $(this).parent().parent().find("td").each(function (index, item) {

                            if ($(item).hasClass("qty-can-input")) {
                                if (valInput >= $(item).data("input")) {
                                    $("#listOtherMaterials ." + classParent + " .message").html("").css("margin-bottom", "0");
                                    $(e.target).removeClass("border-danger");
                                }
                                if (valInput < $(item).data("input")) {
                                    $(e.target).addClass("border-danger");
                                    $("#listOtherMaterials ." + classParent + " .message").html("Số lượng nhập thực tế nhỏ hơn số lượng cần nhập. Vui lòng kiểm tra lại!").css("margin-bottom", "10px");
                                }
                            }
                            if ($(item).find("input.sub-material-date").val() == "" && $(item).find("input.sub-material-date").val() != undefined) {
                                $(".sub-materials-card .message-date").html("Ngày nhập không được để trống");
                                $(item).find("input.sub-material-date").addClass("border-danger");
                                $(item).addClass("disabled");
                            }

                            if ($(item).hasClass("disabled")) {
                                $(item).removeClass("disabled");
                            }
                        });
                    });
                });
                $("#listOtherMaterials .control-qty-ticket").each(function (i, elem) {
                    $(elem).on("change", function (e) {
                        var valueInput = $(e.target).val();
                        if (valueInput == "") {
                            $(e.target).parent().removeAttr("data-qty_ticket");
                        } else {
                            $(e.target).parent().attr("data-qty_ticket", valueInput);
                            $(e.target).parent().parent().find("td").each(function (index, item) {
                                $(item).find("button.btn-check-data").trigger("click");
                            });
                        }

                    });
                });
                $("#listOtherMaterials .btn-check-data").on("click", function (e) {
                    e.preventDefault();
                    var qtyCanImport = 0;
                    var totalEntered = 0;
                    var checkData = true;
                    var classParent = "";
                    $(this).parent().parent().find("td").each(function (index, item) {
                        if ($(item).attr("data-can_import") !== undefined && $(item).attr("data-can_import") !== "") {
                            qtyCanImport += parseInt($(item).attr("data-can_import"), 10);
                        }
                        if ($(item).attr("data-qty_ticket") !== undefined && $(item).attr("data-qty_ticket") !== "") {
                            totalEntered += parseInt($(item).attr("data-qty_ticket"), 10);
                            classParent = $(item).find("input").data("parent_elem");
                        }
                    });
                    if (totalEntered < qtyCanImport || totalEntered > qtyCanImport) {
                        checkData = false;
                    } else {
                        checkData = true;
                    }
                    if (checkData) {
                        $(this).parent().parent().addClass("is-changed");
                        $("#listOtherMaterials ." + classParent + " .message").html("");
                        $(this).parent().parent().css({ "background-color": "", "color": "" });
                        $("#listOtherMaterials ." + classParent + " .btn-show-details").removeClass("disabled");
                    } else {
                        $(this).parent().parent().css({ "background-color": "#ff0000", "color": "#fff" });
                        $("#listOtherMaterials ." + classParent + " .message").html("Số lượng tổng trên phiếu đang lớn hơn hoặc nhỏ hơn số lượng nhập! Vui lòng kiểm tra lại.").css("margin-bottom", "10px");
                        $("#listOtherMaterials ." + classParent + " .btn-show-details").addClass("disabled");
                    }
                });
            }
        });
    }, 1000);
}

function renderRG90ContentItems(data, i, sortedGroupedItems) {
    setTimeout(() => {
        $('.delivery-container .card-item-' + i + ' .spinner-border').addClass('d-none');
        $('.delivery-container .card-item-' + i + ' .overlay').remove();
        $('.delivery-container .card-item-' + i + ' table').removeClass('d-none');
        $(".delivery-container #listRg90itemtable_" + i + "").html(renderTbodyTable(data, i, sortedGroupedItems));
        new DataTable("#rg90Items_" + i + "", {
            language: {
                info: '',
                infoEmpty: '',
                infoFiltered: '',
                lengthMenu: 'Hiển thị _MENU_ trên một trang',
                zeroRecords: '',
            },
            searching: false,
            paging: false,
            ordering: false,
            columnDefs: [
                {
                    target: 0,
                    width: "120px"
                },
                {
                    target: 1,
                    width: "120px"
                },
                {
                    target: 2,
                    width: "120px"
                },
                {
                    target: 3,
                    width: "90px"
                },
                {
                    target: 4,
                    width: "100px"
                },
                {
                    target: 5,
                    width: "100px"
                },
                {
                    target: 6,
                    width: "120px"
                },
                {
                    target: 7,
                    width: "120px"
                },
                {
                    target: 8,
                    width: "120px"
                },
                {
                    target: 9,
                    width: "120px"
                },
                {
                    target: 10,
                    width: "120px"
                },
                {
                    target: 11,
                    className: "d-none"
                }
            ],
            drawCallback: (settings) => {
                $("#rg90Items_" + i + " .qty-import").each(function (i, elem) {
                    $(elem).on("change", function (e) {
                        var valInput = parseInt($(e.target).val(), 10);
                        $(this).parent().attr("data-can_import", valInput);
                        var classParent = $(this).data("parent_elem");
                        $(this).parent().parent().find("td").each(function (index, item) {
                            if ($(item).hasClass("qty-can-input")) {
                                if (valInput >= $(item).data("input")) {
                                    $("#listRG90Materials ." + classParent + " .message").html("").css("margin-bottom", "0");
                                    $(e.target).removeClass("border-danger");
                                }
                                if (valInput < $(item).data("input")) {
                                    $(e.target).addClass("border-danger");
                                    $("#listRG90Materials ." + classParent + " .message").html("Số lượng nhập thực tế nhỏ hơn số lượng cần nhập. Vui lòng kiểm tra lại!").css("margin-bottom", "10px");
                                }
                            }
                            if ($(item).find("input.sub-material-date").val() == "" && $(item).find("input.sub-material-date").val() != undefined) {
                                $(".sub-materials-card .message-date").html("Ngày nhập không được để trống");
                                $(item).find("input.sub-material-date").addClass("border-danger");
                                $(item).addClass("disabled");
                            }

                            if ($(item).hasClass("disabled")) {
                                $(item).removeClass("disabled");
                            }
                        });
                    });
                });
                $("#rg90Items_" + i + " .control-qty-ticket").each(function (i, elem) {
                    $(elem).on("change", function (e) {
                        var valueInput = $(e.target).val();
                        if (valueInput == "") {
                            $(e.target).parent().removeAttr("data-qty_ticket");
                        } else {
                            $(e.target).parent().attr("data-qty_ticket", valueInput);
                            $(e.target).parent().parent().find("td").each(function (index, item) {
                                $(item).find("button.btn-check-data").trigger("click");
                            });
                        }

                    });
                });
                $("#rg90Items_" + i + " .btn-check-data").on("click", function (e) {
                    e.preventDefault();
                    var qtyCanImport = 0;
                    var totalEntered = 0;
                    var checkData = true;
                    var classParent = "";
                    $(this).parent().parent().find("td").each(function (index, item) {
                        if ($(item).attr("data-can_import") !== undefined && $(item).attr("data-can_import") !== "") {
                            qtyCanImport += parseInt($(item).attr("data-can_import"), 10);
                        }
                        if ($(item).attr("data-qty_ticket") !== undefined && $(item).attr("data-qty_ticket") !== "") {
                            totalEntered += parseInt($(item).attr("data-qty_ticket"), 10);
                            classParent = $(item).find("input").data("parent_elem");
                        }
                    });
                    if (totalEntered < qtyCanImport || totalEntered > qtyCanImport) {
                        checkData = false;
                    } else {
                        checkData = true;
                    }
                    if (checkData) {
                        $(this).parent().parent().addClass("is-changed");
                        $("#listRG90Materials ." + classParent + " .message").html("");
                        $(this).parent().parent().css({ "background-color": "", "color": "" });
                        $("#listRG90Materials ." + classParent + " .btn-show-details").removeClass("disabled");
                    } else {
                        $(this).parent().parent().css({ "background-color": "#ff0000", "color": "#fff" });
                        $("#listRG90Materials ." + classParent + " .message").html("Số lượng tổng trên phiếu đang lớn hơn hoặc nhỏ hơn số lượng nhập! Vui lòng kiểm tra lại.").css("margin-bottom", "10px");
                        $("#listRG90Materials ." + classParent + " .btn-show-details").addClass("disabled");
                    }
                });
            }
        });
    }, 1000);
}

function renderTbodyTable(arr, index, sortArr) {
    let html = "";
    for (let i = 0; i < arr.length; i++) {
        let locationPacking = '';
        if (arr[i].processCode == '01070' || (arr[i].processCode == '01074' && !arr[i].inputGoodsCode.includes("-C"))) {
            locationPacking = '01075';
        } else {
            locationPacking = arr[i].processCode;
        }
        let qtyUnused = 0;
        sortArr.forEach((item) => {
            let dateTime = new Date(item.dateTime);
            var dd = String(dateTime.getDate()).padStart(2, '0');
            var mm = String(dateTime.getMonth() + 1).padStart(2, '0');
            var yyyy = dateTime.getFullYear();
            let dateImportStr = `${dd}/${mm}/${yyyy}`;
            var hours = String(dateTime.getHours()).padStart(2, '0');
            var minutes = String(dateTime.getMinutes()).padStart(2, '0');
            let timeImportStr = `${hours}:${minutes}`;
            if (arr[i].inputGoodsCode == item.inputGoodsCode
                && arr[i].dateImport == dateImportStr
                && arr[i].timeImport == timeImportStr) {
                qtyUnused = item.cumulativeQty;
            }
        });
        let qtyCanImport = 0;
        qtyCanImport = qtyUnused - arr[i].ivtQty;
        if (qtyCanImport < 0) {
            qtyCanImport = 0;
        }
        html += "<tr>" +
            "<td data-process_prod='" + locationPacking + "' data-product_code='" + arr[i].itemCode + "' data-input='" + arr[i].workOrder + "' class='process-prod'>" + locationPacking + "" + "</td>" +
            "<td data-item_code='" + arr[i].inputGoodsCode + "' data-unit_code='PC' class='item-code'><span class='item-goodcode'>" + arr[i].inputGoodsCode + "</span></td>" +
            "<td class='qty-can-used' data-input='" + qtyUnused + "'>" + qtyUnused + "</td>" +
            "<td>" + (arr[i].inventoryReal ?? 0) + "</td>" +
            "<td>" + arr[i].totalExported + "</td>" +
            "<td data-qty-inventory='" + arr[i].ivtQty + "'> " + arr[i].ivtQty + "</td>" +
            "<td class='qty-can-input' data-input='" + qtyCanImport + "'>" + qtyCanImport + "</td>" +
            "<td>" +
            "<input type='number' data-parent_elem='card-item-" + index + "' title='Số lượng nhập thực' min='0' class='w-sm-100 qty-import' />" +
            "</td>" +
            "<td class='qty-ticket-1 disabled' data-title='Phiếu 1'>" +
            "<input type='number' title='Số lượng phiếu 1' min='0' data-parent_elem='card-item-" + index + "' class='w-sm-100 control-qty-ticket' />" +
            "</td>" +
            "<td class='qty-ticket-2 disabled' data-title='Phiếu 2'>" +
            "<input type='number' title='Số lượng phiếu 2' min='0' data-parent_elem='card-item-" + index + "' class='w-sm-100 control-qty-ticket' />" +
            "</td>" +
            "<td class='qty-ticket-3 disabled' data-title='Phiếu 3'>" +
            "<input type='number' title='Số lượng phiếu 3' min='0' data-parent_elem='card-item-" + index + "' class='w-sm-100 control-qty-ticket' />" +
            "</td>" +
            "<td class='align-middle d-none'>" +
            "<button type='button' title='Kiểm tra tổng số lượng' class='btn btn-check-data'><i class='bx bx-git-compare'></i></button>" +
            "</td>" +
            "</tr>";
    }
    return html;
}

function creatingDeliveryInExcel(arrValue, dateCreated, dateImport, timeImport, checkRg90) {
    fetch(`${window.baseUrl}Materials/CreatingDeliveryExcel`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            dataImport: JSON.stringify(arrValue),
            newFileName: dateCreated,
            stringDate: dateImport,
            stringTime: timeImport,
            checkItemRG90: checkRg90
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
                $('#ticketDetails .loading').addClass("d-none");
                $('#ticketDetails').hide();
                $('#showNotification').show();
                $("#showNotification").append("<div class='overlay'></div>");
                $('#showNotification #notificationContent').html(data.message);
                $('#buttonExported').on('click', debounce((event) => {
                    event.preventDefault();
                    var result = window.atob(data.fileDownload);
                    var excelName = data.excelName;
                    var buffer = new ArrayBuffer(result.length);
                    var bytes = new Uint8Array(buffer);
                    for (let i = 0; i < result.length; i++) {
                        bytes[i] = result.charCodeAt(i);
                    }
                    var blodArr = new Blob([bytes], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                    saveAs(blodArr, excelName + '.xlsx');
                    alert("Tải về thành công!");
                    window.location.reload();
                }, 300));
            }, 800);
        })
        .catch(error => {
            alert(error);
        })
}
function mergeObjectByKey(arr, key) {
    const map = new Map();
    arr.forEach(obj => {
        const keyValue = obj[key];
        if (map.has(keyValue)) {
            const existingObj = map.get(keyValue);

            Object.keys(obj).forEach(k => {
                if (k != key) {
                    if (!existingObj[k]) {
                        existingObj[k] = [obj[k]];
                    } else if (!Array.isArray(existingObj[k])) {
                        existingObj[k] = [existingObj[k], obj[k]];
                    } else if (!existingObj[k].includes(obj[k])) {
                        existingObj[k].push(obj[k]);
                    }
                }
            });
        } else {
            map.set(keyValue, { ...obj });
        }
    });
    return Array.from(map.values());
}