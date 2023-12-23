

    document.addEventListener("DOMContentLoaded", function() {
    const dropdownLinks = document.querySelectorAll("[data-te-dropdown-toggle-ref]");

    dropdownLinks.forEach(function(link) {
        link.addEventListener("click", function (event) {
            event.preventDefault();
            const targetDropdownId = this.getAttribute("data-dropdown-target");
            const targetDropdown = document.getElementById(targetDropdownId);
            if (targetDropdown) {
                targetDropdown.classList.toggle("hidden");
            }
        });
    });
  });
