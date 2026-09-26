using System;
using System.Web.UI;
using TechStackLearningHub.BLL;

namespace TechStackLearningHub.Web.Pages
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string roleName = AuthBLL.CurrentRoleName;
            if (string.IsNullOrEmpty(roleName))
                return;

            if (roleName == "Admin")
                Response.Redirect("~/Admin/AdminDashboard.aspx");
            else
                Response.Redirect("~/Member/Dashboard.aspx");
        }
    }
}