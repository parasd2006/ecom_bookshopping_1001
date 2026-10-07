var searchData;

$(document).ready(function () {
    loadSearch();
});

function loadSearch() {

    $("#searchForm").submit(function (e) {

        e.preventDefault();

        searchData = {
            searchType: $("#searchType").val(),
            searchText: $("#searchText").val()
        };

        $.ajax({
            "url": "/Customer/Home/Search",
            "type": "GET",
            "data": searchData,

            "success": function (data) {

                $("#productList").html(data);

            },

            "error": function () {

                alert("Something went wrong.");

            }
        });

    });
}