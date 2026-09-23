using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.BLL;

namespace TechStackLearningHub.Web.Student
{
    public partial class CourseCatalog : Page
    {
        private readonly CourseService _courseService = new CourseService();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                ddlTechStackFilter.Items.Add(new ListItem("All stacks", ""));
                foreach (string stack in CourseService.KnownTechStacks)
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
            rptCourses.DataSource = _courseService.GetCatalogueForStudents(filter);
            rptCourses.DataBind();
        }
    }
}