using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;
using TechStackLearningHub.Models;

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
                BindCourses();
            }
        }

        protected void ddlTechStackFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Filter is applied server-side against the published-only dataset,
            // so unpublished courses can never surface in the page source.
            BindCourses();
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
