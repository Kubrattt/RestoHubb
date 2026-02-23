using RestoHub.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RestoHub.Controllers
{
    public class HomeController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            var model = new HomeDashboardViewModel();

            if (SuperAdminMi)
            {
                model.ToplamRestoran  = await db.Restoranlar.CountAsync();
                model.ToplamKullanici = await db.Kullanicilar.CountAsync(x => x.Aktif == true);
                model.Restoranlar     = await db.Restoranlar.ToListAsync();
            }
            else
            {
                var restoran = await db.Restoranlar
                       .FirstOrDefaultAsync(x => x.RestoranId == AktifRestoranId);
                model.AktifRestoran    = restoran;
                model.AktifRestoranAdi = restoran?.RestoranAdi;
                model.RestoranKullanici  = await db.Kullanicilar
                                                   .CountAsync(x => x.RestoranId == AktifRestoranId && x.Aktif == true);
            }

            model.AktifKullaniciAdi = AktifAdSoyad;
            model.AktifYetkiId      = AktifYetkiId;

            return View(model);
        }
    }
}