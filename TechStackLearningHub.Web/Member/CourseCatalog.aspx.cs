using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Helpers;
using TechStackLearningHub.Web.Models;

namespace TechStackLearningHub.Web.Member
{
    public partial class CourseCatalog : Page
    {
        private readonly CourseBLL _courseBLL = new CourseBLL();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Defence in depth: the <location> rule already blocks anonymous
            // users, and RequireLogin re-checks the session on every request.
            // Page_Load is safe for this read-only page because there are no
            // write handlers here.
            AuthBLL.RequireLogin();

            if (!IsPostBack)
            {
                ddlTechStackFilter.Items.Add(new ListItem("All stacks", ""));
                foreach (string stack in CourseBLL.KnownTechStacks)
                {
                    ddlTechStackFilter.Items.Add(new ListItem(stack, stack));
                }

                // The filter is shareable, so it has to come back out of the URL
                // as well as go into it. Matched against the known stacks so a
                // hand-edited query cannot invent a filter.
                string requested = Request.QueryString["TechStack"];
                if (!string.IsNullOrEmpty(requested))
                {
                    ListItem item = ddlTechStackFilter.Items.FindByValue(requested);
                    if (item != null)
                        ddlTechStackFilter.SelectedValue = requested;
                }

                BindCourses();
            }
        }

        protected void ddlTechStackFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Filter is applied server-side against the published-only dataset,
            // so unpublished courses can never surface in the page source.
            BindCourses();
            UrlSync.Sync(this, "TechStack", ddlTechStackFilter.SelectedValue);
        }

        private void BindCourses()
        {
            string filter = ddlTechStackFilter.SelectedValue;
            List<Course> courses = _courseBLL.GetCatalogueForStudents(filter);
            pnlEmpty.Visible = courses.Count == 0;
            rptCourses.DataSource = courses;
            rptCourses.DataBind();
        }
    }
}
