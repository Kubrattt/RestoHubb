using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using RestoHub.Models;

namespace RestoHub.Controllers
{
    public class UrunlersController : BaseController
    {
        private bool DuzenleyebilirMi => SuperAdminMi || RestoranSahibiMi || RestoranMudurMu;

        // GET: Urunlers
        public async Task<ActionResult> Index(string filtre = "hepsi", int? restoranId = null)
        {
            var urunler = db.Urunler.Include(u => u.Kategoriler).AsQueryable();

            if (SuperAdminMi)
            {
                if (restoranId.HasValue)
                    urunler = urunler.Where(x => x.RestoranId == restoranId.Value);
                ViewBag.Restoranlar = new SelectList(db.Restoranlar.ToList(), "RestoranId", "RestoranAdi", restoranId);
            }
            else
            {
                urunler = urunler.Where(x => x.RestoranId == AktifRestoranId);
            }

            if (filtre == "aktif") urunler = urunler.Where(x => x.Aktif == true);
            else if (filtre == "pasif") urunler = urunler.Where(x => x.Aktif == false);
            else if (filtre == "stokta") urunler = urunler.Where(x => x.Aktif == true && x.StokDurumu == true);
            else if (filtre == "stokYok") urunler = urunler.Where(x => x.Aktif == true && x.StokDurumu == false);

            ViewBag.Filtre = filtre;
            ViewBag.SeciliRestoranId = restoranId;
            return View(await urunler.ToListAsync());
        }

        // GET: Create - restoranId query string'den gelir (SuperAdmin restoran seçince)
        public ActionResult Create(int? restoranId = null)
        {
            if (!DuzenleyebilirMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            int rId = SuperAdminMi ? (restoranId ?? 0) : AktifRestoranId;
            ViewBag.SeciliRestoranId = rId;
            DoldurDropDown(rId);
            return View(new Urunler { RestoranId = rId });
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "RestoranId,KategoriId,BirimId,UrunKodu,UrunAdi,Aciklama,KisaAciklama,Fiyat,MaliyetFiyati,IndirimliFiyat,HazirlamaSuresi,StokDurumu")] Urunler urunler)
        {
            if (!DuzenleyebilirMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (!SuperAdminMi) urunler.RestoranId = AktifRestoranId;

            if (await db.Urunler.AnyAsync(x => x.UrunAdi == urunler.UrunAdi && x.RestoranId == urunler.RestoranId))
                ModelState.AddModelError("UrunAdi", "Bu ürün adı bu restoranda zaten var.");

            if (ModelState.IsValid)
            {
                urunler.Aktif = true;
                urunler.OlusturmaTarihi = DateTime.Now;
                db.Urunler.Add(urunler);
                await db.SaveChangesAsync();
                TempData["msj"] = "ÜRÜN BAŞARIYLA EKLENDİ";
                return RedirectToAction("Index");
            }

            DoldurDropDown(urunler.RestoranId, urunler);
            return View(urunler);
        }

        // GET: Edit
        public async Task<ActionResult> Edit(int? id)
        {
            if (!DuzenleyebilirMi || id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Urunler urunler = await db.Urunler.FindAsync(id);
            if (urunler == null) return HttpNotFound();
            if (!SuperAdminMi && urunler.RestoranId != AktifRestoranId) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            DoldurDropDown(urunler.RestoranId, urunler);
            return View(urunler);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "UrunId,RestoranId,KategoriId,BirimId,UrunKodu,UrunAdi,Aciklama,KisaAciklama,Fiyat,MaliyetFiyati,IndirimliFiyat,HazirlamaSuresi,StokDurumu,Aktif,OlusturmaTarihi")] Urunler urunler)
        {
            if (!DuzenleyebilirMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (!SuperAdminMi && urunler.RestoranId != AktifRestoranId) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (ModelState.IsValid)
            {
                urunler.GuncellemeTarihi = DateTime.Now;
                db.Entry(urunler).State = EntityState.Modified;
                await db.SaveChangesAsync();
                TempData["msj"] = "GÜNCELLEME BAŞARILI";
                return RedirectToAction("Index");
            }

            DoldurDropDown(urunler.RestoranId, urunler);
            return View(urunler);
        }

        // GET: Delete
        public async Task<ActionResult> Delete(int? id)
        {
            if (!DuzenleyebilirMi || id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Urunler urunler = await db.Urunler.FindAsync(id);
            if (urunler == null || (!SuperAdminMi && urunler.RestoranId != AktifRestoranId)) return HttpNotFound();
            return View(urunler);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Urunler urunler = await db.Urunler.FindAsync(id);
            if (urunler != null && (SuperAdminMi || urunler.RestoranId == AktifRestoranId))
            {
                urunler.Aktif = false;
                urunler.GuncellemeTarihi = DateTime.Now;
                await db.SaveChangesAsync();
                TempData["msj"] = "ÜRÜN PASİFE ÇEKİLDİ";
            }
            return RedirectToAction("Index");
        }

        // -------------------------------------------------------
        private void DoldurDropDown(int restoranId, Urunler urun = null)
        {
            var restoranlar = SuperAdminMi
                ? db.Restoranlar.Where(r => r.Aktif == true).ToList()
                : db.Restoranlar.Where(r => r.RestoranId == AktifRestoranId && r.Aktif == true).ToList();

            ViewBag.RestoranId = new SelectList(restoranlar, "RestoranId", "RestoranAdi",
                restoranId > 0 ? (int?)restoranId : null);

            // Kategorileri restorana göre filtrele
            var kategoriler = restoranId > 0
                ? db.Kategoriler.Where(k => k.RestoranId == restoranId).ToList()
                : new List<Kategoriler>();

            ViewBag.KategoriId = new SelectList(kategoriler, "KategoriId", "KategoriAdi", urun?.KategoriId);
            ViewBag.BirimId    = new SelectList(db.BirimTipleri, "BirimId", "BirimAdi", urun?.BirimId);
        }

        // Eski imzayı da koru (Edit POST vs. yerlerde kullanılıyorsa)
        private void DoldurDropDown(Urunler urun = null)
        {
            int rId = SuperAdminMi ? (urun?.RestoranId ?? 0) : AktifRestoranId;
            DoldurDropDown(rId, urun);
        }
    }
}