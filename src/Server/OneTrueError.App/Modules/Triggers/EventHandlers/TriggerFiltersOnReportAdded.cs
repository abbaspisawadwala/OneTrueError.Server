﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Container;
using log4net;
using OneTrueError.Api.Core.Incidents.Events;
using OneTrueError.App.Modules.Triggers.Domain;
using OneTrueError.App.Modules.Triggers.Domain.Actions;

namespace OneTrueError.App.Modules.Triggers.EventHandlers
{
    /// <summary>
    ///     Waits on the ReportAdded and then loads all notifications for the application that the report belongs to.
    /// </summary>
    [Component(RegisterAsSelf = true)]
    public class TriggerFiltersOnReportAdded : IApplicationEventSubscriber<ReportAddedToIncident>
    {
        private readonly ITriggerActionFactory _actionFactory;
        private readonly ITriggerRepository _repository;
        private readonly ILog _logger = LogManager.GetLogger(typeof(TriggerFiltersOnReportAdded));


        /// <summary>
        ///     Creates a new instance of <see cref="TriggerFiltersOnReportAdded" />.
        /// </summary>
        /// <param name="repository">repos</param>
        /// <param name="actionFactory">repos</param>
        /// <exception cref="ArgumentNullException">repository; actionFactory</exception>
        public TriggerFiltersOnReportAdded(ITriggerRepository repository, ITriggerActionFactory actionFactory)
        {
            if (repository == null) throw new ArgumentNullException("repository");
            if (actionFactory == null) throw new ArgumentNullException("actionFactory");
            _repository = repository;
            _actionFactory = actionFactory;
        }

        /// <summary>
        ///     Process an event asynchronously.
        /// </summary>
        /// <param name="e">event to process</param>
        /// <returns>
        ///     Task to wait on.
        /// </returns>
        public async Task HandleAsync(ReportAddedToIncident e)
        {
            _logger.Debug("doing filters..");
            IEnumerable<Trigger> triggers;
            try
            {
                triggers = _repository.GetForApplication(e.Incident.ApplicationId);
            }
            catch (Exception exception)
            {
                _logger.Error("Failed to load triggers for " + e.Incident.ApplicationId, exception);
                return;
            }

            foreach (var trigger in triggers)
            {
                var triggerContext = new TriggerExecutionContext
                {
                    Incident = e.Incident,
                    ErrorReport = e.Report
                };
                var triggerResults = trigger.Run(triggerContext);
                foreach (var actionData in triggerResults)
                {
                    var action = _actionFactory.Create(actionData.ActionName);
                    var context = new ActionExecutionContext
                    {
                        Config = actionData,
                        ErrorReport = e.Report,
                        Incident = e.Incident
                    };
                    await action.ExecuteAsync(context);
                }
            }

            _logger.Debug("filters done..");
        }
    }
}