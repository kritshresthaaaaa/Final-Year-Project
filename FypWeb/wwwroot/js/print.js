function printOrderDetails() {
    var printFrame = document.createElement('iframe');
    printFrame.style.visibility = 'hidden';
    printFrame.style.position = 'fixed';
    printFrame.style.right = '0';
    printFrame.style.bottom = '0';

    document.body.appendChild(printFrame);

    var contentToPrint = document.querySelector('.print-container').innerHTML;

    printFrame.contentDocument.open();
    printFrame.contentDocument.write('<html><head><title>Print</title><style>');
    printFrame.contentDocument.write(`
/* CSS for printed content */
body {
    font-family: Arial, sans-serif;
    line-height: 1.6;
    margin: 0;
    padding: 0;
}

h2 {
    font-size: 24px;
    font-weight: bold;
    color: #333;
}

h4 {
    font-size: 18px;
    font-weight: bold;
    color: #666;
}

p {
    font-size: 16px;
    color: #333;
    margin-bottom: 10px;
}

table {
    width: 100%;
    border-collapse: collapse;
    margin-bottom: 20px;
}

th, td {
    border: 1px solid #ddd;
    padding: 8px;
    text-align: left;
}

th {
    background-color: #f2f2f2;
    font-weight: bold;
}

tr:hover {
    background-color: #f5f5f5;
}

.flex {
    display: flex;
    align-items: center;
    justify-content: center;
}

/* Button style */
.button {
    display: inline-block;
    background-color: #007bff;
    color: #fff;
    padding: 10px 20px;
    text-align: center;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    transition: background-color 0.3s;
}

.button:hover {
    background-color: #0056b3;
}
    `);
    printFrame.contentDocument.write('</style></head><body>');
    printFrame.contentDocument.write(contentToPrint);
    printFrame.contentDocument.write('</body></html>');
    printFrame.contentDocument.close();
    printFrame.contentWindow.print();
    document.body.removeChild(printFrame);
}
