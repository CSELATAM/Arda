﻿using System;
using System.Collections.Generic;
using Arda.Kanban.ViewModels;

namespace Arda.Kanban.Models.Repositories
{
    public interface IMetricRepository
    {
        // Add a new metric to the database.
        bool AddNewMetric(MetricViewModel metric);

        // Update some metric data based on id.
        bool EditMetricByID(MetricViewModel metric);

        // Return a list of metrics.
        List<MetricViewModel> GetAllMetrics();

        // Return a list of metrics by year.
        List<MetricViewModel> GetAllMetrics(int year);

        // Return a specific metric by ID.
        MetricViewModel GetMetricByID(Guid id);

        // Delete a metric based on ID
        bool DeleteMetricByID(Guid id);
    }
}
