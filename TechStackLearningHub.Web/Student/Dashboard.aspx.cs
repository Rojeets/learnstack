using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;

namespace TechStackLearningHub.Web.Student
{
    public partial class Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Defence in depth: the <location> rule already blocks anonymous
            // users; this Session guard re-checks on every request.
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                int userId = (int)Session["UserID"];
                DataTable progress = new ProgressService().GetAllCoursesProgressForUser(userId);
                pnlEmpty.Visible = progress.Rows.Count == 0;
                rptCourses.DataSource = progress;
                rptCourses.DataBind();
            }
        }
    }
}