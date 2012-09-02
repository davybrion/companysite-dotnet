<div>
[javascript]
        $.get('/Home/Locations', function (data) {
            $.each(data, function () {
                var id = this.Id;
                var marker = new google.maps.Marker({
                    position: new google.maps.LatLng(this.Coordinates.Lat, this.Coordinates.Lng),
                    map: map
                });

                google.maps.event.addListener(marker, 'click', function (event) {
                    showDialogFor(id);
                });
            });
        });
[/javascript]
</div>
