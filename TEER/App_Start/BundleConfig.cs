using System.Web;
using System.Web.Optimization;

namespace TEER
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            var styleBundle = new StyleBundle("~/static/styles/css");

            string[] cssBase = new string[]{
                "~/static/css/normalize.css",               
                "~/static/css/site-base.css",
                "~/static/css/site-style.css",
                "~/static/css/bootstrap.css",
                "~/static/css/fuelux.css",
                "~/static/css/site-mobile.css",
                "~/Content/bootstrap-datepicker3.css",
                "~/Content/bootstrap-timepicker.css"
            };
            bundles.Add(styleBundle);

            var cssRewriter = new CssRewriteUrlTransformWrapper();
            foreach (var item in cssBase)
            {
                styleBundle.Include(item, cssRewriter);
            }
            bundles.Add(styleBundle);

            bundles.Add(new ScriptBundle("~/static/scripts/js").Include(
                   "~/static/scripts/jquery-{version}.js",
                "~/static/scripts/jquery-migrate-{version}.js",
                "~/static/scripts/jquery-ui.js",
                //"~/static/scripts/jquery.menu.js",
                "~/static/scripts/jquery.blockUI.js",
                "~/static/scripts/jquery.fileDownload.js",
                "~/static/scripts/jquery.maskedinput.js",
                "~/static/scripts/knockout.js",
                "~/static/scripts/underscore.js",
                "~/static/scripts/json2.js",
                "~/static/scripts/keycodes.js",
                "~/static/scripts/helpers.js",
                "~/static/scripts/layout.js",
                "~/static/scripts/wkc.js",
                "~/static/scripts/jquery.bbq-{version}.min.js",
                "~/static/scripts/bootstrap.js",
                "~/static/scripts/fuelux.js",
                "~/static/scripts/response.js",
                "~/static/scripts/jquery.validate.js",
                "~/static/scripts/jquery.validate.unobtrusive.js",
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/bootstrap-timepicker.js",
                "~/Scripts/jquery-sortable.js"
              ));
        }

        public class CssRewriteUrlTransformWrapper : IItemTransform
        {
            public string Process(string includedVirtualPath, string input)
            {
                return new CssRewriteUrlTransform().Process("~" + VirtualPathUtility.ToAbsolute(includedVirtualPath), input);
            }
        }
    }
}
