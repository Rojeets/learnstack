using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;
using TechStackLearningHub.DAL.Models;

namespace TechStackLearningHub.Web.Admin
{
    public partial class Reports : Page
    {
        private readonly CourseService _courseService = new CourseService();
        private readonly ResultService _resultService = new ResultService();
        private readonly ProgressService _progressService = new ProgressService();

        protected void Page_Load(object sender, EventArgs e)
        {
            RoleGuard.RequireAdmin(this);

            if (!IsPostBack)
            {
                ddlCourse.Items.Add(new ListItem("All courses", ""));
                foreach (DataRow row in _courseService.GetAllCoursesForAdmin().Rows)
                {
                    ddlCourse.Items.Add(new ListItem(
                        string.Format("{0} ({1})", row["CourseName"], row["TechStack"]),
                        row["CourseID"].ToString()));
                }
                BindReports();
            }
        }

        protected void ddlCourse_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindReports();
        }

        private void BindReports()
        {
            int courseId;
            int? filter = int.TryParse(ddlCourse.SelectedValue, out courseId) && courseId > 0
                ? courseId
                : (int?)null;

            grdResults.DataSource = _resultService.GetAllResultsForReporting(filter);
            grdResults.DataBind();

            if (filter.HasValue)
            {
                grdProgress.DataSource = _progressService.GetProgressSummariesForReporting(filter.Value);
                grdProgress.DataBind();
                pnlProgress.Visible = true;
            }
            else
            {
                pnlProgress.Visible = false;
            }
        }
    }
}