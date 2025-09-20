document.addEventListener('DOMContentLoaded', function () {
    // Hiển thị modal đọc thẻ eink
    if ($('#tblStockInfo').length > 0) {
        $('#tblStockInfo').DataTable({
            language: {
                info: 'Trang _PAGE_ của _PAGES_ trang',
                infoEmpty: 'Không có bản ghi nào',
                infoFiltered: '(Lọc từ _MAX_ bản ghi)',
                lengthMenu: 'Hiển thị _MENU_ trên một trang',
                zeroRecords: 'Xin lỗi không có kết quả',
                emptyTable: "Không có dữ liệu",
                search: "Tìm kiếm: "
            },
            order: [],
            drawCallback: function (settings) {
                $('.btn-show-eink').on('click', function (e) {
                    let $parentElem = $(this).parent().parent();

                    let arrItem = [];
                    let productItem = $parentElem.find('td.item-material-code').data('productcode');
                    let lotNo = $parentElem.find('td.item-lot').data('lotno');
                    let hansd = $parentElem.find('td.item-date').data('date');
                    let tongton = $parentElem.find('td.item-total-ivt').data('tongton');
                    let tondd = $parentElem.find('td.item-ivtdd').data('tondd');
                    let toncsd = $parentElem.find('td.item-ivtcsd').data('toncsd');
                    let macd = $parentElem.find('td.item-location-code').data('macd');
                    let ideink = $parentElem.find('td.item-location-name').data('ideink');

                    arrItem.push({
                        productItem,
                        lotNo,
                        hansd,
                        tongton,
                        tondd,
                        toncsd,
                        macd,
                        ideink
                    });
                    //Thẻ E-ink đã liên kết
                    $.ajax({
                        url: `${window.baseUrl}Stock/Eink_code`,
                        type: 'GET',
                        data: { id: ideink },
                        success: function (data) {
                            if (data) {
                                $('#txtbarEink').val(data);
                                $('#txtbarEink').prop('disabled', true);
                            } else {
                                $('#txtbarEink').prop('disabled', false);
                                $('#txtbarEink').val('');
                            }
                        }
                    });
                    $('#myModal').modal('show');

                    $('#myModal').on('shown.bs.modal', function () {
                        let html = '';
                        arrItem.forEach(item => {
                            html += `<tr>`;
                            html += `<td>${item.productItem}</td>`;
                            html += `<td>${item.lotNo}</td>`;
                            html += `<td>${item.hansd}</td>`;
                            html += `<td>${item.tongton}</td>`;
                            html += `<td>${item.tondd}</td>`;
                            html += `<td>${item.toncsd}</td>`;
                            html += `<td>${item.macd}</td>`;
                            html += `<td style="display:none">${item.ideink}</td>`;
                            html += '</tr>';
                            $('#myModal #BtnSave').attr('data-productid', item.ideink);
                        });
                        $('#showContent').html('');
                        $('#showContent').append(html);
                        $('#txtbarEink').focus();
                    })

                });
            },
        });
        $('#tblStockInfo').parent().addClass('table-responsive');
    }

    if ($('#tblShowEinks').length > 0) {
        $('#tblShowEinks').DataTable({
            language: {
                info: 'Trang _PAGE_ của _PAGES_ trang',
                infoEmpty: 'Không có bản ghi nào',
                infoFiltered: '(Lọc từ _MAX_ bản ghi)',
                lengthMenu: 'Hiển thị _MENU_ trên một trang',
                zeroRecords: 'Xin lỗi không có kết quả',
                emptyTable: "Không có dữ liệu",
                search: "Tìm kiếm:"
            },
            responsive: true
        });
        $('#tblShowEinks').parent().addClass('table-responsive');
    }

    /*$(document).ready(function () {*/
    $("#tbarcode").on('input', function (event) {
        var input = $(this).val();
        var regex = /^[Mm]\s+\S+\s+\S+\s+/;
        if (!regex.test(input)) {
            var _input = input.replace(/\s\s+/g, ' ');
            var str_arr = _input.split(' ');
            $("#titem").val(str_arr[1]);
            $("#tlot").val(str_arr[2]);
        }
    });

    $('#txtbarEink').on('keypress', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            $('#BtnSave').trigger('click');
        }
    });

    $('#BtnSave').on('click', function (e) {
        e.preventDefault();
        $('.check-load').removeClass('d-none');

        var mac = $('#txtbarEink').val();
        let productIdEink = $(this).attr('data-productid');

        let endpoint = '';
        endpoint = `${window.einkUrl}/${mac}/link/${productIdEink}`;

        if (endpoint != '') {
            $.ajax({
                url: `${window.baseUrl}stock/eink`,
                type: 'POST',
                data: { endpoint: endpoint },
                success: function (response) {
                    $('.check-load').addClass('d-none');
                    swal('Thông báo', response.message, 'success').then((isConfirmed) => {
                        if (isConfirmed) {
                            window.location.reload();
                        }
                    });
                },
                error: function (error) {
                    swal('Thông báo', error.message, 'error', [false, "Ok"]).then((isConfirmed) => {
                        if (isConfirmed) {
                            $('.check-load').addClass('d-none');
                            $('#txtbarEink').val('');
                            $('#txtbarEink').focus();
                        }
                    });
                }

            });
        } else {
            alert('No data');
        }
    });

    // Control E-ink
    $("#tblShowEinks").on('click', '.btn-unlink-esl', function (e) {
        e.preventDefault();
        var mac = $(this).closest('tr').find('.mac').text();
        let productid = $(this).attr('data-productid');
        let endpoint = '';
        endpoint = `${window.einkUrl}/${mac}/unlink`;
        $.ajax({
            url: `${window.baseUrl}eink/remove`,
            type: 'POST',
            data: { endpoint: endpoint, productId: productid },
            success: function (response) {
                swal('Thông báo', response.message, 'success').then((isConfirmed) => {
                    if (isConfirmed) {
                        window.location.reload();
                    }
                });
            },
            error: function (error) {
                swal("Lỗi!", error.responseText, "error");
            }
        });
    });

    //************************ Control master lỗi ***********************************************************************
    //Show Popup
    $("#btnthem").click(function () {
        $('#Lbider').text("new").css('visibility', 'hidden');
        $('#lbtieude').text("Thêm Master lỗi");
        $('#txtnamev').val("");
        $('#txtnamej').val("");
        $('#txtloc').val("");
        $('#txtremark').val("");
        $("#Popup_themerr").modal('show');
    });
    $("body").on('click', '#btnedit', function (e) {
        $("#Popup_themerr").modal('show');
        let parentTr = $(this).parent().parent().parent();
        let id = parentTr.find('td').data('id');
        let idCha = parentTr.find('td.test').data('idcha');
        let loc = parentTr.find('td.loc').data('loc');
        let namev = parentTr.find('td.namev').data('namev');
        let namej = parentTr.find('td.namej').data('namej');
        let remark = parentTr.find('td.remark').data('remark');

        $('#Lbider').text(id).css('visibility', 'hidden');
        $('#lbtieude').text("Sửa Master lỗi");
        $('#idchaerr').val(idCha);
        $('#txtnamev').val(namev);
        $('#txtnamej').val(namej);
        $('#txtloc').val(loc);
        $('#txtremark').val(remark);

    });
    $('#BtnSave_err').click(function () {
        $('.check-load').removeClass('d-none');

        var idcha = $('#idchaerr').val();
        var tnamev = $('#txtnamev').val();
        var namej = $('#txtnamej').val();
        var location = $('#txtloc').val();
        var remark = $('#txtremark').val();

        var id = $('#Lbider').text();

        $.ajax({
            url: `${window.baseUrl}Master/SaveError`,
            type: 'POST',
            data: { tidcha: idcha, tnamev: tnamev, tnamej: namej, tlocation: location, tremark: remark, tid: id },
            success: function (response) {
                $('.check-load').addClass('d-none');
                var alertHTML = '<div id="successAlert" class="alert alert-success alert-dismissible fade show" role="alert">'
                    + response.message +
                    '</div>';
                $('#noticeContainer').html(alertHTML);
                setTimeout(function () {
                    $('#successAlert').fadeOut('slow');
                    $('#Popup_themerr').modal('hide');
                    window.location.reload();
                }, 3000);
            },
            error: function (error) {
                setTimeout(function () {
                    alert('Có lỗi xảy ra: ' + error.responseText);
                    $('.check-load').addClass('d-none');
                }, 500);
            }
        });
    });
    $("body").on('click', '#btndelete', function (e) {
        let parentTr = $(this).parent().parent().parent();
        let id = parentTr.find('td').data('id');
        $.ajax({
            url: `${window.baseUrl}Master/DelError`,
            type: 'POST',
            data: { tid: id },

            success: function (response) {
                swal("Thành công!", "Hệ thống xóa thành công!", "success")
                    .then((value) => {
                        window.location.reload();
                    });
            },
            error: function (error) {
                swal("Lỗi!", error.responseText, "error");
            }
        });
    });

    //************************ Control master item number ***********************************************************************
    $("#Btnthemitem").click(function () {
        $('#Lbiditem').text("new").css('visibility', 'hidden');
        $('#lbtditem').text("Thêm Master Item Number");
        $('#txtitemcode').val("");
        $('#txtnameitem').val("");
        $('#typeitem').val("");
        $('#unititem').val("");
        $('#remarkitem').val("");
        $("#Popup_Themitem").modal('show');
    });
    $("body").on('click', '#Edititem', function (e) {
        e.preventDefault();
        $("#Popup_Themitem").modal('show');

        let parentTr = $(this).parent().parent().parent();
        let iditem = parentTr.find('td.iditem').data('id');
        let itemcode = parentTr.find('td.itemcode').data('itemcode');
        let itemname = parentTr.find('td.itemname').data('itemname');
        let itemtype = parentTr.find('td.itemtype').data('itemtype');
        let unit = parentTr.find('td.unit').data('unit');
        let remark = parentTr.find('td.remark').data('remark');

        $('#lbtditem').text("Sửa Master Item Number");
        $('#Lbiditem').text(iditem).css('visibility', 'hidden');
        $('#txtitemcode').val(itemcode);
        $('#txtnameitem').val(itemname);
        $('#typeitem').val(itemtype);
        $('#unititem').val(unit);
        $('#remarkitem').val(remark);
    });
    $('#BtnSave_item').click(function () {
        $('.check-load').removeClass('d-none');

        var iditem = $('#Lbiditem').text();
        var itemcode = $('#txtitemcode').val();
        var itemname = $('#txtnameitem').val();
        var itemtype = $('#typeitem').val();
        var unit = $('#unititem').val();
        var remark = $('#remarkitem').text();

        $.ajax({
            url: `${window.baseUrl}Master/SaveItem`,
            type: 'POST',
            data: { tid: iditem, titemcode: itemcode, titemname: itemname, titemtype: itemtype, tunit: unit, tremark: remark },
            success: function (response) {
                $('.check-load').addClass('d-none');
                var alertHTML = '<div id="successAlert" class="alert alert-success alert-dismissible fade show" role="alert">'
                    + response.message +
                    '</div>';
                $('#noticeContainer').html(alertHTML);
                setTimeout(function () {
                    $('#successAlert').fadeOut('slow');
                    $('#Popup_Themitem').modal('hide');
                    window.location.reload();
                }, 2000);
            },
            error: function (error) {
                setTimeout(function () {
                    alert('Có lỗi xảy ra: ' + error.responseText);
                    $('.check-load').addClass('d-none');
                }, 500);
            }
        });
    });
    $("body").on('click', '#Deleteitem', function (e) {
        let parentTr = $(this).parent().parent().parent();
        let id = parentTr.find('td.iditem').data('id');
        $.ajax({
            url: `${window.baseUrl}Master/DelItem`,
            type: 'POST',
            data: { tid: id },

            success: function (response) {
                swal("Thành công!", "Hệ thống xóa thành công!", "success")
                    .then((value) => {
                        window.location.reload();
                    });
            },
            error: function (error) {
                swal("Lỗi!", error.responseText, "error");
            }
        });
    });

    //************************ Control master Tiêu chuẩn ***********************************************************************
    $("#Btnthemtc").click(function () {
        $('#Lbidtc').text("new").css('visibility', 'hidden');
        $('#lbtdtc').text("Thêm Master Tiêu chuẩn");
        $('#txttccode').val("");
        $('#txttcname').val("");
        $('#tcmaybit').prop('checked', false);
        $('#tcremark').val("");
        $("#Popup_Themtc").modal('show');
    });
    $("body").on('click', '#Edittc', function (e) {
        $("#Popup_Themtc").modal('show');

        let parentTr = $(this).parent().parent().parent();
        let idtc = parentTr.find('td.tcid').data('tcid');
        let tccode = parentTr.find('td.tccode').data('tccode');
        let tcname = parentTr.find('td.tcname').data('tcname');
        let tcmaybit = parentTr.find('td.tcmay').data('tcmay');
        let tcremark = parentTr.find('td.tcremark').data('tcremark');

        $('#lbtdtc').text("Sửa Master Tiêu chuẩn");
        $('#Lbidtc').text(idtc).css('visibility', 'hidden');
        $('#txttccode').val(tccode);
        $('#txttcname').val(tcname);

        if (tcmaybit == "False") {
            $('#tcmaybit').prop('checked', false);
        } else {
            $('#tcmaybit').prop('checked', true);
        }


        $('#tcremark').val(tcremark);

    });
    $('#BtnSave_tc').click(function () {
        $('.check-load').removeClass('d-none');

        var idtc = $('#Lbidtc').text();
        var tccode = $('#txttccode').val();
        var tcname = $('#txttcname').val();
        var tcmay = $('#tcmaybit').is(':checked');
        var remark = $('#tcremark').text();

        $.ajax({
            url: `${window.baseUrl}Master/SaveTC`,
            type: 'POST',
            data: { tid: idtc, ttccode: tccode, ttcname: tcname, ttcmay: tcmay, tremark: remark },
            success: function (response) {
                $('.check-load').addClass('d-none');
                var alertHTML = '<div id="successAlert" class="alert alert-success alert-dismissible fade show" role="alert">'
                    + response.message +
                    '</div>';
                $('#noticeContainer').html(alertHTML);
                setTimeout(function () {
                    $('#successAlert').fadeOut('slow');
                    $('#Popup_Themtc').modal('hide');
                    window.location.reload();
                }, 2000);
            },
            error: function (error) {
                setTimeout(function () {
                    alert('Có lỗi xảy ra: ' + error.responseText);
                    $('.check-load').addClass('d-none');
                }, 500);
            }
        });
    });
    $("body").on('click', '#Deletetc', function (e) {
        let parentTr = $(this).parent().parent().parent();
        let id = parentTr.find('td.tcid').data('tcid');
        $.ajax({
            url: `${window.baseUrl}Master/DelTC`,
            type: 'POST',
            data: { tid: id },

            success: function (response) {
                swal("Thành công!", "Hệ thống xóa thành công!", "success")
                    .then((value) => {
                        window.location.reload();
                    });
            },
            error: function (error) {
                swal("Lỗi!", error.responseText, "error");
            }
        });
    });

    //************************ Control master chi tiết Tiêu chuẩn ***********************************************************************
    $("#Btnthem_detc").click(function () {
        $('#Lbiddetc').text("new").css('visibility', 'hidden');
        $('#lbtddetc').text("Thêm Master chi tiết Tiêu chuẩn");
        $('#dropcha').val("");
        $('#dropdetail').val("");
        $('#txtdename').val("");
        $('#txtdemota').val("");
        $('#txtdetext').val("");
        $('#txtdeint').val("");
        $('#txtdedecimal').val("");
        $('#txtdeunit').val("");
        $('#txtdevalunit').val("");

        $("#Popup_Themdetc").modal('show');
    });
    $("body").on('click', '#Editdetc123', function (e) {
        $("#Popup_Themdetc").modal('show');

        let parentTr = $(this).parent().parent().parent();
        let iddetc = parentTr.find('td.detcid').data('detcid');
        let dename = parentTr.find('td.detcname').data('detcname');
        let demota = parentTr.find('td.detcmota').data('detcmota');
        let detext = parentTr.find('td.detctext').data('detctext');
        let deint = parentTr.find('td.detcint').data('detcint');
        let dedecimal = parentTr.find('td.detcdecimal').data('detcdecimal');
        let deunit = parentTr.find('td.detcunit').data('detcunit');
        let devaunit = parentTr.find('td.detcvaunit').data('detcvaunit');
        let deidtc = parentTr.find('td.deidtc').data('deidtc');

        $('#lbtddetc').text("Sửa Master Chi tiết Tiêu chuẩn");
        $('#Lbiddetc').text(iddetc).css('visibility', 'hidden');
        $('#txtdename').val(dename);
        $('#txtdemota').val(demota);
        $('#txtdetext').val(detext);
        $('#txtdeint').val(deint);
        $('#txtdedecimal').val(dedecimal);
        $('#txtdeunit').val(deunit);
        $('#txtdevalunit').val(devaunit);

        $('#dropcha').val(deidtc);

    });
    $('#dropcha').change(function () {
        var idcha = $(this).val();
        if (idcha) {
            $.ajax({
                url: `${window.baseUrl}Master/GetTC`,
                type: 'POST',
                data: {
                    idCha: idcha
                },
                success: function (response) {
                    let htmlRender = '';
                    response.dataDetail.forEach(item => {
                        htmlRender += `<option value="${item.idDetail}">${item.tenTc}: ${item.valueText || ''}${item.valueInt || ''}${item.valueDecimal || ''}${item.unit || ''}${item.valueUnit || ''}</option>`;
                    });
                    $('#dropdetail').html(htmlRender);
                },
                error: function (error) {
                    swal("Lỗi!", error.responseText, "error");
                }
            });
        }
    });
    $('#BtnSave_detc').click(function () {
        $('.check-load').removeClass('d-none');

        var iddetc = $('#Lbiddetc').text();
        var dename = $('#txtdename').val();
        var demota = $('#txtdemota').val();
        var detext = $('#txtdetext').val();
        var deint = $('#txtdeint').val();
        var dedecimal = $('#txtdedecimal').val();
        var deunit = $('#txtdeunit').val();
        var devaunit = $('#txtdevalunit').val();
        var idtc = $('#dropcha').val();

        $.ajax({
            url: `${window.baseUrl}Master/SavedeTC`,
            type: 'POST',
            data: { tid: iddetc, tname: dename, tmota: demota, ttext: detext, tint: deint, tdecimal: dedecimal, tunit: deunit, tvaunit: devaunit, tidtc: idtc },
            success: function (response) {
                $('.check-load').addClass('d-none');
                var alertHTML = '<div id="successAlert" class="alert alert-success alert-dismissible fade show" role="alert">'
                    + response.message +
                    '</div>';
                $('#noticeContainer').html(alertHTML);
                setTimeout(function () {
                    $('#successAlert').fadeOut('slow');
                    $('#Popup_Themdetc').modal('hide');
                    window.location.reload();
                }, 2000);
            },
            error: function (error) {
                setTimeout(function () {
                    alert('Có lỗi xảy ra: ' + error.responseText);
                    $('.check-load').addClass('d-none');
                }, 500);
            }
        });
    });
    $("body").on('click', '#Deletedetc', function (e) {
        let parentTr = $(this).parent().parent().parent();
        let id = parentTr.find('td.detcid').data('detcid');
        $.ajax({
            url: `${window.baseUrl}Master/DeldeTC`,
            type: 'POST',
            data: { tid: id },
            success: function (response) {
                swal("Thành công!", "Hệ thống xóa thành công!", "success")
                    .then((value) => {
                        window.location.reload();
                    });
            },
            error: function (error) {
                swal("Lỗi!", error.responseText, "error");
            }
        });
    });

    //************************ Control master Location ***********************************************************************
    $("#Btnthemloc").click(function () {
        $('#Lbidloc').text("new").css('visibility', 'hidden');
        $('#lbtdloc').text("Thêm Master Location");
        $('#txtcodeloc').val("");
        $('#txtnameloc').val("");
        $('#idchaloc').val("");

        $("#Popup_Themloc").modal('show');
    });

    $("body").on('click', '#Editloc', function (e) {
        e.preventDefault();
        $("#Popup_Themloc").modal('show');

        let parentTr = $(this).parent().parent().parent();
        let idloc = parentTr.find('td.idloc').data('idloc');
        let codeloc = parentTr.find('td.loccode').data('loccode');
        let nameloc = parentTr.find('td.locname').data('locname');

        let idchaloc = parentTr.find('td.idcloc').data('idcloc');

        $('#lbtdloc').text("Sửa Master Item Number");
        $('#Lbidloc').text(idloc).css('visibility', 'hidden');
        $('#txtcodeloc').val(codeloc);
        $('#txtnameloc').val(nameloc);
        $('#idchaloc').val(idchaloc);
    });

    $('#BtnSave_loc').click(function () {
        $('.check-load').removeClass('d-none');

        var idloc = $('#Lbidloc').text();
        var loccode = $('#txtcodeloc').val();
        var locname = $('#txtnameloc').val();
        var idchaloc = $('#idchaloc').val();

        $.ajax({
            url: `${window.baseUrl}Master/SaveLoc`,
            type: 'POST',
            data: { tid: idloc, tcode: loccode, tname: locname, tidcha: idchaloc },
            success: function (response) {
                $('.check-load').addClass('d-none');
                var alertHTML = '<div id="successAlert" class="alert alert-success alert-dismissible fade show" role="alert">'
                    + response.message +
                    '</div>';
                $('#noticeContainer').html(alertHTML);
                setTimeout(function () {
                    $('#successAlert').fadeOut('slow');
                    $('#Popup_Themloc').modal('hide');
                    window.location.reload();
                }, 2000);
            },
            error: function (error) {
                setTimeout(function () {
                    alert('Có lỗi xảy ra: ' + error.responseText);
                    $('.check-load').addClass('d-none');
                }, 500);
            }
        });
    });
    $("body").on('click', '#Deleteloc', function (e) {
        let parentTr = $(this).parent().parent().parent();
        let id = parentTr.find('td.idloc').data('idloc');
        $.ajax({
            url: `${window.baseUrl}Master/DelLoc`,
            type: 'POST',
            data: { tid: id },

            success: function (response) {
                swal("Thành công!", "Hệ thống xóa thành công!", "success")
                    .then((value) => {
                        window.location.reload();
                    });
            },
            error: function (error) {
                swal("Lỗi!", error.responseText, "error");
            }
        });
    });

    //************************ Báo cáo tháng ***********************************************************************
    $("#Btnupdate_st").click(function () {
        $('#txtitem_st').val("");
        $('#txtlot_st').val("");
        $('#txtqty_st').val("");
        $('#date_st').val("");

        $("#Popup_Themstock").modal('show');
    });
    $('#BtnSave_st').click(function () {
        $('.check-load').removeClass('d-none');
        var itemcode = $('#txtitem_st').val();
        var lotno = $('#txtlot_st').val();
        var qty = $('#txtqty_st').val();
        var date = $('#date_st').val();
        $.ajax({
            url: `${window.baseUrl}Import/Save_st`,
            type: 'POST',
            data: { titem: itemcode, tlot: lotno, tqty: qty, tdate: date },
            success: function (response) {
                $('.check-load').addClass('d-none');
                var alertHTML = '<div id="successAlert" class="alert alert-success alert-dismissible fade show" role="alert">'
                    + response.message +
                    '</div>';

                $('#noticeContainer').html(alertHTML);
                setTimeout(function () {
                    $('#successAlert').fadeOut('slow');
                    $('#Popup_Themstock').modal('hide');
                    window.location.reload();
                }, 2000);
            },
            error: function (error) {
                setTimeout(function () {
                    alert('Có lỗi xảy ra: ' + error.responseText);
                    $('.check-load').addClass('d-none');
                }, 500);
            }
        });
    });
    //Xóa tồn kho
    $('#BtnDel_st').click(function () {
        $('.check-load').removeClass('d-none');
        var itemcode = $('#txtitem_st').val();
        var lotno = $('#txtlot_st').val();      
        var date = $('#date_st').val();
        $.ajax({
            url: `${window.baseUrl}Import/Del_st`,
            type: 'POST',
            data: { titem: itemcode, tlot: lotno, tdate: date },
            success: function (response) {
                $('.check-load').addClass('d-none');
                var alertHTML = '<div id="successAlert" class="alert alert-success alert-dismissible fade show" role="alert">'
                    + response.message +
                    '</div>';
                $('#noticeContainer').html(alertHTML);
                setTimeout(function () {
                    $('#successAlert').fadeOut('slow');
                    $('#Popup_Themstock').modal('hide');
                    window.location.reload();
                }, 3000);
            },
            error: function (error) {
                setTimeout(function () {
                    alert('Có lỗi xảy ra: ' + error.responseText);
                    $('.check-load').addClass('d-none');
                }, 500);
            }
        });
    });

    // Xuất báo cáo sản lượng tổng hợp theo tháng
    $('#BtnBC_slg').click(function (e) {
        e.preventDefault();
        var date = $('#date_stslg').val();
        $(this).append(`<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`);
        if (date === '') {
            setTimeout(function () {
                $('#date_stslg').addClass('border-danger').parent().append('<p class="text-danger">Vui lòng chọn ngày xuất báo cáo</p>');
                $('.spinner-border').remove();
            }, 500);
        } else {
            $.ajax({
                url: `${window.baseUrl}Import/BC_month`,
                type: 'POST',
                data: { tdate: date },
                success: function (response) {
                    setTimeout(() => {
                        var result = window.atob(response.filePath);
                        var excelName = response.excelName;
                        var buffer = new ArrayBuffer(result.length);
                        var bytes = new Uint8Array(buffer);
                        for (let i = 0; i < result.length; i++) {
                            bytes[i] = result.charCodeAt(i);
                        }
                        var blodArr = new Blob([bytes], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                        saveAs(blodArr, excelName);
                        swal('Thành Công', 'Xuất thành công. Vui lòng truy cập vào File mới tải về để xem. Trân trọng cảm ơn!', 'success')
                            .then((isConfirmed) => {
                                if (isConfirmed) {
                                    window.location.reload();
                                }
                            });
                    }, 500);
                },
                error: function (error) {
                    console.log(error);
                    alert('Có lỗi xảy xa. Vui lòng kiểm tra lại!');
                }
            });
        }
     
    });

    // Reset error
    $('#date_stslg').on('input', function (e) {
        $(this).parent().find('p.text-danger').remove();
        $(this).removeClass('border-danger');
    });
    $('#savebit_tk').on('click', function (e) {
        if ($(this).is(':checked')) {
            $(this).parent().find('p.text-danger').remove();
        }
    });

    // Xuất báo cáo tình hình sử dụng NVL
    $('#BtnBC_thsd').click(function (e) {
        e.preventDefault();
        var date = $('#date_stslg').val();
        var includeDetail = $('#savebit_tk').is(':checked');
        $(this).append(`<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`);
        if (date === '' && includeDetail === false) {
            setTimeout(function () {
                $('#date_stslg').addClass('border-danger').parent().append('<p class="text-danger">Vui lòng chọn ngày xuất báo cáo</p>');
                $('#savebit_tk').parent().append('<p class="text-danger">Vui lòng chọn lưu lại tồn</p>');
                $('.spinner-border').remove();
            }, 500);
        } else if (date === '' && includeDetail === true) {
            setTimeout(function () {
                $('#date_stslg').addClass('border-danger').parent().append('<p class="text-danger">Vui lòng chọn ngày xuất báo cáo</p>');
                $('.spinner-border').remove();
            }, 500);
        } else {
            $.ajax({
                url: `${window.baseUrl}Import/BC_nvl`,
                type: 'POST',
                data: { tdate: date, cbsave: includeDetail},
                success: function (response) {
                    setTimeout(() => {
                        var result = window.atob(response.filePath);
                        var excelName = response.excelName;
                        var buffer = new ArrayBuffer(result.length);
                        var bytes = new Uint8Array(buffer);
                        for (let i = 0; i < result.length; i++) {
                            bytes[i] = result.charCodeAt(i);
                        }
                        var blodArr = new Blob([bytes], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                        saveAs(blodArr, excelName);
                        swal('Thành Công', 'Xuất thành công. Vui lòng truy cập vào File mới tải về để xem. Trân trọng cảm ơn!', 'success')
                            .then((isConfirmed) => {
                                if (isConfirmed) {
                                    window.location.reload();
                                }
                            });
                    }, 500);
                },
                error: function (error) {
                    console.log(error);
                    alert('Có lỗi xảy xa. Vui lòng kiểm tra lại!');
                }
            });
        }
    });

    // Xuất báo cáo tỷ lệ đạt
    $('#Btnex_itemfgj').click(function (e) {
        e.preventDefault();
        $(this).append(`<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`);

        var formData = new FormData();
        var fileInput = $('#FileUpload1')[0];
        if (fileInput.files.length > 0) {
            formData.append("FileUpload1", fileInput.files[0]);
        }

        $.ajax({
            url: `${window.baseUrl}Import/BC_stock`,
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,

            success: function (response) {
                setTimeout(() => {
                    var result = window.atob(response.filePath);
                    var excelName = response.excelName;
                    var buffer = new ArrayBuffer(result.length);
                    var bytes = new Uint8Array(buffer);
                    for (let i = 0; i < result.length; i++) {
                        bytes[i] = result.charCodeAt(i);
                    }
                    var blodArr = new Blob([bytes], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                    saveAs(blodArr, excelName);
                    swal('Thành Công', 'Xuất thành công. Vui lòng truy cập vào File mới tải về để xem. Trân trọng cảm ơn!', 'success')
                        .then((isConfirmed) => {
                            if (isConfirmed) {
                                window.location.reload();                              
                            }
                        });
                }, 500);
            },
            error: function (error) {
                console.log(error);
                alert('Có lỗi xảy xa. Vui lòng kiểm tra lại!');
            }
        });
    });
});

//Hiển thị thông báo hàm
window.onload = function () {
    var successMessage = document.getElementById("success-message");
    var errorMessage = document.getElementById("error-message");
    if (successMessage) {
        setTimeout(function () {
            successMessage.style.display = 'none';
        }, 2000);
    }
    if (errorMessage) {
        setTimeout(function () {
            errorMessage.style.display = 'none';
        }, 3000);
    }
};