using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Member
{
    public partial class Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Defence in depth: the <location> rule already blocks anonymous
            // users; RequireLogin re-checks the session on every request.
            AuthBLL.RequireLogin();

            if (!IsPostBack)
            {
                List<CourseProgressSummary> progress =
                    new ProgressBLL().GetAllCoursesProgressForUser(AuthBLL.CurrentUserId);
                pnlEmpty.Visible = progress.Count == 0;
                rptCourses.DataSource = progress;
                rptCourses.DataBind();
            }
        }
    }
}
