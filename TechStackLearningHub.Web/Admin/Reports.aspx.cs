using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Helpers;
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

                // Read the filter back out of the URL. Without this the filter
                // could be written into the address bar but not restored from
                // it, so a shared or reloaded link would come back unfiltered.
                int requestedCourse;
                if (int.TryParse(Request.QueryString["CourseID"], out requestedCourse) && requestedCourse > 0)
                {
                    ListItem requested = ddlCourse.Items.FindByValue(requestedCourse.ToString());
                    if (requested != null)
                        ddlCourse.SelectedValue = requestedCourse.ToString();
                }

                BindReports();
            }
            else if (GridPaging.IsPagerRequest(this, grdResults) || GridPaging.IsPagerRequest(this, grdProgress))
            {
                BindReports();
                int? requested = GridPaging.RequestedPage(this);
                if (requested.HasValue)
                {
                    GridPaging.ApplyIndex(grdResults, requested.Value);
                    GridPaging.ApplyIndex(grdProgress, requested.Value);
                }
                GridPaging.Rebind(grdResults);
                GridPaging.Rebind(grdProgress);
            }

            GridPaging.Wire(grdResults);
            GridPaging.Wire(grdProgress);
        }

        protected void ddlCourse_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindReports();
            UrlSync.Sync(this, "CourseID", ddlCourse.SelectedValue);
        }

        private void BindReports()
        {
            int courseId;
            int? filter = int.TryParse(ddlCourse.SelectedValue, out courseId) && courseId > 0
                ? courseId
                : (int?)null;

            // Switching the course filter swaps in a different result set, so the
            // old page index no longer means anything and can point past the end
            // of a smaller set. Restart that grid at the first page, but leave it
            // alone for a pager postback so page clicks keep working.
            bool filterChanged = !Equals(ViewState["BoundFilter"], filter);
            if (filterChanged)
            {
                ViewState["BoundFilter"] = filter;
                grdResults.PageIndex = 0;
            }

            grdResults.DataSource = _resultBLL.GetAllResultsForReporting(filter);
            GridPaging.Rebind(grdResults);

            if (filter.HasValue)
            {
                if (filterChanged)
                    grdProgress.PageIndex = 0;

                grdProgress.DataSource = _progressBLL.GetProgressSummariesForReporting(filter.Value);
                GridPaging.Rebind(grdProgress);
                pnlProgress.Visible = true;
            }
            else
            {
                pnlProgress.Visible = false;
            }
        }
    }
}
