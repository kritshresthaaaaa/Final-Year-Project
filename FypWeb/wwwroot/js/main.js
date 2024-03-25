document.addEventListener('DOMContentLoaded', function () {
    const serviceOptions = document.querySelectorAll('.service-option');

    serviceOptions.forEach(option => {
        option.addEventListener('click', function () {
            // Toggle "selected" state using Tailwind CSS classes
            if (this.classList.contains('ring-2')) {
                // It's already selected, so deselect it
                this.classList.remove('ring-2', 'ring-offset-2', 'ring-[#7258DB]');
                // Optionally, clear the selected service if storing the value
document.getElementById('selectedService').value = ''; // Clear selected service
            } else {
                // First, remove the selection styles from all options
                serviceOptions.forEach(opt => opt.classList.remove('ring-2', 'ring-offset-2', 'ring-[#7258DB]'));
                // Then, apply the selection style to the clicked option
                this.classList.add('ring-2', 'ring-offset-2', 'ring-[#7258DB]');
                // Optionally, set the selected service if storing the value
                document.getElementById('selectedService').value = this.dataset.service; // Set selected service
            }
        });
    });
});
