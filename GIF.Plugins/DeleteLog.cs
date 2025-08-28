using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Plugins
{
    public class AgreementDeleteLogPlugin : IPlugin
    {
        private const string AgreementEntityName = "msdyn_agreement";
        private const string DeleteLogEntityName = "gif_deletelog";

        public void Execute(IServiceProvider serviceProvider)
        {
            // Get services
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            tracingService.Trace("AgreementDeleteLogPlugin started.");

            // Only run on Delete
            if (!string.Equals(context.MessageName, "Delete", StringComparison.OrdinalIgnoreCase))
            {
                tracingService.Trace("Message is not Delete. Exiting.");
                return;
            }

            // Validate Target
            if (!(context.InputParameters.TryGetValue("Target", out var targetObj) && targetObj is EntityReference target))
            {
                tracingService.Trace("Target parameter missing or invalid. Exiting.");
                return;
            }

            if (!string.Equals(target.LogicalName, AgreementEntityName, StringComparison.OrdinalIgnoreCase))
            {
                tracingService.Trace($"Target entity {target.LogicalName} is not {AgreementEntityName}. Exiting.");
                return;
            }

            try
            {
                // Retrieve Agreement Number for readability
                var agreement = service.Retrieve(AgreementEntityName, target.Id, new ColumnSet("msdyn_name"));
                string agreementNumber = agreement.Contains("msdyn_name") ? agreement["msdyn_name"].ToString() : target.Id.ToString();

                // Create delete log
                var deleteLog = new Entity(DeleteLogEntityName)
                {
                    ["gif_entityid"] = target.Id,                  // GUID of agreement
                    ["gif_entityname"] = "msdyn_agreement",        // hard-coded entity name
                    ["gif_name"] = $"Agreement Deleted - {agreementNumber}",
                    ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
                };

                service.Create(deleteLog);
                tracingService.Trace($"Delete log created for Agreement {target.Id}");
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Error creating delete log: {ex.Message}");
                throw new InvalidPluginExecutionException("Error while creating delete log record.", ex);
            }

            tracingService.Trace("AgreementDeleteLogPlugin completed successfully.");
        }
    }
}
