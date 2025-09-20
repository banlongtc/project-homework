"use-strict";
document.addEventListener("DOMContentLoaded", function () {
    if ($('.table-all-roles').length > 0) {
        if ($('.table-all-roles table').length > 0) {
            new DataTable($('.table-all-roles table'), {
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
        }
        $('#roleName').on('change', function () {
            var valName = $(this).val();
            if (valName.includes(' ')) {
                valName = valName.replace(' ', '_');
                $('#roleCode').val(valName.toLowerCase());
            } else {
                $('#roleCode').val(valName.toLowerCase());
            }
        });

        $('.btn-save-role').on('click', function (e) {
            e.preventDefault();
            var dataRole = {};
            if ($('#roleName').val() != "") {
                dataRole = {
                    name: $('#roleName').val(),
                    code: $('#roleCode').val(),
                    descriptions: $('.descriptions-role').val()
                };
                fetch(`${window.baseUrl}RoleManagement/add`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json; charset=utf-8'
                    },
                    body: JSON.stringify(dataRole)
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
                        console.log(error);
                    })
            }
        });
    }
});