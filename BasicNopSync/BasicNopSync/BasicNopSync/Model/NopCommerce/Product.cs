﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.Model.NopCommerce
{
    class Product 
    {
        public int Id { get; set; }
        public int ProductTypeId { get; set; }
        public int ParentGroupedProductId { get; set; }        
        public bool VisibleIndividually { get; set; }        
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string AdminComment { get; set; }
        public int ProductTemplateId { get; set; }
        public int VendorId { get; set; }
        public bool ShowOnHomePage { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public bool AllowCustomerReviews { get; set; }
        public int ApprovedRatingSum { get; set; }
        public int NotApprovedRatingSum { get; set; }
        public int ApprovedTotalReviews { get; set; }
        public int NotApprovedTotalReviews { get; set; }
        public bool SubjectToAcl { get; set; }
        public bool LimitedToStores { get; set; }
        public string Sku { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public string Gtin { get; set; }
        public bool IsGiftCard { get; set; }
        public int GiftCardTypeId { get; set; }
        public decimal? OverriddenGiftCardAmount { get; set; }
        public bool RequireOtherProducts { get; set; }
        public string RequiredProductIds { get; set; }
        public bool AutomaticallyAddRequiredProducts { get; set; }
        public bool IsDownload { get; set; }
        public int DownloadId { get; set; }
        public bool UnlimitedDownloads { get; set; }
        public int MaxNumberOfDownloads { get; set; }
        public int? DownloadExpirationDays { get; set; }
        public int DownloadActivationTypeId { get; set; }
        public bool HasSampleDownload { get; set; }
        public int SampleDownloadId { get; set; }
        public bool HasUserAgreement { get; set; }
        public string UserAgreementText { get; set; }
        public bool IsRecurring { get; set; }
        public int RecurringCycleLength { get; set; }
        public int RecurringCyclePeriodId { get; set; }
        public int RecurringTotalCycles { get; set; }
        public bool IsRental { get; set; }
        public int RentalPriceLength { get; set; }
        public int RentalPricePeriodId { get; set; }
        public bool IsShipEnabled { get; set; }
        public bool IsFreeShipping { get; set; }        
        public bool ShipSeparately { get; set; }
        public decimal AdditionalShippingCharge { get; set; }
        public int DeliveryDateId { get; set; }
        public bool IsTaxExempt { get; set; }
        public int TaxCategoryId { get; set; }
        public bool IsTelecommunicationsOrBroadcastingOrElectronicServices { get; set; }
        public int ManageInventoryMethodId { get; set; }
        public bool UseMultipleWarehouses { get; set; }
        public int WarehouseId { get; set; }
        public int StockQuantity { get; set; }
        public bool DisplayStockAvailability { get; set; }
        public bool DisplayStockQuantity { get; set; }
        public int MinStockQuantity { get; set; }
        public int LowStockActivityId { get; set; }
        public int NotifyAdminForQuantityBelow { get; set; }
        public int BackorderModeId { get; set; }
        public bool AllowBackInStockSubscriptions { get; set; }
        public int OrderMinimumQuantity { get; set; }
        public int OrderMaximumQuantity { get; set; }
        public string AllowedQuantities { get; set; }
        public bool AllowAddingOnlyExistingAttributeCombinations { get; set; }
        public bool DisableBuyButton { get; set; }
        public bool DisableWishlistButton { get; set; }
        public bool AvailableForPreOrder { get; set; }
        public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }
        public bool CallForPrice { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public decimal ProductCost { get; set; }
        public decimal? SpecialPrice { get; set; }
        public DateTime? SpecialPriceStartDateTimeUtc { get; set; }
        public DateTime? SpecialPriceEndDateTimeUtc { get; set; }
        public bool CustomerEntersPrice { get; set; }
        public decimal MinimumCustomerEnteredPrice { get; set; }
        public decimal MaximumCustomerEnteredPrice { get; set; }
        public bool BasepriceEnabled { get; set; }
        public decimal BasepriceAmount { get; set; }
        public int BasepriceUnitId { get; set; }
        public decimal BasepriceBaseAmount { get; set; }
        public int BasepriceBaseUnitId { get; set; }
        public bool MarkAsNew { get; set; }
        public DateTime? MarkAsNewStartDateTimeUtc { get; set; }
        public DateTime? MarkAsNewEndDateTimeUtc { get; set; }
        public bool HasTierPrices { get; set; }
        public bool HasDiscountsApplied { get; set; }
        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public DateTime? AvailableStartDateTimeUtc { get; set; }
        public DateTime? AvailableEndDateTimeUtc { get; set; }
        public int DisplayOrder { get; set; }
        public bool Published { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }        

        public Product(string name = "",string fullDescription ="",string manufacturerPartNumber = "",string gtin = "",
                        int stockQuantity = 0, decimal price = 0, decimal weight = 0, decimal length = 0, decimal width = 0,
                        decimal height = 0, decimal condit = 1, string mercatorId = "", int modiftag = 0, int id=0) {
            initProduct();

            Id = id;
            Name = name;
            FullDescription = fullDescription;
            Sku = mercatorId;
            ManufacturerPartNumber = manufacturerPartNumber;
            Gtin = gtin;
            StockQuantity = stockQuantity;
            Price = price;
            Weight = weight;
            Length = length;
            Width = width;
            Height = height;            
        }

        private void initProduct()
        {
            ProductTypeId = 5;
            ParentGroupedProductId = 0;
            VisibleIndividually = true;
            ProductTemplateId = 1;
            VendorId = 0;
            ShowOnHomePage = false;
            AllowCustomerReviews = false;
            ApprovedRatingSum = 0;
            ApprovedTotalReviews = 0;
            NotApprovedTotalReviews = 0;
            SubjectToAcl = false;
            LimitedToStores = false;            
            IsGiftCard = false;
            GiftCardTypeId = 0;
            RequireOtherProducts = false;
            RequiredProductIds = null;
            AutomaticallyAddRequiredProducts = false;
            IsDownload = false;
            DownloadId = 0;
            UnlimitedDownloads = false;
            MaxNumberOfDownloads = 0;
            DownloadActivationTypeId = 0;
            HasSampleDownload = false;
            SampleDownloadId = 0;
            HasUserAgreement = false;
            IsRecurring = false;
            RecurringCycleLength = 0;
            RecurringCyclePeriodId = 0;
            RecurringTotalCycles = 0;
            IsRental = false;
            RentalPriceLength = 0;
            RentalPricePeriodId = 0;
            IsShipEnabled = true;
            IsFreeShipping = false;
            ShipSeparately = false;
            AdditionalShippingCharge = 0;
            DeliveryDateId = 0;
            IsTaxExempt = false;
            TaxCategoryId= 0;
            IsTelecommunicationsOrBroadcastingOrElectronicServices = false;
            ManageInventoryMethodId = 1;
            UseMultipleWarehouses = false;
            WarehouseId = 0;            
            DisplayStockAvailability = true;
            DisplayStockQuantity = true;
            MinStockQuantity = 0;
            LowStockActivityId = 0;
            NotifyAdminForQuantityBelow = 0;
            //BackOrderModeId : 1 = Autorise les commandes quand stock à 0
            BackorderModeId = 0;
            AllowBackInStockSubscriptions = false;
            OrderMinimumQuantity = 1;
            OrderMaximumQuantity = 50000;
            AllowAddingOnlyExistingAttributeCombinations = false;
            DisableBuyButton = false;
            DisableWishlistButton = false;
            AvailableForPreOrder = false;
            CallForPrice = false;            
            OldPrice = 0;            
            SpecialPrice = null;
            ProductCost = 0;
            CustomerEntersPrice = false;
            MinimumCustomerEnteredPrice = 0;
            MaximumCustomerEnteredPrice = 0;
            BasepriceEnabled = false;
            BasepriceAmount = 0;
            BasepriceUnitId = 1;
            BasepriceBaseAmount = 0;
            BasepriceBaseUnitId = 1;
            HasTierPrices = true;
            HasDiscountsApplied = false;
            DisplayOrder = 0;
            Published = true;
            Deleted = false;            
        }
    }
}