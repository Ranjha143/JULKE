using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace JULKE
{
    
    public partial class ShopifyOrderModel
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("order_source")]
        public string OrderSource { get; set; }

        [JsonProperty("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }

        [JsonProperty("app_id")]
        public long AppId { get; set; }

        [JsonProperty("browser_ip")]
        public string BrowserIp { get; set; }

        [JsonProperty("buyer_accepts_marketing")]
        public bool BuyerAcceptsMarketing { get; set; }

        [JsonProperty("cancel_reason")]
        public string CancelReason { get; set; }

        [JsonProperty("cancelled_at")]
        public DateTime? CancelledAt { get; set; }

        [JsonProperty("cart_token")]
        public string CartToken { get; set; }

        [JsonProperty("checkout_id")]
        public long? CheckoutId { get; set; }

        [JsonProperty("checkout_token")]
        public string CheckoutToken { get; set; }

        [JsonProperty("client_details")]
        public ClientDetails ClientDetails { get; set; }

        [JsonProperty("closed_at")]
        public DateTime? ClosedAt { get; set; }

        [JsonProperty("confirmed")]
        public bool Confirmed { get; set; }

        [JsonProperty("contact_email")]
        public string ContactEmail { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("current_subtotal_price")]
        public string CurrentSubtotalPrice { get; set; }

        [JsonProperty("current_subtotal_price_set")]
        public Set CurrentSubtotalPriceSet { get; set; }

        [JsonProperty("current_total_discounts")]
        public string CurrentTotalDiscounts { get; set; }

        [JsonProperty("current_total_discounts_set")]
        public Set CurrentTotalDiscountsSet { get; set; }

        [JsonProperty("current_total_duties_set")]
        public object CurrentTotalDutiesSet { get; set; }

        [JsonProperty("current_total_price")]
        public string CurrentTotalPrice { get; set; }

        [JsonProperty("current_total_price_set")]
        public Set CurrentTotalPriceSet { get; set; }

        [JsonProperty("current_total_tax")]
        public string CurrentTotalTax { get; set; }

        [JsonProperty("current_total_tax_set")]
        public Set CurrentTotalTaxSet { get; set; }

        [JsonProperty("customer_locale")]
        public string CustomerLocale { get; set; }

        [JsonProperty("device_id")]
        public string DeviceId { get; set; }

        [JsonProperty("discount_codes")]
        public List<DiscountCode> DiscountCodes { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("estimated_taxes")]
        public bool EstimatedTaxes { get; set; }

        [JsonProperty("financial_status")]
        public string FinancialStatus { get; set; }

        [JsonProperty("fulfillment_status")]
        public string FulfillmentStatus { get; set; }

        [JsonProperty("gateway")]
        public string Gateway { get; set; }

        [JsonProperty("landing_site")]
        public string LandingSite { get; set; }

        [JsonProperty("landing_site_ref")]
        public string LandingSiteRef { get; set; }

        [JsonProperty("location_id")]
        public string LocationId { get; set; }

        [JsonProperty("merchant_of_record_app_id")]
        public string MerchantOfRecordAppId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("note_attributes")]
        public List<NoteAttribute> NoteAttributes { get; set; }

        [JsonProperty("number")]
        public long Number { get; set; }

        [JsonProperty("order_number")]
        public long OrderNumber { get; set; }

        [JsonProperty("order_status_url")]
        public Uri OrderStatusUrl { get; set; }

        [JsonProperty("original_total_duties_set")]
        public object OriginalTotalDutiesSet { get; set; }

        [JsonProperty("payment_gateway_names")]
        public List<string> PaymentGatewayNames { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("presentment_currency")]
        public string PresentmentCurrency { get; set; }

        [JsonProperty("processed_at")]
        public DateTime? ProcessedAt { get; set; }

        [JsonProperty("processing_method")]
        public string ProcessingMethod { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("referring_site")]
        public string ReferringSite { get; set; }

        [JsonProperty("source_identifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("source_name")]
        public string SourceName { get; set; }

        [JsonProperty("source_url")]
        public string SourceUrl { get; set; }

        [JsonProperty("subtotal_price")]
        public string SubtotalPrice { get; set; }

        [JsonProperty("subtotal_price_set")]
        public Set SubtotalPriceSet { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("tax_lines")]
        public object[] TaxLines { get; set; }

        [JsonProperty("taxes_included")]
        public bool TaxesIncluded { get; set; }

        [JsonProperty("test")]
        public bool Test { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("total_discounts")]
        public string TotalDiscounts { get; set; }

        [JsonProperty("total_discounts_set")]
        public Set TotalDiscountsSet { get; set; }

        [JsonProperty("total_line_items_price")]
        public string TotalLineItemsPrice { get; set; }

        [JsonProperty("total_line_items_price_set")]
        public Set TotalLineItemsPriceSet { get; set; }

        [JsonProperty("total_outstanding")]
        public decimal? TotalOutstanding { get; set; }

        [JsonProperty("total_price")]
        public string TotalPrice { get; set; }

        [JsonProperty("total_price_set")]
        public Set TotalPriceSet { get; set; }

        [JsonProperty("total_shipping_price_set")]
        public Set TotalShippingPriceSet { get; set; }

        [JsonProperty("total_tax")]
        public string TotalTax { get; set; }

        [JsonProperty("total_tax_set")]
        public Set TotalTaxSet { get; set; }

        [JsonProperty("total_tip_received")]
        public string TotalTipReceived { get; set; }

        [JsonProperty("total_weight")]
        public long TotalWeight { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonProperty("user_id")]
        public long? UserId { get; set; }

        [JsonProperty("billing_address")]
        public BillingAddress BillingAddress { get; set; }

        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("customer_name")]
        public string CustomerName { get; set; }

        [JsonProperty("discount_applications")]
        public DiscountApplication[] DiscountApplications { get; set; }

        [JsonProperty("fulfillments")]
        public object[] Fulfillments { get; set; }

        [JsonProperty("line_items")]
        public LineItem[] LineItems { get; set; }

        [JsonProperty("payment_terms")]
        public PaymentTerms PaymentTerms { get; set; }

        [JsonProperty("refunds")]
        public object[] Refunds { get; set; }

        [JsonProperty("shipping_address")]
        public BillingAddress ShippingAddress { get; set; }

        [JsonProperty("shipping_lines")]
        public List<ShippingLine> ShippingLines { get; set; }

        [JsonProperty("order_is_verified")]
        public bool OrderIsVerified { get; set; }

        [JsonProperty("courier")]
        public Courier Courier { get; set; }

        [JsonProperty("courier_error")]
        public CourierError CourierError { get; set; }

        [JsonProperty("order_error_state")]
        public bool OrderErrorState { get; set; }

        //[JsonProperty("courier_retry")]
        //public CourierRetry CourierRetry { get; set; }

        [JsonProperty("order_tracking_status")]
        public OrderTracking OrderTrackingStatus { get; set; }

        [JsonProperty("order_tracking_history")]
        public List<OrderTracking> OrderTrackingHistory { get; set; }

        [JsonProperty("invoice")]
        public Invoice Invoice { get; set; }

        [JsonProperty("printed")]
        public Int32 Printed { get; set; }

        [JsonProperty("dispatched")]
        public Int32 Dispatched { get; set; }  
        
        [JsonProperty("batch_id")]
        public long BatchId { get; set; }

        [JsonProperty("CourierId")]
        public long CourierId { get; set; }

        [JsonProperty("DestinationId")]
        public long DestinationId { get; set; }

        //.Field("isSelected")
        [JsonProperty("isSelected")]
        public bool IsSelected { get; set; } = false;


    }
    public partial class Invoice
    {
        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("invoice_Id")]
        public string InvoiceId { get; set; }

        [JsonProperty("invoice_synced")]
        public long InvoiceSynced { get; set; }

        [JsonProperty("invoice_synced_dateTime")]
        public DateTimeOffset InvoiceSyncedDateTime { get; set; }

        [JsonProperty("synced_invoice_sid")]
        public string SyncedInvoiceSid { get; set; }

        [JsonProperty("invoice_created_dateTime")]
        public DateTimeOffset InvoiceCreatedDateTime { get; set; }
    }


    public partial class ShippingLine
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("carrier_identifier")]
        public object CarrierIdentifier { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("delivery_category")]
        public object DeliveryCategory { get; set; }

        [JsonProperty("discounted_price")]
        public string DiscountedPrice { get; set; }

        [JsonProperty("discounted_price_set")]
        public Set DiscountedPriceSet { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("price_set")]
        public Set PriceSet { get; set; }

        [JsonProperty("requested_fulfillment_service_id")]
        public object RequestedFulfillmentServiceId { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("tax_lines")]
        public List<object> TaxLines { get; set; }

        [JsonProperty("discount_allocations")]
        public List<object> DiscountAllocations { get; set; }
    }


    public partial class BillingAddress
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("address1")]
        public string Address1 { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        [JsonProperty("province")]
        public string Province { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("address2")]
        public string Address2 { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        [JsonProperty("longitude")]
        public string Longitude { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("province_code")]
        public string ProvinceCode { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [JsonProperty("customer_id", NullValueHandling = NullValueHandling.Ignore)]
        public long? CustomerId { get; set; }

        [JsonProperty("country_name", NullValueHandling = NullValueHandling.Ignore)]
        public string CountryName { get; set; }

        [JsonProperty("default", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Default { get; set; }
    }

    public partial class ClientDetails
    {
        [JsonProperty("accept_language")]
        public string AcceptLanguage { get; set; }

        [JsonProperty("browser_height")]
        public string BrowserHeight { get; set; }

        [JsonProperty("browser_ip")]
        public string BrowserIp { get; set; }

        [JsonProperty("browser_width")]
        public string BrowserWidth { get; set; }

        [JsonProperty("session_hash")]
        public string SessionHash { get; set; }

        [JsonProperty("user_agent")]
        public string UserAgent { get; set; }
    }

    public partial class Set
    {
        [JsonProperty("shop_money")]
        public Money ShopMoney { get; set; }

        [JsonProperty("presentment_money")]
        public Money PresentmentMoney { get; set; }
    }

    public partial class Money
    {
        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currency_code")]
        public string CurrencyCode { get; set; }
    }

    public partial class Customer
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("accepts_marketing")]
        public bool AcceptsMarketing { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("verified_email")]
        public bool VerifiedEmail { get; set; }

        [JsonProperty("multipass_identifier")]
        public string MultipassIdentifier { get; set; }

        [JsonProperty("tax_exempt")]
        public bool TaxExempt { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("accepts_marketing_updated_at")]
        public DateTime? AcceptsMarketingUpdatedAt { get; set; }

        [JsonProperty("marketing_opt_in_level")]
        public string MarketingOptInLevel { get; set; }

        [JsonProperty("tax_exemptions")]
        public object[] TaxExemptions { get; set; }

        [JsonProperty("email_marketing_consent")]
        public MarketingConsent EmailMarketingConsent { get; set; }

        [JsonProperty("sms_marketing_consent")]
        public MarketingConsent SmsMarketingConsent { get; set; }

        [JsonProperty("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }

        [JsonProperty("default_address")]
        public BillingAddress DefaultAddress { get; set; }
    }

    public partial class MarketingConsent
    {
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("opt_in_level")]
        public string OptInLevel { get; set; }

        [JsonProperty("consent_updated_at")]
        public DateTime? ConsentUpdatedAt { get; set; }

        [JsonProperty("consent_collected_from", NullValueHandling = NullValueHandling.Ignore)]
        public string ConsentCollectedFrom { get; set; }
    }

    public partial class DiscountApplication
    {
        [JsonProperty("target_type")]
        public string TargetType { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("value_type")]
        public string ValueType { get; set; }

        [JsonProperty("allocation_method")]
        public string AllocationMethod { get; set; }

        [JsonProperty("target_selection")]
        public string TargetSelection { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public partial class DiscountCode
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public partial class LineItem
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("admin_graphql_api_id")]
        public string AdminGraphqlApiId { get; set; }

        [JsonProperty("fulfillable_quantity")]
        public long FulfillableQuantity { get; set; }

        [JsonProperty("fulfillment_service")]
        public string FulfillmentService { get; set; }

        [JsonProperty("fulfillment_status")]
        public string FulfillmentStatus { get; set; }

        [JsonProperty("gift_card")]
        public bool GiftCard { get; set; }

        [JsonProperty("grams")]
        public long Grams { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("price_set")]
        public Set PriceSet { get; set; }

        [JsonProperty("product_exists")]
        public bool ProductExists { get; set; }

        [JsonProperty("product_id")]
        public long ProductId { get; set; }

        [JsonProperty("properties")]
        public object[] Properties { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("scanned_quantity")]
        public long ScannedQuantity { get; set; }

        [JsonProperty("requires_shipping")]
        public bool RequiresShipping { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("taxable")]
        public bool Taxable { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("total_discount")]
        public string TotalDiscount { get; set; }

        [JsonProperty("total_discount_set")]
        public Set TotalDiscountSet { get; set; }

        [JsonProperty("variant_id")]
        public long VariantId { get; set; }

        [JsonProperty("variant_inventory_management")]
        public string VariantInventoryManagement { get; set; }

        [JsonProperty("variant_title")]
        public string VariantTitle { get; set; }

        [JsonProperty("vendor")]
        public string Vendor { get; set; }

        [JsonProperty("tax_lines")]
        public TaxLine[] TaxLines { get; set; }

        [JsonProperty("duties")]
        public object[] Duties { get; set; }

        [JsonProperty("discount_allocations")]
        public DiscountAllocation[] DiscountAllocations { get; set; }

        [JsonProperty("packed")]
        public long Packed { get; set; }

        [JsonProperty("scanned")]
        public Int32 Scanned { get; set; }

        [JsonProperty("image_link")]
        public string ImageLink { get; set; }
    }

    public partial class DiscountAllocation
    {
        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("amount_set")]
        public Set AmountSet { get; set; }

        [JsonProperty("discount_application_index")]
        public long DiscountApplicationIndex { get; set; }
    }

    public partial class TaxLine
    {
        [JsonProperty("channel_liable")]
        public bool ChannelLiable { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("price_set")]
        public Set PriceSet { get; set; }

        [JsonProperty("rate")]
        public double Rate { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public partial class PaymentTerms
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("due_in_days")]
        public string DueInDays { get; set; }

        [JsonProperty("payment_schedules")]
        public PaymentSchedule[] PaymentSchedules { get; set; }

        [JsonProperty("payment_terms_name")]
        public string PaymentTermsName { get; set; }

        [JsonProperty("payment_terms_type")]
        public string PaymentTermsType { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    public partial class PaymentSchedule
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("issued_at")]
        public DateTime? IssuedAt { get; set; }

        [JsonProperty("due_at")]
        public DateTime? DueAt { get; set; }

        [JsonProperty("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    public partial class Courier
    {
        [JsonProperty("CourierId")]
        public long CourierId { get; set; }

        [JsonProperty("courier_name")]
        public string CourierName { get; set; }

        [JsonProperty("cn_number")]
        public string CnNumber { get; set; }

        [JsonProperty("destination_city")]
        public string DestinationCity { get; set; }

        [JsonProperty("DestinationId")]
        public long DestinationId { get; set; }

        [JsonProperty("destination_address")]
        public string DestinationAddress { get; set; }
    }

    public partial class CourierError
    {
        [JsonProperty("courier_error")]
        public bool CourierErrorCourierError { get; set; }

        [JsonProperty("short_inventory")]
        public bool ShortInventory { get; set; }

        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }
    }


    public class DashboardOrderStatusModel
    {
        [JsonProperty("order_tracking_status")]
        public OrderTracking OrderTrackingStatus { get; set; }
    }

    public partial class OrderTracking
    {
        [JsonProperty("status_code")]
        public long StatusCode { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("status_updated_at")]
        public DateTime StatusUpdatedAt { get; set; }
    }

    public partial class NoteAttribute
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

}
