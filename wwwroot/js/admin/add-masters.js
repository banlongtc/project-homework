'use-strict';
document.addEventListener('DOMContentLoaded', function (e) {
    if ($('.form-add-new').length > 0) {
        $('.btn-upload-csv').on('click', function (e) {
            e.preventDefault();
            var csvFile = $('#filePathCSV').val();
            if (csvFile == "") {
                alert('Vui lòng chọn file');
                return;
            }
            let filePath = document.getElementById('filePathCSV');
            const files = filePath.files;
            const fileFormData = new FormData();
            fileFormData.append('file', files[0]);
            fetch(`${window.baseUrl}masterpositions/uploadcsv`, {
                method: 'POST',
                body: fileFormData
            })
                .then(async response => {
                    if (!response.ok) {
                        const errorResponse = await response.json();
                        throw new Error(`${response.status} - ${errorResponse.message}`);
                    }
                    return response.json();
                })
                .then(data => {
                    alert(data);
                    window.location.reload();
                })
                .catch(error => {
                    alert(error);
                })
        });

        $('.btn-upload-tool').on('click', function (e) {
            e.preventDefault();
            var csvFile = $('#filePathCSV').val();
            if (csvFile == "") {
                alert('Vui lòng chọn file');
                return;
            }
            let filePath = document.getElementById('filePathCSV');
            const files = filePath.files;
            const fileFormData = new FormData();
            fileFormData.append('file', files[0]);
            fetch(`${window.baseUrl}mastertools/uploadcsv`, {
                method: 'POST',
                body: fileFormData
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
                    window.location.href = data.href;
                })
                .catch(error => {
                    alert(error);
                })
        });
    }
});