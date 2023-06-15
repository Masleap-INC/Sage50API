using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sage50API
{
    public class PosterRequestRoot
    {
        [JsonProperty("requests")]
        public List<PosterRequest> Requests { get; set; }
    }

    public class PosterRequest
    {
        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("body")]
        public PosterBody Body { get; set; }
    }

    public class PosterBody
    {
        [JsonProperty("find_name")]
        public string FindName { get; set; }

        [JsonProperty("find_cusven_name")]
        public string FindCusVenName { get; set; }

        [JsonProperty("find_invoice_number")]
        public string FindInvoiceNumber { get; set; }

        [JsonProperty("find_journal_id")]
        public string FindJournalId { get; set; }

        [JsonProperty("find_journal_is_last_year")]
        public bool? FindJournalLastYear { get; set; }

        [JsonProperty("find_part_code")]
        public string FindPartCode { get; set; }

        [JsonProperty("find_account_number")]
        public string FindAccountNumber { get; set; }

        [JsonProperty("cusven_name")]
        public string CusVenName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("invoice_number")]
        public string InvoiceNumber { get; set; }

        [JsonProperty("invoice_date")]
        public string InvoiceDate { get; set; }

        [JsonProperty("paid_by_type")]
        public string PaidByType { get; set; }

        [JsonProperty("cheque_number")]
        public string ChequeNumber { get; set; }

        [JsonProperty("journal_date")]
        public string JournalDate { get; set; }

        [JsonProperty("journal_id")]
        public string JournalId { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("detail_lines")]
        public List<PosterDetailLine> DetailLines { get; set; }

        [JsonProperty("tax_lines")]
        public List<PosterTaxDetailLine> TaxDetailLines { get; set; }

        [JsonProperty("sub_total")]
        public string SubTotal { get; set; }

        [JsonProperty("total")]
        public string Total { get; set; }

        [JsonProperty("contact_name")]
        public string Contact { get; set; }

        [JsonProperty("street1")]
        public string Street1 { get; set; }

        [JsonProperty("street2")]
        public string Street2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("province")]
        public string Province { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }

        [JsonProperty("phone1")]
        public string Phone1 { get; set; }

        [JsonProperty("phone2")]
        public string Phone2 { get; set; }

        [JsonProperty("fax")]
        public string Fax { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("currency_code")]
        public string CurrencyCode { get; set; }

        [JsonProperty("tax_id")]
        public string TaxID { get; set; }

        [JsonProperty("part_code")]
        public string PartCode { get; set; }

        [JsonProperty("name_alt")]
        public string NameAlt { get; set; }

        [JsonProperty("is_service")]
        public bool? IsService { get; set; }

        [JsonProperty("is_activity")]
        public bool? IsActivity { get; set; }

        [JsonProperty("stocking_unit")]
        public string StockingUnit { get; set; }

        [JsonProperty("stocking_unit_alt")]
        public string StockingUnitAlt { get; set; }

        [JsonProperty("price_regular")]
        public string PriceRegular { get; set; }

        [JsonProperty("price_preferred")]
        public string PricePreferred { get; set; }

        [JsonProperty("account_asset")]
        public string AccountAsset { get; set; }

        [JsonProperty("account_revenue")]
        public string AccountRevenue { get; set; }

        [JsonProperty("account_expense")]
        public string AccountExpense { get; set; }

        [JsonProperty("account_variance")]
        public string AccountVariance { get; set; }

        [JsonProperty("sin")]
        public string SocialInsuranceNumber { get; set; }

        [JsonProperty("birth_date")]
        public string BirthDate { get; set; }

        [JsonProperty("hire_date")]
        public string HireDate { get; set; }

        [JsonProperty("tax_table_province")]
        public string TaxTableProvince { get; set; }

        [JsonProperty("pay_periods")]
        public string PayPeriods { get; set; }

        [JsonProperty("start_date")]
        public string StartDate { get; set; }

        [JsonProperty("account_number")]
        public string AccountNumber { get; set; }

        [JsonProperty("account_class")]
        public string AccountClass { get; set; }

        [JsonProperty("account_type")]
        public string AccountType { get; set; }

        [JsonProperty("sql_non_query")]
        public string SQLNonQuery { get; set; }
    }

    public class PosterDetailLine
    {
        [JsonProperty("item_number")]
        public string ItemNumber { get; set; }

        [JsonProperty("item_description")]
        public string ItemDescription { get; set; }

        [JsonProperty("quantity")]
        public string Quantity { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("tax_code")]
        public string TaxCode { get; set; }

        [JsonProperty("line_amount")]
        public string LineAmount { get; set; }

        [JsonProperty("ledger_account")]
        public string LedgerAccount { get; set; }

        [JsonProperty("tax_lines")]
        public List<PosterTaxDetailLine> TaxDetailLines { get; set; }

        [JsonProperty("debit_amount")]
        public string DebitAmount { get; set; }

        [JsonProperty("credit_amount")]
        public string CreditAmount { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }

    public class PosterTaxDetailLine
    {
        [JsonProperty("tax_authority")]
        public string TaxAuthority { get; set; }

        [JsonProperty("tax_amount")]
        public string TaxAmount { get; set; }
    }

    public class PosterResponseRoot
    {
        [JsonProperty("job_duration")]
        public string JobDuration { get; set; }

        [JsonProperty("total_requests")]
        public int TotalRequests { get; set; }

        [JsonProperty("responses")]
        public List<PosterResponse> Responses { get; set; }
    }

    public class PosterResponse : PosterBody
    {
        public PosterResponse()
        {
            Messages = new List<string>();
        }

        [JsonProperty("post_ok")]
        public bool? PostOK { get; set; }

        [JsonProperty("rows_affected")]
        public int? RowsAffected { get; set; }

        [JsonProperty("messages")]
        public List<string> Messages { get; set; }
    }
}