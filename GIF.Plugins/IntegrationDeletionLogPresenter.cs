using System;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace GIF.Plugins
{
    public class IntegrationDeletionLogPresenter : IPlugin
    {
        private static readonly (string LogicalName, int OptionSetValue)[] SupportedEntities = new[]
        {
            ("systemuser", 805640000),
            ("customergroup", 805640001),
            ("currency", 805640002),
            ("organization", 805640003),
            ("pricelevel", 805640004),
            ("pricelistitem", 805640005),
            ("product", 805640006),
            ("productinventory", 805640007),
            ("incidenttype", 805640008)
        };

        private static readonly (string LogicalName, int OptionSetValue)[] SupportedActions = new[]
        {
            ("Create", 805640000),
            ("Update", 805640001),
            ("Delete", 805640002),
            ("Re-Assign", 805640003)
        };

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            tracingService.Trace("=== Start of IntegrationDeletionLogPresenter Plugin ===");
            tracingService.Trace("Primary Entity: {0}, Message: {1}", context.PrimaryEntityName, context.MessageName);

            try
            {
                string entityName = context.PrimaryEntityName.ToLower();
                string messageName = context.MessageName;

                tracingService.Trace("Looking up supported entity for: {0}", entityName);
                var entityInfo = SupportedEntities.FirstOrDefault(e => e.LogicalName == entityName);
                tracingService.Trace("Looking up supported action for: {0}", messageName);
                var messageInfo = SupportedActions.FirstOrDefault(e => e.LogicalName == messageName);

                if (entityInfo == default)
                {
                    tracingService.Trace("Entity '{0}' not supported for logging. Exiting plugin.", entityName);
                    return;
                }

                Guid recordId = Guid.Empty;

                switch (messageName)
                {
                    case "Create":
                        tracingService.Trace("Handling Create operation...");
                        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                        {
                            recordId = context.OutputParameters.Contains("id") ? (Guid)context.OutputParameters["id"] : Guid.Empty;
                            tracingService.Trace("Extracted Record ID: {0}", recordId);

                            if (recordId != Guid.Empty)
                                CreateLog(service, tracingService, entityInfo.OptionSetValue, messageInfo.OptionSetValue, isDelete: false, "Create", entityName);
                        }
                        break;

                    case "Update":
                        tracingService.Trace("Handling Update operation...");
                        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity target)
                        {
                            recordId = target.Id;
                            tracingService.Trace("Extracted Record ID: {0}", recordId);

                            if (recordId != Guid.Empty)
                                CreateLog(service, tracingService, entityInfo.OptionSetValue, messageInfo.OptionSetValue, isDelete: false, "Update", entityName);
                        }
                        break;

                    case "Delete":
                        tracingService.Trace("Handling Delete operation...");
                        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference reference)
                        {
                            recordId = reference.Id;
                            tracingService.Trace("Extracted Record ID: {0}", recordId);

                            if (recordId != Guid.Empty)
                                CreateLog(service, tracingService, entityInfo.OptionSetValue, messageInfo.OptionSetValue, isDelete: true, "Delete", entityName);
                        }
                        break;

                    default:
                        tracingService.Trace("Unsupported message name: {0}", messageName);
                        break;
                }

                tracingService.Trace("=== End of IntegrationDeletionLogPresenter Plugin ===");
            }
            catch (Exception ex)
            {
                tracingService.Trace("Exception occurred: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("Integration/Deletion Log plugin failed.", ex);
            }
        }

        private void CreateLog(IOrganizationService service, ITracingService tracingService, int optionSetValue, int messageOptionSetValue, bool isDelete, string messageName, string entityName)
        {
            tracingService.Trace("Entering CreateLog method...");
            tracingService.Trace("Log Type: {0}, Entity: {1}, Action: {2}, OptionSetValue: {3}, ActionOptionSet: {4}",
                isDelete ? "Delete" : "Integration", entityName, messageName, optionSetValue, messageOptionSetValue);

            string logEntityName = isDelete ? "gif_deletelog" : "gif_integrationlog";

            var log = new Entity(logEntityName)
            {
                ["gif_name"] = "Log - " + entityName + " - " + messageName,
                ["gif_entityname"] = new OptionSetValue(optionSetValue),
                ["gif_action"] = new OptionSetValue(messageOptionSetValue)
            };

            tracingService.Trace("Prepared Log Entity - Name: {0}, Entity OptionSet: {1}, Action OptionSet: {2}",
                log["gif_name"], optionSetValue, messageOptionSetValue);

            Guid logId = service.Create(log);
            tracingService.Trace("Log record created successfully. Log ID: {0}", logId);
        }
    }
}