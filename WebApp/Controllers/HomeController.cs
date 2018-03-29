using Aspose.Cells;
using Aspose.Cells.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            
            return View();
        }

        public ActionResult Sopinfo()
        {
            testEntities db = new testEntities();
            return View(db.Sop_Excel.ToList());
        }

        [HttpPost]
        public ActionResult Sopinfo(string name, HttpPostedFileBase file)
        {
            TempData["msg"] = "error";
            testEntities db = new testEntities();
            if (file != null && file.FileName != "")
            {
                var guid = System.Guid.NewGuid();
                string filetype = file.FileName.Substring(file.FileName.LastIndexOf('.') + 1);//获取后缀名
                string RelativePath = $@"/Upload/SopExcel/{guid.ToString()}.{filetype}";//相对路径
                string path = Server.MapPath(RelativePath);
                var SopExcel = new Sop_Excel()
                {
                    id = guid,
                    excelname = name,
                    excelpath = RelativePath,
                    addtime = DateTime.Now,
                };
                db.Sop_Excel.Add(SopExcel);
                db.SaveChanges();
                file.SaveAs(path);
                ExcelToImg(path, SopExcel.id.ToString());
                TempData["msg"] = "success";

                return View(db.Sop_Excel.ToList());
            }

            return View(db.Sop_Excel.ToList());
        }

        public JsonResult Getimglist(string imgid)
        {
            testEntities db = new testEntities();
            var list= db.Sop_Img.Where(r => r.imgid == imgid);
            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
        }

        public void ExcelToImg(string ExcelPath, string excelid)
        {
            testEntities db = new testEntities();
            LicenseHelper.ModifyInMemory.ActivateMemoryPatching();
            Workbook book = new Workbook(ExcelPath);
            var list = book.Worksheets;
            foreach (var item in list)
            {
                item.PageSetup.LeftMargin = 0;
                item.PageSetup.RightMargin = 0;
                item.PageSetup.BottomMargin = 0;
                item.PageSetup.TopMargin = 0;
                ImageOrPrintOptions imgOptions = new ImageOrPrintOptions();
                imgOptions.ImageFormat = System.Drawing.Imaging.ImageFormat.Png;
                imgOptions.OnePagePerSheet = true;
                imgOptions.PrintingPage = PrintingPageType.IgnoreBlank;
                SheetRender sr = new SheetRender(item, imgOptions);
                string guid = System.Guid.NewGuid().ToString();
                string RelativePath = $@"/Upload/Sopimg/{guid}.png";//相对路径

                string filepath = Server.MapPath(RelativePath);
                sr.ToImage(0, filepath);
                db.Sop_Img.Add(new Sop_Img()
                {
                    imgid = excelid,
                    imgpath = RelativePath
                });
                db.SaveChanges();
            }
        }
    }
}