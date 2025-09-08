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
        private const int IncidentOptionSetValue = 805640008;
        private const int PriorityOptionSetValue = 805640011;
        private const int ProjectOptionSetValue = 805640012;

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
                    case "incident": \
                        HandleIncidentDelete(service, tracingService, context, target);
                        break;
                    case "priority":
                        HandlePriorityDelete(service, tracingService, context, target);
                        break;
                    case "project":
                        HandleProjectDelete(service, tracingService, context, target);
                        break;
                    case "workorder":
                        HandleWorkOrderDelete(service, tracingService, context, target);
                        break;
                    case "contact":
                        HandleContactDelete(service, tracingService, context, target);
                        break;
                    case "lead":
                        HandleLeadDelete(service, tracingService, context, target);
                        break;
                    case "pricelevel": 
                        HandlePriceListDelete(service, tracingService, context, target);
                        break;
                    case "productpricelevel": 
                        HandlePriceListItemDelete(service, tracingService, context, target);
                        break;
                    case "activitymonitor":
                        HandleActivityMonitorDelete(service, tracingService, context, target);
                        break;
                    case "gif_pricerequest":
                        HandlePriceRequestDelete(service, tracingService, context, target);
                        break;
                    case "msdyn_productinventory":
                        HandleProductInventoryDelete(service, tracingService, context, target);
                        break;
                    case "product":
                        HandleProductDelete(service, tracingService, context, target);
                        break;
                    case "msdyn_resolution":
                        HandleResolutionDelete(service, tracingService, context, target);
                        break;
                    case "gif_reservationrequest":
                        HandleReservationRequestDelete(service, tracingService, context, target);
                        break;
                    case "appointment":
                        HandleAppointmentDelete(service, tracingService, context, target);
                        break;
                    case "gif_customertransaction":
                        HandleCustomerTransactionDelete(service, tracingService, context, target);
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

                var deleteLog = new Entity(DeleteLogEntityName)
                {
                    ["gif_entityid"] = target.Id.ToString(),
                    ["gif_entityname"] = new OptionSetValue(UserOptionSetValue),
                    ["gif_name"] = $"User Removed from Gif_Team - {target.Id}",
                    ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
                };

                service.Create(deleteLog);
                tracingService.Trace($"[DeleteLogPlugin] Delete log successfully created for systemuser {target.Id}");
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

        // -----------------------------
        // Incident Delete (NEW)
        // -----------------------------
        private void HandleIncidentDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(IncidentOptionSetValue),
                ["gif_name"] = $"Incident Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Incident {target.Id}");
        }
        // -----------------------------
        // Priority Delete
        // -----------------------------
        private void HandlePriorityDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(PriorityOptionSetValue),
                ["gif_name"] = $"Priority Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Priority {target.Id}");
        }

        // -----------------------------
        // Project Delete
        // -----------------------------
        private void HandleProjectDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(ProjectOptionSetValue),
                ["gif_name"] = $"Project Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Project {target.Id}");
        }
        // -----------------------------
        // Work Order Delete
        // -----------------------------
        private void HandleWorkOrderDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(805640015), // Work Order
                ["gif_name"] = $"Work Order Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Work Order {target.Id}");
        }
        // -----------------------------
        // Contact Delete
        // -----------------------------
        private void HandleContactDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(805640014), // Contact
                ["gif_name"] = $"Contact Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Contact {target.Id}");
        }

        // -----------------------------
        // Lead Delete
        // -----------------------------
        private void HandleLeadDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(805640013), // Lead
                ["gif_name"] = $"Lead Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Lead {target.Id}");
        }

        // -----------------------------
        // Price List Item Delete
        // -----------------------------
        private void HandlePriceListItemDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(805640005), // Price List Item
                ["gif_name"] = $"Price List Item Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Price List Item {target.Id}");
        }

        // -----------------------------
        // Price List Delete
        // -----------------------------
        private void HandlePriceListDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(805640004), // Price List
                ["gif_name"] = $"Price List Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Price List {target.Id}");
        }
        // -----------------------------
        // Activity Monitor Delete
        // -----------------------------
        private void HandleActivityMonitorDelete(
            IOrganizationService service,
            ITracingService tracingService,
            IPluginExecutionContext context,
            EntityReference target)
        {
            try
            {
                tracingService.Trace($"[DeleteLogPlugin] Delete triggered for Activity Monitor {target.Id}");

                var deleteLog = new Entity(DeleteLogEntityName)
                {
                    ["gif_entityid"] = target.Id.ToString(),                      // primary key = entity id
                    ["gif_entityname"] = new OptionSetValue(805640016),          // OptionSet value for Activity Monitor
                    ["gif_name"] = $"Activity Monitor Deleted - {target.Id}",    // descriptive name
                    ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
                };

                service.Create(deleteLog);
                tracingService.Trace($"[DeleteLogPlugin] Delete log successfully created for Activity Monitor {target.Id}");
            }
            catch (Exception ex)
            {
                tracingService.Trace($"[DeleteLogPlugin] Error in HandleActivityMonitorDelete: {ex.Message}");
                throw new InvalidPluginExecutionException("Error while creating delete log record for Activity Monitor.", ex);
            }
        }
        // -----------------------------
        // Price Request Delete
        // -----------------------------
        private void HandlePriceRequestDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(805640017),
                ["gif_name"] = $"Price Request Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Price Request {target.Id}");
        }

        // -----------------------------
        // Product Inventory Delete
        // -----------------------------
        private void HandleProductInventoryDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(805640018),
                ["gif_name"] = $"Product Inventory Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Product Inventory {target.Id}");
        }

        // -----------------------------
        // Product Delete
        // -----------------------------
        private void HandleProductDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(805640019),
                ["gif_name"] = $"Product Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Product {target.Id}");
        }

        // -----------------------------
        // Resolution Delete
        // -----------------------------
        private void HandleResolutionDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(805640020),
                ["gif_name"] = $"Resolution Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Resolution {target.Id}");
        }

        // -----------------------------
        // Reservation Request Delete
        // -----------------------------
        private void HandleReservationRequestDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(805640021),
                ["gif_name"] = $"Reservation Request Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Reservation Request {target.Id}");
        }

        // -----------------------------
        // Appointment Delete
        // -----------------------------
        private void HandleAppointmentDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(805640022),
                ["gif_name"] = $"Appointment Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Appointment {target.Id}");
        }

        // -----------------------------
        // Customer Transaction Delete
        // -----------------------------
        private void HandleCustomerTransactionDelete(IOrganizationService service, ITracingService tracingService, IPluginExecutionContext context, EntityReference target)
        {
            var deleteLog = new Entity(DeleteLogEntityName)
            {
                ["gif_entityid"] = target.Id.ToString(),
                ["gif_entityname"] = new OptionSetValue(0), // Replace 0 if CustomerTransaction has a specific OptionSetValue
                ["gif_name"] = $"Customer Transaction Deleted - {target.Id}",
                ["ownerid"] = new EntityReference("systemuser", context.InitiatingUserId)
            };
            service.Create(deleteLog);
            tracingService.Trace($"Delete log created for Customer Transaction {target.Id}");
        }

    }
}
