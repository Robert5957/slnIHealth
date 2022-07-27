
using prjIHealth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using prjiHealth.ViewModels;
using HealthyLifeApp;
using Microsoft.AspNetCore.Http;
using prjiHealth.Models;
using System.Text.Json;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using prjIHealth.ViewModels;
using X.PagedList;

namespace prjIHealth.Controllers
{
    public class MemberController : Controller
    {
        utilities ul = new utilities();
        public static TMember loginUser = null; 
        public static string userName = "登入";
        public static int userID = 0;
        private readonly IHealthContext _context;
        private IWebHostEnvironment _environment;

        public MemberController(IHealthContext context, IWebHostEnvironment iwhe)
        {
            _context = context;
            _environment=iwhe;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(CLoginViewModel vModel)
        {
            if (vModel.fUserName == null || vModel.fPassword == null) { return Content("empty", "text/plain", System.Text.Encoding.UTF8); }

            var q = _context.TMembers.FirstOrDefault(tm => tm.FUserName == vModel.fUserName);
            if (q != null)
            {
                if (q.FPassword == vModel.fPassword)
                {
                    string loginSession = JsonSerializer.Serialize(q);
                    HttpContext.Session.SetString(CDictionary.SK_Logined_User, loginSession);
                    loginUser = JsonSerializer.Deserialize<TMember>(loginSession);
                    userName = $"{loginUser.FUserName}";
                    userID = loginUser.FMemberId;
                    int authorId = (int)loginUser.FAuthorityId;
                    string loginContent = loginUser.FAuthorityId + loginUser.FUserName;
                    return Content(loginContent, "text/plain", System.Text.Encoding.UTF8);
                }
            }
            return Content("false", "text/plain", System.Text.Encoding.UTF8);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove(CDictionary.SK_Logined_User);
            userName = "登入";
            return RedirectToAction("Index","Home");
        }
        public IActionResult Edit()
        {
            if (HttpContext.Session.Keys.Contains(CDictionary.SK_Logined_User))
            {
                var memberEdit = HttpContext.Session.GetString(CDictionary.SK_Logined_User);
                loginUser = JsonSerializer.Deserialize<TMember>(memberEdit);
                var q = _context.TMembers.FirstOrDefault(m => m.FMemberId == loginUser.FMemberId);
                return View(q);
            }
            else
            {
                var q = _context.TMembers.FirstOrDefault(m => m.FMemberId == 8);
                return View(q);
            }
        }
        [HttpPost]
        public IActionResult Edit(CLoginViewModel vModel)
        {
            var q = _context.TMembers.FirstOrDefault(m => m.FMemberId == vModel.fMemberId);
            if (q != null)
            {
                if (vModel.photo!= null)
                {
                    string pName = Guid.NewGuid().ToString() + ".jpg";
                    vModel.photo.CopyTo(new FileStream(_environment.WebRootPath + "/img/member/" + pName, FileMode.Create));
                    q.FPicturePath = pName;
                }
                q.FMemberName = vModel.fMemberName;
                q.FPassword = vModel.fPassword;
                q.FBirthday = vModel.fBirthday;
                q.FAddress = vModel.fAddress;
                q.FPhone = vModel.fPhone;
                q.FEmail = vModel.fEmail;
                q.FRemarks = vModel.fRemarks;
                q.FPhone = vModel.fPhone;

            }       
            _context.SaveChanges();
            return RedirectToAction("Edit","Member");
        }
        // GET: Member
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
              public IActionResult Register(TMember tm)
        {
           
            if (tm.FPassword == null || tm.FUserName == null) { return Content("empty", "text/plain", System.Text.Encoding.UTF8); }
            else
            {
                var q = _context.TMembers.FirstOrDefault(m => m.FUserName == tm.FUserName);
                if (q == null)
                {
                    _context.TMembers.Add(tm);
                    _context.SaveChanges();
                    return Content("true", "text/plain", System.Text.Encoding.UTF8);
                }
                else { return Content("user", "text/plain", System.Text.Encoding.UTF8); }
            }
        }
        public IActionResult Delete(int? id)
        {
            IHealthContext dblIHealth = new IHealthContext();
            TTrackList trackList = dblIHealth.TTrackLists.FirstOrDefault(t => t.FProductId == id);
            if (trackList != null)
            {
                dblIHealth.TTrackLists.Remove(trackList);
                dblIHealth.SaveChanges();
            }
            return RedirectToAction("ShowTrackList");
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ForgotPassword([Bind("fEmail,")] CLoginViewModel vModel)
        {
            if (vModel.fEmail == null) { return Content("empty", "text/plain", System.Text.Encoding.UTF8); }
            else
            {

                var q = _context.TMembers.FirstOrDefault(m => m.FEmail == vModel.fEmail);

                if (q != null)
                {
                    utilities.sendMail(q.FUserName, q.FEmail);
                    return Content(q.FUserName.ToString(), "text/plain", System.Text.Encoding.UTF8);

                }
                else
                {
                    return Content("false", "text/plain", System.Text.Encoding.UTF8);
                }
            }
        }
        public IActionResult ResetPassword() {
            return View();
        }
        [HttpPost]
        public IActionResult ResetPassword(CLoginViewModel vmodel)
        {
            if (vmodel.fEmail == null)
            {
                return Content("empty", "text/plain", System.Text.Encoding.UTF8);
            }
            else
            {
                var q = _context.TMembers.FirstOrDefault(m => m.FEmail == vmodel.fEmail);
                if (q != null)
                {
                    if (q.FPassword==vmodel.fPassword)
                    {
                        if (vmodel.firstPassword == vmodel.confirmPassword)
                        {
                            q.FPassword = vmodel.firstPassword;
                            _context.SaveChanges();
                            return Content(q.FUserName.ToString(), "text/plain", System.Text.Encoding.UTF8);
                        }
                        else { return Content("ConfirmPasswordError", "text/plain", System.Text.Encoding.UTF8); }
                        
                    }
                    else
                    {
                        return Content("PasswordError", "text/plain", System.Text.Encoding.UTF8);
                    }
                }
                else
                {
                    return Content("false", "text/plain", System.Text.Encoding.UTF8);
                }
            }
        }

        //========================追蹤清單===========================    

        public IActionResult ShowTrackList()
        {
            return View();
        }

        public IActionResult ShowTrackProduct(int? id)//MemberID
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var showProducts = from a in _context.TTrackLists
                                   join b in _context.TProducts
                                   on a.FProductId equals b.FProductId
                                   where a.FMemberId == id
                                   select b;
                return Json(showProducts);
            }
        }

        public IActionResult ShowTrackCount(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                int trackNum = _context.TTrackLists.Where(t => t.FMemberId == userID).Select(t=>t).Count();
                return Json(trackNum);
            }
        }

        public IActionResult DeleteTrackList(int? id) //ProductID
        {
            var trackList = (from t in _context.TTrackLists
                             where t.FMemberId == /*1*/userID && t.FProductId == id
                             select t).FirstOrDefault();
            if (trackList != null)
            {
                _context.TTrackLists.Remove(trackList);
                _context.SaveChanges();
            }
            return RedirectToAction("ShowTrackList");
        }

        //===================購買紀錄=============================
        public IActionResult OrderList(int? page)
        {
            var pro = (from o in _context.TOrders
                       join p in _context.TPaymentCategories
                       on o.FPaymentCategoryId equals p.FPaymentCategoryId
                       join s in _context.TStatuses
                       on o.FStatusNumber equals s.FStatusNumber
                       join m in _context.TMembers
                       on o.FMemberId equals m.FMemberId
                       where o.FMemberId == loginUser.FMemberId
                       select new COrderViewModel()
                       {
                           FOrderId = o.FOrderId,
                           FPaymentCategoryId = o.FPaymentCategoryId,
                           fPayment = o.FPaymentCategory,
                           FDate = o.FDate,
                           FAddress = o.FAddress,
                           FMemberId = loginUser.FMemberId,
                           fmember = o.FMember,
                           FContact = o.FContact,
                           FPhone = o.FPhone,
                           FRemarks = o.FRemarks,
                           FStatusNumber = o.FStatusNumber,
                           fstatus = o.FStatusNumberNavigation
                       }).ToList();
            var pageNumber = page ?? 1;
            var onePageOfPro = pro.ToPagedList(pageNumber, 3);
            ViewBag.onePageOfPro = onePageOfPro;
            return View(onePageOfPro);
        }
        public IActionResult OrderDetailList(int? id)
        {
            IHealthContext db = new IHealthContext();
            var odt = (from o in db.TOrderDetails
                       where o.FOrderId == id
                       //where o.FOrderDetailsId == id
                       join or in db.TOrders
                       on o.FOrderId equals or.FOrderId
                       join d in db.TDiscounts
                       on o.FDiscountId equals d.FDiscountId
                       join p in db.TProducts
                       on o.FProductId equals p.FProductId
                       select new COrderDetailViewModel()
                       {
                           FOrderId = o.FOrderId,
                           FOrderDetailsId = o.FOrderDetailsId,
                           FQuantity = o.FQuantity,
                           FUnitprice = o.FUnitprice,
                           fdiscount = o.FDiscount,
                           FDiscountId = o.FDiscountId,
                           FProductId = o.FProductId,
                           fproduct = o.FProduct
                       }).ToList();
            if (odt == null)
            {
                return RedirectToAction("OrderList");
            }
            return View(odt);
        }

    }
}

