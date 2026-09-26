using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Masterpages;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Admin
{
    public partial class AdminDashboard : Page
    {
        private readonly CourseBLL _courseBLL = new CourseBLL();
        private readonly UserBLL _userBLL = new UserBLL();
        private readonly ResultBLL _resultBLL = new ResultBLL();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Shell first, before the IsPostBack early-return: the master
            // renders its title and nav highlight on every response including
            // postbacks, so setting these only on the first GET would blank the
            // heading and drop the "Dashboard" nav state after any postback.
            var master = (AdminMaster)Master;
            master.SetPageTitle("Dashboard");
            master.SetActiveNav("AdminDashboard.aspx");

            // Admin.Master.Page_Init already enforced admin-only access before
            // any postback event ran, so this page needs no guard of its own.
            if (IsPostBack)
                return;

            litActiveStudents.Text = _userBLL.GetActiveStudentCount().ToString();
            litPublishedCourses.Text = _courseBLL.GetPublishedCourseCount().ToString();
            litAttemptsWeek.Text = _resultBLL.GetAttemptCountSince(DateTime.Today.AddDays(-7)).ToString();

            // InvariantCulture because the decimal separator is part of the UI
            // here: a comma-decimal host would otherwise render "72,5%" and read
            // as seventy-two comma five.
            litAvgPassRate.Text = _resultBLL.GetAveragePassRatePercent()
                .ToString("0.0", CultureInfo.InvariantCulture) + "%";

            // A List<T> binds straight to a GridView and an empty list renders
            // EmptyDataText, so no guard is needed here.
            grdStacks.DataSource = _courseBLL.GetCourseCountsByTechStack();
            grdStacks.DataBind();

            // Take(5) on the typed list, not Take(5).CopyToDataTable():
            // CopyToDataTable throws on an empty sequence, so an empty results
            // table would have crashed this page instead of just rendering an
            // empty grid. A List<T> has no such failure mode.
            List<ResultReportRow> recent = _resultBLL.GetAllResultsForReporting(null)
                .Take(5)
                .ToList();
            grdRecent.DataSource = recent;
            grdRecent.DataBind();
        }

        /// <summary>
        /// Formats a bound value as a CSS width for the stack bars. The result
        /// is interpolated into a style attribute, so anything that is not a
        /// plain number becomes 0% rather than being copied through: only
        /// digits, a decimal point and the % sign ever reach the markup, so a
        /// crafted display value cannot break out of the style attribute.
        /// InvariantCulture keeps a comma-decimal host from rendering a width
        /// the browser cannot parse, and the clamp keeps a value over 100 from
        /// overflowing its container.
        /// </summary>
        protected string GetPercent(object value)
        {
            if (value == null || value == DBNull.Value)
                return "0%";

            decimal number;
            if (!decimal.TryParse(
                    Convert.ToString(value, CultureInfo.InvariantCulture),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out number))
            {
                return "0%";
            }

            if (number < 0m)
                number = 0m;
            else if (number > 100m)
                number = 100m;

            return number.ToString("0.##", CultureInfo.InvariantCulture) + "%";
        }
    }
}
