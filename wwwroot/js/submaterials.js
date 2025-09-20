'use strict';
let setLoadSubMaterials;
document.addEventListener('DOMContentLoaded', function () {
    const submaterialsLayout = document.querySelector('.addition-material');
    if (submaterialsLayout) {
        $('#QtyProd').on("input", function (e) {
            let qtyProdPerDay = $(this).val();
            let valDiv5 = qtyProdPerDay / 5;
            let valDiv25 = qtyProdPerDay / 25;
            $(".div-qty-5").attr("value", valDiv5);
            $(".div-qty-25").attr("value", valDiv25);
            $(".same-qty").attr("value", qtyProdPerDay);
        });

        connectionHub.on("ReceiveSubMaterials", function (message) {
            var dataReceived = JSON.parse(message);
            let qtyProdReturn = '';
            dataReceived.forEach(item => {
                qtyProdReturn = item.QtyProdPerDay;
                let newInventoryPre = parseInt(item.InventoryPre, 10);
                let newInventoryAfter = parseInt(item.Inventory, 10);
                let qtyCanInput = parseInt(item.SafeInventory, 10) - newInventoryAfter;
                if (qtyCanInput < 0) {
                    qtyCanInput = 0;
                }
                $(`.addition-material .table-responsive tbody tr[data-product_code="${item.ProductCode}"]`).find('input.ivt-qty').val(newInventoryPre);
                $(`.addition-material .table-responsive tbody tr[data-product_code="${item.ProductCode}"]`).find('input.ivt-after-minus').val(newInventoryAfter);
                $(`.addition-material .table-responsive tbody tr[data-product_code="${item.ProductCode}"]`).find('input.qty-import').val(qtyCanInput);
            });
            //$('#QtyProd').attr("value", qtyProdReturn);
            //let valDiv5 = qtyProdReturn / 5;
            //let valDiv25 = qtyProdReturn / 25;
            //$(".div-qty-5").attr("value", valDiv5);
            //$(".div-qty-25").attr("value", valDiv25);
            //$(".same-qty").attr("value", qtyProdReturn);
            //$(".addition-material .table-responsive tbody tr td").each(function (i, elem) {
            //    if ($(elem).find("input").val() != "") {
            //        $('.btn-export-material').removeClass('disabled');
            //    }
            //});
        });
        

        $(".btn-enter-qty").on("click", function (e) {
            e.preventDefault();
            let html = $(".addition-material .table-responsive").html();
            $(".form-enter-qty").append("<div class='overlay'></div>")
            $(".form-enter-qty").show();
            $(".form-enter-qty #detailContent").html(`<div class="table-responsive">${html}</div>`);
            $(".btn-calculator").removeClass("disabled");
            $(".form-enter-qty #detailContent tbody tr").each(function (i, elem) {
                let valIvt = $(elem).find("div.input-group").find("input.ivt-after-minus").val();
                $(elem).find("div.input-group").removeClass("disabled");
                $(elem).find("div.input-group").find("input.save-day").val("");
                $(elem).find("div.input-group").find("input.qty-rolls").val("");
                $(elem).find("div.input-group").find("input.qty-import").val("");
                $(elem).find("div.input-group").find("input.ivt-qty").attr('value', valIvt);
                $(elem).find("div.input-group").find("input.ivt-after-minus").val("");
                let valQtyPrintRoll = $(elem).find("input.qty-print-day").val();
                let valSafeInventory = $(elem).find("input.save-inventory").val();
                let productCode = $(elem).data("product_code");
                $(".form-enter-qty #detailContent tbody tr[data-product_code='" + productCode + "']").attr("data-qty_print", valQtyPrintRoll);
                $(".form-enter-qty #detailContent tbody tr[data-product_code='" + productCode + "']").attr("data-save_inventory", valSafeInventory);
                $(".form-enter-qty #detailContent tbody tr[data-product_code='" + productCode + "']").attr("data-inventory", valIvt);
            });
            $(".form-enter-qty input.qty-print-day").on("change", function (e) {
                let productCode = $(this).parent().parent().parent().data("product_code");
                $(".form-enter-qty #detailContent tbody tr[data-product_code='" + productCode + "']").addClass("has-change");
                $(".form-enter-qty #detailContent tbody tr[data-product_code='" + productCode + "']").attr("data-qty_print", $(this).val());
            });
            $(".form-enter-qty input.save-inventory").on("change", function (e) {
                let productCode = $(this).parent().parent().parent().data("product_code");
                $(".form-enter-qty #detailContent tbody tr[data-product_code='" + productCode + "']").addClass("has-change");
                $(".form-enter-qty #detailContent tbody tr[data-product_code='" + productCode + "']").attr("data-save_inventory", $(this).val());
            });
        });
        $(".btn-close-modal").on("click", function (e) {
            e.preventDefault();
            $(".form-enter-qty").hide();
            window.location.reload();
        });
        $(".btn-calculator").on("click", function (e) {
            e.preventDefault();
            let arrayCheck = [];
            $(".form-enter-qty #detailContent tbody tr").each(function (i, elem) {
                let qtyPrintDay = parseInt($(elem).data("qty_print"), 10);
                let qtyProdDay = $(elem).find("input.qty-prod-day").val();
                let qtyRoll = Math.ceil(qtyProdDay / qtyPrintDay);
                arrayCheck.push({
                    qtyRolls: qtyRoll,
                    productCode: $(elem).data("product_code")
                });
                $(elem).find("input.qty-rolls").val(qtyRoll);
                $(elem).addClass('has-change');
            });

            var totalQtyByProductCode = {};
            arrayCheck.forEach(function (product) {
                if (totalQtyByProductCode[product.productCode]) {
                    totalQtyByProductCode[product.productCode] += product.qtyRolls;
                } else {
                    totalQtyByProductCode[product.productCode] = product.qtyRolls;
                }
            });
            var result = Object.keys(totalQtyByProductCode).map(function (key) {
                return {
                    productCode: key,
                    totalQtyRolls: totalQtyByProductCode[key]
                };
            });
            for (let i = 0; i < result.length; i++) {
                $(".form-enter-qty #detailContent tbody tr").has("input.ivt-after-minus").each(function (i, elem) {
                    let qtyRoll = result[i].totalQtyRolls;
                    let ivtBefore = parseInt($(elem).data("inventory"), 10);
                    let saveInventory = $(elem).data("save_inventory");
                    let ivtAfterMinus = ivtBefore - qtyRoll;
                    let qtyCanInput = saveInventory - ivtAfterMinus;
                    if (qtyCanInput < 0) {
                        qtyCanInput = 0;
                    }
                    $(elem).find("input.ivt-after-minus").val(ivtAfterMinus);
                    $(elem).find("input.qty-import").val(qtyCanInput);
                });
            }
            $(this).addClass("disabled btn-secondary");
            $(this).removeClass("btn-success");

            $(".btn-save-info").removeClass("disabled");
        });

        $(".btn-save-info").on("click", function (e) {
            e.preventDefault();
            let arrData = [];
            let removeDuplicates;
            $(e.target).parent().append(`<span class="spinner-grow text-primary" role="status"></span>`);
            $(".form-enter-qty #detailContent tbody tr.has-change").each(function (i, elem) {
                let productCode = $(elem).data("product_code");
                let productName = $(elem).find("div.title-material").text();
                let inventory = $(elem).find("input.ivt-after-minus") != undefined ? $(elem).find("input.ivt-after-minus").val() : 0;
                let safeInventory = $(elem).find("input.save-inventory") != undefined ? $(elem).find("input.save-inventory").val() : 0;
                let qtyProd = qtyProdPerDay;
                let ivtBefore = $(elem).find("input.ivt-qty").val();
                let qtyPrinted = $(elem).find("input.qty-print-day") != undefined ? $(elem).find("input.qty-print-day").val() : 0;
                let qtyCanInput = $(elem).find("input.qty-import") != undefined ? $(elem).find("input.qty-import").val() : 0;
                let objValue = {
                    productCode: productCode,
                    productName: productName,
                    inventory: inventory,
                    safeInventory: safeInventory,
                    qtyProd: qtyProd,
                    qtyPrinted: qtyPrinted,
                    qtyCanInput: qtyCanInput,
                    inventoryPre: ivtBefore
                };
                arrData.push(objValue);
                removeDuplicates = arrData.reduce((acc, objValue) => {
                    let isDuplicate = acc.some(i => i.productCode == objValue.productCode);
                    if (!isDuplicate) {
                        acc.push(objValue);
                    }
                    return acc;
                }, []);
            });
            if (removeDuplicates.length > 0) {
                fetch(`${window.baseUrl}Materials/SaveSubMaterials`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json; charset=utf-8'
                    },
                    body: JSON.stringify({
                        dataSave: JSON.stringify(removeDuplicates),
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
                            $(".spinner-grow").remove();
                            alert(data.message);
                            window.location.reload();
                        }, 500);

                    })
                    .catch(error => {
                        alert(error);
                    })
            }
        });
    }
    const tableSubMaterialDelivery = document.querySelector('.sub-materials-card table');
    if (tableSubMaterialDelivery != null) {
        $(".sub-materials-card tbody tr").each(function (i, elem) {
            if ($(elem).find('input').val() != "") {
                $(".btn-save-details").addClass('d-none');
                $(".btn-show-details").removeClass('d-none');
            }
        });

        if ($.fn.DataTable.isDataTable(tableSubMaterialDelivery)) {
            $(layoutDelivery).DataTable().destroy();
        }

        var today = new Date();
        var dd = String(today.getDate()).padStart(2, '0');
        var mm = String(today.getMonth() + 1).padStart(2, '0');
        var yyyy = today.getFullYear();
        today = dd + '/' + mm + '/' + yyyy;
        let isChanging = false;
        new DataTable(tableSubMaterialDelivery, {
            language: {
                info: '',
                infoEmpty: '',
                infoFiltered: '',
                lengthMenu: 'Hiển thị _MENU_ trên một trang',
                zeroRecords: '',
            },
            searching: false,
            ordering: false,
            columnDefs: [
                {
                    target: 3,
                    width: "120px"
                },
                {
                    target: 5,
                    width: "100px"
                },
                {
                    target: 7,
                    className: "d-none"
                }
            ],
            drawCallback: function (settings) {
                $(".sub-material-date").datepicker({
                    dateFormat: "dd/mm/yy",
                    showOn: "both",
                    buttonImage: "../../images/calendar.png",
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
            },
        });

        $(".qty-import").on("change", function (e) {
            var valInput = $(e.target).val();
            $(this).parent().attr("data-can_import", valInput);
            var classParent = $(this).data("parent_elem");
            $(this).parent().parent().addClass("is-changed");
            let qtyCanInput = $(this).parent().parent().find("td.qty-can-input").data("input");
            if (valInput >= qtyCanInput) {
                $("." + classParent + " .message").html("").css("margin-bottom", "0");
                $(e.target).removeClass("border-danger");
            }
            if (valInput < qtyCanInput) {
                $(e.target).addClass("border-danger");
                $("." + classParent + " .message").html("Số lượng nhập thực tế nhỏ hơn số lượng cần nhập. Vui lòng kiểm tra lại!").css("margin-bottom", "10px");
            }
            let dateImport = $(this).parent().parent().find("input.sub-material-date").val();
            let timeImport = $(this).parent().parent().find("input.time-import").val();

            if (dateImport == "" && timeImport == "") {
                $(".sub-materials-card .message-date").html("Ngày nhập không được để trống");
                $(".sub-materials-card .message-time").html("Giờ nhập không được để trống");
                $(this).parent().parent().find("input.sub-material-date").addClass("border-danger");
                $(this).parent().parent().find("input.time-import").addClass("border-danger");
            } else if (dateImport == "" && timeImport !== "") {
                $(".sub-materials-card .message-date").html("Ngày nhập không được để trống");
                $(".sub-materials-card .message-time").html("");
            } else if (dateImport !== "" && timeImport == "") {
                $(".sub-materials-card .message-time").html("Giờ nhập không được để trống");
                $(".sub-materials-card .message-date").html("");
            }

            $(this).parent().parent().find("td.qty-ticket-1").removeClass("disabled");
        });
        $(".sub-material-date").on("change", function () {
            if ($(this).hasClass("border-danger")) {
                $(this).removeClass("border-danger");
                $(".sub-materials-card .message-date").html("");
            }
        });

        $(".time-import").on("change", function () {
            if ($(this).hasClass("border-danger")) {
                $(this).removeClass("border-danger");
                $(".sub-materials-card .message-time").html("");
            }
        });

        $(".control-qty-ticket").on("change", function (e) {
            var valueInput = $(this).val();
            $(this).prop(valueInput);
            if (valueInput == "") {
                $(this).parent().removeAttr("data-qty_ticket");
            } else {
                $(this).parent().attr("data-qty_ticket", valueInput);
                $(this).parent().parent().find("button.btn-check-data").trigger("click");
            }
        });

        $(".btn-check-data").on("click", function (e) {
            e.preventDefault();
            var qtyCanImport = 0;
            var totalEntered = 0;
            var checkData = true;
            var classParent = "";
            $(this).parent().parent().find("td")
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
                $("." + classParent + " .message").html("");
                $(this).parent().parent().css({ "background-color": "", "color": "" });
                $("." + classParent + " .btn-save-details").removeClass("disabled");
            } else {
                $(this).parent().parent().css({ "background-color": "#ff0000", "color": "#fff" });
                $("." + classParent + " .message").html("Số lượng tổng trên phiếu đang lớn hơn hoặc nhỏ hơn số lượng nhập! Vui lòng kiểm tra lại.").css("margin-bottom", "10px");
                $("." + classParent + " .btn-save-details").addClass("disabled");
            }
        });

        $(".btn-save-details").on("click", function (e) {
            e.preventDefault();
            var classParent = $(this).data("class_parent");
            let dataSubMaterial = [];
            var checkItems = false;
            $(".sub-materials-card").append("<div class='spinner-border spinner-border-sm'></div>");
            $(".sub-materials-card").append("<div class='overlay'></div>");
            $(".sub-materials-card tbody tr.is-changed").each(function (i, elem) {
                var itemChanged = []

                let submaterialDate = $(elem).find(".sub-material-date").val();
                let timeImportSub = $(elem).find(".time-import").val();
                $(elem).find("td.qty-ticket-1").each(function (index, item) {
                    var itemDetails = {
                        wordOrder: "",
                        productCode: "",
                        processProd: "",
                        itemCode: "",
                        unitCode: "",
                        qtyTicket: "",
                        titleTicket: "",
                        dateImportSub: submaterialDate,
                        timeImportSub: timeImportSub,
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
                        itemChanged.push(itemDetails);
                    }
                });
                dataSubMaterial.push(...itemChanged);
            });
            if (dataSubMaterial.length == 4) {
                saveDataSubmaterial(dataSubMaterial);
            } else {
                $("." + classParent + " .message").html("Chưa đủ dữ liệu để xử lý. Vui lòng kiểm tra lại!");
            }
        });

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
                let submaterialDate = $(elem).find(".sub-material-date").val();
                let timeImportSub = $(elem).find(".time-import").val();
                $(elem).find("td.qty-ticket-1").each(function (index, item) {
                    itemDetails = {
                        wordOrder: "",
                        productCode: "",
                        processProd: "",
                        itemCode: "",
                        unitCode: "",
                        qtyTicket: "",
                        titleTicket: "",
                        dateImportSub: submaterialDate,
                        timeImportSub: timeImportSub,
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
                arrTicketDetail.push(itemChanged);
                $("#ticketDetails").show();
            });

            if (arrTicketDetail.length > 0) {
                $("#ticket-1 .ticket-content").html("");
                var productCode = [];
                let workOrder = [];
                var dateImport = $("." + classParent + " .sub-material-date").val();
                var timeImport = $("." + classParent + " .time-import").val();
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


        $(".btn-close-modal").on("click", function (e) {
            $("#ticketDetails").hide();
            $("#detailContent .hidden-input").html("");
            $(".overlay").remove();
        });

        const btnCreating = document.getElementById('buttonCreated');
        if (btnCreating != null) {
            btnCreating.addEventListener('click', debounce((event) => {
                var arrValue = [];
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
                var dateTimeEx = "";
                var dateNow = new Date();
                var timestamp = dateNow.getTime();
                if (timeArr[0] < 12) {
                    dateTimeEx = dateArr[2] + "" + dateArr[1] + "" + dateArr[0] + "_" + timeArr[0] + "" + timeArr[1] + "AM_" + timestamp;
                } else {
                    dateTimeEx = dateArr[2] + "" + dateArr[1] + "" + dateArr[0] + "_" + timeArr[0] + "" + timeArr[1] + "PM_" + timestamp;
                }
                $("#ticketDetails .loading").removeClass("d-none");
                creatingDeliveryInExcel(arrValue, dateTimeEx, dateImport, timeImport);
            }, 300));
        }

        function saveDataSubmaterial(dataSubMaterial) {
            fetch(`${window.baseUrl}Materials/SavedSubMaterialExported`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json; charset=utf-8'
                },
                body: JSON.stringify({
                    strDataSub: JSON.stringify(dataSubMaterial),
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
                        $(".sub-materials-card .spinner-border").remove();
                        $(".sub-materials-card .overlay").remove();
                        alert(data.message);
                        window.location.reload();
                        $(".btn-save-details").addClass("d-none");
                        $(".btn-show-details").removeClass("d-none");
                    }, 500);

                })
                .catch(error => {
                    alert(error);
                })
        }
    }
});
function checkAndLoadContent() {
    fetch(`${window.baseUrl}api/GetSubmaterials`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json; charset=utf-8'
        },
        body: JSON.stringify({})
    })
        .then(async response => {
            if (!response.ok) {
                const errorResponse = await response.json();
                throw new Error(`${response.status} - ${errorResponse.message}`);
            }
            return response.json();
        })
        .then(data => {
            let dataSub = data.subMaterials;
            dataSub.map(item => {
                $('.addition-material tbody tr[data-product_code="' + item.productCode + '"] .ivt-qty').val(item.inventoryPre);
                $('.addition-material tbody tr[data-product_code="' + item.productCode + '"] .ivt-after-minus').val(item.inventory);
                $('.addition-material tbody tr[data-product_code="' + item.productCode + '"] .qty-import').val(item.qtyCanInput);
            });
            clearInterval(setLoadSubMaterials);
        })
        .catch(error => {
            alert(error);
            clearInterval(setLoadSubMaterials);
        })
}

function startLoadContent() {
    setLoadSubMaterials = setInterval(checkAndLoadContent, 5000);
}