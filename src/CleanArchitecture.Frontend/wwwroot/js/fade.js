$(document).ready(function () {
    // Fade out effect on page load
    $("body").addClass("fade-in active");

    // Handle link clicks for page transitions
    $("a").on("click", function (e) {
        e.preventDefault();
        var newLocation = this.href;

        $("body").removeClass("fade-in active").addClass("fade-out active");

        setTimeout(function () {
            window.location = newLocation;
        }, 100); // Match this time with the CSS transition duration
    });
});