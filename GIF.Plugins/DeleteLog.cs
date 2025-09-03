using System;
using Microsoft.Xrm.Sdk;

namespace Plugins
{
    public class DeleteLogPlugin : IPlugin
    {
        private const string DeleteLogEntityName = "gif_deletelog";

        // OptionSet values for different entities
        private const int UserOptionSetValue = 805640005;
        private const int AgreementOptionSetValue = 805640009;
        private const int CustomerGroupOptionSetValue = 805640001;
        private const int CustomerAssetOptionSetValue = 805640002;
        private const int AccountOptionSetValue = 805640010;

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            tracingService.Trace("DeleteLogPlugin started.");

            if (!string.Equals(context.MessageName, "Delete", StringComparison.OrdinalIgnoreCase))
                return;

            if (!(context.InputParameters.TryGetValue("Target", out var targetObj) && targetObj is EntityReference target))
                return;

            try
            {
                switch (target.LogicalName)
                {
                    case "systemuser":
                        HandleUserDelete(service, tracingService, context, target);
                        break;
                    case "msdyn_agreement":
                        HandleAgreementDelete(service, tracingService, context, target);
                        break;
                    case "msdyn_customergroup":
                        HandleCustomerGroupDelete(service, tracingService, context, target);
                        break;
                    case "msdyn_customerasset":
                        HandleCustomerAssetDelete(service, tracingService, context, target);
                        break;
                    case "account":
                        HandleAccountDelete(service, tracingService, context, target);
                        break;
                    default:
                        tracingService.Trace($"Entity {target.LogicalName} not handled.");
                        break;
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Error creating delete log: {ex.Message}");
                throw new InvalidPluginExecutionException("Error while creating delete log record.", ex);
            }

            tracingService.Trace("DeleteLogPlugin completed successfully.");
        }

        // -----------------------------
        // Users (Soft Delete via Team Removal or Deactivation)
        // -----------------------------
        private void HandleUserDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            try
            {
                tracingService.Trace($"[DeleteLogPlugin] Delete triggered for systemuser {target.Id}");

                // Optional: check if user is part of Gif_Team before removing
                // For simplicity, assuming removal triggered the plugin
                var entityId = target.Id;
                var entityName = "systemuser";

                var deleteLog = new Entity(DeleteLogEntityName)
                {
                    ["gif_entityid"] = entityId.ToString(),               // primary key = entity id
                    ["gif_entityname"] = new OptionSetValue(UserOptionSetValue), // entity type
                    ["gif_name"] = $"User Removed from Gif_Team - {entityId}",    // descriptive name
                    ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
                };

                service.Create(deleteLog);
                tracingService.Trace($"[DeleteLogPlugin] Delete log successfully created for systemuser {entityId}");
            }
            catch (Exception ex)
            {
                tracingService.Trace($"[DeleteLogPlugin] Error in HandleUserDelete: {ex.Message}");
                throw new InvalidPluginExecutionException("Error while creating delete log record for User.", ex);
            }
        }

        // -----------------------------
        // Agreement Delete
        // -----------------------------
        private void HandleAgreementDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(AgreementOptionSetValue),
                ["gif_name"] = $"Agreement Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Agreement {target.Id}");
        }

        // -----------------------------
        // Customer Group Delete
        // -----------------------------
        private void HandleCustomerGroupDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(CustomerGroupOptionSetValue),
                ["gif_name"] = $"Customer Group Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Customer Group {target.Id}");
        }

        // -----------------------------
        // Customer Asset Delete
        // -----------------------------
        private void HandleCustomerAssetDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(CustomerAssetOptionSetValue),
                ["gif_name"] = $"Customer Asset Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Customer Asset {target.Id}");
        }

        // -----------------------------
        // Account Delete
        // -----------------------------
        private void HandleAccountDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(AccountOptionSetValue),
                ["gif_name"] = $"Account Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Account {target.Id}");
        }
    }
}
