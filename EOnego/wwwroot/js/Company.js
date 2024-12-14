var dataTable;
$(document).ready(function () {
    loadDataTable();
})
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Company/GetAll"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "streetAddress", "width": "15%" },
            { "data": "state", "width": "15%" },
            { "data": "city", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            {
                "data": "isAuthorizedCompany",
                "render": function (data) {
                    if (data) {
                        return `<input type="checkbox" disabled checked />`;
                    }
                    else {
                        return `<input type="checkbox" disabled/>`
                    }
                }
            },
            {
                "data": "id","width":"15%",
                "render": function (data) {
                    return `<a href="/Admin/Company/upsert/${data}" class="btn btn-success"><i class="fas fa-edit"></i></a>
                    <a class="btn btn-danger" onclick=Delete("/Admin/Company/Delete/${data}")><i class="fas fa-trash-alt"></i></a>`;
                }
            }
        ]
    })
}
function Delete(url) {
    swal({
        title: "Want to Delete Data",
        text: "Delete Information",
        icon: "warning",
        buttons: true,
        dangerMode:true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
                })
        }
    })
}