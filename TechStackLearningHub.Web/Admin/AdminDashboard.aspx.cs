using System;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;

namespace TechStackLearningHub.Web.Admin
{
    public partial class AdminDashboard : Page
    {
        private readonly CourseService _courseService = new CourseService();
        private readonly UserService _userService = new UserService();
        private readonly ResultService _resultService = new ResultService();

        protected void Page_Load(object sender, EventArgs e)
        {
            RoleGuard.RequireAdmin(this);

            if (IsPostBack)
                return;

            litPublishedCourses.Text = _courseService.GetPublishedCourseCount().ToString();
            litActiveStudents.Text = _userService.GetActiveStudentCount().ToString();
            litAttemptsWeek.Text = _resultService.GetAttemptCountSince(DateTime.Today.AddDays(-7)).ToString();

            DataTable users = _userService.GetAllUsersForAdmin();
            litTotalUsers.Text = users.Rows.Count.ToString();

            DataTable recent = _resultService.GetAllResultsForReporting(null);
            grdRecent.DataSource = recent.Rows.Cast<DataRow>().Take(10).CopyToDataTable();
            grdRecent.DataBind();
        }
    }
}