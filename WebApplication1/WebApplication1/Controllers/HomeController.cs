using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using WebApplication1.RabbitMQ;

namespace WebApplication1.Controllers
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

        [HttpPost]
        public ActionResult QueueUp(Student stu)
        {
            //先清空一次存储
            StoreHelper.Remove(stu.StudentId);
            //消息入队...
            RabbitMQHelper.Enqueue("testExchange", "testQueue", stu);
            return Json(new { success = true, msg = "排队等待中..."});
        }

        [HttpPost]
        public ActionResult CheckQueueUp(Student stu)
        {
            //获取处理结果
            var result = StoreHelper.Get(stu.StudentId);
            Console.WriteLine(result);
            return Json(new { success = !string.IsNullOrEmpty(result), msg = result});
        }
    }
}