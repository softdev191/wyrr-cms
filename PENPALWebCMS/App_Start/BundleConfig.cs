using System.Web;
using System.Web.Optimization;

namespace PENPALWebCMS
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                      "~/Scripts/jquery-{version}.js",
                      "~/Scripts/jquery-ui-{version}.js",
                      "~/Scripts/jquery.unobtrusive-ajax.js",
                      "~/Scripts/jquery.blockUI.js"));


            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                       "~/Scripts/jquery.validate*",
                       "~/Scripts/MicrosoftAjax.js",
                       "~/Scripts/MicrosoftMvcAjax.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/transactionanalysis").Include(
                     "~/js/TransactionAnalysis.js"));

            bundles.Add(new ScriptBundle("~/bundles/transactionconfiguration").Include(
                    "~/js/TransactionConfiguration.js"));

            bundles.Add(new ScriptBundle("~/bundles/useranalysis").Include(
                    "~/js/UserAnalysis.js"));

            bundles.Add(new ScriptBundle("~/bundles/usertransactionsetting").Include(
                   "~/js/UserTransactionSetting.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/font-awesome.css",
                      "~/Content/ionicons.min.css",
                      "~/Content/AdminLTE.css"

                     ));

        }
    }
}
