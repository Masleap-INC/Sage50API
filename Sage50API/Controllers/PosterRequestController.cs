using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

using System.Drawing;
using Newtonsoft.Json;
using SimplySDK;
using SimplySDK.GeneralModule;
using SimplySDK.InventoryModule;
using SimplySDK.PayableModule;
using SimplySDK.PayrollModule;
using SimplySDK.ProjectModule;
using SimplySDK.ReceivableModule;
using SimplySDK.Support;
using System.Configuration;

namespace Sage50API.Controllers
{
    /// <summary>
    /// PosterRequestController
    /// </summary>
    public class PosterRequestController : ApiController
    {

        /// <summary>
        /// PostVerb
        /// </summary>
        private enum PostVerb
        {
            CREATE,
            LOOKUP,
            ADJUST,
            VOID
        }

        /// <summary>
        /// Defines the set of supported modules for this sample application.
        /// </summary>
        private enum PostModule
        {
            SALES_INVOICE,
            PURCHASE_INVOICE,
            GENERAL_JOURNAL,
            CUSTOMER_LEDGER,
            VENDOR_LEDGER,
            INVENTORY_LEDGER,
            EMPLOYEE_LEDGER,
            PROJECT_LEDGER,
            ACCOUNT_LEDGER,
            SQL_NONQUERY
        }

        private const string ERROR_UNABLE_OPEN_LEDGER = "Error opening ledger. Verify you have sufficient rights and a company file is open.";
        private const string ERROR_UNABLE_OPEN_JOURNAL = "Error opening journal. Verify you have sufficient rights and a company file is open.";

        /// <summary>
        /// Constructs the complete action name used within a request.
        /// </summary>
        /// <param name="action">The action to be performed for the request.</param>
        /// <param name="module">The targetted module for the request.</param>
        /// <returns>the complete action name</returns>
        private string GetActionName(PostVerb action, PostModule module)
        {
            return $"{GetActionVerb(action)}_{GetModuleName(module)}";
        }

        /// <summary>
        /// Constructs the verb portion of the action name used within a request.
        /// </summary>
        /// <param name="action">The action to be performed for the request.</param>
        /// <returns>the verb portion of the action name</returns>
        private string GetActionVerb(PostVerb action)
        {
            string verb = "";
            switch (action)
            {
                case PostVerb.ADJUST:
                    verb = "adjust";
                    break;
                case PostVerb.CREATE:
                    verb = "create";
                    break;
                case PostVerb.LOOKUP:
                    verb = "lookup";
                    break;
                case PostVerb.VOID:
                    verb = "void";
                    break;
            }
            return verb;
        }

        /// <summary>
        /// Constructs the module portion of the action name used within a request.
        /// </summary>
        /// <param name="module">The targetted module for the request.</param>
        /// <returns>the module portion of the action name</returns>
        private string GetModuleName(PostModule module)
        {
            string moduleName = "";
            switch (module)
            {
                case PostModule.CUSTOMER_LEDGER:
                    moduleName = "customer";
                    break;
                case PostModule.PURCHASE_INVOICE:
                    moduleName = "purchase_invoice";
                    break;
                case PostModule.SALES_INVOICE:
                    moduleName = "sales_invoice";
                    break;
                case PostModule.VENDOR_LEDGER:
                    moduleName = "vendor";
                    break;
                case PostModule.GENERAL_JOURNAL:
                    moduleName = "general_journal";
                    break;
                case PostModule.INVENTORY_LEDGER:
                    moduleName = "inventory";
                    break;
                case PostModule.EMPLOYEE_LEDGER:
                    moduleName = "employee";
                    break;
                case PostModule.PROJECT_LEDGER:
                    moduleName = "project";
                    break;
                case PostModule.ACCOUNT_LEDGER:
                    moduleName = "account";
                    break;
                case PostModule.SQL_NONQUERY:
                    moduleName = "sql_non_query";
                    break;
            }
            return moduleName;
        }

        /// <summary>
        /// Post an edit to a chart of account record.
        /// </summary>
        /// <param name="request">Details of the chart of account to save</param>
        /// <returns>the response of the save to display to user</returns>
        private PosterResponse AdjustAccount(PosterBody request)
        {
            try
            {
                PosterResponse response = new PosterResponse();
                AccountLedger acctLedger = null;
                try
                {
                    acctLedger = SDKInstanceManager.Instance.OpenAccountLedger();
                    if (acctLedger == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }

                try
                {
                    int acctNum;
                    if (int.TryParse(request.FindAccountNumber, out acctNum) && acctLedger.LoadByAccountNumber(acctNum))
                        response = PostAccount(request, acctLedger);
                    else
                        response.Messages.Add($"Adjust account {request.FindAccountNumber} FAILED");
                }
                catch (Exception ex)
                {
                    response.Messages.Add($"Adjust FAILED: {ex.Message}");
                }

                return response;
            }
            finally
            {
                SDKInstanceManager.Instance.CloseAccountLedger();
            }
        }

        /// <summary>
        /// Post an edit to a project ledger record.
        /// </summary>
        /// <param name="request">Details of the project to save</param>
        /// <returns>the response of the save to display to user</returns>
        private PosterResponse AdjustProject(PosterBody request)
        {
            try
            {
                PosterResponse response = new PosterResponse();
                ProjectLedger projLedger = null;
                try
                {
                    projLedger = SDKInstanceManager.Instance.OpenProjectLedger();
                    if (projLedger == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }

                try
                {
                    if (projLedger.LoadByName(request.FindName))
                        response = PostProject(request, projLedger);
                    else
                        response.Messages.Add($"Adjust project {request.FindName} FAILED");
                }
                catch (Exception ex)
                {
                    response.Messages.Add($"Adjust FAILED: {ex.Message}");
                }

                return response;
            }
            finally
            {
                SDKInstanceManager.Instance.CloseProjectLedger();
            }
        }

        /// <summary>
        /// Post a transaction to adjust a purchase invoice.
        /// </summary>
        /// <param name="request">Details of the transaction to post</param>
        /// <returns>the response of the post to display to user</returns>
        private PosterResponse AdjustPurchaseInvoice(PosterBody request)
        {
            try
            {
                PurchasesJournal purJourn = null;
                try
                {
                    purJourn = SDKInstanceManager.Instance.OpenPurchasesJournal();
                    if (purJourn == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    PosterResponse response = new PosterResponse();
                    response.Messages.Add(ERROR_UNABLE_OPEN_JOURNAL);
                    return response;
                }
                return AdjustInvoice(request, purJourn);
            }
            finally
            {
                SDKInstanceManager.Instance.ClosePurchasesJournal();
            }
        }

        /// <summary>
        /// Post a transaction to adjust a sales invoice.
        /// </summary>
        /// <param name="request">Details of the transaction to post</param>
        /// <returns>the response of the post to display to user</returns>
        private PosterResponse AdjustSalesInvoice(PosterBody request)
        {
            try
            {
                SalesJournal salJourn = null;
                try
                {
                    salJourn = SDKInstanceManager.Instance.OpenSalesJournal();
                    if (salJourn == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    PosterResponse response = new PosterResponse();
                    response.Messages.Add(ERROR_UNABLE_OPEN_JOURNAL);
                    return response;
                }
                return AdjustInvoice(request, salJourn);
            }
            finally
            {
                SDKInstanceManager.Instance.CloseSalesJournal();
            }
        }

        /// <summary>
        /// Post an edit to a vendor ledger record.
        /// </summary>
        /// <param name="request">Details of the vendor to save</param>
        /// <returns>the response of the save to display to user</returns>
        private PosterResponse AdjustVendor(PosterBody request)
        {
            try
            {
                PosterResponse response = new PosterResponse();
                VendorLedger venLedger = null;
                try
                {
                    venLedger = SDKInstanceManager.Instance.OpenVendorLedger();
                    if (venLedger == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }

                try
                {
                    if (venLedger.LoadByName(request.FindName))
                    {
                        if (!string.IsNullOrWhiteSpace(request.TaxID))
                            venLedger.TaxId = request.TaxID;
                        response = PostCustomerVendor(request, venLedger);
                    }
                    else
                        response.Messages.Add($"Adjust vendor {request.FindName} FAILED");
                }
                catch (Exception ex)
                {
                    response.Messages.Add($"Adjust FAILED: {ex.Message}");
                }

                return response;
            }
            finally
            {
                SDKInstanceManager.Instance.CloseVendorLedger();
            }
        }

        /// <summary>
        /// Post a transaction to adjust an invoice.
        /// </summary>
        /// <param name="request">Details of the transaction to post</param>
        /// <param name="journ">The opened invoice module that will do the posting</param>
        /// <returns>the response of the post to display to user</returns>
        private PosterResponse AdjustInvoice(PosterBody request, InvoiceJournal journ)
        {
            PosterResponse response = new PosterResponse();
            try
            {
                if (!journ.LoadInvoiceForAdjust(request.FindCusVenName, request.FindInvoiceNumber))
                {
                    response.Messages.Add($"Adjust invoice {request.FindInvoiceNumber} for {request.FindCusVenName} FAILED");
                    return response;
                }
                response = PostInvoice(request, journ);
            }
            catch (Exception ex)
            {
                response.Messages.Add($"Adjust FAILED: {ex.Message}");
            }
            finally
            {
                //response.Messages
            }

            return response;
        }

        /// <summary>
        /// Post an edit to a customer ledger record.
        /// </summary>
        /// <param name="request">Details of the customer to save</param>
        /// <returns>the response of the save to display to user</returns>
        private PosterResponse AdjustCustomer(PosterBody request)
        {
            try
            {
                PosterResponse response = new PosterResponse();
                CustomerLedger custLedger = null;
                try
                {
                    custLedger = SDKInstanceManager.Instance.OpenCustomerLedger();
                    if (custLedger == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }

                try
                {
                    if (custLedger.LoadByName(request.FindName))
                        response = PostCustomerVendor(request, custLedger);
                    else
                        response.Messages.Add($"Adjust customer {request.FindName} FAILED");
                }
                catch (Exception ex)
                {
                    response.Messages.Add($"Adjust FAILED: {ex.Message}");
                }

                return response;
            }
            finally
            {
                SDKInstanceManager.Instance.CloseCustomerLedger();
            }
        }

        /// <summary>
        /// Post an edit to an employee ledger record.
        /// </summary>
        /// <param name="request">Details of the employee to save</param>
        /// <returns>the response of the save to display to user</returns>
        private PosterResponse AdjustEmployee(PosterBody request)
        {
            try
            {
                PosterResponse response = new PosterResponse();
                EmployeeLedgerBase empLedger = null;
                try
                {
                    empLedger = SDKInstanceManager.Instance.OpenEmployeeLedger();
                    if (empLedger == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }

                try
                {
                    if (empLedger.LoadByName(request.FindName))
                        response = PostEmployee(request, empLedger);
                    else
                        response.Messages.Add($"Adjust employee {request.FindName} FAILED");
                }
                catch (Exception ex)
                {
                    response.Messages.Add($"Adjust FAILED: {ex.Message}");
                }

                return response;
            }
            finally
            {
                SDKInstanceManager.Instance.CloseEmployeeLedger();
            }
        }

        /// <summary>
        /// Post a transaction to adjust a general journal.
        /// </summary>
        /// <param name="request">Details of the transaction to post</param>
        /// <returns>the response of the post to display to user</returns>
        private PosterResponse AdjustGeneralJournal(PosterBody request)
        {
            try
            {
                PosterResponse response = new PosterResponse();
                GeneralJournal genJourn = null;
                try
                {
                    genJourn = SDKInstanceManager.Instance.OpenGeneralJournal();
                    if (genJourn == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    response.Messages.Add(ERROR_UNABLE_OPEN_JOURNAL);
                    return response;
                }

                try
                {
                    int id;
                    if (int.TryParse(request.FindJournalId, out id) && genJourn.AdjustJournalEntry(id))
                        response = PostGeneralJournal(request, genJourn);
                    else
                        response.Messages.Add($"Adjust general journal {request.FindJournalId} FAILED");
                }
                catch (Exception ex)
                {
                    response.Messages.Add($"Adjust FAILED: {ex.Message}");
                }

                return response;
            }
            finally
            {
                SDKInstanceManager.Instance.CloseGeneralJournal();
            }
        }

        /// <summary>
        /// Post an edit to an inventory ledger record.
        /// </summary>
        /// <param name="request">Details of the inventory to save</param>
        /// <returns>the response of the save to display to user</returns>
        private PosterResponse AdjustInventory(PosterBody request)
        {
            try
            {
                PosterResponse response = new PosterResponse();
                InventoryLedger inventLedger = null;
                try
                {
                    inventLedger = SDKInstanceManager.Instance.OpenInventoryLedger();
                    if (inventLedger == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }

                try
                {
                    if (inventLedger.LoadByPartCode(request.FindPartCode))
                        response = PostInventory(request, inventLedger);
                    else
                        response.Messages.Add($"Adjust inventory {request.FindPartCode} FAILED");
                }
                catch (Exception ex)
                {
                    response.Messages.Add($"Adjust FAILED: {ex.Message}");
                }

                return response;
            }
            finally
            {
                SDKInstanceManager.Instance.CloseInventoryLedger();
            }
        }

        /// <summary>
        /// Post a transaction to create a sales invoice.
        /// </summary>
        /// <param name="request">Details of the transaction to post</param>
        /// <returns>the response of the post to display to user</returns>
        private PosterResponse CreateSalesInvoice(PosterBody request)
        {
            SalesJournal salJourn;
            PosterResponse response = new PosterResponse();
            try
            {
                salJourn = SDKInstanceManager.Instance.OpenSalesJournal();
                if (salJourn == null)
                    throw new SimplyNoAccessException();
            }
            catch
            {
                response.Messages.Add(ERROR_UNABLE_OPEN_JOURNAL);
                return response;
            }

            try
            {
                response = PostInvoice(request, salJourn);
            }
            finally
            {
                //response.Messages;
                SDKInstanceManager.Instance.CloseSalesJournal();
            }

            return response;
        }

        /// <summary>
        /// Create a new chart of account record.
        /// </summary>
        /// <param name="request">Details of the chart of account to save</param>
        /// <returns>the response of the save to display to user</returns>
        private PosterResponse CreateAccount(PosterBody request)
        {
            try
            {
                AccountLedger acctLedger = null;
                try
                {
                    acctLedger = SDKInstanceManager.Instance.OpenAccountLedger();
                    acctLedger.InitializeNew();
                }
                catch
                {
                    PosterResponse response = new PosterResponse();
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }
                return PostAccount(request, acctLedger);
            }
            finally
            {
                SDKInstanceManager.Instance.CloseAccountLedger();
            }
        }

        /// <summary>
        /// Create a new customer record.
        /// </summary>
        /// <param name="request">Details of the customer to save</param>
        /// <returns>the response of the save to display to user</returns>
        private PosterResponse CreateCustomer(PosterBody request)
        {
            try
            {
                CustomerLedger custLedger = null;
                try
                {
                    custLedger = SDKInstanceManager.Instance.OpenCustomerLedger();
                    custLedger.InitializeNew();
                }
                catch
                {
                    PosterResponse response = new PosterResponse();
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }
                return PostCustomerVendor(request, custLedger);
            }
            finally
            {
                SDKInstanceManager.Instance.CloseCustomerLedger();
            }
        }

        /// <summary>
        /// Create a new employee record.
        /// </summary>
        /// <param name="request">Details of the employee to save</param>
        /// <returns>the response of the save to display to user</returns>
        private PosterResponse CreateEmployee(PosterBody request)
        {
            try
            {
                EmployeeLedgerBase emptLedger = null;
                try
                {
                    emptLedger = SDKInstanceManager.Instance.OpenEmployeeLedger();
                    emptLedger.InitializeNew();
                }
                catch
                {
                    PosterResponse response = new PosterResponse();
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }
                return PostEmployee(request, emptLedger);
            }
            finally
            {
                SDKInstanceManager.Instance.CloseEmployeeLedger();
            }
        }

        /// <summary>
        /// Post a transaction to create a general journal.
        /// </summary>
        /// <param name="request">Details of the transaction to post</param>
        /// <returns>the response of the post to display to user</returns>
        private PosterResponse CreateGeneralJournal(PosterBody request)
        {
            try
            {
                GeneralJournal genJourn = null;
                try
                {
                    genJourn = SDKInstanceManager.Instance.OpenGeneralJournal();
                    if (genJourn == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    PosterResponse response = new PosterResponse();
                    response.Messages.Add(ERROR_UNABLE_OPEN_JOURNAL);
                    return response;
                }
                return PostGeneralJournal(request, genJourn);
            }
            finally
            {
                SDKInstanceManager.Instance.CloseGeneralJournal();
            }
        }

        /// <summary>
        /// Create a new inventory record.
        /// </summary>
        /// <param name="request">Details of the inventory to save</param>
        /// <returns>the response of the save to display to user</returns>
        private PosterResponse CreateInventory(PosterBody request)
        {
            try
            {
                InventoryLedger inventLedger = null;
                try
                {
                    inventLedger = SDKInstanceManager.Instance.OpenInventoryLedger();
                    inventLedger.InitializeNew();
                }
                catch
                {
                    PosterResponse response = new PosterResponse();
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }
                return PostInventory(request, inventLedger);
            }
            finally
            {
                SDKInstanceManager.Instance.CloseInventoryLedger();
            }
        }


        /// <summary>
        /// Create a new project record.
        /// </summary>
        /// <param name="request">Details of the project to save</param>
        /// <returns>the response of the save to display to user</returns>
        private PosterResponse CreateProject(PosterBody request)
        {
            try
            {
                ProjectLedger projLedger = null;
                try
                {
                    projLedger = SDKInstanceManager.Instance.OpenProjectLedger();
                    projLedger.InitializeNew();
                }
                catch
                {
                    PosterResponse response = new PosterResponse();
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }
                return PostProject(request, projLedger);
            }
            finally
            {
                SDKInstanceManager.Instance.CloseProjectLedger();
            }
        }

        /// <summary>
        /// Post a transaction to create a purchase invoice.
        /// </summary>
        /// <param name="request">Details of the transaction to post</param>
        /// <returns>the response of the post to display to user</returns>
        private PosterResponse CreatePurchaseInvoice(PosterBody request)
        {
            PurchasesJournal purJourn;
            PosterResponse response = new PosterResponse();
            try
            {
                purJourn = SDKInstanceManager.Instance.OpenPurchasesJournal();
                if (purJourn == null)
                    throw new SimplyNoAccessException();
            }
            catch
            {
                response.Messages.Add(ERROR_UNABLE_OPEN_JOURNAL);
                return response;
            }

            try
            {
                response = PostInvoice(request, purJourn);
            }
            finally
            {
                //response.Messages
                SDKInstanceManager.Instance.ClosePurchasesJournal();
            }

            return response;
        }

        /// <summary>
        /// Create a blank sample request for creating a chart of account.
        /// </summary>
        /// <param name="verb">The desired action to create the request for</param>
        /// <returns>a blank request where recommended fields for input by the user are initialized with an empty string.</returns>
        private PosterRequest CreateTemplateAccount(PostVerb verb)
        {
            PosterRequest request = null;

            switch (verb)
            {
                case PostVerb.ADJUST:
                case PostVerb.CREATE:
                    request = new PosterRequest()
                    {
                        Body = new PosterBody()
                        {
                            Name = "",
                            NameAlt = "",
                            AccountNumber = "",
                            AccountType = "",
                            AccountClass = ""
                        }
                    };

                    if (verb == PostVerb.ADJUST)
                        request.Body.FindAccountNumber = "";

                    break;

                case PostVerb.LOOKUP:
                    request = new PosterRequest()
                    {
                        Body = new PosterBody()
                        {
                            FindAccountNumber = ""
                        }
                    };
                    break;
            }

            return request;
        }

        /// <summary>
        /// Create a blank sample request for creating a customer or vendor.
        /// </summary>
        /// <param name="verb">The desired action to create the request for</param>
        /// <returns>a blank request where recommended fields for input by the user are initialized with an empty string.</returns>
        private PosterRequest CreateTemplateCustomerVendor(PostVerb verb)
        {
            PosterRequest request = null;

            switch (verb)
            {
                case PostVerb.ADJUST:
                case PostVerb.CREATE:
                    request = new PosterRequest()
                    {
                        Body = new PosterBody()
                        {
                            Name = "",
                            Contact = "",
                            Street1 = "",
                            Street2 = "",
                            City = "",
                            Province = "",
                            Country = "",
                            PostalCode = "",
                            Phone1 = "",
                            Phone2 = "",
                            Fax = "",
                            Email = "",
                            Website = "",
                            CurrencyCode = ""
                        }
                    };

                    if (verb == PostVerb.ADJUST)
                        request.Body.FindName = "";

                    break;

                case PostVerb.LOOKUP:
                case PostVerb.VOID:
                    request = new PosterRequest()
                    {
                        Body = new PosterBody()
                        {
                            FindName = ""
                        }
                    };
                    break;
            }

            return request;
        }

        /// <summary>
        /// Create a blank sample request for creating an employee record.
        /// </summary>
        /// <param name="verb">The desired action to create the request for</param>
        /// <returns>a blank request where recommended fields for input by the user are initialized with an empty string.</returns>
        private PosterRequest CreateTemplateEmployee(PostVerb verb)
        {
            PosterRequest request = null;

            switch (verb)
            {
                case PostVerb.ADJUST:
                case PostVerb.CREATE:
                    request = new PosterRequest()
                    {
                        Body = new PosterBody()
                        {
                            Name = "",
                            Street1 = "",
                            Street2 = "",
                            City = "",
                            Province = "",
                            PostalCode = "",
                            Phone1 = "",
                            Phone2 = "",
                            SocialInsuranceNumber = "",
                            BirthDate = "",
                            HireDate = "",
                            TaxTableProvince = "",
                            PayPeriods = ""
                        }
                    };

                    if (verb == PostVerb.ADJUST)
                        request.Body.FindName = "";

                    break;

                case PostVerb.LOOKUP:
                    request = new PosterRequest()
                    {
                        Body = new PosterBody()
                        {
                            FindName = ""
                        }
                    };
                    break;
            }

            return request;
        }

        /// <summary>
        /// Create a blank sample request for creating an inventory.
        /// </summary>
        /// <param name="verb">The desired action to create the request for</param>
        /// <returns>a blank request where recommended fields for input by the user are initialized with an empty string.</returns>
        private PosterRequest CreateTemplateInventory(PostVerb verb)
        {
            PosterRequest request = null;

            switch (verb)
            {
                case PostVerb.ADJUST:
                    request = new PosterRequest()
                    {
                        Body = new PosterBody()
                        {
                            FindPartCode = "",
                            PartCode = "",
                            Name = "",
                            NameAlt = "",
                            IsService = false,
                            IsActivity = false,
                            PriceRegular = "",
                            PricePreferred = "",
                            AccountAsset = "",
                            AccountExpense = "",
                            AccountRevenue = "",
                            AccountVariance = ""
                        }
                    };
                    break;

                case PostVerb.CREATE:
                    request = new PosterRequest()
                    {
                        Body = new PosterBody()
                        {
                            PartCode = "",
                            Name = "",
                            NameAlt = "",
                            IsService = false,
                            IsActivity = false,
                            StockingUnit = "",
                            StockingUnitAlt = "",
                            PriceRegular = "",
                            PricePreferred = "",
                            AccountAsset = "",
                            AccountExpense = "",
                            AccountRevenue = "",
                            AccountVariance = ""
                        }
                    };
                    break;

                case PostVerb.LOOKUP:
                    request = new PosterRequest()
                    {
                        Body = new PosterBody()
                        {
                            FindPartCode = ""
                        }
                    };
                    break;
            }

            return request;
        }

        /// <summary>
        /// Create a blank sample request for creating a general journal.
        /// </summary>
        /// <param name="verb">The desired action to create the request for</param>
        /// <returns>a blank request where recommended fields for input by the user are initialized with an empty string.</returns>
        private PosterRequest CreateTemplateGeneralJournal(PostVerb verb)
        {
            PosterRequest request = null;

            switch (verb)
            {
                case PostVerb.ADJUST:
                case PostVerb.CREATE:
                    request = new PosterRequest()
                    {
                        Body = new PosterBody()
                        {
                            JournalDate = "",
                            Source = "",
                            Comment = "",
                            DetailLines = new List<PosterDetailLine>()
                            {
                                new PosterDetailLine()
                                {
                                    LedgerAccount = "",
                                    DebitAmount = "",
                                    CreditAmount = "",
                                    Comment = ""
                                },
                                new PosterDetailLine()
                                {
                                    LedgerAccount = "",
                                    DebitAmount = "",
                                    CreditAmount = "",
                                    Comment = ""
                                }
                            }
                        }
                    };

                    if (verb == PostVerb.ADJUST)
                        request.Body.FindJournalId = "";

                    break;

                case PostVerb.LOOKUP:
                    request = new PosterRequest()
                    {
                        Body = new PosterBody()
                        {
                            FindJournalId = "",
                            FindJournalLastYear = false
                        }
                    };
                    break;
            }

            return request;
        }

        /// <summary>
        /// Create a blank sample request for creating a project.
        /// </summary>
        /// <param name="verb">The desired action to create the request for</param>
        /// <returns>a blank request where recommended fields for input by the user are initialized with an empty string.</returns>
        private PosterRequest CreateTemplateProject(PostVerb verb)
        {
            PosterRequest request = null;

            switch (verb)
            {
                case PostVerb.ADJUST:
                case PostVerb.CREATE:
                    request = new PosterRequest()
                    {
                        Body = new PosterBody()
                        {
                            Name = "",
                            NameAlt = "",
                            StartDate = ""
                        }
                    };

                    if (verb == PostVerb.ADJUST)
                        request.Body.FindName = "";

                    break;

                case PostVerb.LOOKUP:
                    request = new PosterRequest()
                    {
                        Body = new PosterBody()
                        {
                            FindName = ""
                        }
                    };
                    break;
            }

            return request;
        }


        /// <summary>
        /// Create a blank sample request for creating a sales or purchase invoice.
        /// </summary>
        /// <param name="verb">The desired action to create the request for</param>
        /// <returns>a blank request where recommended fields for input by the user are initialized with an empty string.</returns>
        private PosterRequest CreateTemplateSalesPurchase(PostVerb verb)
        {
            PosterRequest request = null;

            switch (verb)
            {
                case PostVerb.ADJUST:
                case PostVerb.CREATE:
                    request = new PosterRequest()
                    {
                        Body = new PosterBody()
                        {
                            CusVenName = "",
                            InvoiceDate = "",
                            InvoiceNumber = "",
                            PaidByType = "",
                            ChequeNumber = "",
                            DetailLines = new List<PosterDetailLine>() {
                                new PosterDetailLine() {
                                    ItemDescription = "",
                                    ItemNumber = "",
                                    LedgerAccount = "",
                                    LineAmount = "",
                                    Price = "",
                                    Quantity = "",
                                    TaxCode = "",
                                    TaxDetailLines = new List<PosterTaxDetailLine>() {
                                        new PosterTaxDetailLine() {
                                            TaxAmount = "",
                                            TaxAuthority = ""
                                        }
                                    }
                                }
                            },
                            TaxDetailLines = new List<PosterTaxDetailLine>()
                            {
                                new PosterTaxDetailLine() {
                                    TaxAmount = "",
                                    TaxAuthority = ""
                                }
                            }
                        }
                    };

                    if (verb == PostVerb.ADJUST)
                    {
                        request.Body.FindCusVenName = "";
                        request.Body.FindInvoiceNumber = "";
                    }
                    break;

                case PostVerb.LOOKUP:
                case PostVerb.VOID:
                    request = new PosterRequest()
                    {
                        Body = new PosterBody()
                        {
                            FindInvoiceNumber = "",
                            FindCusVenName = ""
                        }
                    };
                    break;
            }

            return request;
        }

        /// <summary>
        /// Create a blank sample request for executing a sql query.
        /// </summary>
        /// <returns>a blank request where recommended fields for input by the user are initialized with an empty string.</returns>
        private PosterRequest CreateTemplateSqlQuery()
        {
            return new PosterRequest()
            {
                Body = new PosterBody()
                {
                    SQLNonQuery = ""
                }
            };
        }

        /// <summary>
        /// Create a new vendor record.
        /// </summary>
        /// <param name="request">Details of the vendor to save</param>
        /// <returns>the response of the save to display to user</returns>
        private PosterResponse CreateVendor(PosterBody request)
        {
            try
            {
                VendorLedger venLedger = null;
                try
                {
                    venLedger = SDKInstanceManager.Instance.OpenVendorLedger();
                    venLedger.InitializeNew();
                }
                catch
                {
                    PosterResponse response = new PosterResponse();
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }

                if (!string.IsNullOrWhiteSpace(request.TaxID))
                    venLedger.TaxId = request.TaxID;
                return PostCustomerVendor(request, venLedger);
            }
            finally
            {
                SDKInstanceManager.Instance.CloseVendorLedger();
            }
        }

        /// <summary>
        /// Lookup a chart of account record.
        /// </summary>
        /// <param name="request">Details of the chart of account to lookup</param>
        /// <returns>the response of the lookup to display to user</returns>
        private PosterResponse LookupAccount(PosterBody request)
        {
            try
            {
                PosterResponse response = new PosterResponse();
                AccountLedger acctLedger = null;
                try
                {
                    acctLedger = SDKInstanceManager.Instance.OpenAccountLedger();
                    if (acctLedger == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }

                try
                {
                    int acctNum;
                    if (int.TryParse(request.FindAccountNumber, out acctNum) && acctLedger.LoadByAccountNumber(acctNum))
                    {
                        response.Name = acctLedger.Name;
                        response.NameAlt = acctLedger.NameAlt;
                        response.AccountNumber = acctLedger.Number.ToString();
                        response.AccountType = acctLedger.Type;
                        response.AccountClass = acctLedger.Class;
                    }
                    else
                        response.Messages.Add($"Lookup account {request.FindName} FAILED");
                }
                catch (Exception ex)
                {
                    response.Messages.Add($"Lookup FAILED: {ex.Message}");
                }
                finally
                {
                    //response.Messages
                }
                return response;
            }
            finally
            {
                SDKInstanceManager.Instance.CloseAccountLedger();
            }
        }

        /// <summary>
        /// Lookup a customer record.
        /// </summary>
        /// <param name="request">Details of the customer to lookup</param>
        /// <returns>the response of the lookup to display to user</returns>
        private PosterResponse LookupCustomer(PosterBody request)
        {
            try
            {
                PosterResponse response = new PosterResponse();
                CustomerLedger custLedger = null;
                try
                {
                    custLedger = SDKInstanceManager.Instance.OpenCustomerLedger();
                    if (custLedger == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }

                try
                {
                    if (custLedger.LoadByName(request.FindName))
                        response = LookupCustomerVendor(request, custLedger);
                    else
                        response.Messages.Add($"Lookup customer {request.FindName} FAILED");
                }
                catch (Exception ex)
                {
                    response.Messages.Add($"Lookup FAILED: {ex.Message}");
                }
                finally
                {
                    //response.Messages
                }
                return response;
            }
            finally
            {
                SDKInstanceManager.Instance.CloseCustomerLedger();
            }
        }

        /// <summary>
        /// Helper method to load customer/vendor details from ledger into object displayable to user.
        /// </summary>
        /// <param name="request">Details of the customer/vendor to lookup</param>
        /// <param name="ledger">The opened ledger that will perform the lookup</param>
        /// <returns>the response of the lookup to display to user</returns>
        private PosterResponse LookupCustomerVendor(PosterBody request, APARLedgerBase ledger)
        {
            PosterResponse response = new PosterResponse();

            try
            {
                response.Name = ledger.Name;
                response.Contact = ledger.Contact;
                response.Street1 = ledger.Street1;
                response.Street2 = ledger.Street2;
                response.City = ledger.City;
                response.Province = ledger.Province;
                response.PostalCode = ledger.PostalCode;
                response.Country = ledger.Country;
                response.Phone1 = ledger.Phone1;
                response.Phone2 = ledger.Phone2;
                response.Fax = ledger.Fax;
                response.Email = ledger.Email;
                response.Website = ledger.WebSite;
                response.CurrencyCode = ledger.CurrencyCode;
            }
            catch (Exception ex)
            {
                response.Messages.Add($"Lookup FAILED: {ex.Message}");
            }

            return response;
        }

        /// <summary>
        /// Lookup an employee record.
        /// </summary>
        /// <param name="request">Details of the employee to lookup</param>
        /// <returns>the response of the lookup to display to user</returns>
        private PosterResponse LookupEmployee(PosterBody request)
        {
            try
            {
                PosterResponse response = new PosterResponse();
                EmployeeLedgerBase empLedger = null;
                try
                {
                    empLedger = SDKInstanceManager.Instance.OpenEmployeeLedger();
                    if (empLedger == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }

                try
                {
                    if (empLedger.LoadByName(request.FindName))
                    {
                        response.Name = empLedger.Name;
                        response.Street1 = empLedger.Street1;
                        response.Street2 = empLedger.Street2;
                        response.City = empLedger.City;
                        response.Province = empLedger.Province;
                        response.PostalCode = empLedger.PostalCode;
                        response.Phone1 = empLedger.Phone1;
                        response.Phone2 = empLedger.Phone2;
                        response.SocialInsuranceNumber = empLedger.SINSSN;
                        response.BirthDate = (empLedger.BirthDate.HasValue ? empLedger.BirthDate.Value.ToString("yyyy-MM-dd") : "");
                        response.HireDate = (empLedger.HireDate.HasValue ? empLedger.HireDate.Value.ToString("yyyy-MM-dd") : "");
                        response.TaxTableProvince = empLedger.TaxTable;
                        response.PayPeriods = empLedger.PayPeriods.ToString();
                    }
                    else
                        response.Messages.Add($"Lookup employee {request.FindName} FAILED");
                }
                catch (Exception ex)
                {
                    response.Messages.Add($"Lookup FAILED: {ex.Message}");
                }
                finally
                {
                   //response.Messages
                }
                return response;
            }
            finally
            {
                SDKInstanceManager.Instance.CloseEmployeeLedger();
            }
        }

        /// <summary>
        /// Lookup a general journal.
        /// </summary>
        /// <param name="request">Details of the transaction to lookup</param>
        /// <returns>the response of the lookup to display to user</returns>
        private PosterResponse LookupGeneralJournal(PosterBody request)
        {
            try
            {
                PosterResponse response = new PosterResponse();
                GeneralJournal genJourn = null;
                try
                {
                    genJourn = SDKInstanceManager.Instance.OpenGeneralJournal();
                    if (genJourn == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    response.Messages.Add(ERROR_UNABLE_OPEN_JOURNAL);
                    return response;
                }

                try
                {
                    int id;
                    if (!int.TryParse(request.FindJournalId, out id) || !request.FindJournalLastYear.HasValue || !genJourn.LookupJournalEntry(id, request.FindJournalLastYear.Value))
                    {
                        response.Messages.Add($"Lookup general journal {request.FindJournalId} FAILED");
                        return response;
                    }

                    response.JournalId = id.ToString();
                    response.Source = genJourn.Source;
                    response.Comment = genJourn.Comment;
                    response.JournalDate = genJourn.GetJournalDate();

                    response.DetailLines = new List<PosterDetailLine>();
                    PosterDetailLine line;
                    string acct;
                    for (int lineNum = 1; lineNum <= genJourn.NumberOfJournalLines; lineNum++)
                    {
                        acct = genJourn.GetAccount(lineNum);
                        if (string.IsNullOrWhiteSpace(acct))
                            continue;

                        line = new PosterDetailLine()
                        {
                            LedgerAccount = acct,
                            CreditAmount = genJourn.GetCredit(lineNum).ToString(),
                            DebitAmount = genJourn.GetDebit(lineNum).ToString(),
                            Comment = genJourn.GetComment(lineNum)
                        };
                        response.DetailLines.Add(line);
                    }
                }
                catch (Exception ex)
                {
                    response.Messages.Add($"Lookup FAILED: {ex.Message}");
                }
                finally
                {
                    //response.Messages
                }

                return response;
            }
            finally
            {
                SDKInstanceManager.Instance.CloseGeneralJournal();
            }
        }

        /// <summary>
        /// Lookup an inventory record.
        /// </summary>
        /// <param name="request">Details of the inventory to lookup</param>
        /// <returns>the response of the lookup to display to user</returns>
        private PosterResponse LookupInventory(PosterBody request)
        {
            try
            {
                PosterResponse response = new PosterResponse();
                InventoryLedger inventLedger = null;
                try
                {
                    inventLedger = SDKInstanceManager.Instance.OpenInventoryLedger();
                    if (inventLedger == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }

                try
                {
                    if (inventLedger.LoadByPartCode(request.FindPartCode))
                    {
                        response.PartCode = inventLedger.Number;
                        response.Name = inventLedger.Name;
                        response.NameAlt = inventLedger.NameAlt;
                        response.IsService = inventLedger.IsServiceType;
                        response.IsActivity = inventLedger.IsActivityType;
                        response.StockingUnit = inventLedger.StockingUnit;
                        response.StockingUnitAlt = inventLedger.StockingUnitAlt;
                        response.PriceRegular = inventLedger.RegularPrice.ToString();
                        response.PricePreferred = inventLedger.PreferredPrice.ToString();
                        response.AccountAsset = inventLedger.AssetAccount;
                        response.AccountExpense = inventLedger.ExpenseAccount;
                        response.AccountRevenue = inventLedger.RevenueAccount;
                        response.AccountVariance = inventLedger.VarianceAccount;
                    }
                    else
                        response.Messages.Add($"Lookup inventory {request.FindName} FAILED");
                }
                catch (Exception ex)
                {
                    response.Messages.Add($"Lookup FAILED: {ex.Message}");
                }
                finally
                {
                    //response.Messages
                }
                return response;
            }
            finally
            {
                SDKInstanceManager.Instance.CloseInventoryLedger();
            }
        }

        /// <summary>
        /// Lookup an invoice.
        /// </summary>
        /// <param name="request">Details of the transaction to lookup</param>
        /// <param name="journ">The opened journal that will perform the lookup</param>
        /// <returns>the response of the lookup to display to user</returns>
        private PosterResponse LookupInvoice(PosterBody request, InvoiceJournal journ)
        {
            PosterResponse response = new PosterResponse();
            try
            {
                if (!journ.LoadInvoiceForLookup(request.FindCusVenName, request.FindInvoiceNumber))
                {
                    response.Messages.Add($"Lookup invoice {request.FindInvoiceNumber} for {request.FindCusVenName} FAILED");
                    return response;
                }

                response.InvoiceNumber = journ.InvoiceNumber;
                response.CusVenName = journ.GetAPARLedgerName();
                response.InvoiceDate = journ.GetJournalDate();
                response.PaidByType = journ.GetPaidByType();
                response.ChequeNumber = journ.ChequeNumber;
                response.SubTotal = journ.SubTotalAmount.ToString();
                response.Total = journ.GetTotalAmount().ToString();

                response.DetailLines = new List<PosterDetailLine>();
                TaxSummaryInfo taxSumm;
                PosterDetailLine detailLine;
                for (int lineNum = 1; lineNum <= journ.NumberOfJournalLines; lineNum++)
                {
                    detailLine = new PosterDetailLine()
                    {
                        ItemNumber = journ.GetItemNumber(lineNum),
                        ItemDescription = journ.GetDescription(lineNum),
                        Quantity = journ.GetQuantity(lineNum).ToString(),
                        Price = journ.GetPrice(lineNum).ToString(),
                        LineAmount = journ.GetLineAmount(lineNum).ToString(),
                        LedgerAccount = journ.GetAccount(lineNum),
                        TaxCode = journ.GetTaxCodeString(lineNum),
                        TaxDetailLines = new List<PosterTaxDetailLine>()
                    };
                    response.DetailLines.Add(detailLine);

                    taxSumm = journ.GetLineTaxAmountInfo(lineNum);
                    for (int lineNum1 = 1; lineNum1 <= taxSumm.GetCount(); lineNum1++)
                    {
                        detailLine.TaxDetailLines.Add(new PosterTaxDetailLine()
                        {
                            TaxAmount = taxSumm.GetTaxAmountByRow(lineNum1).ToString(),
                            TaxAuthority = taxSumm.GetTaxNameByRow(lineNum1)
                        });
                    }
                    taxSumm.Cancel();
                }

                response.TaxDetailLines = new List<PosterTaxDetailLine>();
                taxSumm = journ.GetTotalTaxAmountInfo();
                for (int lineNum1 = 1; lineNum1 <= taxSumm.GetCount(); lineNum1++)
                {
                    response.TaxDetailLines.Add(new PosterTaxDetailLine()
                    {
                        TaxAmount = taxSumm.GetTaxAmountByRow(lineNum1).ToString(),
                        TaxAuthority = taxSumm.GetTaxNameByRow(lineNum1)
                    });
                }
                taxSumm.Cancel();
            }
            catch (Exception ex)
            {
                response.Messages.Add($"Lookup FAILED: {ex.Message}");
            }
            finally
            {
                //response.Messages
            }

            return response;
        }

        /// <summary>
        /// Lookup a project record.
        /// </summary>
        /// <param name="request">Details of the project to lookup</param>
        /// <returns>the response of the lookup to display to user</returns>
        private PosterResponse LookupProject(PosterBody request)
        {
            try
            {
                PosterResponse response = new PosterResponse();
                ProjectLedger projLedger = null;
                try
                {
                    projLedger = SDKInstanceManager.Instance.OpenProjectLedger();
                    if (projLedger == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }

                try
                {
                    if (projLedger.LoadByName(request.FindName))
                    {
                        response.Name = projLedger.Name;
                        response.NameAlt = projLedger.NameAlt;
                        response.StartDate = (projLedger.StartDate.HasValue ? projLedger.StartDate.Value.ToString("yyyy-MM-dd") : "");
                    }
                    else
                        response.Messages.Add($"Lookup project {request.FindName} FAILED");
                }
                catch (Exception ex)
                {
                    response.Messages.Add($"Lookup FAILED: {ex.Message}");
                }
                finally
                {
                    //response.Messages
                }
                return response;
            }
            finally
            {
                SDKInstanceManager.Instance.CloseProjectLedger();
            }
        }

        /// <summary>
        /// Lookup a purchase invoice.
        /// </summary>
        /// <param name="request">Details of the transaction to lookup</param>
        /// <returns>the response of the lookup to display to user</returns>
        private PosterResponse LookupPurchaseInvoice(PosterBody request)
        {
            try
            {
                PurchasesJournal purJourn = null;
                try
                {
                    purJourn = SDKInstanceManager.Instance.OpenPurchasesJournal();
                    if (purJourn == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    PosterResponse response = new PosterResponse();
                    response.Messages.Add(ERROR_UNABLE_OPEN_JOURNAL);
                    return response;
                }
                return LookupInvoice(request, purJourn);
            }
            finally
            {
                SDKInstanceManager.Instance.ClosePurchasesJournal();
            }
        }

        /// <summary>
        /// Lookup a sales invoice.
        /// </summary>
        /// <param name="request">Details of the transaction to lookup</param>
        /// <returns>the response of the lookup to display to user</returns>
        private PosterResponse LookupSalesInvoice(PosterBody request)
        {
            try
            {
                SalesJournal salJourn = null;
                try
                {
                    salJourn = SDKInstanceManager.Instance.OpenSalesJournal();
                    if (salJourn == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    PosterResponse response = new PosterResponse();
                    response.Messages.Add(ERROR_UNABLE_OPEN_JOURNAL);
                    return response;
                }
                return LookupInvoice(request, salJourn);
            }
            finally
            {
                SDKInstanceManager.Instance.CloseSalesJournal();
            }
        }

        /// <summary>
        /// Lookup a vendor record.
        /// </summary>
        /// <param name="request">Details of the vendor to lookup</param>
        /// <returns>the response of the lookup to display to user</returns>
        private PosterResponse LookupVendor(PosterBody request)
        {
            try
            {
                PosterResponse response = new PosterResponse();
                VendorLedger venLedger = null;
                try
                {
                    venLedger = SDKInstanceManager.Instance.OpenVendorLedger();
                    if (venLedger == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    response.Messages.Add(ERROR_UNABLE_OPEN_LEDGER);
                    return response;
                }

                try
                {
                    if (venLedger.LoadByName(request.FindName))
                    {
                        response = LookupCustomerVendor(request, venLedger);
                        response.TaxID = venLedger.TaxId;
                    }
                    else
                        response.Messages.Add($"Lookup vendor {request.FindName} FAILED");
                }
                catch (Exception ex)
                {
                    response.Messages.Add($"Lookup FAILED: {ex.Message}");
                }
                finally
                {
                    //response.Messages
                }
                return response;
            }
            finally
            {
                SDKInstanceManager.Instance.CloseVendorLedger();
            }
        }

        /// <summary>
        /// Post a chart of account record.
        /// </summary>
        /// <param name="request">Details of the chart of account to post</param>
        /// <param name="ledger">The opened chart of account ledger module that will do the saving</param>
        /// <returns>the response of the post to display to user</returns>
        private PosterResponse PostAccount(PosterBody request, AccountLedger ledger)
        {
            PosterResponse response = new PosterResponse();
             
            try
            {
                int intVal;

                if (!string.IsNullOrWhiteSpace(request.Name))
                    ledger.Name = request.Name;
                if (!string.IsNullOrWhiteSpace(request.NameAlt))
                    ledger.NameAlt = request.NameAlt;
                if (!string.IsNullOrWhiteSpace(request.AccountNumber) && int.TryParse(request.AccountNumber, out intVal))
                    ledger.Number = intVal;
                if (!string.IsNullOrWhiteSpace(request.AccountType))
                    ledger.Type = request.AccountType;
                if (!string.IsNullOrWhiteSpace(request.AccountClass))
                    ledger.Class = request.AccountClass;

                response.PostOK = ledger.Save();
            }
            catch (Exception ex)
            {
                response.Messages.Add($"Save FAILED: {ex.Message}");
            }
            finally
            {
                //response.Messages
                 
            }

            return response;
        }

        /// <summary>
        /// Post a customer or vendor ledger record.
        /// </summary>
        /// <param name="request">Details of the transaction to post</param>
        /// <param name="ledger">The opened customer ledger module that will do the posting</param>
        /// <returns>the response of the post to display to user</returns>
        private PosterResponse PostCustomerVendor(PosterBody request, APARLedgerBase ledger)
        {
            PosterResponse response = new PosterResponse();
             
            try
            {
                if (!string.IsNullOrWhiteSpace(request.Name))
                    ledger.Name = request.Name;
                if (!string.IsNullOrWhiteSpace(request.Contact))
                    ledger.Contact = request.Contact;
                if (!string.IsNullOrWhiteSpace(request.Street1))
                    ledger.Street1 = request.Street1;
                if (!string.IsNullOrWhiteSpace(request.Street2))
                    ledger.Street2 = request.Street2;
                if (!string.IsNullOrWhiteSpace(request.City))
                    ledger.City = request.City;
                if (!string.IsNullOrWhiteSpace(request.Province))
                    ledger.Province = request.Province;
                if (!string.IsNullOrWhiteSpace(request.PostalCode))
                    ledger.PostalCode = request.PostalCode;
                if (!string.IsNullOrWhiteSpace(request.Country))
                    ledger.Country = request.Country;
                if (!string.IsNullOrWhiteSpace(request.Phone1))
                    ledger.Phone1 = request.Phone1;
                if (!string.IsNullOrWhiteSpace(request.Phone2))
                    ledger.Phone2 = request.Phone2;
                if (!string.IsNullOrWhiteSpace(request.Fax))
                    ledger.Fax = request.Fax;
                if (!string.IsNullOrWhiteSpace(request.Email))
                    ledger.Email = request.Email;
                if (!string.IsNullOrWhiteSpace(request.Website))
                    ledger.WebSite = request.Website;
                if (!string.IsNullOrWhiteSpace(request.CurrencyCode))
                    ledger.CurrencyCode = request.CurrencyCode;

                response.PostOK = ledger.Save();
            }
            catch (Exception ex)
            {
                response.Messages.Add($"Save FAILED: {ex.Message}");
            }
            finally
            {
                //response.Messages
                 
            }

            return response;
        }

        /// <summary>
        /// Post an employee ledger record.
        /// </summary>
        /// <param name="request">Details of the employee to post</param>
        /// <param name="ledger">The opened employee ledger module that will do the saving</param>
        /// <returns>the response of the post to display to user</returns>
        private PosterResponse PostEmployee(PosterBody request, EmployeeLedgerBase ledger)
        {
            PosterResponse response = new PosterResponse();
             
            try
            {
                short shVal;
                DateTime dtVal;

                if (!string.IsNullOrWhiteSpace(request.Name))
                    ledger.Name = request.Name;
                if (!string.IsNullOrWhiteSpace(request.Street1))
                    ledger.Street1 = request.Street1;
                if (!string.IsNullOrWhiteSpace(request.Street2))
                    ledger.Street2 = request.Street2;
                if (!string.IsNullOrWhiteSpace(request.City))
                    ledger.City = request.City;
                if (!string.IsNullOrWhiteSpace(request.Province))
                    ledger.Province = request.Province;
                if (!string.IsNullOrWhiteSpace(request.PostalCode))
                    ledger.PostalCode = request.PostalCode;
                if (!string.IsNullOrWhiteSpace(request.Phone1))
                    ledger.Phone1 = request.Phone1;
                if (!string.IsNullOrWhiteSpace(request.Phone2))
                    ledger.Phone2 = request.Phone2;
                if (!string.IsNullOrWhiteSpace(request.SocialInsuranceNumber))
                    ledger.SINSSN = request.SocialInsuranceNumber;
                if (!string.IsNullOrWhiteSpace(request.BirthDate) && DateTime.TryParse(request.BirthDate, out dtVal))
                    ledger.BirthDate = dtVal;
                if (!string.IsNullOrWhiteSpace(request.HireDate) && DateTime.TryParse(request.HireDate, out dtVal))
                    ledger.HireDate = dtVal;
                if (!string.IsNullOrWhiteSpace(request.TaxTableProvince))
                    ledger.TaxTable = request.TaxTableProvince;
                if (!string.IsNullOrWhiteSpace(request.PayPeriods) && short.TryParse(request.PayPeriods, out shVal))
                    ledger.PayPeriods = shVal;

                response.PostOK = ledger.Save();
            }
            catch (Exception ex)
            {
                response.Messages.Add($"Save FAILED: {ex.Message}");
            }
            finally
            {
                //response.Messages
                 
            }
            return response;
        }

        /// <summary>
        /// Post a general journal transaction.
        /// </summary>
        /// <param name="request">Details of the transaction to post</param>
        /// <param name="journ">The opened general journal module that will do the posting</param>
        /// <returns>the response of the post to display to user</returns>
        private PosterResponse PostGeneralJournal(PosterBody request, GeneralJournal journ)
        {
            PosterResponse response = new PosterResponse();
             
            try
            {
                if (request.DetailLines != null && journ.IsInAdjustMode)
                {
                    int numLines = journ.NumberOfJournalLines - 1;
                    while (numLines > 0)
                    {
                        journ.RemoveJournalLine(1);
                        numLines--;
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.Source))
                    journ.Source = request.Source;
                if (!string.IsNullOrWhiteSpace(request.Comment))
                    journ.Comment = request.Comment;
                if (!string.IsNullOrWhiteSpace(request.JournalDate))
                    journ.SetJournalDate(request.JournalDate);

                if (request.DetailLines != null)
                {
                    int lineNum = 1;
                    int intVal;
                    double dblVal;
                    foreach (PosterDetailLine line in request.DetailLines)
                    {
                        if (!string.IsNullOrWhiteSpace(line.LedgerAccount) && int.TryParse(line.LedgerAccount, out intVal))
                            journ.SetAccount(Utility.MakeAccountNumber(intVal).ToString(), lineNum);
                        if (!string.IsNullOrWhiteSpace(line.DebitAmount) && double.TryParse(line.DebitAmount, out dblVal))
                            journ.SetDebit(dblVal, lineNum);
                        if (!string.IsNullOrWhiteSpace(line.CreditAmount) && double.TryParse(line.CreditAmount, out dblVal))
                            journ.SetCredit(dblVal, lineNum);
                        if (!string.IsNullOrWhiteSpace(line.Comment))
                            journ.SetComment(line.Comment, lineNum);
                        lineNum++;
                    }
                }

                response.PostOK = journ.Post();
                if (response.PostOK.Value)
                    response.JournalId = Math.Max(journ.GetLastJournalNumber(), journ.GetLastJournalNumber(true)).ToString();
            }
            catch (Exception ex)
            {
                response.Messages.Add($"Post FAILED: {ex.Message}");
            }
            finally
            {
                //response.Messages
                 
            }

            return response;

        }

        /// <summary>
        /// Post an inventory ledger record.
        /// </summary>
        /// <param name="request">Details of the transaction to post</param>
        /// <param name="ledger">The opened inventory ledger module that will do the posting</param>
        /// <returns>the response of the post to display to user</returns>
        private PosterResponse PostInventory(PosterBody request, InventoryLedger ledger)
        {
            PosterResponse response = new PosterResponse();
             
            try
            {
                double dblVal;
                int intVal;

                if (!string.IsNullOrWhiteSpace(request.PartCode))
                    ledger.Number = request.PartCode;
                if (!string.IsNullOrWhiteSpace(request.Name))
                    ledger.Name = request.Name;
                if (!string.IsNullOrWhiteSpace(request.NameAlt))
                    ledger.NameAlt = request.NameAlt;
                if (request.IsService.HasValue)
                    ledger.IsServiceType = request.IsService.Value;
                if (request.IsActivity.HasValue)
                    ledger.IsActivityType = request.IsActivity.Value;
                if (!string.IsNullOrWhiteSpace(request.StockingUnit))
                    ledger.StockingUnit = request.StockingUnit;
                if (!string.IsNullOrWhiteSpace(request.StockingUnitAlt))
                    ledger.StockingUnitAlt = request.StockingUnitAlt;
                if (!string.IsNullOrWhiteSpace(request.PriceRegular) && double.TryParse(request.PriceRegular, out dblVal))
                    ledger.RegularPrice = dblVal;
                if (!string.IsNullOrWhiteSpace(request.PricePreferred) && double.TryParse(request.PricePreferred, out dblVal))
                    ledger.PreferredPrice = dblVal;
                if (!string.IsNullOrWhiteSpace(request.AccountAsset) && int.TryParse(request.AccountAsset, out intVal))
                    ledger.AssetAccount = Utility.MakeAccountNumber(intVal).ToString();
                if (!string.IsNullOrWhiteSpace(request.AccountExpense) && int.TryParse(request.AccountExpense, out intVal))
                    ledger.ExpenseAccount = Utility.MakeAccountNumber(intVal).ToString();
                if (!string.IsNullOrWhiteSpace(request.AccountRevenue) && int.TryParse(request.AccountRevenue, out intVal))
                    ledger.RevenueAccount = Utility.MakeAccountNumber(intVal).ToString();
                if (!string.IsNullOrWhiteSpace(request.AccountVariance) && int.TryParse(request.AccountVariance, out intVal))
                    ledger.VarianceAccount = Utility.MakeAccountNumber(intVal).ToString();

                response.PostOK = ledger.Save();
            }
            catch (Exception ex)
            {
                response.Messages.Add($"Save FAILED: {ex.Message}");
            }
            finally
            {
                //response.Messages
                 
            }
            return response;
        }

        /// <summary>
        /// Post an invoice transaction.
        /// </summary>
        /// <param name="request">Details of the transaction to post</param>
        /// <param name="journ">The opened invoice journal module that will do the posting</param>
        /// <returns>the response of the post to display to user</returns>
        private PosterResponse PostInvoice(PosterBody request, InvoiceJournal journ)
        {
            PosterResponse response = new PosterResponse();
            try
            {
                bool okToPost = true;

                if (request.DetailLines != null && journ.IsInAdjustMode)
                {
                    int numLines = journ.NumberOfJournalLines - 1;
                    while (okToPost && numLines > 0)
                    {
                        okToPost = journ.RemoveJournalLine(1);
                        numLines--;
                    }
                }
                if (!okToPost)
                    response.Messages.Add("Post FAILED: Cannot remove existing line items");
                else
                {
                    TaxSummaryInfo taxSumm;

                    if (!string.IsNullOrWhiteSpace(request.InvoiceNumber))
                        journ.InvoiceNumber = request.InvoiceNumber;
                    if (!string.IsNullOrWhiteSpace(request.CusVenName))
                        journ.SelectAPARLedger(request.CusVenName);
                    if (!string.IsNullOrWhiteSpace(request.InvoiceDate))
                        journ.SetJournalDate(request.InvoiceDate);
                    if (!string.IsNullOrWhiteSpace(request.PaidByType))
                        journ.SelectPaidByType(request.PaidByType);
                    if (!string.IsNullOrWhiteSpace(request.ChequeNumber))
                        journ.ChequeNumber = request.ChequeNumber;

                    if (request.DetailLines != null)
                    {
                        int lineNum = 1;
                        int intVal;
                        double dblVal;
                        foreach (PosterDetailLine line in request.DetailLines)
                        {
                            if (!string.IsNullOrWhiteSpace(line.ItemNumber))
                                journ.SetItemNumber(line.ItemNumber, lineNum);
                            if (!string.IsNullOrWhiteSpace(line.ItemDescription))
                                journ.SetDescription(line.ItemDescription, lineNum);
                            if (!string.IsNullOrWhiteSpace(line.Quantity) && double.TryParse(line.Quantity, out dblVal))
                                journ.SetQuantity(dblVal, lineNum);
                            if (!string.IsNullOrWhiteSpace(line.Price) && double.TryParse(line.Price, out dblVal))
                                journ.SetPrice(dblVal, lineNum);
                            if (!string.IsNullOrWhiteSpace(line.LineAmount) && double.TryParse(line.LineAmount, out dblVal))
                                journ.SetLineAmount(dblVal, lineNum);
                            if (!string.IsNullOrWhiteSpace(line.LedgerAccount) && int.TryParse(line.LedgerAccount, out intVal))
                                journ.SetLineAccount(Utility.MakeAccountNumber(intVal).ToString(), lineNum);
                            if (!string.IsNullOrWhiteSpace(line.TaxCode))
                                journ.SetTaxCodeString(line.TaxCode, lineNum);

                            taxSumm = journ.GetLineTaxAmountInfo(lineNum);
                            foreach (PosterTaxDetailLine taxLine in line.TaxDetailLines)
                            {
                                if (!string.IsNullOrWhiteSpace(taxLine.TaxAuthority) && double.TryParse(taxLine.TaxAmount, out dblVal))
                                    taxSumm.SetTaxAmountByName(taxLine.TaxAuthority, dblVal);
                            }
                            taxSumm.Save();

                            lineNum++;
                        }

                        taxSumm = journ.GetTotalTaxAmountInfo();
                        foreach (PosterTaxDetailLine taxLine in request.TaxDetailLines)
                        {
                            if (!string.IsNullOrWhiteSpace(taxLine.TaxAuthority) && double.TryParse(taxLine.TaxAmount, out dblVal))
                                taxSumm.SetTaxAmountByName(taxLine.TaxAuthority, dblVal);
                        }
                        taxSumm.Save();
                    }

                    string invNum = journ.InvoiceNumber;
                    string cusVenName = journ.GetAPARLedgerName();
                    response.PostOK = journ.Post();
                    if (response.PostOK.Value)
                    {
                        response.CusVenName = cusVenName;
                        response.InvoiceNumber = invNum;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Messages.Add($"Post FAILED: {ex.Message}");
            }
            return response;
        }

        /// <summary>
        /// Post a project ledger record.
        /// </summary>
        /// <param name="request">Details of the project to post</param>
        /// <param name="ledger">The opened project ledger module that will do the saving</param>
        /// <returns>the response of the post to display to user</returns>
        private PosterResponse PostProject(PosterBody request, ProjectLedger ledger)
        {
            PosterResponse response = new PosterResponse();
             
            try
            {
                DateTime dtVal;

                if (!string.IsNullOrWhiteSpace(request.Name))
                    ledger.Name = request.Name;
                if (!string.IsNullOrWhiteSpace(request.NameAlt))
                    ledger.NameAlt = request.NameAlt;
                if (!string.IsNullOrWhiteSpace(request.StartDate) && DateTime.TryParse(request.StartDate, out dtVal))
                    ledger.StartDate = dtVal;

                response.PostOK = ledger.Save();
            }
            catch (Exception ex)
            {
                response.Messages.Add($"Save FAILED: {ex.Message}");
            }
            finally
            {
                //response.Messages
                 
            }
            return response;
        }

        /// <summary>
        /// Run a custom sql query.
        /// </summary>
        /// <param name="request">Details of the sql query to run</param>
        /// <returns>the response of the query to display to user</returns>
        private PosterResponse RunNonQuery(PosterBody request)
        {
            PosterResponse response = new PosterResponse();
             
            try
            {
                response.RowsAffected = ExecuteTransaction(request.SQLNonQuery);
            }
            catch (Exception ex)
            {
                response.Messages.Add($"Query FAILED: {ex.Message}");
            }
            finally
            {
                //response.Messages
                 
            }
            return response;

        }

        /// <summary>
        /// Executes a given query (INSERT, UPDATE or DELETE) in a transaction. A transaction is required for any database changes to be uploaded to Remote Data Access.
        /// </summary>
        /// <param name="query">The query to execute</param>
        /// <returns>the number of rows affected by the query</returns>
        private int ExecuteTransaction(string query)
        {
            int rc;
            bool ok = false;
            SDKDatabaseUtility dbUtil = new SDKDatabaseUtility();
            dbUtil.BeginTransaction();
            try
            {
                rc = dbUtil.RunNonQuery(query);
                dbUtil.Commit();
                ok = true;
            }
            finally
            {
                if (!ok)
                    dbUtil.Rollback();
            }
            return rc;
        }

        /// <summary>
        /// Voids an invoice transaction.
        /// </summary>
        /// <param name="request">Details of the transaction to void</param>
        /// <param name="journ">The opened invoice journal module that will do the voiding</param>
        /// <returns>the response of the void to display to user</returns>
        private PosterResponse VoidInvoice(PosterBody request, InvoiceJournal journ)
        {
            PosterResponse response = new PosterResponse();
            try
            {
                if (!journ.LoadInvoiceForLookup(request.FindCusVenName, request.FindInvoiceNumber))
                {
                    response.Messages.Add($"Void invoice {request.FindInvoiceNumber} for {request.FindCusVenName} FAILED");
                    return response;
                }
                response.PostOK = journ.ReverseInvoice();
            }
            catch (Exception ex)
            {
                response.Messages.Add($"Void FAILED: {ex.Message}");
            }
            finally
            {
                //response.Messages
            }

            return response;
        }

        /// <summary>
        /// Voids a purchase invoice transaction.
        /// </summary>
        /// <param name="request">Details of the transaction to void</param>
        /// <returns>the response of the void to display to user</returns>
        private PosterResponse VoidPurchaseInvoice(PosterBody request)
        {
            try
            {
                PurchasesJournal purJourn = null;
                try
                {
                    purJourn = SDKInstanceManager.Instance.OpenPurchasesJournal();
                    if (purJourn == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    PosterResponse response = new PosterResponse();
                    response.Messages.Add(ERROR_UNABLE_OPEN_JOURNAL);
                    return response;
                }
                return VoidInvoice(request, purJourn);
            }
            finally
            {
                SDKInstanceManager.Instance.ClosePurchasesJournal();
            }
        }

        /// <summary>
        /// Voids a sales invoice transaction.
        /// </summary>
        /// <param name="request">Details of the transaction to void</param>
        /// <returns>the response of the void to display to user</returns>
        private PosterResponse VoidSalesInvoice(PosterBody request)
        {
            try
            {
                SalesJournal salJourn = null;
                try
                {
                    salJourn = SDKInstanceManager.Instance.OpenSalesJournal();
                    if (salJourn == null)
                        throw new SimplyNoAccessException();
                }
                catch
                {
                    PosterResponse response = new PosterResponse();
                    response.Messages.Add(ERROR_UNABLE_OPEN_JOURNAL);
                    return response;
                }
                return VoidInvoice(request, salJourn);
            }
            finally
            {
                SDKInstanceManager.Instance.CloseSalesJournal();
            }
        }



        private void CloaseDatabase()
        {
            try
            {
                SDKInstanceManager.Instance.CloseDatabase();
            }
            catch (Exception ex)
            {

            }
        }


        private IHttpActionResult RunAsyncTask(string username, string password, bool multiuser, PosterRequestRoot reqRoot)
        {

            string filePath = ConfigurationManager.AppSettings["DatabaseFilePath"];

            //SDKInstanceManager.SDKResult sdkResult;
            //if (SDKInstanceManager.Instance.OpenDatabase(filePath, username, password, multiuser, "Sage 50 SDK Web API", "SASWA", 1, out sdkResult))
            if (SDKInstanceManager.Instance.OpenDatabase(filePath, username, password, multiuser, "Sage 50 SDK Web API", "SASWA", 1))
            {

                Utility.InitializeAfterDatabaseOpen();

                PosterResponse response;
                PosterResponseRoot responses = new PosterResponseRoot() { Responses = new List<PosterResponse>() };
                DateTime startTime = DateTime.Now;

                foreach (PosterRequest request in reqRoot.Requests)
                {

                    if (request.Action == null || request.Body == null)
                        continue;

                    response = null;

                    if (request.Action == GetActionName(PostVerb.CREATE, PostModule.SALES_INVOICE))
                        response = CreateSalesInvoice(request.Body);
                    else if (request.Action == GetActionName(PostVerb.LOOKUP, PostModule.SALES_INVOICE))
                        response = LookupSalesInvoice(request.Body);
                    else if (request.Action == GetActionName(PostVerb.ADJUST, PostModule.SALES_INVOICE))
                        response = AdjustSalesInvoice(request.Body);
                    else if (request.Action == GetActionName(PostVerb.VOID, PostModule.SALES_INVOICE))
                        response = VoidSalesInvoice(request.Body);

                    else if (request.Action == GetActionName(PostVerb.CREATE, PostModule.PURCHASE_INVOICE))
                        response = CreatePurchaseInvoice(request.Body);
                    else if (request.Action == GetActionName(PostVerb.LOOKUP, PostModule.PURCHASE_INVOICE))
                        response = LookupPurchaseInvoice(request.Body);
                    else if (request.Action == GetActionName(PostVerb.ADJUST, PostModule.PURCHASE_INVOICE))
                        response = AdjustPurchaseInvoice(request.Body);
                    else if (request.Action == GetActionName(PostVerb.VOID, PostModule.PURCHASE_INVOICE))
                        response = VoidPurchaseInvoice(request.Body);

                    else if (request.Action == GetActionName(PostVerb.CREATE, PostModule.GENERAL_JOURNAL))
                        response = CreateGeneralJournal(request.Body);
                    else if (request.Action == GetActionName(PostVerb.LOOKUP, PostModule.GENERAL_JOURNAL))
                        response = LookupGeneralJournal(request.Body);
                    else if (request.Action == GetActionName(PostVerb.ADJUST, PostModule.GENERAL_JOURNAL))
                        response = AdjustGeneralJournal(request.Body);

                    else if (request.Action == GetActionName(PostVerb.CREATE, PostModule.CUSTOMER_LEDGER))
                        response = CreateCustomer(request.Body);
                    else if (request.Action == GetActionName(PostVerb.ADJUST, PostModule.CUSTOMER_LEDGER))
                        response = AdjustCustomer(request.Body);
                    else if (request.Action == GetActionName(PostVerb.LOOKUP, PostModule.CUSTOMER_LEDGER))
                        response = LookupCustomer(request.Body);

                    else if (request.Action == GetActionName(PostVerb.CREATE, PostModule.VENDOR_LEDGER))
                        response = CreateVendor(request.Body);
                    else if (request.Action == GetActionName(PostVerb.ADJUST, PostModule.VENDOR_LEDGER))
                        response = AdjustVendor(request.Body);
                    else if (request.Action == GetActionName(PostVerb.LOOKUP, PostModule.VENDOR_LEDGER))
                        response = LookupVendor(request.Body);

                    else if (request.Action == GetActionName(PostVerb.CREATE, PostModule.INVENTORY_LEDGER))
                        response = CreateInventory(request.Body);
                    else if (request.Action == GetActionName(PostVerb.ADJUST, PostModule.INVENTORY_LEDGER))
                        response = AdjustInventory(request.Body);
                    else if (request.Action == GetActionName(PostVerb.LOOKUP, PostModule.INVENTORY_LEDGER))
                        response = LookupInventory(request.Body);

                    else if (request.Action == GetActionName(PostVerb.CREATE, PostModule.EMPLOYEE_LEDGER))
                        response = CreateEmployee(request.Body);
                    else if (request.Action == GetActionName(PostVerb.ADJUST, PostModule.EMPLOYEE_LEDGER))
                        response = AdjustEmployee(request.Body);
                    else if (request.Action == GetActionName(PostVerb.LOOKUP, PostModule.EMPLOYEE_LEDGER))
                        response = LookupEmployee(request.Body);

                    else if (request.Action == GetActionName(PostVerb.CREATE, PostModule.PROJECT_LEDGER))
                        response = CreateProject(request.Body);
                    else if (request.Action == GetActionName(PostVerb.ADJUST, PostModule.PROJECT_LEDGER))
                        response = AdjustProject(request.Body);
                    else if (request.Action == GetActionName(PostVerb.LOOKUP, PostModule.PROJECT_LEDGER))
                        response = LookupProject(request.Body);

                    else if (request.Action == GetActionName(PostVerb.CREATE, PostModule.ACCOUNT_LEDGER))
                        response = CreateAccount(request.Body);
                    else if (request.Action == GetActionName(PostVerb.ADJUST, PostModule.ACCOUNT_LEDGER))
                        response = AdjustAccount(request.Body);
                    else if (request.Action == GetActionName(PostVerb.LOOKUP, PostModule.ACCOUNT_LEDGER))
                        response = LookupAccount(request.Body);

                    else if (request.Action == GetActionName(PostVerb.CREATE, PostModule.SQL_NONQUERY))
                        response = RunNonQuery(request.Body);

                    if (response != null)
                    {
                        if (response.Messages.Count == 0)
                            response.Messages = null;
                        responses.Responses.Add(response);
                    }
                }

                int totalDuration = (int)DateTime.Now.Subtract(startTime).TotalSeconds;
                int mins = (int)(totalDuration / 60.0);
                int secs = totalDuration - (mins * 60);
                responses.JobDuration = $"{mins}m {secs}s";

                responses.TotalRequests = responses.Responses.Count;

                CloaseDatabase();

                return Ok(responses);
            }
            else
            {
                return BadRequest("Failed to connect");
            }
        }


        /// <summary>
        /// POST: api/PosterRequest
        /// </summary>
        [ResponseType(typeof(PosterRequestRoot))]
        public async Task<IHttpActionResult> PostCustomerAsync(string username, string password, bool multiuser, PosterRequestRoot reqRoot)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                CloaseDatabase();
                return await Task.Run(() => RunAsyncTask(username, password, multiuser, reqRoot));
                //return RunAsyncTask(username, password, multiuser, reqRoot);
            }
            catch (Exception ex)
            {
                CloaseDatabase();
                return BadRequest(ex.Message);
            }
        }

    }
}
