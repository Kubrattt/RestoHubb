using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using RestoHub.Models;

namespace RestoHub.Controllers
{
    public class SiparisDurumlarisController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View(await db.BirimTipleri.ToListAsync());
        }
        public ActionResult Create()
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "DurumId,DurumAdi,Renk,GorunumSirasi,OlusturmaTarihi")] SiparisDurumlari siparisDurumlari)
        {
            if (AltYetkiMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (await db.SiparisDurumlari.AnyAsync(x => x.DurumAdi == siparisDurumlari.DurumAdi))
                ModelState.AddModelError("DurumAdi", "Bu durum adı sistemde kayıtlı. Lütfen başka bir durum adı giriniz");

            if (!ModelState.IsValid)
                return View(siparisDurumlari);

            siparisDurumlari.OlusturmaTarihi = DateTime.Now;
            db.SiparisDurumlari.Add(siparisDurumlari);
            await db.SaveChangesAsync();

            TempData["msj"] = "KAYIT BAŞARILI";
            return RedirectToAction("Index");
        }
        public async Task<ActionResult> Edit(int? id)
        {

            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            SiparisDurumlari siparisDurumlari = await db.SiparisDurumlari.FindAsync(id);
            if (siparisDurumlari == null) return HttpNotFound();

            return View(siparisDurumlari);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "DurumId,DurumAdi,Renk,GorunumSirasi,OlusturmaTarihi")] SiparisDurumlari siparisDurumlari)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (await db.SiparisDurumlari.AnyAsync(x =>
                x.DurumAdi == siparisDurumlari.DurumAdi &&
                x.DurumId != siparisDurumlari.DurumId))
            {
                ModelState.AddModelError("DurumAdi", "Bu durum adı sistemde kayıtlı. Lütfen başka bir durum adı giriniz");
            }

            if (!ModelState.IsValid)
                return View(siparisDurumlari);

            db.Entry(siparisDurumlari).State = EntityState.Modified;
            await db.SaveChangesAsync();

            TempData["msj"] = "GÜNCELLEME BAŞARILI";
            return RedirectToAction("Index");
        }
        public async Task<ActionResult> Delete(int? id)
        {
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            SiparisDurumlari siparisDurumlari = await db.SiparisDurumlari.FindAsync(id);
            if (siparisDurumlari == null) return HttpNotFound();

            return View(siparisDurumlari);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {   
            if (!SuperAdminMi) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            SiparisDurumlari siparisDurumlari = await db.SiparisDurumlari.FindAsync(id);
            db.SiparisDurumlari.Remove(siparisDurumlari);
            await db.SaveChangesAsync();

            TempData["msj"] = "YETKİ SİLİNDİ";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
