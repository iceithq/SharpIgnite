using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SharpIgnite
{
    public static class FormHelper
    {
        public static string Input(string name)
        {
            return Input(name, "");
        }

        public static string Input(string name, string value)
        {
            return Input(name, value, "");
        }

        public static string Input(string name, string value, string extra)
        {
            return string.Format("<input type='text' name='{0}' value='{1}' {2}/>", name, value, extra);
        }

        public static string Password(string name)
        {
            return Password(name, "");
        }

        public static string Password(string name, string value)
        {
            return Password(name, value, "");
        }

        public static string Password(string name, string value, string extra)
        {
            return string.Format("<input type='password' name='{0}' value='{1}' {2}/>", name, value, extra);
        }

        public static string DropDown(string name, Array options)
        {
            return DropDown(name, options, "", "");
        }

        static string Encode(string str)
        {
            return HttpUtility.HtmlEncode(str);
        }

        public static string DropDown(string name, Array options, string selected, string extra)
        {
            string form = "<select name='" + name + "' " + extra + ">";
            foreach (var key in options.Keys) {
                var value = options[key];
                form += "<option value='" + Encode(key.ToString()) + "'>" + Encode(value.ToString()) + "</option>";
            }
            form += "</select>";
            return form;
        }

        public static string Label(string text, string id)
        {
            return string.Format("<label for='{1}'>{0}</label>", text, id);
        }

        public static string Radio(string name, string value, bool _checked, string extra)
        {
            var c = _checked ? "checked" : "";
            return string.Format("<input type='radio' name='{0}' value='{1}' {2} {3}>", name, value, c, extra);
        }

        public static string CheckBox(string name, string value, bool _checked)
        {
            var c = _checked ? "selected" : "";
            return string.Format("<input type='checkbox' name='{0}' value='{1}' {2}>", name, value, c);
        }

        public static string Email(string name)
        {
            return Email(name, "", "");
        }

        public static string Email(string name, string value, string extra)
        {
            return string.Format("<input type='email' name='{0}' value='{1}' {2}>", name, value, extra);
        }

        public static string Submit(string value, string extra)
        {
            return string.Format("<input type='submit' value='{0}' {1}>", value, extra);
        }
    }
}
