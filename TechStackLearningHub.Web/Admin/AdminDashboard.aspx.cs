using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;
using TechStackLearningHub.Models;

namespace TechStackLearningHub.Web.Admin
{
    public partial class AdminDashboard : Page
    {
        private readonly CourseBLL _courseBLL = new CourseBLL();
        private readonly UserBLL _userBLL = new UserBLL();
        private readonly ResultBLL _resultBLL = new ResultBLL();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Admin.Master.Page_Init already enforced admin-only access before
            // any postback event ran, so this page needs no guard of its own.
            if (IsPostBack)
                return;

            litPublishedCourses.Text = _courseBLL.GetPublishedCourseCount().ToString();
            litActiveStudents.Text = _userBLL.GetActiveStudentCount().ToString();
            litAttemptsWeek.Text = _resultBLL.GetAttemptCountSince(DateTime.Today.AddDays(-7)).ToString();

            List<User> users = _userBLL.GetAllUsersForAdmin();
            litTotalUsers.Text = users.Count.ToString();

            // Take(10) on the typed list, not Take(10).CopyToDataTable():
            // CopyToDataTable throws on an empty sequence, so an empty results
            // table would have crashed this page instead of just rendering an
            // empty grid. A List<T> has no such failure mode.
            List<ResultReportRow> recent = _resultBLL.GetAllResultsForReporting(null)
                .Take(10)
                .ToList();
            grdRecent.DataSource = recent;
            grdRecent.DataBind();
        }
    }
}
