var dataTable;
$(document).ready(function () {
    loadDataTable();
})
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url":"/Admin/CoverType/GetAll"
        },
       
        lengthMenu: [3, 6, 9],
        "columns": [
            {
                "data": "id","class":"text-center",
                "render": function (data) {
                    return `<a href="/Admin/CoverType/upsert/${data}" class="btn btn-info"><i class="fas fa-edit"></i></a>
                      <a class="btn btn-danger" onclick=Delete("/Admin/CoverType/Delete/${data}")><i class="fas fa-trash-alt"></i></a>
`; 
                }
            },
            { "data": "name", "widht": "50%" }

        ]

        })
}
function Delete(url) {
    //alert(url);
    swal({
        title: "want to delete",
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
                        toastr.error(data.message)
                    }
                }
            })
        }

    })
}