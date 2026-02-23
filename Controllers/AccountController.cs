using RestoHub.Filters;
using RestoHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace RestoHub.Controllers
{
    [OturumGerekmez]
    public class AccountController : Controller
    {

        private RestoHubDBEntities db = new RestoHubDBEntities();

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string kullaniciadi, string sifre)
        {
            var kullanici = db.Kullanicilar
            .FirstOrDefault(x => x.KullaniciAdi.ToLower() == kullaniciadi.ToLower() && x.Sifre == sifre && x.Aktif == true);
            if (kullanici == null)
            {
                ViewBag.Hata = "Kullanıcı adı veya şifre yanlış!";
                return View();
            }
            var yetki = db.Yetkiler
            .FirstOrDefault(x => x.YetkiId == kullanici.YetkiId);

            if (kullanici != null)
            {
                
                Session["KullaniciId"] = kullanici.KullaniciId;
                Session["AdSoyad"] = kullanici.Ad + " " + kullanici.Soyad;
                Session["Yetki"] = yetki.YetkiAdi;
                Session["Aktif"] = kullanici.Aktif;
                Session["YetkiId"] = kullanici.YetkiId;
                Session["RestoranId"] = kullanici.RestoranId;

                var restoran = db.Restoranlar.FirstOrDefault(r => r.RestoranId == kullanici.RestoranId);
                Session["RestoranAdi"] = restoran != null ? restoran.RestoranAdi : "RestoHub";


                return RedirectToAction("Index", "Home");
            }

            ViewBag.Hata = "Kullanıcı adı veya şifre yanlış!";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}