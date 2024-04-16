function loadChart(timeFrame, timeValue) {
    // Ensure that barChart is a Chart instance and has the destroy method
    if (barChart && typeof barChart.destroy === 'function') {
        barChart.destroy();
        barChart = null;
    }
    function transparentize(color, opacity) {
        const hexToRgb = hex => {
            let r = parseInt(hex.slice(1, 3), 16),
                g = parseInt(hex.slice(3, 5), 16),
                b = parseInt(hex.slice(5, 7), 16);
            return `rgba(${r}, ${g}, ${b}, ${opacity})`;
        };

        return hexToRgb(color);
    }
    function generateRandomNumbers({ count, min, max }) {
        return Array.from({ length: count }, () => Math.floor(Math.random() * (max - min + 1) + min));
    }

    function generateMonthLabels(count) {
        const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        return months.slice(0, count);
    }
    const DATA_COUNT = 7;
    const NUMBER_CFG = { count: DATA_COUNT, min: -100, max: 100 };
    const CHART_COLORS = {
        red: '#FF0000',
        blue: '#0000FF',
        green: '#00FF00',
        yellow: '#FFFF00'
    };
    // Define labels based on the timeframe
    const labels = generateMonthLabels(DATA_COUNT); // Ensure this method generates 7 month labels

/*    if (timeFrame === 'week') {
        labels = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    } else if (timeFrame === 'month') {
        labels = Array.from({ length: 30 }, (_, i) => `Day ${i + 1}`);
    } else if (timeFrame === 'year') {
        labels = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    }*/
    function generateRandomData(length, max) {
        return Array.from({ length: length }, () => Math.floor(Math.random() * max));
    }
    var barData = {
        labels: labels,
        datasets: [
            {
                label: 'Dataset 1',
                data: generateRandomNumbers(NUMBER_CFG),
                borderColor: CHART_COLORS.red,
                backgroundColor: transparentize(CHART_COLORS.red, 0.5),
            },
            {
                label: 'Dataset 2',
                data: generateRandomNumbers(NUMBER_CFG),
                borderColor: CHART_COLORS.blue,
                backgroundColor: transparentize(CHART_COLORS.blue, 0.5),
            },
            {
                label: 'Dataset 3',
                data: generateRandomNumbers(NUMBER_CFG),
                borderColor: CHART_COLORS.green,
                backgroundColor: transparentize(CHART_COLORS.green, 0.5),
            },
            {
                label: 'Dataset 4',
                data: generateRandomNumbers(NUMBER_CFG),
                borderColor: CHART_COLORS.yellow,
                backgroundColor: transparentize(CHART_COLORS.yellow, 0.5),
            }
        ]

    };

    // Bar Chart Configuration
    const barConfig = {
        type: 'bar',
        data: barData,
        options: {
            indexAxis: 'y', // Makes the bar chart horizontal
            elements: {
                bar: {
                    borderWidth: 2,
                }
            },
            responsive: true,
            plugins: {
                legend: {
                    position: 'right',
                },
                title: {
                    display: true,
                    text: 'Chart.js Horizontal Bar Chart'
                }
            }
        },
    };

    // Creating the chart
    ctxBar = document.getElementById('barChart').getContext('2d');
    barChart = new Chart(ctxBar, barConfig);

    // Fetch and load data
    generateChartData(timeFrame, timeValue);
}

function generateChartData(timeFrame, timeValue) {
    let url = ''; // Define the URL for data fetching based on timeFrame
    if (timeFrame === 'week') {
        url = '/Admin/Home/GetWeeklyShoppingStatus';
    } else if (timeFrame === 'month') {
        url = `/Admin/Home/GetMonthlyShoppingStatus?month=${timeValue.month}&year=${timeValue.year}`;
    } else if (timeFrame === 'year') {
        url = `/Admin/Home/GetYearlyShoppingStatus?year=${timeValue}`;
    }

    fetch(url)
        .then(response => response.json())
        .then(data => {
            var salesData = data.map(item => item.totalSales);
            barChart.data.datasets[0].data = salesData;
            barChart.update();
        })
        .catch(error => console.error('Error fetching data:', error));
}
