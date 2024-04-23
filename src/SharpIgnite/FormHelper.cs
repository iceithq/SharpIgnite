using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static string Password(string name, string value)
        {
            return Password(name, value, "");
        }

        public static string Password(string name, string value, string extra)
        {
            return string.Format("<input type='password' name='{0}' value='{1}' {2}/>", name, value, extra);
        }

        public static string DropDown(string name, Array options, string selected, string extra)
        {
            string form = "<select name='" + name + "' " + extra + ">";
            foreach (var key in options.Keys) {
                var value = options[key];
                form += "<option value='" + key + "'>" + value + "</option>";
            }
            form += "</select>";
            return form;
        }

        public static string Email(string name, string value, string extra)
        {
            return string.Format("<input type='email' name='{0}' value='{1}' {2}/>", name, value, extra);
        }
    }
}
