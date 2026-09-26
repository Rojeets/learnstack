using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TechStackLearningHub.Web.BLL;
using TechStackLearningHub.Web.Helpers;

namespace TechStackLearningHub.Web.Member
{
    public partial class ResultsHistory : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AuthBLL.RequireLogin();

            if (!IsPostBack)
                BindGrid();
            else if (GridPaging.IsPagerRequest(this, grdHistory))
            {
                BindGrid();
                GridPaging.ApplyIndex(grdHistory, GridPaging.RequestedPage(this).GetValueOrDefault());
                GridPaging.Rebind(grdHistory);
            }

            GridPaging.Wire(grdHistory);
        }

        private void BindGrid()
        {
            grdHistory.DataSource = new QuizBLL().GetQuizHistoryForUser(AuthBLL.CurrentUserId);
            GridPaging.Rebind(grdHistory);
        }
    }
}