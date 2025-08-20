using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace GIF.Plugins
{
    public class IntegrationLogUpdater : IPlugin
    {
        private const int STATUS_ACTIVE = 805640000;
        private const int STATUS_PROCESSED = 805640002;

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            tracingService.Trace("=== Start of IntegrationLogUpdater Plugin ===");

            try
            {
                if (!context.InputParameters.Contains("EntityName") || !(context.InputParameters["EntityName"] is string entityName))
                {
                    throw new InvalidPluginExecutionException("EntityName parameter is missing or invalid.");
                }

                entityName = entityName.ToLower();
                tracingService.Trace("Processing Entity: {0}", entityName);

                var transitionalQuery = new QueryExpression("adx_transitionalentityevents")
                {
                    ColumnSet = new ColumnSet("adx_eventtimestamp"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("adx_entityname", ConditionOperator.Equal, entityName),
                            new ConditionExpression("statuscode", ConditionOperator.Equal, STATUS_ACTIVE)
                        }
                    },
                    Orders =
                    {
                        new OrderExpression("adx_eventtimestamp", OrderType.Descending)
                    }
                };

                var activeEvents = service.RetrieveMultiple(transitionalQuery).Entities;

                if (!activeEvents.Any())
                {
                    tracingService.Trace("No active events found for {0}. Exiting.", entityName);
                    return;
                }

                DateTime lastSyncTime = DateTime.UtcNow;
                    //activeEvents
                    //.Max(e => e.GetAttributeValue<DateTime>("adx_eventtimestamp"));
                tracingService.Trace("Latest active event timestamp: {0}", lastSyncTime);

                var logQuery = new QueryExpression("adx_integrationlogs")
                {
                    ColumnSet = new ColumnSet("adx_entityname", "adx_lastsynctime"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("adx_entityname", ConditionOperator.Equal, entityName)
                        }
                    }
                };

                var existingLog = service.RetrieveMultiple(logQuery).Entities.FirstOrDefault();

                if (existingLog != null)
                {
                    tracingService.Trace("Record exists. Updating Last Sync Time.");
                    existingLog["adx_lastsynctime"] = lastSyncTime;
                    service.Update(existingLog);
                }
                else
                {
                    tracingService.Trace("Record does not exist. Creating new Integration Log entry.");
                    var newLog = new Entity("adx_integrationlogs")
                    {
                        ["adx_entityname"] = entityName,
                        ["adx_lastsynctime"] = lastSyncTime
                    };
                    service.Create(newLog);
                }

                tracingService.Trace("Updating status of processed transitional records...");
                foreach (var evt in activeEvents)
                {
                    var updateEvt = new Entity(evt.LogicalName, evt.Id)
                    {
                        ["statuscode"] = new OptionSetValue(STATUS_PROCESSED)
                    };
                    service.Update(updateEvt);
                }

                tracingService.Trace("Processed {0} transitional records for entity {1}.", activeEvents.Count, entityName);

                tracingService.Trace("Integration Logs updated successfully.");
                tracingService.Trace("=== End of IntegrationLogUpdater Plugin ===");
            }
            catch (Exception ex)
            {
                tracingService.Trace("Exception occurred: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("Integration Log Updater failed.", ex);
            }
        }
    }
}