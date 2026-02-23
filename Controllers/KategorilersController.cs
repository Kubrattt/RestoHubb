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
    public class KategorilersController : BaseController
    {
        public async Task<ActionResult> Index(bool? aktif)
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var kategoriler = db.Kategoriler.AsQueryable();

            if (!SuperAdminMi)
                kategoriler = kategoriler.Where(x => x.RestoranId == AktifRestoranId);

            if (aktif.HasValue)
                kategoriler = kategoriler.Where(x => x.Aktif == aktif.Value);

            return View(await kategoriler.ToListAsync());
        }

        public ActionResult Create()
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            DoldurDropDown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "RestoranId,KategoriAdi,Aciklama")] Kategoriler kategoriler)
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (!SuperAdminMi)
                kategoriler.RestoranId = AktifRestoranId;

            if (await db.Kategoriler.AnyAsync(x =>
                x.KategoriAdi == kategoriler.KategoriAdi &&
                x.RestoranId == kategoriler.RestoranId))
            {
                ModelState.AddModelError("KategoriAdi", "Bu kategori adı sistemde kayıtlı. Lütfen başka bir kategori adı giriniz");
            }

            if (!ModelState.IsValid)
            {
                DoldurDropDown(kategoriler);
                return View(kategoriler);
            }

            kategoriler.Aktif = true;
            kategoriler.OlusturmaTarihi = DateTime.Now;

            db.Kategoriler.Add(kategoriler);
            await db.SaveChangesAsync();

            TempData["msj"] = "KAYIT BAŞARILI";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Kategoriler kategoriler = await db.Kategoriler.FindAsync(id);
            if (kategoriler == null)
                return HttpNotFound();

            if (!SuperAdminMi && kategoriler.RestoranId != AktifRestoranId)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            DoldurDropDown(kategoriler);
            return View(kategoriler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "KategoriId,RestoranId,KategoriAdi,Aciklama,Aktif,OlusturmaTarihi")] Kategoriler kategoriler)
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (!SuperAdminMi && kategoriler.RestoranId != AktifRestoranId)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (!SuperAdminMi)
                kategoriler.RestoranId = AktifRestoranId;

            if (await db.Kategoriler.AnyAsync(x =>
                x.KategoriAdi == kategoriler.KategoriAdi &&
                x.RestoranId == kategoriler.RestoranId &&
                x.KategoriId != kategoriler.KategoriId))
            {
                ModelState.AddModelError("KategoriAdi", "Bu kategori adı sistemde kayıtlı.");
            }

            if (!ModelState.IsValid)
            {
                DoldurDropDown(kategoriler);
                return View(kategoriler);
            }

            kategoriler.GuncellemeTarihi = DateTime.Now;
            db.Entry(kategoriler).State = EntityState.Modified;
            await db.SaveChangesAsync();

            TempData["msj"] = "GÜNCELLEME BAŞARILI";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Kategoriler kategoriler = await db.Kategoriler.FindAsync(id);
            if (kategoriler == null)
                return HttpNotFound();

            return View(kategoriler);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            Kategoriler kategoriler = await db.Kategoriler.FindAsync(id);
            kategoriler.Aktif = false;
            await db.SaveChangesAsync();

            TempData["msj"] = "KATEGORİ PASİF YAPILDI";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        private void DoldurDropDown(Kategoriler kategoriler = null)
        {
            var restoranlar = SuperAdminMi
                ? db.Restoranlar.ToList()
                : db.Restoranlar.Where(r => r.RestoranId == AktifRestoranId).ToList();

            ViewBag.RestoranId = new SelectList(restoranlar, "RestoranId", "RestoranAdi", kategoriler?.RestoranId);
        }
    }
}