document.addEventListener('DOMContentLoaded', function () {
    const counterElements = document.querySelectorAll('.counter');

    counterElements.forEach(function (counterElement) {
        const targetCount = parseInt(counterElement.textContent);
        const duration = 500; // Increased duration of the animation in milliseconds
        const frameRate = 60; // Frame rate in frames per second
        const increment = Math.max(1, Math.ceil(targetCount / (animationDuration / 1000 * frameRate)));


        let currentCount = 0;
        const counterInterval = setInterval(function () {
            currentCount = Math.min(currentCount + increment, targetCount); // Ensure it doesn't exceed target

            if (currentCount === targetCount) {
                clearInterval(counterInterval);
            }

            counterElement.textContent = currentCount.toLocaleString(); // Display formatted count
        }, 100 / frameRate);
    });
});