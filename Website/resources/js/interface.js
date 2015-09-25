var uri = 'http://synology.servebeer.com:8089/api/beers/?$orderby=pricePerLitre';

$(document).ready(function () {
    // Send an AJAX request
    $.getJSON(uri)
        .done(function (data) {
            // On success, 'data' contains a list of products.
            $.each(data, function (key, item) {
                // Add a list item for the product.
                $('#beerslist > tbody:last-child').append(formatItem(item));
            });
            //Enable table sorter after filling the table
            $('#beerslist').tablesorter();
        });
});

function formatItem(item) {

    var trOpen = '<tr>';

    if (item.total == item.capacity)
        trOpen = '<tr class=\"reference\">';

    var name = '<td class=\"lalign\">' + item.name + '</td>';
    var total = '<td>' + item.total + '</td>';
    var capacity = '<td>' + item.capacity + '</td>';
    var ppl = '<td>' + item.pricePerLitre + '€</td>';

    return trOpen + name + total + capacity + ppl + '</tr>';
}