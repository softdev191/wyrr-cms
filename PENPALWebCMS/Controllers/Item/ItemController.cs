using PENPAL.DataStore.DataProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PENPAL.VM.UserManagement;

namespace PENPALWebCMS.Controllers.Item
{
    public class ItemController : Controller
    {
        private UserManagementProvider _userManagementProvider = null;
        // GET: Item
        public ActionResult Index()
        {
            return View();
        }

        //public ActionResult GetSearchCriteria()
        //{
        //    ItemDetails itemdetail = new ItemDetails();
        //    try
        //    {
        //        _userManagementProvider = new UserManagementProvider1();

        //        var Itemlst = _userManagementProvider.GetItemDetails();

        //        ViewBag.ItemDetails = from r in Itemlst
        //                              select new
        //                              {
        //                                  ItemId = r.ItemId,
        //                                  ItemName = r.ItemName
        //                              };

        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //    return PartialView("_SearchItem", itemdetail);
        //}

        //public ActionResult ManageItemProduct(ItemDetails item)
        //{
        //    try
        //    {
        //        _userManagementProvider = new UserManagementProvider1();
                
        //        var ItemSeacrhlst = _userManagementProvider.GetItemDescription(item.ItemId);

        //        return Json(new { aaData = ItemSeacrhlst }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}


    }
}