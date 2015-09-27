var uri = 'http://synology.servebeer.com:8089/api/beers/?$orderby=pricePerLitre';

$(document).ready(function () {

    showSpinner();

    // Send an AJAX request
    $.getJSON(uri)
        .done(function (data) {
            // On success, 'data' contains a list of products.
            $.each(data, function (key, item) {
                // Add a list item for the product.
                $('#beerslist > tbody:last-child').append(formatItem(item));
            });

            //Hide spinner
            hideSpinner();

            //Enable table sorter after filling the table
            $('#beerslist').DataTable();
        });
});

function formatItem(item) {

    var trOpen = '<tr>';

    //if (item.total == item.capacity)
    //    trOpen = '<tr class=\"reference\">';
    var img = '<td><img src=\"' + item.imageUrl + '\"/></td>';
    var name = '<td><a href=\"' + item.detailsUrl + '"\" target=\"_blank\">' + item.name + '</a></td>';
    var total = '<td>' + item.total + '</td>';
    var capacity = '<td>' + item.capacity + '</td>';
    var price = '<td>' + item.priceAfter + '</td>';
    var ppl = '<td>' + item.pricePerLitre + '</td>';

    return trOpen + img + name + total + capacity + price + ppl + '</tr>';
}

var spinner;

function showSpinner() {

    var opts = {
          lines: 13 // The number of lines to draw
        , length: 28 // The length of each line
        , width: 14 // The line thickness
        , radius: 20 // The radius of the inner circle
        , scale: 1 // Scales overall size of the spinner
        , corners: 1 // Corner roundness (0..1)
        , color: '#F00' // #rgb or #rrggbb or array of colors
        , opacity: 0.25 // Opacity of the lines
        , rotate: 0 // The rotation offset
        , direction: 1 // 1: clockwise, -1: counterclockwise
        , speed: 1 // Rounds per second
        , trail: 60 // Afterglow percentage
        , fps: 20 // Frames per second when using setTimeout() as a fallback for CSS
        , zIndex: 2e9 // The z-index (defaults to 2000000000)
        , className: 'spinner' // The CSS class to assign to the spinner
        , top: '50%' // Top position relative to parent
        , left: '50%' // Left position relative to parent
        , shadow: false // Whether to render a shadow
        , hwaccel: false // Whether to use hardware acceleration
        , position: 'relative' // Element positioning
    }

    var target = document.getElementById('spinner');
    spinner = new Spinner(opts).spin(target);
}

function hideSpinner() {
    spinner.stop();
    $('#spinner').hide();
}