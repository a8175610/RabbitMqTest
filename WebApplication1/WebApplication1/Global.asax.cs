
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebApplication1.Models;
using WebApplication1.RabbitMQ;

namespace WebApplication1
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //启用监听
            RabbitMQHelper.Listening<Student>("testExchange", "testQueue", student =>
            {
                var resultData = "选课成功...";
                try
                {
                    //模拟耗时操作...
                    Thread.Sleep(50);

                    //存储结果,如果已有对应的key，则覆盖
                    StoreHelper.AddOrUpdate(student.StudentId, resultData);
                    return true;
                }
                catch (Exception e)
                {
                    //存储结果,如果已有对应的key，则覆盖
                    StoreHelper.AddOrUpdate(student.StudentId, resultData);
                    return false;
                }
            });
        }
    }
}
