using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Masterpages;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Admin
{
    public partial class Reports : Page
    {
        private readonly CourseBLL _courseBLL = new CourseBLL();
        private readonly ResultBLL _resultBLL = new ResultBLL();
        private readonly ProgressBLL _progressBLL = new ProgressBLL();

        protected void Page_Load(object sender, EventArgs e)
        {
            // The admin shell (topbar title, highlighted sidebar link) lives in
            // Admin.Master and reads its state from these two calls, so they run
            // before the IsPostBack early-return: a postback render comes back
            // through here too, and would otherwise come up untitled.
            var master = (AdminMaster)Master;
            master.SetPageTitle("Reports");
            master.SetActiveNav("Reports.aspx");

            if (!IsPostBack)
            {
                ddlCourse.Items.Add(new ListItem("All courses", ""));
                foreach (Course course in _courseBLL.GetAllCoursesForAdmin())
                {
                    ddlCourse.Items.Add(new ListItem(
                        string.Format("{0} ({1})", course.CourseName, course.TechStack),
                        course.CourseID.ToString()));
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

            grdResults.DataSource = _resultBLL.GetAllResultsForReporting(filter);
            grdResults.DataBind();

            if (filter.HasValue)
            {
                grdProgress.DataSource = _progressBLL.GetProgressSummariesForReporting(filter.Value);
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
