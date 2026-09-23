using System;
using System.Web.UI;

namespace TechStackLearningHub.Web
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string roleName = Session["RoleName"] as string;
            if (string.IsNullOrEmpty(roleName))
                return;

            if (roleName == "Admin")
                Response.Redirect("~/Admin/AdminDashboard.aspx");
            else
                Response.Redirect("~/Student/Dashboard.aspx");
        }
    }
}