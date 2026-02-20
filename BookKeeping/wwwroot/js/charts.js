(function (window) {
    'use strict';

    var categoryExpenseChart = null;
    var dailyTrendChart = null;
    var moneyFormatter = new Intl.NumberFormat('zh-TW', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    function destroyChart(chartInstance) {
        if (chartInstance) {
            chartInstance.destroy();
        }
    }

    function createCategoryExpenseChart(categoryCanvas, categoryExpenses) {
        destroyChart(categoryExpenseChart);

        categoryExpenseChart = new Chart(categoryCanvas, {
            type: 'doughnut',
            data: {
                labels: categoryExpenses.map(function (item) { return item.label; }),
                datasets: [{
                    data: categoryExpenses.map(function (item) { return item.value; }),
                    backgroundColor: categoryExpenses.map(function (item) { return item.color || '#6C757D'; }),
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: 'nearest',
                    intersect: false
                },
                plugins: {
                    legend: {
                        position: 'bottom'
                    },
                    tooltip: {
                        padding: 12,
                        titleMarginBottom: 8,
                        bodySpacing: 6,
                        callbacks: {
                            label: function (context) {
                                var percentage = categoryExpenses[context.dataIndex] ? categoryExpenses[context.dataIndex].percentage : 0;
                                return context.label + ': $' + moneyFormatter.format(context.raw) + ' (' + percentage.toFixed(2) + '%)';
                            }
                        }
                    }
                }
            }
        });
    }

    function createDailyTrendChart(dailyTrendCanvas, dailyTrends) {
        destroyChart(dailyTrendChart);

        dailyTrendChart = new Chart(dailyTrendCanvas, {
            type: 'line',
            data: {
                labels: dailyTrends.map(function (item) { return item.date; }),
                datasets: [{
                    label: '收入',
                    data: dailyTrends.map(function (item) { return item.income; }),
                    borderColor: '#198754',
                    backgroundColor: 'rgba(25, 135, 84, 0.15)',
                    pointHitRadius: 20,
                    tension: 0.3
                }, {
                    label: '支出',
                    data: dailyTrends.map(function (item) { return item.expense; }),
                    borderColor: '#DC3545',
                    backgroundColor: 'rgba(220, 53, 69, 0.15)',
                    pointHitRadius: 20,
                    tension: 0.3
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: 'index',
                    intersect: false
                },
                plugins: {
                    legend: {
                        position: 'bottom'
                    },
                    tooltip: {
                        padding: 12,
                        titleMarginBottom: 8,
                        bodySpacing: 6
                    }
                },
                scales: {
                    y: {
                        ticks: {
                            callback: function (value) {
                                return '$' + moneyFormatter.format(value);
                            }
                        }
                    }
                }
            }
        });
    }

    async function loadMonthlyCharts(chartDataUrl) {
        var categoryCanvas = document.getElementById('categoryExpenseChart');
        var dailyTrendCanvas = document.getElementById('dailyTrendChart');

        if (!categoryCanvas || !dailyTrendCanvas || typeof Chart === 'undefined') {
            return;
        }

        try {
            var response = await fetch(chartDataUrl, {
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error('Unable to load chart data.');
            }

            var chartData = await response.json();
            createCategoryExpenseChart(categoryCanvas, chartData.categoryExpenses || []);
            createDailyTrendChart(dailyTrendCanvas, chartData.dailyTrends || []);
        } catch (error) {
            console.error('Failed to initialize report charts.', error);
        }
    }

    window.reportCharts = {
        loadMonthlyCharts: loadMonthlyCharts
    };
})(window);
