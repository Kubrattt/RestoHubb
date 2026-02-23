using RestoHub.Models;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RestoHub.Controllers
{
    public class KullanicilarsController : BaseController
    {
        private const int SUPER_ADMIN = 1;
        private const int RESTORAN_SAHIBI = 2;
        private const int RESTORAN_MUDURU = 3;
        private const int GARSON = 4;
        private const int ASCI = 5;
        private const int KASIYER = 6;

        public async Task<ActionResult> Index(bool? aktif)
        {
            if (AltYetkiMi)
                return RedirectToAction("Profilim"); 

            var kullanicilar = db.Kullanicilar.AsQueryable();

            if (!SuperAdminMi)
                kullanicilar = kullanicilar.Where(x => x.RestoranId == AktifRestoranId);

            if (aktif.HasValue)
                kullanicilar = kullanicilar.Where(x => x.Aktif == aktif.Value);

            return View(await kullanicilar.ToListAsync());
        }

        public async Task<ActionResult> Profilim()
        {
            var kullanici = await db.Kullanicilar.FindAsync(AktifKullaniciId);
            if (kullanici == null)
                return HttpNotFound();

            return View(kullanici);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Profilim(
            [Bind(Include = "KullaniciId,Ad,Soyad,Email,Telefon,Sifre")]
            Kullanicilar kullanicilar)
        {
            if (kullanicilar.KullaniciId != AktifKullaniciId)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (await db.Kullanicilar.AnyAsync(x =>
                x.Email == kullanicilar.Email &&
                x.KullaniciId != kullanicilar.KullaniciId))
            {
                ModelState.AddModelError("Email", "Bu email başka bir kullanıcıya ait.");
            }

            if (!ModelState.IsValid)
                return View(kullanicilar);

            var mevcutKullanici = await db.Kullanicilar.FindAsync(AktifKullaniciId);
            mevcutKullanici.Ad              = kullanicilar.Ad;
            mevcutKullanici.Soyad           = kullanicilar.Soyad;
            mevcutKullanici.Email           = kullanicilar.Email;
            mevcutKullanici.Telefon         = kullanicilar.Telefon;
            mevcutKullanici.Sifre           = kullanicilar.Sifre;
            mevcutKullanici.GuncellemeTarihi = DateTime.Now;

            await db.SaveChangesAsync();

            TempData["msj"] = "PROFİLİNİZ GÜNCELLENDİ";
            return RedirectToAction("Profilim");
        }

        public ActionResult Create()
        {
            if (!SuperAdminMi && !RestoranSahibiMi)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            DoldurDropDown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Include = "RestoranId,YetkiId,KullaniciAdi,Ad,Soyad,Email,Telefon,Sifre,BaslamaTarihi")]
            Kullanicilar kullanicilar)
        {
            if (!SuperAdminMi && !RestoranSahibiMi)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (!SuperAdminMi)
            {
                kullanicilar.RestoranId = AktifRestoranId;

                var izinliYetkiler = IzinliAtanabilirYetkiler(AktifYetkiId);
                if (!izinliYetkiler.Contains(kullanicilar.YetkiId))
                {
                    ModelState.AddModelError("YetkiId", "Bu yetkiyi atama izniniz yok.");
                }
            }

            if (await db.Kullanicilar.AnyAsync(x => x.KullaniciAdi == kullanicilar.KullaniciAdi))
                ModelState.AddModelError("KullaniciAdi", "Bu kullanıcı adı kullanılıyor.");

            if (await db.Kullanicilar.AnyAsync(x => x.Email == kullanicilar.Email))
                ModelState.AddModelError("Email", "Bu email kullanılıyor.");

            if (!ModelState.IsValid)
            {
                DoldurDropDown(kullanicilar);
                return View(kullanicilar);
            }

            kullanicilar.Aktif = true;
            kullanicilar.OlusturmaTarihi = DateTime.Now;

            db.Kullanicilar.Add(kullanicilar);
            await db.SaveChangesAsync();

            TempData["msj"] = "KAYIT BAŞARILI";
            return RedirectToAction("Index");
        }
        public async Task<ActionResult> Edit(int? id)
        {
            if (AltYetkiMi)
                return RedirectToAction("Profilim");

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var kullanicilar = await db.Kullanicilar.FindAsync(id);
            if (kullanicilar == null)
                return HttpNotFound();

            if (!SuperAdminMi && kullanicilar.RestoranId != AktifRestoranId)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            DoldurDropDown(kullanicilar);
            return View(kullanicilar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(
            [Bind(Include = "KullaniciId,RestoranId,YetkiId,KullaniciAdi,Ad,Soyad,Email,Telefon,Sifre,BaslamaTarihi,Aktif")]
            Kullanicilar kullanicilar)
        {
            if (AltYetkiMi)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var mevcutKullanici = await db.Kullanicilar.AsNoTracking()
                                          .FirstOrDefaultAsync(x => x.KullaniciId == kullanicilar.KullaniciId);
            if (mevcutKullanici == null)
                return HttpNotFound();

            if (!SuperAdminMi && mevcutKullanici.RestoranId != AktifRestoranId)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (!SuperAdminMi)
            {
                kullanicilar.RestoranId = AktifRestoranId;

                if (RestoranMudurMu)
                    kullanicilar.YetkiId = mevcutKullanici.YetkiId; 

                if (RestoranSahibiMi)
                {
                    var izinliYetkiler = IzinliAtanabilirYetkiler(AktifYetkiId);
                    if (!izinliYetkiler.Contains(kullanicilar.YetkiId))
                    {
                        ModelState.AddModelError("YetkiId", "Bu yetkiyi atama izniniz yok.");
                    }
                }
            }

            if (await db.Kullanicilar.AnyAsync(x =>
                x.KullaniciAdi == kullanicilar.KullaniciAdi &&
                x.KullaniciId != kullanicilar.KullaniciId))
            {
                ModelState.AddModelError("KullaniciAdi", "Bu kullanıcı adı başka bir kullanıcıya ait.");
            }

            if (await db.Kullanicilar.AnyAsync(x =>
                x.Email == kullanicilar.Email &&
                x.KullaniciId != kullanicilar.KullaniciId))
            {
                ModelState.AddModelError("Email", "Bu email başka bir kullanıcıya ait.");
            }

            if (!ModelState.IsValid)
            {
                DoldurDropDown(kullanicilar);
                return View(kullanicilar);
            }

            kullanicilar.GuncellemeTarihi = DateTime.Now;
            db.Entry(kullanicilar).State = EntityState.Modified;
            await db.SaveChangesAsync();

            TempData["msj"] = "GÜNCELLEME BAŞARILI";
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (!SuperAdminMi && !RestoranSahibiMi)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var kullanicilar = await db.Kullanicilar.FindAsync(id);
            if (kullanicilar == null)
                return HttpNotFound();

            if (!SuperAdminMi && kullanicilar.RestoranId != AktifRestoranId)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(kullanicilar);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if (!SuperAdminMi && !RestoranSahibiMi)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            var kullanicilar = await db.Kullanicilar.FindAsync(id);
            if (kullanicilar == null)
                return HttpNotFound();

            if (!SuperAdminMi && kullanicilar.RestoranId != AktifRestoranId)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            kullanicilar.Aktif = false;
            kullanicilar.GuncellemeTarihi = DateTime.Now;
            await db.SaveChangesAsync();

            TempData["msj"] = "KULLANICI PASİF YAPILDI";
            return RedirectToAction("Index");
        }
        private int[] IzinliAtanabilirYetkiler(int yetkiId)
        {
            switch (yetkiId)
            {
                case SUPER_ADMIN: return new[] { SUPER_ADMIN, RESTORAN_SAHIBI, RESTORAN_MUDURU, GARSON, ASCI, KASIYER };
                case RESTORAN_SAHIBI: return new[] { RESTORAN_MUDURU, GARSON, ASCI, KASIYER };
                case RESTORAN_MUDURU: return new[] { GARSON, ASCI, KASIYER };
                default: return new int[0];
            }
        }

        private void DoldurDropDown(Kullanicilar kullanicilar = null)
        {
            var restoranlar = SuperAdminMi
                ? db.Restoranlar.ToList()
                : db.Restoranlar.Where(r => r.RestoranId == AktifRestoranId).ToList();

            var izinliYetkiIdler = IzinliAtanabilirYetkiler(AktifYetkiId);
            var yetkiler = db.Yetkiler
                             .Where(y => izinliYetkiIdler.Contains(y.YetkiId))
                             .ToList();

            ViewBag.RestoranId = new SelectList(restoranlar, "RestoranId", "RestoranAdi", kullanicilar?.RestoranId);
            ViewBag.YetkiId    = new SelectList(yetkiler, "YetkiId", "YetkiAdi", kullanicilar?.YetkiId);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}