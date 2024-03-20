using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SharpIgnite
{
    public partial class WebForm : System.Web.UI.Page
    {
        public Database Database {
            get { return AppContext.Instance.Database; }
        }

        public Input Input {
            get { return AppContext.Instance.Input; }
        }

        public void Write(string s)
        {
            Response.Write(s);
        }
    }
}