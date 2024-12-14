var dataTable;
$(document).ready(function () {
    loadDataTable();
})
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Product/GetAll"
        },
        "columns": [
            { "data": "title", "width": "15%" },
            { "data": "description", "width": "20%" },
            { "data": "author", "width": "15%" },
            { "data": "isbn", "width": "15%" },
            { "data": "price", "width": "15%" },

            {
                "data": "id", "class": "text-center",
                "render": function (data) {
                    return `<a href="/Admin/Product/upsert/${data}" class="btn btn-info"><i class="fas fa-edit"></i></a>
                            <a class="btn btn-danger" onclick=Delete("/Admin/Product/Delete/${data}")><i class="fas fa-trash-alt"></i></a>`;
                }
            }
        ]
    })
}
function Delete(url) {
    //alert(url);
    swal({
        title: "Want to Delete data",
        text: "Delete Information",
        icon: "error",
        buttons: true,
        dangerModel: true
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
                        toastr.error(data.message)
                    }
                }
            })
        }
    })
}