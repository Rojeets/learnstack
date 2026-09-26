using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;

namespace TechStackLearningHub.Web.Member
{
    public partial class ResultsHistory : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AuthBLL.RequireLogin();

            if (IsPostBack)
                return;

            grdHistory.DataSource = new QuizBLL().GetQuizHistoryForUser(AuthBLL.CurrentUserId);
            grdHistory.DataBind();
        }
    }
}