using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using GdPicture14.WEB;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using GdPicture14;

namespace aspnet_mvc_razor_app
{
    public class MvcApplication : HttpApplication
    {
        public static readonly int SESSION_TIMEOUT = 20; //Set to 20 minutes. use -1 to handle DocuVieware session timeout through asp.net session mechanism.
        private const bool STICKY_SESSION = true; //Set false to use DocuVieware on Servers Farm witn non sticky sessions.
        private const DocuViewareSessionStateMode DOCUVIEWARE_SESSION_STATE_MODE = DocuViewareSessionStateMode.InProc; //Set DocuViewareSessionStateMode.File is STICKY_SESSION is False.

        public static string GetCacheDirectory()
        {
            return HttpRuntime.AppDomainAppPath + "\\cache";
        }

        public static string GetDocumentsDirectory()
        {
            return HttpRuntime.AppDomainAppPath + "\\documents";
        }

        protected void Application_Start()
        {
            try
            {
                Assembly.Load("GdPicture.NET.14.WEB.DocuVieware");
            }
            catch (System.IO.FileNotFoundException)
            {
                throw new System.IO.FileNotFoundException(" The system cannot find the DocuVieware assembly. Please set the Copy Local Property of the GdPicture.NET.14.WEB.DocuVieware reference to true. More information: https://msdn.microsoft.com/en-us/library/t1zz5y8c(v=vs.100).aspx");
            }

            DocuViewareManager.SetupConfiguration(STICKY_SESSION, DOCUVIEWARE_SESSION_STATE_MODE, GetCacheDirectory());
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            DocuViewareLicensing.RegisterKEY("0458397874583347869311664"); //Unlocking DocuVieware. Please set your demo or commercial license key here.
            DocuViewareEventsHandler.NewDocumentLoaded += NewDocumentLoadedHandler;
            DocuViewareEventsHandler.CustomAction += CustomActionDispatcher;
        }

        private static void NewDocumentLoadedHandler(object sender, NewDocumentLoadedEventArgs e)
        {
            e.docuVieware.PagePreload = e.docuVieware.PageCount <= 50 ? PagePreloadMode.AllPages : PagePreloadMode.AdjacentPages;
        }

        private static void CustomActionDispatcher(object sender, CustomActionEventArgs e)
        {
            switch (e.actionName)
            {
                case "delete":
                    if (e.docuVieware.GetDocumentType() == DocumentType.DocumentTypePDF)
                    {
                        GdPicturePDF oGdPicturePDF = new GdPicturePDF();
                        //prepare array of selected pages
                        List<int> selectedPages = JsonConvert.DeserializeObject<List<int>>(e.args.ToString());
                        selectedPages = selectedPages.OrderByDescending(t => t).ToList();

                        //delete pages from the native PDF
                        if (e.docuVieware.GetNativePDF(out oGdPicturePDF) == GdPictureStatus.OK)
                        {
                            foreach (int page in selectedPages)
                            {
                                if (oGdPicturePDF.DeletePage(page) != GdPictureStatus.OK)
                                {
                                    e.result = oGdPicturePDF.GetStat().ToString();
                                    break;
                                }
                            }
                        }

                        e.docuVieware.RedrawPage();
                    }
                    else
                    {
                        e.message= new DocuViewareMessage("Wrong document format");
                    }
                    
                  
                    break;
            }
        }
    }
}