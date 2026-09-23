using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;

namespace TechStackLearningHub.Web.Student
{
    public partial class ProgressDashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (IsPostBack)
                return;

            int userId = (int)Session["UserID"];
            rptCourses.DataSource = new ProgressService().GetAllCoursesProgressForUser(userId);
            rptCourses.DataBind();
        }
    }
}