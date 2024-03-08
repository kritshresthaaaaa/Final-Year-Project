function printOrderDetails() {
    // Create an iframe element
    var printFrame = document.createElement('iframe');
    printFrame.style.visibility = 'hidden';
    printFrame.style.position = 'fixed';
    printFrame.style.right = '0';
    printFrame.style.bottom = '0';

    document.body.appendChild(printFrame);

    // Get the HTML content you want to print
    var contentToPrint = document.querySelector('.print-container').innerHTML;
    printFrame.contentDocument.open();
    printFrame.contentDocument.write('<html><head><title>Print</title>');
    // Optionally include a link to a CSS file for print styling
    printFrame.contentDocument.write('<link rel="stylesheet" href="./css/print-styles.css">');
    printFrame.contentDocument.write('</head><body>');
    printFrame.contentDocument.write(contentToPrint);
    printFrame.contentDocument.write('</body></html>');
    printFrame.contentDocument.close();

    // Print the iframe's content
    printFrame.contentWindow.onload = function () {
        printFrame.contentWindow.print();

        // Remove the iframe after printing
        document.body.removeChild(printFrame);
    };
}
