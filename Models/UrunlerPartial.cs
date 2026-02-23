using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestoHub.Models
{
    public partial class Urunler
    {
        public string HazirlamaSuresiText
        {
            get
            {
                if (!HazirlamaSuresi.HasValue)
                    return "";

                int saat = HazirlamaSuresi.Value / 60;
                int dakika = HazirlamaSuresi.Value % 60;

                if (saat > 0)
                    return $"{saat} saat {dakika} dakika";

                return $"{dakika} dakika";
            }
        }
    }
}