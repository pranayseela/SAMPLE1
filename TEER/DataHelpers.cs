using TEER.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace TEER
{
    public static class DataHelpers
    {
        public static MvcHtmlString HiddenJsonFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            return HiddenJsonFor(htmlHelper, expression, (IDictionary<string, object>)null);
        }

        public static MvcHtmlString HiddenJsonFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        {
            return HiddenJsonFor(htmlHelper, expression, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }
        //public static string LocationNameFromId(this List<Location> locations, int? locationId, string defaultValue = null)
        //{
        //    if (locationId == null)
        //    {
        //        return defaultValue;
        //    }

        //    var location = locations.Find(l => l.LocationId == locationId.Value);

        //    if (location == null)
        //    {
        //        return defaultValue;
        //    }
        //    else
        //    {
        //        return location.LocationName;
        //    }
        //}
        public static MvcHtmlString HiddenJsonFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);

            var tagBuilder = new TagBuilder("input");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("name", name);
            tagBuilder.MergeAttribute("type", "hidden");

            var json = JsonConvert.SerializeObject(metadata.Model);

            tagBuilder.MergeAttribute("value", json);

            return MvcHtmlString.Create(tagBuilder.ToString());
        }
    }
}