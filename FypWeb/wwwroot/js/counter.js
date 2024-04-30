document.addEventListener('DOMContentLoaded', function () {
    const counterElements = document.querySelectorAll('.counter');

    counterElements.forEach(function (counterElement) {
        const targetCount = parseInt(counterElement.textContent);
        const duration = 8000; // Increased duration of the animation in milliseconds
        const frameRate = 20; // Frame rate in frames per second
        const increment = Math.ceil(targetCount / (duration / 1000 * frameRate)); // Increment value per frame

        let currentCount = 0;

        const counterInterval = setInterval(function () {
            currentCount += increment;
            if (currentCount >= targetCount) {
                clearInterval(counterInterval);
                currentCount = targetCount;
            }
            counterElement.textContent = currentCount.toLocaleString(); // Display the current count
        }, 1000 / frameRate);
    });
});
