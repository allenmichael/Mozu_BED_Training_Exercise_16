using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mozu.Api;
using Autofac;
using Mozu.Api.ToolKit.Config;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace Mozu_BED_Training_Exercise_16
{
    [TestClass]
    public class MozuDataConnectorTests
    {
        private IApiContext _apiContext;
        private IContainer _container;

        [TestInitialize]
        public void Init()
        {
            _container = new Bootstrapper().Bootstrap().Container;
            var appSetting = _container.Resolve<IAppSetting>();
            var tenantId = int.Parse(appSetting.Settings["TenantId"].ToString());
            var siteId = int.Parse(appSetting.Settings["SiteId"].ToString());

            _apiContext = new ApiContext(tenantId, siteId);
        }

        [TestMethod]
        public void Exercise_16_Add_Discount_And_Customer_To_Segment()
        {
            var discountResource = new Mozu.Api.Resources.Commerce.Catalog.Admin.DiscountResource(_apiContext);
            var customerSegmentResouce = new Mozu.Api.Resources.Commerce.Customer.CustomerSegmentResource(_apiContext);
            var customerAccountResource = new Mozu.Api.Resources.Commerce.Customer.CustomerAccountResource(_apiContext);

            var discount = (discountResource.GetDiscountsAsync(filter: "Content.Name eq '10% Off Scarves'").Result).Items[0];
            
            var customerSegment = (customerSegmentResouce.GetSegmentsAsync(filter:"Name eq 'High Volume Customer'").Result).Items[0];
            var segmentToAdd = new Mozu.Api.Contracts.ProductAdmin.CustomerSegment()
            {
                Id = customerSegment.Id
            };

            if (!(discount.Conditions.CustomerSegments.Exists(x => x.Id == segmentToAdd.Id)))
            {
                discount.Conditions.CustomerSegments.Add(segmentToAdd);
                var updatedDiscount = discountResource.UpdateDiscountAsync(discount, (int)discount.Id).Result;
            }

            var customerAccountIds = new List<int>();

            var customerAccount = (customerAccountResource.GetAccountsAsync(filter:"FirstName eq 'Malcolm'").Result).Items[0];

            customerAccountIds.Add(customerAccount.Id);

            if(!(customerAccount.Segments.Exists(x => x.Id == customerSegment.Id)))
            {
                customerSegmentResouce.AddSegmentAccountsAsync(customerAccountIds, customerSegment.Id).Wait();
            }
        }
    }
}
