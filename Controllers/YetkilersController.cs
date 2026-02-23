using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using RestoHub.Models;

namespace RestoHub.Controllers
{
    public class YetkilersController : BaseController
    {
        // Erişim: SuperAdmin(1), RestoranSahibi(2), RestoranMüdürü(3)
        public async Task<ActionResult> Index()
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(await db.Yetkiler.ToListAsync());
        }

        // Erişim: SuperAdmin(1), RestoranSahibi(2), RestoranMüdürü(3)
        public ActionResult Create()
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "YetkiAdi,Aciklama")] Yetkiler yetkiler)
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (await db.Yetkiler.AnyAsync(x => x.YetkiAdi == yetkiler.YetkiAdi))
                ModelState.AddModelError("YetkiAdi", "Bu yetki sistemde kayıtlı. Lütfen başka bir yetki giriniz");

            if (!ModelState.IsValid)
                return View(yetkiler);

            yetkiler.OlusturmaTarihi = DateTime.Now;
            db.Yetkiler.Add(yetkiler);
            await db.SaveChangesAsync();

            TempData["msj"] = "KAYIT BAŞARILI";
            return RedirectToAction("Index");
        }
        public async Task<ActionResult> Edit(int? id)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Yetkiler yetkiler = await db.Yetkiler.FindAsync(id);
            if (yetkiler == null) return HttpNotFound();

            return View(yetkiler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "YetkiId,YetkiAdi,Aciklama,OlusturmaTarihi")] Yetkiler yetkiler)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (await db.Yetkiler.AnyAsync(x =>
                x.YetkiAdi == yetkiler.YetkiAdi &&
                x.YetkiId != yetkiler.YetkiId))
            {
                ModelState.AddModelError("YetkiAdi", "Bu yetki sistemde kayıtlı. Lütfen başka bir yetki giriniz");
            }

            if (!ModelState.IsValid)
                return View(yetkiler);

            db.Entry(yetkiler).State = EntityState.Modified;
            await db.SaveChangesAsync();

            TempData["msj"] = "GÜNCELLEME BAŞARILI";
            return RedirectToAction("Index");
        }
        public async Task<ActionResult> Delete(int? id)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Yetkiler yetkiler = await db.Yetkiler.FindAsync(id);
            if (yetkiler == null) return HttpNotFound();

            return View(yetkiler);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            Yetkiler yetkiler = await db.Yetkiler.FindAsync(id);
            db.Yetkiler.Remove(yetkiler);
            await db.SaveChangesAsync();

            TempData["msj"] = "YETKİ SİLİNDİ";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}