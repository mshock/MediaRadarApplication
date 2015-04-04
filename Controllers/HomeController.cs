using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MediaRadarProject.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Ads()
        {
            MediaRadarServiceRef.AdDataServiceClient ads_client = new MediaRadarServiceRef.AdDataServiceClient();
            MediaRadarServiceRef.Ad[] ads = ads_client.GetAdDataByDateRange(
                Convert.ToDateTime("1/1/2011"),
                Convert.ToDateTime("4/1/2011")
                );

            if (Request.Form["c50"] != null)
            {
                ViewBag.Ads = ads.Where(a => a.Position == "Cover" && a.NumPages >= (decimal)0.5);
            }
            else if (Request.Form["t5"] != null) {
                // hash of ads - distinct brand to max page coverage
                Hashtable brands_coverage = new Hashtable();
                
                foreach (var ad in ads)
                {
                    string brandname = ad.Brand.BrandName;
                    decimal coverage = ad.NumPages;
                    if (brands_coverage.ContainsKey(brandname))
                    {
                        if ((decimal)((MediaRadarServiceRef.Ad)brands_coverage[brandname]).NumPages < coverage)
                        {
                            brands_coverage[brandname] = ad;
                        }

                    }
                    else
                    {
                        brands_coverage.Add(brandname, ad);                        
                    }
                }

                // order the hash into a list of top 5 ads
                List<DictionaryEntry> order_brands = brands_coverage.Cast<DictionaryEntry>().OrderByDescending(entry => (decimal)((MediaRadarServiceRef.Ad)entry.Value).NumPages).ToList();
                List<MediaRadarServiceRef.Ad> ads_list = new List<MediaRadarServiceRef.Ad>();

                foreach (DictionaryEntry entry in order_brands)
                {
                    ads_list.Add((MediaRadarServiceRef.Ad) entry.Value);
                }
                ViewBag.Ads = ads_list.Take(5);

            }
            else if (Request.Form["t5o"] != null) {
                Hashtable brands_coverage = new Hashtable();
                Hashtable brandid_lookup = new Hashtable();
                foreach (var ad in ads)
                {
                    string brandname = ad.Brand.BrandName;
                    decimal coverage = ad.NumPages;
                    if (brands_coverage.ContainsKey(brandname))
                    {
                        decimal prev = (decimal) brands_coverage[brandname];
                        brands_coverage[brandname] = prev + coverage;
                    }
                    else
                    {
                        brands_coverage.Add(brandname, coverage);
                        brandid_lookup.Add(brandname, ad.Brand.BrandId);
                    }
                }

                List<DictionaryEntry> order_brands = brands_coverage.Cast<DictionaryEntry>().OrderByDescending(entry => (decimal)entry.Value).ToList();
                List<MediaRadarServiceRef.Ad> ads_list = new List<MediaRadarServiceRef.Ad>();
                foreach (DictionaryEntry entry in order_brands)
                {
                    MediaRadarServiceRef.Ad ad = new MediaRadarServiceRef.Ad();
                    ad.Brand = new MediaRadarServiceRef.Brand();
                    ad.Brand.BrandName = (string) entry.Key;
                    ad.Brand.BrandId = (int)brandid_lookup[entry.Key];
                    ad.NumPages = (decimal)entry.Value;
                    
                    ads_list.Add(ad);
                }
                ViewBag.Ads = ads_list.Take(5);

            }
            else
            {
                // all ads ordered by name
                ViewBag.Ads = ads.OrderBy(a => a.Brand.BrandName);
            }
            ViewBag.numads = Enumerable.Count(ViewBag.Ads);
            return View();
        }

        public ActionResult Contact()
        {

            return View();
        }
    }
}