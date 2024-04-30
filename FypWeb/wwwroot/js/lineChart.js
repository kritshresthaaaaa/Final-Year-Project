var myChart = null;
var ctx = null;

document.addEventListener('DOMContentLoaded', function () {
    ctx = document.getElementById('myChart');
    loadChart('week');
    document.getElementById('week').addEventListener('click', function () {
        loadChart('week');
        document.getElementById('monthPickerContainer').classList.add('hidden');
        document.getElementById('yearPickerContainer').classList.add('hidden');
    });
    document.getElementById('month').addEventListener('click', function () {
        document.getElementById('monthPickerContainer').classList.remove('hidden');
        document.getElementById('yearPickerContainer').classList.add('hidden');
        // Set the default value to the current month and year
        var currentDate = new Date();
        var currentYear = currentDate.getFullYear();
        var currentMonth = currentDate.getMonth() + 1; // getMonth() is zero-based
        var formattedMonth = currentMonth < 10 ? '0' + currentMonth : currentMonth;
        var formattedDate = currentYear + '-' + formattedMonth;

        var monthPicker = document.getElementById('monthPicker');
        monthPicker.value = formattedDate;

        // Trigger the change event manually if you want to load the chart right away
        loadChart('month', { year: currentYear, month: currentMonth });
    });
    document.getElementById('year').addEventListener('click', function () {
        document.getElementById('monthPickerContainer').classList.add('hidden');
        document.getElementById('yearPickerContainer').classList.remove('hidden');

        // Set the default value to the current year
        var currentYear = new Date().getFullYear();

        var yearPicker = document.getElementById('yearPicker');
        yearPicker.value = currentYear; // Set the year picker to the current year

        // Optionally, load the chart for the current year immediately
        loadChart('year', currentYear);
    });

    // Added 'change' event listener for monthPicker
    document.getElementById('monthPicker').addEventListener('change', function () {
        var selectedMonthYear = this.value; // YYYY-MM
        if (selectedMonthYear) {
            var [year, month] = selectedMonthYear.split('-');
            loadChart('month', { year: parseInt(year), month: parseInt(month) });
        }
    });

});
function highlightActiveTimeframe(timeframe) {
    // Remove active class from all buttons
    document.querySelectorAll('.timeframe-btn').forEach(button => {
        button.classList.remove('active-timeframe');
    });

    // Add active class to the selected timeframe button
    document.getElementById(timeframe).classList.add('active-timeframe');
}

document.getElementById('yearPicker').addEventListener('change', function () {
    var selectedYear = this.value;
    console.log(selectedYear);
    loadChart('year', selectedYear);
});

function loadChart(timeFrame, timeValue) {
    // If the chart already exists, destroy it to start fresh
    if (myChart !== null) {
        myChart.destroy();
    }
    // Setting static labels for the days of the week
    var staticLabels = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

    // Placeholder for initial chart data
    var initialData = {
        labels: staticLabels, // Initialize with empty labels
        datasets: [{
            label: 'Shopping Status',
            backgroundColor: '#D1E7DD',
            borderColor: '#198754',
            data: [], // Initialize with empty data
        }]
    };
    let chartType = 'line';
    // Chart configuration
    const config = {
        type: chartType,
        data: initialData,
        options: {
            responsive: true,
            plugins: {
                subtitle: {
                    display: true,
                    text: 'Shopping Status',
                    font: {
                        size: 16,
                    },
                    padding: {
                        bottom: 5,
                    }
                },
                legend: {
                    position: 'bottom',
                },
            }
        },
    };

    // Create a new chart instance with the initial data
    ctx = document.getElementById('myChart');
    myChart = new Chart(ctx, config);

    // Load new data for the chart
    generateChartData(timeFrame, timeValue);
}

function generateChartData(timeFrame, timeValue) {
    if (timeFrame === 'week') {
        fetch('/Admin/Home/GetWeeklyShoppingStatus')
            .then(response => response.json())
            .then(data => {
                // Initialize an array to hold sales data for each day, defaulting to 0
                const salesDataForWeek = []; // Sunday to Saturday

                // Map each dayOfWeek to an index (assuming your dayOfWeek is a string like "Sunday")
                const dayOfWeekToIndex = {
                    "Sunday": 0,
                    "Monday": 1,
                    "Tuesday": 2,
                    "Wednesday": 3,
                    "Thursday": 4,
                    "Friday": 5,
                    "Saturday": 6
                };

                // Populate salesDataForWeek based on fetched data
                data.forEach(item => {
                    const dayIndex = dayOfWeekToIndex[item.dayOfWeek];
                    if (dayIndex !== undefined) { // Check if the dayOfWeek from data is valid
                        salesDataForWeek[dayIndex] = item.totalSales;
                    }
                });

                // Ensure that sales data is set for all days of the week
                for (let i = 0; i < staticLabels.length; i++) {
                    if (salesDataForWeek[i] === undefined) {
                        salesDataForWeek[i] = 0; // Set sales data to zero for missing days
                    }
                }

            })
            .catch(error => console.error('Error fetching weekly data:', error));
    }
    if (timeFrame == 'month') {
        fetch('/Admin/Home/GetMonthlyShoppingStatus?month=' + timeValue.month + '&year=' + timeValue.year)
            .then(response => response.json())
            .then(data => {
                // Initialize an array to hold sales data for each day, defaulting to 0
                var salesDataForMonth = data.map(item => item.totalSales);
                var daysOfMonth = data.map(item => item.day); // Or however you

                if (myChart) {
                    myChart.data.labels = daysOfMonth;
                    myChart.data.datasets[0].data = salesDataForMonth;
                    myChart.data.datasets[0].label = 'Total Sales ';
                    myChart.update();
                }
            })
            .catch(error => console.error('Error fetching monthly data:', error));
    }
    if (timeFrame === 'year') {
        fetch(`/Admin/Home/GetYearlyShoppingStatus?year=${timeValue}`)
            .then(response => response.json())
            .then(data => {
                // Initialize an array to hold sales data for each month, defaulting to 0
                var salesDataForYear = data.map(item => item.totalSales);
                var monthsOfYear = data.map(item => item.month); // Or however you

                if (myChart) {
                    myChart.data.labels = monthsOfYear;
                    myChart.data.datasets[0].data = salesDataForYear;
                    myChart.data.datasets[0].label = 'Total Sales for ' + timeValue;
                    myChart.update();
                }
            })
            .catch(error => console.error('Error fetching yearly data:', error));
    }

}